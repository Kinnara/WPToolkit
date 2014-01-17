using System;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Windows;

namespace Microsoft.Phone.Controls
{
    /// <summary>
    /// Shared resources.
    /// </summary>
    [SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix")]
    public sealed class SharedResources : ResourceDictionary
    {
        private static readonly Uri _source = new Uri("/Microsoft.Phone.Controls.Toolkit;component/Common/SharedResources.xaml", UriKind.Relative);

        private static readonly ResourceDictionary _instance;

        [SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline")]
        static SharedResources()
        {
            if (!DesignerProperties.IsInDesignTool)
            {
                _instance = new ResourceDictionary();
                Application.LoadComponent(_instance, _source);
            }
        }

        /// <summary>
        /// Initializes a new instance of the
        /// <see cref="T:Microsoft.Phone.Controls.SharedResources" /> class.
        /// </summary>
        public SharedResources()
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
