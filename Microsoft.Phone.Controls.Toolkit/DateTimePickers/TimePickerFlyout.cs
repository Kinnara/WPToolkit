using Microsoft.Phone.Controls.Primitives;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Microsoft.Phone.Controls
{
    /// <summary>
    /// Represents a control that allows a user to pick a time value.
    /// </summary>
    public sealed class TimePickerFlyout : PickerFlyoutBase
    {
        private TimePickerFlyoutPresenter _presenter;
        private PickerFlyoutHelper<TimeSpan?> _helper;

        /// <summary>
        /// Initializes a new instance of the TimePickerFlyout class.
        /// </summary>
        public TimePickerFlyout()
        {
            _presenter = new TimePickerFlyoutPresenter(this);
            _helper = new PickerFlyoutHelper<TimeSpan?>(this);

            Time = DateTime.Now.TimeOfDay;
            SetTitle(this, DateTimePickerResources.TimePickerTitle);
        }

        #region public TimeSpan Time

        /// <summary>
        /// Gets or sets the time currently set in the time picker.
        /// </summary>
        /// 
        /// <returns>
        /// The time currently set in the time picker.
        /// </returns>
        public TimeSpan Time
        {
            get { return (TimeSpan)GetValue(TimeProperty); }
            set { SetValue(TimeProperty, value); }
        }

        /// <summary>
        /// Gets the identifier for the Time dependency property.
        /// </summary>
        /// 
        /// <returns>
        /// The identifier for the Time dependency property.
        /// </returns>
        public static readonly DependencyProperty TimeProperty = DependencyProperty.Register(
            "Time",
            typeof(TimeSpan),
            typeof(TimePickerFlyout),
            new PropertyMetadata((d, e) => ((TimePickerFlyout)d).OnTimeChanged(e)));

        private void OnTimeChanged(DependencyPropertyChangedEventArgs e)
        {
            _presenter.Time = Time;
        }

        #endregion

        public event EventHandler<TimePickerValueChangedEventArgs> TimePicked;

        public Task<TimeSpan?> ShowAsync()
        {
            return _helper.ShowAsync();
        }

        protected override bool ShouldShowConfirmationButtons()
        {
            return true;
        }

        /// <summary>
        /// Initializes a control to show the flyout content.
        /// </summary>
        /// <returns>
        /// The control that displays the content of the flyout.
        /// </returns>
        protected override Control CreatePresenter()
        {
            return _presenter;
        }

        protected override void OnConfirmed()
        {
            _presenter.Commit();

            RaiseTimePicked();
        }

        internal override void OnOpening()
        {
            base.OnOpening();

            _presenter.Time = Time;
        }

        internal override void OnClosed()
        {
            _helper.CompleteShowAsync(null);

            base.OnClosed();
        }

        internal override void RequestHide()
        {
            if (_helper.IsAsyncOperationInProgress)
            {
                return;
            }

            base.RequestHide();
        }

        private void RaiseTimePicked()
        {
            TimeSpan oldTime = Time;
            TimeSpan newTime = _presenter.Time;

            Time = newTime;

            _helper.CompleteShowAsync(Time);

            var handler = TimePicked;
            if (handler != null)
            {
                handler(this, new TimePickerValueChangedEventArgs(oldTime, newTime));
            }
        }
    }
}
