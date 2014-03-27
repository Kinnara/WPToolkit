using Microsoft.Phone.Shell;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Navigation;

namespace Microsoft.Phone.Controls.Primitives
{
    /// <summary>
    /// Represents the base class for flyout controls.
    /// </summary>
    public abstract class FlyoutBase : DependencyObject
    {
        private FlyoutPopup _popup;
        private Control _presenter;

        private ITransition _inTransition;
        private ITransition _outTransition;

        /// <summary>
        /// Provides base class initialization behavior for FlyoutBase derived classes.
        /// </summary>
        protected FlyoutBase()
        {
            Transition = new FlyoutTransition
            {
                In = new SwivelTransition {  Mode = SwivelTransitionMode.In },
                Out = new SwivelTransition { Mode = SwivelTransitionMode.Out },
            };
        }

        #region public FlyoutTransition Transition

        public FlyoutTransition Transition
        {
            get { return (FlyoutTransition)GetValue(TransitionProperty); }
            set { SetValue(TransitionProperty, value); }
        }

        public static readonly DependencyProperty TransitionProperty = DependencyProperty.Register(
            "Transition",
            typeof(FlyoutTransition),
            typeof(FlyoutBase),
            null);

        #endregion

        #region AttachedFlyout

        /// <summary>
        /// Gets the flyout associated with the specified element.
        /// </summary>
        /// <param name="element">The element for which to get the associated flyout.</param>
        /// <returns>The flyout attached to the specified element.</returns>
        public static FlyoutBase GetAttachedFlyout(FrameworkElement element)
        {
            return (FlyoutBase)element.GetValue(AttachedFlyoutProperty);
        }

        /// <summary>
        /// Associates the specified flyout with the specified FrameworkElement.
        /// </summary>
        /// <param name="element">The element to associate the flyout with.</param>
        /// <param name="value">The flyout to associate with the specified element.</param>
        public static void SetAttachedFlyout(FrameworkElement element, FlyoutBase value)
        {
            element.SetValue(AttachedFlyoutProperty, value);
        }

        /// <summary>
        /// Shows the flyout associated with the specified element, if any.
        /// </summary>
        /// <param name="flyoutOwner">The element for which to show the associated flyout.</param>
        public static void ShowAttachedFlyout(FrameworkElement flyoutOwner)
        {
            FlyoutBase flyout = GetAttachedFlyout(flyoutOwner);
            if (flyout != null)
            {
                flyout.Show();
            }
        }

        /// <summary>
        /// Identifies the FlyoutBase.AttachedFlyout XAML attached property.
        /// </summary>
        public static readonly DependencyProperty AttachedFlyoutProperty = DependencyProperty.RegisterAttached(
            "AttachedFlyout",
            typeof(FlyoutBase),
            typeof(FlyoutBase),
            null);

        #endregion

        /// <summary>
        /// Occurs before the flyout is shown.
        /// </summary>
        public event EventHandler Opening;

        /// <summary>
        /// Occurs when the flyout is shown.
        /// </summary>
        public event EventHandler Opened;

        /// <summary>
        /// Occurs before the flyout is hidden.
        /// </summary>
        internal event EventHandler Closing;

        /// <summary>
        /// Occurs when the flyout is hidden.
        /// </summary>
        public event EventHandler Closed;

        /// <summary>
        /// Shows the flyout.
        /// </summary>
        public void Show()
        {
            if (_popup != null)
            {
                return;
            }

            var popup = new FlyoutPopup
            {
                Child = _presenter ?? (_presenter = CreatePresenter()),
                ApplicationBar = CreateApplicationBar()
            };
            popup.PopupOpened += OnPopupOpened;
            popup.PopupCancelled += OnPopupCancelled;

            _popup = popup;

            _presenter.Opacity = 0;

            OnOpening();
            _popup.Show();
            OnOpened();
        }

        /// <summary>
        /// Closes the flyout.
        /// </summary>
        public void Hide()
        {
            RequestHide();
        }

        /// <summary>
        /// When overridden in a derived class, initializes a control to show the flyout content as appropriate for the derived control.
        /// </summary>
        /// <returns>The control that displays the content of the flyout.</returns>
        protected abstract Control CreatePresenter();

        internal virtual ApplicationBar CreateApplicationBar()
        {
            return null;
        }

        internal virtual void OnOpening()
        {
            SafeRaise.Raise(Opening, this);
        }

        internal virtual void OnOpened()
        {
            SafeRaise.Raise(Opened, this);
        }

        internal virtual void OnClosing()
        {
            SafeRaise.Raise(Closing, this);
        }

        internal virtual void OnClosed()
        {
            SafeRaise.Raise(Closed, this);
        }

        internal virtual void RequestHide()
        {
            InternalHide(true);
        }

