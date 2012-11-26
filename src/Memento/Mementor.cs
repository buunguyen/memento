namespace Memento
{
    using System;
    using System.Linq;

    /// <summary>
    /// Provides undo and redo services.
    /// </summary>
    public class Mementor : IDisposable
    {
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
        /// <param name="isEnabled">Whether this instance is enabled or not.</param>
        public Mementor(bool isEnabled = true)
        {
            IsTrackingEnabled = isEnabled;
        }

        #region Core

        /// <summary>
        /// Marks an event. This method also serves as an extensibility point for custom events.
        /// </summary>
        /// <param name="anEvent">The event to be marked.</param>
        public void MarkEvent(BaseEvent anEvent)
        {
            if (!IsTrackingEnabled) return;
            if (anEvent == null) throw new ArgumentNullException("anEvent");
            (_currentBatch ?? _undoStack).Push(anEvent);
            if (!IsInBatch) PerformPostMarkAction(anEvent);
        }

        /// <summary>
        /// Marks a batch during which all events are combined so that <see cref="Undo"/> only needs calling once.
        /// </summary>
        /// <param name="codeBlock">The code block performing batch change marking.</param>
        /// <seealso cref="BeginBatch"/>
        /// <remarks>Batches cannot be nested. At any point, there must be only one active batch.</remarks>
        public void Batch(Action codeBlock)
        {
            if (!IsTrackingEnabled) return;
            if (codeBlock == null) throw new ArgumentNullException("codeBlock");

            BeginBatch();
            try {
                codeBlock();
            }
            finally {
                // Must not call EndBatch() because CheckPreconditions() might return false
                BaseEvent @event = InternalEndBatch(_undoStack);
                if (@event != null)
                    PerformPostMarkAction(@event);
            }
        }

        /// <summary>
        /// Explicitly marks the beginning of a batch. Use this instead of <see cref="Batch"/>
        /// changes can be made in different places instead of inside one certain block of code.
        /// When finish, end the batch by invoking <see cref="EndBatch"/>.
        /// </summary>
        public void BeginBatch()
        {
            if (!IsTrackingEnabled) return;
            if (IsInBatch) throw new InvalidOperationException("Re-entrant batch is not supported");

            _currentBatch = new BatchEvent();
        }

        /// <summary>
        /// Ends a batch.
        /// </summary>
        public void EndBatch()
        {
            if (!IsTrackingEnabled) return;
            if (!IsInBatch) throw new InvalidOperationException("A batch has not been started yet");

            BaseEvent @event = InternalEndBatch(_undoStack);
            if (@event != null)
                PerformPostMarkAction(@event);
        }

        /// <summary>
        /// Executes the supplied code block with <see cref="IsTrackingEnabled"/> turned off.
        /// </summary>
        /// <param name="codeBlock">The code block to be executed.</param>
        /// <seealso cref="IsTrackingEnabled"/>
        public void ExecuteNoTrack(Action codeBlock)
        {
            var previousState = IsTrackingEnabled;
            IsTrackingEnabled = false;
            try {
                codeBlock();
            }
            finally {
                IsTrackingEnabled = previousState;
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
            RollbackEvent(@event is BatchEvent ? new BatchEvent((BatchEvent) @event) : @event, true);
            NotifyChange(@event);
        }

        /// <summary>
        /// Performs a redo.
        /// </summary>
        public void Redo()
        {
            if (!CanRedo) throw new InvalidOperationException("There is nothing to redo");
            if (IsInBatch) throw new InvalidOperationException("Finish the active batch first");

            var @event = _redoStack.Pop();
            RollbackEvent(@event is BatchEvent ? new BatchEvent((BatchEvent) @event) : @event, false);
            NotifyChange(@event);
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
        /// <seealso cref="ExecuteNoTrack"/>
        public bool IsTrackingEnabled { get; set; }

        /// <summary>
        /// Returns <c>true</c> if a batch is being already begun but not ended.
        /// </summary>
        public bool IsInBatch
        {
            get { return _currentBatch != null; }
        }

        /// <summary>
        /// Resets the state of this <see cref="Mementor"/> object to its initial state.
        /// This effectively clears the redo stack, undo stack and current batch (if one is active).
        /// </summary>
        public void Reset()
        {
            bool shouldNotify = UndoCount > 0 || RedoCount > 0;
            _undoStack.Clear();
            _redoStack.Clear();
            _currentBatch = null;
            IsTrackingEnabled = true;
            if (shouldNotify) NotifyChange(null);
        }

        /// <summary>
        /// Disposes the this mementor and clears redo and undo stacks.
        /// This method won't fire <see cref="Changed"/> event.
        /// </summary>
        public void Dispose()
        {
            Changed = null;
            _undoStack.Clear();
            _redoStack.Clear();
            _currentBatch = null;
        }

        #endregion

        #region Private

        private void RollbackEvent(BaseEvent @event, bool undoing)
        {
            ExecuteNoTrack(() => {
                var reverse = @event.Rollback();
                if (reverse == null) return;
                if (reverse is BatchEvent)
                {
                    if (!(@event is BatchEvent))
                        throw new InvalidOperationException("Must not return BatchEvent in Rollback()");
                    reverse = ProcessBatch((BatchEvent) reverse);
                    if (reverse == null) return;
                }
                (undoing ? _redoStack : _undoStack).Push(reverse);
            });
        }

        private BaseEvent InternalEndBatch(BatchEvent stack)
        {
            BaseEvent processed = ProcessBatch(_currentBatch);
            if (processed != null) stack.Push(processed);
            _currentBatch = null;
            return processed;
        }

        private BaseEvent ProcessBatch(BatchEvent batchEvent)
        {
            if (batchEvent.Count == 0) return null;
            if (batchEvent.Count == 1) return batchEvent.Pop();
            return batchEvent;
        }

        private void PerformPostMarkAction(BaseEvent @event)
        {
            _redoStack.Clear();
            NotifyChange(@event);
        }

        private void NotifyChange(BaseEvent @event)
        {
            if (Changed != null) Changed(this, new MementorChangedEventArgs {Event = @event});
        }

        #endregion
    }
}