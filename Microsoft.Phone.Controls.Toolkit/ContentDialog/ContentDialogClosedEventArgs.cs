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

        public ContentDialogResult Result { get; private set; }
    }
}
