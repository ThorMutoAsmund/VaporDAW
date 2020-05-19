using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Windows.Controls;

namespace VaporDAW
{
    public static class ItemCollectionExtensions
    {
        public static IEnumerable<T> WhereIs<T>(this ItemCollection collection)
        {
            return collection.Cast<object>().Where(i => i is T).Cast<T>();
        }
    }
    public static class UIElementCollectionExtensions
    {
        public static IEnumerable<T> WhereIs<T>(this UIElementCollection collection)
        {
            return collection.Cast<object>().Where(i => i is T).Cast<T>();
        }
    }
}
