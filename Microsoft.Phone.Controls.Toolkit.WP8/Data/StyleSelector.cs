using System.Windows;

namespace System.Windows.Controls
{
    /// <summary>
    /// Provides a way to apply styles based on custom logic.
    /// </summary>
    public class StyleSelector
    {
        /// <summary>
        /// When overridden in a derived class, returns a <see cref="T:System.Windows.Style"/> based on custom logic.
        /// </summary>
        /// 
        /// <returns>
        /// Returns an application-specific style to apply; otherwise, null.
        /// </returns>
        /// <param name="item">The content.</param><param name="container">The element to which the style will be applied.</param>
        public virtual Style SelectStyle(object item, DependencyObject container)
        {
            return null;
        }
    }
}
