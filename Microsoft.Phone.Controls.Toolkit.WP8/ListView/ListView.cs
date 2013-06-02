using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
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
        private const string SelectorName = "Selector";
        private const string ItemContainerStyleName = "ItemContainerStyle";
        private const string JumpListStyleName = "JumpListStyle";

        private LongListSelector _selector;
        private HashSet<ListViewItem> _realizedItems = new HashSet<ListViewItem>();

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Microsoft.Phone.Controls.ListView"/> class.
        /// </summary>
        public ListView()
        {
            DefaultStyleKey = typeof(ListView);
        }

        #region ListHeaderTemplate

        /// <summary>
        /// Gets or sets the <see cref="T:System.Windows.DataTemplate"/>for an item to display at the head of the <see cref="T:Microsoft.Phone.Controls.LongListSelector"/>.
        /// </summary>
        /// 
        /// <returns>
        /// The <see cref="T:System.Windows.DataTemplate"/> for an item to display at the head of the <see cref="T:Microsoft.Phone.Controls.LongListSelector"/>.
        /// </returns>
        public DataTemplate ListHeaderTemplate
        {
            get { return (DataTemplate)GetValue(ListHeaderTemplateProperty); }
            set { SetValue(ListHeaderTemplateProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="P:Microsoft.Phone.Controls.LongListSelector.ListHeaderTemplate"/> dependency property.
        /// </summary>
        /// 
        /// <returns>
        /// The identifier for the <see cref="P:Microsoft.Phone.Controls.LongListSelector.ListHeaderTemplate"/> dependency property.
        /// </returns>
        public static readonly DependencyProperty ListHeaderTemplateProperty = DependencyProperty.Register(
            "ListHeaderTemplate",
            typeof(DataTemplate),
            typeof(ListView),
            new PropertyMetadata(null));

        #endregion

        #region ListHeader

        /// <summary>
        /// Gets or sets the object to display at the head of the <see cref="T:Microsoft.Phone.Controls.LongListSelector"/>.
        /// </summary>
        /// 
        /// <returns>
        /// The <see cref="T:System.Object"/> that is displayed at the head of the <see cref="T:Microsoft.Phone.Controls.LongListSelector"/>.
        /// </returns>
        public object ListHeader
        {
            get { return (object)GetValue(ListHeaderProperty); }
            set { SetValue(ListHeaderProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="P:Microsoft.Phone.Controls.LongListSelector.ListHeader"/> dependency property.
        /// </summary>
        /// 
        /// <returns>
        /// The identifier for the <see cref="P:Microsoft.Phone.Controls.LongListSelector.ListHeader"/> dependency property.
        /// </returns>
        public static readonly DependencyProperty ListHeaderProperty = DependencyProperty.Register(
            "ListHeader",
            typeof(object),
            typeof(ListView),
            new PropertyMetadata(null));

        #endregion

        #region ListFooterTemplate

        /// <summary>
        /// Gets or sets the <see cref="T:System.Windows.DataTemplate"/>for an item to display at the foot of the <see cref="T:Microsoft.Phone.Controls.LongListSelector"/>.
        /// </summary>
        /// 
        /// <returns>
        /// The <see cref="T:System.Windows.DataTemplate"/> for an item to display at the foot of the <see cref="T:Microsoft.Phone.Controls.LongListSelector"/>.
        /// </returns>
        public DataTemplate ListFooterTemplate
        {
            get { return (DataTemplate)GetValue(ListFooterTemplateProperty); }
            set { SetValue(ListFooterTemplateProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="P:Microsoft.Phone.Controls.LongListSelector.ListFooterTemplate"/> dependency property.
        /// </summary>
        /// 
        /// <returns>
        /// The identifier for the <see cref="P:Microsoft.Phone.Controls.LongListSelector.ListFooterTemplate"/> dependency property.
        /// </returns>
        public static readonly DependencyProperty ListFooterTemplateProperty = DependencyProperty.Register(
            "ListFooterTemplate",
            typeof(DataTemplate),
            typeof(ListView),
            new PropertyMetadata(null));

        #endregion

        #region ListFooter

        /// <summary>
        /// Gets or sets the object that is displayed at the foot of the <see cref="T:Microsoft.Phone.Controls.LongListSelector"/>.
        /// </summary>
        /// 
        /// <returns>
        /// The <see cref="T:System.Object"/> that is displayed at the foot of the <see cref="T:Microsoft.Phone.Controls.LongListSelector"/>.
        /// </returns>
        public object ListFooter
        {
            get { return (object)GetValue(ListFooterProperty); }
            set { SetValue(ListFooterProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="P:Microsoft.Phone.Controls.LongListSelector.ListFooter"/> dependency property.
        /// </summary>
        /// 
        /// <returns>
        /// The identifier for the <see cref="P:Microsoft.Phone.Controls.LongListSelector.ListFooter"/> dependency property.
        /// </returns>
        public static readonly DependencyProperty ListFooterProperty = DependencyProperty.Register(
            "ListFooter",
            typeof(object),
            typeof(ListView),
            new PropertyMetadata(null));

        #endregion

        #region GroupHeaderTemplate

        /// <summary>
        /// Gets or sets the template for the group header in the <see cref="T:Microsoft.Phone.Controls.LongListSelector"/>.
        /// </summary>
        /// 
        /// <returns>
        /// The <see cref="T:System.Windows.DataTemplate"/> for the group header in the <see cref="T:Microsoft.Phone.Controls.LongListSelector"/>.
        /// </returns>
        public DataTemplate GroupHeaderTemplate
        {
            get { return (DataTemplate)GetValue(GroupHeaderTemplateProperty); }
            set { SetValue(GroupHeaderTemplateProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="P:Microsoft.Phone.Controls.LongListSelector.GroupHeaderTemplate"/> dependency property.
        /// </summary>
        /// 
        /// <returns>
        /// The identifier for the <see cref="P:Microsoft.Phone.Controls.LongListSelector.GroupHeaderTemplate"/> dependency property.
        /// </returns>
        public static readonly DependencyProperty GroupHeaderTemplateProperty = DependencyProperty.Register(
            "GroupHeaderTemplate",
            typeof(DataTemplate),
            typeof(ListView),
            new PropertyMetadata(null));

        #endregion

        #region GroupFooterTemplate

        /// <summary>
        /// Gets or sets the template for the group footer in the <see cref="T:Microsoft.Phone.Controls.LongListSelector"/>.
        /// </summary>
        /// 
        /// <returns>
        /// The <see cref="T:System.Windows.DataTemplate"/> that provides the templates for the group footer in the <see cref="T:Microsoft.Phone.Controls.LongListSelector"/>.
        /// </returns>
        public DataTemplate GroupFooterTemplate
        {
            get { return (DataTemplate)GetValue(GroupFooterTemplateProperty); }
            set { SetValue(GroupFooterTemplateProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="P:Microsoft.Phone.Controls.LongListSelector.GroupFooterTemplate"/> dependency property.
        /// </summary>
        /// 
        /// <returns>
        /// The identifier for the <see cref="P:Microsoft.Phone.Controls.LongListSelector.GroupFooterTemplate"/> dependency property.
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
        public IEnumerable ItemsSource
        {
            get { return (IEnumerable)GetValue(ItemsSourceProperty); }
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
            typeof(IEnumerable),
            typeof(ListView),
            new PropertyMetadata(null, (d, e) => ((ListView)d).OnItemsSourceChanged(e)));

        private void OnItemsSourceChanged(DependencyPropertyChangedEventArgs e)
        {
            ApplyItemsSource();
        }

        private void ApplyItemsSource()
        {
            if (_selector == null)
            {
                return;
            }

            if (ItemsSource != null)
            {
                _selector.ItemsSource = ItemsSource as IList ?? ItemsSource.Cast<object>().ToList();
            }
            else
            {
                _selector.ItemsSource = null;
            }
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
            new PropertyMetadata(null, (d, e) => ((ListView)d).OnItemTemplateChanged(e)));

        private void OnItemTemplateChanged(DependencyPropertyChangedEventArgs e)
        {
            ApplyLiveItems(item => item.ContentTemplate = e.NewValue as DataTemplate);
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

            Style itemContainerStyle = ItemContainerStyle;
            if (itemContainerStyle != null)
            {
                listViewItem.Style = itemContainerStyle;
                listViewItem.IsStyleSetFromListView = true;
            }
            else
            {
                listViewItem.ClearValue(FrameworkElement.StyleProperty);
                listViewItem.IsStyleSetFromListView = false;
            }
        }

        #endregion

        #region LayoutMode

        /// <summary>
        /// Gets or sets a value that specifies if the <see cref="T:Microsoft.Phone.Controls.LongListSelector"/> is in a list mode or grid mode from the <see cref="T:Microsoft.Phone.Controls.LongListSelectorLayoutMode"/> enum.
        /// </summary>
        /// 
        /// <returns>
        /// A <see cref="T:Microsoft.Phone.Controls.LongListSelectorLayoutMode"/> value that specifies if the <see cref="T:Microsoft.Phone.Controls.LongListSelector"/> is in a list mode or grid mode.
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
        }

        #endregion

        #region GridCellSize

        /// <summary>
        /// Gets or sets the size used when displaying an item in the <see cref="T:Microsoft.Phone.Controls.LongListSelector"/>.
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
        /// Identifies the <see cref="P:Microsoft.Phone.Controls.LongListSelector.GridCellSize"/> dependency property.
        /// </summary>
        /// 
        /// <returns>
        /// The identifier for the <see cref="P:Microsoft.Phone.Controls.LongListSelector.GridCellSize"/> dependency property.
        /// </returns>
        public static readonly DependencyProperty GridCellSizeProperty = DependencyProperty.Register(
            "GridCellSize",
            typeof(Size),
            typeof(ListView),
            new PropertyMetadata(Size.Empty));

        #endregion

        #region HideEmptyGroups

        /// <summary>
        /// Gets or sets a value that indicates whether to hide empty groups in the <see cref="T:Microsoft.Phone.Controls.LongListSelector"/>.
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
        /// Identifies the <see cref="P:Microsoft.Phone.Controls.LongListSelector.HideEmptyGroups"/> dependency property.
        /// </summary>
        /// 
        /// <returns>
        /// The identifier for the <see cref="P:Microsoft.Phone.Controls.LongListSelector.HideEmptyGroups"/> dependency property.
        /// </returns>
        public static readonly DependencyProperty HideEmptyGroupsProperty = DependencyProperty.Register(
            "HideEmptyGroups",
            typeof(bool),
            typeof(ListView),
            new PropertyMetadata(false));

        #endregion

        #region IsGroupingEnabled

        /// <summary>
        /// Gets or sets a value that indicates whether grouping is enabled in the <see cref="T:Microsoft.Phone.Controls.LongListSelector"/>.
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
        /// Identifies the <see cref="P:Microsoft.Phone.Controls.LongListSelector.IsGroupingEnabled"/> dependency property.
        /// </summary>
        /// 
        /// <returns>
        /// The identifier for the <see cref="P:Microsoft.Phone.Controls.LongListSelector.IsGroupingEnabled"/> dependency property.
        /// </returns>
        public static readonly DependencyProperty IsGroupingEnabledProperty = DependencyProperty.Register(
            "IsGroupingEnabled",
            typeof(bool),
            typeof(ListView),
            new PropertyMetadata(false));

        #endregion

        #region JumpListStyle

        /// <summary>
        /// Gets or sets the <see cref="T:System.Windows.Style"/> for jump list in the <see cref="T:Microsoft.Phone.Controls.LongListSelector"/>.
        /// </summary>
        /// 
        /// <returns>
        /// The <see cref="T:System.Windows.Style"/> for the jump list in the <see cref="T:Microsoft.Phone.Controls.LongListSelector"/>.
        /// </returns>
        public Style JumpListStyle
        {
            get { return (Style)GetValue(JumpListStyleProperty); }
            set { SetValue(JumpListStyleProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="P:Microsoft.Phone.Controls.LongListSelector.JumpListStyle"/> dependency property.
        /// </summary>
        /// 
        /// <returns>
        /// The identifier for the <see cref="P:Microsoft.Phone.Controls.LongListSelector.JumpListStyle"/> dependency property.
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
                UpdateRealizedItemsSelectionState();
            }
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
                UpdateRealizedItemsSelectionState();
            }
        }

        #endregion

        /// <summary>
        /// Gets the state of manipulation handling on the <see cref="T:Microsoft.Phone.Controls.LongListSelector"/> control.
        /// </summary>
        /// 
        /// <returns>
        /// The state of manipulation handling on the <see cref="T:Microsoft.Phone.Controls.LongListSelector"/> control.
        /// </returns>
        public ManipulationState ManipulationState
        {
            get { return _selector == null ? ManipulationState.Idle : _selector.ManipulationState; }
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
            }

            _selector = this.GetTemplateChild(SelectorName) as LongListSelector;

            if (_selector != null)
            {
                _selector.LayoutMode = LayoutMode;
                _selector.SelectionChanged += OnSelectorSelectionChanged;
                _selector.ItemRealized += OnSelectorItemRealized;
                _selector.ItemUnrealized += OnSelectorItemUnrealized;
                _selector.JumpListOpening += OnSelectorJumpListOpening;
                _selector.JumpListClosed += OnSelectorJumpListClosed;
                _selector.ManipulationStateChanged += OnSelectorManipulationStateChanged;

                ApplyItemsSource();
                _selector.SelectedItem = SelectedItem;
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

                        listViewItem.ContentTemplate = ItemTemplate;

                        object item = e.Container.Content;

                        listViewItem.Item = item;
                        listViewItem.IsSelected = !IsItemClickEnabled && object.Equals(SelectedItem, item);

                        listViewItem.Tap += OnItemTap;

                        _realizedItems.Add(listViewItem);
                    }
                }
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

                item.ContentTemplate = ItemTemplate;
            }
        }

        private void UpdateRealizedItemsSelectionState()
        {
            bool isSelectionEnabled = !IsItemClickEnabled;
            ApplyLiveItems(item =>
            {
                item.IsSelected = isSelectionEnabled && object.Equals(item.Item, SelectedItem);
            });
        }
    }
}
