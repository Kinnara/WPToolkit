using System.Windows;
using System.Windows.Controls;

namespace Microsoft.Phone.Controls
{
    /// <summary>
    /// Represents a Page UI.
    /// </summary>
    [StyleTypedProperty(Property = "TitleStyle", StyleTargetType = typeof(ContentControl))]
    [StyleTypedProperty(Property = "HeaderStyle", StyleTargetType = typeof(ContentControl))]
    public class PageUI : ContentControl
    {
        private BasePage _parentPage;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Microsoft.Phone.Controls.PageUI" /> class.
        /// </summary>
        public PageUI()
        {
            DefaultStyleKey = typeof(PageUI);
        }

        #region Title

        /// <summary>
        /// Gets or sets the title.
        /// </summary>
        public object Title
        {
            get { return (object)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="P:Microsoft.Phone.Controls.PageUI.Title"/> dependency property.
        /// </summary>
        /// 
        /// <returns>
        /// The identifier for the <see cref="P:Microsoft.Phone.Controls.PageUI.Title"/> dependency property.
        /// </returns>
        public static readonly DependencyProperty TitleProperty = DependencyProperty.Register(
            "Title",
            typeof(object),
            typeof(PageUI),
            null);

        #endregion

        #region TitleTemplate

        /// <summary>
        /// Gets or sets the template used to display the title.
        /// </summary>
        public DataTemplate TitleTemplate
        {
            get { return (DataTemplate)GetValue(TitleTemplateProperty); }
            set { SetValue(TitleTemplateProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="P:Microsoft.Phone.Controls.PageUI.TitleTemplate"/> dependency property.
        /// </summary>
        /// 
        /// <returns>
        /// The identifier for the <see cref="P:Microsoft.Phone.Controls.PageUI.TitleTemplate"/> dependency property.
        /// </returns>
        public static readonly DependencyProperty TitleTemplateProperty = DependencyProperty.Register(
            "TitleTemplate",
            typeof(DataTemplate),
            typeof(PageUI),
            null);

        #endregion

        #region TitleStyle

        /// <summary>
        /// Gets or sets the style that is used when rendering the title.
        /// </summary>
        public Style TitleStyle
        {
            get { return (Style)GetValue(TitleStyleProperty); }
            set { SetValue(TitleStyleProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="P:Microsoft.Phone.Controls.PageUI.TitleStyle"/> dependency property.
        /// </summary>
        /// 
        /// <returns>
        /// The identifier for the <see cref="P:Microsoft.Phone.Controls.PageUI.TitleStyle"/> dependency property.
        /// </returns>
        public static readonly DependencyProperty TitleStyleProperty = DependencyProperty.Register(
            "TitleStyle",
            typeof(Style),
            typeof(PageUI),
            null);

        #endregion

        #region Header

        /// <summary>
        /// Gets or sets the header.
        /// </summary>
        public object Header
        {
            get { return (object)GetValue(HeaderProperty); }
            set { SetValue(HeaderProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="P:Microsoft.Phone.Controls.PageUI.Header"/> dependency property.
        /// </summary>
        /// 
        /// <returns>
        /// The identifier for the <see cref="P:Microsoft.Phone.Controls.PageUI.Header"/> dependency property.
        /// </returns>
        public static readonly DependencyProperty HeaderProperty = DependencyProperty.Register(
            "Header",
            typeof(object),
            typeof(PageUI),
            null);

        #endregion

        #region HeaderTemplate

        /// <summary>
        /// Gets or sets the template used to display the header.
        /// </summary>
        public DataTemplate HeaderTemplate
        {
            get { return (DataTemplate)GetValue(HeaderTemplateProperty); }
            set { SetValue(HeaderTemplateProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="P:Microsoft.Phone.Controls.PageUI.HeaderTemplate"/> dependency property.
        /// </summary>
        /// 
        /// <returns>
        /// The identifier for the <see cref="P:Microsoft.Phone.Controls.PageUI.HeaderTemplate"/> dependency property.
        /// </returns>
        public static readonly DependencyProperty HeaderTemplateProperty = DependencyProperty.Register(
            "HeaderTemplate",
            typeof(DataTemplate),
            typeof(PageUI),
            null);

        #endregion

        #region HeaderStyle

        /// <summary>
        /// Gets or sets the style that is used when rendering the header.
        /// </summary>
        public Style HeaderStyle
        {
            get { return (Style)GetValue(HeaderStyleProperty); }
            set { SetValue(HeaderStyleProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="P:Microsoft.Phone.Controls.PageUI.HeaderStyle"/> dependency property.
        /// </summary>
        /// 
        /// <returns>
        /// The identifier for the <see cref="P:Microsoft.Phone.Controls.PageUI.HeaderStyle"/> dependency property.
        /// </returns>
        public static readonly DependencyProperty HeaderStyleProperty = DependencyProperty.Register(
            "HeaderStyle",
            typeof(Style),
            typeof(PageUI),
            null);

        #endregion

        #region TitlePanelStyle

        /// <summary>
        /// Gets or sets the style applied to the title panel.
        /// </summary>
        public Style TitlePanelStyle
        {
            get { return (Style)GetValue(TitlePanelStyleProperty); }
            set { SetValue(TitlePanelStyleProperty, value); }
        }

        /// <summary>
        /// Identifies the TitlePanelStyle dependency property.
        /// </summary>
        public static readonly DependencyProperty TitlePanelStyleProperty = DependencyProperty.Register(
            "TitlePanelStyle",
            typeof(Style),
            typeof(PageUI),
            null);

        #endregion

        /// <summary>
        /// Builds the visual tree for the
        /// <see cref="T:Microsoft.Phone.Controls.PageUI" /> control
        /// when a new template is applied.
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (_parentPage != null)
            {
                _parentPage.StopLayoutUpdates(this);
            }

            _parentPage = this.GetParentByType<BasePage>();

            if (_parentPage != null)
            {
                _parentPage.StartLayoutUpdates(this);
            }
        }
    }
}
