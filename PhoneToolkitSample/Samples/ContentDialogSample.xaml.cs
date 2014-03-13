// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.Phone.Controls;

namespace PhoneToolkitSample.Samples
{
    public partial class ContentDialogSample : BasePage
    {
        public ContentDialogSample()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Displays a ContentDialog with no content.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event information.</param>
        private async void BasicContentDialog_Click(object sender, RoutedEventArgs e)
        {
            var contentDialog = new BasicContentDialog()
            {                
                FullSizeDesired = (bool)FullScreenCheckBox.IsChecked
            };

            switch (await contentDialog.ShowAsync())
            {
                case ContentDialogResult.Primary:
                    // Do something.
                    break;
                case ContentDialogResult.Secondary:
                    // Do something.
                    break;
                case ContentDialogResult.None:
                    // Do something.
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Displays a ContentDialog with a HyperlinkButton as content.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event information.</param>
        private async void ContentDialogWithHyperlinkButton_Click(object sender, RoutedEventArgs e)
        {
            var contentDialog = new ContentDialogWithHyperlinkButton()
            {
                FullSizeDesired = (bool)FullScreenCheckBox.IsChecked
            };

            switch (await contentDialog.ShowAsync())
            {
                case ContentDialogResult.Primary:
                    // Do something.
                    break;
                case ContentDialogResult.Secondary:
                    // Do something.
                    break;
                case ContentDialogResult.None:
                    // Do something.
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Displays a ContentDialog with a CheckBox as content.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event information.</param>
        private async void ContentDialogWithCheckBox_Click(object sender, RoutedEventArgs e)
        {
            var contentDialog = new ContentDialogWithCheckBox()
            {
                FullSizeDesired = (bool)FullScreenCheckBox.IsChecked
            };

            switch (await contentDialog.ShowAsync())
            {
                case ContentDialogResult.Primary:
                    // Launch Marketplace review task.
                    // Do not ask again.
                    break;
                case ContentDialogResult.Secondary:
                case ContentDialogResult.None:
                    if ((bool)contentDialog.checkBox.IsChecked)
                    {
                        // Do not launch Marketplace review task.
                        // Do not ask again.
                    }
                    else
                    {
                        // Do not launch Marketplace review task.
                        // Ask again later.
                    }
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Displays a ContentDialog with a ListPicker as content.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event information.</param>
        private async void ContentDialogWithListPicker_Click(object sender, RoutedEventArgs e)
        {
            var contentDialog = new ContentDialogWithListPicker()
            {
                FullSizeDesired = (bool)FullScreenCheckBox.IsChecked
            };

            switch (await contentDialog.ShowAsync())
            {
                case ContentDialogResult.Primary:
                    // Do something.
                    break;
                case ContentDialogResult.Secondary:
                case ContentDialogResult.None:
                    // Do something.
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Displays a ContentDialog with a Pivot as content 
        /// by getting its ContentTemplate from a DataTemplate
        /// stored as a resource.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event information.</param>
        private async void ContentDialogWithPivot_Click(object sender, RoutedEventArgs e)
        {
            var contentDialog = new ContentDialogWithPivot()
            {
                FullSizeDesired = true, // Pivots should always be full-screen.
                Margin = new Thickness()
            };

            switch (await contentDialog.ShowAsync())
            {
                case ContentDialogResult.Primary:
                    // Do something.
                    break;
                case ContentDialogResult.Secondary:
                    // Do something.
                    break;
                case ContentDialogResult.None:
                    // Do something.
                    break;
                default:
                    break;
            }
        }
    }
}