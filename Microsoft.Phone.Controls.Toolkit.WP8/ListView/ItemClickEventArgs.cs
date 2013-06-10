using System.Diagnostics.CodeAnalysis;
using System.Windows;

namespace Microsoft.Phone.Controls
{
    /// <summary>
    /// Represents the method that will handle an ItemClick event.
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1003:UseGenericEventHandlerInstances", Justification = "Consistency with WinRT")]
    public delegate void ItemClickEventHandler(object sender, ItemClickEventArgs e);

    /// <summary>
    /// Provides event data for the ItemClick event.
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
