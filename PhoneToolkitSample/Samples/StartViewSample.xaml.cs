using Microsoft.Phone.Controls;
using PhoneToolkitSample.Data;
using System.Linq;
using System.Windows;

namespace PhoneToolkitSample.Samples
{
    public partial class StartViewSample : BasePage
    {
        public StartViewSample()
        {
            InitializeComponent();

            AppList.ItemsSource = ColorExtensions.AccentColors().Select(i => char.ToUpper(i[0]) + i.Substring(1)).ToArray();
        }

        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            Pano.SelectedIndex = 1;
        }
    }
}