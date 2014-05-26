using System;

namespace Microsoft.Phone.Controls
{
    /// <summary>
    /// Provides data for the button click events.
    /// </summary>
    public sealed class ContentDialogButtonClickEventArgs : EventArgs
    {
        internal ContentDialogButtonClickEventArgs()
        {
        }

        /// <summary>
        /// Gets or sets a value that can cancel the button click. A true value for Cancel cancels the default behavior.
        /// </summary>
        /// <returns>
        /// True to cancel the button click; Otherwise, false.
        /// </returns>
        public bool Cancel { get; set; }
    }
}
