using Microsoft.Phone.Controls.Primitives;
using System;
using System.Threading.Tasks;

namespace Microsoft.Phone.Controls
{
    internal class PickerFlyoutHelper<T>
    {
        private PickerFlyoutBase _flyout;
        private bool _isFlyoutOpen;

        private TaskCompletionSource<T> _tcs;

        public PickerFlyoutHelper(PickerFlyoutBase flyout)
        {
            _flyout = flyout;
            _flyout.Opened += OnFlyoutOpened;
            _flyout.Closed += OnFlyoutClosed;
        }

        public bool IsAsyncOperationInProgress
        {
            get { return _tcs != null; }
        }

        public Task<T> ShowAsync()
        {
            if (_tcs != null)
            {
                throw new InvalidOperationException();
            }

            if (_isFlyoutOpen)
            {
                EventHandler<object> onClosed = null;
                onClosed = delegate
                {
                    _flyout.Closed -= onClosed;
                    _flyout.Show();
                };
                _flyout.Closed += onClosed;
            }
            else
            {
                _flyout.Show();
            }

            _tcs = new TaskCompletionSource<T>();
            return _tcs.Task;
        }

        public void CompleteShowAsync(T result)
        {
            if (_tcs != null)
            {
                _tcs.TrySetResult(result);
                _tcs = null;
            }
        }

        private void OnFlyoutOpened(object sender, object e)
        {
            _isFlyoutOpen = true;
        }

        private void OnFlyoutClosed(object sender, object e)
        {
            _isFlyoutOpen = false;
        }
    }
}
