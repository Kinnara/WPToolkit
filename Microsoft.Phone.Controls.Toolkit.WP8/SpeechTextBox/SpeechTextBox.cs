// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Windows.Phone.Speech.Recognition;

namespace Microsoft.Phone.Controls
{
    /// <summary>
    /// An extended PhoneTextBox that behaves as a speech-enabled textbox, with an optional event handler to override default behavior.
    /// </summary>
    public class SpeechTextBox : PhoneTextBox
    {

        private bool _focused = false; //textbox currently focused or not
        private bool _useSelectedTextReplacement = false; //determines if textbox was focused when mic button clicked
        private int _selectionStart;
        private int _selectionEnd;

        /// <summary>
        /// Whether there is a speech recogniztion in progress.
        /// </summary>
        private bool _handlingSpeech;

        /// <summary>
        /// Creates a new instance of the SpeechTextBox class.
        /// </summary>
        public SpeechTextBox()
        {
            this.ActionIcon = new BitmapImage(new Uri("/Microsoft.Phone.Controls.Toolkit;Component/images/microphone.png", UriKind.Relative));
            this.ActionIconTapped += Mic_ActionIconTapped;
            this.GotFocus += PhraseBoxFocused;
            this.LostFocus += PhraseBoxUnFocused;
        }

        /// <summary>
        /// Applies the template and adds a ManipulationStarted handler to the ActionIconBorder, if it exists.
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (ActionIconBorder != null)
            {
                ActionIconBorder.ManipulationStarted += MicButtonManipulationStarted;
            }
        }

        /// <summary>
        /// Speech Recognized event to optionally override default speech recognition processing.
        /// </summary>
        public event EventHandler<SpeechRecognizedEventArgs> SpeechRecognized;

        /// <summary>
        /// Handler for when the ActionIcon is tapped.
        /// </summary>
        /// <param name="sender">The sender object</param>
        /// <param name="e">Event arguments.</param>
        private void Mic_ActionIconTapped(object sender, EventArgs e)
        {
            HandleSpeech();
        }

        /// <summary>
        /// Method to instantiate recognizer with appropriate grammar and perform recognition.
        /// </summary>
        private async void HandleSpeech()
        {
            if (_handlingSpeech)
            {
                return;
            }

            _handlingSpeech = true;
            try
            {
                SpeechRecognizerUI recognizer = new SpeechRecognizerUI();
                SpeechRecognitionUIResult result = null;

                if (this.InputScope != null && (this.InputScope.Names[0] as InputScopeName).NameValue.Equals(InputScopeNameValue.Search))
                {
                    recognizer.Recognizer.Grammars.AddGrammarFromPredefinedType("WebSearchGrammar", SpeechPredefinedGrammar.WebSearch);
                }

                try
                {
                    result = await recognizer.RecognizeWithUIAsync();
                }
                catch (OperationCanceledException)
                {
                    return;
                }
                catch (Exception ex)
                {
                    if ((uint)ex.HResult == 0x80045508)
                    {
                        // This can occur when speech recognition is interupted by navigation away from
                        // the app. We'll just swallow the exception to work around it.
                        return;
                    }

                    MessageBox.Show("An error occured. \n" + ex.Message);
                    return;
                }

                // The SpeechRecognizerUI component will handle cases where the speech was not recognized and prompt 
                // user to retry. This check is just to make sure that the speech recognition request was not 
                // canceled by the back button, navigation, etc.
                if (result.ResultStatus == SpeechRecognitionUIStatus.Succeeded)
                {
                    // Raise SpeechRecognized event
                    var handler = SpeechRecognized;
                    SpeechRecognizedEventArgs eventArgs = new SpeechRecognizedEventArgs(result.RecognitionResult);

                    if (handler != null)
                    {
                        handler(this, eventArgs);

                        if (eventArgs.Canceled)
                        {
                            return;
                        }
                    }

                    // Update display
                    string originalText = this.Text;

                    if (_useSelectedTextReplacement)
                    {
                        string newText = originalText.Substring(0, _selectionStart) + result.RecognitionResult.Text + originalText.Substring(_selectionEnd + 1);
                        this.Text = newText;
                        this.Select(_selectionStart, result.RecognitionResult.Text.Length);
                    }
                    else
                    {
                        this.Text = result.RecognitionResult.Text;
                        this.Focus();
                        this.Select(_selectionStart, result.RecognitionResult.Text.Length);
                    }
                }
            }
            finally
            {
                _handlingSpeech = false;
            }
        }

        /// <summary>
        /// Handler to record a snapshot of important state when the microphone button is clicked.
        /// </summary>
        /// <param name="sender">The sender object</param>
        /// <param name="e">Event arguments.</param>
        private void MicButtonManipulationStarted(object sender, EventArgs e)
        {
            _useSelectedTextReplacement = _focused;

            _selectionStart = this.SelectionStart;

            _selectionEnd = _selectionStart + this.SelectionLength - 1;

        }

        /// <summary>
        /// Handler to set focused to true
        /// </summary>
        /// <param name="sender">The sender object</param>
        /// <param name="e">Event arguments.</param>
        private void PhraseBoxFocused(object sender, RoutedEventArgs e)
        {
            _focused = true;
        }

        /// <summary>
        /// Handler to set focused to false
        /// </summary>
        /// <param name="sender">The sender object</param>
        /// <param name="e">Event arguments.</param>
        private void PhraseBoxUnFocused(object sender, RoutedEventArgs e)
        {
            _focused = false;
        }

    }
}
