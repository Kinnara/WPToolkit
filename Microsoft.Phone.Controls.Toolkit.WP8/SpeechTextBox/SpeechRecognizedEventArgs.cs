// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Phone.Speech.Recognition;

namespace Microsoft.Phone.Controls
{
    /// <summary>
    /// Event arguments for the SpeechTextBox SpeechRecognized event.
    /// </summary>
    public sealed class SpeechRecognizedEventArgs: EventArgs
    {

        /// <summary>
        /// Creates a new instance of the SpeechRecognizedEventArgs class.
        /// </summary>
        public SpeechRecognizedEventArgs(SpeechRecognitionResult result)
        {
            Result = result;
            Canceled = false;
        }

        /// <summary>
        /// Gets the speech recognition results for processing.
        /// </summary>
        public SpeechRecognitionResult Result { get; private set; }

        /// <summary>
        /// Gets or sets the canceled property. 
        /// </summary>
        public bool Canceled { get; set; }
    }
}
