using System.Diagnostics.CodeAnalysis;
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

        internal void OnIsSelectedChanged(bool newValue)
        {
            if (ParentFlipView != null)
            {
                ParentFlipView.NotifyItemSelected(this, newValue);
            }
        }

        /// <summary>
        /// Called when the <see cref="E:System.Windows.UIElement.ManipulationStarted" /> event occurs.
        /// This member overrides
        /// <see cref="M:System.Windows.Controls.Control.OnManipulationStarted(System.Windows.Input.ManipulationStartedEventArgs)" />.
        /// </summary>
        /// <param name="e">Event data for the event.</param>
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Justification = "Standard pattern.")]
        protected override void OnManipulationStarted(System.Windows.Input.ManipulationStartedEventArgs e)
        {
            base.OnManipulationStarted(e);

            if (ParentFlipView != null)
            {
                ParentFlipView.OnManipulationStarted(this, e);
            }

            e.Handled = true;
        }

        /// <summary>
        /// Called when the <see cref="E:System.Windows.UIElement.ManipulationDelta" /> event occurs.
        /// This member overrides
        /// <see cref="M:System.Windows.Controls.Control.OnManipulationDelta(System.Windows.Input.ManipulationDeltaEventArgs)" />.
        /// </summary>
        /// <param name="e">Event data for the event.</param>
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Justification = "Standard pattern.")]
        protected override void OnManipulationDelta(System.Windows.Input.ManipulationDeltaEventArgs e)
        {
            base.OnManipulationDelta(e);

            if (ParentFlipView != null)
            {
                ParentFlipView.OnManipulationDelta(this, e);
            }

            e.Handled = true;
        }

        /// <summary>
        /// Called when the <see cref="E:System.Windows.UIElement.ManipulationCompleted" /> event occurs.
        /// This member overrides
        /// <see cref="M:System.Windows.Controls.Control.OnManipulationCompleted(System.Windows.Input.ManipulationCompletedEventArgs)" />.
        /// </summary>
        /// <param name="e">Event data for the event.</param>
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Justification = "Standard pattern.")]
        protected override void OnManipulationCompleted(System.Windows.Input.ManipulationCompletedEventArgs e)
        {
            base.OnManipulationCompleted(e);

            if (ParentFlipView != null)
            {
                ParentFlipView.OnManipulationCompleted(this, e);
            }

            e.Handled = true;
        }
    }
}
