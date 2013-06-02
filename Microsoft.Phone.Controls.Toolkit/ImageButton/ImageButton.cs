using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Microsoft.Phone.Controls
{
    /// <summary>
    /// Represents a control that is a Button which has an image as its content. 
    /// </summary>
    [TemplatePart(Name = ElementImageBrushName, Type = typeof(ImageBrush))]
    public class ImageButton : Button
    {
        private const string ElementImageBrushName = "ImageBrush";

        private ImageBrush ElementImageBrush { get; set; }

        /// <summary>
        /// Initializes a new instance of the
        /// <see cref="T:Microsoft.Phone.Controls.ImageButton" /> class.
        /// </summary>
        public ImageButton()
        {
            DefaultStyleKey = typeof(ImageButton);
        }

        #region public ImageSource ImageSource

        /// <summary>
        /// Gets or sets the image displayed by this <see cref="T:Microsoft.Phone.Controls.ImageButton"/>.
        /// </summary>
        /// 
        /// <returns>
        /// The image displayed by this <see cref="T:Microsoft.Phone.Controls.ImageButton"/>.
        /// </returns>
        public ImageSource ImageSource
        {
            get { return (ImageSource)GetValue(ImageSourceProperty); }
            set { SetValue(ImageSourceProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="P:Microsoft.Phone.Controls.ImageButton.ImageSource"/> dependency property.
        /// </summary>
        /// 
        /// <returns>
        /// The identifier for the <see cref="P:Microsoft.Phone.Controls.ImageButton.ImageSource"/> dependency property.
        /// </returns>
        public static readonly DependencyProperty ImageSourceProperty = DependencyProperty.Register(
            "ImageSource",
            typeof(ImageSource),
            typeof(ImageButton),
            new PropertyMetadata((d, e) => ((ImageButton)d).OnImageSourceChanged()));

        private void OnImageSourceChanged()
        {
            UpdateImageBrush();
        }

        #endregion

        /// <summary>
        /// Builds the visual tree for the
        /// <see cref="T:Microsoft.Phone.Controls.ImageButton" /> control
        /// when a new template is applied.
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            ElementImageBrush = GetTemplateChild(ElementImageBrushName) as ImageBrush;

            UpdateImageBrush();
        }

        private void UpdateImageBrush()
        {
            if (ElementImageBrush != null)
            {
                ElementImageBrush.ImageSource = ImageSource;
            }
        }
    }
}
