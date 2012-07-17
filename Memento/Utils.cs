namespace Memento
{
    using System;
    using System.Linq.Expressions;

    internal static class Utils
    {
        public static string Name<TProp>(this Expression<Func<TProp>> propertySelector)
        {
            return ((MemberExpression)propertySelector.Body).Member.Name;
        }
    }
}
