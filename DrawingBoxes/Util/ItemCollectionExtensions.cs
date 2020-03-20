using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace DrawingBoxes
{
    public static class ItemCollectionExtensions
    {
        public static IEnumerable<T> WhereIs<T>(this ItemCollection collection)
        {
            return collection.Cast<object>().Where(i => i is T).Cast<T>();
        }
    }
}
