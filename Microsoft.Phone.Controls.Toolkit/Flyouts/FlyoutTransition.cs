using System.Windows;

namespace Microsoft.Phone.Controls
{
    /// <summary>
    /// Has
    /// <see cref="T:Microsoft.Phone.Controls.TransitionElement"/>s
    /// for the designer experiences.
    /// </summary>
    public class FlyoutTransition : DependencyObject
    {
        /// <summary>
        /// The
        /// <see cref="T:System.Windows.DependencyProperty"/>
        /// for the in
        /// <see cref="T:Microsoft.Phone.Controls.TransitionElement"/>.
        /// </summary>
        public static readonly DependencyProperty InProperty =
            DependencyProperty.Register("Backward", typeof(TransitionElement), typeof(FlyoutTransition), null);

        /// <summary>
        /// The
        /// <see cref="T:System.Windows.DependencyProperty"/>
        /// for the out
        /// <see cref="T:Microsoft.Phone.Controls.TransitionElement"/>.
        /// </summary>
        public static readonly DependencyProperty OutProperty =
            DependencyProperty.Register("Forward", typeof(TransitionElement), typeof(FlyoutTransition), null);

        /// <summary>
        /// Gets or sets the in
        /// <see cref="T:Microsoft.Phone.Controls.TransitionElement"/>.
        /// </summary>
        public TransitionElement In
        {
            get
            {
                return (TransitionElement)GetValue(InProperty);
            }
            set
            {
                SetValue(InProperty, value);
            }
        }

        /// <summary>
        /// Gets or sets the out
        /// <see cref="T:Microsoft.Phone.Controls.TransitionElement"/>.
        /// </summary>
        public TransitionElement Out
        {
            get
            {
                return (TransitionElement)GetValue(OutProperty);
            }
            set
            {
                SetValue(OutProperty, value);
            }
        }
    }
}