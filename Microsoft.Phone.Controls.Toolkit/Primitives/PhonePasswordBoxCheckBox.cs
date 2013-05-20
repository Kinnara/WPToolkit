using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Microsoft.Phone.Controls.Primitives
{
    /// <summary>
    /// Represents a check box on a PhonePasswordBox control. Not intended for general use.
    /// </summary>
    [TemplateVisualState(Name = VisualStates.StateNormal, GroupName = VisualStates.GroupCommon)]
    [TemplateVisualState(Name = VisualStates.StateDisabled, GroupName = VisualStates.GroupCommon)]
    [TemplateVisualState(Name = StateChecked, GroupName = GroupCheck)]
    [TemplateVisualState(Name = StateUnchecked, GroupName = GroupCheck)]
    public class PhonePasswordBoxCheckBox : ContentControl
    {
        private const string StateChecked = "Checked";
        private const string StateUnchecked = "Unchecked";
        private const string GroupCheck = "CheckStates";

        private bool _isMouseCaptured;
        private bool _isMouseLeftButtonDown;
        private Point _mousePosition;
        private bool _suspendStateChanges;

        /// <summary>
        /// Initializes a new instance of the
        /// <see cref="T:Microsoft.Phone.Controls.Primitives.PhonePasswordBoxCheckBox" /> class.
        /// </summary>
        public PhonePasswordBoxCheckBox()
        {
            DefaultStyleKey = typeof(PhonePasswordBoxCheckBox);

            Loaded += delegate
            {
                UpdateVisualState(false);
            };

            IsEnabledChanged += OnIsEnabledChanged;
        }

        #region IsChecked

        /// <summary>
        /// Gets or sets whether the <see cref="T:Microsoft.Phone.Controls.Primitives.PhonePasswordBoxCheckBox"/> is checked.
        /// </summary>
        /// 
        /// <returns>
        /// true if the <see cref="T:Microsoft.Phone.Controls.Primitives.PhonePasswordBoxCheckBox"/> is checked;
        /// false if the <see cref="T:Microsoft.Phone.Controls.Primitives.PhonePasswordBoxCheckBox"/> is unchecked.
        /// The default is false.
        /// </returns>
        public bool IsChecked
        {
            get { return (bool)GetValue(IsCheckedProperty); }
            set { SetValue(IsCheckedProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="P:Microsoft.Phone.Controls.Primitives.PhonePasswordBoxCheckBox.IsChecked"/> dependency property.
        /// </summary>
        /// 
        /// <returns>
        /// The identifier for the <see cref="P:Microsoft.Phone.Controls.Primitives.PhonePasswordBoxCheckBox.IsChecked"/> dependency property.
        /// </returns>
        public static readonly DependencyProperty IsCheckedProperty = DependencyProperty.Register(
            "IsChecked",
            typeof(bool),
            typeof(PhonePasswordBoxCheckBox),
            new PropertyMetadata((d, e) => ((PhonePasswordBoxCheckBox)d).UpdateVisualState()));

        #endregion

        #region IsPressed

        private bool IsPressed
        {
            get { return (bool)GetValue(IsPressedProperty); }
            set { SetValue(IsPressedProperty, value); }
        }

        private static readonly DependencyProperty IsPressedProperty = DependencyProperty.Register(
            "IsPressed",
            typeof(bool),
            typeof(PhonePasswordBoxCheckBox),
            new PropertyMetadata((d, e) => ((PhonePasswordBoxCheckBox)d).UpdateVisualState()));

        #endregion

        /// <summary>
        /// Builds the visual tree for the <see cref="T:Microsoft.Phone.Controls.Primitives.PhonePasswordBoxCheckBox"/> when a new template is applied.
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            UpdateVisualState(false);
        }

        /// <summary>
        /// Provides handling for the <see cref="E:System.Windows.UIElement.GotFocus"/> event.
        /// </summary>
        /// <param name="e">The event data.</param>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="e"/> is null.</exception>
        protected override void OnGotFocus(RoutedEventArgs e)
        {
            base.OnGotFocus(e);
            UpdateVisualState();
        }

        /// <summary>
        /// Provides handling for the <see cref="E:System.Windows.UIElement.LostFocus"/> event.
        /// </summary>
        /// <param name="e">The event data for the <see cref="E:System.Windows.UIElement.LostFocus"/> event.</param>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="e"/> is null.</exception>
        protected override void OnLostFocus(RoutedEventArgs e)
        {
            base.OnLostFocus(e);
            _suspendStateChanges = true;
            try
            {
                IsPressed = false;
                ReleaseMouseCaptureInternal();
            }
            finally
            {
                _suspendStateChanges = false;
                UpdateVisualState();
            }
        }

        /// <summary>
        /// Provides class handling for the <see cref="E:System.Windows.UIElement.MouseLeave"/> routed event that occurs when the user’s touch leaves an element.
        /// </summary>
        /// <param name="e">The event data for the <see cref="E:System.Windows.UIElement.MouseLeave"/> event.</param>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="e"/> is null.</exception>
        protected override void OnMouseLeave(MouseEventArgs e)
        {
            base.OnMouseLeave(e);
            _suspendStateChanges = true;
            try
            {
                if (IsEnabled)
                {
                    IsPressed = false;
                }
            }
            finally
            {
                _suspendStateChanges = false;
                UpdateVisualState();
            }
        }

        /// <summary>
        /// Provides class handling for the <see cref="E:System.Windows.UIElement.MouseLeftButtonDown"/> event that occurs when the user touches the control.
        /// </summary>
        /// <param name="e">The event data. </param>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="e"/> is null.</exception>
        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);

            if (e.Handled)
            {
                return;
            }

            _isMouseLeftButtonDown = true;

            if (!IsEnabled)
            {
                return;
            }

            e.Handled = true;

            _suspendStateChanges = true;
            try
            {
                CaptureMouseInternal();
                if (_isMouseCaptured)
                {
                    IsPressed = true;
                }
            }
            finally
            {
                _suspendStateChanges = false;
                UpdateVisualState();
            }
        }

        /// <summary>
        /// Provides class handling for the <see cref="E:System.Windows.UIElement.MouseLeftButtonUp"/> event that occurs when the user lifts their finger while it is over this control.
        /// </summary>
        /// <param name="e">The event data.</param>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="e"/> is null.</exception>
        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonUp(e);

            if (e.Handled)
            {
                return;
            }

            _isMouseLeftButtonDown = false;

            if (!IsEnabled)
            {
                return;
            }

            e.Handled = true;

            if (IsPressed)
            {
                OnClick();
            }

            ReleaseMouseCaptureInternal();
            IsPressed = false;
        }

        /// <summary>
        /// Provides handling for the <see cref="E:System.Windows.UIElement.LostMouseCapture"/> event.
        /// </summary>
        /// <param name="e">A <see cref="T:System.Windows.Input.MouseEventArgs"/> that contains the event data.</param>
        protected override void OnLostMouseCapture(MouseEventArgs e)
        {
            ReleaseMouseCaptureInternal();
            IsPressed = false;
        }

        /// <summary>
        /// Provides class handling for the <see cref="E:System.Windows.UIElement.MouseMove"/> event that occurs when the mouse pointer moves while over this element.
        /// </summary>
        /// <param name="e">The event data.</param>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="e"/> is null.</exception>
        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            _mousePosition = e.GetPosition(this);
            if (_isMouseLeftButtonDown && IsEnabled && _isMouseCaptured)
            {
                IsPressed = IsValidMousePosition();
            }
        }

        private void UpdateVisualState()
        {
            UpdateVisualState(true);
        }

        private void UpdateVisualState(bool useTransitions)
        {
            if (_suspendStateChanges)
            {
                return;
            }

            ChangeVisualState(useTransitions);
        }

        private void ChangeVisualState(bool useTransitions)
        {
            if (!IsEnabled)
            {
                GoToState(useTransitions, VisualStates.StateDisabled);
            }
            else if (IsPressed)
            {
                GoToState(useTransitions, VisualStates.StatePressed);
            }
            else
            {
                GoToState(useTransitions, VisualStates.StateNormal);
            }

            GoToState(useTransitions, IsChecked ? StateChecked : StateUnchecked);
        }

        private bool GoToState(bool useTransitions, string stateName)
        {
            return VisualStateManager.GoToState(this, stateName, useTransitions);
        }

        private void CaptureMouseInternal()
        {
            if (_isMouseCaptured)
            {
                return;
            }

            _isMouseCaptured = CaptureMouse();
        }

        private void ReleaseMouseCaptureInternal()
        {
            ReleaseMouseCapture();
            _isMouseCaptured = false;
        }

        private void OnIsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            _suspendStateChanges = true;
            try
            {
                if (!IsEnabled)
                {
                    IsPressed = false;
                    _isMouseCaptured = false;
                    _isMouseLeftButtonDown = false;
                }
            }
            finally
            {
                _suspendStateChanges = false;
                UpdateVisualState();
            }
        }

        private void OnClick()
        {
            IsChecked = !IsChecked;
        }

        private bool IsValidMousePosition()
        {
            return (_mousePosition.X >= 0) && (_mousePosition.X <= ActualWidth) && (_mousePosition.Y >= 0) && (_mousePosition.Y <= ActualHeight);
        }
    }
}
