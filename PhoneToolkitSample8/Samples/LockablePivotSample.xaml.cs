// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System.Windows;
using System.Windows.Controls;
using Microsoft.Phone.Controls;
using PhoneToolkitSample.Data;

namespace PhoneToolkitSample.Samples
{
    public partial class LockablePivotSample : PhoneApplicationPage
    {
        private Slider[] sliders;
        public LockablePivotSample()
        {
            InitializeComponent();

            sliders = new Slider[] {
                slider1,
                slider2,
                slider3,
                slider4,
                slider5,
                slider6
                };

            InjectSampleData();

            MessageBox.Show(
@"The LockablePivot functionality is now built into the Windows Phone 8 Pivot control.

This sample and the sample code demonstrates how to use the new, built-in functionality.");
        }

        private void InjectSampleData()
        {
            textBlock1.Text = LoremIpsum.GetParagraph(10);
        }

        private void lockButton_Click(object sender, RoutedEventArgs e)
        {
            pivot.IsLocked = !pivot.IsLocked;
            lockButton.Content = (pivot.IsLocked ? "Unlock" : "Lock");

            foreach (Slider s in sliders)
            {
                s.IsEnabled = pivot.IsLocked;
            }
        }
    }
}