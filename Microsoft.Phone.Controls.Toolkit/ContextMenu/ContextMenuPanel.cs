using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Microsoft.Phone.Controls
{
    /// <summary>
    /// Implements a custom Panel for the ContextMenu control.
    /// </summary>
    public class ContextMenuPanel : Panel
    {
        private const double LandscapeOffset = 58;

        /// <summary>
        /// Stores a reference to the current root visual.
        /// </summary>
        private PhoneApplicationFrame _rootVisual;

        /// <summary>
        /// Handles the measure pass for the control.
        /// </summary>
        /// 
        /// <returns>
        /// The desired size.
        /// </returns>
        /// <param name="availableSize">The available size.</param>
        protected sealed override Size MeasureOverride(Size availableSize)
        {
            Size stackDesiredSize = new Size();
            UIElementCollection children = Children;
            Size layoutSlotSize = availableSize;

            double logicalVisibleSpace, childLogicalSize;

            //
            // Initialize child sizing and iterator data
            // Allow children as much size as they want along the stack. 
            //
            layoutSlotSize.Height = Double.PositiveInfinity;
            logicalVisibleSpace = availableSize.Height;

            //
            //  Iterate through children. 
            // 
            for (int i = 0, count = children.Count; i < count; ++i)
            {
                // Get next child. 
                UIElement child = children[i];

                if (child == null) { continue; }

                // Measure the child.
                child.Measure(layoutSlotSize);
                Size childDesiredSize = child.DesiredSize;

                // Accumulate child size. 
                stackDesiredSize.Width = Math.Max(stackDesiredSize.Width, childDesiredSize.Width);
                stackDesiredSize.Height += childDesiredSize.Height;
                childLogicalSize = childDesiredSize.Height;
            }

            if ((_rootVisual != null || PhoneHelper.TryGetPhoneApplicationFrame(out _rootVisual)))
            {
                if (_rootVisual.IsPortrait())
                {
                    switch (children.Count)
                    {
                        case 2:
                        case 4:
                            stackDesiredSize.Height += stackDesiredSize.Height / children.Count;
                            break;
                    }
                }
                else
                {
                    stackDesiredSize.Height = _rootVisual.ActualWidth;

                    ItemsPresenter itemsPresenter = VisualTreeHelper.GetParent(this) as ItemsPresenter;
                    if (itemsPresenter != null)
                    {
                        stackDesiredSize.Height -= itemsPresenter.Margin.Top + itemsPresenter.Margin.Bottom;
                    }
                }
            }

            return stackDesiredSize;
        }

        /// <summary>
        /// Handles the arrange pass for the control.
        /// </summary>
        /// 
        /// <returns>
        /// The render size.
        /// </returns>
        /// <param name="finalSize">The final size.</param>
        protected sealed override Size ArrangeOverride(Size finalSize)
        {
            UIElementCollection children = Children;
            Rect rcChild = new Rect(new Point(), finalSize);
            double previousChildSize = 0.0;

            if ((_rootVisual != null || PhoneHelper.TryGetPhoneApplicationFrame(out _rootVisual)) && !_rootVisual.IsPortrait())
            {
                rcChild.Y = LandscapeOffset;
            }

            //
            // Arrange and Position Children. 
            //
            for (int i = 0, count = children.Count; i < count; ++i)
            {
                UIElement child = (UIElement)children[i];

                if (child == null) { continue; }

                rcChild.Y += previousChildSize;
                previousChildSize = child.DesiredSize.Height;
                rcChild.Height = previousChildSize;
                rcChild.Width = Math.Max(finalSize.Width, child.DesiredSize.Width);

                child.Arrange(rcChild);
            }
            return finalSize;
        }
    }
}
