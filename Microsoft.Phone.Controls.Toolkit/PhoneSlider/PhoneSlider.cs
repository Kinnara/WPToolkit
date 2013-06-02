using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace Microsoft.Phone.Controls
{
    /// <summary>
    /// An extended Slider for Windows Phone which implements tick marks and snap points.
    /// </summary>
#if WP7
    [TemplatePart(Name = ElementHorizontalFillName, Type = typeof(FrameworkElement))]
    [TemplatePart(Name = ElementHorizontalTrackName, Type = typeof(FrameworkElement))]
    [TemplatePart(Name = ElementHorizontalCenterElementName, Type = typeof(FrameworkElement))]
    [TemplatePart(Name = ElementVerticalFillName, Type = typeof(FrameworkElement))]
    [TemplatePart(Name = ElementVerticalTrackName, Type = typeof(FrameworkElement))]
    [TemplatePart(Name = ElementVerticalCenterElementName, Type = typeof(FrameworkElement))]
#endif
    public class PhoneSlider : Slider
    {
        private bool _isSettingValue;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Microsoft.Phone.Controls.PhoneSlider"/> class.
        /// </summary>
        public PhoneSlider()
        {
            DefaultStyleKey = typeof(PhoneSlider);
#if WP7
            SizeChanged += delegate
            {
                UpdateTrackLayout();
            };

            SetBinding(IsDirectionReversedShadowProperty, new Binding("IsDirectionReversed") { Source = this });
            SetBinding(OrientationShadowProperty, new Binding("Orientation") { Source = this });
#endif
        }

        #region public double TickFrequency

        /// <summary>
        /// Gets or sets the increment of the value range that ticks should
        /// be created for. For example, if a PhoneSlider has Maximum of 20, Minimum of 0, and TickFrequency of 2, 10 tick marks are defined not counting the zero point.
        /// </summary>
        /// 
        /// <returns>
        /// The increment to create tick marks for.
        /// </returns>
        public double TickFrequency
        {
            get { return (double)GetValue(TickFrequencyProperty); }
            set { SetValue(TickFrequencyProperty, value); }
        }

        /// <summary>
        /// Identifies the TickFrequency dependency property.
        /// </summary>
        /// 
        /// <returns>
        /// The identifier for the TickFrequency dependency property.
        /// </returns>
        public static readonly DependencyProperty TickFrequencyProperty = DependencyProperty.Register(
            "TickFrequency",
            typeof(double),
            typeof(PhoneSlider),
            new PropertyMetadata(0.0, OnTickFrequencyChanged));

        private static void OnTickFrequencyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!IsValidChange(e.NewValue))
            {
                throw new ArgumentException(TickFrequencyProperty.ToString());
            }
        }

        #endregion

        /// <summary>
        /// Updates the current position of the <see cref="T:Microsoft.Phone.Controls.PhoneSlider"/> when the <see cref="P:System.Windows.Controls.Primitives.RangeBase.Value"/> property changes.
        /// </summary>
        /// <param name="oldValue">The old <see cref="P:System.Windows.Controls.Primitives.RangeBase.Value"/> of the <see cref="T:Microsoft.Phone.Controls.PhoneSlider"/>.</param>
        /// <param name="newValue">The new <see cref="P:System.Windows.Controls.Primitives.RangeBase.Value"/> of the <see cref="T:Microsoft.Phone.Controls.PhoneSlider"/>.</param>
        protected override void OnValueChanged(double oldValue, double newValue)
        {
            if (!_isSettingValue && TickFrequency > 0)
            {
                double nearestTick = Math.Round(newValue / TickFrequency) * TickFrequency;

                double snapValue;
                if (Math.Abs(nearestTick - newValue) < Math.Abs(Maximum - newValue))
                {
                    snapValue = nearestTick;
                }
                else
                {
                    snapValue = Maximum;
                }

                if (newValue != snapValue)
                {
                    _isSettingValue = true;
                    Value = snapValue;
                    _isSettingValue = false;
                    return;
                }
            }

            base.OnValueChanged(oldValue, newValue);
#if WP7
            UpdateTrackLayout();
#endif
        }

        private static bool IsValidDoubleValue(object value)
        {
            double d = (double)value;
            return !double.IsNaN(d) && !double.IsInfinity(d);
        }

        private static bool IsValidChange(object value)
        {
            return IsValidDoubleValue(value) && (double)value >= 0;
        }

