using System.Windows.Controls;

namespace Microsoft.Phone.Controls
{
    /// <summary>
    /// Display the content of a Flyout.
    /// </summary>
    public class FlyoutPresenter : ContentControl
    {
        private OrientationHelper _orientationHelper;

        /// <summary>
        /// Initializes a new instance of the FlyoutPresenter class.
        /// </summary>
        public FlyoutPresenter()
        {
            DefaultStyleKey = typeof(FlyoutPresenter);

            _orientationHelper = new OrientationHelper(this);
        }

        /// <summary>
        /// Builds the visual tree for the control when a new template is applied.
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _orientationHelper.OnApplyTemplate();
        }
    }
}
