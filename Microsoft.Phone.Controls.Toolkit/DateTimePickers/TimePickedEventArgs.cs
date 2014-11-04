using System;
using System.Windows;

namespace Microsoft.Phone.Controls
{
    /// <summary>
    /// Provides data for the TimePicked event.
    /// </summary>
    public sealed class TimePickedEventArgs : DependencyObject
    {
        internal TimePickedEventArgs(TimeSpan oldTime, TimeSpan newTime)
        {
            OldTime = oldTime;
            NewTime = newTime;
        }

        /// <summary>
        /// Gets the old time value.
        /// </summary>
        /// 
        /// <returns>
        /// The old time value.
        /// </returns>
        public TimeSpan OldTime { get; private set; }

        /// <summary>
        /// Gets the time that was selected by the user.
        /// </summary>
        /// 
        /// <returns>
        /// The time that was selected by the user.
        /// </returns>
        public TimeSpan NewTime { get; private set; }
    }
}
