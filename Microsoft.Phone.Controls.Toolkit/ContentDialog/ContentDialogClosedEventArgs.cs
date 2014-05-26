using System;

namespace Microsoft.Phone.Controls
{
    /// <summary>
    /// Provides data for the Closed event.
    /// </summary>
    public sealed class ContentDialogClosedEventArgs : EventArgs
    {
        internal ContentDialogClosedEventArgs(ContentDialogResult result)
        {
            Result = result;
        }

        /// <summary>
        /// Gets the ContentDialogResult of the button click event.
        /// </summary>
        /// <returns>
        /// The result of the button click event.
        /// </returns>
        public ContentDialogResult Result { get; private set; }
    }
}
