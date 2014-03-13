using System;

namespace Microsoft.Phone.Controls
{
    public sealed class ContentDialogButtonClickEventArgs : EventArgs
    {
        internal ContentDialogButtonClickEventArgs()
        {
        }

        public bool Cancel { get; set; }
    }
}
