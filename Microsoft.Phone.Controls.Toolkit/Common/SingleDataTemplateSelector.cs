using System.Windows;
using System.Windows.Controls;

namespace Microsoft.Phone.Controls
{
    /// <summary>
    /// A simple data template selector that selects the specified template only if the data object is not null.
    /// </summary>
    public sealed class SingleDataTemplateSelector : DataTemplateSelector
    {
        /// <summary>
        /// Gets or sets the DataTemplate.
        /// </summary>
        public DataTemplate Template { get; set; }

        /// <summary>
        /// Returns a <see cref="T:System.Windows.DataTemplate"/> based on custom logic.
        /// </summary>
        /// 
        /// <returns>
        /// The specified <see cref="T:System.Windows.DataTemplate"/> if the data object is not null; otherwise, null.
        /// </returns>
        /// <param name="item">The data object for which to select the template.</param>
        /// <param name="container">The data-bound object.</param>
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            return item != null ? Template : null;
        }
    }
}
