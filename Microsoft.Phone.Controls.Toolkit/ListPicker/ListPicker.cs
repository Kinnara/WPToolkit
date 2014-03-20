// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Navigation;
using Microsoft.Phone.Controls.Primitives;

namespace Microsoft.Phone.Controls
{
    /// <summary>
    /// Class that implements a flexible list-picking experience with a custom interface for few/many items.
    /// </summary>
    /// <QualityBand>Preview</QualityBand>
    [TemplatePart(Name = ItemsPresenterPartName, Type = typeof(ItemsPresenter))]
    [TemplatePart(Name = ItemsPresenterTranslateTransformPartName, Type = typeof(TranslateTransform))]
    [TemplatePart(Name = ItemsPresenterHostPartName, Type = typeof(Canvas))]
    [TemplatePart(Name = ButtonPartName, Type = typeof(ButtonBase))]
    [TemplateVisualState(GroupName = VisualStates.GroupCommon, Name = VisualStates.StateNormal)]
    [TemplateVisualState(GroupName = VisualStates.GroupCommon, Name = StateHighlighted)]
    [TemplateVisualState(GroupName = VisualStates.GroupCommon, Name = VisualStates.StateDisabled)]
    [TemplateVisualState(GroupName = GroupPresenter, Name = StateInlineNormal)]
    [TemplateVisualState(GroupName = GroupPresenter, Name = StateInlinePlaceholder)]
    public class ListPicker : SimpleSelector
    {
        private const string ItemsPresenterPartName = "ItemsPresenter";
        private const string ItemsPresenterTranslateTransformPartName = "ItemsPresenterTranslateTransform";
        private const string ItemsPresenterHostPartName = "ItemsPresenterHost";
        private const string ButtonPartName = "Button";

        private const string StateHighlighted = "Highlighted";

        private const string GroupPresenter = "PresenterStates";
        private const string StateInlineNormal = "InlineNormal";
        private const string StateInlinePlaceholder = "InlinePlaceholder";

        /// <summary>
        /// In Mango, the size of list pickers in expanded mode was given extra offset.
        /// </summary>
        private const double NormalModeOffset = 10;

        private static readonly Duration AnimationDuration = TimeSpan.FromSeconds(0.25);

        private readonly DoubleAnimation _heightAnimation = new DoubleAnimation();
        private readonly DoubleAnimation _translateAnimation = new DoubleAnimation();
        private readonly Storyboard _storyboard = new Storyboard();

        private UIElement _root;
        private PhoneApplicationFrame _frame;
        private PhoneApplicationPage _page;
        private FrameworkElement _itemsPresenterHostParent;
        private Canvas _itemsPresenterHostPart;
        private ItemsPresenter _itemsPresenterPart;
        private TranslateTransform _itemsPresenterTranslateTransformPart;
        private ButtonBase _buttonPart;

        #region public string PlaceholderText

        /// <summary>
        /// Gets or sets the text that is displayed in the control until the value is changed by a user action or some other operation.
        /// </summary>
        /// 
        /// <returns>
        /// The text that is displayed in the control when no value is selected. The default is an empty string ("").
        /// </returns>
        public string PlaceholderText
        {
            get { return (string)GetValue(PlaceholderTextProperty); }
            set { SetValue(PlaceholderTextProperty, value); }
        }

        /// <summary>
        /// Identifies the PlaceholderText dependency property.
        /// </summary>
        /// 
        /// <returns>
        /// The identifier for the PlaceholderText dependency property.
        /// </returns>
        public static readonly DependencyProperty PlaceholderTextProperty = DependencyProperty.Register(
            "PlaceholderText",
            typeof(string),
            typeof(ListPicker),
            new PropertyMetadata(string.Empty, (d, e) => ((ListPicker)d).OnPlaceholderTextChanged(e)));

        private void OnPlaceholderTextChanged(DependencyPropertyChangedEventArgs e)
        {
            UpdateVisualStates(false);
        }

        #endregion

        /// <summary>
        /// Gets or sets the flag that indicates whether the ListPicker is expanded.
        /// </summary>
        public bool IsExpanded
        {
            get { return (bool)GetValue(IsExpandedProperty); }
            private set { SetValue(IsExpandedProperty, value); }
        }

        /// <summary>
        /// Identifies the IsExpanded DependencyProperty.
        /// </summary>
        public static readonly DependencyProperty IsExpandedProperty =
            DependencyProperty.Register("IsExpanded", typeof(bool), typeof(ListPicker), new PropertyMetadata(OnIsExpandedChanged));

