using Microsoft.Phone.Controls.Primitives;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace Microsoft.Phone.Controls
{
    internal sealed class DatePickerFlyoutPresenterHelper : DateTimePickerFlyoutPresenterHelperBase
    {
        private DatePickerFlyoutPresenter _presenter;

        internal DatePickerFlyoutPresenterHelper(DatePickerFlyout flyout, DatePickerFlyoutPresenter presenter)
            : base(flyout, presenter)
        {
            _presenter = presenter;
        }

        protected override void InitializePickers()
        {
            FirstPicker.DataSource = new YearDataSource();
            SecondPicker.DataSource = new MonthDataSource();
            ThirdPicker.DataSource = new DayDataSource();
        }

        protected override void CommitCore(DateTime value)
        {
            _presenter.Date = value;
        }

        /// <summary>
        /// Gets a sequence of LoopingSelector parts ordered according to culture string for date/time formatting.
        /// </summary>
        /// <returns>LoopingSelectors ordered by culture-specific priority.</returns>
        protected override IEnumerable<LoopingSelector> GetSelectorsOrderedByCulturePattern()
        {
            string pattern = CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern.ToUpperInvariant();

            if (DateTimePickerBase.DateShouldFlowRTL())
            {
                char[] reversedPattern = pattern.ToCharArray();
                Array.Reverse(reversedPattern);
                pattern = new string(reversedPattern);
            }

            return DateTimePickerFlyoutPresenterHelperBase.GetSelectorsOrderedByCulturePattern(
                pattern,
                new char[] { 'Y', 'M', 'D' },
                new LoopingSelector[] { FirstPicker, SecondPicker, ThirdPicker });
        }
    }
}
