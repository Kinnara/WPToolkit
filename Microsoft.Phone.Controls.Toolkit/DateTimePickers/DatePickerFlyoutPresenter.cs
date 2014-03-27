using Microsoft.Phone.Controls.Primitives;
using System;
using System.Windows;
using System.Windows.Controls;

namespace Microsoft.Phone.Controls
{
    /// <summary>
    /// Represents the visual container for the DatePickerFlyout.
    /// </summary>
    [TemplatePart(Name = TitlePresenterName, Type = typeof(TextBlock))]
    [TemplatePart(Name = FirstPickerHostName, Type = typeof(Border))]
    [TemplatePart(Name = SecondPickerHostName, Type = typeof(Border))]
    [TemplatePart(Name = ThirdPickerHostName, Type = typeof(Border))]
    public sealed class DatePickerFlyoutPresenter : Control
    {
        private const string TitlePresenterName = "TitlePresenter";
        private const string FirstPickerHostName = "FirstPickerHost";
        private const string SecondPickerHostName = "SecondPickerHost";
        private const string ThirdPickerHostName = "ThirdPickerHost";

        private DatePickerFlyout _flyout;
        private DatePickerFlyoutPresenterHelper _helper;
        private OrientationHelper _orientationHelper;

        internal DatePickerFlyoutPresenter(DatePickerFlyout flyout)
        {
            _flyout = flyout;

            DefaultStyleKey = typeof(DatePickerFlyoutPresenter);

            _helper = new DatePickerFlyoutPresenterHelper(flyout, this);
            _orientationHelper = new OrientationHelper(this);
        }

        internal DateTimeOffset Date
        {
            get { return _helper.Value.ToLocalTime(); }
            set { _helper.Value = value.LocalDateTime; }
        }

        private TextBlock TitlePresenter { get; set; }

        private Border FirstPickerHost { get; set; }

        private Border SecondPickerHost { get; set; }

        private Border ThirdPickerHost { get; set; }

        /// <summary>
        /// Builds the visual tree for the control when a new template is applied.
        /// </summary>
        public override void OnApplyTemplate()
        {
            if (FirstPickerHost != null)
            {
                FirstPickerHost.Child = null;
            }

            if (SecondPickerHost != null)
            {
                SecondPickerHost.Child = null;
            }

            if (ThirdPickerHost != null)
            {
                ThirdPickerHost.Child = null;
            }

            base.OnApplyTemplate();

            TitlePresenter = GetTemplateChild(TitlePresenterName) as TextBlock;
            FirstPickerHost = GetTemplateChild(FirstPickerHostName) as Border;
            SecondPickerHost = GetTemplateChild(SecondPickerHostName) as Border;
            ThirdPickerHost = GetTemplateChild(ThirdPickerHostName) as Border;

            UpdateTitlePresenter();

            if (FirstPickerHost != null)
            {
                FirstPickerHost.Child = _helper.FirstPicker;
            }

            if (SecondPickerHost != null)
            {
                SecondPickerHost.Child = _helper.SecondPicker;
            }

            if (ThirdPickerHost != null)
            {
                ThirdPickerHost.Child = _helper.ThirdPicker;
            }

            _helper.OnApplyTemplate();

            _orientationHelper.OnApplyTemplate();
        }

        internal void Commit()
        {
            _helper.Commit();
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
