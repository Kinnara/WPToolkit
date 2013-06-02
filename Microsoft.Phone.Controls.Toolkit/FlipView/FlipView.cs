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
    [TemplatePart(Name = ElementScrollViewerName, Type = typeof(ScrollViewer))]
    [TemplatePart(Name = ElementItemsPresenterName, Type = typeof(ItemsPresenter))]
    public class FlipView : ItemsControl, ISupportInitialize
    {
        private const string ElementScrollViewerName = "ScrollViewer";
        private const string ElementItemsPresenterName = "ItemsPresenter";

        private const double CompressLimit = 125;
        private static readonly Duration ZeroDuration = TimeSpan.Zero;
        private static readonly Duration DefaultDuration = TimeSpan.FromSeconds(0.5);

        private readonly IEasingFunction _easingFunction = new ExponentialEase { Exponent = 5 };

        private InitializingData _initializingData;
        private bool _updatingSelection;

        private Orientation _orientation = Orientation.Horizontal;
        private Size _itemsHostSize = new Size(double.NaN, double.NaN);
        private List<FlipViewItem> _realizedItems = new List<FlipViewItem>();
        private bool _loaded;

        private AnimationDirection? _animationHint;
        private bool _animating;
        private bool _isEffectiveDragging;
        private bool _dragging;
        private DragLock _dragLock;
        private WeakReference _gestureSource;
        private Point _gestureOrigin;
        private Animator _panAnimator;
        private int? _deferredSelectedIndex;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Microsoft.Phone.Controls.FlipView" /> class.
        /// </summary>
        public FlipView()
        {
            DefaultStyleKey = typeof(FlipView);

            Loaded += OnLoaded;
            Unloaded += OnUnloaded;

            ManipulationStarted += OnManipulationStarted;
            ManipulationDelta += OnManipulationDelta;
            ManipulationCompleted += OnManipulationCompleted;
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

        private ScrollViewer ElementScrollViewer { get; set; }

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
            get { return Items.Count > 1 && ElementItemsPresenter != null; }
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
            _panAnimator = null;
            ItemsHost = null;
            ItemsHostSize = new Size(double.NaN, double.NaN);

            if (ElementItemsPresenter != null)
            {
                LayoutUpdated -= OnLayoutUpdated;
            }

            base.OnApplyTemplate();

            ElementScrollViewer = GetTemplateChild(ElementScrollViewerName) as ScrollViewer;
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
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
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
                            newSelectedIndex = 0;
                            newSelectedItem = Items[0];
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

            UpdateSelection(oldSelectedIndex, newSelectedIndex, oldSelectedItem, newSelectedItem);
        }

        /// <summary>
        /// Determines if the specified item is (or is eligible to be) its own item container.
        /// </summary>
        /// 
        /// <returns>
        /// true if the item is its own item container; otherwise, false.
        /// </returns>
        /// <param name="item">The specified item.</param>
        protected override bool IsItemItsOwnContainerOverride(object item)
        {
            return item is FlipViewItem;
        }

        /// <summary>
        /// Creates or identifies the element used to display a specified item.
        /// </summary>
        /// 
        /// <returns>
        /// A <see cref="T:Microsoft.Phone.Controls.FlipViewItem"/> corresponding to a specified item.
        /// </returns>
        protected override DependencyObject GetContainerForItemOverride()
        {
            return new FlipViewItem();
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

        private void OnSelectionChanged()
        {
            if (_animating)
            {
                GoTo(0, ZeroDuration);

                _animationHint = null;
                _animating = false;
                _deferredSelectedIndex = null;
            }

            if (_panAnimator != null)
            {
                _panAnimator.GoTo(0, ZeroDuration);
            }

            ScrollSelectionIntoView();
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
            if (oldSelectedIndex == newSelectedIndex && AreValuesEqual(oldSelectedItem, newSelectedItem))
            {
                return;
            }

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
                OnSelectionChanged();

                if (!AreValuesEqual(oldSelectedItem, newSelectedItem))
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
            int index = SelectedIndex;

            if (ItemsHost != null && ElementScrollViewer != null && _loaded && index >= 0)
            {
                ElementScrollViewer.UpdateLayout();

                if (Orientation == Orientation.Horizontal)
                {
                    ElementScrollViewer.ScrollToHorizontalOffset(index);
                }
                else
                {
                    if (ElementScrollViewer.ViewportHeight != 1)
                    {
                        for (int i = 0; i < ElementScrollViewer.ExtentHeight; i++)
                        {
                            ElementScrollViewer.ScrollToVerticalOffset(i);
                            ElementScrollViewer.UpdateLayout();

                            if (ElementScrollViewer.ViewportHeight == 1)
                            {
                                break;
                            }
                        }
                    }

                    ElementScrollViewer.ScrollToVerticalOffset(index);
                }
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

        private void OnManipulationStarted(object sender, ManipulationStartedEventArgs e)
        {
            _gestureSource = new WeakReference(e.ManipulationContainer);
            _gestureOrigin = e.ManipulationOrigin;
            _dragLock = DragLock.Unset;
            _dragging = false;
        }

        private void OnManipulationDelta(object sender, ManipulationDeltaEventArgs e)
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

        private void OnManipulationCompleted(object sender, ManipulationCompletedEventArgs e)
        {
            ManipulationDelta totalManipulation = null;

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

        private double CalculateContentDestination(AnimationDirection direction)
        {
            double destination = 0;
            double itemSize = Orientation == Orientation.Horizontal ? _itemsHostSize.Width : _itemsHostSize.Height;
            switch (direction)
            {
                case AnimationDirection.Previous:
                    destination = -itemSize;
                    break;
                case AnimationDirection.Next:
                    destination = itemSize;
                    break;
            }
            return destination;
        }

        private void GesturesComplete(ManipulationDelta totalManipulation)
        {
            if (ShouldHandleManipulation)
            {
                if (totalManipulation != null && _isEffectiveDragging)
                {
                    bool horizontal = Orientation == Orientation.Horizontal;
                    double translation = horizontal ? totalManipulation.Translation.X : totalManipulation.Translation.Y;
                    double absoluteTranslation = Math.Abs(translation);
                    double itemSize = horizontal ? _itemsHostSize.Width : _itemsHostSize.Height;
                    if (translation != 0 && absoluteTranslation >= itemSize / 2)
                    {
                        NavigateByIndexChange(translation < 0 ? 1 : -1);
                    }
                }

                if (!_animating)
                {
                    GoTo(CalculateContentDestination(AnimationDirection.Center), DefaultDuration, _easingFunction);
                }
            }

            _isEffectiveDragging = false;
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
                        NavigateByIndexChange(intAngle == 180 ? 1 : -1);
                        break;
                }
            }
        }

        private void Drag(ManipulationDeltaEventArgs e)
        {
            _isEffectiveDragging = true;

            if (_animating || !ShouldHandleManipulation)
            {
                return;
            }

            double targetOffset = Orientation == Orientation.Horizontal ? e.CumulativeManipulation.Translation.X : e.CumulativeManipulation.Translation.Y;

            if (SelectedIndex <= 0)
            {
                if (targetOffset > CompressLimit)
                {
                    targetOffset = CompressLimit;
                }
            }
            else if (SelectedIndex >= Items.Count - 1)
            {
                if (targetOffset < -CompressLimit)
                {
                    targetOffset = -CompressLimit;
                }
            }

            GoTo(targetOffset, ZeroDuration);
        }

        private void NavigateByIndexChange(int indexDelta)
        {
            if (_animating)
            {
                GoTo(CalculateContentDestination(_animationHint.Value), ZeroDuration);

                int newSelectedIndex = _deferredSelectedIndex.Value;

                _animationHint = null;
                _animating = false;
                _deferredSelectedIndex = null;

                SelectedIndex = newSelectedIndex;
            }

            if (indexDelta == 1)
            {
                if (SelectedIndex == Items.Count - 1)
                {
                    return;
                }
            }
            else
            {
                if (SelectedIndex == 0)
                {
                    return;
                }
            }

            _animationHint = indexDelta > 0 ? AnimationDirection.Previous : AnimationDirection.Next;
            _animating = true;
            _deferredSelectedIndex = SelectedIndex + indexDelta;

            GoTo(CalculateContentDestination(_animationHint.Value), DefaultDuration, _easingFunction, () =>
            {
                int newSelectedIndex = _deferredSelectedIndex.Value;

                _animationHint = null;
                _animating = false;
                _deferredSelectedIndex = null;

                SelectedIndex = newSelectedIndex;
            });
        }

        private void ReleaseMouseCaptureAtGestureOrigin()
        {
            if (_gestureSource != null)
            {
                FrameworkElement gestureSource = _gestureSource.Target as FrameworkElement;
                if (gestureSource != null)
                {
                    foreach (UIElement element in VisualTreeHelper.FindElementsInHostCoordinates(
                            gestureSource.TransformToVisual(null).Transform(_gestureOrigin), Application.Current.RootVisual))
                    {
                        element.ReleaseMouseCapture();
                    }
                }
            }
        }

        private void GoTo(double targetOffset, Duration duration)
        {
            GoTo(targetOffset, duration, null, null);
        }

        private void GoTo(double targetOffset, Duration duration, IEasingFunction easingFunction)
        {
            GoTo(targetOffset, duration, easingFunction, null);
        }

        private void GoTo(double targetOffset, Duration duration, IEasingFunction easingFunction, Action completionAction)
        {
            if (Animator.TryEnsureAnimator(ElementItemsPresenter, Orientation, ref _panAnimator))
            {
                _panAnimator.GoTo(targetOffset, duration, easingFunction, completionAction);
            }
        }

        private static double AngleFromVector(double x, double y)
        {
            double atan2 = Math.Atan2(y, x);
            if (atan2 < 0)
            {
                atan2 = 2 * Math.PI + atan2;
            }
            return atan2 * 360 / (2 * Math.PI);
        }

        private static bool AreValuesEqual(object o1, object o2)
        {
            if (o1 == o2)
            {
                return true;
            }
            if (o1 == null || o2 == null)
            {
                return false;
            }
            if (o1.GetType().IsValueType || o1.GetType() == typeof(string))
            {
                return Equals(o1, o2);
            }
            return ReferenceEquals(o1, o2);
        }

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

            [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
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

            [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
            public void GoTo(double targetOffset, Duration duration, Action completionAction)
            {
                GoTo(targetOffset, duration, null, completionAction);
            }

            [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
            public void GoTo(double targetOffset, Duration duration, IEasingFunction easingFunction)
            {
                GoTo(targetOffset, duration, easingFunction, null);
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

                return container.RenderTransform as CompositeTransform;
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

        private enum AnimationDirection
        {
            Center,
            Previous,
            Next
        }
    }
}