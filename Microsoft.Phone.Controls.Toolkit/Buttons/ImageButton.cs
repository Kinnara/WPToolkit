using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Microsoft.Phone.Controls
{
    /// <summary>
    /// Represents a control that is a Button which has an image as its content. 
    /// </summary>
    public class ImageButton : Button
    {
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
            null);

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
        /// Identifies the <see cref="P:Microsoft.Phone.Controls.ImageButton.CornerRadius"/> dependency property.
        /// </summary>
        /// 
        /// <returns>
        /// The identifier for the <see cref="P:Microsoft.Phone.Controls.ImageButton.CornerRadius"/> dependency property.
        /// </returns>
        public static readonly DependencyProperty CornerRadiusProperty = DependencyProperty.Register(
            "CornerRadius",
            typeof(CornerRadius),
            typeof(ImageButton),
            null);

        #endregion
    }
}
