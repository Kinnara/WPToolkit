using Microsoft.Phone.Controls.LocalizedResources;
using Microsoft.Phone.Controls.Primitives;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;

namespace Microsoft.Phone.Controls
{
    /// <summary>
    /// An extended password box for Windows Phone which implements header, placeholder text, and a 
    /// show password check box.
    /// </summary>
    [TemplateVisualState(Name = VisualStates.StateNormal, GroupName = VisualStates.GroupCommon)]
    [TemplateVisualState(Name = VisualStates.StateDisabled, GroupName = VisualStates.GroupCommon)]
    [TemplatePart(Name = PasswordBoxName, Type = typeof(PasswordBox))]
    [TemplatePart(Name = TextBoxName, Type = typeof(TextBox))]
    [TemplatePart(Name = ShowPasswordCheckBoxName, Type = typeof(PhonePasswordBoxCheckBox))]
    public class PhonePasswordBox : Control
    {
        private static PasswordBox DefaultPasswordBox = new PasswordBox();
        private static RoutedEventArgs EmptyRoutedEventArgs = new RoutedEventArgs();

        #region Properties & Variables

        private PasswordBox _passwordBox;
        private TextBox _textBox;
        private FrameworkElement _placeholderTextElement;
        private PhonePasswordBoxCheckBox _showPasswordCheckBox;

        private bool _suppressSelectAll;

        #endregion

        #region Constants

        /// <summary>
        /// Main password box.
        /// </summary>
        private const string PasswordBoxName = "PasswordBox";

        /// <summary>
        /// Main text box.
        /// </summary>
        private const string TextBoxName = "TextBox";

        /// <summary>
        /// Placeholder Text.
        /// </summary>
        private const string PlaceholderTextElementName = "PlaceholderTextElement";

        /// <summary>
        /// Show password check box.
        /// </summary>
        private const string ShowPasswordCheckBoxName = "ShowPasswordCheckBox";
        #endregion

        #region FontSource

        private FontSource _fontSource;

        /// <summary>
        /// Gets or sets the font source that is applied to the password box for rendering content.
        /// </summary>
        /// 
        /// <returns>
        /// The font source used to render content in the text box. The default is null.
        /// </returns>
        /// <exception cref="T:System.ArgumentOutOfRangeException">The value set is not a valid source.</exception>
        public FontSource FontSource
        {
            get
            {
                return _fontSource;
            }
            set
            {
                if (_fontSource != value)
                {
                    _fontSource = value;
                    UpdateEditBoxesFontSource();
                }
            }
        }

        private void UpdateEditBoxesFontSource()
        {
            if (_passwordBox != null)
            {
                _passwordBox.FontSource = FontSource;
            }
            if (_textBox != null)
            {
                _textBox.FontSource = FontSource;
            }
        }

        #endregion

        #region Password

        /// <summary>
        /// Gets or sets the password currently held by the <see cref="T:Microsoft.Phone.Controls.PhonePasswordBox"/>.
        /// </summary>
        /// 
        /// <returns>
        /// A string representing the password currently held by the <see cref="T:Microsoft.Phone.Controls.PhonePasswordBox"/>.The default value is <see cref="F:System.String.Empty"/>.
        /// </returns>
        /// <exception cref="T:System.ArgumentNullException">The property is set to a null value.</exception>
        [SuppressMessage("Microsoft.Usage", "CA2208:InstantiateArgumentExceptionsCorrectly")]
        public string Password
        {
            get { return (string)GetValue(PasswordProperty); }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("Password");
                }

