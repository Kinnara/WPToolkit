using Microsoft.Phone.Controls.LocalizedResources;
using Microsoft.Phone.Shell;
using System;
using System.Windows;

namespace Microsoft.Phone.Controls.Primitives
{
    /// <summary>
    /// Represents a base class for picker controls.
    /// </summary>
    public abstract class PickerFlyoutBase : FlyoutBase
    {
        private static readonly Uri DoneButtonIconUri = new Uri("/Toolkit.Content/ApplicationBar.Check.png", UriKind.Relative);
        private static readonly Uri CancelButtonIconUri = new Uri("/Toolkit.Content/ApplicationBar.Cancel.png", UriKind.Relative);

        /// <summary>
        /// Provides base-class initialization behavior for classes that are derived from the PickerFlyoutBase class.
        /// </summary>
        protected PickerFlyoutBase()
        {
        }

        #region Title

        /// <summary>
        /// Gets the title displayed on the picker control.
        /// </summary>
        /// <param name="element">The dependency object for which to get the title.</param>
        /// <returns>The title displayed on the picker control.</returns>
        public static string GetTitle(DependencyObject element)
        {
            return (string)element.GetValue(TitleProperty);
        }

        /// <summary>
        /// Sets the title displayed on a picker control.
        /// </summary>
        /// <param name="element">The dependency object for which to set the title.</param>
        /// <param name="value">The title you want to display.</param>
        public static void SetTitle(DependencyObject element, string value)
        {
            element.SetValue(TitleProperty, value);
        }

        /// <summary>
        /// Identifies the attached title property.
        /// </summary>
        /// 
        /// <returns>
        /// The identifier for the attached title property.
        /// </returns>
        public static readonly DependencyProperty TitleProperty = DependencyProperty.RegisterAttached(
            "Title",
            typeof(string),
            typeof(PickerFlyoutBase),
            new PropertyMetadata(string.Empty));

        #endregion

        protected virtual void OnConfirmed()
        {
            InternalHide(false);
        }

        protected abstract bool ShouldShowConfirmationButtons();

        internal override ApplicationBar CreateApplicationBar()
        {
            if (ShouldShowConfirmationButtons())
            {
                var doneButton = new ApplicationBarIconButton(DoneButtonIconUri)
                {
                    Text = ControlResources.DateTimePickerDoneText
                };

                var cancelButton = new ApplicationBarIconButton(CancelButtonIconUri)
                {
                    Text = ControlResources.DateTimePickerCancelText
                };

                EventHandler onButtonClick = null;
                onButtonClick = delegate
                {
                    doneButton.Click -= OnDoneButtonClick;
                    doneButton.Click -= onButtonClick;
                    cancelButton.Click -= OnCancelButtonClick;
                    cancelButton.Click -= onButtonClick;
                };

                doneButton.Click += OnDoneButtonClick;
                doneButton.Click += onButtonClick;
                cancelButton.Click += OnCancelButtonClick;
                cancelButton.Click += onButtonClick;

                return new ApplicationBar
                {
                    Buttons = { doneButton, cancelButton }
                };
            }

            return base.CreateApplicationBar();
        }

        private void OnDoneButtonClick(object sender, EventArgs e)
        {
            OnConfirmed();
        }

        private void OnCancelButtonClick(object sender, EventArgs e)
        {
            // Close without committing a value
            InternalHide(true);
        }
    }
}
