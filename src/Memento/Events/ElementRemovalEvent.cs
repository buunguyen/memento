namespace Memento
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Represents a collection element removal event.
    /// </summary>
    public sealed class ElementRemovalEvent<T> : BaseEvent
    {
        /// <summary>
        /// The collection this event occurs on.
        /// </summary>
        public IList<T> Collection { get; private set; }

        /// <summary>
        /// The element removed.
        /// </summary>
        public T Element { get; private set; }

        /// <summary>
        /// The index to be restored too when undo.
        /// </summary>
        public int Index { get; private set; }

        /// <summary>
        /// Creates the event.
        /// </summary>
        /// <param name="collection">The collection object.</param>
        /// <param name="element">The element to be removed.</param>
        /// <param name="index"/>The index of the element in the collection. If not supplied, use current index of <paramref name="element"/> in the <paramref name="collection"/>.
        public ElementRemovalEvent(IList<T> collection, T element, int? index = null)
        {
            if (collection == null) throw new ArgumentNullException("collection");
            index = index ?? collection.IndexOf(element);
            if (index == -1)
                throw new ArgumentException("Must provide a valid index if element does not exist in the collection");

            Collection = collection;
            Element = element;
            Index = index.Value;
        }

        protected internal override BaseEvent Rollback()
        {
            var reverse = new ElementAdditionEvent<T>(Collection, Element);
            Collection.Insert(Index, Element);
            return reverse;
        }
    }
}