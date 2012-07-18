namespace Memento
{
    using System.Collections.Generic;

    /// <summary>
    /// Must be implemented by all events.
    /// </summary>
    public abstract class BaseEvent
    {
        /// <summary>
        /// <para>Rollback this event. This method is executed with 
        /// <see cref="Mementor.IsTrackingEnabled"/> off, so no change marking will be done during its execution.</para>
        /// 
        /// <para>Because undo and redo are symmetric, this method might return one or more 
        /// "reverse events" which will be used to rollback the effect of the current method.</para>
        /// </summary>
        /// <returns>One or more symmetric reverse events for this rollback action.</returns>
        protected internal abstract IEnumerable<BaseEvent> Rollback();
    }
}
