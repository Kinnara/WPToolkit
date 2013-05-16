using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Microsoft.Phone.Controls.Primitives
{
    /// <summary>
    /// Represents a tick mark on a PhoneSlider control. Not intended for general use.
    /// </summary>
    public sealed class TickBar : Control
    {
        private const double ReservedSpace = 12;
        private const double TickLength = 2;

        private const string MinimumPropertyName = "Minimum";
        private const string MaximumPropertyName = "Maximum";
        private const string TickFrequencyPropertyName = "TickFrequency";
        private const string IsDirectionReversedPropertyName = "IsDirectionReversed";
        private const string OrientationPropertyName = "Orientation";

        private Canvas _container;

        private bool _ingorePropertyChanges;

        /// <summary>
        /// Initializes a new instance of the TickBar class.
        /// </summary>
        public TickBar()
        {
            DefaultStyleKey = typeof(TickBar);

            SizeChanged += OnSizeChanged;
        }

        #region public Brush Fill

        /// <summary>
        /// Gets or sets the Brush that draws on the background area of the TickBar.
        /// </summary>
        /// 
        /// <returns>
        /// The Brush that draws on the background area of the TickBar.
        /// </returns>
        public Brush Fill
        {
            get { return (Brush)GetValue(FillProperty); }
            set { SetValue(FillProperty, value); }
        }

        /// <summary>
        /// Identifies the Fill dependency property.
        /// </summary>
        /// 
        /// <returns>
        /// The identifier for the Fill dependency property.
        /// </returns>
        public static readonly DependencyProperty FillProperty = DependencyProperty.Register(
            "Fill",
            typeof(Brush),
            typeof(TickBar),
            new PropertyMetadata((d, e) => ((TickBar)d).OnFillChanged(e)));

        private void OnFillChanged(DependencyPropertyChangedEventArgs e)
        {
            if (_container != null)
            {
                foreach (Rectangle tick in _container.Children.OfType<Rectangle>())
                {
                    tick.Fill = (Brush)e.NewValue;
                }
            }
        }

        #endregion

        #region private double Minimum

        private double Minimum
        {
            get { return (double)GetValue(MinimumProperty); }
            set { SetValue(MinimumProperty, value); }
        }

        private static readonly DependencyProperty MinimumProperty = DependencyProperty.Register(
            MinimumPropertyName,
            typeof(double),
            typeof(TickBar),
            new PropertyMetadata(0.0, (d, e) => ((TickBar)d).UpdateTicks()));

        #endregion

        #region private double Maximum

        private double Maximum
        {
            get { return (double)GetValue(MaximumProperty); }
            set { SetValue(MaximumProperty, value); }
        }

        public static readonly DependencyProperty MaximumProperty = DependencyProperty.Register(
            MaximumPropertyName,
            typeof(double),
            typeof(TickBar),
            new PropertyMetadata(1.0, (d, e) => ((TickBar)d).UpdateTicks()));

        #endregion

        #region private double TickFrequency

        private double TickFrequency
        {
            get { return (double)GetValue(TickFrequencyProperty); }
            set { SetValue(TickFrequencyProperty, value); }
        }

        private static readonly DependencyProperty TickFrequencyProperty = DependencyProperty.Register(
            TickFrequencyPropertyName,
            typeof(double),
            typeof(TickBar),
            new PropertyMetadata(0.0, (d, e) => ((TickBar)d).UpdateTicks()));

        #endregion

        #region private bool IsDirectionReversed

        private bool IsDirectionReversed
        {
            get { return (bool)GetValue(IsDirectionReversedProperty); }
            set { SetValue(IsDirectionReversedProperty, value); }
        }

        private static readonly DependencyProperty IsDirectionReversedProperty = DependencyProperty.Register(
            IsDirectionReversedPropertyName,
            typeof(bool),
            typeof(TickBar),
            new PropertyMetadata(false, (d, e) => ((TickBar)d).UpdateTicks()));

        #endregion

        #region private Orientation Orientation

        private Orientation Orientation
        {
            get { return (Orientation)GetValue(OrientationProperty); }
            set { SetValue(OrientationProperty, value); }
        }

        private static readonly DependencyProperty OrientationProperty = DependencyProperty.Register(
            OrientationPropertyName,
            typeof(Orientation),
            typeof(TickBar),
            new PropertyMetadata(Orientation.Horizontal));

        #endregion

        /// <summary>
        /// Builds the visual tree for the TickBar control when a new template is applied.
        /// </summary>
        public override void OnApplyTemplate()
        {
            _ingorePropertyChanges = true;

            base.OnApplyTemplate();

            _container = this.GetFirstLogicalChildByType<Canvas>(false);

            PhoneSlider parent = this.GetParentByType<PhoneSlider>();
            if (parent != null)
            {
                BindToTemplatedParent(TickFrequencyProperty, TickFrequencyPropertyName);
                BindToTemplatedParent(MinimumProperty, MinimumPropertyName);
                BindToTemplatedParent(MaximumProperty, MaximumPropertyName);
                BindToTemplatedParent(IsDirectionReversedProperty, IsDirectionReversedPropertyName);
                BindToTemplatedParent(OrientationProperty, OrientationPropertyName);
            }

            _ingorePropertyChanges = false;

            UpdateTicks();
        }

        private void UpdateTicks()
        {
            if (!_ingorePropertyChanges && _container != null)
            {
                if (_container.Children.Count > 0)
                {
                    _container.Children.Clear();
                }

                if (TickFrequency <= 0)
                {
                    return;
                }

                double range = Maximum - Minimum;
                if (range <= 0)
                {
                    return;
                }

                Size size = new Size(ActualWidth, ActualHeight);
                double logicalToPhysical;

                if (Orientation == Orientation.Horizontal)
                {
                    if (NumericExtensions.IsGreaterThan(ReservedSpace, size.Width))
                    {
                        return;
                    }
                    size.Width -= ReservedSpace;
                    logicalToPhysical = size.Width / range;
                }
                else
                {
                    if (NumericExtensions.IsGreaterThan(ReservedSpace, size.Height))
                    {
                        return;
                    }
                    size.Height -= ReservedSpace;
                    logicalToPhysical = size.Height / range * -1;
                }

                if (IsDirectionReversed)
                {
                    logicalToPhysical *= -1;
                }

                if (TickFrequency > 0 && size.Width > 0 && size.Height > 0)
                {
                    double interval = TickFrequency;
                    double halfReservedSpace = ReservedSpace / 2;

                    for (double i = interval; i < range; i += interval)
                    {
                        Rectangle tick;

                        if (Orientation == Orientation.Horizontal)
                        {
                            double tickCenter = i * logicalToPhysical;
                            if (IsDirectionReversed)
                            {
                                tickCenter += size.Width;
                            }
                            double tickLeft = tickCenter + halfReservedSpace;

                            if (IsDirectionReversed)
                            {
                                if (NumericExtensions.IsGreaterThan(ReservedSpace, tickLeft))
                                {
                                    return;
                                }
                            }
                            else
                            {
                                if (NumericExtensions.IsGreaterThan(tickLeft, size.Width - ReservedSpace))
                                {
                                    return;
                                }
                            }

                            tick = new Rectangle
                            {
                                Width = TickLength,
                                Height = size.Height
                            };
                            Canvas.SetLeft(tick, tickLeft);
                        }
                        else
                        {
                            double tickCenter = i * logicalToPhysical;
                            if (!IsDirectionReversed)
                            {
                                tickCenter += size.Height;
                            }
                            double tickTop = tickCenter + halfReservedSpace;

                            if (IsDirectionReversed)
                            {
                                if (NumericExtensions.IsGreaterThan(tickTop, size.Height - ReservedSpace))
                                {
                                    return;
                                }
                            }
                            else
                            {
                                if (NumericExtensions.IsGreaterThan(ReservedSpace, tickTop))
                                {
                                    return;
                                }
                            }

                            tick = new Rectangle
                            {
                                Width = size.Width,
                                Height = TickLength
                            };
                            Canvas.SetTop(tick, tickTop);
                        }

                        tick.Fill = Fill;
                        _container.Children.Add(tick);
                    }
                }
            }
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateTicks();
        }

        private void BindToTemplatedParent(DependencyProperty target, string source)
        {
            Binding binding = new Binding();
            binding.RelativeSource = new RelativeSource(RelativeSourceMode.TemplatedParent);
            binding.Path = new PropertyPath(source);
            SetBinding(target, binding);
        }
    }
}
