using System;

namespace Microsoft.Phone.Controls
{
    public sealed class ContentDialogClosingEventArgs : EventArgs
    {
        internal ContentDialogClosingEventArgs(ContentDialogResult result)
        {
            Result = result;
        }

        public bool Cancel { get; set; }

        public ContentDialogResult Result { get; private set; }
    }
}
