using System;

namespace Microsoft.Phone.Controls
{
    /// <summary>
    /// Provides event data for the TimePicker.TimeChanged event.
    /// </summary>
    public sealed class TimePickerValueChangedEventArgs : EventArgs
    {
        internal TimePickerValueChangedEventArgs(TimeSpan oldTime, TimeSpan newTime)
        {
            OldTime = oldTime;
            NewTime = newTime;
        }

        /// <summary>
        /// Gets the time previously selected in the picker.
        /// </summary>
        /// 
        /// <returns>
        /// The time previously selected in the picker.
        /// </returns>
        public TimeSpan OldTime { get; private set; }

        /// <summary>
        /// Gets the new time selected in the picker.
        /// </summary>
        /// 
        /// <returns>
        /// The new time selected in the picker.
        /// </returns>
        public TimeSpan NewTime { get; private set; }
    }
}
