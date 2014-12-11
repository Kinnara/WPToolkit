using System.Diagnostics.CodeAnalysis;
using System.Windows;

namespace Microsoft.Phone.Controls
{
    /// <summary>
    /// An extended Pivot.
    /// </summary>
    [SuppressMessage("Microsoft.Naming", "CA1711:IdentifiersShouldNotHaveIncorrectSuffix")]
    public class PivotEx : Pivot
    {
        /// <summary>
        /// Initializes a new instance of the PivotEx class.
        /// </summary>
        public PivotEx()
        {
            DefaultStyleKey = typeof(PivotEx);
        }

        #region public object Subtitle

        /// <summary>
        /// Gets or sets the subtitle to be optionally set above the headers.
        /// </summary>
        /// 
        /// <returns>
        /// Returns <see cref="T:System.Object"/>.
        /// </returns>
        public object Subtitle
        {
            get { return (object)GetValue(SubtitleProperty); }
            set { SetValue(SubtitleProperty, value); }
        }

        /// <summary>
        /// Identifies the Subtitle dependency property.
        /// </summary>
        public static readonly DependencyProperty SubtitleProperty = DependencyProperty.Register(
            "Subtitle",
            typeof(object),
            typeof(PivotEx),
            null);

        #endregion

        #region public DataTemplate SubtitleTemplate

        /// <summary>
        /// Gets or sets the subtitle template used for displaying the subtitle above the headers area.
        /// </summary>
        /// 
        /// <returns>
        /// Returns <see cref="T:System.Windows.DataTemplate"/>.
        /// </returns>
        public DataTemplate SubtitleTemplate
        {
            get { return (DataTemplate)GetValue(SubtitleTemplateProperty); }
            set { SetValue(SubtitleTemplateProperty, value); }
        }

        /// <summary>
        /// Identifies the SubtitleTemplate dependency property.
        /// </summary>
        public static readonly DependencyProperty SubtitleTemplateProperty = DependencyProperty.Register(
            "SubtitleTemplate",
            typeof(DataTemplate),
            typeof(PivotEx),
            null);

        #endregion

        #region public Style TitlePanelStyle

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
            typeof(PivotEx),
            null);

        #endregion
    }
}
