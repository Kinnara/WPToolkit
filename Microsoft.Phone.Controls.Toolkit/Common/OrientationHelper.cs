using System.Windows;
using System.Windows.Controls;

namespace Microsoft.Phone.Controls
{
    internal class OrientationHelper
    {
        private PhoneApplicationFrame _frame;

        public OrientationHelper(Control control)
        {
            Control = control;

            Control.Loaded += OnLoaded;
            Control.Unloaded += OnUnloaded;
        }

        public Control Control { get; private set; }

        public void OnApplyTemplate()
        {
            UpdateVisualState();
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            UpdateVisualState();

            if (PhoneHelper.TryGetPhoneApplicationFrame(out _frame))
            {
                _frame.OrientationChanged += OnOrientationChanged;
            }
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            if (_frame != null)
            {
                _frame.OrientationChanged -= OnOrientationChanged;
                _frame = null;
            }
        }

        private void OnOrientationChanged(object sender, OrientationChangedEventArgs e)
        {
            UpdateVisualState();
        }

        private void UpdateVisualState()
        {
            PhoneApplicationFrame frame;
            if (PhoneHelper.TryGetPhoneApplicationFrame(out frame))
            {
                VisualStateManager.GoToState(Control, frame.Orientation.ToString(), false);
            }
        }
    }
}
