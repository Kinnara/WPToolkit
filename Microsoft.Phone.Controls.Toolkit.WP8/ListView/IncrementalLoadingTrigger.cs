namespace Microsoft.Phone.Controls
{
    /// <summary>
    /// Defines constants that specify list view incremental loading behavior (IncrementalLoadingTrigger property).
    /// </summary>
    public enum IncrementalLoadingTrigger
    {
        /// <summary>
        /// Incremental loading does not occur.
        /// </summary>
        None,

        /// <summary>
        /// Uses an "edge" offset for incremental loading visual behavior, and enables
        /// the list view to notify the scroll host of incremental load per interaction
        /// with other settings (IncrementalLoadingThreshold, DataFetchSize).
        /// </summary>
        Edge,
    }
}
