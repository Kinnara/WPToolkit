// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace Microsoft.Phone.Controls
{
    /// <summary>
    /// Provides helper methods to work with LongListSelector.
    /// </summary>
    public static class LongListSelectorExtensions
    {
        /// <summary>
        /// Gets the items that are currently in the view port
        /// of an Control.
        /// </summary>
        /// <param name="list">The Control to search on.</param>
        /// <returns>
        /// A list of weak references to the items in the view port.
        /// </returns>
        public static IList<WeakReference> GetItemsInViewPort(Control list)
        {
            IList<WeakReference> viewPortItems = new List<WeakReference>();

            GetItemsInViewPort(list, viewPortItems);

            return viewPortItems;
        }

        /// <summary>
        /// Gets the items that are currently in the view port
        /// of an Control and adds them
        /// into a list of weak references.
        /// </summary>
        /// <param name="list">
        /// The Control to search on.
        /// </param>
        /// <param name="items">
        /// The list of weak references where the items in 
        /// the view port will be added.
        /// </param>
        public static void GetItemsInViewPort(Control list, IList<WeakReference> items)
        {
            int index;
            FrameworkElement container;
            GeneralTransform itemTransform;
            Rect boundingBox;

            if (VisualTreeHelper.GetChildrenCount(list) == 0)
            {
                // no child yet
                return;
            }

            var scrollHost = list.GetFirstLogicalChildByType<ViewportControl>(false);

            list.UpdateLayout();

            if (scrollHost == null)
            {
                return;
            }

            var contentPanel = scrollHost.Content as Canvas;
            if (contentPanel == null || contentPanel.Children.Count == 0)
            {
                return;
            }

            var itemsPanel = contentPanel.Children[0] as Canvas;
            if (itemsPanel == null)
            {
                return;
            }

            var containers = contentPanel.Children
                .OfType<ContentPresenter>()
                .Concat(itemsPanel.Children.OfType<ContentPresenter>())
                .OrderBy(c => Canvas.GetTop(c))
                .Select(c => c.GetFirstLogicalChildByType<FrameworkElement>(false))
                .OfType<FrameworkElement>()
                .ToList();

            for (index = 0; index < containers.Count; index++)
            {
                container = containers[index];
                if (container != null)
                {
                    itemTransform = null;
                    try
                    {
                        itemTransform = container.TransformToVisual(scrollHost);
                    }
                    catch (ArgumentException)
                    {
                        // Ignore failures when not in the visual tree
                        return;
                    }

                    boundingBox = new Rect(itemTransform.Transform(new Point()), itemTransform.Transform(new Point(container.ActualWidth, container.ActualHeight)));

                    if (boundingBox.Bottom > 0)
                    {
                        items.Add(new WeakReference(container));
                        index++;
                        break;
                    }
                }
            }

            for (; index < containers.Count; index++)
            {
                container = containers[index];
                itemTransform = null;
                try
                {
                    itemTransform = container.TransformToVisual(scrollHost);
                }
                catch (ArgumentException)
                {
                    // Ignore failures when not in the visual tree
                    return;
                }

                boundingBox = new Rect(itemTransform.Transform(new Point()), itemTransform.Transform(new Point(container.ActualWidth, container.ActualHeight)));

                if (boundingBox.Top < scrollHost.ActualHeight)
                {
                    items.Add(new WeakReference(container));
                }
                else
                {
                    break;
                }
            }

            return;
        }
    }
}
