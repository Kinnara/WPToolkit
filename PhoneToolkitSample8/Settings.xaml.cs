using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.Globalization;
using System.Threading;
using PhoneToolkitSample8;

namespace PhoneToolkitSample
{
    public partial class Settings : PhoneApplicationPage
    {
        List<string> _UICultureList, _RegionalCultureList;
        public Settings()
        {
            InitializeComponent();
            this.Loaded += Settings_Loaded;
            _UICultureList = new List<string>(){"ar-SA","az-Latn-AZ","be-BY","bg-BG","ca-ES","cs-CZ",
                "da-DK","de-DE","el-GR","en-GB","en-US","es-ES","es-MX","et-EE","fa-IR","fi-FI","fil-PH","fr-FR",
                "fr-CA","he-IL","hi-IN","hr-HR","hu-HU","id-ID","it-IT","ja-JP","kk-KZ","ko-KR","lt-LT","lv-LV",
                "mk-MK","ms-MY","nb-NO","nl-NL","pl-PL","pt-BR","pt-PT","ro-RO","ru-RU","sk-SK","sl-SI","sq-AL",
                "sr-Latn-CS","sv-SE","th-TH","tr-TR","uk-UA","uz-Latn-UZ","vi-VN","zh-CN","zh-TW"};

            _RegionalCultureList = new List<string>(){"ar-SA","az-Latn-AZ","be-BY","bg-BG","ca-ES","cs-CZ",
                "da-DK","de-DE","el-GR","en-GB","en-US","es-ES","es-MX","et-EE","fa-IR","fi-FI","fil-PH","fr-FR",
                "fr-CA","he-IL","hi-IN","hr-HR","hu-HU","id-ID","it-IT","ja-JP","kk-KZ","ko-KR","lt-LT","lv-LV",
                "mk-MK","ms-MY","nb-NO","nl-NL","pl-PL","pt-BR","pt-PT","ro-RO","ru-RU","sk-SK","sl-SI","sq-AL",
                "sr-Latn-CS","sv-SE","th-TH","tr-TR","uk-UA","uz-Latn-UZ","vi-VN","zh-CN","zh-TW"};
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