using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using PhoneToolkitSample.Data;
using System;
using System.Collections.ObjectModel;
using System.Windows.Controls;

namespace PhoneToolkitSample.Samples
{
    public partial class FlipViewSample : BasePage
    {
        private ObservableCollection<string> _items = new ObservableCollection<string>(ColorExtensions.AccentColors());

        public FlipViewSample()
        {
            InitializeComponent();

            AppBarPrevious = (ApplicationBarIconButton)ApplicationBar.Buttons[0];
            AppBarNext = (ApplicationBarIconButton)ApplicationBar.Buttons[1];
            AppBarRemove = (ApplicationBarMenuItem)ApplicationBar.MenuItems[0];

            OrientationPicker.SelectionChanged += OrientationPicker_SelectionChanged;
            OrientationPicker.ItemsSource = new[]
            {
                System.Windows.Controls.Orientation.Horizontal,
                System.Windows.Controls.Orientation.Vertical
            };
            OrientationPicker.SelectedIndex = 0;

            UpdateSelectionModePicker.ItemsSource = new[]
            {
                UpdateSelectionMode.AfterTransition,
                UpdateSelectionMode.BeforeTransition
            };
            UpdateSelectionModePicker.SelectedIndex = 0;

            DataContext = _items;
        }

        private void OrientationPicker_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if ((System.Windows.Controls.Orientation)OrientationPicker.SelectedItem == System.Windows.Controls.Orientation.Horizontal)
            {
                HorizontalFlipView.Visibility = System.Windows.Visibility.Visible;
                VerticalFlipView.Visibility = System.Windows.Visibility.Collapsed;
                AppBarPrevious.IconUri = new Uri("/Assets/AppBar/back.png", UriKind.Relative);
                AppBarNext.IconUri = new Uri("/Assets/AppBar/next.png", UriKind.Relative);
            }
            else
            {
                HorizontalFlipView.Visibility = System.Windows.Visibility.Collapsed;
                VerticalFlipView.Visibility = System.Windows.Visibility.Visible;
                AppBarPrevious.IconUri = new Uri("/Assets/AppBar/up.png", UriKind.Relative);
                AppBarNext.IconUri = new Uri("/Assets/AppBar/down.png", UriKind.Relative);
            }
        }

        private void HorizontalFlipView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateAppBar();
        }

        private void AppBarPrevious_Click(object sender, EventArgs e)
        {
            if (HorizontalFlipView.SelectedIndex > 0)
            {
                HorizontalFlipView.SelectedIndex--;
            }
        }

        private void AppBarNext_Click(object sender, EventArgs e)
        {
            if (HorizontalFlipView.SelectedIndex < HorizontalFlipView.Items.Count)
            {
                HorizontalFlipView.SelectedIndex++;
            }
        }

        private void AppBarRemove_Click(object sender, EventArgs e)
        {
            if (HorizontalFlipView.SelectedItem != null)
            {
                _items.Remove((string)HorizontalFlipView.SelectedItem);
            }
        }

        private void UpdateAppBar()
        {
            int itemsCount = HorizontalFlipView.Items.Count;
            int selectedIndex = HorizontalFlipView.SelectedIndex;
            AppBarPrevious.IsEnabled = itemsCount > 0 && selectedIndex > 0;
            AppBarNext.IsEnabled = itemsCount > 0 && selectedIndex < itemsCount - 1;
            AppBarRemove.IsEnabled = selectedIndex != -1;
        }
    }
}