using Microsoft.Phone.Shell;
using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Navigation;

namespace Microsoft.Phone.Controls
{
    /// <summary>
    /// Represents a dialog box that can be customized to contain checkboxes, hyperlinks, buttons and any other XAML content.
    /// </summary>
    [TemplatePart(Name = ContainerName, Type = typeof(FrameworkElement))]
    [TemplatePart(Name = LayoutRootName, Type = typeof(FrameworkElement))]
    [TemplatePart(Name = Button1HostName, Type = typeof(Border))]
    [TemplatePart(Name = Button2HostName, Type = typeof(Border))]
    public class ContentDialog : ContentControl
    {
        /// <summary>
        /// Holds a weak reference to the content dialog that is currently displayed.
        /// </summary>
        private static WeakReference _currentInstance;

        /// <summary>
        /// The current screen width.
        /// </summary>
        private static readonly double _screenWidth = Application.Current.Host.Content.ActualWidth;

        /// <summary>
        /// The current screen height.
        /// </summary>
        private static readonly double _screenHeight = Application.Current.Host.Content.ActualHeight;

        private const string ContainerName = "Container";
        private const string LayoutRootName = "LayoutRoot";
        private const string Button1HostName = "Button1Host";
        private const string Button2HostName = "Button2Host";

        /// <summary>
        /// Identifies whether the application bar and the system tray
        /// must be restored after the content dialog is closed. 
        /// </summary>
        private static bool _mustRestore = true;

        private static TaskCompletionSource<ContentDialogResult> _tcs;

        /// <summary>
        /// The popup used to display the content dialog.
        /// </summary>
        private Popup _popup;

        /// <summary>
        /// The root visual of the application.
        /// </summary>
        private PhoneApplicationFrame _frame;

        /// <summary>
        /// The current application page.
        /// </summary>
        private PhoneApplicationPage _page;

        /// <summary>
        /// Identifies whether the application bar is visible or not before
        /// opening the content dialog.
        /// </summary>
        private bool _hasApplicationBar;

        /// <summary>
        /// The current color of the system tray.
        /// </summary>
        private Color _systemTrayColor;

        /// <summary>
        /// The current opacity of the system tray.
        /// </summary>
        private double _systemTrayOpacity;

        /// <summary>
        /// Whether the content dialog is currently in the process of being 
        /// closed.
        /// </summary>
        private bool _isBeingClosed = false;

        private OrientationHelper _orientationHelper;

        /// <summary>
        /// Initializes a new instance of the ContentDialog class.
        /// </summary>
        public ContentDialog()
        {
            DefaultStyleKey = typeof(ContentDialog);

            _orientationHelper = new OrientationHelper(this);
        }

        #region public object Title

        /// <summary>
        /// Gets or sets the title of the dialog.
        /// </summary>
        /// <returns>
        /// The title of the dialog.
        /// </returns>
        public object Title
        {
            get { return (object)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }

        /// <summary>
        /// Identifies the Title dependency property.
        /// </summary>
        /// 
        /// <returns>
        /// The identifier for the Title dependency property.
        /// </returns>
        public static readonly DependencyProperty TitleProperty = DependencyProperty.Register(
            "Title",
            typeof(object),
            typeof(ContentDialog),
            null);

        #endregion

        #region public DataTemplate TitleTemplate

        /// <summary>
        /// Gets or sets the title template.
        /// </summary>
        /// 
        /// <returns>
        /// The title template.
        /// </returns>
        public DataTemplate TitleTemplate
        {
            get { return (DataTemplate)GetValue(TitleTemplateProperty); }
            set { SetValue(TitleTemplateProperty, value); }
        }

        /// <summary>
        /// Identifies the TitleTemplate dependency property.
        /// </summary>
        /// 
        /// <returns>
        /// The identifier for the TitleTemplate dependency property.
        /// </returns>
        public static readonly DependencyProperty TitleTemplateProperty = DependencyProperty.Register(
            "TitleTemplate",
            typeof(DataTemplate),
            typeof(ContentDialog),
            null);

