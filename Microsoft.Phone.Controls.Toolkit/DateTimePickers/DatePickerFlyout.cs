using Microsoft.Phone.Controls.Primitives;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Windows.Foundation;

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

        #region public bool DayVisible

        /// <summary>
        /// Gets or sets a value that indicates whether the day selector is shown.
        /// </summary>
        /// 
        /// <returns>
        /// True if the day selector is shown; otherwise, false. The default is true.
        /// </returns>
        public bool DayVisible
        {
            get { return (bool)GetValue(DayVisibleProperty); }
            set { SetValue(DayVisibleProperty, value); }
        }

        /// <summary>
        /// Gets the identifier for the DayVisible dependency property.
        /// </summary>
        /// 
        /// <returns>
        /// The identifier for the DayVisible dependency property.
        /// </returns>
        public static readonly DependencyProperty DayVisibleProperty = DependencyProperty.Register(
            "DayVisible",
            typeof(bool),
            typeof(DatePickerFlyout),
            new PropertyMetadata(true));

        #endregion

        #region public bool MonthVisible

        /// <summary>
        /// Gets or sets a value that indicates whether the month selector is shown.
        /// </summary>
        /// 
        /// <returns>
        /// True if the month selector is shown; otherwise, false. The default is true.
        /// </returns>
        public bool MonthVisible
        {
            get { return (bool)GetValue(MonthVisibleProperty); }
            set { SetValue(MonthVisibleProperty, value); }
        }

        /// <summary>
        /// Gets the identifier for the MonthVisible dependency property.
        /// </summary>
        /// 
        /// <returns>
        /// The identifier for the MonthVisible dependency property.
        /// </returns>
        public static readonly DependencyProperty MonthVisibleProperty = DependencyProperty.Register(
            "MonthVisible",
            typeof(bool),
            typeof(DatePickerFlyout),
            new PropertyMetadata(true));

        #endregion

        #region public bool YearVisible

        /// <summary>
        /// Gets or sets a value that indicates whether the year selector is shown.
        /// </summary>
        /// 
        /// <returns>
        /// True if the year selector is shown; otherwise, false. The default is true.
        /// </returns>
        public bool YearVisible
        {
            get { return (bool)GetValue(YearVisibleProperty); }
            set { SetValue(YearVisibleProperty, value); }
        }

        /// <summary>
        /// Gets the identifier for the YearVisible dependency property.
        /// </summary>
        /// 
        /// <returns>
        /// The identifier for the YearVisible dependency property.
        /// </returns>
        public static readonly DependencyProperty YearVisibleProperty = DependencyProperty.Register(
            "YearVisible",
            typeof(bool),
            typeof(DatePickerFlyout),
            new PropertyMetadata(true));

        #endregion

        /// <summary>
        /// Occurs when a date has been picked by the user.
        /// </summary>
        public event TypedEventHandler<DatePickerFlyout, DatePickedEventArgs> DatePicked;

        /// <summary>
        /// Begins an asynchronous operation to show the flyout placed in relation to the specified element.
        /// </summary>
        /// <returns>
        /// An asynchronous operation.
        /// </returns>
        /// <param name="target">The element to use as the flyout's placement target.</param>
        public Task<DateTimeOffset?> ShowAtAsync(FrameworkElement target)
        {
            return _helper.ShowAsync();
        }

        /// <summary>
        /// Gets or sets whether confirmation buttons should be shown in the picker.
        /// </summary>
        /// <returns>
        /// True if confirmation buttons should be shown in the picker; Otherwise, false.
        /// </returns>
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

        /// <summary>
        /// Notifies PickerFlyoutBase subclasses when a user has confirmed a selection.
        /// </summary>
        protected override void OnConfirmed()
        {
            _presenter.Commit();

            RaiseDatePicked();

            base.OnConfirmed();
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
                handler(this, new DatePickedEventArgs(oldDate, newDate));
            }
        }
    }
}
