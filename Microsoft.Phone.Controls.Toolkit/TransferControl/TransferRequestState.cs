// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

namespace Microsoft.Phone.Controls
{
    /// <summary>
    /// Defines all possible states for the transfer request
    /// </summary>
    public enum TransferRequestState
    {
        /// <summary>
        /// The request is waiting  to begin
        /// </summary>
        Pending,
        /// <summary>
        /// The request is downloading a file
        /// </summary>
        Downloading,
        /// <summary>
        /// The request is uploading a file
        /// </summary>
        Uploading,
        /// <summary>
        /// The request is paused
        /// </summary>
        Paused,
        /// <summary>
        /// The request is waiting for the system
        /// </summary>
        Waiting,
        /// <summary>
        /// The request is complete
        /// </summary>
        Complete,
        /// <summary>
        /// The request  has failed
        /// </summary>
        Failed,
        /// <summary>
        /// The request is in an unknown state
        /// </summary>
        Unknown
    }
}