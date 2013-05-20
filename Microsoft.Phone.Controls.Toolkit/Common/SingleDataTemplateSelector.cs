using System.Windows;

namespace Microsoft.Phone.Controls
{
    /// <summary>
    /// A simple data template selector that selects the specified template only if the item is not null.
    /// </summary>
    public sealed class SingleDataTemplateSelector : DataTemplateSelector
    {
        /// <summary>
        /// Gets or sets the DataTemplate.
        /// </summary>
        public DataTemplate Template { get; set; }

        /// <summary>
        /// Returns a specific DataTemplate for a given item or container.
        /// </summary>
        /// <param name="item">The item to return a template for.</param>
        /// <param name="container">The parent container for the templated item.</param>
        /// <returns>The template to use for the given item and/or container.</returns>
        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {
            return item != null ? Template : null;
        }
    }
}
