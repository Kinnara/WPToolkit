using Microsoft.Phone.Controls;
using System.Windows;

namespace PhoneToolkitSample.Samples
{
    public partial class PropertyUISample : BasePage
    {
        public PropertyUISample()
        {
            InitializeComponent();
        }

        private void PropertyUI_Click(object sender, RoutedEventArgs e)
        {
            var source = (PropertyUI)sender;
            MessageBox.Show(source.Content.ToString(), source.Header.ToString(), MessageBoxButton.OK);
        }
    }
}