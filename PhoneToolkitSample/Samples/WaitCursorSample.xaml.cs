using Microsoft.Phone.Controls;
using System;
using System.Windows;
using System.Windows.Threading;

namespace PhoneToolkitSample.Samples
{
    public partial class WaitCursorSample : BasePage
    {
        public WaitCursorSample()
        {
            InitializeComponent();
        }

        private void ShowButton_Click(object sender, RoutedEventArgs e)
        {
            WaitIndicator.IsVisible = true;

            var timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(6) };
            timer.Tick += delegate
            {
                timer.Stop();
                WaitIndicator.IsVisible = false;
            };
            timer.Start();
        }
    }
}