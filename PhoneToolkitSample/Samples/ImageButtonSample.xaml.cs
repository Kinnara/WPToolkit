using Microsoft.Phone.Controls;
using System.Windows;

namespace PhoneToolkitSample.Samples
{
    public partial class ImageButtonSample : BasePage
    {
        public ImageButtonSample()
        {
            InitializeComponent();
        }

        private void ImageButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show((string)((FrameworkElement)sender).DataContext);
        }
    }
}