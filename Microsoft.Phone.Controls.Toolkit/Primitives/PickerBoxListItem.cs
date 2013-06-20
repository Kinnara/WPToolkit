using System;
using System.Diagnostics.CodeAnalysis;
using System.Windows.Controls;
using System.Windows.Input;

namespace Microsoft.Phone.Controls.Primitives
{
    /// <summary>
    /// Represents a selectable item in a <see cref="T:Microsoft.Phone.Controls.Primitives.PickerBoxList"/>.
    /// </summary>
    public class PickerBoxListItem : ListBoxItem
    {
        private bool _isMouseCaptured;

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
            CaptureMouseInternal();
        }

        /// <summary>
        /// Called before the <see cref="E:System.Windows.UIElement.MouseLeftButtonUp" /> event occurs.
        /// </summary>
        /// <param name="e">The data for the event.</param>
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Justification = "Standard pattern.")]
        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonUp(e);

            if (!e.Handled)
            {
                e.Handled = true;

                if (_isMouseCaptured)
                {
                    SafeRaise.Raise(Click, this);
                }

                ReleaseMouseCaptureInternal();
            }
        }

        /// <summary>
        /// Called before the <see cref="E:System.Windows.UIElement.LostMouseCapture" /> event occurs to
        /// provide handling for the event in a derived class without attaching a delegate.
        /// </summary>
        /// <param name="e">A <see cref="T:System.Windows.Input.MouseEventArgs" /> that contains the
        /// event data.</param>
        protected override void OnLostMouseCapture(MouseEventArgs e)
        {
            ReleaseMouseCaptureInternal();
        }

        private void CaptureMouseInternal()
        {
            if (_isMouseCaptured)
            {
                return;
            }

            _isMouseCaptured = CaptureMouse();
        }

        private void ReleaseMouseCaptureInternal()
        {
            ReleaseMouseCapture();
            _isMouseCaptured = false;
        }
    }
}
