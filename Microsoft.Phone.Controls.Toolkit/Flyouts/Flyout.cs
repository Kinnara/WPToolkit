using Microsoft.Phone.Controls.Primitives;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Markup;

namespace Microsoft.Phone.Controls
{
    /// <summary>
    /// Represents a control that displays lightweight UI that is either information, or requires user interaction.
    /// Unlike a dialog, a Flyout can be light dismissed by clicking or tapping off of it.
    /// </summary>
    [ContentProperty("Content")]
    public class Flyout : FlyoutBase
    {
        private FlyoutPresenter _presenter;

        #region public UIElement Content

        /// <summary>
        /// Gets or sets the content of the Flyout.
        /// </summary>
        /// <returns>
        /// The content of the Flyout.
        /// </returns>
        public UIElement Content
        {
            get { return (UIElement)GetValue(ContentProperty); }
            set { SetValue(ContentProperty, value); }
        }

        /// <summary>
        /// Gets or sets an instance Style applied to the Flyout content.
        /// </summary>
        /// <returns>
        /// The applied Style for the Flyout content, if present; otherwise, null. The default is null.
        /// </returns>
        public static readonly DependencyProperty ContentProperty = DependencyProperty.Register(
            "Content",
            typeof(UIElement),
            typeof(Flyout),
            null);

        #endregion

        #region public Style FlyoutPresenterStyle

        /// <summary>
        /// Gets or sets the content of the Flyout.
        /// </summary>
        /// <returns>
        /// The content of the Flyout.
        /// </returns>
        public Style FlyoutPresenterStyle
        {
            get { return (Style)GetValue(FlyoutPresenterStyleProperty); }
            set { SetValue(FlyoutPresenterStyleProperty, value); }
        }

        /// <summary>
        /// Gets the identifier for the FlyoutPresenterStyle dependency property.
        /// </summary>
        /// <returns>
        /// The identifier for the FlyoutPresenterStyle dependency property.
        /// </returns>
        public static readonly DependencyProperty FlyoutPresenterStyleProperty = DependencyProperty.Register(
            "FlyoutPresenterStyle",
            typeof(Style),
            typeof(Flyout),
            new PropertyMetadata((d, e) => ((Flyout)d).OnFlyoutPresenterStyleChanged()));

        private void OnFlyoutPresenterStyleChanged()
        {
            ApplyFlyoutPresenterStyle();
        }

        private void ApplyFlyoutPresenterStyle()
        {
            if (_presenter != null && _presenter.Style != FlyoutPresenterStyle)
            {
                _presenter.Style = FlyoutPresenterStyle;
            }
        }

        #endregion

        protected override Control CreatePresenter()
        {
            if (_presenter == null)
            {
                _presenter = new FlyoutPresenter();
                _presenter.SetBinding(FlyoutPresenter.ContentProperty, new Binding("Content") { Source = this });
                ApplyFlyoutPresenterStyle();
            }

            return _presenter;
        }
    }
}
