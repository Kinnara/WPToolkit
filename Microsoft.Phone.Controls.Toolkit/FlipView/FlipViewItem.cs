using System.Windows;
using System.Windows.Controls;

namespace Microsoft.Phone.Controls
{
    /// <summary>
    /// Represents the container for an item in a <see cref="T:Microsoft.Phone.Controls.FlipView"/> control.
    /// </summary>
    public class FlipViewItem : ContentControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:Microsoft.Phone.Controls.FlipViewItem" /> class.
        /// </summary>
        public FlipViewItem()
        {
            DefaultStyleKey = typeof(FlipViewItem);
        }

        #region IsSelected

        /// <summary>
        /// Gets or sets a value that indicates whether the item is selected.
        /// </summary>
        /// 
        /// <returns>
        /// True if the item is selected; otherwise, false.
        /// </returns>
        public bool IsSelected
        {
            get { return (bool)GetValue(IsSelectedProperty); }
            set { SetValue(IsSelectedProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="P:Microsoft.Phone.Controls.FlipViewItem.IsSelected"/> dependency property.
        /// </summary>
        /// 
        /// <returns>
        /// The identifier for the <see cref="P:Microsoft.Phone.Controls.FlipViewItem.IsSelected"/> dependency property.
        /// </returns>
        public static readonly DependencyProperty IsSelectedProperty = FlipView.IsSelectedProperty;

        #endregion

        internal FlipView ParentFlipView { get; set; }

        internal object Item { get; set; }

        internal void OnIsSelectedChanged(bool oldValue, bool newValue)
        {
            if (ParentFlipView != null)
            {
                ParentFlipView.NotifyItemSelected(this, newValue);
            }
        }
    }
}
