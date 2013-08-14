// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using Microsoft.Phone.BackgroundTransfer;
using Microsoft.Phone.Controls.LocalizedResources;

namespace Microsoft.Phone.Controls
{
    /// <summary>
    /// This class is a wrapper around BackgroundTransferRequest.
    /// </summary>
    public class TransferMonitor : INotifyPropertyChanged
    {
        /// <summary>
        /// The background transfer request
        /// </summary>
        private readonly BackgroundTransferRequest _request;

        /// <summary>
        /// Event occurs when the transfer progress updates
        /// </summary>
        public event EventHandler<BackgroundTransferEventArgs> ProgressChanged;

        /// <summary>
        /// Event ocurs when the transfer has been added to the queue
        /// <remarks>This is not necessarily when the file begans downloading and uploading</remarks> 
        /// </summary>
        public event EventHandler<BackgroundTransferEventArgs> Started;

        /// <summary>
        /// Event occurs when the transfer has failed to transfer
        /// </summary>
        public event EventHandler<BackgroundTransferEventArgs> Failed;

        /// <summary>
        /// Event occurs when the file has finished transferring successfully
        /// </summary>
        public event EventHandler<BackgroundTransferEventArgs> Complete;

        /// <summary>
        /// Identifies which type of transfer this is
        /// </summary>
        public TransferType TransferType { get; private set; }

        /// <summary>
        /// Provides an error message when the transfer fails
        /// </summary>
        public string ErrorMessage { get; private set; }

        private string _name;
        /// <summary>
        /// The name of the transfer. If not specified in the constructor, it attempts to parse the filename from the request.
        /// </summary>
        public string Name
        {
            get
            {
                if (_name != null && _name.Trim().Length != 0)
                    return _name;

                string fullPath = TransferType == TransferType.Upload
                                   ? _request.UploadLocation.ToString()
                                   : _request.DownloadLocation.ToString();
                try
                {
                    return HttpUtility.UrlDecode(Path.GetFileNameWithoutExtension(fullPath));
                }
                catch (ArgumentException err)
                {
                    Debug.WriteLine("Could not parse the file name from the request URI.");
                    Debug.WriteLine(err);
                    return fullPath;
                }
            }
            set
            {
                _name = value;
                OnPropertyChanged("Name");
            }
        }

        /// <summary>
        /// How much of the transfer has completed.
        /// </summary>
        public double PercentComplete
        {
            get
            {
                if (TotalBytesToTransfer == 0)
                    return 0;
                return BytesTransferred / (double)TotalBytesToTransfer;
            }
        }

        private TransferRequestState _state;

        /// <summary>
        /// Returns the current state of the transfer
        /// </summary>
        public TransferRequestState State
        {
            get { return _state; }
            protected set
            {
                if (_state == value) return;

                // This checks to see if the transfer has successfully added to the transfer queue
                if (Started != null
                    && _state == TransferRequestState.Pending
                    && (value != TransferRequestState.Complete || value != TransferRequestState.Failed))
                    Started(this, new BackgroundTransferEventArgs(_request));

                if (Complete != null && value == TransferRequestState.Complete)
                    Complete(this, new BackgroundTransferEventArgs(_request));

                if (Failed != null && value == TransferRequestState.Failed)
                    Failed(this, new BackgroundTransferEventArgs(_request));

                _state = value;
                OnPropertyChanged("State");
            }
        }

        private string _statusText;

        /// <summary>
        /// This is the text that is displayed along with the progress bar
        /// </summary>
        public string StatusText
        {
            get { return _statusText; }
            protected set
            {
                _statusText = value;
                OnPropertyChanged("StatusText");
            }
        }

        private long _bytesTransferred;

        /// <summary>
        /// The number of bytes sent or received
        /// </summary>
        public long BytesTransferred
        {
            get { return _bytesTransferred; }
            protected set
            {
                _bytesTransferred = value;
                OnPropertyChanged("BytesTransferred");
                OnPropertyChanged("PercentComplete");
            }
        }

        private long _totalBytesToTransfer;

        /// <summary>
        /// The number of bytes that will be sent or received
        /// </summary>
        public long TotalBytesToTransfer
        {
            get { return _totalBytesToTransfer; }
            protected set
            {
                _totalBytesToTransfer = value;
                OnPropertyChanged("TotalBytesToTransfer");
                OnPropertyChanged("PercentComplete");
            }
        }

