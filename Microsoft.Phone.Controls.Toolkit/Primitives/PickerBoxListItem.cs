using System;
using System.Windows.Controls;
using System.Windows.Input;

namespace Microsoft.Phone.Controls.Primitives
{
    /// <summary>
    /// Represents a selectable item in a <see cref="T:Microsoft.Phone.Controls.Primitives.PickerBoxList"/>.
    /// </summary>
    public class PickerBoxListItem : ListBoxItem
    {
        private ClickHelper _clickHelper;

        /// <summary>
        /// Initializes a new instance of the
        /// <see cref="T:Microsoft.Phone.Controls.Primitives.PickerBoxList" /> class.
        /// </summary>
        public PickerBoxListItem()
        {
            _clickHelper = ClickHelper.Create(this);
            _clickHelper.Click += delegate { SafeRaise.Raise(Click, this); };
        }

        /// <summary>
        /// Occurs when the
        /// <see cref="Microsoft.Phone.Controls.Primitives.PickerBoxListItem"/>
        /// is clicked.
        /// </summary>
        public event EventHandler Click;

        /// <summary>
        /// Called when the <see cref="E:System.Windows.UIElement.ManipulationStarted" /> event occurs.
        /// This member overrides
        /// <see cref="M:System.Windows.Controls.Control.OnManipulationStarted(System.Windows.Input.ManipulationStartedEventArgs)" />.
        /// </summary>
        /// <param name="e">Event data for the event.</param>
        protected override void OnManipulationStarted(ManipulationStartedEventArgs e)
        {
        }

        /// <summary>
        /// Called when the <see cref="E:System.Windows.UIElement.ManipulationCompleted" /> event occurs.
        /// This member overrides
        /// <see cref="M:System.Windows.Controls.Control.OnManipulationCompleted(System.Windows.Input.ManipulationCompletedEventArgs)" />.
        /// </summary>
        /// <param name="e">Event data for the event.</param>
        protected override void OnManipulationCompleted(ManipulationCompletedEventArgs e)
        {
        }

        /// <summary>
        /// Provides handling for the <see cref="E:System.Windows.UIElement.MouseLeftButtonDown" /> event.
        /// </summary>
        /// <param name="e">The event data.</param>
        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
        }
    }
}
