using System;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Microsoft.Phone.Controls
{
    /// <summary>
    /// Displays the content of a <see cref="T:Microsoft.Phone.Controls.Scroller"/> control.
    /// </summary>
    [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Scroller")]
    public sealed class ScrollerContentPresenter : ContentPresenter
    {
        internal Scroller ScrollOwner { get; set; }

        /// <summary>
        /// Handles the measure pass for the control.
        /// </summary>
        /// 
        /// <returns>
        /// The desired size.
        /// </returns>
        /// <param name="availableSize">The available size.</param>
        protected override Size MeasureOverride(Size availableSize)
        {
            Size desiredSize = new Size();
            int count = VisualTreeHelper.GetChildrenCount(this);

            if (count > 0)
            {
                if (ScrollOwner == null)
                {
                    desiredSize = base.MeasureOverride(availableSize);
                }
                else
                {
                    Size childConstraint = availableSize;

                    if (ScrollOwner.HorizontalScrollBarVisibility != ScrollBarVisibility.Disabled) { childConstraint.Width = Double.PositiveInfinity; }
                    if (ScrollOwner.VerticalScrollBarVisibility != ScrollBarVisibility.Disabled) { childConstraint.Height = Double.PositiveInfinity; }

                    desiredSize = base.MeasureOverride(childConstraint);
                }
            }

            return desiredSize;
        }
    }
}
