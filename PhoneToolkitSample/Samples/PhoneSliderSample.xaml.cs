using Microsoft.Phone.Controls;
using System.Windows;

namespace PhoneToolkitSample.Samples
{
    public partial class PhoneSliderSample : PhoneApplicationPage
    {
        public PhoneSliderSample()
        {
            InitializeComponent();

            TextSizeSlider.ValueChanged += TextSizeSlider_ValueChanged;
            UpdateSampleText();
        }

        private void TextSizeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            UpdateSampleText();
        }

        private void UpdateSampleText()
        {
            SampleTextBlock.Style = (Style)Resources["TextSize" + (int)TextSizeSlider.Value];
        }
    }
}