#if WP7
        private const string ElementHorizontalTemplateName = "HorizontalTemplate";
        private const string ElementHorizontalFillName = "HorizontalFill";
        private const string ElementHorizontalTrackName = "HorizontalTrack";
        private const string ElementHorizontalCenterElementName = "HorizontalCenterElement";
        private const string ElementVerticalTemplateName = "VerticalTemplate";
        private const string ElementVerticalFillName = "VerticalFill";
        private const string ElementVerticalTrackName = "VerticalTrack";
        private const string ElementVerticalCenterElementName = "VerticalCenterElement";

        private FrameworkElement ElementHorizontalTemplate { get; set; }
        private FrameworkElement ElementHorizontalFill { get; set; }
        private FrameworkElement ElementHorizontalTrack { get; set; }
        private FrameworkElement ElementHorizontalCenterElement { get; set; }
        private FrameworkElement ElementVerticalTemplate { get; set; }
        private FrameworkElement ElementVerticalFill { get; set; }
        private FrameworkElement ElementVerticalTrack { get; set; }
        private FrameworkElement ElementVerticalCenterElement { get; set; }

        private bool? _isDragging;
        private double _dragValue;

        private static readonly DependencyProperty IsDirectionReversedShadowProperty = DependencyProperty.Register(
            "IsDirectionReversedShadow",
            typeof(bool),
            typeof(PhoneSlider),
            new PropertyMetadata((d, e) => ((PhoneSlider)d).UpdateTrackLayout()));

        private static readonly DependencyProperty OrientationShadowProperty = DependencyProperty.Register(
            "OrientationShadow",
            typeof(Orientation),
            typeof(PhoneSlider),
            new PropertyMetadata(Orientation.Horizontal, (d, e) => ((PhoneSlider)d).UpdateTrackLayout()));

        /// <summary>
        /// Builds the visual tree for the <see cref="T:Microsoft.Phone.Controls.PhoneSlider"/> control when a new template is applied.
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            ElementHorizontalTemplate = GetTemplateChild(ElementHorizontalTemplateName) as FrameworkElement;
            ElementHorizontalFill = GetTemplateChild(ElementHorizontalFillName) as FrameworkElement;
            ElementHorizontalTrack = GetTemplateChild(ElementHorizontalTrackName) as FrameworkElement;
            ElementHorizontalCenterElement = GetTemplateChild(ElementHorizontalCenterElementName) as FrameworkElement;
            ElementVerticalTemplate = GetTemplateChild(ElementVerticalTemplateName) as FrameworkElement;
            ElementVerticalFill = GetTemplateChild(ElementVerticalFillName) as FrameworkElement;
            ElementVerticalTrack = GetTemplateChild(ElementVerticalTrackName) as FrameworkElement;
            ElementVerticalCenterElement = GetTemplateChild(ElementVerticalCenterElementName) as FrameworkElement;

            ManipulationStarted += OnManipulationStarted;
            ManipulationDelta += OnManipulationDelta;
            ManipulationCompleted += OnManipulationCompleted;
        }

        /// <summary>
        /// Called when the <see cref="P:System.Windows.Controls.Primitives.RangeBase.Minimum"/> property changes.
        /// </summary>
        /// <param name="oldMinimum">Old value of the <see cref="P:System.Windows.Controls.Primitives.RangeBase.Minimum"/> property.</param>
        /// <param name="newMinimum">New value of the <see cref="P:System.Windows.Controls.Primitives.RangeBase.Minimum"/> property.</param>
        protected override void OnMinimumChanged(double oldMinimum, double newMinimum)
        {
            base.OnMinimumChanged(oldMinimum, newMinimum);
            UpdateTrackLayout();
        }

        /// <summary>
        /// Called when the <see cref="P:System.Windows.Controls.Primitives.RangeBase.Maximum"/> property changes.
        /// </summary>
        /// <param name="oldMaximum">Old value of the <see cref="P:System.Windows.Controls.Primitives.RangeBase.Maximum"/> property.</param>
        /// <param name="newMaximum">New value of the <see cref="P:System.Windows.Controls.Primitives.RangeBase.Maximum"/> property.</param>
        protected override void OnMaximumChanged(double oldMaximum, double newMaximum)
        {
            base.OnMaximumChanged(oldMaximum, newMaximum);
            UpdateTrackLayout();
        }

        private void OnManipulationStarted(object sender, ManipulationStartedEventArgs e)
        {
            UpdateSliderPositionOnManipulation(e.ManipulationContainer, e.ManipulationOrigin);
        }

        private void OnManipulationDelta(object sender, ManipulationDeltaEventArgs e)
        {
            if (e.Handled)
            {
                return;
            }

            if (!_isDragging.HasValue)
            {
                Point translation = e.DeltaManipulation.Translation;
                bool horizontal = Math.Abs(translation.Y) / Math.Abs(translation.X) < Math.Tan(Math.PI / 8);
                bool vertical = !horizontal && Math.Abs(translation.X) / Math.Abs(translation.Y) < Math.Tan(Math.PI / 8);
                if (horizontal)
                {
                    _isDragging = Orientation == Orientation.Horizontal;
                }
                else if (vertical)
                {
                    _isDragging = Orientation == Orientation.Vertical;
                }
            }

            if (_isDragging != false)
            {
                UpdateSliderPositionOnManipulation(e.ManipulationContainer, e.ManipulationOrigin);
                e.Handled = true;
            }
        }

        private void OnManipulationCompleted(object sender, ManipulationCompletedEventArgs e)
        {
            bool? isDragging = _isDragging;
            _isDragging = null;

            if (!e.Handled)
            {
                e.Handled = isDragging != false;
            }
        }

        private void UpdateSliderPositionOnManipulation(UIElement manipulationContainer, Point manipulationOrigin)
        {
            if (Orientation == Orientation.Horizontal && ElementHorizontalTemplate == null || Orientation == Orientation.Vertical && ElementVerticalTemplate == null)
            {
                return;
            }

            double centerElementSpace = 0;

            if (Orientation == Orientation.Horizontal && ElementHorizontalCenterElement != null)
            {
                centerElementSpace = ElementHorizontalCenterElement.ActualWidth;
            }
            else if (Orientation == Orientation.Vertical && ElementVerticalCenterElement != null)
            {
                centerElementSpace = ElementVerticalCenterElement.ActualHeight;
            }

            Point point = manipulationContainer
                .TransformToVisual(Orientation == Orientation.Horizontal ? ElementHorizontalTemplate : ElementVerticalTemplate)
                .Transform(manipulationOrigin);
            double dragValue = Orientation == Orientation.Horizontal ?
                (point.X - centerElementSpace / 2) / (ElementHorizontalTemplate.ActualWidth - centerElementSpace) * (Maximum - Minimum) + Minimum :
                Maximum - (point.Y - centerElementSpace / 2) / (ElementVerticalTemplate.ActualHeight - centerElementSpace) * (Maximum - Minimum);
            if (double.IsNaN(dragValue) || double.IsInfinity(dragValue))
            {
                return;
            }

            _dragValue = IsDirectionReversed ? Maximum - dragValue : dragValue;

            double newValue = Math.Min(Maximum, Math.Max(Minimum, _dragValue));
            if (newValue != Value)
            {
                Value = newValue;
            }
        }

        private void UpdateTrackLayout()
        {
            double maximum = Maximum;
            double minimum = Minimum;
            double value = Value;
            double relativeValue = 1 - (maximum - value) / (maximum - minimum);
            if (IsDirectionReversed)
            {
                relativeValue = 1 - relativeValue;
            }

            Grid grid = Orientation == Orientation.Horizontal ? ElementHorizontalTemplate as Grid : ElementVerticalTemplate as Grid;
            if (grid == null)
            {
                return;
            }

            if (Orientation == Orientation.Horizontal)
            {
                if (ElementHorizontalFill == null || ElementHorizontalCenterElement == null)
                {
                    return;
                }

                double translateX = Math.Max(0, relativeValue * (grid.ActualWidth - ElementHorizontalCenterElement.ActualWidth));

                RectangleGeometry rectangleGeometry = ElementHorizontalFill.Clip as RectangleGeometry;
                if (rectangleGeometry != null)
                {
                    rectangleGeometry.Rect = new Rect(
                        rectangleGeometry.Rect.X,
                        rectangleGeometry.Rect.Y,
                        ElementHorizontalCenterElement.ActualWidth / 2 + translateX,
                        rectangleGeometry.Rect.Height);
                }

                TranslateTransform translateTransform = ElementHorizontalCenterElement.RenderTransform as TranslateTransform;
                if (translateTransform != null)
                {
                    translateTransform.X = translateX;
                }
            }
            else
            {
                if (ElementVerticalFill == null || ElementVerticalCenterElement == null)
                {
                    return;
                }

                double translateY = Math.Max(0, (1 - relativeValue) * (grid.ActualHeight - ElementVerticalCenterElement.ActualHeight));

                RectangleGeometry rectangleGeometry = ElementVerticalFill.Clip as RectangleGeometry;
                if (rectangleGeometry != null)
                {
                    rectangleGeometry.Rect = new Rect(
                        rectangleGeometry.Rect.X,
                        ElementVerticalCenterElement.ActualHeight / 2 + translateY,
                        rectangleGeometry.Rect.Width,
                        Math.Max(grid.ActualHeight - ElementVerticalCenterElement.ActualHeight / 2 + translateY, 0));
                }

                TranslateTransform translateTransform = ElementVerticalCenterElement.RenderTransform as TranslateTransform;
                if (translateTransform != null)
                {
                    translateTransform.Y = translateY;
                }
            }
        }
#endif
    }
}