        #endregion

        #region public string PrimaryButtonText

        /// <summary>
        /// Gets or sets the text to display on the primary button.
        /// </summary>
        /// 
        /// <returns>
        /// The text to display on the primary button. To hide this button, set the text to null or string.Empty. The default is empty.
        /// </returns>
        public string PrimaryButtonText
        {
            get { return (string)GetValue(PrimaryButtonTextProperty); }
            set { SetValue(PrimaryButtonTextProperty, value); }
        }

        /// <summary>
        /// Identifies the PrimaryButtonText dependency property.
        /// </summary>
        /// 
        /// <returns>
        /// The identifier for the PrimaryButtonText dependency property.
        /// </returns>
        public static readonly DependencyProperty PrimaryButtonTextProperty = DependencyProperty.Register(
            "PrimaryButtonText",
            typeof(string),
            typeof(ContentDialog),
            new PropertyMetadata(string.Empty, (d, e) => ((ContentDialog)d).OnPrimaryButtonTextChanged(e)));

        private void OnPrimaryButtonTextChanged(DependencyPropertyChangedEventArgs e)
        {
            UpdateButton1Visibility();
        }

        #endregion

        #region public ICommand PrimaryButtonCommand

        /// <summary>
        /// Gets or sets the command to invoke when the primary button is tapped.
        /// </summary>
        /// 
        /// <returns>
        /// The command to invoke when the primary button is tapped.
        /// </returns>
        public ICommand PrimaryButtonCommand
        {
            get { return (ICommand)GetValue(PrimaryButtonCommandProperty); }
            set { SetValue(PrimaryButtonCommandProperty, value); }
        }

        /// <summary>
        /// Identifies the PrimaryButtonCommand dependency property.
        /// </summary>
        /// 
        /// <returns>
        /// The identifier for the PrimaryButtonCommand dependency property.
        /// </returns>
        public static readonly DependencyProperty PrimaryButtonCommandProperty = DependencyProperty.Register(
            "PrimaryButtonCommand",
            typeof(ICommand),
            typeof(ContentDialog),
            null);

        #endregion

        #region public object PrimaryButtonCommandParameter

        /// <summary>
        /// Gets or sets the parameter to pass to the command for the primary button.
        /// </summary>
        /// 
        /// <returns>
        /// The parameter to pass to the command for the primary button. The default is null.
        /// </returns>
        public object PrimaryButtonCommandParameter
        {
            get { return (object)GetValue(PrimaryButtonCommandParameterProperty); }
            set { SetValue(PrimaryButtonCommandParameterProperty, value); }
        }

        /// <summary>
        /// Identifies the PrimaryButtonCommandParameter dependency property.
        /// </summary>
        /// 
        /// <returns>
        /// The identifier for the PrimaryButtonCommandParameter dependency property.
        /// </returns>
        public static readonly DependencyProperty PrimaryButtonCommandParameterProperty = DependencyProperty.Register(
            "PrimaryButtonCommandParameter",
            typeof(object),
            typeof(ContentDialog),
            null);

        #endregion

        #region public bool IsPrimaryButtonEnabled

        public bool IsPrimaryButtonEnabled
        {
            get { return (bool)GetValue(IsPrimaryButtonEnabledProperty); }
            set { SetValue(IsPrimaryButtonEnabledProperty, value); }
        }

        public static readonly DependencyProperty IsPrimaryButtonEnabledProperty = DependencyProperty.Register(
            "IsPrimaryButtonEnabled",
            typeof(bool),
            typeof(ContentDialog),
            new PropertyMetadata(true));

        #endregion

        #region public string SecondaryButtonText

