using PhoneToolkitSample.Data;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace PhoneToolkitSample.Samples
{
    public partial class AccentColorPicker : UserControl
    {
        public AccentColorPicker()
        {
            InitializeComponent();

            Picker.ItemsSource = ColorExtensions.AccentColors();
            Picker.SelectedItem = "cobalt";

            Picker.SelectionChanged += OnPickerSelectionChanged;
            Picker.ItemClick += OnPickerItemClick;

            SetBinding(SelectedItemProperty, new Binding("SelectedItem") { Source = Picker, Mode = BindingMode.TwoWay });
        }

        #region public object SelectedItem

        public object SelectedItem
        {
            get { return (object)GetValue(SelectedItemProperty); }
            set { SetValue(SelectedItemProperty, value); }
        }

        public static readonly DependencyProperty SelectedItemProperty = DependencyProperty.Register(
            "SelectedItem",
            typeof(object),
            typeof(AccentColorPicker),
            null);

        #endregion

        public event EventHandler ItemPicked;

        private void OnPickerSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            RaiseItemPicked();
        }

        private void OnPickerItemClick(object sender, EventArgs e)
        {
            RaiseItemPicked();
        }

        private void RaiseItemPicked()
        {
            var handler = ItemPicked;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }
    }
}
