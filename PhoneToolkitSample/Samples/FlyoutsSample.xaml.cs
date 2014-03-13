using Microsoft.Phone.Controls;
using System;

namespace PhoneToolkitSample.Samples
{
    public partial class FlyoutsSample : BasePage
    {
        public FlyoutsSample()
        {
            InitializeComponent();
        }

        private void AccentColorPicker_ItemPicked(object sender, EventArgs e)
        {
            AccentColorPickerFlyout.Hide();
        }
    }
}