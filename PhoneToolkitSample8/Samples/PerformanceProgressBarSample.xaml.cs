// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System;
using System.Windows;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

namespace PhoneToolkitSample.Samples
{
    public partial class PerformanceProgressBarSample : PhoneApplicationPage
    {
        private ProgressIndicator _progressIndicator;

        public PerformanceProgressBarSample()
        {
            InitializeComponent();

            MessageBox.Show(
@"The PerformanceProgressBar is obsolete in Windows Phone 8, as the built-in progress bar control now animates off of the UI thread.

This sample demonstrates the usage of either the progress indicator or the progress bar control to provide performant loading feedback for users.");
        }

        private void Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            // This blocks the user interface thread for 4 seconds.
            System.Threading.Thread.Sleep(TimeSpan.FromSeconds(4));
        }

        private void CheckBox_Checked(object sender, System.Windows.RoutedEventArgs e)
        {
            if (null == _progressIndicator)
            {
                _progressIndicator = new ProgressIndicator();
                _progressIndicator.IsVisible = true;
                SystemTray.ProgressIndicator = _progressIndicator;
            }

            _progressIndicator.IsIndeterminate = true;
        }

        private void CheckBox_Unchecked(object sender, System.Windows.RoutedEventArgs e)
        {
            _progressIndicator.IsIndeterminate = false;
        }

    }
}