                SetValue(PasswordProperty, value);
            }
        }

        /// <summary>
        /// Identifies the <see cref="P:Microsoft.Phone.Controls.PhonePasswordBox.Password"/> dependency property.
        /// </summary>
        /// 
        /// <returns>
        /// The identifier for the <see cref="P:Microsoft.Phone.Controls.PhonePasswordBox.Password"/> dependency property.
        /// </returns>
        public static readonly DependencyProperty PasswordProperty =
            DependencyProperty.Register(
                "Password",
                typeof(string),
                typeof(PhonePasswordBox),
                new PropertyMetadata(DefaultPasswordBox.Password, (d, e) => ((PhonePasswordBox)d).OnPasswordChanged()));

        private void OnPasswordChanged()
        {
            if (Password == null)
            {
                Password = string.Empty;
                return;
            }

            UpdateEditBoxesValue();

            RaisePasswordChanged();
        }

        private void UpdateEditBoxesValue()
        {
            if (_passwordBox != null)
            {
                _passwordBox.Password = Password;
            }
            if (_textBox != null)
            {
                _textBox.Text = Password;
            }
        }

        /// <summary>
        /// Occurs when the value of the <see cref="P:Microsoft.Phone.Controls.PhonePasswordBox.Password"/> property changes.
        /// </summary>
        public event RoutedEventHandler PasswordChanged;

        private void RaisePasswordChanged()
        {
            RoutedEventHandler handler = PasswordChanged;
            if (handler != null)
            {
                handler(this, EmptyRoutedEventArgs);
            }
        }

        private void OnPasswordBoxPasswordChanged(object sender, RoutedEventArgs e)
        {
            Password = _passwordBox.Password;
        }

        private void OnTextBoxTextChanged(object sender, TextChangedEventArgs e)
        {
            Password = _textBox.Text;
        }

        #endregion

        #region PasswordChar

        /// <summary>
        /// Gets or sets the masking character for the <see cref="T:Microsoft.Phone.Controls.PhonePasswordBox"/>.
        /// </summary>
        /// 
        /// <returns>
        /// A masking character to echo when the user enters text into the <see cref="T:Microsoft.Phone.Controls.PhonePasswordBox"/>. The default value is a bullet character (●).
        /// </returns>
        public char PasswordChar
        {
            get { return (char)GetValue(PasswordCharProperty); }
            set { SetValue(PasswordCharProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="P:Microsoft.Phone.Controls.PhonePasswordBox.PasswordChar"/> dependency property.
        /// </summary>
        /// 
        /// <returns>
        /// The identifier for the <see cref="P:Microsoft.Phone.Controls.PhonePasswordBox.PasswordChar"/> dependency property.
        /// </returns>
        public static readonly DependencyProperty PasswordCharProperty =
            DependencyProperty.Register(
                "PasswordChar",
                typeof(char),
                typeof(PhonePasswordBox),
                new PropertyMetadata(DefaultPasswordBox.PasswordChar));

        #endregion

        #region MaxLength

        /// <summary>
        /// Gets or sets the maximum length for passwords to be handled by this <see cref="T:Microsoft.Phone.Controls.PhonePasswordBox"/>.
        /// </summary>
        /// 
        /// <returns>
        /// An integer specifying the maximum length, in character, for passwords to be handled by this <see cref="T:Microsoft.Phone.Controls.PhonePasswordBox"/>. A value of zero (0) means no limit.The default value is 0 (no length limit).
        /// </returns>
        /// <exception cref="T:System.ArgumentOutOfRangeException">The property is set to a negative value.</exception>
        [SuppressMessage("Microsoft.Usage", "CA2208:InstantiateArgumentExceptionsCorrectly")]
        public int MaxLength
        {
            get { return (int)GetValue(MaxLengthProperty); }
            set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException("MaxLength");
                }

                SetValue(MaxLengthProperty, value);
            }
        }

        /// <summary>
        /// Identifies the <see cref="P:Microsoft.Phone.Controls.PhonePasswordBox.MaxLength"/> dependency property.
        /// </summary>
        /// 
        /// <returns>
        /// The identifier for the <see cref="P:Microsoft.Phone.Controls.PhonePasswordBox.MaxLength"/> dependency property.
        /// </returns>
        public static readonly DependencyProperty MaxLengthProperty =
            DependencyProperty.Register(
                "MaxLength",
                typeof(int),
                typeof(PhonePasswordBox),
                new PropertyMetadata(DefaultPasswordBox.MaxLength));

        #endregion

        #region BaselineOffset

        /// <summary>
        /// Gets a value by which each line of text is offset from a baseline.
        /// </summary>
        /// 
        /// <returns>
        /// The amount by which each line of text is offset from the baseline, in device independent pixels. <see cref="F:System.Double.NaN"/> indicates that an optimal baseline offset is automatically calculated from the current font characteristics. The default is <see cref="F:System.Double.NaN"/>.
        /// </returns>
        public double BaselineOffset
        {
            get
            {
                if (_passwordBox != null)
                {
                    return _passwordBox.BaselineOffset;
                }
                if (_textBox != null)
                {
                    return _textBox.BaselineOffset;
                }
                return double.NaN;
            }
        }

        #endregion

        #region SelectionBackground

        /// <summary>
        /// Gets or sets the brush used to render the background for the selected text.
        /// </summary>
        /// 
        /// <returns>
        /// The brush that fills the background of the selected text.
        /// </returns>
        public Brush SelectionBackground
        {
            get { return (Brush)GetValue(SelectionBackgroundProperty); }
            set { SetValue(SelectionBackgroundProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="P:Microsoft.Phone.Controls.PhonePasswordBox.SelectionBackground"/> dependency property.
        /// </summary>
        /// 
        /// <returns>
        /// The identifier for the <see cref="P:Microsoft.Phone.Controls.PhonePasswordBox.SelectionBackground"/> dependency property.
        /// </returns>
        public static readonly DependencyProperty SelectionBackgroundProperty =
            DependencyProperty.Register(
                "SelectionBackground",
                typeof(Brush),
                typeof(PhonePasswordBox),
                new PropertyMetadata(DefaultPasswordBox.SelectionBackground));

        #endregion

        #region SelectionForeground

        /// <summary>
        /// Gets or sets the brush used for the selected text in the <see cref="T:Microsoft.Phone.Controls.PhonePasswordBox"/>.
        /// </summary>
        /// 
        /// <returns>
        /// The brush used for the selected text in the <see cref="T:Microsoft.Phone.Controls.PhonePasswordBox"/>.
        /// </returns>
        public Brush SelectionForeground
        {
            get { return (Brush)GetValue(SelectionForegroundProperty); }
            set { SetValue(SelectionForegroundProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="P:Microsoft.Phone.Controls.PhonePasswordBox.SelectionForeground"/> dependency property.
        /// </summary>
        /// 
        /// <returns>
        /// The identifier for the <see cref="P:Microsoft.Phone.Controls.PhonePasswordBox.SelectionForeground"/> dependency property.
        /// </returns>
        public static readonly DependencyProperty SelectionForegroundProperty =
            DependencyProperty.Register(
                "SelectionForeground",
                typeof(Brush),
                typeof(PhonePasswordBox),
                new PropertyMetadata(DefaultPasswordBox.SelectionForeground));

        #endregion

        #region CaretBrush

        /// <summary>
        /// Gets or sets the brush that is used to render the vertical bar that indicates the insertion point.
        /// </summary>
        /// 
        /// <returns>
        /// The brush that is used to render the vertical bar that indicates the insertion point.
        /// </returns>
        public Brush CaretBrush
        {
            get { return (Brush)GetValue(CaretBrushProperty); }
            set { SetValue(CaretBrushProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="P:Microsoft.Phone.Controls.PhonePasswordBox.CaretBrush"/> dependency property.
        /// </summary>
        /// 
        /// <returns>
        /// The identifier for the <see cref="P:Microsoft.Phone.Controls.PhonePasswordBox.CaretBrush"/> dependency property.
        /// </returns>
        public static readonly DependencyProperty CaretBrushProperty =
            DependencyProperty.Register(
                "CaretBrush",
                typeof(Brush),
                typeof(PhonePasswordBox),
                new PropertyMetadata(DefaultPasswordBox.CaretBrush));

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
        /// <see cref="P:Microsoft.Phone.Controls.PhonePasswordBox.Header" />
        /// dependency property.
        /// </summary>
        /// <value>
        /// The identifier for the
        /// <see cref="P:Microsoft.Phone.Controls.PhonePasswordBox.Header" />
        /// dependency property.
        /// </value>
        public static readonly DependencyProperty HeaderProperty = DependencyProperty.Register(
            "Header",
            typeof(object),
            typeof(PhonePasswordBox),
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
        /// <see cref="P:Microsoft.Phone.Controls.PhonePasswordBox.HeaderTemplate" />
        /// dependency property.
        /// </summary>
        /// <value>
        /// The identifier for the
        /// <see cref="P:Microsoft.Phone.Controls.PhonePasswordBox.HeaderTemplate" />
        /// dependency property.
        /// </value>
        public static readonly DependencyProperty HeaderTemplateProperty = DependencyProperty.Register(
            "HeaderTemplate",
            typeof(DataTemplate),
            typeof(PhonePasswordBox),
            null);

        #endregion

        #region PlaceholderText
        /// <summary>
        /// The placeholder text in the password box when the password box doesn't have the input focus and the user hasn't entered any characters.
        /// </summary>
        /// <value>
        /// The placeholder text to display in the password box.
        /// </value>
        public string PlaceholderText
        {
            get { return (string)GetValue(PlaceholderTextProperty); }
            set { SetValue(PlaceholderTextProperty, value); }
        }

        /// <summary>
        /// Identifies the
        /// <see cref="P:Microsoft.Phone.Controls.PhonePasswordBox.PlaceholderText" />
        /// dependency property.
        /// </summary>
        /// <value>
        /// The identifier for the
        /// <see cref="P:Microsoft.Phone.Controls.PhonePasswordBox.PlaceholderText" />
        /// dependency property.
        /// </value>
        public static readonly DependencyProperty PlaceholderTextProperty = DependencyProperty.Register(
            "PlaceholderText",
            typeof(string),
            typeof(PhonePasswordBox),
            new PropertyMetadata(string.Empty));

        /// <summary>
        /// Determines if the PlaceholderText should be shown or not based on if there is content in the PhonePasswordBox.
        /// </summary>
        private void UpdatePlaceholderTextVisibility()
        {
            if (_placeholderTextElement != null)
            {
                if (string.IsNullOrEmpty(Password))
                {
                    _placeholderTextElement.Visibility = Visibility.Visible;
                }
                else
                {
                    _placeholderTextElement.Visibility = Visibility.Collapsed;
                }
            }
        }

        #endregion

        #region ShowPassword

        /// <summary>
        /// Gets or sets a value that determines whether the visual UI of the <see cref="T:Microsoft.Phone.Controls.PhonePasswordBox" />
        /// should include a check box element that toggles showing or hiding the typed characters.
        /// </summary>
        /// 
        /// <returns>
        /// True to show a show password check box; false to not show a show password check box.
        /// </returns>
        public bool AllowShowPassword
        {
            get { return (bool)GetValue(AllowShowPasswordProperty); }
            set { SetValue(AllowShowPasswordProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="P:Microsoft.Phone.Controls.PhonePasswordBox.AllowShowPassword"/> dependency property.
        /// </summary>
        /// 
        /// <returns>
        /// The identifier for the <see cref="P:Microsoft.Phone.Controls.PhonePasswordBox.AllowShowPassword"/> dependency property.
        /// </returns>
        public static readonly DependencyProperty AllowShowPasswordProperty = DependencyProperty.Register(
            "AllowShowPassword",
            typeof(bool),
            typeof(PhonePasswordBox),
            new PropertyMetadata(false, (d, e) => ((PhonePasswordBox)d).OnAllowShowPasswordChanged()));

        private void OnAllowShowPasswordChanged()
        {
            Visibility visibility = AllowShowPassword ? Visibility.Visible : Visibility.Collapsed;
            Thickness outerPadding = AllowShowPassword ? new Thickness(0, 0, 0, 50) : new Thickness();

            if (_showPasswordCheckBox != null)
            {
                _showPasswordCheckBox.Visibility = visibility;
            }

            if (_passwordBox != null)
            {
                _textBox.Visibility = visibility;
                SetMarginOnImplementationRoot(_passwordBox, outerPadding);
            }

            if (_textBox != null)
            {
                _textBox.Visibility = visibility;
                SetMarginOnImplementationRoot(_textBox, outerPadding);
            }

            if (!AllowShowPassword)
            {
                ShowPassword = false;
            }
        }

        private bool ShowPassword
        {
            get { return (bool)GetValue(ShowPasswordProperty); }
            set { SetValue(ShowPasswordProperty, value); }
        }

        private static readonly DependencyProperty ShowPasswordProperty = DependencyProperty.Register(
            "ShowPassword",
            typeof(bool),
            typeof(PhonePasswordBox),
            new PropertyMetadata((d, e) => ((PhonePasswordBox)d).OnShowPasswordChanged()));

        private void OnShowPasswordChanged()
        {
            bool focusSet = false;

            if (IsEnabled && AllowShowPassword)
            {
                if (ShowPassword)
                {
                    if (_textBox != null)
                    {
                        if (_textBox.Focus())
                        {
                            _textBox.SelectionStart = _textBox.Text.Length;
                            focusSet = true;
                        }
                    }
                }
                else
                {
                    if (_passwordBox != null)
                    {
                        focusSet = _passwordBox.Focus();
                    }
                }
            }

            _suppressSelectAll = focusSet;

            if (!focusSet)
            {
                UpdateEditBoxesInteractivity();
            }
        }

        #endregion

        /// <summary>
        /// Initializes a new instance of the
        /// <see cref="T:Microsoft.Phone.Controls.PhonePasswordBox" /> class.
        /// </summary>
        public PhonePasswordBox()
        {
            DefaultStyleKey = typeof(PhonePasswordBox);
            IsEnabledChanged += OnIsEnabledChanged;
        }

        /// <summary>
        /// Builds the visual tree for the
        /// <see cref="T:Microsoft.Phone.Controls.PhonePasswordBox" /> control
        /// when a new template is applied.
        /// </summary>
        public override void OnApplyTemplate()
        {
            if (_passwordBox != null)
            {
                _passwordBox.GotFocus -= OnEditBoxGotFocus;
                _passwordBox.PasswordChanged -= OnPasswordBoxPasswordChanged;
            }

            if (_textBox != null)
            {
                _textBox.GotFocus -= OnEditBoxGotFocus;
                _textBox.TextChanged -= OnTextBoxTextChanged;
            }

            if (_showPasswordCheckBox != null)
            {
                ClearValue(ShowPasswordProperty);
            }

            base.OnApplyTemplate();

            _passwordBox = GetTemplateChild(PasswordBoxName) as PasswordBox;
            _textBox = GetTemplateChild(TextBoxName) as TextBox;
            _placeholderTextElement = GetTemplateChild(PlaceholderTextElementName) as FrameworkElement;
            _showPasswordCheckBox = GetTemplateChild(ShowPasswordCheckBoxName) as PhonePasswordBoxCheckBox;

            if (_passwordBox != null)
            {
                _passwordBox.GotFocus += OnEditBoxGotFocus;
                _passwordBox.PasswordChanged += OnPasswordBoxPasswordChanged;
            }

            if (_textBox != null)
            {
                _textBox.GotFocus += OnEditBoxGotFocus;
                _textBox.TextChanged += OnTextBoxTextChanged;
            }

            if (_showPasswordCheckBox != null)
            {
                _showPasswordCheckBox.Content = ControlResources.ShowPassword;
                SetBinding(ShowPasswordProperty, new Binding("IsChecked") { Source = _showPasswordCheckBox, Mode = BindingMode.TwoWay });
            }

            UpdateEditBoxesFontSource();
            UpdateEditBoxesInteractivity();
            UpdateEditBoxesValue();
            UpdatePlaceholderTextVisibility();
            OnAllowShowPasswordChanged();

            ChangeVisualState(false);
        }

        /// <summary>
        /// Selects all the character in the <see cref="T:Microsoft.Phone.Controls.PhonePasswordBox"/>.
        /// </summary>
        public void SelectAll()
        {
            if (_passwordBox != null)
            {
                _passwordBox.SelectAll();
            }
            if (_textBox != null)
            {
                _textBox.SelectAll();
            }
        }

        /// <summary>
        /// Provides handling for the
        /// <see cref="E:System.Windows.UIElement.LostFocus" /> event.
        /// </summary>
        /// <param name="e">A <see cref="T:System.Windows.RoutedEventArgs" />
        /// that contains the event data.</param>
        protected override void OnLostFocus(RoutedEventArgs e)
        {
            UpdatePlaceholderTextVisibility();
            base.OnLostFocus(e);
        }

        private void OnEditBoxGotFocus(object sender, RoutedEventArgs e)
        {
            UpdateEditBoxesInteractivity();

            if (_placeholderTextElement != null)
            {
                _placeholderTextElement.Visibility = Visibility.Collapsed;
            }

            if (_suppressSelectAll)
            {
                _suppressSelectAll = false;
            }
            else
            {
                SelectAll();
            }
        }

        private void UpdateEditBoxesInteractivity()
        {
            UIElement visibleEditBox;
            UIElement hiddenEditBox;

            if (ShowPassword)
            {
                visibleEditBox = _textBox;
                hiddenEditBox = _passwordBox;
            }
            else
            {
                visibleEditBox = _passwordBox;
                hiddenEditBox = _textBox;
            }

            if (visibleEditBox != null)
            {
                visibleEditBox.IsHitTestVisible = true;
                visibleEditBox.Opacity = 1;
            }

            if (hiddenEditBox != null)
            {
                hiddenEditBox.IsHitTestVisible = false;
                hiddenEditBox.Opacity = 0;
            }
        }

        private void OnIsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            ShowPassword = false;
            ChangeVisualState();
        }

        private void ChangeVisualState()
        {
            ChangeVisualState(true);
        }

        private void ChangeVisualState(bool useTransitions)
        {
            GoToState(useTransitions, IsEnabled ? VisualStates.StateNormal : VisualStates.StateDisabled);
        }

        private bool GoToState(bool useTransitions, string stateName)
        {
            return VisualStateManager.GoToState(this, stateName, useTransitions);
        }

        private static void SetMarginOnImplementationRoot(Control control, Thickness margin)
        {
            control.ApplyTemplate();
            FrameworkElement root = VisualStates.GetImplementationRoot(control);
            if (root != null)
            {
                root.Margin = margin;
            }
        }
    }
}