        /// <summary>
        /// Gets or sets the text to be displayed on the secondary button.
        /// </summary>
        /// 
        /// <returns>
        /// The text to be displayed on the secondary button. To hide this button, set the value to null or string.Empty.
        /// </returns>
        public string SecondaryButtonText
        {
            get { return (string)GetValue(SecondaryButtonTextProperty); }
            set { SetValue(SecondaryButtonTextProperty, value); }
        }

        /// <summary>
        /// Identifies the SecondaryButtonText dependency property.
        /// </summary>
        /// 
        /// <returns>
        /// The identifier for the SecondaryButtonText dependency property.
        /// </returns>
        public static readonly DependencyProperty SecondaryButtonTextProperty = DependencyProperty.Register(
            "SecondaryButtonText",
            typeof(string),
            typeof(ContentDialog),
            new PropertyMetadata(string.Empty, (d, e) => ((ContentDialog)d).OnSecondaryButtonTextChanged(e)));

        private void OnSecondaryButtonTextChanged(DependencyPropertyChangedEventArgs e)
        {
            UpdateButton2Visibility();
        }

        #endregion

        #region public ICommand SecondaryButtonCommand

        /// <summary>
        /// Gets or sets the command to invoke when the secondary button is tapped.
        /// </summary>
        /// 
        /// <returns>
        /// The command to invoke when the secondary button is tapped.
        /// </returns>
        public ICommand SecondaryButtonCommand
        {
            get { return (ICommand)GetValue(SecondaryButtonCommandProperty); }
            set { SetValue(SecondaryButtonCommandProperty, value); }
        }

        /// <summary>
        /// Identifies the SecondaryButtonCommand dependency property.
        /// </summary>
        /// 
        /// <returns>
        /// The identifier for the SecondaryButtonCommand dependency property.
        /// </returns>
        public static readonly DependencyProperty SecondaryButtonCommandProperty = DependencyProperty.Register(
            "SecondaryButtonCommand",
            typeof(ICommand),
            typeof(ContentDialog),
            null);

        #endregion

        #region public object SecondaryButtonCommandParameter

        /// <summary>
        /// Gets or sets the parameter to pass to the command for the secondary button.
        /// </summary>
        /// 
        /// <returns>
        /// The command parameter for the secondary button. The default is null.
        /// </returns>
        public object SecondaryButtonCommandParameter
        {
            get { return (object)GetValue(SecondaryButtonCommandParameterProperty); }
            set { SetValue(SecondaryButtonCommandParameterProperty, value); }
        }

        /// <summary>
        /// Identifies the SecondaryButtonCommandParameter dependency property.
        /// </summary>
        /// 
        /// <returns>
        /// The identifier for the SecondaryButtonCommandParameter dependency property.
        /// </returns>
        public static readonly DependencyProperty SecondaryButtonCommandParameterProperty = DependencyProperty.Register(
            "SecondaryButtonCommandParameter",
            typeof(object),
            typeof(ContentDialog),
            null);

        #endregion

        #region public bool IsSecondaryButtonEnabled

        public bool IsSecondaryButtonEnabled
        {
            get { return (bool)GetValue(IsSecondaryButtonEnabledProperty); }
            set { SetValue(IsSecondaryButtonEnabledProperty, value); }
        }

        public static readonly DependencyProperty IsSecondaryButtonEnabledProperty = DependencyProperty.Register(
            "IsSecondaryButtonEnabled",
            typeof(bool),
            typeof(ContentDialog),
            new PropertyMetadata(true));

        #endregion

        #region public bool FullSizeDesired

        /// <summary>
        /// Determines whether a request is being made to display the dialog full screen.
        /// </summary>
        /// 
        /// <returns>
        /// True to request that the dialog is displayed full screen; Otherwise, false. The default is false.
        /// </returns>
        public bool FullSizeDesired
        {
            get { return (bool)GetValue(FullSizeDesiredProperty); }
            set { SetValue(FullSizeDesiredProperty, value); }
        }