        private static void OnIsExpandedChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            ((ListPicker)o).OnIsExpandedChanged((bool)e.OldValue, (bool)e.NewValue);
        }

        private void OnIsExpandedChanged(bool oldValue, bool newValue)
        {
            if (oldValue)
            {
                if (null != _root)
                {
                    _root.Tap -= OnRootTap;
                    _root = null;
                }

                if (null != _page)
                {
                    _page.BackKeyPress -= OnPageBackKeyPress;
                    _page = null;
                }

                if (null != _frame)
                {
                    _frame.Navigated -= OnFrameNavigated;
                    _frame = null;
                }

                if (null != _itemsPresenterPart)
                {
                    _itemsPresenterPart.IsHitTestVisible = false;
                }
            }

            if (newValue)
            {
                if (null == _root)
                {
                    _root = this.GetVisualAncestors().LastOrDefault();
                    if (null != _root)
                    {
                        _root.AddHandler(TapEvent, new EventHandler<System.Windows.Input.GestureEventArgs>(OnRootTap), true);
                    }
                }

                // Hook up to frame if not already done
                if (null == _frame)
                {
                    _frame = Application.Current.RootVisual as PhoneApplicationFrame;
                    if (null != _frame)
                    {
                        _frame.Navigated += OnFrameNavigated;
                    }
                }

                if (null != _frame)
                {
                    _page = _frame.Content as PhoneApplicationPage;
                    if (null != _page)
                    {
                        _page.BackKeyPress += OnPageBackKeyPress;
                    }
                }

                if (null != _itemsPresenterPart)
                {
                    _itemsPresenterPart.IsHitTestVisible = true;
                }

                TiltEffect.UpdateCurrentTiltEffectReturnAnimationDuration(null, AnimationDuration);
            }

            if (null != _buttonPart)
            {
                TiltEffect.SetSuppressTilt(_buttonPart, newValue);
            }

            UpdateVisualStates(true);
            SizeForAppropriateView(true);
            IsHighlighted = newValue;
        }


        /// <summary>
        /// Whether the list picker is highlighted.
        /// This occurs when the user is manipulating the box or when in expanded mode.
        /// </summary>
        private bool IsHighlighted
        {
            get { return (bool)GetValue(IsHighlightedProperty); }
            set { SetValue(IsHighlightedProperty, value); }
        }

        private static readonly DependencyProperty IsHighlightedProperty =
            DependencyProperty.Register("IsHighlighted",
                                        typeof(bool),
                                        typeof(ListPicker),
                                        new PropertyMetadata(false, new PropertyChangedCallback(OnIsHighlightedChanged)));

