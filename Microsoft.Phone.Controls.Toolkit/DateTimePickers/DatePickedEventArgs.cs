using System;
using System.Windows;

namespace Microsoft.Phone.Controls
{
    /// <summary>
    /// Provides data for the DatePicked event.
    /// </summary>
    public sealed class DatePickedEventArgs : DependencyObject
    {
        internal DatePickedEventArgs(DateTimeOffset oldDate, DateTimeOffset newDate)
        {
            OldDate = oldDate;
            NewDate = newDate;
        }

        /// <summary>
        /// Gets the previous date.
        /// </summary>
        /// 
        /// <returns>
        /// The previous date.
        /// </returns>
        public DateTimeOffset OldDate { get; private set; }

        /// <summary>
        /// Gets the date that was selected by the user.
        /// </summary>
        /// 
        /// <returns>
        /// The date that was selected by the user.
        /// </returns>
        public DateTimeOffset NewDate { get; private set; }
    }
}
