namespace Memento
{
    using System;

    /// <summary>
    /// Must be implemented by all events.
    /// </summary>
    public abstract class BaseEvent
    {
        /// <summary>
        /// <para>Rollback this event. This method is executed with 
        /// <see cref="Mementor.IsTrackingEnabled"/> off, so no change marking will be done during its execution.</para>
        /// <para>Because undo and redo are symmetric, this method might return a
        /// "reverse event" which will be used to rollback the effect of the current method.
        /// This method must now, however, return an isntance of <see cref="BatchEvent"/>.</para>
        /// </summary>
        /// <returns>A symmetric reverse event for this rollback action.</returns>
        protected internal abstract BaseEvent Rollback();
    }
}
