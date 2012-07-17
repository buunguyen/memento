namespace Memento
{
    /// <summary>
    /// Represents argument of the <see cref="MementorChanged"/> event.
    /// </summary>
    public class MementorChangedEventArgs
    {
        /// <summary>
        /// The event associated with the the event.
        /// </summary>
        public BaseEvent Event { get; set; }
    }
}