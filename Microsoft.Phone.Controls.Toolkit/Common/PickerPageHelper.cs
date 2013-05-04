using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace Microsoft.Phone.Controls
{
    internal class PickerPageHelper<T> where T : class
    {
        private PhoneApplicationFrame _frame;

        private object _frameContentWhenOpened;
        private NavigationInTransition _savedNavigationInTransition;
        private NavigationOutTransition _savedNavigationOutTransition;
        private T _pickerPage;

        private Control _picker;
        private Action<T> _onOpening;
        private Action _close;
        private Action<T> _onClosed;

        /// <summary>
        /// Whether this picker has the picker page opened.
        /// </summary>
        private bool _hasPickerPageOpen;

        public PickerPageHelper(Control picker, Action<T> onOpening, Action close, Action<T> onClosed)
        {
            if (picker == null)
            {
                throw new ArgumentNullException("picker");
            }

            _picker = picker;
            _onOpening = onOpening;
            _close = close;
            _onClosed = onClosed;

            _picker.Unloaded += OnPickerUnloaded;
        }

        public void OpenPickerPage(Uri pickerPageUri)
        {
            if (null == pickerPageUri)
            {
                throw new ArgumentException("pickerPageUri");
            }

            if (null == _frame)
            {
                // Hook up to necessary events and navigate
                _frame = Application.Current.RootVisual as PhoneApplicationFrame;
                if (null != _frame)
                {
                    _frameContentWhenOpened = _frame.Content;

                    // Save and clear host page transitions for the upcoming "popup" navigation
                    UIElement frameContentWhenOpenedAsUIElement = _frameContentWhenOpened as UIElement;

                    if (null != frameContentWhenOpenedAsUIElement)
                    {
                        _savedNavigationInTransition = TransitionService.GetNavigationInTransition(frameContentWhenOpenedAsUIElement);
                        TransitionService.SetNavigationInTransition(frameContentWhenOpenedAsUIElement, null);
                        _savedNavigationOutTransition = TransitionService.GetNavigationOutTransition(frameContentWhenOpenedAsUIElement);
                        TransitionService.SetNavigationOutTransition(frameContentWhenOpenedAsUIElement, null);

                    }

                    _frame.Navigated += OnFrameNavigated;
                    _frame.NavigationStopped += OnFrameNavigationStoppedOrFailed;
                    _frame.NavigationFailed += OnFrameNavigationStoppedOrFailed;

                    _hasPickerPageOpen = true;

                    _frame.Navigate(pickerPageUri);
                }
            }
        }

        public void ClosePickerPage()
        {
            if (null == _frame)
            {
                // Unhook from events
                _frame = Application.Current.RootVisual as PhoneApplicationFrame;
                if (null != _frame)
                {
                    _frame.Navigated -= OnFrameNavigated;
                    _frame.NavigationStopped -= OnFrameNavigationStoppedOrFailed;
                    _frame.NavigationFailed -= OnFrameNavigationStoppedOrFailed;

                    // Restore host page transitions for the completed "popup" navigation
                    UIElement frameContentWhenOpenedAsUIElement = _frameContentWhenOpened as UIElement;

                    if (null != frameContentWhenOpenedAsUIElement)
                    {
                        TransitionService.SetNavigationInTransition(frameContentWhenOpenedAsUIElement, _savedNavigationInTransition);
                        _savedNavigationInTransition = null;
                        TransitionService.SetNavigationOutTransition(frameContentWhenOpenedAsUIElement, _savedNavigationOutTransition);
                        _savedNavigationOutTransition = null;
                    }

                    _frame = null;
                    _frameContentWhenOpened = null;
                }
            }

            // Commit the value if available
            if (null != _pickerPage)
            {
                if (_onClosed != null)
                {
                    _onClosed(_pickerPage);
                }
                _pickerPage = null;
            }
        }

        private void OnFrameNavigated(object sender, NavigationEventArgs e)
        {
            if (e.Content == _frameContentWhenOpened)
            {
                // Navigation to original page; close the picker page
                if (_close != null)
                {
                    _close();
                }
            }
            else if (null == _pickerPage && _hasPickerPageOpen)
            {
                _hasPickerPageOpen = false;
                _pickerPage = e.Content as T;
                if (null != _pickerPage)
                {
                    if (_onOpening != null)
                    {
                        _onOpening(_pickerPage);
                    }
                }
            }
        }

        private void OnFrameNavigationStoppedOrFailed(object sender, EventArgs e)
        {
            // Abort
            if (_close != null)
            {
                _close();
            }
        }

        private void OnPickerUnloaded(object sender, RoutedEventArgs e)
        {
            _frame = null;
        }
    }
}
