using Microsoft.Phone.Controls;

namespace PhoneToolkitSample.Samples
{
    public partial class ContentDialogWithPivot : ContentDialog
    {
        public ContentDialogWithPivot()
        {
            InitializeComponent();
        }

        private void ContentDialog_SecondaryButtonClick(object sender, ContentDialogButtonClickEventArgs e)
        {
            e.Cancel = true;
        }
    }
}
