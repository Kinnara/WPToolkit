using Microsoft.Phone.Controls.Primitives;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Microsoft.Phone.Controls
{
    /// <summary>
    /// Represents an items control that displays one item at a time, and enables "flip" behavior for traversing its collection of items.
    /// </summary>
    [TemplatePart(Name = ElementScrollingHostName, Type = typeof(ScrollViewer))]
    [TemplatePart(Name = ElementItemsPresenterName, Type = typeof(ItemsPresenter))]
    [StyleTypedProperty(Property = "ItemContainerStyle", StyleTargetType = typeof(FlipViewItem))]
    public class FlipView : TemplatedItemsControl<FlipViewItem>, ISupportInitialize
    {
        private const string ElementScrollingHostName = "ScrollingHost";
        private const string ElementItemsPresenterName = "ItemsPresenter";

        private const double MaxDraggingSquishDistance = 125;
        private static readonly Duration ZeroDuration = TimeSpan.Zero;
        private static readonly Duration DefaultDuration = TimeSpan.FromSeconds(0.4);
        private static readonly Duration UnsquishDuration = TimeSpan.FromSeconds(0.3);
        private static readonly IEasingFunction DefaultEase = new ExponentialEase { Exponent = 5 };
        private static readonly IEasingFunction UnsquishEase = DefaultEase;

        private InitializingData _initializingData;
        private bool _updatingSelection;

        private Orientation _orientation = Orientation.Horizontal;
        private Size _itemsHostSize = new Size(double.NaN, double.NaN);
        private List<FlipViewItem> _realizedItems = new List<FlipViewItem>();
        private bool _loaded;

        private bool _animating;
        private bool _isEffectiveDragging;
        private bool _dragging;
        private DragLock _dragLock;
        private WeakReference _gestureSource;
        private Point _gestureOrigin;
        private ManipulationStartedEventArgs _gestureStartedEventArgs;
        private Animator _animator;
        private int? _deferredSelectedIndex;
        private bool _suppressAnimation;
        private double? _offsetWhenDragStarted;
        private bool _squishing;
        private bool _supressHandleManipulation;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Microsoft.Phone.Controls.FlipView" /> class.
        /// </summary>
        public FlipView()
        {
            DefaultStyleKey = typeof(FlipView);

            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
            SizeChanged += OnSizeChanged;
        }

        #region SelectedIndex

        /// <summary>
        /// Gets or sets the index of the selected item.
        /// </summary>
        /// 
        /// <returns>
        /// The index of the selected item. The default is -1.
        /// </returns>
        public int SelectedIndex
        {
            get { return (int)GetValue(SelectedIndexProperty); }
            set { SetValue(SelectedIndexProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="P:Microsoft.Phone.Controls.FlipView.SelectedIndex"/> dependency property.
        /// </summary>
        /// 
        /// <returns>
        /// The identifier for the <see cref="P:Microsoft.Phone.Controls.FlipView.SelectedIndex"/> dependency property.
        /// </returns>
        public static readonly DependencyProperty SelectedIndexProperty = DependencyProperty.Register(
            "SelectedIndex",
            typeof(int),
            typeof(FlipView),
            new PropertyMetadata(-1, (d, e) => ((FlipView)d).OnSelectedIndexChanged((int)e.OldValue, (int)e.NewValue)));

        [SuppressMessage("Microsoft.Usage", "CA2208:InstantiateArgumentExceptionsCorrectly")]
        private void OnSelectedIndexChanged(int oldIndex, int newIndex)
        {
            if (_updatingSelection || IsInit)
            {
                return;
            }

            if (newIndex >= -1 && newIndex < Items.Count)
            {
                UpdateSelection(oldIndex, newIndex, SelectedItem, Items[newIndex]);
            }
            else
            {
                SelectedIndex = oldIndex;

                throw new ArgumentOutOfRangeException("SelectedIndex");
            }
        }

        #endregion

        #region SelectedItem

        /// <summary>
        /// Gets or sets the selected item.
        /// </summary>
        /// 
        /// <returns>
        /// The selected item. The default is null.
        /// </returns>
        public object SelectedItem
        {
            get { return (object)GetValue(SelectedItemProperty); }
            set { SetValue(SelectedItemProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="P:Microsoft.Phone.Controls.FlipView.SelectedItem"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty SelectedItemProperty = DependencyProperty.Register(
            "SelectedItem",
            typeof(object),
            typeof(FlipView),
            new PropertyMetadata((d, e) => ((FlipView)d).OnSelectedItemChanged(e.OldValue, e.NewValue)));

        private void OnSelectedItemChanged(object oldValue, object newValue)
        {
            if (_updatingSelection || IsInit)
            {
                return;
            }

            int index = Items.IndexOf(newValue);
            if (index != -1 || (newValue == null && Items.Count == 0))
            {
                UpdateSelection(SelectedIndex, index, oldValue, newValue);
            }
            else
            {
                SelectedItem = oldValue;
            }
        }

        #endregion

        #region UseTouchAnimationsForAllNavigation

        /// <summary>
        /// Gets or sets a value that indicates whether transition animations are always used whether the navigation is touch-based and programmatic.
        /// </summary>
        /// 
        /// <returns>
        /// true if transition animations are always used; false if transition animations are used only for touch navigation. The default is true.
        /// </returns>
        public bool UseTouchAnimationsForAllNavigation
        {
            get { return (bool)GetValue(UseTouchAnimationsForAllNavigationProperty); }
            set { SetValue(UseTouchAnimationsForAllNavigationProperty, value); }
        }

        /// <summary>
        /// Identifies the UseTouchAnimationsForAllNavigation dependency property.
        /// </summary>
        /// 
        /// <returns>
        /// The identifier for the UseTouchAnimationsForAllNavigation dependency property.
        /// </returns>
        public static readonly DependencyProperty UseTouchAnimationsForAllNavigationProperty = DependencyProperty.Register(
            "UseTouchAnimationsForAllNavigation",
            typeof(bool),
            typeof(FlipView),
            new PropertyMetadata(true));

        #endregion

        #region UpdateSelectionMode

        /// <summary>
        /// Gets or sets a value that determines the timing of selection updates .
        /// </summary>
        /// 
        /// <returns>
        /// One of the UpdateSelectionMode values. The default is AfterTransition.
        /// </returns>
        public UpdateSelectionMode UpdateSelectionMode
        {
            get { return (UpdateSelectionMode)GetValue(UpdateSelectionModeProperty); }
            set { SetValue(UpdateSelectionModeProperty, value); }
        }

        /// <summary>
        /// Identifies the UpdateSelectionMode dependency property.
        /// </summary>
        /// 
        /// <returns>
        /// The identifier for the UpdateSelectionMode dependency property.
        /// </returns>
        public static readonly DependencyProperty UpdateSelectionModeProperty = DependencyProperty.Register(
            "UpdateSelectionMode",
            typeof(UpdateSelectionMode),
            typeof(FlipView),
            null);

        #endregion

        #region IsSelected

        internal static readonly DependencyProperty IsSelectedProperty = DependencyProperty.RegisterAttached(
            "IsSelected",
            typeof(bool),
            typeof(FlipView),
            new PropertyMetadata(OnIsSelectedChanged));

        private static void OnIsSelectedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            FlipViewItem container = d as FlipViewItem;
            if (container != null)
            {
                container.OnIsSelectedChanged((bool)e.NewValue);
            }
        }

        #endregion

        private Panel ItemsHost { get; set; }

        private Size ItemsHostSize
        {
            get { return _itemsHostSize; }
            set
            {
                if (_itemsHostSize != value)
                {
                    _itemsHostSize = value;
                    UpdateItemsSize();
                }
            }
        }

        private double ItemSize
        {
            get { return Orientation == Orientation.Horizontal ? ItemsHostSize.Width : ItemsHostSize.Height; }
        }

        private ScrollViewer ElementScrollingHost { get; set; }

        private ItemsPresenter ElementItemsPresenter { get; set; }

        private Orientation Orientation
        {
            get { return _orientation; }
            set
            {
                if (_orientation != value)
                {
                    _orientation = value;
                    UpdateItemsSize();
                }
            }
        }

        private bool IsInit
        {
            get { return _initializingData != null; }
        }

        private bool ShouldHandleManipulation
        {
            get { return Items.Count > 1 && ElementItemsPresenter != null && !_supressHandleManipulation; }
        }

        private int EffectiveSelectedIndex
        {
            get { return _deferredSelectedIndex.GetValueOrDefault(SelectedIndex); }
        }

        private double ScrollOffset
        {
            get
            {
                if (ElementScrollingHost != null)
                {
                    return Orientation == Orientation.Horizontal ? ElementScrollingHost.HorizontalOffset : ElementScrollingHost.VerticalOffset;
                }

                return 0;
            }
        }

        private double TransformOffset
        {
            get { return _animator != null ? _animator.CurrentOffset : 0; }
        }

        /// <summary>
        /// Occurs when the currently selected item changes.
        /// </summary>
        public event SelectionChangedEventHandler SelectionChanged;

        /// <summary>
        /// Builds the visual tree for the <see cref="T:Microsoft.Phone.Controls.FlipView"/> control when a new template is applied.
        /// </summary>
        public override void OnApplyTemplate()
        {
            _animator = null;
            ItemsHost = null;
            ItemsHostSize = new Size(double.NaN, double.NaN);

            if (ElementItemsPresenter != null)
            {
                LayoutUpdated -= OnLayoutUpdated;
            }

            base.OnApplyTemplate();

            ElementScrollingHost = GetTemplateChild(ElementScrollingHostName) as ScrollViewer;
            ElementItemsPresenter = GetTemplateChild(ElementItemsPresenterName) as ItemsPresenter;

            if (ElementItemsPresenter != null)
            {
                InitializeItemsHost();

                if (ItemsHost == null)
                {
                    LayoutUpdated += OnLayoutUpdated;
                }
            }
        }

        /// <summary>
        /// Handles the measure pass for the control.
        /// </summary>
        /// 
        /// <returns>
        /// The desired size.
        /// </returns>
        /// <param name="availableSize">The available size.</param>
        protected override Size MeasureOverride(Size availableSize)
        {
            if (double.IsNaN(_itemsHostSize.Width) && double.IsNaN(_itemsHostSize.Height))
            {
                _itemsHostSize = availableSize;
                UpdateItemsSize();
            }

            return base.MeasureOverride(availableSize);
        }

        /// <summary>
        /// Provides handling for the <see cref="E:System.Windows.Controls.ItemContainerGenerator.ItemsChanged"/> event.
        /// </summary>
        /// <param name="e">A <see cref="T:System.Collections.Specialized.NotifyCollectionChangedEventArgs"/> that contains the event data.</param>
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Justification = "Standard pattern.")]
        protected override void OnItemsChanged(NotifyCollectionChangedEventArgs e)
        {
            base.OnItemsChanged(e);

            int oldSelectedIndex = SelectedIndex;
            int newSelectedIndex = oldSelectedIndex;
            object oldSelectedItem = SelectedItem;
            object newSelectedItem = oldSelectedItem;

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    for (int index = 0; index < e.NewItems.Count; ++index)
                    {
                        FlipViewItem container = e.NewItems[index] as FlipViewItem;
                        if (container != null && container.IsSelected)
                        {
                            newSelectedIndex = e.NewStartingIndex + index;
                            newSelectedItem = container;
                        }
                    }

                    if (newSelectedIndex == oldSelectedIndex && e.NewStartingIndex <= oldSelectedIndex && !IsInit)
                    {
                        newSelectedIndex = oldSelectedIndex + e.NewItems.Count;
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    if (e.OldItems.Contains(oldSelectedItem))
                    {
                        if (e.OldStartingIndex <= Items.Count - 1)
                        {
                            newSelectedIndex = e.OldStartingIndex;
                            newSelectedItem = Items[newSelectedIndex];
                        }
                        else if (Items.Count > 0)
                        {
                            newSelectedIndex = Items.Count - 1;
                            newSelectedItem = Items[newSelectedIndex];
                        }
                        else
                        {
                            newSelectedIndex = -1;
                            newSelectedItem = null;
                        }
                    }
                    else if (e.OldStartingIndex + e.OldItems.Count <= oldSelectedIndex)
                    {
                        newSelectedIndex -= e.OldItems.Count;
                    }
                    break;
                case NotifyCollectionChangedAction.Replace:
                    if (e.OldItems.Contains(oldSelectedItem))
                    {
                        FlipViewItem container = GetContainer(oldSelectedIndex) as FlipViewItem;
                        if (container != null)
                        {
                            container.IsSelected = true;
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Reset:
                    if (Items.Count > 0)
                    {
                        newSelectedIndex = 0;
                        newSelectedItem = Items[0];

                        if (ItemTemplate == null)
                        {
                            for (int index = 0; index < Items.Count; ++index)
                            {
                                FlipViewItem container = GetContainer(index);
                                if (container != null && container.IsSelected)
                                {
                                    newSelectedIndex = index;
                                    newSelectedItem = Items[index];
                                }
                            }
                        }
                    }
                    else
                    {
                        newSelectedIndex = -1;
                        newSelectedItem = null;
                    }
                    break;
                default:
                    throw new InvalidOperationException();
            }

            if (newSelectedIndex < 0 && Items.Count > 0)
            {
                newSelectedIndex = 0;
                newSelectedItem = Items[0];
            }

            _suppressAnimation = true;
            UpdateSelection(oldSelectedIndex, newSelectedIndex, oldSelectedItem, newSelectedItem);
            _suppressAnimation = false;

            if (_animating)
            {
                _deferredSelectedIndex = null;
                CompleteAnimateTo();
            }

            if (_gestureStartedEventArgs != null)
            {
                _supressHandleManipulation = true;
                _gestureStartedEventArgs.Complete();
                _supressHandleManipulation = false;
                GoTo(0);
            }
        }

        /// <summary>
        /// Prepares the specified element to display the specified item.
        /// </summary>
        /// <param name="element">The element used to display the specified item.</param>
        /// <param name="item">The item to display</param>
        protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
        {
            base.PrepareContainerForItemOverride(element, item);

            FlipViewItem container = (FlipViewItem)element;

            container.ParentFlipView = this;
            if (!ReferenceEquals(element, item))
            {
                container.Item = item;
            }

            int index = ItemContainerGenerator.IndexFromContainer(element);
            if (index != -1)
            {
                container.IsSelected = SelectedIndex == index;
            }

            _realizedItems.Add(container);

            UpdateItemSize(container, Orientation == Orientation.Horizontal);
        }

        /// <summary>
        /// Removes any bindings and templates applied to the item container for the specified content.
        /// </summary>
        /// <param name="element">The combo box item used to display the specified content.</param>
        /// <param name="item">The item content.</param>
        protected override void ClearContainerForItemOverride(DependencyObject element, object item)
        {
            base.ClearContainerForItemOverride(element, item);

            FlipViewItem container = (FlipViewItem)element;

            if (!container.Equals(item))
            {
                container.ClearValue(ContentControl.ContentProperty);
            }

            container.Item = null;

            _realizedItems.Remove(container);
        }

        internal void NotifyItemSelected(FlipViewItem container, bool isSelected)
        {
            if (_updatingSelection)
            {
                return;
            }

            int index = ItemContainerGenerator.IndexFromContainer(container);
            if (index < 0 || index >= Items.Count)
            {
                return;
            }

            object item = container.Item ?? container;
            if (isSelected)
            {
                UpdateSelection(SelectedIndex, index, SelectedItem, item);
            }
            else
            {
                if (SelectedIndex == index)
                {
                    UpdateSelection(SelectedIndex, -1, SelectedItem, null);
                }
            }
        }

        private void InvokeSelectionChanged(List<object> unselectedItems, List<object> selectedItems)
        {
            OnSelectionChanged(new SelectionChangedEventArgs(unselectedItems, selectedItems));
        }

        private void OnSelectionChanged(SelectionChangedEventArgs e)
        {
            if (SelectionChanged != null)
            {
                SelectionChanged(this, e);
            }
        }

        private FlipViewItem GetContainer(int index)
        {
            if (index < 0 || Items.Count <= index)
            {
                return null;
            }
            else
            {
                return Items[index] as FlipViewItem ?? ItemContainerGenerator.ContainerFromIndex(index) as FlipViewItem;
            }
        }

        private void SetItemIsSelected(object item, bool value)
        {
            FlipViewItem container = item as FlipViewItem ?? ItemContainerGenerator.ContainerFromItem(item) as FlipViewItem;
            if (container != null)
            {
                container.IsSelected = value;
            }
        }

        private void UpdateSelection(int oldSelectedIndex, int newSelectedIndex, object oldSelectedItem, object newSelectedItem)
        {
            int indexDelta = newSelectedIndex - oldSelectedIndex;
            bool selectedItemChanged = !InternalUtils.AreValuesEqual(oldSelectedItem, newSelectedItem);

            if (indexDelta == 0 && !selectedItemChanged)
            {
                return;
            }

            bool animate =
                _loaded &&
                UseTouchAnimationsForAllNavigation &&
                Items.Count > 1 &&
                oldSelectedIndex != -1 &&
                Math.Abs(indexDelta) == 1 &&
                selectedItemChanged &&
                !(_animating && _deferredSelectedIndex.HasValue && _deferredSelectedIndex == newSelectedIndex) &&
                !_suppressAnimation;

            try
            {
                _updatingSelection = true;

                if (newSelectedIndex < 0 && Items.Count > 0)
                {
                    newSelectedIndex = 0;
                    newSelectedItem = Items[newSelectedIndex];
                }

                SelectedIndex = newSelectedIndex;
                SelectedItem = newSelectedItem;

                if (selectedItemChanged)
                {
                    List<object> unselected = new List<object>();
                    List<object> selected = new List<object>();

                    if (oldSelectedItem != null)
                    {
                        SetItemIsSelected(oldSelectedItem, false);
                        unselected.Add(oldSelectedItem);
                    }

                    if (newSelectedItem != null)
                    {
                        SetItemIsSelected(newSelectedItem, true);
                        selected.Add(newSelectedItem);
                    }

                    InvokeSelectionChanged(unselected, selected);
                }
            }
            finally
            {
                _updatingSelection = false;
            }

            if (animate)
            {
                AnimateTo(SelectedIndex, false);
            }
            else if (!_isEffectiveDragging)
            {
                if (_animating)
                {
                    if (!UseTouchAnimationsForAllNavigation)
                    {
                        _deferredSelectedIndex = null;
                        CompleteAnimateTo();
                    }
                }
                else
                {
                    ScrollSelectionIntoView();
                }
            }
        }

        private void UpdateItemsHostSize()
        {
            ItemsHostSize = new Size(ItemsHost.ActualWidth, ItemsHost.ActualHeight);
        }

        private void UpdateItemsSize()
        {
            bool horizontal = Orientation == Orientation.Horizontal;

            foreach (FlipViewItem container in _realizedItems)
            {
                UpdateItemSize(container, horizontal);
            }
        }

        private void UpdateItemSize(FlipViewItem container, bool horizontal)
        {
            if (horizontal)
            {
                container.Width = _itemsHostSize.Width;
                container.ClearValue(FrameworkElement.HeightProperty);
            }
            else
            {
                container.ClearValue(FrameworkElement.WidthProperty);
                container.Height = _itemsHostSize.Height;
            }
        }

        private void InitializeItemsHost()
        {
            ItemsHost = ElementItemsPresenter.GetFirstLogicalChildByType<Panel>(false);
            if (ItemsHost != null)
            {
                UpdateItemsHostSize();
                ItemsHost.SizeChanged += OnItemsHostSizeChanged;

                VirtualizingStackPanel vsp = ItemsHost as VirtualizingStackPanel;
                if (vsp != null)
                {
                    Orientation = vsp.Orientation;
                }
                else
                {
                    throw new InvalidOperationException(Properties.Resources.FlipView_NotAllowedItemsPanel);
                }

                ScrollSelectionIntoView();
            }
        }

        private void OnItemsHostSizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateItemsHostSize();
        }

        private void OnLayoutUpdated(object sender, EventArgs e)
        {
            InitializeItemsHost();

            if (ItemsHost != null)
            {
                LayoutUpdated -= OnLayoutUpdated;
            }
        }

        private void ScrollSelectionIntoView()
        {
            int index = EffectiveSelectedIndex;

            if (ItemsHost != null && ElementScrollingHost != null && _loaded && index >= 0 && index != ScrollOffset)
            {
                ElementScrollingHost.UpdateLayout();

                if (Orientation == Orientation.Horizontal)
                {
                    ElementScrollingHost.ScrollToHorizontalOffset(index);
                }
                else
                {
                    ElementScrollingHost.ScrollToVerticalOffset(index);
                }

                ElementScrollingHost.UpdateLayout();
            }
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            _loaded = true;

            ScrollSelectionIntoView();
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            _loaded = false;
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            ScrollSelectionIntoView();
        }

        internal void OnManipulationStarted(object sender, ManipulationStartedEventArgs e)
        {
            _gestureSource = new WeakReference(e.ManipulationContainer);
            _gestureOrigin = e.ManipulationOrigin;
            _gestureStartedEventArgs = e;
            _dragLock = DragLock.Unset;
            _dragging = false;
        }

        internal void OnManipulationDelta(object sender, ManipulationDeltaEventArgs e)
        {
            if (!_dragging)
            {
                ReleaseMouseCaptureAtGestureOrigin();
            }

            _dragging = true;

            if (_dragLock == DragLock.Unset)
            {
                double angle = AngleFromVector(e.CumulativeManipulation.Translation.X, e.CumulativeManipulation.Translation.Y) % 180;
                _dragLock = angle <= 45 || angle >= 135 ? DragLock.Horizontal : DragLock.Vertical;
            }

            e.Handled = true;

            if (_dragLock == DragLock.Horizontal && e.DeltaManipulation.Translation.X != 0 && Orientation == Orientation.Horizontal ||
                _dragLock == DragLock.Vertical && e.DeltaManipulation.Translation.Y != 0 && Orientation == Orientation.Vertical)
            {
                Drag(e);
            }
        }

        internal void OnManipulationCompleted(object sender, ManipulationCompletedEventArgs e)
        {
            ManipulationDelta totalManipulation = null;

            _gestureStartedEventArgs = null;
            _dragLock = DragLock.Unset;
            _dragging = false;

            if (e.IsInertial)
            {
                double angle = AngleFromVector(e.FinalVelocities.LinearVelocity.X, e.FinalVelocities.LinearVelocity.Y);

                if (Orientation == Orientation.Vertical)
                {
                    angle -= 90;
                    if (angle < 0)
                    {
                        angle += 360;
                    }
                }

                if (angle <= 45 || angle >= 315)
                {
                    angle = 0;
                }
                else if (angle >= 135 && angle <= 225)
                {
                    angle = 180;
                }

                ReleaseMouseCaptureAtGestureOrigin();

                Flick(angle);

                if (angle == 0 || angle == 180)
                {
                    e.Handled = true;
                }
            }
            else if (e.TotalManipulation.Translation.X != 0 || e.TotalManipulation.Translation.Y != 0)
            {
                totalManipulation = e.TotalManipulation;

                if (_isEffectiveDragging)
                {
                    e.Handled = true;
                }
            }

            GesturesComplete(totalManipulation);
        }

        private void Drag(ManipulationDeltaEventArgs e)
        {
            _isEffectiveDragging = true;

            if (!ShouldHandleManipulation)
            {
                return;
            }

            if (_animating)
            {
                _animating = false;

                double oldScrollOffset = ScrollOffset;
                double oldTransformOffset = TransformOffset;

                ScrollSelectionIntoView();

                if (!_offsetWhenDragStarted.HasValue)
                {
                    _offsetWhenDragStarted = (ScrollOffset - oldScrollOffset) * ItemSize + oldTransformOffset;
                }
            }

            double targetOffset = Orientation == Orientation.Horizontal ? e.CumulativeManipulation.Translation.X : e.CumulativeManipulation.Translation.Y;
            if (_offsetWhenDragStarted.HasValue)
            {
                targetOffset += _offsetWhenDragStarted.Value;
            }

            _squishing = false;

            if (EffectiveSelectedIndex <= 0)
            {
                if (targetOffset > MaxDraggingSquishDistance)
                {
                    targetOffset = MaxDraggingSquishDistance;
                }

                if (targetOffset > 0)
                {
                    _squishing = true;
                }
            }
            else if (EffectiveSelectedIndex >= Items.Count - 1)
            {
                if (targetOffset < -MaxDraggingSquishDistance)
                {
                    targetOffset = -MaxDraggingSquishDistance;
                }

                if (targetOffset < 0)
                {
                    _squishing = true;
                }
            }

            GoTo(targetOffset);
        }

        private void Flick(double angle)
        {
            if (ShouldHandleManipulation)
            {
                int intAngle = (int)angle;
                switch (intAngle)
                {
                    case 0:
                    case 180:
                        AnimateTo(EffectiveSelectedIndex + (intAngle == 180 ? 1 : -1));
                        break;
                }
            }
        }

        private void GesturesComplete(ManipulationDelta totalManipulation)
        {
            if (ShouldHandleManipulation)
            {
                if (totalManipulation != null && _isEffectiveDragging)
                {
                    AnimateTo((int)Math.Round(ScrollOffset - TransformOffset / ItemSize));
                }
                else if (totalManipulation == null && !_animating)
                {
                    AnimateTo(EffectiveSelectedIndex);
                }
            }

            _isEffectiveDragging = false;
            _offsetWhenDragStarted = null;
            _squishing = false;
        }

        private void ReleaseMouseCaptureAtGestureOrigin()
        {
            if (_gestureSource != null)
            {
                FrameworkElement gestureSource = _gestureSource.Target as FrameworkElement;
                if (gestureSource != null)
                {
                    try
                    {
                        foreach (UIElement element in VisualTreeHelper.FindElementsInHostCoordinates(
                                gestureSource.TransformToVisual(null).Transform(_gestureOrigin), Application.Current.RootVisual))
                        {
                            element.ReleaseMouseCapture();
                        }
                    }
                    catch (ArgumentException)
                    {
                    }
                }
            }
        }

        private void AnimateTo(int index, bool changeIndex = true)
        {
            if (_suppressAnimation)
            {
                return;
            }

            double? oldScrollOffset = null;
            double? oldTransformOffset = null;

            if (_animating)
            {
                if (ValidateIndex(index))
                {
                    oldScrollOffset = ScrollOffset;
                    oldTransformOffset = TransformOffset;
                    _deferredSelectedIndex = null;
                    CompleteAnimateTo();
                }
            }

            if (changeIndex)
            {
                if (!ValidateIndex(index))
                {
                    return;
                }
            }

            if (changeIndex && UpdateSelectionMode == UpdateSelectionMode.BeforeTransition)
            {
                changeIndex = false;

                _suppressAnimation = true;
                SelectedIndex = index;
                _suppressAnimation = false;
            }

            _animating = true;

            if (changeIndex)
            {
                _deferredSelectedIndex = index;
            }

            if (oldScrollOffset.HasValue && oldTransformOffset.HasValue)
            {
                GoTo((ScrollOffset - oldScrollOffset.Value) * ItemSize + oldTransformOffset.Value);
            }

            Duration duration;
            IEasingFunction easingFunction;

            if (_squishing)
            {
                duration = UnsquishDuration;
                easingFunction = UnsquishEase;
            }
            else
            {
                duration = DefaultDuration;
                easingFunction = DefaultEase;
            }

            GoTo(
                (ScrollOffset - EffectiveSelectedIndex) * ItemSize,
                duration,
                easingFunction,
                CompleteAnimateTo);
        }

        private void CompleteAnimateTo()
        {
            int? newSelectedIndex = _deferredSelectedIndex;

            _animating = false;
            _deferredSelectedIndex = null;

            if (newSelectedIndex.HasValue)
            {
                _suppressAnimation = true;
                SelectedIndex = newSelectedIndex.Value;
                _suppressAnimation = false;
            }

            ScrollSelectionIntoView();
            GoTo(0);
        }

        private bool ValidateIndex(int index)
        {
            return index >= 0 && index <= Items.Count - 1;
        }

        private void GoTo(double targetOffset)
        {
            GoTo(targetOffset, ZeroDuration, null, null);
        }

        private void GoTo(double targetOffset, Duration duration, IEasingFunction easingFunction, Action completionAction)
        {
            if (Animator.TryEnsureAnimator(ElementItemsPresenter, Orientation, ref _animator))
            {
                _animator.GoTo(targetOffset, duration, easingFunction, completionAction);
            }
        }

        private static double AngleFromVector(double x, double y)
        {
            double num = Math.Atan2(y, x);
            if (num < 0)
            {
                num = 2 * Math.PI + num;
            }
            return num * 360 / (2 * Math.PI);
        }

        #region ISupportInitialize

        void ISupportInitialize.BeginInit()
        {
            _initializingData = new InitializingData
            {
                InitialItem = SelectedItem,
                InitialIndex = SelectedIndex
            };
        }

        void ISupportInitialize.EndInit()
        {
            if (_initializingData == null)
            {
                throw new InvalidOperationException();
            }

            int selectedIndex = SelectedIndex;
            object selectedItem = SelectedItem;

            if (_initializingData.InitialIndex != selectedIndex)
            {
                SelectedIndex = _initializingData.InitialIndex;
                _initializingData = null;
                SelectedIndex = selectedIndex;
            }
            else if (!ReferenceEquals(_initializingData.InitialItem, selectedItem))
            {
                SelectedItem = _initializingData.InitialItem;
                _initializingData = null;
                SelectedItem = selectedItem;
            }

            _initializingData = null;
        }

        #endregion

        #region Nested Types

        private class Animator
        {
            private static readonly PropertyPath TranslateXPropertyPath = new PropertyPath(CompositeTransform.TranslateXProperty);
            private static readonly PropertyPath TranslateYPropertyPath = new PropertyPath(CompositeTransform.TranslateYProperty);

            private readonly Storyboard _sbRunning = new Storyboard();

            private readonly DoubleAnimation _daRunning = new DoubleAnimation();

            private readonly Orientation _orientation;

            private CompositeTransform _transform;

            private Action _oneTimeAction;

            public Animator(CompositeTransform compositeTransform, Orientation orientation)
            {
                _transform = compositeTransform;
                _orientation = orientation;

                _sbRunning.Completed += OnCompleted;
                _sbRunning.Children.Add(_daRunning);
                Storyboard.SetTarget(_daRunning, _transform);
                Storyboard.SetTargetProperty(_daRunning, _orientation == Orientation.Horizontal ? TranslateXPropertyPath : TranslateYPropertyPath);
            }

            public double CurrentOffset
            {
                get { return _orientation == Orientation.Horizontal ? _transform.TranslateX : _transform.TranslateY; }
            }

            public Orientation Orientation
            {
                get { return _orientation; }
            }

            public void GoTo(double targetOffset, Duration duration)
            {
                GoTo(targetOffset, duration, null, null);
            }

            public void GoTo(double targetOffset, Duration duration, IEasingFunction easingFunction, Action completionAction)
            {
                _daRunning.To = targetOffset;
                _daRunning.Duration = duration;
                _daRunning.EasingFunction = easingFunction;
                _sbRunning.Begin();
                _sbRunning.SeekAlignedToLastTick(TimeSpan.Zero);
                _oneTimeAction = completionAction;
            }

            private void OnCompleted(object sender, EventArgs e)
            {
                Action action = _oneTimeAction;
                if (action != null && _sbRunning.GetCurrentState() != ClockState.Active)
                {
                    _oneTimeAction = null;
                    action();
                }
            }

            public static bool TryEnsureAnimator(FrameworkElement targetElement, Orientation orientation, ref Animator animator)
            {
                if (animator == null || animator.Orientation != orientation)
                {
                    CompositeTransform transform = Animator.GetCompositeTransform(targetElement);
                    if (transform != null)
                    {
                        animator = new Animator(transform, orientation);
                    }
                    else
                    {
                        animator = null;
                        return false;
                    }
                }
                return true;
            }

            public static CompositeTransform GetCompositeTransform(UIElement container)
            {
                if (container == null)
                {
                    return null;
                }

                CompositeTransform transform = container.RenderTransform as CompositeTransform;
                if (transform == null)
                {
                    transform = new CompositeTransform();
                    container.RenderTransform = transform;
                }

                return transform;
            }
        }

        private class InitializingData
        {
            public int InitialIndex;
            public object InitialItem;
        }

        private enum DragLock
        {
            Unset,
            Free,
            Vertical,
            Horizontal,
        }

        #endregion
    }
}