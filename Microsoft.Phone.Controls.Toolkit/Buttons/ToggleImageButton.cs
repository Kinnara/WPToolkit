using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace Microsoft.Phone.Controls
{
    /// <summary>
    /// Represents a control that is a ToggleButton which has an image as its content. 
    /// </summary>
    [TemplatePart(Name = ElementTextLabelName, Type = typeof(TextBlock))]
    public class ToggleImageButton : ToggleButton
    {
        private const string ElementTextLabelName = "TextLabel";

        private TextBlock ElementTextLabel { get; set; }

        /// <summary>
        /// Initializes a new instance of the
        /// <see cref="T:Microsoft.Phone.Controls.ToggleImageButton" /> class.
        /// </summary>
        public ToggleImageButton()
        {
            DefaultStyleKey = typeof(ToggleImageButton);
        }

        #region public ImageSource ImageSource

        /// <summary>
        /// Gets or sets the image displayed by this <see cref="T:Microsoft.Phone.Controls.ToggleImageButton"/>.
        /// </summary>
        /// 
        /// <returns>
        /// The image displayed by this <see cref="T:Microsoft.Phone.Controls.ToggleImageButton"/>.
        /// </returns>
        public ImageSource ImageSource
        {
            get { return (ImageSource)GetValue(ImageSourceProperty); }
            set { SetValue(ImageSourceProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="P:Microsoft.Phone.Controls.ToggleImageButton.ImageSource"/> dependency property.
        /// </summary>
        /// 
        /// <returns>
        /// The identifier for the <see cref="P:Microsoft.Phone.Controls.ToggleImageButton.ImageSource"/> dependency property.
        /// </returns>
        public static readonly DependencyProperty ImageSourceProperty = DependencyProperty.Register(
            "ImageSource",
            typeof(ImageSource),
            typeof(ToggleImageButton),
            new PropertyMetadata((d, e) => ((ToggleImageButton)d).OnImageSourceChanged()));

        private void OnImageSourceChanged()
        {
            UpdateActualCheckedImageSource();
        }

        #endregion

        #region public ImageSource CheckedImageSource

        /// <summary>
        /// Gets or sets the image displayed by this <see cref="T:Microsoft.Phone.Controls.ToggleImageButton"/> when checked.
        /// </summary>
        /// 
        /// <returns>
        /// The image displayed by this <see cref="T:Microsoft.Phone.Controls.ToggleImageButton"/> when checked.
        /// </returns>
        public ImageSource CheckedImageSource
        {
            get { return (ImageSource)GetValue(CheckedImageSourceProperty); }
            set { SetValue(CheckedImageSourceProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="P:Microsoft.Phone.Controls.ToggleImageButton.CheckedImageSource"/> dependency property.
        /// </summary>
        /// 
        /// <returns>
        /// The identifier for the <see cref="P:Microsoft.Phone.Controls.ToggleImageButton.CheckedImageSource"/> dependency property.
        /// </returns>
        public static readonly DependencyProperty CheckedImageSourceProperty = DependencyProperty.Register(
            "CheckedImageSource",
            typeof(ImageSource),
            typeof(ToggleImageButton),
            new PropertyMetadata((d, e) => ((ToggleImageButton)d).OnCheckedImageSourceChanged()));

        private void OnCheckedImageSourceChanged()
        {
            UpdateActualCheckedImageSource();
        }

        #endregion

        #region public ImageSource ActualCheckedImageSource

        /// <summary>
        /// Gets the actual image displayed by this <see cref="T:Microsoft.Phone.Controls.ToggleImageButton"/> when checked.
        /// </summary>
        /// 
        /// <returns>
        /// The actual image displayed by this <see cref="T:Microsoft.Phone.Controls.ToggleImageButton"/> when checked.
        /// </returns>
        public ImageSource ActualCheckedImageSource
        {
            get { return (ImageSource)GetValue(ActualCheckedImageSourceProperty); }
            private set { SetValue(ActualCheckedImageSourceProperty, value); }
        }

        private static readonly DependencyProperty ActualCheckedImageSourceProperty = DependencyProperty.Register(
            "ActualCheckedImageSource",
            typeof(ImageSource),
            typeof(ToggleImageButton),
            null);

        private void UpdateActualCheckedImageSource()
        {
            ActualCheckedImageSource = CheckedImageSource ?? ImageSource;
        }

        #endregion

        #region public string Label

        /// <summary>
        /// Gets or sets the label for the button.
        /// </summary>
        /// 
        /// <returns>
        /// The label for the button.
        /// </returns>
        public string Label
        {
            get { return (string)GetValue(LabelProperty); }
            set { SetValue(LabelProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="P:Microsoft.Phone.Controls.ToggleImageButton.Label"/> dependency property.
        /// </summary>
        /// 
        /// <returns>
        /// The identifier for the <see cref="P:Microsoft.Phone.Controls.ToggleImageButton.Label"/> dependency property.
        /// </returns>
        public static readonly DependencyProperty LabelProperty = DependencyProperty.Register(
            "Label",
            typeof(string),
            typeof(ToggleImageButton),
            new PropertyMetadata(string.Empty, (d, e) => ((ToggleImageButton)d).OnLabelChanged()));

        private void OnLabelChanged()
        {
            UpdateTextLabelVisibility();
        }

        private void UpdateTextLabelVisibility()
        {
            if (ElementTextLabel != null)
            {
                ElementTextLabel.Visibility = string.IsNullOrEmpty(Label) ? Visibility.Collapsed : Visibility.Visible;
            }
        }

        #endregion

        #region public CornerRadius CornerRadius

        /// <summary>
        /// Gets or sets the radius for the corners of the button.
        /// </summary>
        /// 
        /// <returns>
        /// The degree to which the corners are rounded.
        /// </returns>
        public CornerRadius CornerRadius
        {
            get { return (CornerRadius)GetValue(CornerRadiusProperty); }
            set { SetValue(CornerRadiusProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="P:Microsoft.Phone.Controls.ToggleImageButton.CornerRadius"/> dependency property.
        /// </summary>
        /// 
        /// <returns>
        /// The identifier for the <see cref="P:Microsoft.Phone.Controls.ToggleImageButton.CornerRadius"/> dependency property.
        /// </returns>
        public static readonly DependencyProperty CornerRadiusProperty = DependencyProperty.Register(
            "CornerRadius",
            typeof(CornerRadius),
            typeof(ToggleImageButton),
            null);

        #endregion

        /// <summary>
        /// Builds the visual tree for the
        /// <see cref="T:Microsoft.Phone.Controls.ToggleImageButton" /> control
        /// when a new template is applied.
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            ElementTextLabel = GetTemplateChild(ElementTextLabelName) as TextBlock;

            UpdateActualCheckedImageSource();
            UpdateTextLabelVisibility();
        }
    }
}
