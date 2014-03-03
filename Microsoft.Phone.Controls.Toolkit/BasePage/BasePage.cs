using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace Microsoft.Phone.Controls
{
    /// <summary>
    /// Typical implementation of PhoneApplicationPage that provides the following important conveniences:
    /// <list type="bullet">
    /// <item>
    /// <description>Orientation to visual state mapping</description>
    /// </item>
    /// </list>
    /// </summary>
    public class BasePage : PhoneApplicationPage
    {
        private List<Control> _layoutAwareControls;

        /// <summary>
        /// Initializes a new instance of the <see cref="BasePage"/> class.
        /// </summary>
        public BasePage()
        {
        }

        /// <summary>
        /// Called when a page becomes the active page in a frame.
        /// </summary>
        /// <param name="e">An object that contains the event data.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (_layoutAwareControls == null)
            {
                StartLayoutUpdates(this);
            }
        }

        #region Visual state switching

        /// <summary>
        /// Start layout updates for the specified control.
        /// </summary>
        /// <param name="control"></param>
        public void StartLayoutUpdates(Control control)
        {
            if (control == null) return;
            if (_layoutAwareControls == null)
            {
                // Start listening to orientation changes when there are controls interested in updates
                OrientationChanged += OnOrientationChanged;
                _layoutAwareControls = new List<Control>();
            }
            _layoutAwareControls.Add(control);

            // Set the initial visual state of the control
            VisualStateManager.GoToState(control, DetermineVisualState(Orientation), false);
        }

        private void OnOrientationChanged(object sender, OrientationChangedEventArgs e)
        {
            InvalidateVisualState();
        }

        /// <summary>
        /// Stop layout updates for the specified control.
        /// </summary>
        /// <param name="control"></param>
        public void StopLayoutUpdates(Control control)
        {
            if (control == null || _layoutAwareControls == null) return;
            _layoutAwareControls.Remove(control);
            if (_layoutAwareControls.Count == 0)
            {
                // Stop listening to orientation changes when no controls are interested in updates
                _layoutAwareControls = null;
                OrientationChanged -= OnOrientationChanged;
            }
        }

        /// <summary>
        /// Translates <see cref="PageOrientation"/> values into strings for visual state
        /// management within the page.  The default implementation uses the names of enum values.
        /// Subclasses may override this method to control the mapping scheme used.
        /// </summary>
        /// <param name="orientation">Orientation for which a visual state is desired.</param>
        /// <returns>Visual state name used to drive the
        /// <see cref="VisualStateManager"/></returns>
        /// <seealso cref="InvalidateVisualState"/>
        protected virtual string DetermineVisualState(PageOrientation orientation)
        {
            return orientation.ToString();
        }

        /// <summary>
        /// Updates all controls that are listening for visual state changes with the correct
        /// visual state.
        /// </summary>
        /// <remarks>
        /// Typically used in conjunction with overriding <see cref="DetermineVisualState"/> to
        /// signal that a different value may be returned even though the orientation has not
        /// changed.
        /// </remarks>
        public void InvalidateVisualState()
        {
            if (_layoutAwareControls != null)
            {
                string visualState = DetermineVisualState(Orientation);
                foreach (var layoutAwareControl in _layoutAwareControls)
                {
                    VisualStateManager.GoToState(layoutAwareControl, visualState, false);
                }
            }
        }

        #endregion
    }
}
