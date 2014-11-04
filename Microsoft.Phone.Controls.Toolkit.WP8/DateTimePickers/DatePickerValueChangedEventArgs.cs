using System;

namespace Microsoft.Phone.Controls
{
    /// <summary>
    /// Provides event data for the DatePicker.DateChanged event.
    /// </summary>
    public sealed class DatePickerValueChangedEventArgs
    {
        internal DatePickerValueChangedEventArgs(DateTimeOffset? oldDate, DateTimeOffset? newDate)
        {
            OldDate = oldDate;
            NewDate = newDate;
        }

        /// <summary>
        /// Gets the new date selected in the picker.
        /// </summary>
        /// 
        /// <returns>
        /// The new date selected in the picker.
        /// </returns>
        public DateTimeOffset? NewDate { get; private set; }

        /// <summary>
        /// Gets the date previously selected in the picker.
        /// </summary>
        /// 
        /// <returns>
        /// The date previously selected in the picker.
        /// </returns>
        public DateTimeOffset? OldDate { get; private set; }
    }
}
