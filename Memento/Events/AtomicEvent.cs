namespace Memento
{
    /// <summary>
    /// Represents an event.
    /// </summary>
    public abstract class AtomicEvent : IEvent
    {
        /// <summary>
        /// The target object this event occurs on.
        /// </summary>
        public object TargetObject { get; set; }
    }
}