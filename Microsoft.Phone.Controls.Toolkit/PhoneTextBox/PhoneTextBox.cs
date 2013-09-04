// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Microsoft.Phone.Controls
{
    /// <summary>
    /// An extended TextBox for Windows Phone which implements header, placeholder text, an action icon, and a 
    /// length indicator.
    /// </summary>
    /// <QualityBand>Experimental</QualityBand>
    [TemplateVisualState(Name = LengthIndicatorVisibleState, GroupName = LengthIndicatorStates)]
    [TemplateVisualState(Name = LengthIndicatorHiddenState, GroupName = LengthIndicatorStates)]
    [TemplateVisualState(Name = NormalState, GroupName = CommonStates)]
    [TemplateVisualState(Name = DisabledState, GroupName = CommonStates)]
    [TemplateVisualState(Name = ReadOnlyState, GroupName = CommonStates)]
    [TemplateVisualState(Name = FocusedState, GroupName = FocusStates)]
    [TemplateVisualState(Name = UnfocusedState, GroupName = FocusStates)]
    [TemplatePart(Name = PlaceholderTextElementName, Type = typeof(FrameworkElement))]
    [TemplatePart(Name = LengthIndicatorName, Type = typeof(TextBlock))]
    public class PhoneTextBox : TextBox
    {

        #region PhoneTextBox Properties & Variables
        private Grid _rootGrid;
        private Border _mainBorder;

        // Used to measure the height of the TextBox to determine if the action icon is being overlapped. 
        private TextBlock _measurementTextBlock;

        // Placeholder Text Private Variables.
        private FrameworkElement _placeholderTextElement;

        // Length Indicator Private Variables.
        private TextBlock _lengthIndicator;

        // Ignore flags for the dependency properties.
        private bool _ignorePropertyChange = false;

        //Temporarily ignore focus?
        private bool _ignoreFocus = false;

        /// <summary>
        /// Border for PhoneTextBox action icon
        /// </summary>
        protected Border ActionIconBorder { get; set; }

        private bool IsFocused { get; set; }

        #endregion

        #region Constants
        /// <summary>
        /// Root grid.
        /// </summary>
        private const string RootGridName = "RootGrid";

        /// <summary>
        /// Main border.
        /// </summary>
        private const string MainBorderName = "MainBorder";

        /// <summary>
        /// Placeholder Text.
        /// </summary>
        private const string PlaceholderTextElementName = "PlaceholderTextElement";

        /// <summary>
        /// Length indicator name.
        /// </summary>
        private const string LengthIndicatorName = "LengthIndicator";

        /// <summary>
        /// Action icon canvas.
        /// </summary>
        private const string ActionIconCanvasName = "ActionIconCanvas";

        /// <summary>
        /// Measurement Text Block name.
        /// </summary>
        private const string MeasurementTextBlockName = "MeasurementTextBlock";

        /// <summary>
        /// Action icon image name.
        /// </summary>
        private const string ActionIconBorderName = "ActionIconBorder";
        #endregion

        #region Visual States
        /// <summary>
        /// Length indicator states.
        /// </summary>
        private const string LengthIndicatorStates = "LengthIndicatorStates";

        /// <summary>
        /// Length indicator visible visual state.
        /// </summary>
        private const string LengthIndicatorVisibleState = "LengthIndicatorVisible";

        /// <summary>
        /// Length indicator hidden visual state.
        /// </summary>
        private const string LengthIndicatorHiddenState = "LengthIndicatorHidden";

        /// <summary>
        /// Common States.
        /// </summary>
        private const string CommonStates = "CommonStates";

        /// <summary>
        /// Normal state.
        /// </summary>
        private const string NormalState = "Normal";

        /// <summary>
        /// Disabled state.
        /// </summary>
        private const string DisabledState = "Disabled";

        /// <summary>
        /// ReadOnly state.
        /// </summary>
        private const string ReadOnlyState = "ReadOnly";

        /// <summary>
        /// Focus states.
        /// </summary>
        private const string FocusStates = "FocusStates";

        /// <summary>
        /// Focused state.
        /// </summary>
        private const string FocusedState = "Focused";

        /// <summary>
        /// Unfocused state.
        /// </summary>
        private const string UnfocusedState = "Unfocused";
        #endregion

        #region Header

        /// <summary>
        /// Gets or sets the content for the header of the control.
        /// </summary>
        /// <value>
        /// The content for the header of the control. The default value is
        /// null.
        /// </value>
        public object Header
        {
            get { return (object)GetValue(HeaderProperty); }
            set { SetValue(HeaderProperty, value); }
        }

        /// <summary>
        /// Identifies the
        /// <see cref="P:Microsoft.Phone.Controls.PhoneTextBox.Header" />
        /// dependency property.
        /// </summary>
        /// <value>
        /// The identifier for the
        /// <see cref="P:Microsoft.Phone.Controls.PhoneTextBox.Header" />
        /// dependency property.
        /// </value>
        public static readonly DependencyProperty HeaderProperty = DependencyProperty.Register(
            "Header",
            typeof(object),
            typeof(PhoneTextBox),
            null);

        /// <summary>
        /// Gets or sets the template that is used to display the content of the
        /// control's header.
        /// </summary>
        /// <value>
        /// The template that is used to display the content of the control's
        /// header. The default is null.
        /// </value>
        public DataTemplate HeaderTemplate
        {
            get { return (DataTemplate)GetValue(HeaderTemplateProperty); }
            set { SetValue(HeaderTemplateProperty, value); }
        }

        /// <summary>
        /// Identifies the
        /// <see cref="P:Microsoft.Phone.Controls.PhoneTextBox.HeaderTemplate" />
        /// dependency property.
        /// </summary>
        /// <value>
        /// The identifier for the
        /// <see cref="P:Microsoft.Phone.Controls.PhoneTextBox.HeaderTemplate" />
        /// dependency property.
        /// </value>
        public static readonly DependencyProperty HeaderTemplateProperty = DependencyProperty.Register(
            "HeaderTemplate",
            typeof(DataTemplate),
            typeof(PhoneTextBox),
            null);

        #endregion

        #region Placeholder Text
        /// <summary>
        /// Gets or sets the text that is displayed in the control until the value is changed by a user action or some other operation.
        /// </summary>
        /// 
        /// <returns>
        /// The text that is displayed in the control when no value is entered. The default is an empty string ("").
        /// </returns>
        public string PlaceholderText
        {
            get { return (string)GetValue(PlaceholderTextProperty); }
            set { SetValue(PlaceholderTextProperty, value); }
        }

        /// <summary>
        /// Identifies the
        /// <see cref="P:Microsoft.Phone.Controls.PhoneTextBox.PlaceholderText" />
        /// dependency property.
        /// </summary>
        /// <value>
        /// The identifier for the
        /// <see cref="P:Microsoft.Phone.Controls.PhoneTextBox.PlaceholderText" />
        /// dependency property.
        /// </value>
        public static readonly DependencyProperty PlaceholderTextProperty = DependencyProperty.Register(
            "PlaceholderText",
            typeof(string),
            typeof(PhoneTextBox),
            new PropertyMetadata(string.Empty));

        /// <summary>
        /// Determines if the PlaceholderText should be shown or not based on if there is content in the TextBox.
        /// </summary>
        private void UpdatePlaceholderTextVisibility()
        {
            if (_placeholderTextElement != null)
            {
                if (!IsFocused && string.IsNullOrEmpty(Text))
                {
                    _placeholderTextElement.Visibility = Visibility.Visible;
                }
                else
                {
                    _placeholderTextElement.Visibility = Visibility.Collapsed;
                }
            }
        }

        /// <summary>
        /// Override the Blur/LostFocus event to show the PlaceholderText if needed.
        /// </summary>
        /// <param name="e">Event arguments.</param>
        protected override void OnLostFocus(RoutedEventArgs e)
        {
            IsFocused = false;
            UpdatePlaceholderTextVisibility();
            base.OnLostFocus(e);
        }

        /// <summary>
        /// Override the Focus event to hide the PlaceholderText.
        /// </summary>
        /// <param name="e">Event arguments.</param>
        protected override void OnGotFocus(RoutedEventArgs e)
        {
            if (_ignoreFocus)
            {
                _ignoreFocus = false;

                var rootFrame = Application.Current.RootVisual as Frame;
                rootFrame.Focus();

                return;
            }

            IsFocused = true;

            if (_placeholderTextElement != null)
            {
                _placeholderTextElement.Visibility = Visibility.Collapsed;
            }

            base.OnGotFocus(e);
        }

        #endregion

        #region Length Indicator
        /// <summary>
        /// Length Indicator Visibile Dependency Property.
        /// </summary>
        public static readonly DependencyProperty LengthIndicatorVisibleProperty =
            DependencyProperty.Register("LengthIndicatorVisible", typeof(Boolean), typeof(PhoneTextBox), new PropertyMetadata(
                new PropertyChangedCallback(OnLengthIndicatorVisibleChanged)
               )
            );

        /// <summary>
        /// Boolean that determines if the length indicator should be visible.
        /// </summary>
        public Boolean LengthIndicatorVisible
        {
            get { return (bool)base.GetValue(LengthIndicatorVisibleProperty); }
            set { base.SetValue(LengthIndicatorVisibleProperty, value); }
        }

        private static void OnLengthIndicatorVisibleChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            PhoneTextBox phoneTextBox = sender as PhoneTextBox;

            phoneTextBox.UpdateLengthIndicatorVisibility();
        }

        /// <summary>
        /// Length Indicator Visibility Threshold Dependency Property.
        /// </summary>
        public static readonly DependencyProperty LengthIndicatorThresholdProperty =
            DependencyProperty.Register("LengthIndicatorThreshold", typeof(int), typeof(PhoneTextBox), new PropertyMetadata(
                new PropertyChangedCallback(OnLengthIndicatorThresholdChanged)
               )
            );

        /// <summary>
        /// Threshold after which the length indicator will appear.
        /// </summary>
        public int LengthIndicatorThreshold
        {
            get { return (int)base.GetValue(LengthIndicatorThresholdProperty); }
            set { base.SetValue(LengthIndicatorThresholdProperty, value); }
        }

        [SuppressMessage("Microsoft.Usage", "CA2208:InstantiateArgumentExceptionsCorrectly", Justification = "The property name is the appropriate value to throw.")]
        private static void OnLengthIndicatorThresholdChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            PhoneTextBox phoneTextBox = sender as PhoneTextBox;

            if (phoneTextBox._ignorePropertyChange)
            {
                phoneTextBox._ignorePropertyChange = false;
                return;
            }

            if (phoneTextBox.LengthIndicatorThreshold < 0)
            {
                phoneTextBox._ignorePropertyChange = true;
                phoneTextBox.SetValue(LengthIndicatorThresholdProperty, args.OldValue);

                throw new ArgumentOutOfRangeException("LengthIndicatorThreshold", "The length indicator visibility threshold must be greater than zero.");
            }

        }

        /// <summary>
        /// The displayed maximum length of text that can be entered. This value takes
        /// priority over the MaxLength property in the Length Indicator display.
        /// </summary>
        public static readonly DependencyProperty DisplayedMaxLengthProperty =
            DependencyProperty.Register("DisplayedMaxLength", typeof(int), typeof(PhoneTextBox), new PropertyMetadata(
                new PropertyChangedCallback(DisplayedMaxLengthChanged)
              )  
             );


        /// <summary>
        /// The displayed value for the maximum length of the input.
        /// </summary>
        public int DisplayedMaxLength
        {
            get { return (int)base.GetValue(DisplayedMaxLengthProperty); }
            set { base.SetValue(DisplayedMaxLengthProperty, value); }
        }

        [SuppressMessage("Microsoft.Usage", "CA2208:InstantiateArgumentExceptionsCorrectly", Justification = "The property name is the appropriate value to throw.")]
        private static void DisplayedMaxLengthChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            PhoneTextBox phoneTextBox = sender as PhoneTextBox;

            if (phoneTextBox.DisplayedMaxLength > phoneTextBox.MaxLength && phoneTextBox.MaxLength > 0)
            {
                throw new ArgumentOutOfRangeException("DisplayedMaxLength", "The displayed maximum length cannot be greater than the MaxLength.");
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Microsoft.Globalization", 
            "CA1303:Do not pass literals as localized parameters", 
            MessageId = "System.Windows.Controls.TextBlock.set_Text(System.String)",
            Justification = "At this time the length indicator is not culture-specific or retrieved from the resources.")]
        private void UpdateLengthIndicatorVisibility()
        {
            if (_rootGrid == null || _lengthIndicator == null)
            {
                return;
            }

            bool isHidden = true;
            if (LengthIndicatorVisible)
            {
                // The current implementation is culture invariant.
                _lengthIndicator.Text = String.Format(
                    CultureInfo.InvariantCulture,
                    "{0}/{1}", Text.Length, 
                    ((DisplayedMaxLength > 0) ? DisplayedMaxLength : MaxLength));

                if (Text.Length >= LengthIndicatorThreshold)
                {
                    isHidden = false;
                }
            }

            VisualStateManager.GoToState(this, isHidden ? LengthIndicatorHiddenState : LengthIndicatorVisibleState, IsFocused);
        }

        #endregion

        #region Action Icon
        /// <summary>
        /// Identifies the ActionIcon DependencyProperty.
        /// </summary>
        public static readonly DependencyProperty ActionIconProperty =
            DependencyProperty.Register("ActionIcon", typeof(ImageSource), typeof(PhoneTextBox), new PropertyMetadata(
                new PropertyChangedCallback(OnActionIconChanged)
               )
            );

        /// <summary>
        /// Gets or sets the ActionIcon.
        /// </summary>
        public ImageSource ActionIcon
        {
            get { return base.GetValue(ActionIconProperty) as ImageSource; }
            set { base.SetValue(ActionIconProperty, value); }
        }


        /// <summary>
        /// Gets or sets whether the ActionItem is hidden when there is not text entered in the PhoneTextBox.
        /// </summary>
        public bool HidesActionItemWhenEmpty
        {
            get { return (bool)GetValue(HidesActionItemWhenEmptyProperty); }
            set { SetValue(HidesActionItemWhenEmptyProperty, value); }
        }

        /// <summary>
        /// Identifies the HidesActionItemWhenEmpty DependencyProperty.
        /// </summary>
        public static readonly DependencyProperty HidesActionItemWhenEmptyProperty =
            DependencyProperty.Register("HidesActionItemWhenEmpty", typeof(bool), typeof(PhoneTextBox), new PropertyMetadata(false, OnActionIconChanged));

        

        /// <summary>
        /// Action Icon Tapped event.
        /// </summary>
        public event EventHandler ActionIconTapped;

        private static void OnActionIconChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            PhoneTextBox phoneTextBox = sender as PhoneTextBox;

            if (phoneTextBox != null)
            {
                phoneTextBox.UpdateActionIconVisibility();
            }
        }

        private void UpdateActionIconVisibility()
        {
            if (ActionIconBorder != null)
            {
                if (ActionIcon == null || (HidesActionItemWhenEmpty && string.IsNullOrEmpty(Text)))
                {
                    ActionIconBorder.Visibility = Visibility.Collapsed;
                    if (_mainBorder != null)
                    {
                        _mainBorder.Padding = new Thickness(0);
                    }
                }
                else
                {
                    ActionIconBorder.Visibility = Visibility.Visible;

                    if (_mainBorder != null && TextWrapping != System.Windows.TextWrapping.Wrap)
                    {
                        _mainBorder.Padding = new Thickness(0, 0, 48, 0);
                    }
                }
            }
        }

        /// <summary>
        /// Determines if the developer set an event for ActionIconTapped.
        /// </summary>
        /// <param name="sender">The sender object</param>
        /// <param name="e">The RoutedEventArgs for the event</param>
        private void OnActionIconTapped(object sender, RoutedEventArgs e)
        {
            _ignoreFocus = true;

            if (TiltEffect.CurrentTiltElement == ActionIconBorder)
            {
                TiltEffect.EndCurrentTiltEffect(true);
            }

            var handler = ActionIconTapped;

            if (handler != null)
            {
                handler(this, e);
            }
        }

        private void ResizeTextBox()
        {
            if (ActionIcon == null || TextWrapping != System.Windows.TextWrapping.Wrap) { return; }

            _measurementTextBlock.Width = ActualWidth;

            if (_measurementTextBlock.ActualHeight > ActualHeight - 72)
            {
                Height = ActualHeight + 72;
            }
            else if (ActualHeight > _measurementTextBlock.ActualHeight + 144)
            {
                Height = ActualHeight - 72;
            }
        }

        #endregion

        /// <summary>
        /// Initializes a new instance of the PhoneTextBox class.
        /// </summary>
        public PhoneTextBox()
        {
            DefaultStyleKey = typeof(PhoneTextBox);
            CacheMode = new BitmapCache();
            TextChanged += OnTextChanged;
        }

        /// <summary>
        /// Applies the template and checks to see if the PlaceholderText should be shown
        /// when the page is first loaded.
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (ActionIconBorder != null)
            {
                ActionIconBorder.Tap -= OnActionIconTapped;
            }

            _rootGrid = GetTemplateChild(RootGridName) as Grid;
            
            // Getting template children for the placeholder text.
            _placeholderTextElement = GetTemplateChild(PlaceholderTextElementName) as FrameworkElement;
            _mainBorder = GetTemplateChild(MainBorderName) as Border;

            if (_placeholderTextElement != null)
            {
                UpdatePlaceholderTextVisibility();
            }
            
            // Getting template children for the length indicator.
            _lengthIndicator = GetTemplateChild(LengthIndicatorName) as TextBlock;
            
            // Getting template child for the action icon
            ActionIconBorder = GetTemplateChild(ActionIconBorderName) as Border;

            if (_rootGrid != null && _lengthIndicator != null)
            {
                UpdateLengthIndicatorVisibility();
            }

            if (ActionIconBorder != null)
            {
                ActionIconBorder.Tap += OnActionIconTapped;
                UpdateActionIconVisibility(); // Add back the padding if needed.
            }

            
            // Get template child for the action icon measurement text block.
            _measurementTextBlock = GetTemplateChild(MeasurementTextBlockName) as TextBlock;
        }

        /// <summary>
        /// Called when the selection changed event occurs. This determines whether the length indicator should be shown or
        /// not and if the TextBox needs to grow.
        /// </summary>
        /// <param name="sender">Sender TextBox</param>
        /// <param name="e">Event arguments</param>
        private void OnTextChanged(object sender, RoutedEventArgs e)
        {
            UpdateLengthIndicatorVisibility();
            UpdateActionIconVisibility();
            UpdatePlaceholderTextVisibility();
            ResizeTextBox();
        }
    }
}