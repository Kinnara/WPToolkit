using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Phone.Data
{
    /// <summary>
    /// Wraps the asynchronous results of a LoadMoreItemsAsync call.
    /// </summary>
    [SuppressMessage("Microsoft.Performance", "CA1815:OverrideEqualsAndOperatorEqualsOnValueTypes")]
    public struct LoadMoreItemsResult
    {
        /// <summary>
        /// The number of items that were actually loaded.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Justification = "Consistency with WinRT")]
        public uint Count;
    }
}
