using System;

namespace Microsoft.Phone.Controls
{
    /// <summary>
    /// Provides data for the closing event.
    /// </summary>
    public sealed class ContentDialogClosingEventArgs : EventArgs
    {
        internal ContentDialogClosingEventArgs(ContentDialogResult result)
        {
            Result = result;
        }

        /// <summary>
        /// Gets or sets a value that can cancel the closing of the dialog. A true value for Cancel cancels the default behavior.
        /// </summary>
        /// <returns>
        /// True to cancel the closing of the dialog; Otherwise, false.
        /// </returns>
        public bool Cancel { get; set; }

        /// <summary>
        /// Gets the ContentDialogResult of the closing event.
        /// </summary>
        /// <returns>
        /// The ContentDialogResult of the closing event.
        /// </returns>
        public ContentDialogResult Result { get; private set; }
    }
}
