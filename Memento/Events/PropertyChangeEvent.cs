namespace Memento
{
    /// <summary>
    /// Represents a property value change event.
    /// </summary>
    public class PropertyChangeEvent : AtomicEvent
    {
        /// <summary>
        /// The name of the changed property.
        /// </summary>
        public string PropertyName { get; set; }

        /// <summary>
        /// The value to be restored to when undo.
        /// </summary>
        public object PropertyValue { get; set; }
    }
}
