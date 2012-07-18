namespace Memento.Test
{
    /// <summary>
    /// The same mementor instance is usually used throughout the application 
    /// or editor session, therefore there should be some centralized way to 
    /// get that instance.
    /// </summary>
    internal static class Session
    {
        public static Mementor Mementor;

        public static Mementor New()
        {
            return Mementor = new Mementor();
        }

        public static void End()
        {
            Mementor.Dispose();
            Mementor = null;
        }
    }
}