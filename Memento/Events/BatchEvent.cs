namespace Memento
{
    using System.Collections.Generic;

    /// <summary>
    /// Represents a batch of events.
    /// </summary>
    public class BatchEvent : Stack<IEvent>, IEvent
    {
        public BatchEvent()
        {    
        }

        public BatchEvent(IEnumerable<IEvent> other)
            : base(other)
        {
        }
    }
}
