using Microsoft.Phone.Controls.Primitives;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

namespace Microsoft.Phone.Controls
{
    /// <summary>
    /// Represents a custom picker control.
    /// </summary>
    [ContentProperty("Content")]
    public sealed class PickerFlyout : PickerFlyoutBase
    {
        private PickerFlyoutHelper<bool> _helper;

        /// <summary>
        /// Initializes a new instance of the PickerFlyout class.
        /// </summary>
        public PickerFlyout()
        {
            _helper = new PickerFlyoutHelper<bool>(this);
        }

        #region public bool ConfirmationButtonsVisible

        public bool ConfirmationButtonsVisible
        {
            get { return (bool)GetValue(ConfirmationButtonsVisibleProperty); }
            set { SetValue(ConfirmationButtonsVisibleProperty, value); }
        }

        public static readonly DependencyProperty ConfirmationButtonsVisibleProperty = DependencyProperty.Register(
            "ConfirmationButtonsVisible",
            typeof(bool),
            typeof(PickerFlyout),
            null);

        #endregion

        #region public UIElement Content

        /// <summary>
        /// Gets or sets the content that is contained within the picker.
        /// </summary>
        /// 
        /// <returns>
        /// The content of the user control.
        /// </returns>
        public UIElement Content
        {
            get { return (UIElement)GetValue(ContentProperty); }
            set { SetValue(ContentProperty, value); }
        }

        /// <summary>
        /// Gets the identifier for the Content dependency property.
        /// </summary>
        /// 
        /// <returns>
        /// The identifier for the Content dependency property.
        /// </returns>
        public static readonly DependencyProperty ContentProperty = DependencyProperty.Register(
            "Content",
            typeof(UIElement),
            typeof(PickerFlyout),
            null);

        #endregion

        public event EventHandler Confirmed;

        /// <summary>
        /// Begins an asynchronous operation to show the flyout.
        /// </summary>
        /// <returns>An asynchronous operation.</returns>
        public Task<bool> ShowAsync()
        {
            return _helper.ShowAsync();
        }

        protected override bool ShouldShowConfirmationButtons()
        {
            return ConfirmationButtonsVisible;
        }

        /// <summary>
        /// Initializes a control to show the flyout content.
        /// </summary>
        /// <returns>
        /// The control that displays the content of the flyout.
        /// </returns>
        protected override Control CreatePresenter()
        {
            return new PickerFlyoutPresenter(this);
        }

        protected override void OnConfirmed()
        {
            RaiseConfirmed();

            base.OnConfirmed();
        }

        internal override void RequestHide()
        {
            if (_helper.IsAsyncOperationInProgress)
            {
                return;
            }

            base.RequestHide();
        }

        internal override void OnClosed()
        {
            _helper.CompleteShowAsync(false);

            base.OnClosed();
        }

        private void RaiseConfirmed()
        {
            _helper.CompleteShowAsync(true);
            SafeRaise.Raise(Confirmed, this);
        }
    }
}
