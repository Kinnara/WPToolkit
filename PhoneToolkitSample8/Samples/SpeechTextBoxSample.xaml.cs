// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Windows.Phone.Speech.Recognition;

namespace PhoneToolkitSample.Samples
{
    public partial class SpeechTextBoxSample : PhoneApplicationPage
    {
        public SpeechTextBoxSample()
        {
            InitializeComponent();

            ReverseTextBox.SpeechRecognized += ReverseTextBox_SpeechRecognized;
        }

        private void ReverseTextBox_SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            SpeechRecognitionResult recoResult = e.Result;

            ReverseTextBox.Text = ReverseString(recoResult.Text);

            e.Canceled = true;
        }

        private string ReverseString(string text)
        {
            char[] rawCharacters = text.ToArray();
            Array.Reverse(rawCharacters);

            return new string(rawCharacters);
        }
    }
}