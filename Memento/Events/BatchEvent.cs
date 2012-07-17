namespace Memento
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Represents a batch of events.
    /// </summary>
    public sealed class BatchEvent : BaseEvent, IEnumerable<BaseEvent>
    {
        private readonly Stack<BaseEvent> _events;

        internal BatchEvent(BatchEvent other = null)
        {
            _events = other == null
                          ? new Stack<BaseEvent>()
                          : new Stack<BaseEvent>(other._events.Reverse());
        }

        /// <summary>
        /// The number of events in this batch.
        /// </summary>
        public int Count
        {
            get { return _events.Count; }
        }

        internal BaseEvent Pop()
        {
            return _events.Pop();
        }

        internal void Push(BaseEvent @event)
        {
            _events.Push(@event);
        }

        internal void Clear()
        {
            _events.Clear();
        }

        protected internal override IEnumerable<BaseEvent> Rollback()
        {
            while (Count > 0)
                foreach (var reverseEvent in Pop().Rollback())
                    yield return reverseEvent;
        }

        /// <summary>
        /// Returns the enumerator to access child events.
        /// </summary>
        /// <returns>The enumerator to access child events.</returns>
        public IEnumerator<BaseEvent> GetEnumerator()
        {
            return _events.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}