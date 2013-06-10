using Microsoft.Phone.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Media;

namespace Microsoft.Phone.Controls
{
    /// <summary>
    /// Represents a control that displays a vertical list of data items.
    /// </summary>
    [TemplatePart(Name = SelectorName, Type = typeof(LongListSelector))]
    [StyleTypedProperty(Property = ItemContainerStyleName, StyleTargetType = typeof(ListViewItem))]
    [StyleTypedProperty(Property = JumpListStyleName, StyleTargetType = typeof(LongListSelector))]
    public class ListView : Control
    {
        private const int MinValue = -0x800000;
        private const int MaxValue = 0x7FFF00;
        private const int DefaultMaxBufferedHeights = 64;
        private static readonly LoadMoreItemsResult EmptyLoadResult = new LoadMoreItemsResult();

        private const string SelectorName = "Selector";
        private const string ItemContainerStyleName = "ItemContainerStyle";
        private const string JumpListStyleName = "JumpListStyle";

        private IList<ListViewItem> _realizedItems = new List<ListViewItem>();
        private Queue<double> _itemHeights = new Queue<double>(DefaultMaxBufferedHeights);
        private int _maxBufferedHeights = DefaultMaxBufferedHeights;
        private int _gridColumnsInRow = 1;
        private double _bufferedItemHeights;
        private double _viewportMoveThreshold;

        private LongListSelector _selector;
        private ViewportControl _container;

        private Task<LoadMoreItemsResult> _loadOperation;
        private bool _areIncrementalLoadingSettingsDirty;
        private bool _ignoreBoundsChange;
        private bool _ignoreVerticalOffsetChange;
        private bool _deferLoadData;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Microsoft.Phone.Controls.ListView"/> class.
        /// </summary>
        public ListView()
        {
            DefaultStyleKey = typeof(ListView);

            _viewportMoveThreshold = Application.Current.Host.Content.ActualHeight / 8;
        }

        #region ListHeaderTemplate

