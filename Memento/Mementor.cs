namespace Memento
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using Fasterflect;

    /// <summary>
    /// Provides undo and redo services.
    /// </summary>
    public class Mementor : IDisposable
    {
#region Fields & Creation
        /// <summary>
        /// Fired after an undo or redo is performed.
        /// </summary>
        public event MementorChanged Changed;

        private readonly BatchEvent _undoStack = new BatchEvent();
        private readonly BatchEvent _redoStack = new BatchEvent();
        private BatchEvent _currentBatch;

        /// <summary>
        /// Creates an instance of <see cref="Mementor"/>.
        /// </summary>
        /// <returns>A new instance of <see cref="Mementor"/>.</returns>
        public static Mementor Create()
        {
            return new Mementor(true);
        }

        private Mementor(bool isEnabled)
        {
            IsTrackingEnabled = isEnabled;
        }
#endregion

#region Implements IDisposable
        public void Dispose()
        {
            Changed = null;
            _undoStack.Clear();
            _redoStack.Clear();
            _currentBatch = null;
        }
#endregion

        #region Implements IUndoService
        /// <summary>
        /// Marks a property change. 
        /// </summary>
        /// <typeparam name="TProp">The type of the property.</typeparam>
        /// <param name="target">The target object.</param>
        /// <param name="propertySelector">The property selector expression.</param>
        /// <param name="previousValue">The value to be restored to when undo. 
        /// If not supplied, will be retrieved directly from the <paramref name="target"/>.</param>
        public void PropertyChange<TProp>(object target, Expression<Func<TProp>> propertySelector, object previousValue = null)
        {
            PropertyChange(target, propertySelector.Name(), previousValue);
        }

        /// <summary>
        /// Marks a property change.
        /// </summary>
        /// <param name="target">The target object.</param>
        /// <param name="propertyName">The name of the property.</param>
        /// <param name="previousValue">The value to be restored to when undo. 
        /// If not supplied, will be retrieved directly from the <paramref name="target"/>.</param>
        public void PropertyChange(object target, string propertyName, object previousValue = null)
        {
            if (!CheckPreconditions(target, "target", propertyName, "propertyName"))
                return;

            LogPropertyChange(_currentBatch ?? _undoStack, target, propertyName, previousValue);

            if (!IsInBatch)
                PerformLogPostAction();
        }

        /// <summary>
        /// Marks a collection addition. 
        /// </summary>
        /// <typeparam name="T">The generic type parameter of the collection.</typeparam>
        /// <param name="collection">The collection object.</param>
        /// <param name="element">The element being added.</param>
        public void ElementAdd<T>(IList<T> collection, object element)
        {
            if (!CheckPreconditions(collection, "collection")) 
                return;

            LogElementAdd(_currentBatch ?? _undoStack, collection, element);

            if (!IsInBatch)
                PerformLogPostAction();
        }

        /// <summary>
        /// Marks a collection removal.
        /// </summary>
        /// <typeparam name="T">The generic type parameter of the collection.</typeparam>
        /// <param name="collection">The collection object.</param>
        /// <param name="element">The element being removed</param>
        /// <param name="elementIndex">The index of the element being removed. If not supplied, will retrieve via IndexOf.</param>
        public void ElementRemove<T>(IList<T> collection, object element, int? elementIndex = null)
        {
            if (!CheckPreconditions(collection, "collection"))
                return;

            LogElementRemove(_currentBatch ?? _undoStack, collection, element, elementIndex);

            if (!IsInBatch)
                PerformLogPostAction();
        }

        /// <summary>
        /// Marks a collection index change.
        /// </summary>
        /// <typeparam name="T">The generic type parameter of the collection.</typeparam>
        /// <param name="collection">The collection object.</param>
        /// <param name="element">The element whose index is being changed</param>
        /// <param name="elementIndex">The index of the element whose index is being changed. If not supplied, will retrieve via IndexOf.</param>
        public void ElementIndexChange<T>(IList<T> collection, object element, int? elementIndex = null)
        {
            if (!CheckPreconditions(collection, "collection")) 
                return;

            LogElementIndexChange(_currentBatch ?? _undoStack, collection, element, elementIndex);

            if (!IsInBatch)
                PerformLogPostAction();
        }

        /// <summary>
        /// Marks a batch during which all events are combined so that when undo, only need to invoke 
        /// <see cref="Undo"/> once.
        /// </summary>
        /// <param name="codeBlock">The code block. Calls to <see cref="ElementAdd{T}"/>, <see cref="PropertyChange{TProp}"/> etc. 
        /// should happen in this block.</param>
        /// <seealso cref="BeginBatch"/>
        /// <remarks>Batches cannot be nested. At any point, there must be only one active batch.</remarks>
        public void Batch(Action codeBlock)
        {
            if (!CheckPreconditions(codeBlock, "codeBlock")) 
                return;

            BeginBatch();
            try
            {
                codeBlock();
            }
            finally
            {
                // Must not call EndBatch() because CheckPreconditions() might return false
                if (InternalEndBatch(_undoStack))
                    PerformLogPostAction();
            }
        }

        /// <summary>
        /// Explicitly marks the beginning of a batch.
        /// </summary>
        public void BeginBatch()
        {
            if (!CheckPreconditions())
                return;

            if (IsInBatch)
                throw new InvalidOperationException("Re-entrant batch is not supported");

            InternalBeginBatch();
        }

        /// <summary>
        /// Ends a batch.
        /// </summary>
        public void EndBatch()
        {
            if (!CheckPreconditions())
                return;

            if (IsInBatch)
                throw new InvalidOperationException("A batch has not been started yet");

            if (InternalEndBatch(_undoStack))
                PerformLogPostAction();
        }

        /// <summary>
        /// Executes the supplied code block with <see cref="IsTrackingEnabled"/> turned off.
        /// </summary>
        /// <param name="codeBlock">The code block to be executed.</param>
        public void ExecuteNoTracking(Action codeBlock)
        {
            IsTrackingEnabled = false;
            try
            {
                codeBlock();
            }
            finally
            {
                IsTrackingEnabled = true;
            }
        }

        /// <summary>
        /// Performs an undo.
        /// </summary>
        public void Undo()
        {
            if (!CanUndo) throw new InvalidOperationException("There is nothing to undo");
            if (IsInBatch) throw new InvalidOperationException("Finish the active batch first");

            var @event = _undoStack.Pop();
            IEvent tmpEvent = @event is BatchEvent ? new BatchEvent((BatchEvent) @event) : @event;
            ReverseEvent(@event, true);
            NotifyChange(tmpEvent);
        }

        /// <summary>
        /// Performs a redo.
        /// </summary>
        public void Redo()
        {
            if (!CanRedo) throw new InvalidOperationException("There is nothing to redo");
            if (IsInBatch) throw new InvalidOperationException("Finish the active batch first");

            var @event = _redoStack.Pop();
            IEvent tmpEvent = @event is BatchEvent ? new BatchEvent((BatchEvent)@event) : @event;
            ReverseEvent(@event, false);
            NotifyChange(tmpEvent);
        }

        /// <summary>
        /// Returns <c>true</c> if can undo.
        /// </summary>
        public bool CanUndo
        {
            get { return _undoStack.Count > 0; }
        }

        /// <summary>
        /// Returns <c>true</c> if can redo.
        /// </summary>
        public bool CanRedo
        {
            get { return _redoStack.Count > 0; }
        }

        /// <summary>
        /// How many undos in the stack.
        /// </summary>
        public int UndoCount
        {
            get { return _undoStack.Count; }
        }

        /// <summary>
        /// How many redos in the stack.
        /// </summary>
        public int RedoCount
        {
            get { return _redoStack.Count; }
        }

        /// <summary>
        /// If <c>true</c>, all calls to mark changes are ignored.
        /// </summary>
        public bool IsTrackingEnabled { get; set; }

        /// <summary>
        /// Returns <c>true</c> if a batch is being already begun but not ended.
        /// </summary>
        public bool IsInBatch
        {
            get { return _currentBatch != null; }
        }

        /// <summary>
        /// Resets the state of this <see cref="Mementor"/> object.
        /// </summary>
        public void Reset()
        {
            _undoStack.Clear();
            _redoStack.Clear();
            _currentBatch = null;
            IsTrackingEnabled = true;
            NotifyChange();
        }
#endregion

#region Private
        private void ReverseEvent(IEvent @event, bool undoing)
        {
            if (@event is BatchEvent)
            {
                BeginBatch();
                try
                {
                    var batch = (BatchEvent)@event;
                    while (batch.Count > 0)
                    {
                        var concreteChange = (AtomicEvent)batch.Pop();
                        ReverseEvent(concreteChange, true /* doesn't matter what this value is */);
                    }
                }
                finally
                {
                    InternalEndBatch(undoing ? _redoStack : _undoStack);
                }
            }
            else
            {
                var stackToUse = _currentBatch ?? (undoing ? _redoStack : _undoStack);
                if (@event is PropertyChangeEvent)
                {
                    var pce = (PropertyChangeEvent)@event;
                    LogPropertyChange(
                        stackToUse, 
                        pce.TargetObject, 
                        pce.PropertyName, 
                        pce.TargetObject.GetPropertyValue(pce.PropertyName));
                    ExecuteNoTracking(()
                        => pce.TargetObject.SetPropertyValue(pce.PropertyName, pce.PropertyValue));
                }

                else if (@event is ElementAdditionEvent)
                {
                    var eae = (ElementAdditionEvent)@event;
                    LogElementRemove(stackToUse, eae.TargetObject, eae.Element);
                    ExecuteNoTracking(() => eae.TargetObject.CallMethod("Remove",
                                      new[] { eae.TargetObject.GetType().GetGenericArguments()[0] },
                                      eae.Element));
                }

                else if (@event is ElementRemovalEvent)
                {
                    var ere = (ElementRemovalEvent)@event;
                    LogElementAdd(stackToUse, ere.TargetObject, ere.Element);
                    ExecuteNoTracking(() => ere.TargetObject.CallMethod("Insert", ere.Index, ere.Element));
                }

                else if (@event is ElementIndexChangeEvent)
                {
                    var epce = (ElementIndexChangeEvent)@event;
                    var collection = epce.TargetObject;
                    var element = epce.Element;
                    LogElementIndexChange(stackToUse, collection, element);
                    ExecuteNoTracking(() => {
                        collection.CallMethod("Remove", element);
                        collection.CallMethod("Insert", epce.Index, element);
                    });
                }

                else
                {
                    throw new ArgumentException("Not supported event type");
                }
            }
        }

        private void LogPropertyChange(BatchEvent batch, object target, string propertyName, object previousValue)
        {
            previousValue = previousValue ?? target.GetPropertyValue(propertyName);

            batch.Push(new PropertyChangeEvent
            {
                TargetObject = target,
                PropertyName = propertyName,
                PropertyValue = previousValue
            });
        }

        private void LogElementAdd(BatchEvent batch, object collection, object element)
        {
            batch.Push(new ElementAdditionEvent
            {
                TargetObject = collection,
                Element = element
            });
        }

        private void LogElementRemove(BatchEvent batch, object collection, object element, int? index = null)
        {
            index = index ?? (int)collection.CallMethod("IndexOf", element);
            if (index == -1)
                throw new ArgumentException("Must provide a valid index if element does not exist in the collection");
            
            batch.Push(new ElementRemovalEvent
            {
                TargetObject = collection,
                Element = element,
                Index = index.Value
            });
        }

        private void LogElementIndexChange(BatchEvent batch, object collection, object element, int? index = null)
        {
            index = index ?? (int)collection.CallMethod("IndexOf", element);
            if (index == -1) 
                throw new ArgumentException("Must provide a valid index if element does not exist in the collection");
            
            batch.Push(new ElementIndexChangeEvent 
            {
                TargetObject = collection,
                Element = element,
                Index = index.Value
            });
        }

        private void InternalBeginBatch()
        {
            _currentBatch = new BatchEvent();
        }

        private bool InternalEndBatch(BatchEvent batch)
        {
            if (_currentBatch.Count > 0)
            {
                batch.Push(_currentBatch);
                _currentBatch = null;
                return true;
            }
            return false;
        }

        private bool CheckPreconditions(params object[] objects)
        {
            if (!IsTrackingEnabled) return false;

            for (int i = 0; i < objects.Length; i += 2)
                if (objects[i] == null) 
                    throw new ArgumentNullException((string)objects[i + 1]);
            
            return true;
        }

        private void PerformLogPostAction()
        {
            _redoStack.Clear();
            NotifyChange();
        }

        private void NotifyChange(IEvent @event = null)
        {
            if (Changed != null)
                Changed(this, new MementorChangedEventArgs { Event = @event });
        }
#endregion
    }
}