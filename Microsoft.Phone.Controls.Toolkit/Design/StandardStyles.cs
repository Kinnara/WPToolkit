using System;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Windows;

namespace Microsoft.Phone.Controls
{
    /// <summary>
    /// Standard styles for Windows Phone.
    /// </summary>
    [SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix")]
    public sealed class StandardStyles : ResourceDictionary
    {
        private static readonly Uri _source = new Uri("/Microsoft.Phone.Controls.Toolkit;component/Design/StandardStyles.xaml", UriKind.Relative);

        private static readonly ResourceDictionary _instance;

        [SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline")]
        static StandardStyles()
        {
            if (!DesignerProperties.IsInDesignTool)
            {
                _instance = new ResourceDictionary();
                Application.LoadComponent(_instance, _source);
            }
        }

        /// <summary>
        /// Initializes a new instance of the
        /// <see cref="T:Microsoft.Phone.Controls.StandardStyles" /> class.
        /// </summary>
        public StandardStyles()
        {
            if (DesignerProperties.IsInDesignTool)
            {
                Source = _source;
            }
            else
            {
                foreach (DictionaryEntry entry in _instance)
                {
                    Add(entry.Key, entry.Value);
                }
            }
        }
    }
}
