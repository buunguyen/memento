namespace Memento
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;

    /// <summary>
    /// Convenient extension methods to mark built-in events.
    /// </summary>
    public static class MementorExtensions
    {
        /// <summary>
        /// Marks a property change event. 
        /// </summary>
        /// <typeparam name="TProp">The type of the property.</typeparam>
        /// <param name="mementor">The mementor object.</param>
        /// <param name="target">The target object.</param>
        /// <param name="propertySelector">The property selector expression.</param>
        /// <param name="previousValue">The value to be restored to when undo. 
        /// If not supplied, will be retrieved directly from the <paramref name="target"/>.</param>
        public static void PropertyChange<TProp>(this Mementor mementor, 
            object target, Expression<Func<TProp>> propertySelector, object previousValue = null)
        {
            if (mementor.IsTrackingEnabled) 
                PropertyChange(mementor, target, propertySelector.Name(), previousValue);
        }

        private static string Name<TProp>(this Expression<Func<TProp>> propertySelector)
        {
            return ((MemberExpression)propertySelector.Body).Member.Name;
        }

        /// <summary>
        /// Marks a property change event.
        /// </summary>
        /// <param name="mementor">The mementor object.</param>
        /// <param name="target">The target object.</param>
        /// <param name="propertyName">The name of the property.</param>
        /// <param name="previousValue">The value to be restored to when undo. 
        /// If not supplied, will be retrieved directly from the <paramref name="target"/>.</param>
        public static void PropertyChange(this Mementor mementor, 
            object target, string propertyName, object previousValue = null)
        {
            if (mementor.IsTrackingEnabled) 
                mementor.MarkEvent(new PropertyChangeEvent(target, propertyName, previousValue));
        }

        /// <summary>
        /// Marks an element addition event. 
        /// </summary>
        /// <typeparam name="T">The generic type parameter of the collection.</typeparam>
        /// <param name="mementor">The mementor object.</param>
        /// <param name="collection">The collection object.</param>
        /// <param name="element">The element being added.</param>
        public static void ElementAdd<T>(this Mementor mementor, 
            IList<T> collection, T element)
        {
            if (mementor.IsTrackingEnabled) 
                mementor.MarkEvent(new ElementAdditionEvent<T>(collection, element));
        }

        /// <summary>
        /// Marks an element removal event.
        /// </summary>
        /// <typeparam name="T">The generic type parameter of the collection.</typeparam>
        /// <param name="mementor">The mementor object.</param>
        /// <param name="collection">The collection object.</param>
        /// <param name="element">The element being removed</param>
        /// <param name="elementIndex">The index of the element being removed. If not supplied, will retrieve via <see cref="IList{T}.IndexOf"/>.</param>
        public static void ElementRemove<T>(this Mementor mementor, 
            IList<T> collection, T element, int? elementIndex = null)
        {
            if (mementor.IsTrackingEnabled) 
                mementor.MarkEvent(new ElementRemovalEvent<T>(collection, element, elementIndex));
        }

        /// <summary>
        /// Marks an element index change event.
        /// </summary>
        /// <typeparam name="T">The generic type parameter of the collection.</typeparam>
        /// <param name="mementor">The mementor object.</param>
        /// <param name="collection">The collection object.</param>
        /// <param name="element">The element whose index is being changed</param>
        /// <param name="elementIndex">The index of the element being removed. If not supplied, will retrieve via <see cref="IList{T}.IndexOf"/>.</param>
        public static void ElementIndexChange<T>(this Mementor mementor, 
            IList<T> collection, T element, int? elementIndex = null)
        {
            if (mementor.IsTrackingEnabled) 
                mementor.MarkEvent(new ElementIndexChangeEvent<T>(collection, element, elementIndex));
        }
    }
}