        private bool _isProgressIndeterminate;
        /// <summary>
        /// True if the total size of the transfer is not known
        /// </summary>
        public bool IsProgressIndeterminate
        {
            get { return _isProgressIndeterminate; }
            protected set
            {
                _isProgressIndeterminate = value;
                OnPropertyChanged("IsProgressIndeterminate");
            }
        }

        /// <summary>
        /// This helper wraps around an instance of BackgroundTransferRequest
        /// </summary>
        /// <param name="request">The background request to display</param>
        public TransferMonitor(BackgroundTransferRequest request)
            : this(request, "")
        {
        }

        /// <summary>
        /// This helper wraps around an instance of BackgroundTransferRequest
        /// </summary>
        /// <param name="request">The background request to display</param>
        /// <param name="name">The name to display</param>
        public TransferMonitor(BackgroundTransferRequest request, string name)
        {
            if (request == null)
                throw new ArgumentNullException("request");
            if (name == null)
                throw new ArgumentNullException("name");

            _request = request;
            _name = name;
            TransferType = (_request.DownloadLocation == null) ? TransferType.Upload : TransferType.Download;


            _request.TransferStatusChanged -= RequestStateChanged;
            _request.TransferStatusChanged += RequestStateChanged;

            _request.TransferProgressChanged -= RequestProgressChanged;
            _request.TransferProgressChanged += RequestProgressChanged;

            RequestStateChanged(_request, new BackgroundTransferEventArgs(_request)); // initializes the state of the helper
        }


        /// <summary>
        /// Requests the transfer be added to the BackgroundTransferService queue
        /// <remarks>The transfer may not begin immediately, but this attempts add the request to the queue</remarks>
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        public void RequestStart()
        {
            try
            {
                BackgroundTransferService.Add(_request);
            }
            catch (ArgumentNullException err)
            {
                Debug.WriteLine("The request argument cannot be null.");
                Debug.WriteLine(err);
                ErrorMessage = "Invalid request";
                State=TransferRequestState.Failed;
                StatusText = ControlResources.StatusFailed;
            }
            catch (InvalidOperationException err)
            {
                Debug.WriteLine("The request has already been submitted.");
                Debug.WriteLine(err);
                ErrorMessage = "The request has already been submitted.";
                State = TransferRequestState.Failed;
            }
            catch (SystemException err)
            {
                Debug.WriteLine("The maximum number of requests on the device has been reached.");
                Debug.WriteLine(err);
                ErrorMessage = "The maximum number of requests on the device has been reached.";
                State = TransferRequestState.Failed;
                StatusText = ControlResources.StatusFailed;
            }
        }

        /// <summary>
        /// Requests cancellation of the transfer
        /// </summary>
        public void RequestCancel()
        {
            try
            {
                BackgroundTransferService.Remove(_request);
            }
            catch (InvalidOperationException err)
            {
                Debug.WriteLine("The request has already been canceled.");
                Debug.WriteLine(err);
                ErrorMessage = "The request has already been canceled.";
                State = TransferRequestState.Failed;
                StatusText = ControlResources.StatusFailed;
            }
            catch (ArgumentNullException err)
            {
                Debug.WriteLine("The request argument cannot be null.");
                Debug.WriteLine(err);
                ErrorMessage = "Invalid request";
                State = TransferRequestState.Failed;
                StatusText = ControlResources.StatusFailed;
            }
        }

