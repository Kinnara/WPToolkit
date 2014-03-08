using Microsoft.Phone.Shell;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
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

        /// <summary>
        /// Provides base class initialization behavior for FlyoutBase derived classes.
        /// </summary>
        protected FlyoutBase()
        {
        }

        /// <summary>
        /// Occurs before the flyout is shown.
        /// </summary>
        public event EventHandler Opening;

        /// <summary>
        /// Occurs when the flyout is shown.
        /// </summary>
        public event EventHandler Opened;

        ///// <summary>
        ///// Occurs before the flyout is hidden.
        ///// </summary>
        internal event EventHandler<FlyoutClosingEventArgs> Closing;

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
            popup.PopupCancelled += OnPopupCancelled;

            _popup = popup;

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

        internal virtual void OnClosing(FlyoutClosingEventArgs e)
        {
            SafeRaise.Raise(Closing, this, e);
        }

        internal virtual void OnClosed()
        {
            SafeRaise.Raise(Closed, this);
        }

        internal virtual void RequestHide()
        {
            InternalHide(true);
        }

        internal void InternalHide(bool isCancelable)
        {
            var e = new FlyoutClosingEventArgs(isCancelable);

            OnClosing(e);

            if (e.Cancel)
            {
                return;
            }

            if (_popup != null)
            {
                _popup.Hide();
                _popup.Child = null;
                _popup = null;
            }

            OnClosed();
        }

        private void OnPopupCancelled(object sender, FlyoutPopupCancelledEventArgs e)
        {
            InternalHide(e.IsDeferrable);
        }

        private class FlyoutPopup
        {
            private Color _systemTrayColor;
            private double _systemTrayOpacity;
            private IApplicationBar _applicationBar;
            private bool _wasApplicationBarVisible;
            private Popup _hostPopup;
            internal PhoneApplicationPage _hostPage;
            private bool _isCancelled;

            public FrameworkElement Child
            {
                get { return _hostPopup.Child as FrameworkElement; }
                set { _hostPopup.Child = value; }
            }

            public IApplicationBar ApplicationBar { get; set; }

            public event EventHandler<FlyoutPopupCancelledEventArgs> PopupCancelled;

            public FlyoutPopup()
            {
                _hostPopup = new Popup();
            }

            public void Show()
            {
                PrepareAppForFullScreen();
                SetSizeAndOffset();
                _hostPopup.IsOpen = true;
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

                    SystemTray.BackgroundColor = ((SolidColorBrush)Application.Current.Resources["PhoneChromeBrush"]).Color;

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

                if (_applicationBar != null && _hostPage != null && _hostPage.ApplicationBar != _applicationBar)
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

                _isCancelled = true;

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
                    bool isApplicationBarVisible = _hostPage != null && _hostPage.ApplicationBar != null && _hostPage.ApplicationBar.IsVisible;

                    PageOrientation orientation = frame.Orientation;
                    CompositeTransform transform = null;
                    double frameWidth = Application.Current.Host.Content.ActualWidth;
                    double frameHeight = Application.Current.Host.Content.ActualHeight;
                    double width = frameWidth;
                    double height = frameHeight;
                    switch (orientation)
                    {
                        case PageOrientation.Landscape:
                        case PageOrientation.LandscapeLeft:
                            transform = new CompositeTransform { Rotation = 90, TranslateX = frameWidth };
                            width = frameHeight;
                            height = frameWidth;

                            if (SystemTray.IsVisible)
                            {
                                transform.TranslateY += PhoneHelper.SystemTrayLandscapeWidth;
                                width -= PhoneHelper.SystemTrayLandscapeWidth;
                            }

                            if (isApplicationBarVisible)
                            {
                                width -= PhoneHelper.ApplicationBarLandscapeWidth;
                            }

                            break;
                        case PageOrientation.LandscapeRight:
                            transform = new CompositeTransform { Rotation = -90, TranslateY = frameHeight };
                            width = frameHeight;
                            height = frameWidth;

                            if (SystemTray.IsVisible)
                            {
                                width -= PhoneHelper.SystemTrayLandscapeWidth;
                            }

                            if (isApplicationBarVisible)
                            {
                                transform.TranslateY -= PhoneHelper.SystemTrayLandscapeWidth;
                                width -= PhoneHelper.ApplicationBarLandscapeWidth;
                            }

                            break;
                        default:
                            if (SystemTray.IsVisible)
                            {
                                transform = new CompositeTransform { TranslateY = PhoneHelper.SystemTrayPortraitHeight };
                                height -= PhoneHelper.SystemTrayPortraitHeight;
                            }

                            if (isApplicationBarVisible)
                            {
                                height -= _hostPage.ApplicationBar.DefaultSize;
                            }

                            break;
                    }

                    Child.RenderTransform = transform;
                    Child.Width = width;
                    Child.Height = height;
                }
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

    internal sealed class FlyoutClosingEventArgs : EventArgs
    {
        private bool _cancel;

        public FlyoutClosingEventArgs(bool isCancelable)
        {
            IsCancelable = isCancelable;
        }

        public bool IsCancelable { get; private set; }

        public bool Cancel
        {
            get { return _cancel; }
            set
            {
                if (IsCancelable)
                {
                    _cancel = value;
                }
                else if (value)
                {
                    throw new InvalidOperationException();
                }
            }
        }
    }
}
