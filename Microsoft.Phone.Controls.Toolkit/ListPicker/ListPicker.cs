// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
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
    [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Justification = "This is a complicated control.")]
    [TemplatePart(Name = ItemsPresenterPartName, Type = typeof(ItemsPresenter))]
    [TemplatePart(Name = ItemsPresenterTranslateTransformPartName, Type = typeof(TranslateTransform))]
    [TemplatePart(Name = ItemsPresenterHostPartName, Type = typeof(Canvas))]
    [TemplateVisualState(GroupName = PickerStatesGroupName, Name = PickerStatesNormalStateName)]
    [TemplateVisualState(GroupName = PickerStatesGroupName, Name = PickerStatesHighlightedStateName)]
    [TemplateVisualState(GroupName = PickerStatesGroupName, Name = PickerStatesDisabledStateName)]
    public class ListPicker : TemplatedItemsControl<ListPickerItem>
    {
        private const string ItemsPresenterPartName = "ItemsPresenter";
        private const string ItemsPresenterTranslateTransformPartName = "ItemsPresenterTranslateTransform";
        private const string ItemsPresenterHostPartName = "ItemsPresenterHost";
        private const string BorderPartName = "Border";

        private const string PickerStatesGroupName = "PickerStates";
        private const string PickerStatesNormalStateName = "Normal";
        private const string PickerStatesHighlightedStateName = "Highlighted";
        private const string PickerStatesDisabledStateName = "Disabled";

        /// <summary>
        /// In Mango, the size of list pickers in expanded mode was given extra offset.
        /// </summary>
        private const double NormalModeOffset = 10;

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
        private bool _updatingSelection;
        private int _deferredSelectedIndex = -1;
        private object _deferredSelectedItem = null;

        private Border _border;

        /// <summary>
        /// Event that is raised when the selection changes.
        /// </summary>
        public event SelectionChangedEventHandler SelectionChanged;

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
            }

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


        /// <summary>
        /// Enabled property changed
        /// </summary>
        private static void OnIsEnabledChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            (o as ListPicker).OnIsEnabledChanged();
        }

        /// <summary>
        /// Enabled property changed
        /// </summary>
        private void OnIsEnabledChanged()
        {
            UpdateVisualStates(true);
        }

        /// <summary>
        /// Gets or sets the index of the selected item.
        /// </summary>
        public int SelectedIndex
        {
            get { return (int)GetValue(SelectedIndexProperty); }
            set { SetValue(SelectedIndexProperty, value); }
        }

        /// <summary>
        /// Identifies the SelectedIndex DependencyProperty.
        /// </summary>
        public static readonly DependencyProperty SelectedIndexProperty =
            DependencyProperty.Register("SelectedIndex", typeof(int), typeof(ListPicker), new PropertyMetadata(-1, OnSelectedIndexChanged));

        private static void OnSelectedIndexChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            ((ListPicker)o).OnSelectedIndexChanged((int)e.OldValue, (int)e.NewValue);
        }

        [SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "SelectedIndex", Justification = "Property name.")]
        private void OnSelectedIndexChanged(int oldValue, int newValue)
        {
            // Validate new value
            if ((Items.Count <= newValue) ||
                ((0 < Items.Count) && (newValue < 0)) ||
                ((0 == Items.Count) && (newValue != -1)))
            {
                if ((null == Template) && (0 <= newValue))
                {
                    // Can't set the value now; remember it for later
                    _deferredSelectedIndex = newValue;
                    return;
                }
                throw new InvalidOperationException(Properties.Resources.InvalidSelectedIndex);
            }

            // Synchronize SelectedItem property
            if (!_updatingSelection)
            {
                _updatingSelection = true;
                SelectedItem = (-1 != newValue) ? Items[newValue] : null;
                _updatingSelection = false;
            }

            if (-1 != oldValue)
            {
                // Toggle container selection
                ListPickerItem oldContainer = (ListPickerItem)ItemContainerGenerator.ContainerFromIndex(oldValue);
                if (null != oldContainer)
                {
                    oldContainer.IsSelected = false;
                }
            }
        }

        /// <summary>
        /// Gets or sets the selected item.
        /// </summary>
        public object SelectedItem
        {
            get { return (object)GetValue(SelectedItemProperty); }
            set { SetValue(SelectedItemProperty, value); }
        }

        /// <summary>
        /// Identifies the SelectedItem DependencyProperty.
        /// </summary>
        public static readonly DependencyProperty SelectedItemProperty =
            DependencyProperty.Register("SelectedItem", typeof(object), typeof(ListPicker), new PropertyMetadata(null, OnSelectedItemChanged));

        private static void OnSelectedItemChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            ((ListPicker)o).OnSelectedItemChanged(e.OldValue, e.NewValue);
        }

        [SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "SelectedItem", Justification = "Property name.")]
        private void OnSelectedItemChanged(object oldValue, object newValue)
        {
            if (newValue != null && (null == Items || Items.Count == 0))
            {
                if (null == Template)
                {
                    // Can't set the value now; remember it for later
                    _deferredSelectedItem = newValue;
                    return;
                }
                else
                {
                    throw new InvalidOperationException(Properties.Resources.InvalidSelectedItem);
                }
            }

            // Validate new value
            int newValueIndex = (null != newValue) ? Items.IndexOf(newValue) : -1;

            if ((-1 == newValueIndex) && (0 < Items.Count))
            {
                throw new InvalidOperationException(Properties.Resources.InvalidSelectedItem);
            }

            // Synchronize SelectedIndex property
            if (!_updatingSelection)
            {
                _updatingSelection = true;
                SelectedIndex = newValueIndex;
                _updatingSelection = false;
            }

            // Switch to Normal mode or size for current item
            if (IsExpanded)
            {
                IsExpanded = false;
            }
            else
            {
                SizeForAppropriateView(false);
            }

            // Fire SelectionChanged event
            var handler = SelectionChanged;
            if (null != handler)
            {
                IList removedItems = (null == oldValue) ? new object[0] : new object[] { oldValue };
                IList addedItems = (null == newValue) ? new object[0] : new object[] { newValue };
                handler(this, new SelectionChangedEventArgs(removedItems, addedItems));
            }
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
        /// Gets the selected items.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification="Want to allow this to be bound to.")]
        public IList SelectedItems
        {
            get { return (IList)GetValue(SelectedItemsProperty); }
            set { SetValue(SelectedItemsProperty, value); }
        }

        /// <summary>
        /// Identifies the SelectedItems DependencyProperty.
        /// </summary>
        public static readonly DependencyProperty SelectedItemsProperty = DependencyProperty.Register(
            "SelectedItems",
            typeof(IList),
            typeof(ListPicker),
            new PropertyMetadata(OnSelectedItemsChanged)
            );

        private static void OnSelectedItemsChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            ((ListPicker)o).OnSelectedItemsChanged((IList)e.OldValue, (IList)e.NewValue);
        }

        private void OnSelectedItemsChanged(IList oldValue, IList newValue)
        {
            // Fire SelectionChanged event
            var handler = SelectionChanged;
            if (null != handler)
            {
                IList removedItems = new List<object>();
                if (null != oldValue)
                {
                    foreach (object o in oldValue)
                    {
                        if (null == newValue || !newValue.Contains(o))
                        {
                            removedItems.Add(o);
                        }
                    }
                }
                IList addedItems = new List<object>();
                if (null != newValue)
                {
                    foreach (object o in newValue)
                    {
                        if (null == oldValue || !oldValue.Contains(o))
                        {
                            addedItems.Add(o);
                        }
                    }
                }

                handler(this, new SelectionChangedEventArgs(removedItems, addedItems));
            }
        }

        /// <summary>
        /// Initializes a new instance of the ListPicker class.
        /// </summary>
        public ListPicker()
        {
            DefaultStyleKey = typeof(ListPicker);

            Storyboard.SetTargetProperty(_heightAnimation, new PropertyPath(FrameworkElement.HeightProperty));
            Storyboard.SetTargetProperty(_translateAnimation, new PropertyPath(TranslateTransform.YProperty));

            // Would be nice if these values were customizable (ex: as DependencyProperties or in Template as VSM states)
            Duration duration = TimeSpan.FromSeconds(0.2);
            _heightAnimation.Duration = duration;
            _translateAnimation.Duration = duration;
            IEasingFunction easingFunction = new ExponentialEase { EasingMode = EasingMode.EaseInOut, Exponent = 4 };
            _heightAnimation.EasingFunction = easingFunction;
            _translateAnimation.EasingFunction = easingFunction;

            this.RegisterNotification("IsEnabled", OnIsEnabledChanged);

            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            UpdateVisualStates (true);
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
            _storyboard.Stop();

            base.OnApplyTemplate();

            // Hook up to new elements
            _itemsPresenterPart = GetTemplateChild(ItemsPresenterPartName) as ItemsPresenter;
            _itemsPresenterTranslateTransformPart = GetTemplateChild(ItemsPresenterTranslateTransformPartName) as TranslateTransform;
            _itemsPresenterHostPart = GetTemplateChild(ItemsPresenterHostPartName) as Canvas;
            _itemsPresenterHostParent = (null != _itemsPresenterHostPart) ? _itemsPresenterHostPart.Parent as FrameworkElement : null;
            _border = GetTemplateChild(BorderPartName) as Border;

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


            // Commit deferred SelectedIndex (if any)
            if (-1 != _deferredSelectedIndex)
            {
                SelectedIndex = _deferredSelectedIndex;
                _deferredSelectedIndex = -1;
            }
            if (null != _deferredSelectedItem)
            {
                SelectedItem = _deferredSelectedItem;
                _deferredSelectedItem = null;
            }

            OnSelectedItemsChanged(SelectedItems, SelectedItems);
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

            if ((0 < Items.Count) && (null == SelectedItem))
            {
                // Nothing selected (and no pending Binding); select the first item
                if ((null == GetBindingExpression(SelectedIndexProperty)) &&
                    (null == GetBindingExpression(SelectedItemProperty)))
                {
                    SelectedIndex = 0;
                }
            }
            else if (0 == Items.Count)
            {
                // No items; select nothing
                SelectedIndex = -1;
                IsExpanded = false;
            }
            else if (Items.Count <= SelectedIndex)
            {
                // Selected item no longer present; select the last item
                SelectedIndex = Items.Count - 1;
            }
            else
            {
                // Re-synchronize SelectedIndex with SelectedItem if necessary
                if (!object.Equals(Items[SelectedIndex], SelectedItem))
                {
                    int selectedItemIndex = Items.IndexOf(SelectedItem);
                    if (-1 == selectedItemIndex)
                    {
                        SelectedItem = Items[0];
                    }
                    else
                    {
                        SelectedIndex = selectedItemIndex;
                    }
                }
            }

            // Translate it into view once layout has been updated for the added/removed item(s)
            Dispatcher.BeginInvoke(() => SizeForAppropriateView(false));
        }

        private bool IsValidManipulation(object OriginalSource, Point p)
        {
            DependencyObject element = OriginalSource as DependencyObject;

            while (null != element)
            {
                if (_itemsPresenterHostPart == element || _border == element)
                {
                    double Padding = 11.0;
                    return (p.X > 0 && p.Y > 0 - Padding && p.X < _border.RenderSize.Width && p.Y < _border.RenderSize.Height + Padding);
                }

                element = VisualTreeHelper.GetParent(element);
            }
            return false;
        }

        /// <summary>
        /// Handles the tap event.
        /// </summary>
        /// <param name="e">Event args</param>
        protected override void OnTap(System.Windows.Input.GestureEventArgs e)
        {
            if (null == e)
            {
                throw new ArgumentNullException("e");
            }

            if (!IsExpanded)
            {
                if (!IsEnabled)
                {
                    e.Handled = true;
                    return;
                }

                Point p = e.GetPosition((UIElement)e.OriginalSource);
                if (IsValidManipulation(e.OriginalSource, p))
                {
                    if (Open())
                    {
                        e.Handled = true;
                    }
                }
            }
        }
        /// <summary>
        /// Called when the ManipulationStarted event occurs.
        /// </summary>
        /// <param name="e">Event data for the event.</param>
        protected override void OnManipulationStarted(ManipulationStartedEventArgs e)
        {
            if (null == e)
            {
                throw new ArgumentNullException("e");
            }

            base.OnManipulationStarted(e);

            if (!IsExpanded)
            {
                if (!IsEnabled)
                {
                    e.Complete();
                    return;
                }

                Point p = e.ManipulationOrigin;

                if (e.OriginalSource != e.ManipulationContainer)
                {
                    p = e.ManipulationContainer.TransformToVisual((UIElement)e.OriginalSource).Transform(p);
                }

                if (IsValidManipulation(e.OriginalSource, p))
                {
                    IsHighlighted = true;
                }
            }
        }

        /// <summary>
        /// Called when the ManipulationDelta event occurs.
        /// </summary>
        /// <param name="e">Event data for the event.</param>
        protected override void OnManipulationDelta(ManipulationDeltaEventArgs e)
        {
            if (null == e)
            {
                throw new ArgumentNullException("e");
            }

            base.OnManipulationDelta(e);

            if (!IsExpanded)
            {
                if (!IsEnabled)
                {
                    e.Complete();
                    return;
                }

                Point p = e.ManipulationOrigin;

                if (e.OriginalSource != e.ManipulationContainer)
                {
                    p = e.ManipulationContainer.TransformToVisual((UIElement)e.OriginalSource).Transform(p);
                }

                if (!IsValidManipulation(e.OriginalSource, p))
                {
                    IsHighlighted = false;
                    e.Complete();
                }
            }
        }

        /// <summary>
        /// Called when the ManipulationCompleted event occurs.
        /// </summary>
        /// <param name="e">Event data for the event.</param>
        protected override void OnManipulationCompleted(ManipulationCompletedEventArgs e)
        {
            if (null == e)
            {
                throw new ArgumentNullException("e");
            }

            base.OnManipulationCompleted(e);

            if (!IsEnabled)
            {
                return;
            }

            if (!IsExpanded)
            {
                // Style box to look unselected
                IsHighlighted = false;
            }
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
            ContentControl container = (ContentControl)ItemContainerGenerator.ContainerFromItem(SelectedItem);
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
                DependencyObject cancelElement = (DependencyObject)_border ?? (DependencyObject)this;
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
                VisualStateManager.GoToState(this, PickerStatesDisabledStateName, useTransitions);
            }
            else if (IsHighlighted)
            {
                VisualStateManager.GoToState(this, PickerStatesHighlightedStateName, useTransitions);
            }
            else
            {
                VisualStateManager.GoToState(this, PickerStatesNormalStateName, useTransitions);
            }
        }
    }
}