        /// <summary>
        /// Identifies the FullSizeDesired dependency property.
        /// </summary>
        /// 
        /// <returns>
        /// The identifier for the FullSizeDesired dependency property.
        /// </returns>
        public static readonly DependencyProperty FullSizeDesiredProperty = DependencyProperty.Register(
            "FullSizeDesired",
            typeof(bool),
            typeof(ContentDialog),
            new PropertyMetadata((d, e) => ((ContentDialog)d).OnFullSizeDesiredChanged(e)));

        private void OnFullSizeDesiredChanged(DependencyPropertyChangedEventArgs e)
        {
            ApplyFullSizeDesired();
        }

        private void ApplyFullSizeDesired()
        {
            if (LayoutRoot != null)
            {
                LayoutRoot.VerticalAlignment = FullSizeDesired ? VerticalAlignment.Stretch : VerticalAlignment.Top;
            }

            if (Button1Host != null)
            {
                Grid.SetRow(Button1Host, FullSizeDesired ? 3 : 2);
            }

            if (Button2Host != null)
            {
                Grid.SetRow(Button2Host, FullSizeDesired ? 3 : 2);
            }
        }

        #endregion

        private FrameworkElement Container { get; set; }

        private FrameworkElement LayoutRoot { get; set; }

        private Border Button1Host { get; set; }

        private Border Button2Host { get; set; }

        private Button Button1 { get; set; }

        private Button Button2 { get; set; }

        /// <summary>
        /// Occurs after the dialog is opened.
        /// </summary>
        public event EventHandler<ContentDialogOpenedEventArgs> Opened;

        public event EventHandler<ContentDialogClosingEventArgs> Closing;

        /// <summary>
        /// Occurs after the dialog is closed.
        /// </summary>
        public event EventHandler<ContentDialogClosedEventArgs> Closed;

        /// <summary>
        /// Occurs after the primary button has been tapped.
        /// </summary>
        public event EventHandler<ContentDialogButtonClickEventArgs> PrimaryButtonClick;

        /// <summary>
        /// Occurs after the secondary button has been tapped.
        /// </summary>
        public event EventHandler<ContentDialogButtonClickEventArgs> SecondaryButtonClick;

        /// <summary>
        /// Builds the visual tree for the control when a new template is applied.
        /// </summary>
        public override void OnApplyTemplate()
        {
            if (Button1 != null)
            {
                Button1.Click -= OnButton1Click;
            }

            if (Button2 != null)
            {
                Button2.Click -= OnButton2Click;
            }

            base.OnApplyTemplate();

            Container = GetTemplateChild(ContainerName) as FrameworkElement;
            LayoutRoot = GetTemplateChild(LayoutRootName) as FrameworkElement;
            Button1Host = GetTemplateChild(Button1HostName) as Border;
            Button2Host = GetTemplateChild(Button2HostName) as Border;

            if (Container != null)
            {
                if (Container.ReadLocalValue(FlowDirectionProperty) == DependencyProperty.UnsetValue)
                {
                    Container.FlowDirection = this.GetUsefulFlowDirection();
                }
            }

            if (LayoutRoot != null)
            {
                AnimationHelper.PrepareForCompositor(LayoutRoot);
                LayoutRoot.Opacity = 0;
            }

            if (Button1Host != null)
            {
                Button1 = Button1Host.Child as Button;
            }
            else
            {
                Button1 = null;
            }

            if (Button2Host != null)
            {
                Button2 = Button2Host.Child as Button;
            }
            else
            {
                Button2 = null;
            }

            if (Button1 != null)
            {
                UpdateButton1Visibility();
                Button1.Click += OnButton1Click;
            }

            if (Button2 != null)
            {
                UpdateButton2Visibility();
                Button2.Click += OnButton2Click;
            }

            ApplyFullSizeDesired();

            SetSizeAndOffset();

            _orientationHelper.OnApplyTemplate();
        }

