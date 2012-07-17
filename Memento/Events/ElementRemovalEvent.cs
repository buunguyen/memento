namespace Memento
{
    /// <summary>
    /// Represents a collection element removal event.
    /// </summary>
    public class ElementRemovalEvent : AtomicEvent
    {
        /// <summary>
        /// The element removed.
        /// </summary>
        public object Element { get; set; }

        /// <summary>
        /// The index to be restored too when undo.
        /// </summary>
        public int Index { get; set; }
    }
}
