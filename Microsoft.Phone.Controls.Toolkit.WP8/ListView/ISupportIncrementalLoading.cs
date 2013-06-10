using System.Threading.Tasks;

namespace Microsoft.Phone.Data
{
    /// <summary>
    /// Specifies a calling contract for collection views that support incremental loading.
    /// </summary>
    public interface ISupportIncrementalLoading
    {
        /// <summary>
        /// Gets a sentinel value that supports incremental loading implementations.
        /// </summary>
        /// 
        /// <returns>
        /// True if additional unloaded items remain in the view; otherwise, false.
        /// </returns>
        bool HasMoreItems { get; }

        /// <summary>
        /// Initializes incremental loading from the view.
        /// </summary>
        /// 
        /// <returns>
        /// The wrapped results of the load operation.
        /// </returns>
        /// <param name="count">The number of items to load.</param>
        Task<LoadMoreItemsResult> LoadMoreItemsAsync(uint count);
    }
}