        internal void InternalHide(bool useTransitions)
        {
            if (_popup == null)
            {
                return;
            }

            OnClosing();

            CompleteInTransition();

            if (useTransitions)
            {
                if (_outTransition == null)
                {
                    FlyoutTransition flyoutTransition = Transition;
                    if (flyoutTransition != null)
                    {
                        TransitionElement outTransitionElement = flyoutTransition.Out;
                        if (outTransitionElement != null)
                        {
                            _outTransition = outTransitionElement.GetTransition(_presenter);
                            _outTransition.Completed += OnOutTransitionCompleted;
                            _outTransition.Begin();
                        }
                    }
                }

                if (_outTransition != null)
                {
                    return;
                }
            }
            else
            {
                CompleteOutTransition();
            }

            if (_popup == null)
            {
                return;
            }

            _popup.Hide();
            _popup.Child = null;
            _popup = null;

            OnClosed();
        }

        private void OnPopupOpened(object sender, EventArgs e)
        {
            CompleteInTransition();
            CompleteOutTransition();

            FlyoutTransition flyoutTransition = Transition;
            ITransition inTransition = null;
            if (flyoutTransition != null)
            {
                TransitionElement inTransitionElement = flyoutTransition.In;
                if (inTransitionElement != null)
                {
                    _inTransition = inTransitionElement.GetTransition(_presenter);
                    _inTransition.Completed += OnInTransitionCompleted;
                    inTransition = _inTransition;
                }
            }

            if (_inTransition != null)
            {
                AnimationHelper.InvokeOnSecondRendering(() =>
                {
                    _presenter.Opacity = 1;

                    if (_inTransition == inTransition && _inTransition.GetCurrentState() != ClockState.Active)
                    {
                        _inTransition.Begin();
                        _presenter.IsHitTestVisible = true;
                    }
                });
            }
            else
            {
                _presenter.Opacity = 1;
            }
        }

        private void OnPopupCancelled(object sender, FlyoutPopupCancelledEventArgs e)
        {
            InternalHide(e.IsDeferrable);
        }

        private void CompleteInTransition()
        {
            if (_inTransition != null)
            {
                _inTransition.SkipToFill();
                _inTransition = null;
            }
        }

        private void CompleteOutTransition()
        {
            if (_outTransition != null)
            {
                _outTransition.SkipToFill();
                _outTransition = null;
            }
        }

        private void OnInTransitionCompleted(object sender, EventArgs e)
        {
            ((ITransition)sender).Stop();
        }

        private void OnOutTransitionCompleted(object sender, EventArgs e)
        {
            ((ITransition)sender).Stop();

            InternalHide(false);
        }

        private class FlyoutPopup
        {
            private static WeakReference _currentInstance;

            private Color _systemTrayColor;
            private double _systemTrayOpacity;
            private IApplicationBar _applicationBar;
            private bool _wasApplicationBarVisible;
            private Popup _hostPopup;
            internal PhoneApplicationPage _hostPage;
            private bool _isCancelled;

            public Control Child
            {
                get { return _hostPopup.Child as Control; }
                set { _hostPopup.Child = value; }
            }

            public IApplicationBar ApplicationBar { get; set; }

            public event EventHandler PopupOpened;

            public event EventHandler<FlyoutPopupCancelledEventArgs> PopupCancelled;

            public FlyoutPopup()
            {
                _hostPopup = new Popup();
                _hostPopup.Opened += OnPopupOpened;
            }

            public void Show()
            {
                if (_currentInstance != null)
                {
                    FlyoutPopup target = _currentInstance.Target as FlyoutPopup;
                    if (target != null)
                    {
                        target.CancelPopup(false);
                    }
                }

                PrepareAppForFullScreen();
                SetSizeAndOffset();
                _hostPopup.IsOpen = true;

                _currentInstance = new WeakReference(this);
            }

            public void Hide()
            {
                _hostPopup.IsOpen = false;
                RestoreSettings();
            }

            private void PrepareAppForFullScreen()
            {
                if (SystemTray.IsVisible)
                {
                    _systemTrayColor = SystemTray.BackgroundColor;
                    _systemTrayOpacity = SystemTray.Opacity;

                    Color backgroundColor;
                    SolidColorBrush childBackground = Child.Background as SolidColorBrush;
                    if (childBackground != null)
                    {
                        backgroundColor = childBackground.Color;
                    }
                    else
                    {
                        backgroundColor = ((SolidColorBrush)Application.Current.Resources["PhoneChromeBrush"]).Color;
                    }
                    SystemTray.BackgroundColor = backgroundColor;

                    if (SystemTray.Opacity < 1)
                    {
                        SystemTray.Opacity = 0;
                    }
                }

                PhoneApplicationFrame frame = Application.Current.RootVisual as PhoneApplicationFrame;
                _hostPage = null;

                if (frame != null)
                {
                    _hostPage = frame.Content as PhoneApplicationPage;
                }

                if (_hostPage != null)
                {
                    if (_hostPage.ApplicationBar != null)
                    {
                        _applicationBar = _hostPage.ApplicationBar;

                        if (_hostPage.ApplicationBar.IsVisible)
                        {
                            _wasApplicationBarVisible = true;
                            _hostPage.ApplicationBar.IsVisible = false;
                        }
                    }

                    if (ApplicationBar != null)
                    {
                        if (_applicationBar == null || _applicationBar.Opacity < 1)
                        {
                            ApplicationBar.Opacity = 0.999;
                        }

                        _hostPage.ApplicationBar = ApplicationBar;
                    }
                }

                if (frame != null)
                {
                    frame.BackKeyPress += OnBackKeyPress;
                    frame.Navigating += OnFrameNavigating;
                    frame.OrientationChanged += OnOrientationChanged;
                }
            }

