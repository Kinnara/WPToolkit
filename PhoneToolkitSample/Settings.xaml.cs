using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.Threading;
using System.Globalization;

namespace PhoneToolkitSample
{
    public partial class Settings : PhoneApplicationPage
    {
        List<string> _UICultureList, _RegionalCultureList;
        public Settings()
        {
            InitializeComponent();
            this.Loaded += Settings_Loaded;
            _UICultureList = new List<string>(){"cs-CZ","da-DK","de-DE","el-GR","en-US","en-GB","es-ES",
                "fi-FI","fr-FR","hu-HU","id-ID","it-IT","ja-JP","ko-KR","ms-MY","nb-NO","nl-NL","pl-PL","pt-BR",
                "pt-PT","ru-RU","sv-SE","zh-CN","zh-TW"};

            _RegionalCultureList = new List<string>(){"cs-CZ","da-DK","de-DE","el-GR","en-US","en-GB","es-ES",
                "fi-FI","fr-FR","hu-HU","id-ID","it-IT","ja-JP","ko-KR","ms-MY","nb-NO","nl-NL","pl-PL","pt-BR",
                "pt-PT","ru-RU","sv-SE","zh-CN","zh-TW"};
        }

        void Settings_Loaded(object sender, RoutedEventArgs e)
        {
            this.currentUICulture_listpicker.ItemsSource = this._UICultureList;
            this.currentCulture_listpicker.ItemsSource = this._RegionalCultureList;

            this.currentCulture_listpicker.SelectedItem = App.RegionalCultureOverride.Name;
            this.currentUICulture_listpicker.SelectedItem = App.UICultureOverride.Name;

            this.currentUICulture_listpicker.SelectionChanged += currentUICulture_listpicker_SelectionChanged;
            this.currentCulture_listpicker.SelectionChanged += currentCulture_listpicker_SelectionChanged;
        }

        void currentCulture_listpicker_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            App.RegionalCultureOverride = Thread.CurrentThread.CurrentCulture = new CultureInfo(e.AddedItems[0] as string);
        }

        void currentUICulture_listpicker_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            App.UICultureOverride = Thread.CurrentThread.CurrentUICulture = new CultureInfo(e.AddedItems[0] as string);
        }
    }
}