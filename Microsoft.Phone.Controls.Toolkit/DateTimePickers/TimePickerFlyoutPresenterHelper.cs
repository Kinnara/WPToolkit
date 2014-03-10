using Microsoft.Phone.Controls.Primitives;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace Microsoft.Phone.Controls
{
    internal sealed class TimePickerFlyoutPresenterHelper : DateTimePickerFlyoutPresenterHelperBase
    {
        private TimePickerFlyoutPresenter _presenter;

        internal TimePickerFlyoutPresenterHelper(TimePickerFlyout flyout, TimePickerFlyoutPresenter presenter)
            : base(flyout, presenter)
        {
            _presenter = presenter;
        }

        protected override void InitializePickers()
        {
            FirstPicker.DataSource = DateTimeWrapper.CurrentCultureUsesTwentyFourHourClock() ?
                 (DataSource)(new TwentyFourHourDataSource()) :
                 (DataSource)(new TwelveHourDataSource());
            SecondPicker.DataSource = new MinuteDataSource();
            ThirdPicker.DataSource = new AmPmDataSource();
        }

        protected override void CommitCore(DateTime value)
        {
            _presenter.Time = value.TimeOfDay;
        }

        /// <summary>
        /// Gets a sequence of LoopingSelector parts ordered according to culture string for date/time formatting.
        /// </summary>
        /// <returns>LoopingSelectors ordered by culture-specific priority.</returns>
        protected override IEnumerable<LoopingSelector> GetSelectorsOrderedByCulturePattern()
        {
            string pattern = CultureInfo.CurrentCulture.DateTimeFormat.LongTimePattern.ToUpperInvariant();

            // The goal is to put the AM/PM part at the beginning for RTL languages.
            if (DateTimePickerHelper.IsRTLLanguage())
            {
                var parts = pattern.Split(' ');
                Array.Reverse(parts);
                pattern = string.Join(" ", parts);
            }

            return DateTimePickerFlyoutPresenterHelperBase.GetSelectorsOrderedByCulturePattern(
                pattern,
                new char[] { 'H', 'M', 'T' },
                new LoopingSelector[] { FirstPicker, SecondPicker, ThirdPicker });
        }
    }
}