        /// <summary>
        /// Begins an asynchronous operation to show the dialog.
        /// </summary>
        /// 
        /// <returns>
        /// An asynchronous operation showing the dialog. When complete, returns a ContentDialogResult.
        /// </returns>
        public Task<ContentDialogResult> ShowAsync()
        {
            if (_tcs != null)
            {
                throw new InvalidOperationException(Properties.Resources.ContentDialog_AlreadyOpen);
            }

            Loaded += OnLoaded;

            _frame = Application.Current.RootVisual as PhoneApplicationFrame;
            _page = _frame.Content as PhoneApplicationPage;

            // Change the color of the system tray if necessary.
            if (SystemTray.IsVisible)
            {
                // Cache the original color of the system tray.
                _systemTrayColor = SystemTray.BackgroundColor;

                // Cache the original opacity of the system tray.
                _systemTrayOpacity = SystemTray.Opacity;

                // Change the color of the system tray to match the content dialog.
                if (Background is SolidColorBrush)
                {
                    SystemTray.BackgroundColor = ((SolidColorBrush)Background).Color;
                }
                else
                {
                    SystemTray.BackgroundColor = ((SolidColorBrush)Application.Current.Resources["PhoneChromeBrush"]).Color;
                }

                if (SystemTray.Opacity < 1)
                {
                    SystemTray.Opacity = 0;
                }
            }

            // Hide the application bar if necessary.
            if (_page.ApplicationBar != null)
            {
                // Cache the original visibility of the system tray.
                _hasApplicationBar = _page.ApplicationBar.IsVisible;

                // Hide it.
                if (_hasApplicationBar)
                {
                    _page.ApplicationBar.IsVisible = false;
                }
            }
            else
            {
                _hasApplicationBar = false;
            }

            // Close the current content dialog if there is any.
            if (_currentInstance != null)
            {
                _mustRestore = false;

                ContentDialog target = _currentInstance.Target as ContentDialog;

                if (target != null)
                {
                    _systemTrayColor = target._systemTrayColor;
                    _systemTrayOpacity = target._systemTrayOpacity;
                    _hasApplicationBar = target._hasApplicationBar;
                    target.Hide();
                }
            }

            _mustRestore = true;

            // Create and open the popup.
            _popup = new Popup();
            _popup.Child = this;
            SetSizeAndOffset();
            _popup.IsOpen = true;
            _currentInstance = new WeakReference(this);

            // Attach event handlers.
            if (_page != null)
            {
                _page.BackKeyPress += OnBackKeyPress;
                _page.OrientationChanged += OnOrientationChanged;
            }

            if (_frame != null)
            {
                _frame.Navigating += OnNavigating;
            }

            _tcs = new TaskCompletionSource<ContentDialogResult>();

            OnOpened();

            return _tcs.Task;
        }

        /// <summary>
        /// Hides the dialog.
        /// </summary>
        public void Hide()
        {
            if (_tcs != null)
            {
                Close(ContentDialogResult.None, true);
            }
        }

        /// <summary>
        /// Closes the content dialog.
        /// </summary>
        /// <param name="source">
        /// The source that caused the close.
        /// </param>
        /// <param name="useTransition">
        /// If true, the content dialog is closed after swiveling out.
        /// </param>
        private void Close(ContentDialogResult source, bool useTransition)
        {
            // Ensure only a single Close is being handled at a time
            if (_isBeingClosed)
            {
                return;
            }
            _isBeingClosed = true;

            // Handle the closing event.
            var handlerClosing = Closing;
            if (handlerClosing != null)
            {
                ContentDialogClosingEventArgs args = new ContentDialogClosingEventArgs(source);
                handlerClosing(this, args);

                if (args.Cancel)
                {
                    _isBeingClosed = false;
                    return;
                }
            }

            // Set the current instance to null.
            _currentInstance = null;

            // Cache this variable to avoid a race condition.
            bool restoreOriginalValues = _mustRestore;

            // Close popup.
            if (useTransition && LayoutRoot != null)
            {
                SwivelTransition transitionElement = new SwivelTransition { Mode = SwivelTransitionMode.Out };
                ITransition transition = transitionElement.GetTransition(LayoutRoot);
                transition.Completed += (s, e) =>
                {
                    transition.Stop();
                    ClosePopup(restoreOriginalValues, source);
                };
                transition.Begin();
            }
            else
            {
                ClosePopup(restoreOriginalValues, source);
            }

            _isBeingClosed = false;
        }

