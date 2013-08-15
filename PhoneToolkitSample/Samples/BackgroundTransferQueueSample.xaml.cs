// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Microsoft.Phone.BackgroundTransfer;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

namespace PhoneToolkitSample.Samples
{
    public partial class BackgroundTransferQueue : PhoneApplicationPage
    {
        private ApplicationBarIconButton _selectButton = new ApplicationBarIconButton
            {
                IconUri = new Uri("/Toolkit.Content/ApplicationBar.Select.png", UriKind.Relative),
                Text = "select"
            };

        private ApplicationBarIconButton _cancelButton = new ApplicationBarIconButton
            {
                IconUri = new Uri("/Toolkit.Content/ApplicationBar.Cancel.png", UriKind.Relative),
                Text = "cancel",
            };
        private ApplicationBarIconButton _addButton = new ApplicationBarIconButton
            {
                IconUri = new Uri("/Toolkit.Content/ApplicationBar.Add.png", UriKind.Relative),
                Text = "add",
            };

        private ObservableCollection<TransferMonitor> _list = new ObservableCollection<TransferMonitor>();

        public BackgroundTransferQueue()
        {
            InitializeComponent();

            // Load existing background requests into the collection

            foreach (BackgroundTransferRequest request in BackgroundTransferService.Requests)
            {
                var monitor = new TransferMonitor(request);
                SubscribeMonitor(monitor);
                _list.Add(monitor);
            }

            TransfersList.DataContext = _list;


            // Subscribe buttons to actions
            _selectButton.Click += OnSelectButtonClick;
            _addButton.Click += OnAddButtonClick;
            _cancelButton.Click += OnCancelButtonClick;
            _list.CollectionChanged += delegate { _selectButton.IsEnabled = (_list.Count > 0); };

            ReloadApplicationBar();
        }

        public void SubscribeMonitor(TransferMonitor monitor)
        {
            UnsubscribeMonitor(monitor); // prevent duplicate subscription
            monitor.Failed += TransferCanceled;
        }

        private void UnsubscribeMonitor(TransferMonitor monitor)
        {
            monitor.Failed -= TransferCanceled;
        }

        /// <summary>
        /// Automatically removes a monitor from the queue when it is canceled.
        /// </summary>
        private void TransferCanceled(object sender, BackgroundTransferEventArgs transferEventArgs)
        {
            if (transferEventArgs == null)
                throw new ArgumentNullException("transferEventArgs");

            var monitor = sender as TransferMonitor;
            if (monitor == null ||  monitor.ErrorMessage.Contains("canceled")==false) return;

            UnsubscribeMonitor(monitor);
            if (_list.Contains(monitor))
                _list.Remove(monitor);
        }

        /// <summary>
        /// This clears all completed transfers from the transfer queue
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ClearCompletedItems(object sender, EventArgs e)
        {
            var i = 0;
            // Perhaps a goofy looking iteration, but a foreach will not work because items are being removed from _list
            while (i < _list.Count)
            {
                if (_list.ElementAt(i).State == TransferRequestState.Complete)
                {
                    TransferMonitor monitor = _list.ElementAt(i);
                    monitor.RequestCancel(); //ensures that the request is removed from the queue, even if complete
                    UnsubscribeMonitor(monitor);
                    _list.RemoveAt(i);
                }
                else
                    i++;
            }

        }

        protected override void OnBackKeyPress(CancelEventArgs e)
        {
            if (TransfersList.IsSelectionEnabled)
            {
                TransfersList.IsSelectionEnabled = false;
                e.Cancel = true;
            }
            base.OnBackKeyPress(e);
        }

        private void OnCancelButtonClick(object sender, EventArgs e)
        {
            while (TransfersList.SelectedItems.Count > 0)
            {
                TransferMonitor monitor = (TransferMonitor)TransfersList.SelectedItems[0];
                if (monitor != null)
                    monitor.RequestCancel();
                _list.Remove(monitor);
            }
        }

        private void OnSelectButtonClick(object sender, EventArgs e)
        {
            TransfersList.IsSelectionEnabled = true;
        }

        /// <summary>
        /// Called when the number of items in the multi-select list changes
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TransfersList_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateCancelButton();
        }

        /// <summary>
        /// Updates the enabled state of the cancel button in the application bar
        /// </summary>
        private void UpdateCancelButton()
        {
            if (TransfersList.IsSelectionEnabled)
            {
                bool hasSelection = (TransfersList.SelectedItems != null && TransfersList.SelectedItems.Count > 0);
                _cancelButton.IsEnabled = hasSelection;
            }
        }

        private void TransfersList_OnIsSelectionEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            ReloadApplicationBar();
        }

        /// <summary>
        /// This clears the application bar and loads it with the correct buttons (depends on the selection mode of the multi-select list)
        /// </summary>
        private void ReloadApplicationBar()
        {

            ClearApplicationBar();
            if (TransfersList.IsSelectionEnabled)
            {
                ApplicationBar.Buttons.Add(_cancelButton);
                UpdateCancelButton();
            }
            else
            {
                _selectButton.IsEnabled = (_list.Count > 0);
                ApplicationBar.Buttons.Add(_selectButton);
                ApplicationBar.Buttons.Add(_addButton);
            }
        }

        /// <summary>
        /// Resets the application bar by removing all items
        /// </summary>
        void ClearApplicationBar()
        {
            while (ApplicationBar.Buttons.Count > 0)
            {
                ApplicationBar.Buttons.RemoveAt(0);
            }

        }
        /// <summary>
        /// This function is for demo purposes. It adds three files to the background download queue and displays them in the multi-select list.
        /// </summary>
        private void OnAddButtonClick(object sender, EventArgs e)
        {
            Dictionary<string, Uri> urlPresets = new Dictionary<string, Uri>
                {
                    {"21 MB File",new Uri("http://media.ch9.ms/ch9/ecbc/cfcb0ad7-fbdd-47b0-aabf-4da5e3e0ecbc/WP8JumpStart06.mp3",UriKind.Absolute)},
                    {"34 MB File",new Uri("http://media.ch9.ms/ch9/7e13/ce6ea97c-e233-4e7c-a74d-ee1c81e37e13/WP8JumpStart04.mp3",UriKind.Absolute)},
                    {"92 MB File",new Uri("http://media.ch9.ms/ch9/7e13/ce6ea97c-e233-4e7c-a74d-ee1c81e37e13/WP8JumpStart04.wmv",UriKind.Absolute)},
                };

            foreach (var preset in urlPresets)
            {

                Uri saveLocation = new Uri("/shared/transfers/" + preset.Key, UriKind.Relative);
                BackgroundTransferRequest request = new BackgroundTransferRequest(preset.Value, saveLocation)
                {
                    TransferPreferences = TransferPreferences.AllowBattery   // Note: this will not use cellular data to download
                };
                TransferMonitor monitor = new TransferMonitor(request, preset.Key);
                try
                {
                    BackgroundTransferService.Add(request);
                }
                catch (Exception err) // An exception is thrown if this transfer is already requested.
                {
                    Debug.WriteLine(err);
                    continue;
                }
                monitor.Failed += TransferCanceled;
                _list.Add(monitor);
            }
        }
    }


    /// <summary>
    /// This converts the number zero to Visibility.Collapsed
    /// </summary>
    public class EmptyTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (int)value == 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

}