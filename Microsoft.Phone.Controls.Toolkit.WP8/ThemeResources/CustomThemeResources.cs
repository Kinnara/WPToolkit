using System;
using System.Collections;
using System.ComponentModel;
using System.Globalization;
using System.Windows;

namespace Microsoft.Phone.Controls
{
    public sealed class CustomThemeResources : ResourceDictionary
    {
        private static readonly Uri _source = new Uri(
            string.Format(
                CultureInfo.InvariantCulture,
                "/Microsoft.Phone.Controls.Toolkit;component/ThemeResources/{0}.xaml",
                Application.Current.Resources.IsDarkThemeActive() ? "Dark" : "Light"),
            UriKind.Relative);

        private static readonly ResourceDictionary _instance;

        static CustomThemeResources()
        {
            if (!DesignerProperties.IsInDesignTool)
            {
                _instance = new ResourceDictionary();
                Application.LoadComponent(_instance, _source);
            }
        }

        public CustomThemeResources()
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
