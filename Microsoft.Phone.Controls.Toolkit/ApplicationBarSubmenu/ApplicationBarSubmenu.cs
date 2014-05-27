using Microsoft.Phone.Controls.Primitives;
using System;
using System.Windows;

namespace Microsoft.Phone.Controls
{
    /// <summary>
    /// Represents an Application Bar Submenu in Windows Phone applications.
    /// </summary>
    public class ApplicationBarSubmenu : MenuBase
    {
        /// <summary>
        /// Width of the system tray in Landscape Mode
        /// </summary>
        private const double SystemTrayLandscapeWidth = 72;

        private static SwivelTransition ShowTransition = new SwivelTransition { Mode = SwivelTransitionMode.BackwardIn };
        private static SwivelTransition HideTransition = new SwivelTransition { Mode = SwivelTransitionMode.BackwardOut };

        private FullScreenPopup _popup;
        private bool _isHiding;
        private bool _isOpen;
        public bool IsOpen { get { return _isOpen; } }

        /// <summary>
        /// Initializes a new instance of the ApplicationBarSubmenu class.
        /// </summary>
        public ApplicationBarSubmenu()
        {
            DefaultStyleKey = typeof(ApplicationBarSubmenu);
        }

        /// <summary>
        /// Displays the Application Bar Submenu.
        /// </summary>
        public void Show()
        {
            if (ReadLocalValue(FlowDirectionProperty) == DependencyProperty.UnsetValue)
            {
                FlowDirection = this.GetUsefulFlowDirection();
            }

            SetOrientation();

            PhoneApplicationFrame frame = Application.Current.RootVisual as PhoneApplicationFrame;
            if (frame != null)
            {
                frame.OrientationChanged += OnOrientationChanged;
                frame.Focus();
            }

            LayoutUpdated += OnLayoutUpdated;

            _popup = new FullScreenPopup()
            {
                Child = this
            };
            _popup.PopupCancelled += OnPopupCancelled;
            _popup.Show();
            _isOpen = true;
        }

        /// <summary>
        /// Called when a child MenuItem is clicked.
        /// </summary>
        internal void ChildMenuItemClicked()
        {
            if (_popup != null)
            {
                Hide();
            }
        }

        private void Hide()
        {
            if (_isHiding)
            {
                return;
            }

            _isHiding = true;

            ITransition transition = HideTransition.GetTransition(this);
            transition.Completed += delegate
            {
                transition.Stop();

                PhoneApplicationFrame frame = Application.Current.RootVisual as PhoneApplicationFrame;
                if (frame != null)
                {
                    frame.OrientationChanged -= OnOrientationChanged;
                }

                _popup.Hide();
                _popup.Child = null;
                _popup = null;

                _isHiding = false;
                _isOpen = false;
            };
            transition.Begin();
        }

        private void OnLayoutUpdated(object sender, EventArgs e)
        {
            LayoutUpdated -= OnLayoutUpdated;

            ITransition transition = ShowTransition.GetTransition(this);
            transition.Completed += delegate
            {
                transition.Stop();
            };
            transition.Begin();
        }

        private void OnPopupCancelled(object sender, EventArgs e)
        {
            Hide();
        }

        private void OnOrientationChanged(object sender, OrientationChangedEventArgs e)
        {
            SetOrientation();
        }

        private void SetOrientation()
        {
            Thickness margin = new Thickness();

            PhoneApplicationFrame frame = Application.Current.RootVisual as PhoneApplicationFrame;
            if (frame != null)
            {
                PageOrientation orientation = frame.Orientation;
                switch (orientation)
                {
                    case PageOrientation.LandscapeLeft:
                        margin.Left = SystemTrayLandscapeWidth;
                        break;
                    case PageOrientation.LandscapeRight:
                        margin.Right = SystemTrayLandscapeWidth;
                        break;
                }
            }

            Margin = margin;
        }
    }
}
