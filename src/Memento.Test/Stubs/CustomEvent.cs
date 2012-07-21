namespace Memento.Test.Stubs
{
    internal class CustomEvent : BaseEvent
    {
        public bool IsRolledback { get; set; }
        public CustomEvent ReverseEvent { get; set; }

        public CustomEvent(CustomEvent reverseEvent)
        {
            ReverseEvent = reverseEvent;
        }

        protected override BaseEvent Rollback()
        {
            IsRolledback = true;
            return ReverseEvent;
        }
    }
}