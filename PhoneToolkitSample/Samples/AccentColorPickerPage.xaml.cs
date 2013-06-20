using Microsoft.Phone.Controls;
using System.Windows;
using System.Windows.Media.Animation;

namespace PhoneToolkitSample.Samples
{
    public partial class AccentColorPickerPage : PickerBoxPageBase
    {
        public AccentColorPickerPage()
        {
            InitializeComponent();

            InitializePickerBoxPage(Picker, HeaderTitle);
        }

        protected override Storyboard AnimationForElement(FrameworkElement element, int index)
        {
            if (index > 0)
            {
                index = (index - 1) / 4;
            }

            return base.AnimationForElement(element, index);
        }
    }
}