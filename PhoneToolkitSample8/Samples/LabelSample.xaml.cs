using Microsoft.Phone.Controls;
using System.Linq;
using System.Windows;

namespace PhoneToolkitSample.Samples
{
    public partial class LabelSample : BasePage
    {
        public LabelSample()
        {
            InitializeComponent();

            StyleList.ItemsSource = new StandardStyles().Where(pair =>
            {
                var style = pair.Value as Style;
                return style != null && style.TargetType == typeof(Label) && !((string)pair.Key).EndsWith("Base");
            }).ToList();

            UpdateMaxLinesButtons();
        }

        private void MaxLinesIncreaseButton_Click(object sender, RoutedEventArgs e)
        {
            MaxLinesSampleLabel.MaxLines++;
            UpdateMaxLinesButtons();
        }

        private void MaxLinesDecreaseButton_Click(object sender, RoutedEventArgs e)
        {
            MaxLinesSampleLabel.MaxLines--;
            UpdateMaxLinesButtons();
        }

        private void UpdateMaxLinesButtons()
        {
            MaxLinesDecreaseButton.IsEnabled = MaxLinesSampleLabel.MaxLines > 0;
        }
    }
}