        /// <summary>
        /// Closes the pop up.
        /// </summary>
        private void ClosePopup(bool restoreOriginalValues, ContentDialogResult source)
        {
            TaskCompletionSource<ContentDialogResult> tcs = _tcs;
            _tcs = null;

            // Remove the popup.
            if (_popup != null)
            {
                _popup.IsOpen = false;
                _popup = null;
            }

            // If there is no other content dialog displayed.  
            if (restoreOriginalValues)
            {
                // Set the system tray back to its original 
                // color nad opacity if necessary.
                if (SystemTray.IsVisible)
                {
                    SystemTray.BackgroundColor = _systemTrayColor;
                    SystemTray.Opacity = _systemTrayOpacity;
                }

                // Bring the application bar if necessary.
                if (_hasApplicationBar)
                {
                    _hasApplicationBar = false;

                    // Application bar can be nulled during the Closed event
                    // so a null check needs to be performed here.
                    if (_page.ApplicationBar != null)
                    {
                        _page.ApplicationBar.IsVisible = true;
                    }
                }
            }

            // Dettach event handlers.
            if (_page != null)
            {
                _page.BackKeyPress -= OnBackKeyPress;
                _page.OrientationChanged -= OnOrientationChanged;
                _page = null;
            }

            if (_frame != null)
            {
                _frame.Navigating -= OnNavigating;
                _frame = null;
            }

            // Handle the closed event.
            var handlerClosed = Closed;
            if (handlerClosed != null)
            {
                ContentDialogClosedEventArgs args = new ContentDialogClosedEventArgs(source);
                handlerClosed(this, args);
            }

            if (tcs != null)
            {
                tcs.TrySetResult(source);
            }
        }

        private void OnOpened()
        {
            // Handle the opened event.
            var handlerOpened = Opened;
            if (handlerOpened != null)
            {
                ContentDialogOpenedEventArgs args = new ContentDialogOpenedEventArgs();
                handlerOpened(this, args);
            }
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            ApplyTemplate();

            if (LayoutRoot != null)
            {
                AnimationHelper.InvokeOnSecondRendering(() =>
                    {
                        SwivelTransition transitionElement = new SwivelTransition { Mode = SwivelTransitionMode.In };
                        ITransition transition = transitionElement.GetTransition(LayoutRoot);
                        transition.Completed += (s1, e1) =>
                        {
                            transition.Stop();
                            LayoutRoot.Opacity = 1;
                        };
                        transition.Begin();
                    });
            }

            Loaded -= OnLoaded;
        }

        /// <summary>
        /// Called when the back key is pressed. This event handler cancels
        /// the backward navigation and closes the content dialog.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event information.</param>
        private void OnBackKeyPress(object sender, CancelEventArgs e)
        {
            e.Cancel = true;
            Close(ContentDialogResult.None, true);
        }

        /// <summary>
        /// Called when the application frame is navigating.
        /// This event handler closes the content dialog.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event information.</param>
        private void OnNavigating(object sender, NavigatingCancelEventArgs e)
        {
            Close(ContentDialogResult.None, false);
        }

        private void UpdateButton1Visibility()
        {
            if (Button1 != null)
            {
                Button1.Visibility = GetVisibilityFromString(PrimaryButtonText);
            }
        }

