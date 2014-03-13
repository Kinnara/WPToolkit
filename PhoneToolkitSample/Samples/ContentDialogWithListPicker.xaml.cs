using Microsoft.Phone.Controls;

namespace PhoneToolkitSample.Samples
{
    public partial class ContentDialogWithListPicker : ContentDialog
    {
        public ContentDialogWithListPicker()
        {
            InitializeComponent();

            Picker.ItemsSource = new string[] { "5 minutes", "10 minutes", "1 hour", "4 hours", "1 day" };
        }

        private void ContentDialog_Closing(object sender, ContentDialogClosingEventArgs e)
        {
            if (Picker.IsExpanded)
            {
                e.Cancel = true;
            }
        }
    }
}
