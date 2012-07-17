namespace Memento.Test.Stubs
{
    using System.Collections.Generic;

    internal class CustomEvent : BaseEvent
    {
        public bool IsRolledback { get; set; }
        public CustomEvent ReverseEvent { get; set; }

        public CustomEvent(CustomEvent reverseEvent)
        {
            ReverseEvent = reverseEvent;
        }

        protected override IEnumerable<BaseEvent> Rollback()
        {
            IsRolledback = true;
            yield return ReverseEvent;
        }
    }
}