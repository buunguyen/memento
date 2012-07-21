namespace Memento.Test.Stubs
{
    internal class InvalidCustomEvent : BaseEvent
    {
        public BatchEvent Batch { get; set; }

        public InvalidCustomEvent(BatchEvent batch)
        {
            Batch = batch;
        }

        protected override BaseEvent Rollback()
        {
            return Batch;
        }
    }
}