        private void UpdateButton2Visibility()
        {
            if (Button2 != null)
            {
                Button2.Visibility = GetVisibilityFromString(SecondaryButtonText);
            }
        }

        private void OnButton1Click(object sender, RoutedEventArgs e)
        {
            if (RaiseButtonClick(PrimaryButtonClick))
            {
                return;
            }

            Close(ContentDialogResult.Primary, true);
        }

        private void OnButton2Click(object sender, RoutedEventArgs e)
        {
            if (RaiseButtonClick(SecondaryButtonClick))
            {
                return;
            }

            Close(ContentDialogResult.Secondary, true);
        }

        private bool RaiseButtonClick(EventHandler<ContentDialogButtonClickEventArgs> handler)
        {
            if (handler != null)
            {
                var args = new ContentDialogButtonClickEventArgs();
                handler(this, args);

                if (args.Cancel)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Called when the current page changes orientation.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event information.</param>
        private void OnOrientationChanged(object sender, OrientationChangedEventArgs e)
        {
            SetSizeAndOffset();
        }

        /// <summary>
        /// Sets The vertical and horizontal offset of the popup,
        /// as well as the size of its child container.
        /// </summary>
        private void SetSizeAndOffset()
        {
            // Set the size the container.
            Rect client = GetTransformedRect();
            if (Container != null)
            {
                Container.RenderTransform = GetTransform();

                Container.Width = client.Width;
                Container.Height = client.Height;
            }
        }

        /// <summary>
        /// Gets a rectangle that occupies the entire page.
        /// </summary>
        /// <returns>The width, height and location of the rectangle.</returns>
        private static Rect GetTransformedRect()
        {
            bool isLandscape = IsLandscape(GetPageOrientation());

            return new Rect(0, 0,
                isLandscape ? _screenHeight : _screenWidth,
                isLandscape ? _screenWidth : _screenHeight);
        }

        /// <summary>
        /// Determines whether the orientation is landscape.
        /// </summary>
        /// <param name="orientation">The orientation.</param>
        /// <returns>True if the orientation is landscape.</returns>
        private static bool IsLandscape(PageOrientation orientation)
        {
            return (orientation == PageOrientation.Landscape) || (orientation == PageOrientation.LandscapeLeft) || (orientation == PageOrientation.LandscapeRight);
        }

        /// <summary>
        /// Gets a transform for popup elements based
        /// on the current page orientation.
        /// </summary>
        /// <returns>
        /// A composite transform determined by page orientation.
        /// </returns>
        private static Transform GetTransform()
        {
            PageOrientation orientation = GetPageOrientation();

            switch (orientation)
            {
                case PageOrientation.LandscapeLeft:
                case PageOrientation.Landscape:
                    return new CompositeTransform() { Rotation = 90, TranslateX = _screenWidth };
                case PageOrientation.LandscapeRight:
                    return new CompositeTransform() { Rotation = -90, TranslateY = _screenHeight };
                default:
                    return null;
            }
        }

        /// <summary>
        /// Gets the current page orientation.
        /// </summary>
        /// <returns>
        /// The current page orientation.
        /// </returns>
        private static PageOrientation GetPageOrientation()
        {
            PhoneApplicationFrame frame = Application.Current.RootVisual as PhoneApplicationFrame;

            if (frame != null)
            {
                PhoneApplicationPage page = frame.Content as PhoneApplicationPage;

                if (page != null)
                {
                    return page.Orientation;
                }
            }

            return PageOrientation.None;
        }

        /// <summary>
        /// Returns a visibility value based on the value of a string.
        /// </summary>
        /// <param name="str">The string.</param>
        /// <returns>
        /// Visibility.Collapsed if the string is null or empty.
        /// Visibility.Visible otherwise.
        /// </returns>
        private static Visibility GetVisibilityFromString(string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return Visibility.Collapsed;
            }
            else
            {
                return Visibility.Visible;
            }
        }
    }
}
