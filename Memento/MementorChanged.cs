namespace Memento
{
    /// <summary>
    /// Fired whenever there is a change in <see cref="Mementor"/>.
    /// Spefically, these actions will trigger this event:
    /// <list type="bullet">
    /// <item><description>Any change logged by client code, e.g. property change, collection addition etc.</description></item>
    /// <item><description><see cref="Mementor"/>'s: undo, redo, reset</description></item>
    /// </list>
    /// </summary>
    /// <param name="sender">The firing mementor object.</param>
    /// <param name="args">The event argument.</param>
    public delegate void MementorChanged(Mementor sender, MementorChangedEventArgs args);
}