namespace Memento
{
    /// <summary>
    /// Represents a collection element addition event.
    /// </summary>
    public class ElementAdditionEvent : AtomicEvent
    {
        /// <summary>
        /// The element added.
        /// </summary>
        public object Element { get; set; }
    }
}
