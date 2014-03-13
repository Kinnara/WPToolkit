using Microsoft.Phone.Controls.Primitives;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Microsoft.Phone.Controls
{
    /// <summary>
    /// Represents a control that allows a user to pick a date.
    /// </summary>
    public sealed class DatePickerFlyout : PickerFlyoutBase
    {
        private DatePickerFlyoutPresenter _presenter;
        private PickerFlyoutHelper<DateTimeOffset?> _helper;

        /// <summary>
        /// Initializes a new instance of the DatePickerFlyout class.
        /// </summary>
        public DatePickerFlyout()
        {
            _presenter = new DatePickerFlyoutPresenter(this);
            _presenter.Date = Date;
            _helper = new PickerFlyoutHelper<DateTimeOffset?>(this);

            Date = DateTimeOffset.Now;
            SetTitle(this, DateTimePickerResources.DatePickerTitle);
        }

        #region public DateTimeOffset Date

        /// <summary>
        /// Gets or sets the date currently set in the date picker.
        /// </summary>
        /// 
        /// <returns>
        /// The date currently set in the date picker.
        /// </returns>
        public DateTimeOffset Date
        {
            get { return (DateTimeOffset)GetValue(DateProperty); }
            set { SetValue(DateProperty, value); }
        }

        /// <summary>
        /// Gets the identifier for the Date dependency property.
        /// </summary>
        /// 
        /// <returns>
        /// The identifier for the Date dependency property.
        /// </returns>
        public static readonly DependencyProperty DateProperty = DependencyProperty.Register(
            "Date",
            typeof(DateTimeOffset),
            typeof(DatePickerFlyout),
            new PropertyMetadata((d, e) => ((DatePickerFlyout)d).OnDateChanged(e)));

        private void OnDateChanged(DependencyPropertyChangedEventArgs e)
        {
            _presenter.Date = Date;
        }

        #endregion

        public event EventHandler<DatePickerValueChangedEventArgs> DatePicked;

        public Task<DateTimeOffset?> ShowAsync()
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

            RaiseDatePicked();
        }

        internal override void OnOpening()
        {
            base.OnOpening();

            _presenter.Date = Date;
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

        private void RaiseDatePicked()
        {
            DateTimeOffset oldDate = Date;
            DateTimeOffset newDate = _presenter.Date;

            Date = newDate;

            _helper.CompleteShowAsync(Date);

            var handler = DatePicked;
            if (handler != null)
            {
                handler(this, new DatePickerValueChangedEventArgs(oldDate, newDate));
            }
        }
    }
}
