using System.Windows;

namespace Microsoft.Phone.Controls
{
    /// <summary>
    /// Enables custom template selection logic at the application level.
    /// </summary>
    public class DataTemplateSelector
    {
        /// <summary>
        /// Returns a specific DataTemplate for a given item or container.
        /// </summary>
        /// <param name="item">The item to return a template for.</param>
        /// <param name="container">The parent container for the templated item.</param>
        /// <returns>The template to use for the given item and/or container.</returns>
        public DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            return SelectTemplateCore(item, container);
        }

        /// <summary>
        /// When implemented by a derived class, returns a specific DataTemplate for a given item or container.
        /// </summary>
        /// <param name="item">The item to return a template for.</param>
        /// <param name="container">The parent container for the templated item.</param>
        /// <returns>The template to use for the given item and/or container.</returns>
        protected virtual DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {
            return null;
        }
    }
}
