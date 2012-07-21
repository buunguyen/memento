namespace Memento
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;

    /// <summary>
    /// Represents a property value change event.
    /// </summary>
    public sealed class PropertyChangeEvent : BaseEvent
    {
        /// <summary>
        /// The target object this event occurs on.
        /// </summary>
        public object TargetObject { get; private set; }

        /// <summary>
        /// The name of the changed property.
        /// </summary>
        public string PropertyName { get; private set; }

        /// <summary>
        /// The value to be restored to when undo.
        /// </summary>
        public object PropertyValue { get; private set; }

        /// <summary>
        /// Creates the event.
        /// </summary>
        /// <param name="target">The target object whose property is changed.</param>
        /// <param name="propertyName">The name of the property being changed.</param>
        /// <param name="propertyValue">The value of the property. If not supplied, use the current value of <paramref name="propertyName"/> in <paramref name="target"/></param>
        public PropertyChangeEvent(object target, string propertyName, object propertyValue = null)
        {
            if (target == null) throw new ArgumentNullException("target");
            if (propertyName == null) throw new ArgumentNullException("propertyName");
            TargetObject = target;
            PropertyName = propertyName;
            PropertyValue = propertyValue ?? PropertyInfo().GetValue(target, null);
        }

        protected internal override BaseEvent Rollback()
        {
            var reverse = new PropertyChangeEvent(TargetObject, PropertyName);
            PropertyInfo().SetValue(TargetObject, PropertyValue, null);
            return reverse;
        }

        private PropertyInfo PropertyInfo()
        {
            return TargetObject.GetType().GetProperty(PropertyName, 
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        }
    }
}