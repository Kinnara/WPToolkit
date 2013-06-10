using System.Windows;

namespace System.Windows.Controls
{
    /// <summary>
    /// Provides a way to choose a <see cref="T:System.Windows.DataTemplate"/> based on the data object and the data-bound element.
    /// </summary>
    public class DataTemplateSelector
    {
        /// <summary>
        /// When overridden in a derived class, returns a <see cref="T:System.Windows.DataTemplate"/> based on custom logic.
        /// </summary>
        /// 
        /// <returns>
        /// Returns a <see cref="T:System.Windows.DataTemplate"/> or null. The default value is null.
        /// </returns>
        /// <param name="item">The data object for which to select the template.</param>
        /// <param name="container">The data-bound object.</param>
        public virtual DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            return null;
        }
    }
}
