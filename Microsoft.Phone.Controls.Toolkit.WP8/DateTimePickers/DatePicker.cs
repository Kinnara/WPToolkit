using Microsoft.Phone.Controls.LocalizedResources;
using Microsoft.Phone.Controls.Primitives;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Windows;

namespace Microsoft.Phone.Controls
{
    /// <summary>
    /// Represents a control that allows a user to pick a date value.
    /// </summary>
    public class DatePicker : DateTimePickerBase
    {
        private string _fallbackValueStringFormat;

        /// <summary>
        /// Initializes a new instance of the DatePicker class.
        /// </summary>
        public DatePicker()
        {
            DefaultStyleKey = typeof(DatePicker);

            Date = GetCurrentDate();
        }

        #region Date

        /// <summary>
        /// Gets or sets the date currently set in the date picker.
        /// </summary>
        /// 
        /// <returns>
        /// The date currently set in the picker.
        /// </returns>
        public DateTimeOffset? Date
        {
            get { return (DateTimeOffset?)GetValue(DateProperty); }
            set { SetValue(DateProperty, value); }
        }

        /// <summary>
        /// Identifies the Date dependency property.
        /// </summary>
        public static readonly DependencyProperty DateProperty = DependencyProperty.Register(
            "Date",
            typeof(DateTimeOffset?),
            typeof(DatePicker),
            new PropertyMetadata(null, (d, e) => ((DatePicker)d).OnDateChanged(e)));

        private void OnDateChanged(DependencyPropertyChangedEventArgs e)
        {
            var oldDate = (DateTimeOffset?)e.OldValue;
            var newDate = (DateTimeOffset?)e.NewValue;

            if (PickerFlyout != null)
            {
                PickerFlyout.Date = Date.GetValueOrDefault(GetCurrentDate());
            }

            InvalidateValue();

            var handler = DateChanged;
            if (handler != null)
            {
                handler(this, new DatePickerValueChangedEventArgs(oldDate, newDate));
            }
        }

        #endregion

        #region DayVisible

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
        /// Identifies the DayVisible dependency property.
        /// </summary>
        public static readonly DependencyProperty DayVisibleProperty = DependencyProperty.Register(
            "DayVisible",
            typeof(bool),
            typeof(DatePicker),
            new PropertyMetadata(true, (d, e) => ((DatePicker)d).OnDayVisibleChanged(e)));

        private void OnDayVisibleChanged(DependencyPropertyChangedEventArgs e)
        {
            if (PickerFlyout != null)
            {
                PickerFlyout.DayVisible = DayVisible;
            }
        }

        #endregion

        #region MonthVisible

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
        /// Identifies the MonthVisible dependency property.
        /// </summary>
        public static readonly DependencyProperty MonthVisibleProperty = DependencyProperty.Register(
            "MonthVisible",
            typeof(bool),
            typeof(DatePicker),
            new PropertyMetadata(true, (d, e) => ((DatePicker)d).OnMonthVisibleChanged(e)));

        private void OnMonthVisibleChanged(DependencyPropertyChangedEventArgs e)
        {
            if (PickerFlyout != null)
            {
                PickerFlyout.MonthVisible = MonthVisible;
            }
        }

        #endregion

        #region YearVisible

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
        /// Identifies the YearVisible dependency property.
        /// </summary>
        public static readonly DependencyProperty YearVisibleProperty = DependencyProperty.Register(
            "YearVisible",
            typeof(bool),
            typeof(DatePicker),
            new PropertyMetadata(true, (d, e) => ((DatePicker)d).OnYearVisibleChanged(e)));

        private void OnYearVisibleChanged(DependencyPropertyChangedEventArgs e)
        {
            if (PickerFlyout != null)
            {
                PickerFlyout.YearVisible = YearVisible;
            }
        }

        #endregion

        #region DateStringFormat

        /// <summary>
        /// Gets or sets a composite string that specifies how to format the date if it is displayed as a string.
        /// </summary>
        /// 
        /// <returns>
        /// A composite string that specifies how to format the date if it is displayed as a string.
        /// </returns>
        public string DateStringFormat
        {
            get { return (string)GetValue(DateStringFormatProperty); }
            set { SetValue(DateStringFormatProperty, value); }
        }

        /// <summary>
        /// Identifies the DateStringFormat dependency property.
        /// </summary>
        public static readonly DependencyProperty DateStringFormatProperty = DependencyProperty.Register(
            "DateStringFormat",
            typeof(string),
            typeof(DatePicker),
            new PropertyMetadata(null, (d, e) => ((DatePicker)d).OnDateStringFormatChanged(e)));

        private void OnDateStringFormatChanged(DependencyPropertyChangedEventArgs e)
        {
            InvalidateValueStringFormat();
        }

        #endregion

        /// <summary>
        /// Gets the date and time to be formatted.
        /// </summary>
        protected override DateTimeOffset? Value
        {
            get { return Date; }
        }

        /// <summary>
        /// Gets the display format for the date.
        /// </summary>
        protected override string ValueStringFormat
        {
            get { return DateStringFormat; }
        }

        /// <summary>
        /// Gets the fallback value for the ValueStringFormat property.
        /// </summary>
        protected override string ValueStringFormatFallback
        {
            get
            {
                if (_fallbackValueStringFormat == null)
                {
                    string pattern = CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern;

                    if (DateTimePickerHelper.DateShouldFlowRTL())
                    {
                        char[] reversedPattern = pattern.ToCharArray();
                        Array.Reverse(reversedPattern);
                        pattern = new string(reversedPattern);
                    }

                    _fallbackValueStringFormat = "{0:" + pattern + "}";
                }

                return _fallbackValueStringFormat;
            }
        }

        /// <summary>
        /// Gets the default flyout title.
        /// </summary>
        protected override string DefaultFlyoutTitle
        {
            get { return DateTimePickerResources.DatePickerTitle; }
        }

        private DatePickerFlyout PickerFlyout
        {
            get { return Flyout as DatePickerFlyout; }
        }

        /// <summary>
        /// Occurs when the date value is changed.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1009:DeclareEventHandlersCorrectly")]
        public event EventHandler<DatePickerValueChangedEventArgs> DateChanged;

        /// <summary>
        /// Initializes a picker flyout that allows a user to pick a date value.
        /// </summary>
        /// 
        /// <returns>
        /// A picker flyout that allows a user to pick a date value.
        /// </returns>
        protected override PickerFlyoutBase CreateFlyout()
        {
            var flyout = new DatePickerFlyout
            {
                Date = Date.GetValueOrDefault(GetCurrentDate()),
                DayVisible = DayVisible,
                MonthVisible = MonthVisible,
                YearVisible = YearVisible
            };
            flyout.DatePicked += OnFlyoutDatePicked;
            return flyout;
        }

        private void OnFlyoutDatePicked(DatePickerFlyout sender, DatePickedEventArgs args)
        {
            Date = args.NewDate;
        }

        private static DateTimeOffset GetCurrentDate()
        {
            return DateTimeOffset.Now;
        }
    }
}
