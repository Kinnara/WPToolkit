using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using PhoneToolkitSample.Resources;

namespace PhoneToolkitSample8
{
    public partial class MainPage : PhoneApplicationPage
    {
        private SampleItem[] _samples;

        // Constructor
        public MainPage()
        {
            InitializeComponent();
            InitializeData();
            lls.ItemsSource = _samples;
        }

        private void InitializeData()
        {
            // These items show up sorted by the second parameter, not the name of the file
            _samples = new SampleItem[] {
                new SampleItem("/Samples/AutoCompleteBoxSample.xaml","autocompletebox","completion of text based on items" ),
                new SampleItem("/Samples/ContextMenuSample.xaml","contextmenu","tap & hold menu options control" ),
                new SampleItem("/Samples/CustomMessageBoxSample.xaml","custommessagebox","a modular dialog box to display notifications" ),
                new SampleItem("/Samples/DateTimeConvertersSample.xaml","date+time converters","localized time display for data binding" ),
                new SampleItem("/Samples/DateTimePickerSample.xaml","date & time pickers","controls to select dates or times" ),
                new SampleItem("/Samples/ExpanderViewSample.xaml","expanderview","shows sub-items similar to the new email app" ),
                new SampleItem("/Samples/FeatheredTransitionsSample1.xaml","featheredtransitions","animate multiple elements between pages" ),
                new SampleItem("/Samples/GestureSample.xaml","gestures*","a gesture service for tap pinch, etc." ),
                new SampleItem("/Samples/HubTileSample.xaml","hubtile","tile control that is animated and alive" ),
                new SampleItem("/Samples/ListPickerSample.xaml","listpicker","select from a few items inline or many" ),
                new SampleItem("/Samples/LockablePivotSample.xaml","lockablepivot*","a pivot that allows disabling navigation" ),
                new SampleItem("/Samples/LongListMultiSelectorSample.xaml","longlistmultiselector","long list selector with multiple selection" ),
                new SampleItem("/Samples/LongListSelectorSample.xaml","longlistselector*","a grouping items control with great performance" ),
                new SampleItem("/Samples/MapsSample.xaml","mapssample*","a sample of map api" ),
                new SampleItem("/Samples/LongListMultiSelectorSample.xaml?multiselect","multiselectlist*","support multiple selection in a list" ),
                new SampleItem("/Samples/PerformanceProgressBarSample.xaml","performanceprogressbar*","show indeterminate progress in style" ),
                new SampleItem("/Samples/PhoneTextBoxSample.xaml","phonetextbox","add new features to the textbox" ),
                new SampleItem("/Samples/SpeechTextBoxSample.xaml","speechtextbox","a speech-enabled phonetextbox" ),
                new SampleItem("/Samples/RatingControlSample.xaml","ratingcontrol","simple control for star-based rating" ),
                new SampleItem("/Samples/SlideInEffectSample.xaml","slideineffect","make elements responsive to pivotitem sliding" ),
                new SampleItem("/Samples/ToggleSwitchSample.xaml","toggleswitch","offer a touch control for on/off choices" ),
                new SampleItem("/Samples/TiltEffectSample.xaml","tilteffect","make buttons visually responsive to touch" ),
                new SampleItem("/Samples/TransferControlSample.xaml","transfercontrol","controls to show background downloads and uploads" ),
                new SampleItem("/Samples/TransitionsSample.xaml","transitions","beautifully animate between pages" ),
                new SampleItem("/Samples/WrapPanelSample.xaml","wrappanel","a non-virtualized wrapping control" ) 
            };
        }

        private void ApplicationBarIconSettingsButton_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/Settings.xaml", UriKind.RelativeOrAbsolute));
        }   
    }

    public class SampleItem
    {
        public SampleItem(string url, string header, string description)
        {
            Url = url;
            Header = header;
            Description = description;
        }

        public string Url { get; private set; }
        public string Header { get; private set; }
        public string Description { get; private set; }
    }
}