using Microsoft.Phone.Controls.Primitives;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Microsoft.Phone.Controls
{
    /// <summary>
    /// Represents a custom picker.
    /// </summary>
    [TemplatePart(Name = TitlePresenterName, Type = typeof(TextBlock))]
    public sealed class PickerFlyoutPresenter : ContentControl
    {
        private const string TitlePresenterName = "TitlePresenter";

        private PickerFlyout _flyout;
        private OrientationHelper _orientationHelper;

        internal PickerFlyoutPresenter(PickerFlyout flyout)
        {
            _flyout = flyout;

            DefaultStyleKey = typeof(PickerFlyoutPresenter);

            _orientationHelper = new OrientationHelper(this);

            SetBinding(ContentProperty, new Binding("Content") { Source = _flyout });
        }

        private TextBlock TitlePresenter { get; set; }

        /// <summary>
        /// Builds the visual tree for the control when a new template is applied.
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            TitlePresenter = GetTemplateChild(TitlePresenterName) as TextBlock;

            UpdateTitlePresenter();

            _orientationHelper.OnApplyTemplate();
        }

        private void UpdateTitlePresenter()
        {
            if (TitlePresenter != null)
            {
                TitlePresenter.Text = PickerFlyoutBase.GetTitle(_flyout);
            }
        }
    }
}