        /// <summary>
        /// Gets or sets the <see cref="T:System.Windows.DataTemplate"/>for an item to display at the head of the <see cref="T:Microsoft.Phone.Controls.ListView"/>.
        /// </summary>
        /// 
        /// <returns>
        /// The <see cref="T:System.Windows.DataTemplate"/> for an item to display at the head of the <see cref="T:Microsoft.Phone.Controls.ListView"/>.
        /// </returns>
        public DataTemplate ListHeaderTemplate
        {
            get { return (DataTemplate)GetValue(ListHeaderTemplateProperty); }
            set { SetValue(ListHeaderTemplateProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="P:Microsoft.Phone.Controls.ListView.ListHeaderTemplate"/> dependency property.
        /// </summary>
        /// 
        /// <returns>
        /// The identifier for the <see cref="P:Microsoft.Phone.Controls.ListView.ListHeaderTemplate"/> dependency property.
        /// </returns>
        public static readonly DependencyProperty ListHeaderTemplateProperty = DependencyProperty.Register(
            "ListHeaderTemplate",
            typeof(DataTemplate),
            typeof(ListView),
            new PropertyMetadata(null));

        #endregion

        #region ListHeader

        /// <summary>
        /// Gets or sets the object to display at the head of the <see cref="T:Microsoft.Phone.Controls.ListView"/>.
        /// </summary>
        /// 
        /// <returns>
        /// The <see cref="T:System.Object"/> that is displayed at the head of the <see cref="T:Microsoft.Phone.Controls.ListView"/>.
        /// </returns>
        public object ListHeader
        {
            get { return (object)GetValue(ListHeaderProperty); }
            set { SetValue(ListHeaderProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="P:Microsoft.Phone.Controls.ListView.ListHeader"/> dependency property.
        /// </summary>
        /// 
        /// <returns>
        /// The identifier for the <see cref="P:Microsoft.Phone.Controls.ListView.ListHeader"/> dependency property.
        /// </returns>
        public static readonly DependencyProperty ListHeaderProperty = DependencyProperty.Register(
            "ListHeader",
            typeof(object),
            typeof(ListView),
            new PropertyMetadata(null));

        #endregion

        #region ListFooterTemplate

        /// <summary>
        /// Gets or sets the <see cref="T:System.Windows.DataTemplate"/>for an item to display at the foot of the <see cref="T:Microsoft.Phone.Controls.ListView"/>.
        /// </summary>
        /// 
        /// <returns>
        /// The <see cref="T:System.Windows.DataTemplate"/> for an item to display at the foot of the <see cref="T:Microsoft.Phone.Controls.ListView"/>.
        /// </returns>
        public DataTemplate ListFooterTemplate
        {
            get { return (DataTemplate)GetValue(ListFooterTemplateProperty); }
            set { SetValue(ListFooterTemplateProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="P:Microsoft.Phone.Controls.ListView.ListFooterTemplate"/> dependency property.
        /// </summary>
        /// 
        /// <returns>
        /// The identifier for the <see cref="P:Microsoft.Phone.Controls.ListView.ListFooterTemplate"/> dependency property.
        /// </returns>
        public static readonly DependencyProperty ListFooterTemplateProperty = DependencyProperty.Register(
            "ListFooterTemplate",
            typeof(DataTemplate),
            typeof(ListView),
            new PropertyMetadata(null));

        #endregion

        #region ListFooter

        /// <summary>
        /// Gets or sets the object that is displayed at the foot of the <see cref="T:Microsoft.Phone.Controls.ListView"/>.
        /// </summary>
        /// 
        /// <returns>
        /// The <see cref="T:System.Object"/> that is displayed at the foot of the <see cref="T:Microsoft.Phone.Controls.ListView"/>.
        /// </returns>
        public object ListFooter
        {
            get { return (object)GetValue(ListFooterProperty); }
            set { SetValue(ListFooterProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="P:Microsoft.Phone.Controls.ListView.ListFooter"/> dependency property.
        /// </summary>
        /// 
        /// <returns>
        /// The identifier for the <see cref="P:Microsoft.Phone.Controls.ListView.ListFooter"/> dependency property.
        /// </returns>
        public static readonly DependencyProperty ListFooterProperty = DependencyProperty.Register(
            "ListFooter",
            typeof(object),
            typeof(ListView),
            new PropertyMetadata(null));

        #endregion

        #region GroupHeaderTemplate

        /// <summary>
        /// Gets or sets the template for the group header in the <see cref="T:Microsoft.Phone.Controls.ListView"/>.
        /// </summary>
        /// 
        /// <returns>
        /// The <see cref="T:System.Windows.DataTemplate"/> for the group header in the <see cref="T:Microsoft.Phone.Controls.ListView"/>.
        /// </returns>
        public DataTemplate GroupHeaderTemplate
        {
            get { return (DataTemplate)GetValue(GroupHeaderTemplateProperty); }
            set { SetValue(GroupHeaderTemplateProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="P:Microsoft.Phone.Controls.ListView.GroupHeaderTemplate"/> dependency property.
        /// </summary>
        /// 
        /// <returns>
        /// The identifier for the <see cref="P:Microsoft.Phone.Controls.ListView.GroupHeaderTemplate"/> dependency property.
        /// </returns>
        public static readonly DependencyProperty GroupHeaderTemplateProperty = DependencyProperty.Register(
            "GroupHeaderTemplate",
            typeof(DataTemplate),
            typeof(ListView),
            new PropertyMetadata(null));

        #endregion

        #region GroupFooterTemplate

        /// <summary>
        /// Gets or sets the template for the group footer in the <see cref="T:Microsoft.Phone.Controls.ListView"/>.
        /// </summary>
        /// 
        /// <returns>
        /// The <see cref="T:System.Windows.DataTemplate"/> that provides the templates for the group footer in the <see cref="T:Microsoft.Phone.Controls.ListView"/>.
        /// </returns>
        public DataTemplate GroupFooterTemplate
        {
            get { return (DataTemplate)GetValue(GroupFooterTemplateProperty); }
            set { SetValue(GroupFooterTemplateProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="P:Microsoft.Phone.Controls.ListView.GroupFooterTemplate"/> dependency property.
        /// </summary>
        /// 
        /// <returns>
        /// The identifier for the <see cref="P:Microsoft.Phone.Controls.ListView.GroupFooterTemplate"/> dependency property.
        /// </returns>
        public static readonly DependencyProperty GroupFooterTemplateProperty = DependencyProperty.Register(
            "GroupFooterTemplate",
            typeof(DataTemplate),
            typeof(ListView),
            new PropertyMetadata(null));

        #endregion

        #region ItemsSource

        /// <summary>
        /// Gets or sets a collection used to generate the content of the <see cref="T:Microsoft.Phone.Controls.ListView"/>.
        /// </summary>
        /// 
        /// <returns>
        /// The  object that is used to generate the content of the <see cref="T:Microsoft.Phone.Controls.ListView"/>. The default is null.
        /// </returns>
        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "Framework LongListSelector as a R/W IList ItemsSource property.")]
        public IList ItemsSource
        {
            get { return (IList)GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="P:Microsoft.Phone.Controls.ListView.ItemsSource"/> dependency property.
        /// </summary>
        /// 
        /// <returns>
        /// The identifier for the <see cref="P:Microsoft.Phone.Controls.ListView.ItemsSource"/> dependency property.
        /// </returns>
        public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register(
            "ItemsSource",
            typeof(IList),
            typeof(ListView),
            new PropertyMetadata(null, (d, e) => ((ListView)d).OnItemsSourceChanged()));

        private void OnItemsSourceChanged()
        {
            if (_selector != null)
            {
                _selector.ItemsSource = ItemsSource;
            }

            ResetItemHeights();
            UpdateIsIncrementalLoadingEnabled();
        }

        #endregion

        #region ItemTemplate

        /// <summary>
        /// Gets or sets the <see cref="T:System.Windows.DataTemplate"/> used to display each item.
        /// </summary>
        /// 
        /// <returns>
        /// The template that specifies the visualization of the data objects. The default is null.
        /// </returns>
        public DataTemplate ItemTemplate
        {
            get { return (DataTemplate)GetValue(ItemTemplateProperty); }
            set { SetValue(ItemTemplateProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="P:Microsoft.Phone.Controls.ListView.ItemTemplate"/> dependency property.
        /// </summary>
        /// 
        /// <returns>
        /// The identifier for the <see cref="P:Microsoft.Phone.Controls.ListView.ItemTemplate"/> dependency property.
        /// </returns>
        public static readonly DependencyProperty ItemTemplateProperty = DependencyProperty.Register(
            "ItemTemplate",
            typeof(DataTemplate),
            typeof(ListView),
            new PropertyMetadata(null, (d, e) => ((ListView)d).OnItemTemplateChanged()));

        private void OnItemTemplateChanged()
        {
            ApplyLiveItems(ApplyItemTemplate);
        }

        private void ApplyItemTemplate(ListViewItem listViewItem)
        {
            DataTemplate template = ItemTemplate;

            if (template == null)
            {
                DataTemplateSelector selector = ItemTemplateSelector;
                if (selector != null)
                {
                    template = selector.SelectTemplate(listViewItem.Content, listViewItem);
                }
            }

            listViewItem.ContentTemplate = template;
        }

        #endregion

        #region ItemTemplateSelector

        /// <summary>
        /// Gets or sets the custom logic for choosing a template used to display each item.
        /// </summary>
        /// 
        /// <returns>
        /// A custom <see cref="T:System.Windows.Controls.DataTemplateSelector"/> object that provides logic and returns a <see cref="T:System.Windows.DataTemplate"/>. The default is null.
        /// </returns>
        public DataTemplateSelector ItemTemplateSelector
        {
            get { return (DataTemplateSelector)GetValue(ItemTemplateSelectorProperty); }
            set { SetValue(ItemTemplateSelectorProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="P:Microsoft.Phone.Controls.ListView.ItemTemplateSelector"/> dependency property.
        /// </summary>
        /// 
        /// <returns>
        /// The identifier for the <see cref="P:Microsoft.Phone.Controls.ListView.ItemTemplateSelector"/> dependency property.
        /// </returns>
        public static readonly DependencyProperty ItemTemplateSelectorProperty = DependencyProperty.Register(
            "ItemTemplateSelector",
            typeof(DataTemplateSelector),
            typeof(ListView),
            new PropertyMetadata(null, (d, e) => ((ListView)d).OnItemTemplateSelectorChanged()));

        private void OnItemTemplateSelectorChanged()
        {
            ApplyLiveItems(ApplyItemTemplate);
        }

        #endregion

        #region ItemContainerStyle

        /// <summary>
        /// Gets or sets the style that is used when rendering the item containers.
        /// </summary>
        /// 
        /// <returns>
        /// The style applied to the item containers. The default is null.
        /// </returns>
        public Style ItemContainerStyle
        {
            get { return (Style)GetValue(ItemContainerStyleProperty); }
            set { SetValue(ItemContainerStyleProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="P:Microsoft.Phone.Controls.ListView.ItemContainerStyle"/> dependency property.
        /// </summary>
        /// 
        /// <returns>
        /// The identifier for the <see cref="P:Microsoft.Phone.Controls.ListView.ItemContainerStyle"/> dependency property.
        /// </returns>
        public static readonly DependencyProperty ItemContainerStyleProperty = DependencyProperty.Register(
            ItemContainerStyleName,
            typeof(Style),
            typeof(ListView),
            new PropertyMetadata(null, (d, e) => ((ListView)d).OnItemContainerStyleChanged()));

        private void OnItemContainerStyleChanged()
        {
            ApplyLiveItems(ApplyItemContainerStyle);
        }

        private void ApplyItemContainerStyle(ListViewItem listViewItem)
        {
            if (listViewItem.ReadLocalValue(FrameworkElement.StyleProperty) != DependencyProperty.UnsetValue && !listViewItem.IsStyleSetFromListView)
            {
                return;
            }

            Style style = ItemContainerStyle;

            if (style == null)
            {
                StyleSelector selector = ItemContainerStyleSelector;
                if (selector != null)
                {
                    style = selector.SelectStyle(listViewItem.Content, listViewItem);
                }
            }

            if (style != null)
            {
                listViewItem.Style = style;
                listViewItem.IsStyleSetFromListView = true;
            }
            else
            {
                listViewItem.ClearValue(FrameworkElement.StyleProperty);
                listViewItem.IsStyleSetFromListView = false;
            }
        }

        #endregion

        #region ItemContainerStyleSelector

        /// <summary>
        /// Gets or sets custom style-selection logic for a style that can be applied to each generated container element.
        /// </summary>
        /// 
        /// <returns>
        /// A <see cref="T:System.Windows.Controls.StyleSelector"/> object that contains logic that chooses the style to use as the <see cref="P:Microsoft.Phone.Controls.ListView.ItemContainerStyle"/>. The default is null.
        /// </returns>
        public StyleSelector ItemContainerStyleSelector
        {
            get { return (StyleSelector)GetValue(ItemContainerStyleSelectorProperty); }
            set { SetValue(ItemContainerStyleSelectorProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="P:Microsoft.Phone.Controls.ListView.ItemContainerStyleSelector"/> dependency property.
        /// </summary>
        /// 
        /// <returns>
        /// The identifier for the <see cref="P:Microsoft.Phone.Controls.ListView.ItemContainerStyleSelector"/> dependency property.
        /// </returns>
        public static readonly DependencyProperty ItemContainerStyleSelectorProperty = DependencyProperty.Register(
            "ItemContainerStyleSelector",
            typeof(StyleSelector),
            typeof(ListView),
            new PropertyMetadata(null, (d, e) => ((ListView)d).OnItemContainerStyleSelectorChanged()));

        private void OnItemContainerStyleSelectorChanged()
        {
            ApplyLiveItems(ApplyItemContainerStyle);
        }

        #endregion

        #region LayoutMode

        /// <summary>
        /// Gets or sets a value that specifies if the <see cref="T:Microsoft.Phone.Controls.ListView"/> is in a list mode or grid mode from the <see cref="T:Microsoft.Phone.Controls.ListViewLayoutMode"/> enum.
        /// </summary>
        /// 
        /// <returns>
        /// A <see cref="T:Microsoft.Phone.Controls.ListViewLayoutMode"/> value that specifies if the <see cref="T:Microsoft.Phone.Controls.ListView"/> is in a list mode or grid mode.
        /// </returns>
        public LongListSelectorLayoutMode LayoutMode
        {
            get { return (LongListSelectorLayoutMode)GetValue(LayoutModeProperty); }
            set { SetValue(LayoutModeProperty, value); }
        }

        private static readonly DependencyProperty LayoutModeProperty = DependencyProperty.Register(
            "LayoutMode",
            typeof(LongListSelectorLayoutMode),
            typeof(ListView),
            new PropertyMetadata(LongListSelectorLayoutMode.List, (d, e) => ((ListView)d).OnLayoutModeChanged()));

        private void OnLayoutModeChanged()
        {
            if (_selector != null)
            {
                _selector.LayoutMode = LayoutMode;
            }

            if (LayoutMode == LongListSelectorLayoutMode.Grid)
            {
                ResetItemHeights();
            }

            UpdateGridColumns();
        }

        #endregion

        #region GridCellSize

        /// <summary>
        /// Gets or sets the size used when displaying an item in the <see cref="T:Microsoft.Phone.Controls.ListView"/>.
        /// </summary>
        /// 
        /// <returns>
        /// The size used when displaying an item.
        /// </returns>
        public Size GridCellSize
        {
            get { return (Size)GetValue(GridCellSizeProperty); }
            set { SetValue(GridCellSizeProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="P:Microsoft.Phone.Controls.ListView.GridCellSize"/> dependency property.
        /// </summary>
        /// 
        /// <returns>
        /// The identifier for the <see cref="P:Microsoft.Phone.Controls.ListView.GridCellSize"/> dependency property.
        /// </returns>
        public static readonly DependencyProperty GridCellSizeProperty = DependencyProperty.Register(
            "GridCellSize",
            typeof(Size),
            typeof(ListView),
            new PropertyMetadata(Size.Empty, (d, e) => ((ListView)d).OnGridCellSizeChanged()));

        private void OnGridCellSizeChanged()
        {
            if (LayoutMode == LongListSelectorLayoutMode.Grid)
            {
                UpdateGridColumns();
            }
        }

        #endregion

        #region HideEmptyGroups

        /// <summary>
        /// Gets or sets a value that indicates whether to hide empty groups in the <see cref="T:Microsoft.Phone.Controls.ListView"/>.
        /// </summary>
        /// 
        /// <returns>
        /// true if empty groups are hidden; otherwise false.Default is false.
        /// </returns>
        public bool HideEmptyGroups
        {
            get { return (bool)GetValue(HideEmptyGroupsProperty); }
            set { SetValue(HideEmptyGroupsProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="P:Microsoft.Phone.Controls.ListView.HideEmptyGroups"/> dependency property.
        /// </summary>
        /// 
        /// <returns>
        /// The identifier for the <see cref="P:Microsoft.Phone.Controls.ListView.HideEmptyGroups"/> dependency property.
        /// </returns>
        public static readonly DependencyProperty HideEmptyGroupsProperty = DependencyProperty.Register(
            "HideEmptyGroups",
            typeof(bool),
            typeof(ListView),
            new PropertyMetadata(false));

        #endregion

        #region IsGroupingEnabled

        /// <summary>
        /// Gets or sets a value that indicates whether grouping is enabled in the <see cref="T:Microsoft.Phone.Controls.ListView"/>.
        /// </summary>
        /// 
        /// <returns>
        /// true if grouping is enabled; otherwise false.
        /// </returns>
        public bool IsGroupingEnabled
        {
            get { return (bool)GetValue(IsGroupingEnabledProperty); }
            set { SetValue(IsGroupingEnabledProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="P:Microsoft.Phone.Controls.ListView.IsGroupingEnabled"/> dependency property.
        /// </summary>
        /// 
        /// <returns>
        /// The identifier for the <see cref="P:Microsoft.Phone.Controls.ListView.IsGroupingEnabled"/> dependency property.
        /// </returns>
        public static readonly DependencyProperty IsGroupingEnabledProperty = DependencyProperty.Register(
            "IsGroupingEnabled",
            typeof(bool),
            typeof(ListView),
            new PropertyMetadata(false, (d, e) => ((ListView)d).OnIsGroupingEnabledChanged()));

        private void OnIsGroupingEnabledChanged()
        {
            ResetItemHeights();
            UpdateIsIncrementalLoadingEnabled();
        }

        #endregion

        #region JumpListStyle

        /// <summary>
        /// Gets or sets the <see cref="T:System.Windows.Style"/> for jump list in the <see cref="T:Microsoft.Phone.Controls.ListView"/>.
        /// </summary>
        /// 
        /// <returns>
        /// The <see cref="T:System.Windows.Style"/> for the jump list in the <see cref="T:Microsoft.Phone.Controls.ListView"/>.
        /// </returns>
        public Style JumpListStyle
        {
            get { return (Style)GetValue(JumpListStyleProperty); }
            set { SetValue(JumpListStyleProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="P:Microsoft.Phone.Controls.ListView.JumpListStyle"/> dependency property.
        /// </summary>
        /// 
        /// <returns>
        /// The identifier for the <see cref="P:Microsoft.Phone.Controls.ListView.JumpListStyle"/> dependency property.
        /// </returns>
        public static readonly DependencyProperty JumpListStyleProperty = DependencyProperty.Register(
            JumpListStyleName,
            typeof(Style),
            typeof(ListView),
            new PropertyMetadata(null));

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
        /// Identifies the <see cref="P:Microsoft.Phone.Controls.ListView.SelectedItem"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty SelectedItemProperty = DependencyProperty.Register(
            "SelectedItem",
            typeof(object),
            typeof(ListView),
            new PropertyMetadata(null, (d, e) => ((ListView)d).OnSelectedItemChanged(e)));

        private void OnSelectedItemChanged(DependencyPropertyChangedEventArgs e)
        {
            if (_selector != null)
            {
                _selector.SelectedItem = e.NewValue;
            }

            if (!IsItemClickEnabled)
            {
                ApplySelection();
            }
        }

        private void ApplySelection()
        {
            bool isSelectionEnabled = !IsItemClickEnabled;
            ApplyLiveItems(item =>
            {
                item.IsSelected = isSelectionEnabled && object.Equals(item.Item, SelectedItem);
            });
        }

        #endregion

        #region IsItemClickEnabled

        /// <summary>
        /// Gets or sets a value that indicates whether items in the view raise an <see cref="E:Microsoft.Phone.Controls.ListView.ItemClick"/> event in response to interaction.
        /// </summary>
        /// 
        /// <returns>
        /// True if interaction raises an <see cref="E:Microsoft.Phone.Controls.ListView.ItemClick"/> event; otherwise, false. The default is true.
        /// </returns>
        public bool IsItemClickEnabled
        {
            get { return (bool)GetValue(IsItemClickEnabledProperty); }
            set { SetValue(IsItemClickEnabledProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="P:Microsoft.Phone.Controls.ListView.IsItemClickEnabled"/> dependency property.
        /// </summary>
        /// 
        /// <returns>
        /// The identifier for the <see cref="P:Microsoft.Phone.Controls.ListView.IsItemClickEnabled"/> dependency property.
        /// </returns>
        public static readonly DependencyProperty IsItemClickEnabledProperty = DependencyProperty.Register(
            "IsItemClickEnabled",
            typeof(bool),
            typeof(ListView),
            new PropertyMetadata(true, (d, e) => ((ListView)d).OnIsItemClickEnabledChanged()));

        private void OnIsItemClickEnabledChanged()
        {
            if (SelectedItem != null)
            {
                ApplySelection();
            }
        }

        #endregion

        #region DataFetchSize

        /// <summary>
        /// Gets or sets the amount of data to fetch for virtualizing/prefetch operations.
        /// </summary>
        /// 
        /// <returns>
        /// The amount of data to fetch per interval, in pages.
        /// </returns>
        public double DataFetchSize
        {
            get { return (double)GetValue(DataFetchSizeProperty); }
            set { SetValue(DataFetchSizeProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="P:Microsoft.Phone.Controls.ListView.DataFetchSize"/> dependency property.
        /// </summary>
        /// 
        /// <returns>
        /// The identifier for the <see cref="P:Microsoft.Phone.Controls.ListView.DataFetchSize"/> dependency property.
        /// </returns>
        public static readonly DependencyProperty DataFetchSizeProperty = DependencyProperty.Register(
            "DataFetchSize",
            typeof(double),
            typeof(ListView),
            new PropertyMetadata(3.0));

        #endregion

        #region IncrementalLoadingThreshold

        /// <summary>
        /// Gets or sets the threshold range that governs when the ListViewBase class will begin to prefetch more items.
        /// </summary>
        /// 
        /// <returns>
        /// The loading threshold, in terms of pages.
        /// </returns>
        public double IncrementalLoadingThreshold
        {
            get { return (double)GetValue(IncrementalLoadingThresholdProperty); }
            set { SetValue(IncrementalLoadingThresholdProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="P:Microsoft.Phone.Controls.ListView.IncrementalLoadingThreshold"/> dependency property.
        /// </summary>
        /// 
        /// <returns>
        /// The identifier for the <see cref="P:Microsoft.Phone.Controls.ListView.IncrementalLoadingThreshold"/> dependency property.
        /// </returns>
        public static readonly DependencyProperty IncrementalLoadingThresholdProperty = DependencyProperty.Register(
            "IncrementalLoadingThreshold",
            typeof(double),
            typeof(ListView),
            new PropertyMetadata((d, e) => ((ListView)d).OnIncrementalLoadingSettingsChanged()));

        #endregion

        #region IncrementalLoadingTrigger

        /// <summary>
        /// Gets or sets a value that indicates the conditions for prefetch operations by the ListView class.
        /// </summary>
        /// 
        /// <returns>
        /// An enumeration value that indicates the conditions that trigger prefetch operations. The default is Edge.
        /// </returns>
        public IncrementalLoadingTrigger IncrementalLoadingTrigger
        {
            get { return (IncrementalLoadingTrigger)GetValue(IncrementalLoadingTriggerProperty); }
            set { SetValue(IncrementalLoadingTriggerProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="P:Microsoft.Phone.Controls.ListView.IncrementalLoadingTrigger"/> dependency property.
        /// </summary>
        /// 
        /// <returns>
        /// The identifier for the <see cref="P:Microsoft.Phone.Controls.ListView.IncrementalLoadingTrigger"/> dependency property.
        /// </returns>
        public static readonly DependencyProperty IncrementalLoadingTriggerProperty = DependencyProperty.Register(
            "IncrementalLoadingTrigger",
            typeof(IncrementalLoadingTrigger),
            typeof(ListView),
            new PropertyMetadata(IncrementalLoadingTrigger.Edge, (d, e) => ((ListView)d).OnIncrementalLoadingSettingsChanged()));

        #endregion

        /// <summary>
        /// Gets the state of manipulation handling on the <see cref="T:Microsoft.Phone.Controls.ListView"/> control.
        /// </summary>
        /// 
        /// <returns>
        /// The state of manipulation handling on the <see cref="T:Microsoft.Phone.Controls.ListView"/> control.
        /// </returns>
        public ManipulationState ManipulationState
        {
            get { return _selector == null ? ManipulationState.Idle : _selector.ManipulationState; }
        }

        #region Bounds

        private Rect Bounds
        {
            get { return (Rect)GetValue(BoundsProperty); }
        }

        private static readonly DependencyProperty BoundsProperty = DependencyProperty.Register(
            "Bounds",
            typeof(Rect),
            typeof(ListView),
            new PropertyMetadata(new Rect(0, 0, float.MaxValue, float.MaxValue), (d, e) => ((ListView)d).OnBoundsChanged(e)));

        private void OnBoundsChanged(DependencyPropertyChangedEventArgs e)
        {
            if (_ignoreBoundsChange)
            {
                return;
            }

            _ignoreBoundsChange = true;

            Dispatcher.BeginInvoke(() =>
            {
                if (_ignoreBoundsChange)
                {
                    _ignoreBoundsChange = false;
                    OnBoundsChanged((Rect)e.OldValue, Bounds);
                }
            });
        }

        private void OnBoundsChanged(Rect oldValue, Rect newValue)
        {
            if (oldValue.Top != newValue.Top)
            {
                _areIncrementalLoadingSettingsDirty = true;
                UpdateVerticalOffset();
            }

            if (_areIncrementalLoadingSettingsDirty || oldValue.Height != newValue.Height)
            {
                OnIncrementalLoadingSettingsChanged();
            }
        }

        #endregion

        #region ViewportHeight

        private double _viewportHeight;
        private double ViewportHeight
        {
            get { return _viewportHeight; }
            set
            {
                if (_viewportHeight != value)
                {
                    _viewportHeight = value;
                    OnIncrementalLoadingSettingsChanged();
                }
            }
        }

        private void UpdateViewportHeight()
        {
            double containerViewportHeight = 0;

            if (_container != null)
            {
                containerViewportHeight = _container.Viewport.Height;
            }

            ViewportHeight = Math.Max(containerViewportHeight, ActualHeight);
        }

        #endregion

        #region VerticalOffset

        private double _verticalOffset;
        private double VerticalOffset
        {
            get { return _verticalOffset; }
            set
            {
                if (_verticalOffset != value)
                {
                    double oldValue = _verticalOffset;
                    _verticalOffset = value;
                    OnVerticalOffsetChanged(oldValue);
                }
            }
        }

        private void OnVerticalOffsetChanged(double oldValue)
        {
            if (_ignoreVerticalOffsetChange)
            {
                return;
            }

            _ignoreVerticalOffsetChange = true;

            Dispatcher.BeginInvoke(() =>
            {
                if (_ignoreVerticalOffsetChange)
                {
                    Dispatcher.BeginInvoke(() =>
                    {
                        if (_ignoreVerticalOffsetChange)
                        {
                            _ignoreVerticalOffsetChange = false;
                            OnVerticalOffsetChanged(oldValue, VerticalOffset);
                        }
                    });
                }
            });
        }

        private void OnVerticalOffsetChanged(double oldValue, double newValue)
        {
            if (newValue > oldValue)
            {
                OnIncrementalLoadingSettingsChanged();
            }
        }

        private void UpdateVerticalOffset(bool force = false)
        {
            if (_container != null)
            {
                double topBound = _container.Bounds.Top;
                if (topBound == MinValue)
                {
                    topBound = 0;
                }

                double verticalOffset = _container.Viewport.Top - topBound;
                if (force || Math.Abs(VerticalOffset - verticalOffset) > _viewportMoveThreshold)
                {
                    VerticalOffset = verticalOffset;
                }
            }
            else
            {
                VerticalOffset = 0;
            }
        }

        #endregion

        #region IsIncrementalLoadingEnabled

        private bool _isIncrementalLoadingEnabled;
        private bool IsIncrementalLoadingEnabled
        {
            get { return _isIncrementalLoadingEnabled; }
            set
            {
                if (_isIncrementalLoadingEnabled != value)
                {
                    _isIncrementalLoadingEnabled = value;
                    OnIsIncrementalLoadingEnabledChanged();
                }
            }
        }

        private void OnIsIncrementalLoadingEnabledChanged()
        {
            if (_container != null)
            {
                if (IsIncrementalLoadingEnabled)
                {
                    InitializeIncrementalLoading();
                }
                else
                {
                    UninitializeIncrementalLoading();
                }
            }
        }

        private void UpdateIsIncrementalLoadingEnabled()
        {
            IsIncrementalLoadingEnabled = ItemsSource is ISupportIncrementalLoading && !IsGroupingEnabled;
        }

        private void InitializeIncrementalLoading()
        {
            SizeChanged += OnSizeChanged;

            SetBinding(BoundsProperty, new Binding("Bounds") { Source = _container });
            _container.ViewportChanged += OnContainerViewportChanged;
            _container.SizeChanged += OnContainerSizeChanged;

            UpdateViewportHeight();
            UpdateVerticalOffset();
        }

        private void UninitializeIncrementalLoading()
        {
            SizeChanged -= OnSizeChanged;

            ClearValue(BoundsProperty);
            _container.ViewportChanged -= OnContainerViewportChanged;
            _container.SizeChanged -= OnContainerSizeChanged;
        }

        #endregion

        private double EstimatedItemHeight
        {
            get { return LayoutMode != LongListSelectorLayoutMode.Grid ? (_itemHeights.Count > 0 ? _bufferedItemHeights / _itemHeights.Count : 0) : GridCellSize.Height; }
        }

        private bool CanLoadMoreItems
        {
            get
            {
                if (_container != null && !IsGroupingEnabled)
                {
                    ISupportIncrementalLoading source = ItemsSource as ISupportIncrementalLoading;
                    if (source != null && source.HasMoreItems)
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        private bool AutomaticLoadingInProgress
        {
            get { return _loadOperation != null; }
        }

        private bool CanStartAutomaticLoading
        {
            get
            {
                return !AutomaticLoadingInProgress &&
                    IncrementalLoadingTrigger != IncrementalLoadingTrigger.None &&
                    CanLoadMoreItems &&
                    ViewportHeight > 0;
            }
        }

        /// <summary>
        /// Occurs when the currently selected item changes.
        /// </summary>
        public event SelectionChangedEventHandler SelectionChanged;

        /// <summary>
        /// Occurs when a new item is realized.
        /// </summary>
        public event EventHandler<ItemRealizationEventArgs> ItemRealized;

        /// <summary>
        /// Occurs when an item in the <see cref="T:Microsoft.Phone.Controls.ListView"/> is unrealized.
        /// </summary>
        public event EventHandler<ItemRealizationEventArgs> ItemUnrealized;

        /// <summary>
        /// Occurs when a jump list is opened.
        /// </summary>
        public event EventHandler JumpListOpening;

        /// <summary>
        /// Occurs when the jump list is closed.
        /// </summary>
        public event EventHandler JumpListClosed;

        /// <summary>
        /// Occurs when <see cref="T:Microsoft.Phone.Controls.ManipulationState"/> changes.
        /// </summary>
        public event EventHandler ManipulationStateChanged;

        /// <summary>
        /// Occurs when an item in the list view receives an interaction, and the <see cref="P:Microsoft.Phone.Controls.ListView.IsItemClickEnabled"/> property is true.
        /// </summary>
        public event ItemClickEventHandler ItemClick;

        /// <summary>
        /// Scrolls to a specified item in the <see cref="T:Microsoft.Phone.Controls.ListView"/>.
        /// </summary>
        /// <param name="item">The list item to scroll to.</param>
        public void ScrollTo(object item)
        {
            if (_selector != null)
            {
                _selector.ScrollTo(item);
            }
        }

        /// <summary>
        /// Initiates the asynchronous request to load more data items, in accordance with the active incremental loading settings.
        /// </summary>
        /// 
        /// <returns>
        /// A LoadMoreItemsResult payload.
        /// </returns>
        public Task<LoadMoreItemsResult> LoadMoreItemsAsync()
        {
            if (CanLoadMoreItems)
            {
                return ((ISupportIncrementalLoading)ItemsSource).LoadMoreItemsAsync(CalculateIncrementalLoadingCount());
            }

            return Task.FromResult(EmptyLoadResult);
        }

        /// <summary>
        /// Builds the visual tree for the <see cref="T:Microsoft.Phone.Controls.ListView"/> control when a new template is applied.
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _realizedItems.Clear();

            if (_selector != null)
            {
                _selector.SelectionChanged -= OnSelectorSelectionChanged;
                _selector.ItemRealized -= OnSelectorItemRealized;
                _selector.ItemUnrealized -= OnSelectorItemUnrealized;
                _selector.JumpListOpening -= OnSelectorJumpListOpening;
                _selector.JumpListClosed -= OnSelectorJumpListClosed;
                _selector.ManipulationStateChanged -= OnSelectorManipulationStateChanged;

                if (_container != null)
                {
                    if (IsIncrementalLoadingEnabled)
                    {
                        UninitializeIncrementalLoading();
                    }
                }
            }

            _selector = this.GetTemplateChild(SelectorName) as LongListSelector;
            _container = null;

            if (_selector != null)
            {
                _selector.LayoutMode = LayoutMode;
                _selector.SelectionChanged += OnSelectorSelectionChanged;
                _selector.ItemRealized += OnSelectorItemRealized;
                _selector.ItemUnrealized += OnSelectorItemUnrealized;
                _selector.JumpListOpening += OnSelectorJumpListOpening;
                _selector.JumpListClosed += OnSelectorJumpListClosed;
                _selector.ManipulationStateChanged += OnSelectorManipulationStateChanged;
                _selector.ItemsSource = ItemsSource;
                _selector.SelectedItem = SelectedItem;

                _container = _selector.GetFirstLogicalChildByType<ViewportControl>(true);
                if (_container != null)
                {
                    if (IsIncrementalLoadingEnabled)
                    {
                        InitializeIncrementalLoading();
                    }
                }
            }
        }

        /// <summary>
        /// Executes an action to all live items
        /// </summary>
        /// <param name="action"></param>
        protected void ApplyLiveItems(Action<ListViewItem> action)
        {
            if (action != null)
            {
                foreach (ListViewItem item in _realizedItems)
                {
                    action(item);
                }
            }
        }

        /// <summary>
        /// Configure an item's template according to the current state
        /// </summary>
        /// <param name="item"></param>
        internal void ConfigureItem(ListViewItem item)
        {
            if (item != null)
            {
                ApplyItemContainerStyle(item);

                ApplyItemTemplate(item);
            }
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateViewportHeight();
        }

        private void OnIncrementalLoadingSettingsChanged()
        {
            if (IsIncrementalLoadingEnabled)
            {
                _areIncrementalLoadingSettingsDirty = true;

                if (!_deferLoadData)
                {
                    LoadDataIfNecessary();
                }
            }
        }

        private void OnItemTap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            if (IsItemClickEnabled && ItemClick != null)
            {
                ItemClick(this, new ItemClickEventArgs(((ListViewItem)sender).Item));
            }
        }

        private void OnSelectorSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SelectedItem = _selector.SelectedItem;

            if (SelectionChanged != null)
            {
                SelectionChanged(this, e);
            }
        }

        private void OnSelectorItemRealized(object sender, ItemRealizationEventArgs e)
        {
            if (e.ItemKind == LongListSelectorItemKind.Item)
            {
                int childrenCount = VisualTreeHelper.GetChildrenCount(e.Container);
                if (childrenCount > 0)
                {
                    ListViewItem listViewItem = VisualTreeHelper.GetChild(e.Container, 0) as ListViewItem;
                    if (listViewItem != null)
                    {
                        ApplyItemContainerStyle(listViewItem);

                        ApplyItemTemplate(listViewItem);

                        object item = e.Container.Content;

                        listViewItem.Item = item;
                        listViewItem.IsSelected = !IsItemClickEnabled && object.Equals(SelectedItem, item);

                        listViewItem.Tap += OnItemTap;

                        _realizedItems.Add(listViewItem);
                    }
                }

                _maxBufferedHeights = Math.Max(_maxBufferedHeights, _realizedItems.Count + 1);

                if (_itemHeights.Count >= _maxBufferedHeights)
                {
                    _bufferedItemHeights -= _itemHeights.Dequeue();
                }

                _itemHeights.Enqueue(e.Container.DesiredSize.Height);
                _bufferedItemHeights += e.Container.DesiredSize.Height;
            }

            if (ItemRealized != null)
            {
                ItemRealized(sender, e);
            }
        }

        private void OnSelectorItemUnrealized(object sender, ItemRealizationEventArgs e)
        {
            if (e.ItemKind == LongListSelectorItemKind.Item)
            {
                int childrenCount = VisualTreeHelper.GetChildrenCount(e.Container);
                if (childrenCount > 0)
                {
                    ListViewItem listViewItem = VisualTreeHelper.GetChild(e.Container, 0) as ListViewItem;
                    if (listViewItem != null)
                    {
                        listViewItem.Item = null;

                        listViewItem.Tap -= OnItemTap;

                        _realizedItems.Remove(listViewItem);
                    }
                }
            }

            if (ItemUnrealized != null)
            {
                ItemUnrealized(sender, e);
            }
        }

        private void OnSelectorJumpListOpening(object sender, EventArgs e)
        {
            if (JumpListOpening != null)
            {
                JumpListOpening(sender, e);
            }
        }

        private void OnSelectorJumpListClosed(object sender, EventArgs e)
        {
            if (JumpListClosed != null)
            {
                JumpListClosed(sender, e);
            }
        }

        private void OnSelectorManipulationStateChanged(object sender, EventArgs e)
        {
            if (ManipulationStateChanged != null)
            {
                ManipulationStateChanged(sender, e);
            }

            if (IsIncrementalLoadingEnabled)
            {
                UpdateVerticalOffset(true);
            }
        }

        private void OnContainerViewportChanged(object sender, ViewportChangedEventArgs e)
        {
            _deferLoadData = true;
            UpdateViewportHeight();
            UpdateVerticalOffset();
            _deferLoadData = false;

            if (_areIncrementalLoadingSettingsDirty)
            {
                OnIncrementalLoadingSettingsChanged();
            }
        }

        private void OnContainerSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (e.NewSize.Width != e.PreviousSize.Width)
            {
                if (LayoutMode == LongListSelectorLayoutMode.Grid)
                {
                    UpdateGridColumns();
                }
            }
        }

        private void UpdateGridColumns()
        {
            int columns = 1;

            if (_container != null)
            {
                if (LayoutMode == LongListSelectorLayoutMode.Grid && _container.ActualWidth > GridCellSize.Width && GridCellSize.Width != 0)
                {
                    columns = (int)Math.Floor(_container.ActualWidth / GridCellSize.Width);
                }
            }

            if (_gridColumnsInRow != columns)
            {
                _gridColumnsInRow = columns;
            }
        }

        private void ResetItemHeights()
        {
            _itemHeights.Clear();
            _bufferedItemHeights = 0;
        }

        private uint CalculateIncrementalLoadingCount()
        {
            uint count = 0;

            double itemHeight = EstimatedItemHeight;
            if (itemHeight != 0)
            {
                double dataFetchSize = DataFetchSize;
                if (dataFetchSize > 0)
                {
                    double minCount = ViewportHeight / itemHeight * dataFetchSize;
                    if (LayoutMode == LongListSelectorLayoutMode.Grid)
                    {
                        minCount *= _gridColumnsInRow;
                    }
                    count = (uint)Math.Ceiling(minCount);
                }
            }

            return Math.Max(count, 1);
        }

        private void LoadDataIfNecessary()
        {
            if (_areIncrementalLoadingSettingsDirty)
            {
                _areIncrementalLoadingSettingsDirty = false;

                if (CanStartAutomaticLoading)
                {
                    if (((ISupportIncrementalLoading)ItemsSource).HasMoreItems)
                    {
                        double threshold = Math.Max(IncrementalLoadingThreshold, 0);
                        double viewportHeight = ViewportHeight;

                        double extentHeight;
                        if (ItemsSource.Count > 0)
                        {
                            extentHeight = _container.Bounds.Height;
                        }
                        else
                        {
                            extentHeight = 0;
                        }

                        if (extentHeight - viewportHeight - (VerticalOffset + viewportHeight) < viewportHeight * threshold)
                        {
                            LoadData();
                        }
                    }
                }
            }
        }

        private async void LoadData()
        {
            _loadOperation = ((ISupportIncrementalLoading)ItemsSource).LoadMoreItemsAsync(CalculateIncrementalLoadingCount());

            try
            {
                await _loadOperation;
            }
            finally
            {
                _loadOperation = null;
            }
        }
    }
}
