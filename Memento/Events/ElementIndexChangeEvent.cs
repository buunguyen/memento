namespace Memento
{
    /// <summary>
    /// Represents a collection element index change event.
    /// </summary>
    public class ElementIndexChangeEvent : AtomicEvent
    {
        /// <summary>
        /// The element whose index was changed.
        /// </summary>
        public object Element { get; set; }

        /// <summary>
        /// The index to be restored too when undo.
        /// </summary>
        public int Index { get; set; }
    }
}