        /// <summary>
        /// Highlight property changed
        /// </summary>
        private static void OnIsHighlightedChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            (o as ListPicker).OnIsHighlightedChanged();
        }

        /// <summary>
        /// Highlight property changed
        /// </summary>
        private void OnIsHighlightedChanged()
        {
            UpdateVisualStates(true);
        }


        private static readonly DependencyProperty IsPressedProperty = DependencyProperty.Register(
            "IsPressed",
            typeof(bool),
            typeof(ListPicker),
            new PropertyMetadata((d, e) => ((ListPicker)d).OnIsPressedChanged(e)));

        private void OnIsPressedChanged(DependencyPropertyChangedEventArgs e)
        {
            if (!IsExpanded)
            {
                IsHighlighted = (bool)e.NewValue;
            }
        }


        /// <summary>
        /// Enabled property changed
        /// </summary>
        private void OnIsEnabledChanged()
        {
            UpdateVisualStates(true);
        }

        /// <summary>
        /// Gets or sets the header of the control.
        /// </summary>
        public object Header
        {
            get { return (object)GetValue(HeaderProperty); }
            set { SetValue(HeaderProperty, value); }
        }

        /// <summary>
        /// Identifies the Header DependencyProperty.
        /// </summary>
        public static readonly DependencyProperty HeaderProperty =
            DependencyProperty.Register("Header", typeof(object), typeof(ListPicker), null);

        /// <summary>
        /// Gets or sets the template used to display the control's header.
        /// </summary>
        public DataTemplate HeaderTemplate
        {
            get { return (DataTemplate)GetValue(HeaderTemplateProperty); }
            set { SetValue(HeaderTemplateProperty, value); }
        }

        /// <summary>
        /// Identifies the HeaderTemplate DependencyProperty.
        /// </summary>
        public static readonly DependencyProperty HeaderTemplateProperty =
            DependencyProperty.Register("HeaderTemplate", typeof(DataTemplate), typeof(ListPicker), null);

        /// <summary>
        /// Initializes a new instance of the ListPicker class.
        /// </summary>
        public ListPicker()
        {
            DefaultStyleKey = typeof(ListPicker);

            CacheMode = new BitmapCache();

            Storyboard.SetTargetProperty(_heightAnimation, new PropertyPath(FrameworkElement.HeightProperty));
            Storyboard.SetTargetProperty(_translateAnimation, new PropertyPath(TranslateTransform.YProperty));

            // Would be nice if these values were customizable (ex: as DependencyProperties or in Template as VSM states)
            Duration duration = AnimationDuration;
            _heightAnimation.Duration = duration;
            _translateAnimation.Duration = duration;
            IEasingFunction easingFunction = new ExponentialEase { EasingMode = EasingMode.EaseInOut, Exponent = 3 };
            _heightAnimation.EasingFunction = easingFunction;
            _translateAnimation.EasingFunction = easingFunction;

            IsEnabledChanged += delegate { OnIsEnabledChanged(); };

            Loaded += OnLoaded;
            Unloaded += OnUnloaded;

            SizeChanged += OnFirstSizeChanged;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            UpdateVisualStates(false);
            SizeForAppropriateView(false);
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            if (null != _root)
            {
                _root.Tap -= OnRootTap;
                _root = null;
            }

            // Unhook any remaining event handlers
            if (null != _frame)
            {
                _frame.Navigated -= OnFrameNavigated;
                _frame = null;
            }
        }

        private void OnFirstSizeChanged(object sender, SizeChangedEventArgs e)
        {
            SizeChanged -= OnFirstSizeChanged;

            UpdateVisualStates(false);
        }

        /// <summary>
        /// Builds the visual tree for the control when a new template is applied.
        /// </summary>
        public override void OnApplyTemplate()
        {
            // Unhook from old elements
            if (null != _itemsPresenterHostParent)
            {
                _itemsPresenterHostParent.SizeChanged -= OnItemsPresenterHostParentSizeChanged;
            }
            if (null != _buttonPart)
            {
                _buttonPart.Click -= OnButtonClick;
                ClearValue(IsPressedProperty);
            }
            _storyboard.Stop();

            base.OnApplyTemplate();

            // Hook up to new elements
            _itemsPresenterPart = GetTemplateChild(ItemsPresenterPartName) as ItemsPresenter;
            _itemsPresenterTranslateTransformPart = GetTemplateChild(ItemsPresenterTranslateTransformPartName) as TranslateTransform;
            _itemsPresenterHostPart = GetTemplateChild(ItemsPresenterHostPartName) as Canvas;
            _itemsPresenterHostParent = (null != _itemsPresenterHostPart) ? _itemsPresenterHostPart.Parent as FrameworkElement : null;
            _buttonPart = GetTemplateChild(ButtonPartName) as ButtonBase;

            if (null != _itemsPresenterHostParent)
            {
                _itemsPresenterHostParent.SizeChanged += OnItemsPresenterHostParentSizeChanged;
            }
            if (null != _itemsPresenterHostPart)
            {
                Storyboard.SetTarget(_heightAnimation, _itemsPresenterHostPart);
                if (!_storyboard.Children.Contains(_heightAnimation))
                {
                    _storyboard.Children.Add(_heightAnimation);
                }
            }
            else
            {
                if (_storyboard.Children.Contains(_heightAnimation))
                {
                    _storyboard.Children.Remove(_heightAnimation);
                }
            }
            if (null != _itemsPresenterTranslateTransformPart)
            {
                Storyboard.SetTarget(_translateAnimation, _itemsPresenterTranslateTransformPart);
                if (!_storyboard.Children.Contains(_translateAnimation))
                {
                    _storyboard.Children.Add(_translateAnimation);
                }
            }
            else
            {
                if (_storyboard.Children.Contains(_translateAnimation))
                {
                    _storyboard.Children.Remove(_translateAnimation);
                }
            }
            if (null != _buttonPart)
            {
                _buttonPart.Click += OnButtonClick;
                SetBinding(IsPressedProperty, new Binding("IsPressed") { Source = _buttonPart });
            }
        }

        /// <summary>
        /// Creates or identifies the element that is used to display the given item.
        /// </summary>
        /// 
        /// <returns>
        /// The element that is used to display the given item.
        /// </returns>
        protected override DependencyObject GetContainerForItemOverride()
        {
            ListPickerItem container = new ListPickerItem();
            container.CacheMode = new BitmapCache();
            return container;
        }

        /// <summary>
        /// Prepares the specified element to display the specified item.
        /// </summary>
        /// <param name="element">The element used to display the specified item.</param>
        /// <param name="item">The item to display.</param>
        protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
        {
            base.PrepareContainerForItemOverride(element, item);

            // Hook up to interesting events
            ContentControl container = (ContentControl)element;
            container.Tap += OnContainerTap;
            container.SizeChanged += OnListPickerItemSizeChanged;

            // Size for selected item if it's this one
            if (object.Equals(item, SelectedItem))
            {
                SizeForAppropriateView(false);
            }
        }

        /// <summary>
        /// Undoes the effects of the PrepareContainerForItemOverride method.
        /// </summary>
        /// <param name="element">The container element.</param>
        /// <param name="item">The item.</param>
        protected override void ClearContainerForItemOverride(DependencyObject element, object item)
        {
            base.ClearContainerForItemOverride(element, item);

            // Unhook from events
            ContentControl container = (ContentControl)element;
            container.Tap -= OnContainerTap;
            container.SizeChanged -= OnListPickerItemSizeChanged;
        }

        /// <summary>
        /// Provides handling for the ItemContainerGenerator.ItemsChanged event.
        /// </summary>
        /// <param name="e">A NotifyCollectionChangedEventArgs that contains the event data.</param>
        protected override void OnItemsChanged(NotifyCollectionChangedEventArgs e)
        {
            base.OnItemsChanged(e);

            // Translate it into view once layout has been updated for the added/removed item(s)
            Dispatcher.BeginInvoke(() => SizeForAppropriateView(false));
        }

        internal override void OnSelectionChanged(int oldIndex, int newIndex, object oldValue, object newValue)
        {
            base.OnSelectionChanged(oldIndex, newIndex, oldValue, newValue);

            // Switch to Normal mode or size for current item
            if (IsExpanded)
            {
                IsExpanded = false;
            }
            else
            {
                SizeForAppropriateView(false);
            }
        }

        internal override bool OnSelectorItemClicked(SimpleSelectorItem item)
        {
            return false;
        }

        internal override void NotifySelectorItemSelected(SimpleSelectorItem selectorItem, bool isSelected)
        {
        }

        /// <summary>
        /// Opens the picker for selection into Expanded mode.
        /// </summary>
        /// <returns>Whether the picker was succesfully opened.</returns>
        public bool Open()
        {
            // On interaction, switch to Expanded mode
            if (!IsExpanded)
            {
                IsExpanded = true;
                return true;
            }

            return false;
        }

        private void OnButtonClick(object sender, RoutedEventArgs e)
        {
            Open();
        }

        private void OnItemsPresenterHostParentSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (null != _itemsPresenterPart && null != _itemsPresenterHostPart && (e.NewSize.Width != e.PreviousSize.Width || e.NewSize.Width == 0))
            {
                // The control size has changed and we need to update the items presenter's size as well
                // as its host's size (the canvas).
                UpdateItemsPresenterWidth(e.NewSize.Width);
            }

            // Update clip to show only the selected item in Normal mode
            _itemsPresenterHostParent.Clip = new RectangleGeometry { Rect = new Rect(new Point(), e.NewSize) };
        }

        private void UpdateItemsPresenterWidth(double availableWidth)
        {
            // First, we clear everthing and we measure the items presenter desired size.
            _itemsPresenterPart.Width = _itemsPresenterHostPart.Width = double.NaN;
            _itemsPresenterPart.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));

            // We set the host's width to the presenter's desired width only if no explicit width is set and
            // the horizontal alignment isn't stretch (when the horizontal alignment is stretch, the canvas is
            // automatically stretched).
            if (double.IsNaN(Width) && HorizontalAlignment != HorizontalAlignment.Stretch)
            {
                _itemsPresenterHostPart.Width = _itemsPresenterPart.DesiredSize.Width;
            }

            if (availableWidth > _itemsPresenterPart.DesiredSize.Width)
                _itemsPresenterPart.Width = availableWidth;
        }

        private void OnListPickerItemSizeChanged(object sender, SizeChangedEventArgs e)
        {
            // Update size accordingly
            ContentControl container = (ContentControl)sender;
            if (object.Equals(ItemContainerGenerator.ItemFromContainer(container), SelectedItem))
            {
                SizeForAppropriateView(false);
            }

            // Updates the host's width to reflect the items presenter desired width.
            if (double.IsNaN(Width) && HorizontalAlignment != HorizontalAlignment.Stretch)
            {
                _itemsPresenterHostPart.Width = _itemsPresenterPart.DesiredSize.Width;
            }
        }

        private void OnPageBackKeyPress(object sender, CancelEventArgs e)
        {
            // Revert to Normal mode
            IsExpanded = false;
            e.Cancel = true;
        }

        private void SizeForAppropriateView(bool animate)
        {
            if (!IsExpanded)
            {
                SizeForNormalMode(animate);
            }
            else
            {
                SizeForExpandedMode();
            }

            // Play the height/translation animations
            _storyboard.Begin();
            if (!animate)
            {
                _storyboard.SkipToFill();
            }
        }

        private void SizeForNormalMode(bool animate)
        {
            ContentControl container = (ContentControl)ItemContainerGenerator.ContainerFromItem(SelectedItem ?? Items.FirstOrDefault());
            if (null != container)
            {
                // Set height/translation to show just the selected item
                if (0 < container.ActualHeight)
                {
                    SetContentHeight(container.ActualHeight + container.Margin.Top + container.Margin.Bottom - (NormalModeOffset * 2));
                }
                if (null != _itemsPresenterTranslateTransformPart)
                {
                    if (!animate)
                    {
                        _itemsPresenterTranslateTransformPart.Y = -NormalModeOffset;
                    }
                    _translateAnimation.To = container.Margin.Top - LayoutInformation.GetLayoutSlot(container).Top - NormalModeOffset;
                    _translateAnimation.From = animate ? null : _translateAnimation.To;
                }
            }
            else
            {
                // Resize to minimum height
                SetContentHeight(0);
            }

            // Clear highlight of previously selected container
            ListPickerItem oldContainer = (ListPickerItem)ItemContainerGenerator.ContainerFromIndex(SelectedIndex);
            if (null != oldContainer)
            {
                oldContainer.IsSelected = false;
            }
        }

        private void SizeForExpandedMode()
        {
            // Set height and align first element at top
            if (null != _itemsPresenterPart)
            {
                SetContentHeight(_itemsPresenterPart.ActualHeight);
            }
            if (null != _itemsPresenterTranslateTransformPart)
            {
                _translateAnimation.To = 0;
            }

            // Highlight selected container
            ListPickerItem container = (ListPickerItem)ItemContainerGenerator.ContainerFromIndex(SelectedIndex);
            if (null != container)
            {
                container.IsSelected = true;
            }
        }

        private void SetContentHeight(double height)
        {
            if ((null != _itemsPresenterHostPart) && !double.IsNaN(height))
            {
                double canvasHeight = _itemsPresenterHostPart.Height;
                _heightAnimation.From = double.IsNaN(canvasHeight) ? height : canvasHeight;
                _heightAnimation.To = height;
            }
        }

        private void OnRootTap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            if (IsExpanded)
            {
                // Manipulation outside an Expanded ListPicker reverts to Normal mode
                DependencyObject element = e.OriginalSource as DependencyObject;
                DependencyObject cancelElement = this;
                while (null != element)
                {
                    if (cancelElement == element)
                    {
                        return;
                    }
                    element = VisualTreeHelper.GetParent(element);
                }
                IsExpanded = false;
            }
        }

        private void OnFrameNavigated(object sender, NavigationEventArgs e)
        {
            if (IsExpanded)
            {
                IsExpanded = false;
            }
        }

        private void OnContainerTap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            if (IsExpanded)
            {
                // Manipulation of a container selects the item and reverts to Normal mode
                ContentControl container = (ContentControl)sender;
                SelectedItem = ItemContainerGenerator.ItemFromContainer(container);
                IsExpanded = false;
                e.Handled = true;
            }
        }

        private void UpdateVisualStates(bool useTransitions)
        {
            if (!IsEnabled)
            {
                VisualStateManager.GoToState(this, VisualStates.StateDisabled, useTransitions);
            }
            else if (IsHighlighted)
            {
                VisualStateManager.GoToState(this, StateHighlighted, useTransitions);
            }
            else
            {
                VisualStateManager.GoToState(this, VisualStates.StateNormal, useTransitions);
            }

            if (SelectedIndex == -1 && !IsExpanded)
            {
                VisualStateManager.GoToState(this, StateInlinePlaceholder, useTransitions);
            }
            else
            {
                VisualStateManager.GoToState(this, StateInlineNormal, useTransitions);
            }
        }
    }
}