        /// <summary>
        /// Updates the state of the helper based on the state of the request
        /// </summary>
        /// <param name="sender">The request</param>
        /// <param name="args">The new state of the request</param>
        protected void RequestStateChanged(object sender, BackgroundTransferEventArgs args)
        {
            if (args == null) return;
            TransferStatus status = args.Request.TransferStatus;

            switch (status)
            {
                case TransferStatus.None:
                    State = TransferRequestState.Pending;
                    StatusText = ControlResources.StatusPending;
                    break;
                case TransferStatus.Transferring:
                    if (BytesTransferred <= 0)
                        State = TransferRequestState.Pending;
                    else
                        State = TransferType == TransferType.Upload ? TransferRequestState.Uploading : TransferRequestState.Downloading;

                    StatusText = TransferringStatusText();
                    break;
                case TransferStatus.Waiting:
                    State = TransferRequestState.Waiting;
                    StatusText = ControlResources.StatusWaiting;
                    break;
                case TransferStatus.WaitingForWiFi:
                    State = TransferRequestState.Waiting;
                    StatusText = ControlResources.StatusWaitingForWiFi;
                    break;
                case TransferStatus.WaitingForExternalPower:
                    State = TransferRequestState.Waiting;
                    StatusText = ControlResources.StatusWaitingForExternalPower;
                    break;
                case TransferStatus.WaitingForExternalPowerDueToBatterySaverMode:
                    State = TransferRequestState.Waiting;
                    StatusText = ControlResources.StatusWaitingForExternalPowerDueToBatterySaverMode;
                    break;
                case TransferStatus.WaitingForNonVoiceBlockingNetwork:
                    State = TransferRequestState.Waiting;
                    StatusText = ControlResources.StatusWaitingForNonVoiceBlockingNetwork;
                    break;
                case TransferStatus.Paused:
                    State = TransferRequestState.Paused;
                    StatusText = ControlResources.StatusPaused;
                    break;
                case TransferStatus.Completed:
                    if (_request.TransferError != null)
                    {
                        ErrorMessage = _request.TransferError.Message;
                        State = TransferRequestState.Failed;
                        StatusText = ErrorMessage.Contains("canceled") ? ControlResources.StatusCancelled : ControlResources.StatusFailed;
                    }
                    else
                    {
                        State = TransferRequestState.Complete;
                        StatusText = ControlResources.StatusComplete;
                    }
                    break;
                case TransferStatus.Unknown:
                    State = TransferRequestState.Unknown;
                    StatusText = ControlResources.StatusCancelled;
                    break;
            }

        }

        /// <summary>
        /// Updates the progress bar and status text when the progress updates
        /// </summary>
        /// <param name="sender">the request</param>
        /// <param name="e">the background transfer event</param>
        private void RequestProgressChanged(object sender, BackgroundTransferEventArgs e)
        {
            if (ProgressChanged != null) ProgressChanged(this, e);

            State = TransferType == TransferType.Upload
                        ? TransferRequestState.Uploading
                        : TransferRequestState.Downloading;
            StatusText = TransferringStatusText();
        }


        /// <summary>
        /// Calculates the progress of the transfer when it is actively downloading/uploading
        /// </summary>
        /// <returns>The status text string</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", MessageId = "System.String.Format(System.String,System.Object,System.Object)")]
        protected string TransferringStatusText()
        {
            IsProgressIndeterminate = TransferType == TransferType.Upload ? (_request.TotalBytesToSend <= 0) : (_request.TotalBytesToReceive <= 0);

            long total = TransferType == TransferType.Upload ? _request.TotalBytesToSend : _request.TotalBytesToReceive;
            long progress = TransferType == TransferType.Upload ? _request.BytesSent : _request.BytesReceived;

            if (progress <= 0)
                return ControlResources.StatusPending;

            string direction = TransferType == TransferType.Upload ? ControlResources.StatusUploading : ControlResources.StatusDownloading;
            string fraction;

            if (IsProgressIndeterminate)
                fraction = progress <= 0 ? "" : BytesToString(progress);
            else
                fraction = string.Format(ControlResources.PartOfWhole, BytesToString(progress),
                                         BytesToString(total));
            TotalBytesToTransfer = total;
            BytesTransferred = progress;

            return string.Format("{0} {1}", direction, fraction);
        }

        /// <summary>
        /// Converts bytes into a human-readable string
        /// </summary>
        /// <param name="bytes">The number of bytes</param>
        /// <returns>Human readable file size</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", MessageId = "System.String.Format(System.String,System.Object,System.Object)")]
        protected static string BytesToString(long bytes)
        {
            string[] suffix = { ControlResources.Byte, ControlResources.Kilobyte, ControlResources.Megabyte, ControlResources.Gigabyte };

            if (bytes <= 0)
                return "0 " + suffix[0];

            int place = Convert.ToInt32(Math.Min(3, Math.Floor(Math.Log(bytes, 1024)))); // maxes out at GB
            double num = Math.Round(bytes / Math.Pow(1024, place), 1);

            return string.Format("{0} {1}", num, suffix[place]);
        }

        /// <summary>
        /// Occurs when a property of the helper changes
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Triggers the PropertyChanged event
        /// </summary>
        /// <param name="propertyName">The name of the property that changed</param>
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}