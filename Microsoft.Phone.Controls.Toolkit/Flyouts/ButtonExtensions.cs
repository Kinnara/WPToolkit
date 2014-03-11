using Microsoft.Phone.Controls.Primitives;
using System.Windows;
using System.Windows.Controls;

namespace Microsoft.Phone.Controls
{
    /// <summary>
    /// Represents a class that can extend the capabilities of the <see cref="Button"/> class.
    /// </summary>
    public static class ButtonExtensions
    {
        #region Flyout

        /// <summary>
        /// Gets the flyout associated with the specified button.
        /// </summary>
        /// <param name="element">The button for which to get the associated flyout.</param>
        /// <returns>The flyout attached to the specified button.</returns>
        public static FlyoutBase GetFlyout(Button element)
        {
            return (FlyoutBase)element.GetValue(FlyoutProperty);
        }

        /// <summary>
        /// Associates the specified flyout with the specified button.
        /// </summary>
        /// <param name="element">The button to associate the flyout with.</param>
        /// <param name="value">The flyout to associate with the specified button.</param>
        public static void SetFlyout(Button element, FlyoutBase value)
        {
            element.SetValue(FlyoutProperty, value);
        }

        /// <summary>
        /// Identifies the ButtonExtensions.Flyout XAML attached property.
        /// </summary>
        public static readonly DependencyProperty FlyoutProperty = DependencyProperty.RegisterAttached(
            "Flyout",
            typeof(FlyoutBase),
            typeof(ButtonExtensions),
            new PropertyMetadata(OnFlyoutChanged));

        private static void OnFlyoutChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var element = (Button)d;
            var oldFlyout = (FlyoutBase)e.OldValue;
            var newFlyout = (FlyoutBase)e.NewValue;

            if (oldFlyout != null)
            {
                element.Click -= OnButtonClick;
            }

            if (newFlyout != null)
            {
                element.Click += OnButtonClick;
            }
        }

        #endregion

        private static void OnButtonClick(object sender, RoutedEventArgs e)
        {
            GetFlyout(((Button)sender)).Show();
        }
    }
}
