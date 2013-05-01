using System.Windows;

namespace Microsoft.Phone.Controls
{
    /// <summary>
    /// Represents the method that will handle an <see cref="E:Microsoft.Phone.Controls.ListView.ItemClick"/> event.
    /// </summary>
    public delegate void ItemClickEventHandler(object sender, ItemClickEventArgs e);

    /// <summary>
    /// Provides event data for the <see cref="E:Microsoft.Phone.Controls.ListView.ItemClick"/> event.
    /// </summary>
    public sealed class ItemClickEventArgs : RoutedEventArgs
    {
        private readonly object _clickedItem;

        /// <summary>
        /// Gets a reference to the clicked item.
        /// </summary>
        /// 
        /// <returns>
        /// The clicked item.
        /// </returns>
        public object ClickedItem
        {
            get { return _clickedItem; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Microsoft.Phone.Controls.ItemClickEventArgs"/> class.
        /// </summary>
        internal ItemClickEventArgs(object clickedItem)
        {
            _clickedItem = clickedItem;
        }
    }
}
