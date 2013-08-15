// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System;
using System.Diagnostics;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Windows;
using Microsoft.Phone.BackgroundTransfer;
using Microsoft.Phone.Controls;
using GestureEventArgs = System.Windows.Input.GestureEventArgs;

namespace PhoneToolkitSample.Samples
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable")]
    public partial class BackgroundTransferControl : PhoneApplicationPage
    {
        private BackgroundTransferRequest _request;
        private TransferMonitor _monitor;

        public BackgroundTransferControl()
        {
            InitializeComponent();

            _request = BackgroundTransferService.Requests.FirstOrDefault(s => s.Tag == "Channel9Download");

            if (_request != null)
            {
                _monitor = new TransferMonitor(_request);
                _monitor.Failed += TransferFailed;
                Control1.DataContext = _monitor;
            }
        }

        private void TransferFailed(object sender, BackgroundTransferEventArgs e)
        {
            if (_monitor.ErrorMessage.Contains("canceled")) return; //The failed event is raised when a user cancels the transfer, but we don't want to show the retry dialog here

            var result = MessageBox.Show("This download failed. Would you like to try again?", "Retry download?",
                                         MessageBoxButton.OKCancel);

            if (result == MessageBoxResult.OK)
                RestartDownload();
        }

        private void RestartDownload()
        {

            try
            {
                BackgroundTransferService.Remove(_request);
            }
            catch (Exception)
            {
                //No need to worry. These errors can be surpressed. We are just making sure the download has been completed cancelled.
            }

            _request = null;
            _monitor = null;

            BeginDownload();
        }

        private void BeginDownload()
        {
            using (var storage = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (storage.FileExists("/shared/transfers/channel9.mp3"))
                    storage.DeleteFile("/shared/transfers/channel9.mp3");
            }

            _request = new BackgroundTransferRequest(
                        new Uri("http://media.ch9.ms/ch9/7e13/ce6ea97c-e233-4e7c-a74d-ee1c81e37e13/WP8JumpStart04.mp3",
                                UriKind.Absolute),
                        new Uri("/shared/transfers/channel9.mp3", UriKind.Relative)) { Tag = "Channel9Download", TransferPreferences = TransferPreferences.AllowBattery };
            _monitor = new TransferMonitor(_request);
            Control1.DataContext = _monitor;

            _monitor.RequestStart(); //This adds request to the BackgroundTransferService queue.
        }

        private void Download(object sender, RoutedEventArgs e)
        {
            if (_request == null)
            {
                BeginDownload();
                return;
            }
            else if (_request.TransferStatus == TransferStatus.Completed)
            {
                var status = (_request.TransferError == null) ? "has already completed" : "failed to complete";
                var result = MessageBox.Show("The download " + status + ". Would you like to try again?",
                                             "Download again?", MessageBoxButton.OKCancel);
                if (result == MessageBoxResult.OK)
                    RestartDownload();
                return;
            }
            MessageBox.Show("This download has already been started.", "Download error", MessageBoxButton.OK);
        }

        private void Control1_OnTap(object sender, GestureEventArgs e)
        {
            var control = sender as TransferControl;
            if (control == null) return;

            var monitor = control.Monitor;

            if (monitor == null || monitor.State != TransferRequestState.Failed) return;

            monitor.RequestCancel();

            MessageBox.Show(monitor.ErrorMessage, "Download error", MessageBoxButton.OK);
        }

        private void CancelDownload(object sender, RoutedEventArgs e)
        {
            var helper = Control1.Monitor;
            if (helper != null) helper.RequestCancel();
        }

    }
}