            private void RestoreSettings()
            {
                if (SystemTray.IsVisible)
                {
                    SystemTray.BackgroundColor = _systemTrayColor;
                    SystemTray.Opacity = _systemTrayOpacity;
                }

                PhoneApplicationFrame frame = Application.Current.RootVisual as PhoneApplicationFrame;

                if (_hostPage != null && _hostPage.ApplicationBar != _applicationBar)
                {
                    _hostPage.ApplicationBar = _applicationBar;
                }

                if (_wasApplicationBarVisible && _hostPage != null && _hostPage.ApplicationBar != null)
                {
                    _hostPage.ApplicationBar.IsVisible = true;
                }

                if (frame != null)
                {
                    frame.BackKeyPress -= OnBackKeyPress;
                    frame.Navigating -= OnFrameNavigating;
                    frame.OrientationChanged -= OnOrientationChanged;
                }

                _currentInstance = null;
            }

            private void OnPopupOpened(object sender, EventArgs e)
            {
                SafeRaise.Raise(PopupOpened, this);
            }

            private void OnBackKeyPress(object sender, CancelEventArgs args)
            {
                CancelPopup(true);
                args.Cancel = true;
            }

            private void OnFrameNavigating(object sender, NavigatingCancelEventArgs args)
            {
                CancelPopup(false);
            }

            private void OnOrientationChanged(object sender, OrientationChangedEventArgs args)
            {
                SetSizeAndOffset();
            }

            private void CancelPopup(bool isDeferrable)
            {
                if (_isCancelled)
                {
                    return;
                }

                if (!isDeferrable)
                {
                    _isCancelled = true;
                }

                var handler = PopupCancelled;
                if (handler != null)
                {
                    handler(this, new FlyoutPopupCancelledEventArgs(isDeferrable));
                }
            }

            private void SetSizeAndOffset()
            {
                PhoneApplicationFrame frame = Application.Current.RootVisual as PhoneApplicationFrame;
                if (frame != null)
                {
                    bool reserveSystemTraySpace = true;
                    bool reserveApplicationBarSpace = _hostPage != null && _hostPage.ApplicationBar != null && _hostPage.ApplicationBar.IsVisible;

                    CompositeTransform transform = null;
                    Thickness padding = new Thickness();
                    double frameWidth = Application.Current.Host.Content.ActualWidth;
                    double frameHeight = Application.Current.Host.Content.ActualHeight;
                    double width = frameWidth;
                    double height = frameHeight;

                    switch (frame.Orientation)
                    {
                        case PageOrientation.Landscape:
                        case PageOrientation.LandscapeLeft:
                            transform = new CompositeTransform { Rotation = 90, TranslateX = frameWidth };
                            width = frameHeight;
                            height = frameWidth;

                            if (reserveSystemTraySpace)
                            {
                                padding.Left = PhoneHelper.SystemTrayLandscapeWidth;
                            }

                            if (reserveApplicationBarSpace)
                            {
                                padding.Right = PhoneHelper.ApplicationBarLandscapeWidth;
                            }

                            break;
                        case PageOrientation.LandscapeRight:
                            transform = new CompositeTransform { Rotation = -90, TranslateY = frameHeight };
                            width = frameHeight;
                            height = frameWidth;

                            if (reserveSystemTraySpace)
                            {
                                padding.Right = PhoneHelper.SystemTrayLandscapeWidth;
                            }

                            if (reserveApplicationBarSpace)
                            {
                                padding.Left = PhoneHelper.ApplicationBarLandscapeWidth;
                            }

                            break;
                        default:
                            if (reserveSystemTraySpace)
                            {
                                padding.Top = PhoneHelper.SystemTrayPortraitHeight;
                            }

                            if (reserveApplicationBarSpace)
                            {
                                padding.Bottom = 1;
                                height -= GetApplicationBarSize(_hostPage.ApplicationBar) - 1;
                            }

                            break;
                    }

                    Child.RenderTransform = transform;
                    Child.Padding = padding;
                    Child.Width = width;
                    Child.Height = height;
                }
            }

            private static double GetApplicationBarSize(IApplicationBar applicationBar)
            {
                return applicationBar.Mode == ApplicationBarMode.Minimized ? applicationBar.MiniSize : applicationBar.DefaultSize;
            }
        }

        private class FlyoutPopupCancelledEventArgs : EventArgs
        {
            public FlyoutPopupCancelledEventArgs(bool isDeferrable)
            {
                IsDeferrable = isDeferrable;
            }

            public bool IsDeferrable { get; private set; }
        }
    }
}
