using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Media;

namespace Microsoft.Phone.Controls.Primitives
{
    /// <summary>
    /// Represents a wait cursor in Windows Phone applications.
    /// </summary>
    [TemplateVisualState(GroupName = VisualStateGroupName, Name = NormalState)]
    [TemplateVisualState(GroupName = VisualStateGroupName, Name = HiddenState)]
    public sealed class WaitCursorControl : Control
    {
        #region Visual States
        private const string VisualStateGroupName = "VisibilityStates";
        private const string NormalState = "Normal";
        private const string HiddenState = "Hidden";
        #endregion

        private bool _isLoaded;

        private Popup _popup;

        private VisualStateGroup _visualStateGroup;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Microsoft.Phone.Controls.WaitCursorControl"/> class.
        /// </summary>
        public WaitCursorControl()
        {
            DefaultStyleKey = typeof(WaitCursorControl);

            _popup = new Popup { Child = this };
            _popup.Opened += OnPopupOpened;

            Loaded += OnLoaded;
        }

        #region public bool IsVisible

        /// <summary>
        /// Gets or sets a value that indicates whether the wait cursor is visible.
        /// </summary>
        /// 
        /// <returns>
        /// true if the wait cursor is visible; otherwise, false.
        /// </returns>
        public bool IsVisible
        {
            get { return (bool)GetValue(IsVisibleProperty); }
            set { SetValue(IsVisibleProperty, value); }
        }

        /// <summary>
        /// The dependency property for <see cref="P:Microsoft.Phone.Controls.WaitCursorControl.IsVisible"/>.
        /// </summary>
        public static readonly DependencyProperty IsVisibleProperty = DependencyProperty.Register(
            "IsVisible",
            typeof(bool),
            typeof(WaitCursorControl),
            new PropertyMetadata(false, OnIsVisibleChanged));

        private static void OnIsVisibleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((WaitCursorControl)d).OnIsVisibleChanged(e);
        }

        private void OnIsVisibleChanged(DependencyPropertyChangedEventArgs e)
        {
            var frame = Application.Current.RootVisual as PhoneApplicationFrame;

            if ((bool)e.NewValue)
            {
                if (frame != null)
                {
                    frame.OrientationChanged += OnOrientationChanged;
                }

                SetOrientation();

                ClosePopup();
                OpenPopup();
            }
            else
            {
                if (frame != null)
                {
                    frame.OrientationChanged -= OnOrientationChanged;
                }

                UpdateVisualStates(true);
            }
        }

        #endregion

        #region public string Text

        /// <summary>
        /// Gets or sets the text of the wait cursor.
        /// </summary>
        /// 
        /// <returns>
        /// The text of the wait cursor.
        /// </returns>
        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        /// <summary>
        /// The dependency property for <see cref="P:Microsoft.Phone.Controls.WaitCursorControl.Text"/>.
        /// </summary>
        public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
            "Text",
            typeof(string),
            typeof(WaitCursorControl),
            new PropertyMetadata(string.Empty));

        #endregion

        #region internal FlowDirection ImplementationRootFlowDirection

        internal FlowDirection ImplementationRootFlowDirection
        {
            get { return (FlowDirection)GetValue(ImplementationRootFlowDirectionProperty); }
            set { SetValue(ImplementationRootFlowDirectionProperty, value); }
        }

        private static readonly DependencyProperty ImplementationRootFlowDirectionProperty = DependencyProperty.Register(
            "ImplementationRootFlowDirection",
            typeof(FlowDirection),
            typeof(WaitCursorControl),
            null);

        #endregion

        private FrameworkElement ImplementationRoot { get; set; }

        /// <summary>
        /// Builds the visual tree for the <see cref="T:Microsoft.Phone.Controls.WaitCursorControl"/> control when a new template is applied.
        /// </summary>
        public override void OnApplyTemplate()
        {
            if (ImplementationRoot != null)
            {
                ImplementationRoot.ClearValue(FlowDirectionProperty);
            }

            if (_visualStateGroup != null)
            {
                _visualStateGroup.CurrentStateChanged -= OnVisualStateChanged;
            }

            base.OnApplyTemplate();

            ImplementationRoot = VisualStates.GetImplementationRoot(this);
            _visualStateGroup = VisualStates.TryGetVisualStateGroup(this, VisualStateGroupName);

            if (ImplementationRoot != null)
            {
                ImplementationRoot.SetBinding(FlowDirectionProperty, new Binding("ImplementationRootFlowDirection") { Source = this });
            }

            if (_visualStateGroup != null)
            {
                _visualStateGroup.CurrentStateChanged += OnVisualStateChanged;
            }

            UpdateVisualStates(false);
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            Loaded -= OnLoaded;

            _isLoaded = true;
        }

        private void OnOrientationChanged(object sender, OrientationChangedEventArgs e)
        {
            SetOrientation();
        }

        private void OnVisualStateChanged(object sender, VisualStateChangedEventArgs e)
        {
            if (e.OldState != null && e.NewState != null && e.NewState.Name == HiddenState)
            {
                ClosePopup();
            }
        }

        private void UpdateVisualStates(bool useTransitions)
        {
            if (IsVisible)
            {
                OpenPopup();
            }

            VisualStateManager.GoToState(
                this,
                _isLoaded && IsVisible ? NormalState : HiddenState,
                useTransitions);
        }

        private void SetOrientation()
        {
            PhoneApplicationFrame frame = Application.Current.RootVisual as PhoneApplicationFrame;
            if (frame == null)
                return;
            PageOrientation orientation = frame.Orientation;
            Transform transform = null;
            double hostWidth = Application.Current.Host.Content.ActualWidth;
            double hostHeight = Application.Current.Host.Content.ActualHeight;
            double newWidth = hostWidth;
            double newHeight = hostHeight;
            switch (orientation)
            {
                case PageOrientation.Landscape:
                case PageOrientation.LandscapeLeft:
                    transform = new CompositeTransform
                    {
                        Rotation = 90.0,
                        TranslateX = hostWidth
                    };
                    newWidth = hostHeight;
                    newHeight = hostWidth;
                    break;
                case PageOrientation.LandscapeRight:
                    transform = new CompositeTransform
                    {
                        Rotation = -90.0,
                        TranslateY = hostHeight
                    };
                    newWidth = hostHeight;
                    newHeight = hostWidth;
                    break;
            }
            RenderTransform = transform;
            Width = newWidth;
            Height = newHeight;
        }

        private void OpenPopup()
        {
            if (!_popup.IsOpen)
            {
                _popup.IsOpen = true;
            }
        }

        private void ClosePopup()
        {
            if (_popup.IsOpen)
            {
                _popup.IsOpen = false;
            }
        }

        private void OnPopupOpened(object sender, EventArgs e)
        {
            if (IsVisible)
            {
                UpdateVisualStates(true);
            }
            else
            {
                ClosePopup();
            }
        }
    }
}
