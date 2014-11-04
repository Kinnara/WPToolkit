using Microsoft.Phone.Controls.Primitives;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace Microsoft.Phone.Controls
{
    internal sealed class DatePickerFlyoutPresenterHelper : DateTimePickerFlyoutPresenterHelperBase
    {
        private DatePickerFlyout _flyout;
        private DatePickerFlyoutPresenter _presenter;

        internal DatePickerFlyoutPresenterHelper(DatePickerFlyout flyout, DatePickerFlyoutPresenter presenter)
            : base(flyout, presenter)
        {
            _flyout = flyout;
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

            if (DateTimePickerHelper.DateShouldFlowRTL())
            {
                char[] reversedPattern = pattern.ToCharArray();
                Array.Reverse(reversedPattern);
                pattern = new string(reversedPattern);
            }

            var patternCharacters = new List<char>();
            var selectors = new List<LoopingSelector>();

            if (_flyout.YearVisible)
            {
                patternCharacters.Add('Y');
                selectors.Add(FirstPicker);
            }

            if (_flyout.MonthVisible)
            {
                patternCharacters.Add('M');
                selectors.Add(SecondPicker);
            }

            if (_flyout.DayVisible)
            {
                patternCharacters.Add('D');
                selectors.Add(ThirdPicker);
            }

            return DateTimePickerFlyoutPresenterHelperBase.GetSelectorsOrderedByCulturePattern(
                pattern,
                patternCharacters.ToArray(),
                selectors.ToArray());
        }
    }
}
