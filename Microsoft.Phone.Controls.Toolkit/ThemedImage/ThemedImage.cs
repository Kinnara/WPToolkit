using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Microsoft.Phone.Controls
{
    /// <summary>
    /// Represents a control that displays an image.
    /// The control automatically colors the image according to the theme selection (light or dark).
    /// </summary>
    [TemplatePart(Name = ElementImageBrushName, Type = typeof(ImageBrush))]
    public class ThemedImage : Control
    {
        private const string ElementImageBrushName = "ImageBrush";

        private ImageBrush ElementImageBrush { get; set; }

        private bool _inMeasure;

        /// <summary>
        /// Initializes a new instance of the
        /// <see cref="T:Microsoft.Phone.Controls.ThemedImage" /> class.
        /// </summary>
        public ThemedImage()
        {
            DefaultStyleKey = typeof(ThemedImage);
        }

        #region public ImageSource Source

        /// <summary>
        /// Gets or sets the source for the image.
        /// </summary>
        /// 
        /// <returns>
        /// A source object for the drawn image.
        /// </returns>
        public ImageSource Source
        {
            get { return (ImageSource)GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="P:Microsoft.Phone.Controls.ThemedImage.Source"/> dependency property.
        /// </summary>
        /// 
        /// <returns>
        /// The identifier for the <see cref="P:Microsoft.Phone.Controls.ThemedImage.Source"/> dependency property.
        /// </returns>
        public static readonly DependencyProperty SourceProperty = DependencyProperty.Register(
            "Source",
            typeof(ImageSource),
            typeof(ThemedImage),
            new PropertyMetadata((d, e) => ((ThemedImage)d).OnSourceChanged()));

        private void OnSourceChanged()
        {
            ApplySource();
            UpdateImageSize();
        }

        private void ApplySource()
        {
            if (ElementImageBrush != null)
            {
                ElementImageBrush.ImageSource = Source;
            }
        }

        #endregion

        #region public Stretch Stretch

        /// <summary>
        /// Gets or sets a value that describes how an <see cref="T:Microsoft.Phone.Controls.ThemedImage"/> should be stretched to fill the destination rectangle.
        /// </summary>
        /// 
        /// <returns>
        /// <see cref="T:System.Windows.Media.Stretch"/>A value of the enumeration that specifies how the source image is applied if the <see cref="P:System.Windows.FrameworkElement.Height"/> and <see cref="P:System.Windows.FrameworkElement.Width"/> of the <see cref="T:Microsoft.Phone.Controls.ThemedImage.Source"/> are specified and are different than the source image's height and width.The default value is Uniform.
        /// </returns>
        public Stretch Stretch
        {
            get { return (Stretch)GetValue(StretchProperty); }
            set { SetValue(StretchProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="P:Microsoft.Phone.Controls.ThemedImage.Source.Stretch"/> dependency property.
        /// </summary>
        /// 
        /// <returns>
        /// The identifier for the <see cref="P:Microsoft.Phone.Controls.ThemedImage.Source.Stretch"/> dependency property.
        /// </returns>
        public static readonly DependencyProperty StretchProperty = DependencyProperty.Register(
            "Stretch",
            typeof(Stretch),
            typeof(ThemedImage),
            new PropertyMetadata(Stretch.Uniform, (d, e) => ((ThemedImage)d).OnStretchChanged()));

        private void OnStretchChanged()
        {
            ApplyStretch();
            InvalidateMeasure();
        }

        private void ApplyStretch()
        {
            if (ElementImageBrush != null)
            {
                ElementImageBrush.Stretch = Stretch;
            }
        }

        #endregion

        #region private Size ImageSize

        private Size ImageSize
        {
            get { return (Size)GetValue(ImageSizeProperty); }
            set { SetValue(ImageSizeProperty, value); }
        }

        private static readonly DependencyProperty ImageSizeProperty = DependencyProperty.Register(
            "ImageSize",
            typeof(Size),
            typeof(ThemedImage),
            new PropertyMetadata(Size.Empty, (d, e) => ((ThemedImage)d).OnImageSizeChanged()));

        private void OnImageSizeChanged()
        {
            if (!_inMeasure)
            {
                InvalidateMeasure();
            }
        }

        private void UpdateImageSize()
        {
            BitmapSource source = Source as BitmapSource;
            if (source != null && source.PixelWidth > 0 && source.PixelHeight > 0)
            {
                ImageSize = new Size(source.PixelWidth, source.PixelHeight);
            }
            else
            {
                ClearValue(ImageSizeProperty);
            }
        }

        #endregion

        /// <summary>
        /// Occurs when there is an error associated with image retrieval or format.
        /// </summary>
        public event EventHandler<ExceptionRoutedEventArgs> ImageFailed;

        /// <summary>
        /// Occurs when the image source is downloaded and decoded with no failure. You can use this event to determine the size of an image before rendering it.
        /// </summary>
        public event EventHandler<RoutedEventArgs> ImageOpened;

        /// <summary>
        /// Builds the visual tree for the
        /// <see cref="T:Microsoft.Phone.Controls.ThemedImage" /> control
        /// when a new template is applied.
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (ElementImageBrush != null)
            {
                ElementImageBrush.ImageFailed -= OnImageFailed;
                ElementImageBrush.ImageOpened -= OnImageOpened;
            }

            ElementImageBrush = GetTemplateChild(ElementImageBrushName) as ImageBrush;

            if (ElementImageBrush != null)
            {
                ElementImageBrush.ImageFailed += OnImageFailed;
                ElementImageBrush.ImageOpened += OnImageOpened;
            }

            ApplySource();
            ApplyStretch();
        }

        /// <summary>
        /// Handles the measure pass for the control.
        /// </summary>
        /// 
        /// <returns>
        /// The desired size.
        /// </returns>
        /// <param name="availableSize">The available size.</param>
        protected override Size MeasureOverride(Size availableSize)
        {
            try
            {
                _inMeasure = true;

                UpdateImageSize();

                Size imageSize = ImageSize;

                if (imageSize.IsEmpty)
                {
                    return new Size();
                }

                Stretch stretch = Stretch;
                bool availableWidthIsInfinity = double.IsInfinity(availableSize.Width);
                bool availableHeightIsInfinity = double.IsInfinity(availableSize.Height);

                if (stretch == Stretch.None ||
                    availableWidthIsInfinity && availableHeightIsInfinity)
                {
                    return imageSize;
                }

                if (availableWidthIsInfinity)
                {
                    return new Size(imageSize.Width / imageSize.Height * availableSize.Height, availableSize.Height);
                }

                if (availableHeightIsInfinity)
                {
                    return new Size(availableSize.Width, availableSize.Width / (imageSize.Width / imageSize.Height));
                }

                if (stretch == Stretch.Fill)
                {
                    return availableSize;
                }

                double constraintAspectRatio = availableSize.Width / availableSize.Height;
                double imageAspectRatio = imageSize.Width / imageSize.Height;
                double desiredWidth = 0, desiredHeight = 0;

                if (stretch == Stretch.UniformToFill)
                {
                    if (imageAspectRatio < constraintAspectRatio)
                    {
                        desiredWidth = availableSize.Width;
                        desiredHeight = desiredWidth / imageAspectRatio;
                    }
                    else
                    {
                        desiredHeight = availableSize.Height;
                        desiredWidth = desiredHeight * imageAspectRatio;
                    }
                }
                else
                {
                    if (imageAspectRatio > constraintAspectRatio)
                    {
                        desiredWidth = availableSize.Width;
                        desiredHeight = desiredWidth / imageAspectRatio;
                    }
                    else
                    {
                        desiredHeight = availableSize.Height;
                        desiredWidth = desiredHeight * imageAspectRatio;
                    }
                }

                return new Size(desiredWidth, desiredHeight);
            }
            finally
            {
                _inMeasure = false;
            }
        }

        private void OnImageFailed(object sender, ExceptionRoutedEventArgs e)
        {
            SafeRaise.Raise(ImageFailed, this, e);
        }

        private void OnImageOpened(object sender, RoutedEventArgs e)
        {
            SafeRaise.Raise(ImageOpened, this, e);

            UpdateImageSize();
        }
    }
}
