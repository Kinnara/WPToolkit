using Microsoft.Phone.Controls;
using PhoneToolkitSample.Data;
using System.Windows.Controls;

namespace PhoneToolkitSample.Samples
{
    public partial class FlipViewSample : PhoneApplicationPage
    {
        public FlipViewSample()
        {
            InitializeComponent();

            OrientationPicker.SelectionChanged += OrientationPicker_SelectionChanged;
            OrientationPicker.ItemsSource = new[]
            {
                System.Windows.Controls.Orientation.Horizontal,
                System.Windows.Controls.Orientation.Vertical
            };

            DataContext = ColorExtensions.AccentColors();
        }

        private void OrientationPicker_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if ((System.Windows.Controls.Orientation)OrientationPicker.SelectedItem == System.Windows.Controls.Orientation.Horizontal)
            {
                HorizontalFlipView.Visibility = System.Windows.Visibility.Visible;
                VerticalFlipView.Visibility = System.Windows.Visibility.Collapsed;
            }
            else
            {
                HorizontalFlipView.Visibility = System.Windows.Visibility.Collapsed;
                VerticalFlipView.Visibility = System.Windows.Visibility.Visible;
            }
        }
    }
}