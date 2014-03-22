using Microsoft.Phone.Controls.LocalizedResources;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace Microsoft.Phone.Controls
{
    /// <summary>
    /// Represents the UI for a property.
    /// </summary>
    [TemplatePart(Name = ElementButtonName, Type = typeof(ButtonBase))]
    [TemplateVisualState(Name = StateInteractive, GroupName = GroupIsInteractive)]
    [TemplateVisualState(Name = StateNonInteractive, GroupName = GroupIsInteractive)]
    [StyleTypedProperty(Property = ContextMenuStylePropertyName, StyleTargetType = typeof(ContextMenu))]
    public class PropertyUI : HeaderedContentControl
    {
        private const string ElementButtonName = "Button";

        private const string GroupIsInteractive = "IsInteractiveStates";
        private const string StateInteractive = "Interactive";
        private const string StateNonInteractive = "NonInteractive";

        private const string ContextMenuStylePropertyName = "ContextMenuStyle";

        private ContextMenu _contextMenu;
        private MenuItem _contextMenuCopy;

        /// <summary>
        /// Initializes a new instance of the PropertyUI class.
        /// </summary>
        public PropertyUI()
        {
            DefaultStyleKey = typeof(PropertyUI);
        }

        #region public ICommand Command

        /// <summary>
        /// Gets or sets the command associated with the PropertyUI.
        /// </summary>
        public ICommand Command
        {
            get { return (ICommand)GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }

        /// <summary>
        /// Identifies the Command dependency property.
        /// </summary>
        public static readonly DependencyProperty CommandProperty = DependencyProperty.Register(
            "Command",
            typeof(ICommand),
            typeof(PropertyUI),
            null);

        #endregion

        #region public object CommandParameter

        /// <summary>
        /// Gets or sets the parameter to pass to the Command property of a PropertyUI.
        /// </summary>
        public object CommandParameter
        {
            get { return (object)GetValue(CommandParameterProperty); }
            set { SetValue(CommandParameterProperty, value); }
        }

        /// <summary>
        /// Identifies the CommandParameter dependency property.
        /// </summary>
        public static readonly DependencyProperty CommandParameterProperty = DependencyProperty.Register(
            "CommandParameter",
            typeof(object),
            typeof(PropertyUI),
            null);

        #endregion

        #region public bool IsInteractive

        /// <summary>
        /// Gets or sets a value that indicates whether the control raises a Click event.
        /// </summary>
        /// 
        /// <returns>
        /// true if the control raises a Click event; otherwise, false. The default is true.
        /// </returns>
        public bool IsInteractive
        {
            get { return (bool)GetValue(IsInteractiveProperty); }
            set { SetValue(IsInteractiveProperty, value); }
        }

        /// <summary>
        /// Identifies the IsInteractive dependency property.
        /// </summary>
        public static readonly DependencyProperty IsInteractiveProperty = DependencyProperty.Register(
            "IsInteractive",
            typeof(bool),
            typeof(PropertyUI),
            new PropertyMetadata(true, (d, e) => ((PropertyUI)d).OnIsInteractiveChanged()));

        private void OnIsInteractiveChanged()
        {
            ChangeVisualState(true);
        }

        #endregion

        #region public bool IsContextMenuEnabled

        /// <summary>
        /// Gets or sets a value that indicates whether the context menu is enabled.
        /// </summary>
        /// 
        /// <returns>
        /// true if the context menu is enabled; otherwise, false. The default is true.
        /// </returns>
        public bool IsContextMenuEnabled
        {
            get { return (bool)GetValue(IsContextMenuEnabledProperty); }
            set { SetValue(IsContextMenuEnabledProperty, value); }
        }

        /// <summary>
        /// Identifies the IsContextMenuEnabled dependency property.
        /// </summary>
        public static readonly DependencyProperty IsContextMenuEnabledProperty = DependencyProperty.Register(
            "IsContextMenuEnabled",
            typeof(bool),
            typeof(PropertyUI),
            new PropertyMetadata(true, (d, e) => ((PropertyUI)d).UpdateContextMenu()));

        private void InitializeContextMenu()
        {
            if (_contextMenu == null)
            {
                _contextMenuCopy = new MenuItem
                {
                    Header = ControlResources.Copy
                };
                _contextMenuCopy.Click += OnContextMenuCopyClick;

                _contextMenu = new ContextMenu
                {
                    Items = { _contextMenuCopy }
                };
                _contextMenu.Opened += OnContextMenuOpened;

                ApplyContextMenuStyle();
            }
        }

        private void UpdateContextMenu()
        {
            if (IsContextMenuEnabled)
            {
                InitializeContextMenu();
                ContextMenuService.SetContextMenu(this, _contextMenu);
            }
            else
            {
                if (_contextMenu != null && ContextMenuService.GetContextMenu(this) == _contextMenu)
                {
                    ClearValue(ContextMenuService.ContextMenuProperty);
                }
            }
        }

        private void OnContextMenuOpened(object sender, RoutedEventArgs e)
        {
            FrameworkElement el = _contextMenu.Owner as FrameworkElement;
            UIElement rootVisual = Application.Current.RootVisual;
            if (el != null && rootVisual != null)
            {
                GeneralTransform t = el.TransformToVisual(rootVisual);

                Rect roi = new Rect(t.Transform(new Point()), new Size(el.ActualWidth, el.ActualHeight));
                roi.Height -= Padding.Bottom;

                _contextMenu.RegionOfInterest = roi;
            }
            else
            {
                _contextMenu.ClearValue(ContextMenu.RegionOfInterestProperty);
            }
        }

        private void OnContextMenuCopyClick(object sender, RoutedEventArgs e)
        {
            if (Content != null)
            {
                Clipboard.SetText(Content.ToString());
            }
        }

        #endregion

        #region public Style ContextMenuStyle

        /// <summary>
        /// Gets or sets the style that is used when rendering the context menu.
        /// </summary>
        public Style ContextMenuStyle
        {
            get { return (Style)GetValue(ContextMenuStyleProperty); }
            set { SetValue(ContextMenuStyleProperty, value); }
        }

        /// <summary>
        /// Identifies the ContextMenuStyleProperty dependency property.
        /// </summary>
        public static readonly DependencyProperty ContextMenuStyleProperty = DependencyProperty.Register(
            ContextMenuStylePropertyName,
            typeof(Style),
            typeof(PropertyUI),
            new PropertyMetadata((d, e) => ((PropertyUI)d).OnContextMenuStyleChanged()));

        private void OnContextMenuStyleChanged()
        {
            ApplyContextMenuStyle();
        }

        private void ApplyContextMenuStyle()
        {
            if (_contextMenu != null)
            {
                _contextMenu.Style = ContextMenuStyle;
            }
        }

        #endregion

        private ButtonBase ElementButton { get; set; }

        /// <summary>
        /// Occurs when a PropertyUI is clicked.
        /// </summary>
        public event RoutedEventHandler Click;

        /// <summary>
        /// Called when the template's tree is generated.
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (ElementButton != null)
            {
                ElementButton.Click -= OnButtonClick;
            }

            ElementButton = GetTemplateChild(ElementButtonName) as ButtonBase;

            if (ElementButton != null)
            {
                ElementButton.Click += OnButtonClick;
            }

            UpdateContextMenu();

            ChangeVisualState(false);
        }

        private void OnButtonClick(object sender, RoutedEventArgs e)
        {
            RoutedEventHandler handler = Click;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        private void ChangeVisualState(bool useTransitions)
        {
            VisualStateManager.GoToState(this, IsInteractive ? StateInteractive : StateNonInteractive, useTransitions);
        }
    }
}
