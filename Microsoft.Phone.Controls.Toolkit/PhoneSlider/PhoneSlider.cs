using System;
using System.Windows;
using System.Windows.Controls;

namespace Microsoft.Phone.Controls
{
    /// <summary>
    /// An extended Slider for Windows Phone which implements tick marks and snap points.
    /// </summary>
    public class PhoneSlider : Slider
    {
        private bool _isSettingValue;

        /// <summary>
        /// Initializes a new instance of the PhoneSlider class.
        /// </summary>
        public PhoneSlider()
        {
            DefaultStyleKey = typeof(PhoneSlider);
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
            new PropertyMetadata(0.0, (d, e) => ((PhoneSlider)d).OnTickFrequencyChanged(e)));

        private void OnTickFrequencyChanged(DependencyPropertyChangedEventArgs e)
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
    }
}
