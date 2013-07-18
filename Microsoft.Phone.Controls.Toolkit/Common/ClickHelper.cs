using System;
using System.Windows;
using System.Windows.Input;

namespace Microsoft.Phone.Controls
{
    internal class ClickHelper
    {
        private bool _isMouseCaptured;

        private ClickHelper(UIElement target)
        {
            Target = target;
        }

        public UIElement Target { get; private set; }

        public event EventHandler Click;

        public static ClickHelper Create(UIElement target)
        {
            ClickHelper helper = new ClickHelper(target);
            helper.Start();
            return helper;
        }

        public void Start()
        {
            Target.MouseLeftButtonDown += Target_MouseLeftButtonDown;
            Target.MouseLeftButtonUp += Target_MouseLeftButtonUp;
            Target.LostMouseCapture += Target_LostMouseCapture;
        }

        private void Target_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            CaptureMouse();
        }

        private void Target_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;

            if (_isMouseCaptured)
            {
                SafeRaise.Raise(Click, this);
            }

            ReleaseMouseCapture();
        }

        private void Target_LostMouseCapture(object sender, MouseEventArgs e)
        {
            ReleaseMouseCapture();
        }

        private void CaptureMouse()
        {
            if (_isMouseCaptured)
            {
                return;
            }

            _isMouseCaptured = Target.CaptureMouse();
        }

        private void ReleaseMouseCapture()
        {
            Target.ReleaseMouseCapture();
            _isMouseCaptured = false;
        }
    }
}
