using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Windows;
using System.Windows.Controls;

namespace VaporDAW
{
    public static class ListViewExtensions
    {
        public static void CheckDragDropMove<T>(this ListView listView, Vector diff, Object originalSource, string dataObjectFormat)
        {
            // Get the dragged ListViewItem
            ListViewItem listViewItem =
                FindAncestor<ListViewItem>((DependencyObject)originalSource);

            if (listViewItem != null && (Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance || Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance))
            {

                // Find the data behind the ListViewItem
                T data = (T)listView.ItemContainerGenerator.ItemFromContainer(listViewItem);

                // Initialize the drag & drop operation
                DataObject dragData = new DataObject(dataObjectFormat, data);
                DragDrop.DoDragDrop(listViewItem, dragData, DragDropEffects.Copy);
            }
        }
        private static T FindAncestor<T>(DependencyObject current) where T : DependencyObject
        {
            do
            {
                if (current is T)
                {
                    return (T)current;
                }
                current = System.Windows.Media.VisualTreeHelper.GetParent(current);
            }
            while (current != null);
            return null;
        }
    }
}
