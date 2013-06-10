// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Microsoft.Phone.Controls
{
    /// <summary>
    /// Provides helper methods to work with LongListSelector.
    /// </summary>
    internal static class LongListSelectorExtensions
    {
        /// <summary>
        /// Gets the items that are currently in the viewport of a 
        /// <see cref="T:LongListSelector" /> and adds them into a list of weak references.
        /// </summary>
        /// <param name="list">
        /// The LongListSelector instance to search on.
        /// </param>
        /// <param name="items">
        /// The list of weak references where the items in 
        /// the viewport will be added.
        /// </param>
        public static void GetItemsInViewPort(this LongListSelector list, IList<WeakReference> items)
        {
            DependencyObject child = list;
            int childCount;

            childCount = VisualTreeHelper.GetChildrenCount(list);
            if (childCount == 0)
            {
                // no child yet
                return;
            }

            list.UpdateLayout();

            Canvas headersPanel;

            do
            {
                child = VisualTreeHelper.GetChild(child, 0);
                childCount = VisualTreeHelper.GetChildrenCount(child);
                headersPanel = child as Canvas;
            } while ((headersPanel == null) && childCount > 0);

            if (headersPanel != null &&
                childCount > 0)
            {
                Canvas itemsPanel = VisualTreeHelper.GetChild(headersPanel, 0) as Canvas;
                if (itemsPanel != null)
                {
                    var itemsInList = new List<KeyValuePair<double, FrameworkElement>>();

                    AddVisibileContainers(list, itemsPanel, itemsInList, /* selectContent = */ false);
                    AddVisibileContainers(list, headersPanel, itemsInList, /* selectContent = */ true);

                    foreach (var pair in itemsInList.OrderBy(selector => selector.Key))
                    {
                        items.Add(new WeakReference(pair.Value));
                    }
                }
            }
        }

        /// <summary>
        /// Retrieves the visible containers in a LongListSelector and adds them to <paramref name="items"/>.
        /// </summary>
        /// <param name="list">LongListSelector that contains the items.</param>
        /// <param name="itemsPanel">Direct parent of the items.</param>
        /// <param name="items">List to populate with the containers currently in the viewport</param>
        /// <param name="selectContent">
        /// Specifies whether to return the container or its content.
        /// For headers, we can't apply projections on the container directly 
        /// (or everything will go blank), so we will apply them on the 
        /// content instead.
        /// </param>
        private static void AddVisibileContainers(LongListSelector list, Canvas itemsPanel, List<KeyValuePair<double, FrameworkElement>> items, bool selectContent)
        {
            foreach (DependencyObject obj in itemsPanel.GetVisualChildren())
            {
                ContentPresenter container = obj as ContentPresenter;
                if (container != null &&
                    (!selectContent ||
                    (VisualTreeHelper.GetChildrenCount(container) == 1 &&
                     VisualTreeHelper.GetChild(container, 0) is FrameworkElement)))
                {
                    GeneralTransform itemTransform = null;
                    try
                    {
                        itemTransform = container.TransformToVisual(list);
                    }
                    catch (ArgumentException)
                    {
                        // Ignore failures when not in the visual tree
                        break;
                    }

                    Rect boundingBox = new Rect(itemTransform.Transform(new Point()), itemTransform.Transform(new Point(container.ActualWidth, container.ActualHeight)));

                    if (boundingBox.Bottom > 0 && boundingBox.Top < list.ActualHeight)
                    {
                        items.Add(
                            new KeyValuePair<double, FrameworkElement>(
                                boundingBox.Top, 
                                selectContent 
                                ? (FrameworkElement)VisualTreeHelper.GetChild(container, 0) 
                                : container));
                    }
                }
            }
        }
    }
}
