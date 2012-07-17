namespace Memento
{
    /// <summary>
    /// Fired whenever there is a change in either the undo or redo stack of a <see cref="Mementor"/> instance.
    /// Spefically, these actions will trigger this event:
    /// <list type="bullet">
    /// <item><description>Any change marked by client code</description></item>
    /// <item><description><see cref="Mementor"/>'s: <see cref="Mementor.Undo"/>, <see cref="Mementor.Redo"/> and possibly <see cref="Mementor.Reset"/> unless the undo and redo stacks are already empty.</description></item>
    /// </list>
    /// </summary>
    /// <param name="sender">The firing mementor object.</param>
    /// <param name="args">The event argument.</param>
    public delegate void MementorChanged(Mementor sender, MementorChangedEventArgs args);
}