using Microsoft.Phone.Controls.LocalizedResources;
using Microsoft.Phone.Controls.Primitives;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Windows;

namespace Microsoft.Phone.Controls
{
    /// <summary>
    /// Represents a control that allows a user to pick a time value.
    /// </summary>
    public class TimePicker : DateTimePickerBase
    {
        private string _fallbackValueStringFormat;

        /// <summary>
        /// Initializes a new instance of the TimePicker class.
        /// </summary>
        public TimePicker()
        {
            DefaultStyleKey = typeof(TimePicker);

            Time = GetCurrentTime();
        }

        #region Time

        /// <summary>
        /// Gets or sets the time currently set in the time picker.
        /// </summary>
        /// 
        /// <returns>
        /// The time currently set in the time picker.
        /// </returns>
        public TimeSpan? Time
        {
            get { return (TimeSpan?)GetValue(TimeProperty); }
            set { SetValue(TimeProperty, value); }
        }

        /// <summary>
        /// Gets the identifier for the Time dependency property.
        /// </summary>
        public static readonly DependencyProperty TimeProperty = DependencyProperty.Register(
            "Time",
            typeof(object),
            typeof(TimePicker),
            new PropertyMetadata(null, (d, e) => ((TimePicker)d).OnTimeChanged(e)));

        private void OnTimeChanged(DependencyPropertyChangedEventArgs e)
        {
            var oldTime = (TimeSpan?)e.OldValue;
            var newTime = (TimeSpan?)e.NewValue;

            if (PickerFlyout != null)
            {
                PickerFlyout.Time = Time.GetValueOrDefault(GetCurrentTime());
            }

            InvalidateValue();

            var handler = TimeChanged;
            if (handler != null)
            {
                handler(this, new TimePickerValueChangedEventArgs(oldTime, newTime));
            }
        }

        #endregion

        #region TimeStringFormat

        /// <summary>
        /// Gets or sets a composite string that specifies how to format the time if it is displayed as a string.
        /// </summary>
        /// 
        /// <returns>
        /// A composite string that specifies how to format the time if it is displayed as a string.
        /// </returns>
        public string TimeStringFormat
        {
            get { return (string)GetValue(TimeStringFormatProperty); }
            set { SetValue(TimeStringFormatProperty, value); }
        }

        /// <summary>
        /// Identifies the TimeStringFormat dependency property.
        /// </summary>
        public static readonly DependencyProperty TimeStringFormatProperty = DependencyProperty.Register(
            "TimeStringFormat",
            typeof(string),
            typeof(TimePicker),
            new PropertyMetadata(null, (d, e) => ((TimePicker)d).OnTimeStringFormatChanged(e)));

        private void OnTimeStringFormatChanged(DependencyPropertyChangedEventArgs e)
        {
            InvalidateValueStringFormat();
        }

        #endregion

        /// <summary>
        /// Gets the date and time to be formatted.
        /// </summary>
        protected override DateTimeOffset? Value
        {
            get
            {
                if (Time.HasValue)
                {
                    return new DateTimeOffset(DateTime.Today.Add(Time.Value));
                }
                else
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// Gets the display format for the time.
        /// </summary>
        protected override string ValueStringFormat
        {
            get { return TimeStringFormat; }
        }

        /// <summary>
        /// Gets the fallback value for the ValueStringFormat property.
        /// </summary>
        protected override string ValueStringFormatFallback
        {
            get
            {
                if (null == _fallbackValueStringFormat)
                {
                    // Need to convert LongTimePattern into ShortTimePattern to work around a platform bug
                    // such that only LongTimePattern respects the "24-hour clock" override setting.
                    // This technique is not perfect, but works for all the initially-supported languages.
                    string pattern = CultureInfo.CurrentCulture.DateTimeFormat.LongTimePattern.Replace(":ss", "");
                    string lang = CultureInfo.CurrentCulture.TwoLetterISOLanguageName;

                    if (lang == "ar" || lang == "fa")
                    {
                        // For arabic and persian, we want the am/pm designator to be displayed at the left.
                        pattern = "\u200F" + pattern;
                    }
                    else
                    {
                        // For LTR languages and Hebrew, we want the am/pm designator to be displayed at the right.
                        pattern = "\u200E" + pattern;
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
            get { return ControlResources.TimePickerTitle; }
        }

        private TimePickerFlyout PickerFlyout
        {
            get { return Flyout as TimePickerFlyout; }
        }

        /// <summary>
        /// Occurs when the time value is changed.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1009:DeclareEventHandlersCorrectly")]
        public event EventHandler<TimePickerValueChangedEventArgs> TimeChanged;

        /// <summary>
        /// Initializes a picker flyout that allows a user to pick a time value.
        /// </summary>
        /// 
        /// <returns>
        /// A picker flyout that allows a user to pick a time value.
        /// </returns>
        protected override PickerFlyoutBase CreateFlyout()
        {
            var flyout = new TimePickerFlyout
            {
                Time = Time.GetValueOrDefault(GetCurrentTime()),
            };
            flyout.TimePicked += OnFlyoutTimePicked;
            return flyout;
        }

        private void OnFlyoutTimePicked(TimePickerFlyout sender, TimePickedEventArgs args)
        {
            Time = args.NewTime;
        }

        private static TimeSpan GetCurrentTime()
        {
            return DateTimeOffset.Now.TimeOfDay;
        }
    }
}
