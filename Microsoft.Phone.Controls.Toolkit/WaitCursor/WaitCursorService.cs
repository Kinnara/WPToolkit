using Microsoft.Phone.Controls.Primitives;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Media;
using System.Windows.Navigation;

namespace Microsoft.Phone.Controls
{
    /// <summary>
    /// Provides the system implementation for displaying a WaitCursor.
    /// </summary>
    public static class WaitCursorService
    {
        private const byte BackgroundAlpha = 204;

        private static WaitCursorControl _control = new WaitCursorControl();
        private static PhoneApplicationFrame _frame;

        /// <summary>
        /// Gets the value of the <see cref="P:Microsoft.Phone.Controls.WaitCursor"/> attached property for a specified phone application page.
        /// </summary>
        /// 
        /// <returns>
        /// The wait cursor on the current application page.
        /// </returns>
        /// <param name="element">The page for which to get the <see cref="P:Microsoft.Phone.Controls.WaitCursor"/> attached property.</param>
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
        public static WaitCursor GetWaitCursor(PhoneApplicationPage element)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }

            return (WaitCursor)element.GetValue(WaitCursorProperty);
        }

        /// <summary>
        /// Sets the value of the <see cref="P:Microsoft.Phone.Controls.WaitCursor"/> attached property for a specified phone application page.
        /// </summary>
        /// <param name="element">The page for which to set the <see cref="P:Microsoft.Phone.Controls.WaitCursor"/> attached property.</param>
        /// <param name="waitCursor">The wait cursor to set.</param>
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
        public static void SetWaitCursor(PhoneApplicationPage element, WaitCursor waitCursor)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }

            element.SetValue(WaitCursorProperty, waitCursor);
        }

        /// <summary>
        /// The dependency property for <see cref="P:Microsoft.Phone.Controls.WaitCursor"/>.
        /// </summary>
        public static readonly DependencyProperty WaitCursorProperty = DependencyProperty.RegisterAttached(
            "WaitCursor",
            typeof(WaitCursor),
            typeof(WaitCursorService),
            new PropertyMetadata(OnWaitCursorChanged));

        /// <summary>
        /// Gets or sets the wait cursor on the current application page.
        /// </summary>
        /// 
        /// <returns>
        /// The wait cursor on the current application page.
        /// </returns>
        /// <exception cref="T:System.InvalidOperationException">There is no active <see cref="T:Microsoft.Phone.Controls.PhoneApplicationPage"/> object.</exception>
        [SuppressMessage("Microsoft.Naming", "CA1721:PropertyNamesShouldNotMatchGetMethods")]
        public static WaitCursor WaitCursor
        {
            get
            {
                PhoneApplicationPage activePage = GetActivePage();
                if (activePage == null) throw new InvalidOperationException();
                return GetWaitCursor(activePage);
            }
            set
            {
                PhoneApplicationPage activePage = GetActivePage();
                if (activePage == null) throw new InvalidOperationException();
                SetWaitCursor(activePage, value);
            }
        }

        internal static PhoneApplicationFrame Frame
        {
            get
            {
                Initialize();
                return _frame;
            }
        }

        internal static PhoneApplicationPage GetActivePage()
        {
            PhoneApplicationFrame frame = Frame;
            if (frame != null)
            {
                return frame.Content as PhoneApplicationPage;
            }
            return null;
        }

        internal static WaitCursor GetActiveWaitCursor()
        {
            PhoneApplicationPage activePage = GetActivePage();
            if (activePage != null)
            {
                return GetWaitCursor(activePage);
            }
            return null;
        }

        internal static void UpdateControl()
        {
            _control.Language = WaitCursorService.Frame.Language;
            _control.ImplementationRootFlowDirection = WaitCursorService.Frame.FlowDirection;

            bool isVisible = false;
            string text = string.Empty;
            Brush background = null;
            Brush foreground = null;

            WaitCursor waitCursor = GetActiveWaitCursor();
            if (waitCursor != null)
            {
                isVisible = waitCursor.IsVisible;
                text = waitCursor.Text;
                background = waitCursor.Background;
                foreground = waitCursor.Foreground;

                waitCursor.UpdateOwner();
            }

            if (background == null)
            {
                Color backgroundColor = ((SolidColorBrush)Application.Current.Resources["PhoneBackgroundBrush"]).Color;
                backgroundColor.A = BackgroundAlpha;
                background = new SolidColorBrush(backgroundColor);
            }

            _control.IsVisible = isVisible;
            _control.Background = background;
            _control.Foreground = foreground ?? (SolidColorBrush)Application.Current.Resources["PhoneForegroundBrush"];
            if (_control.IsVisible)
            {
                _control.Text = text;
            }
        }

        private static void Initialize()
        {
            if (_frame != null)
            {
                return;
            }

            if (PhoneHelper.TryGetPhoneApplicationFrame(out _frame))
            {
                Frame.Navigated += OnFrameNavigated;
                Frame.OrientationChanged += OnFrameOrientationChanged;

                UpdateControl();
            }
            else
            {
                Deployment.Current.Dispatcher.BeginInvoke(Initialize);
            }
        }

        private static void OnWaitCursorChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            PhoneApplicationPage element = o as PhoneApplicationPage;
            if (element != null)
            {
                WaitCursor oldWaitCursor = e.OldValue as WaitCursor;
                if (oldWaitCursor != null)
                {
                    oldWaitCursor.Owner = null;
                }
                WaitCursor newWaitCursor = e.NewValue as WaitCursor;
                if (newWaitCursor != null)
                {
                    newWaitCursor.Owner = element;
                }
            }
        }

        private static void OnFrameNavigated(object sender, NavigationEventArgs e)
        {
            UpdateControl();
        }

        private static void OnFrameOrientationChanged(object sender, OrientationChangedEventArgs e)
        {
            UpdateControl();
        }
    }
}
