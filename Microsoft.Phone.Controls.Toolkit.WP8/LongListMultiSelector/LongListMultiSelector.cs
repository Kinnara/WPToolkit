// Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Media;

namespace Microsoft.Phone.Controls
{
    /// <summary>
    /// Extension of the standard LongListSelector control which allows multiple selection of items
    /// </summary>
    [StyleTypedProperty(Property = "ItemContainerStyle", StyleTargetType = typeof(LongListMultiSelectorItem))]
    [TemplatePart(Name = InnerSelectorName, Type = typeof(LongListSelector))]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Multi")]
    public class LongListMultiSelector : Control
    {
        #region Implementation Fields
        private const string InnerSelectorName = "InnerSelector";

        LongListSelector _innerSelector = null;
        HashSet<WeakReference<LongListMultiSelectorItem>> _realizedItems = new HashSet<WeakReference<LongListMultiSelectorItem>>();
        SelectedItemsList _selectedItems = new SelectedItemsList();

        // backing property as this is not a DependencyProperty in the LongListSelector
        LongListSelectorLayoutMode _layoutMode = LongListSelectorLayoutMode.List;


        /// <summary>
        ///    Gets the state of manipulation handling on the Microsoft.Phone.Controls.LongListSelector
        ///    control.
        /// </summary>
        public ManipulationState ManipulationState { get { return (_innerSelector == null) ? ManipulationState.Idle : _innerSelector.ManipulationState; } }

        #endregion

        #region Dependency Properties
        /// <summary>
        ///     Gets or sets the size used when displaying an item in the Microsoft.Phone.Controls.LongListMultiSelector.
        /// </summary>
        public Size GridCellSize
        {
            get { return (Size)GetValue(GridCellSizeProperty); }
            set { SetValue(GridCellSizeProperty, value); }
        }

        /// <summary>
        ///     Identifies the Microsoft.Phone.Controls.LongListMultiSelector.GridCellSize dependency
        ///     property.
        /// </summary>
        public static readonly DependencyProperty GridCellSizeProperty =
            DependencyProperty.Register("GridCellSize", typeof(Size), typeof(LongListMultiSelector), new PropertyMetadata(Size.Empty));

        /// <summary>
        ///     Gets or sets the template for the group footer in the Microsoft.Phone.Controls.LongListMultiSelector.
        /// </summary>
        public DataTemplate GroupFooterTemplate
        {
            get { return (DataTemplate)GetValue(GroupFooterTemplateProperty); }
            set { SetValue(GroupFooterTemplateProperty, value); }
        }

        /// <summary>
        ///     Identifies the Microsoft.Phone.Controls.LongListMultiSelector.GroupFooterTemplate dependency property.
        /// </summary>
        public static readonly DependencyProperty GroupFooterTemplateProperty =
            DependencyProperty.Register("GroupFooterTemplate", typeof(DataTemplate), typeof(LongListMultiSelector), new PropertyMetadata(null));

        /// <summary>
        ///    Gets or sets the template for the group header in the Microsoft.Phone.Controls.LongListMultiSelector.
        /// </summary>
        public DataTemplate GroupHeaderTemplate
        {
            get { return (DataTemplate)GetValue(GroupHeaderTemplateProperty); }
            set { SetValue(GroupHeaderTemplateProperty, value); }
        }

        /// <summary>
        ///    Identifies the Microsoft.Phone.Controls.LongListMultiSelector.GroupHeaderTemplate dependency property.
        /// </summary>
        public static readonly DependencyProperty GroupHeaderTemplateProperty =
            DependencyProperty.Register("GroupHeaderTemplate", typeof(DataTemplate), typeof(LongListMultiSelector), new PropertyMetadata(null));

        /// <summary>
        /// Gets or sets a value that indicates whether to hide empty groups in the Microsoft.Phone.Controls.LongListMultiSelector.
        /// </summary>
        public bool HideEmptyGroups
        {
            get { return (bool)GetValue(HideEmptyGroupsProperty); }
            set { SetValue(HideEmptyGroupsProperty, value); }
        }

        /// <summary>
        /// Identifies the Microsoft.Phone.Controls.LongListMultiSelector.HideEmptyGroups  dependency property.
        /// </summary>
        public static readonly DependencyProperty HideEmptyGroupsProperty =
            DependencyProperty.Register("HideEmptyGroups", typeof(bool), typeof(LongListMultiSelector), new PropertyMetadata(false));

        /// <summary>
        /// Gets or sets a value that indicates whether grouping is enabled in the Microsoft.Phone.Controls.LongListMultiSelector.
        /// </summary>
        public bool IsGroupingEnabled
        {
            get { return (bool)GetValue(IsGroupingEnabledProperty); }
            set { SetValue(IsGroupingEnabledProperty, value); }
        }

        /// <summary>
        /// Identifies the Microsoft.Phone.Controls.LongListMultiSelector.IsGroupingEnabled dependency property.
        /// </summary>
        public static readonly DependencyProperty IsGroupingEnabledProperty =
            DependencyProperty.Register("IsGroupingEnabled", typeof(bool), typeof(LongListMultiSelector), new PropertyMetadata(false));

        /// <summary>
        /// Gets or sets the template for items hosting in the Microsoft.Phone.Controls.LongListMultiSelector, targeted to customize selection highlighting
        /// </summary>
        public Style ItemContainerStyle
        {
            get { return (Style)GetValue(ItemContainerStyleProperty); }
            set { SetValue(ItemContainerStyleProperty, value); }
        }

        /// <summary>
        /// Identifies the Microsoft.Phone.Controls.LongListMultiSelector.ItemContainerStyle dependency property.
        /// </summary>
        public static readonly DependencyProperty ItemContainerStyleProperty =
            DependencyProperty.Register("ItemContainerStyle", typeof(Style), typeof(LongListMultiSelector), new PropertyMetadata(null, OnItemContainerStylePropertyChanged));

        /// <summary>
        /// Called when ItemContainerStyle property has been changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        static void OnItemContainerStylePropertyChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            LongListMultiSelector This = sender as LongListMultiSelector;
            if (This != null)
            {
                This.OnItemContainerStyleChanged();
            }
        }

        /// <summary>
        /// Gets or sets a collection used to generate the content of the Microsoft.Phone.Controls.LongListMultiSelector.
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "Framework LongListSelector as a R/W IList ItemsSource property.")]
        public IList ItemsSource
        {
            get { return (IList)GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }

        /// <summary>
        /// Identifies the Microsoft.Phone.Controls.LongListMultiSelector.ItemsSource dependency property.
        /// </summary>
        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register("ItemsSource", typeof(IList), typeof(LongListMultiSelector), new PropertyMetadata(null, OnItemsSourcePropertyChanged));

        /// <summary>
        /// Handles the change of ItemsSource property : disconnects event handler from old value and reconnects event handler to the new value
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        static void OnItemsSourcePropertyChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            LongListMultiSelector This = sender as LongListMultiSelector;
            if (This != null)
            {
                This.OnItemsSourceChanged(e.OldValue, e.NewValue);
            }
        }

        /// <summary>
        /// Gets or sets the template for the items in the items view
        /// </summary>
        public DataTemplate ItemTemplate
        {
            get { return (DataTemplate)GetValue(ItemTemplateProperty); }
            set { SetValue(ItemTemplateProperty, value); }
        }

        /// <summary>
        /// Identifies the Microsoft.Phone.Controls.LongListMultiSelector.ItemTemplate dependency property.
        /// </summary>
        public static readonly DependencyProperty ItemTemplateProperty =
            DependencyProperty.Register("ItemTemplate", typeof(DataTemplate), typeof(LongListMultiSelector), new PropertyMetadata(null));

        /// <summary>
        /// Gets or sets the data template that is to be right align in default template and will not move when selection is opened
        /// </summary>
        public DataTemplate ItemInfoTemplate
        {
            get { return (DataTemplate)GetValue(ItemInfoTemplateProperty); }
            set { SetValue(ItemInfoTemplateProperty, value); }
        }

        /// <summary>
        /// Identifies the ItemInfoTemplate dependency property.
        /// </summary>
        public static readonly DependencyProperty ItemInfoTemplateProperty =
            DependencyProperty.Register("ItemInfoTemplate", typeof(DataTemplate), typeof(LongListMultiSelector), new PropertyMetadata(null, OnItemInfoTemplatePropertyChanged));

        /// <summary>
        /// Called when ItemInfoTemplate property has been changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        static void OnItemInfoTemplatePropertyChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            LongListMultiSelector This = sender as LongListMultiSelector;
            if (This != null)
            {
                This.OnItemInfoTemplateChanged();
            }
        }

        /// <summary>
        /// Gets or sets the System.Windows.Style for jump list in the Microsoft.Phone.Controls.LongListSelector.
        /// </summary>
        public Style JumpListStyle
        {
            get { return (Style)GetValue(JumpListStyleProperty); }
            set { SetValue(JumpListStyleProperty, value); }
        }

        /// <summary>
        /// Identifies the Microsoft.Phone.Controls.LongListMultiSelector.JumpListStyle dependency property.
        /// </summary>
        public static readonly DependencyProperty JumpListStyleProperty =
            DependencyProperty.Register("JumpListStyle", typeof(Style), typeof(LongListMultiSelector), new PropertyMetadata(null));


        /// <summary>
        ///     Gets or sets a value that specifies if the Microsoft.Phone.Controls.LongListSelector
        ///     is in a list mode or grid mode from the Microsoft.Phone.Controls.LongListSelectorLayoutMode
        ///     enum.
        /// </summary>
        public LongListSelectorLayoutMode LayoutMode
        {
            get { return _layoutMode; }
            set
            {
                _layoutMode = value;
                if (_innerSelector != null)
                {
                    _innerSelector.LayoutMode = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets the object that is displayed at the foot of the Microsoft.Phone.Controls.LongListSelector.
        /// </summary>
        public object ListFooter
        {
            get { return (object)GetValue(ListFooterProperty); }
            set { SetValue(ListFooterProperty, value); }
        }

        /// <summary>
        /// Identifies the Microsoft.Phone.Controls.LongListMultiSelector.ListFooter dependency property.
        /// </summary>
        public static readonly DependencyProperty ListFooterProperty =
            DependencyProperty.Register("ListFooter", typeof(object), typeof(LongListMultiSelector), new PropertyMetadata(null));

        /// <summary>
        ///    Gets or sets the System.Windows.DataTemplatefor an item to display at the
        ///    foot of the Microsoft.Phone.Controls.LongListSelector.
        /// </summary>
        public DataTemplate ListFooterTemplate
        {
            get { return (DataTemplate)GetValue(ListFooterTemplateProperty); }
            set { SetValue(ListFooterTemplateProperty, value); }
        }

        /// <summary>
        /// Identifies the Microsoft.Phone.Controls.LongListMultiSelector.ListFooterTemplate dependency property.
        /// </summary>
        public static readonly DependencyProperty ListFooterTemplateProperty =
            DependencyProperty.Register("ListFooterTemplate", typeof(DataTemplate), typeof(LongListMultiSelector), new PropertyMetadata(null));

        /// <summary>
        /// Gets or sets the object to display at the head of the Microsoft.Phone.Controls.LongListSelector.
        /// </summary>
        public object ListHeader
        {
            get { return (object)GetValue(ListHeaderProperty); }
            set { SetValue(ListHeaderProperty, value); }
        }

        /// <summary>
        /// Identifies the Microsoft.Phone.Controls.LongListMultiSelector.ListHeader dependency property.
        /// </summary>
        public static readonly DependencyProperty ListHeaderProperty =
            DependencyProperty.Register("ListHeader", typeof(object), typeof(LongListMultiSelector), new PropertyMetadata(null));

        /// <summary>
        ///    Gets or sets the System.Windows.DataTemplatefor an item to display at the
        ///    head of the Microsoft.Phone.Controls.LongListSelector.
        /// </summary>
        public DataTemplate ListHeaderTemplate
        {
            get { return (DataTemplate)GetValue(ListHeaderTemplateProperty); }
            set { SetValue(ListHeaderTemplateProperty, value); }
        }

        /// <summary>
        ///    Identifies the Microsoft.Phone.Controls.LongListMultiSelector.ListHeaderTemplate dependency
        ///    property.
        /// </summary>
        public static readonly DependencyProperty ListHeaderTemplateProperty =
            DependencyProperty.Register("ListHeaderTemplate", typeof(DataTemplate), typeof(LongListMultiSelector), new PropertyMetadata(null));

        /// <summary>
        ///    Gets the currently selected items in the Microsoft.Phone.Controls.LongListSelector.
        /// </summary>
        public IList SelectedItems
        {
            get { return (IList)GetValue(SelectedItemsProperty); }
        }

        /// <summary>
        ///    Identifies the Microsoft.Phone.Controls.LongListMultiSelector.SelectedItems dependency
        ///    property.
        /// </summary>
        public static readonly DependencyProperty SelectedItemsProperty =
            DependencyProperty.Register("SelectedItems", typeof(IList), typeof(LongListMultiSelector), new PropertyMetadata(null));



        /// <summary>
        /// Gets or sets the flag that indicates if the list
        /// is in selection mode or not.
        /// </summary>
        public bool IsSelectionEnabled
        {
            get { return (bool)GetValue(IsSelectionEnabledProperty); }
            set { SetValue(IsSelectionEnabledProperty, value); }
        }

        /// <summary>
        /// Identifies the IsSelectionEnabled dependency property.
        /// </summary>
        public static readonly DependencyProperty IsSelectionEnabledProperty =
            DependencyProperty.Register("IsSelectionEnabled", typeof(bool), typeof(LongListMultiSelector), new PropertyMetadata(false, OnIsSelectionEnabledPropertyChanged));


        /// <summary>
        /// Gets or sets the EnforceIsSelectionEnabled property
        /// </summary>
        public bool EnforceIsSelectionEnabled
        {
            get { return (bool)GetValue(EnforceIsSelectionEnabledProperty); }
            set { SetValue(EnforceIsSelectionEnabledProperty, value); }
        }

        /// <summary>
        /// Identifies the EnforceIsSelectionEnabled dependency property.
        /// </summary>
        public static readonly DependencyProperty EnforceIsSelectionEnabledProperty =
            DependencyProperty.Register("EnforceIsSelectionEnabled", typeof(bool), typeof(LongListMultiSelector), new PropertyMetadata(false, OnEnforceIsSelectionEnabledPropertyChanged));





        /// <summary>
        /// Gets or sets the DefaultListItemContainerStyle property
        /// </summary>
        internal Style DefaultListItemContainerStyle
        {
            get { return (Style)GetValue(DefaultListItemContainerStyleProperty); }
            set { SetValue(DefaultListItemContainerStyleProperty, value); }
        }

        /// <summary>
        /// Identifies the DefaultListItemContainerStyle dependency property.
        /// </summary>
        internal static readonly DependencyProperty DefaultListItemContainerStyleProperty =
            DependencyProperty.Register("DefaultListItemContainerStyle", typeof(Style), typeof(LongListMultiSelector), new PropertyMetadata(null));

        /// <summary>
        /// Gets or sets the DefaultGridItemContainerStyle property
        /// </summary>
        internal Style DefaultGridItemContainerStyle
        {
            get { return (Style)GetValue(DefaultGridItemContainerStyleProperty); }
            set { SetValue(DefaultGridItemContainerStyleProperty, value); }
        }

        /// <summary>
        /// Identifies the DefaultGridItemContainerStyle dependency property.
        /// </summary>
        internal static readonly DependencyProperty DefaultGridItemContainerStyleProperty =
            DependencyProperty.Register("DefaultGridItemContainerStyle", typeof(Style), typeof(LongListMultiSelector), new PropertyMetadata(null));


        #endregion

        #region Events
        /// <summary>
        ///    Occurs when a new item is realized.
        /// </summary>
        public event EventHandler<ItemRealizationEventArgs> ItemRealized;

        /// <summary>
        ///    Occurs when an item in the Microsoft.Phone.Controls.LongListMultiSelector is unrealized.
        /// </summary>
        public event EventHandler<ItemRealizationEventArgs> ItemUnrealized;

        /// <summary>
        ///    Occurs when the jump list is closed.
        /// </summary>
        public event EventHandler JumpListClosed;

        /// <summary>
        ///    Occurs when a jump list is opened.
        /// </summary>
        public event EventHandler JumpListOpening;

        /// <summary>
        ///    Occurs when Microsoft.Phone.Controls.ManipulationState changes.
        /// </summary>
        public event EventHandler ManipulationStateChanged;

        /// <summary>
        ///    Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        ///    Occurs when the currently selected item changes.
        /// </summary>
        public event SelectionChangedEventHandler SelectionChanged;

        /// <summary>
        /// Occurs when the selection mode is opened or closed.
        /// </summary>
        public event DependencyPropertyChangedEventHandler IsSelectionEnabledChanged;
        #endregion

        /// <summary>
        /// Initializes a new instance of the Microsoft.Phone.Controls.LongListMultiSelector
        /// </summary>
        public LongListMultiSelector()
        {
            this.DefaultStyleKey = typeof(LongListMultiSelector);
            this.SetValue(SelectedItemsProperty, _selectedItems);
            _selectedItems.CollectionCleared += OnSelectedItemsCollectionCleared;
            ((INotifyCollectionChanged)_selectedItems).CollectionChanged += OnSelectedItemsCollectionChanged;
        }


        /// <summary>
        /// Template application : gets and hooks the inner LongListSelector
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _realizedItems.Clear();
            if (_innerSelector != null)
            {
                _innerSelector.ItemRealized -= OnInnerSelectorItemRealized;
                _innerSelector.ItemUnrealized -= OnInnerSelectorItemUnrealized;
                _innerSelector.JumpListClosed -= OnInnerSelectorJumpListClosed;
                _innerSelector.JumpListOpening -= OnInnerSelectorJumpListOpening;
                _innerSelector.ManipulationStateChanged -= OnInnerSelectorManipulationStateChanged;
                _innerSelector.PropertyChanged -= OnInnerSelectorPropertyChanged;
            }
            _innerSelector = this.GetTemplateChild(InnerSelectorName) as LongListSelector;
            if (_innerSelector != null)
            {
                _innerSelector.LayoutMode = LayoutMode;
                _innerSelector.ItemRealized += OnInnerSelectorItemRealized;
                _innerSelector.ItemUnrealized += OnInnerSelectorItemUnrealized;
                _innerSelector.JumpListClosed += OnInnerSelectorJumpListClosed;
                _innerSelector.JumpListOpening += OnInnerSelectorJumpListOpening;
                _innerSelector.ManipulationStateChanged += OnInnerSelectorManipulationStateChanged;
                _innerSelector.PropertyChanged += OnInnerSelectorPropertyChanged;
            }
        }

        /// <summary>
        /// Applyies the new style to already realized items
        /// </summary>
        void OnItemContainerStyleChanged()
        {
            ApplyLiveItems(item =>
            {
                item.Style = ItemContainerStyle;
            });
        }

        /// <summary>
        /// Called when the ItemsSource property if the LLMS has been changed
        /// </summary>
        /// <param name="oldValue"></param>
        /// <param name="newValue"></param>
        protected virtual void OnItemsSourceChanged(object oldValue, object newValue)
        {
            INotifyCollectionChanged eventSource = oldValue as INotifyCollectionChanged;
            if (eventSource != null)
            {
                eventSource.CollectionChanged -= OnItemsSourceCollectionChanged;
            }
            eventSource = newValue as INotifyCollectionChanged;
            if (eventSource != null)
            {
                eventSource.CollectionChanged += OnItemsSourceCollectionChanged;
            }
            SelectedItems.Clear();
        }

        /// <summary>
        /// Handles changes inside the ItemSource collection : removes removed item from the selection
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected virtual void OnItemsSourceCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if ((e != null) && (e.OldItems != null))
            {
                UnselectItems(e.OldItems);
            }
        }

        /// <summary>
        /// Applyies the new style to already realized items
        /// </summary>
        protected virtual void OnItemInfoTemplateChanged()
        {
            ApplyLiveItems(item =>
            {
                item.ContentInfoTemplate = ItemInfoTemplate;
            });
        }

        /// <summary>
        /// Relays event from the inner LongListSelector
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void OnInnerSelectorPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(sender, e);
            }
        }

        /// <summary>
        /// Relays event from the inner LongListSelector
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void OnInnerSelectorManipulationStateChanged(object sender, EventArgs e)
        {
            if (ManipulationStateChanged != null)
            {
                ManipulationStateChanged(sender, e);
            }
        }

        /// <summary>
        /// Relays event from the inner LongListSelector
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void OnInnerSelectorJumpListOpening(object sender, EventArgs e)
        {
            if (JumpListOpening != null)
            {
                JumpListOpening(sender, e);
            }
        }

        /// <summary>
        /// Relays event from the inner LongListSelector
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void OnInnerSelectorJumpListClosed(object sender, EventArgs e)
        {
            if (JumpListClosed != null)
            {
                JumpListClosed(sender, e);
            }
        }

        /// <summary>
        /// Disconnects an item when it is unrealized
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void OnInnerSelectorItemUnrealized(object sender, ItemRealizationEventArgs e)
        {
            if (e.ItemKind == LongListSelectorItemKind.Item)
            {
                int childrenCount = VisualTreeHelper.GetChildrenCount(e.Container);
                if (childrenCount > 0)
                {
                    LongListMultiSelectorItem llItem = VisualTreeHelper.GetChild(e.Container, 0) as LongListMultiSelectorItem;
                    if (llItem != null)
                    {
                        llItem.IsSelectedChanged -= OnLongListMultiSelectorItemIsSelectedChanged;

                        _realizedItems.Remove(llItem.WR);
                    }
                }
            }
            if (ItemUnrealized != null)
            {
                ItemUnrealized(sender, e);
            }
        }

        /// <summary>
        /// Configure an item's template according to the current state
        /// </summary>
        /// <param name="item"></param>
        internal void ConfigureItem(LongListMultiSelectorItem item)
        {
            if (item != null)
            {
                item.ContentTemplate = ItemTemplate;

                if (ItemContainerStyle != null)
                {
                    if (item.Style != ItemContainerStyle)
                    {
                        item.Style = ItemContainerStyle;
                    }
                }
                else if (LayoutMode == LongListSelectorLayoutMode.Grid)
                {
                    if (item.Style != DefaultGridItemContainerStyle)
                    {
                        item.Style = DefaultGridItemContainerStyle;
                    }
                }
                else
                {
                    if (item.Style != DefaultListItemContainerStyle)
                    {
                        item.Style = DefaultListItemContainerStyle;
                    }
                }
                if ((ItemInfoTemplate != null) && (item.ContentInfoTemplate != ItemInfoTemplate))
                {
                    item.SetBinding(LongListMultiSelectorItem.ContentInfoProperty, new Binding());
                    item.ContentInfoTemplate = ItemInfoTemplate;
                }
            }
        }

        /// <summary>
        /// Called when an item is realized : 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void OnInnerSelectorItemRealized(object sender, ItemRealizationEventArgs e)
        {
            if (e.ItemKind == LongListSelectorItemKind.Item)
            {
                int childrenCount = VisualTreeHelper.GetChildrenCount(e.Container);
                if (childrenCount > 0)
                {
                    LongListMultiSelectorItem llItem = VisualTreeHelper.GetChild(e.Container, 0) as LongListMultiSelectorItem;
                    if (llItem != null)
                    {
                        ConfigureItem(llItem);

                        // Check if item should be selected
                        llItem.IsSelected = _selectedItems.Contains(llItem.Content);
                        llItem.IsSelectedChanged += OnLongListMultiSelectorItemIsSelectedChanged;
                        llItem.GotoState(IsSelectionEnabled ? LongListMultiSelectorItem.State.Opened : LongListMultiSelectorItem.State.Closed);

                        _realizedItems.Add(llItem.WR);
                    }
                }
            }
            if (ItemRealized != null)
            {
                ItemRealized(sender, e);
            }
        }

        /// <summary>
        /// Called when an Item's IsSelected property has changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void OnLongListMultiSelectorItemIsSelectedChanged(object sender, EventArgs e)
        {
            LongListMultiSelectorItem item = sender as LongListMultiSelectorItem;
            if (item != null)
            {
                object content = item.Content;
                if (content != null)
                {
                    if (item.IsSelected)
                    {
                        // Check if the item is already in the SelectedItems
                        // collection, otherwise a double-add will happen.
                        if (!SelectedItems.Contains(content))
                        {
                            SelectedItems.Add(content);
                        }
                    }
                    else
                    {
                        SelectedItems.Remove(content);
                    }
                }
            }
        }

        /// <summary>
        /// Changes the state of the items given the value of the property
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        static void OnIsSelectionEnabledPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            LongListMultiSelector This = sender as LongListMultiSelector;
            if (This != null)
            {
                This.OnIsSelectionEnabledChanged(e);
            }
        }

        /// <summary>
        /// Called when the IsSelectionEnabled property is changed.
        /// </summary>
        /// <param name="e">DependencyPropertyChangedEventArgs associated to the event</param>
        protected virtual void OnIsSelectionEnabledChanged(DependencyPropertyChangedEventArgs e)
        {
            bool newValue = (bool)e.NewValue;
            if (!newValue)
            {
                SelectedItems.Clear();
            }
            ApplyItemsState(newValue ? LongListMultiSelectorItem.State.Opened : LongListMultiSelectorItem.State.Closed, true);
            if (IsSelectionEnabledChanged != null)
            {
                IsSelectionEnabledChanged(this, e);
            }
        }

        /// <summary>
        /// Called when the OnEnforceIsSelectionEnabled dependency property has been changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        static void OnEnforceIsSelectionEnabledPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            LongListMultiSelector This = sender as LongListMultiSelector;
            if (This != null)
            {
                This.OnEnforceIsSelectionEnabledChanged();
            }
        }

        /// <summary>
        /// Called when the OnEnforceIsSelectionEnabled property has been changed
        /// </summary>
        protected virtual void OnEnforceIsSelectionEnabledChanged()
        {
            if (!EnforceIsSelectionEnabled)
            {
                SelectedItems.Clear();
            }
            UpdateIsSelectionEnabled();
        }

        /// <summary>
        /// Updates the IsSelectionEnabled property according to the possibly enforced value and the selected items count
        /// </summary>
        protected virtual void UpdateIsSelectionEnabled()
        {
            IsSelectionEnabled = (EnforceIsSelectionEnabled || (SelectedItems.Count > 0));
        }

        /// <summary>
        /// Triggers SelectionChanged event and updates the IsSelectionEnabled property
        /// </summary>
        /// <param name="removedItems"></param>
        /// <param name="addedItems"></param>
        void OnSelectionChanged(IList removedItems, IList addedItems)
        {
            UpdateIsSelectionEnabled();
            if (SelectionChanged != null)
            {
                SelectionChanged(this, new SelectionChangedEventArgs(removedItems ?? new List<object>(), addedItems ?? new List<object>()));
            }
        }


        /// <summary>
        /// Executes an action to all live items and cleanup dead references
        /// </summary>
        /// <param name="action"></param>
        protected void ApplyLiveItems(Action<LongListMultiSelectorItem> action)
        {
            if (action != null)
            {
                HashSet<WeakReference<LongListMultiSelectorItem>> liveItems = new HashSet<WeakReference<LongListMultiSelectorItem>>();
                foreach (var wr in _realizedItems)
                {
                    LongListMultiSelectorItem item;
                    if (wr.TryGetTarget(out item))
                    {
                        action(item);
                        liveItems.Add(wr);
                    }
                }
                _realizedItems = liveItems;
            }
        }

        /// <summary>
        /// Handles selection changes made throught the SelectedItems property
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void OnSelectedItemsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Reset:
                    // Do nothing as the event CollectionCleared will have the removed items
                    break;
                case NotifyCollectionChangedAction.Add:
                    SelectItems(e.NewItems);
                    OnSelectionChanged(null, e.NewItems);
                    break;
                case NotifyCollectionChangedAction.Remove:
                    UnselectItems(e.OldItems);
                    OnSelectionChanged(e.OldItems, null);
                    break;
                case NotifyCollectionChangedAction.Replace:
                    UnselectItems(e.OldItems);
                    SelectItems(e.NewItems);
                    OnSelectionChanged(e.OldItems, e.NewItems);
                    break;
            }
        }

        void OnSelectedItemsCollectionCleared(object sender, LongListMultiSelector.ClearedChangedArgs e)
        {
            ApplyLiveItems(item => item.IsSelected = false);
            OnSelectionChanged(e.OldItems, null);
        }


        /// <summary>
        /// Selects the LongListMultiSelectorItems whose content matches the provided list
        /// </summary>
        /// <param name="items">List of content (i.e. from ItemsSource)</param>
        void SelectItems(IList items)
        {
            ApplyLiveItems(item =>
            {
                if (items.Contains(item.Content))
                {
                    item.IsSelected = true;
                }
            });
        }

        /// <summary>
        /// Unselects the LongListMultiSelectorItems whose content matches the provided list
        /// </summary>
        /// <param name="items">List of content (i.e. from ItemsSource)</param>
        void UnselectItems(IList items)
        {
            ApplyLiveItems(item =>
            {
                if (items.Contains(item.Content))
                {
                    item.IsSelected = false;
                }
            });
        }

        /// <summary>
        /// Returns the LongListMultiSelectorItem corresponding to the given item
        /// </summary>
        /// <param name="item">Item whose container has to be returned</param>
        /// <returns></returns>
        public object ContainerFromItem(object item)
        {
            object ret = null;
            ApplyLiveItems(llmsItem =>
            {
                if (llmsItem.Content == item)
                {
                    ret = llmsItem;
                }
            });
            return ret;
        }

        /// <summary>
        /// Scrolls to a specified item in the Microsoft.Phone.Controls.LongListSelector.
        /// </summary>
        /// <param name="item">The list item to scroll to.</param>
        public void ScrollTo(object item)
        {
            if (_innerSelector != null)
            {
                _innerSelector.ScrollTo(item);
            }
        }

        /// <summary>
        /// Applies a new state to all items. Visible items will use transitions if useTransitions parameter is set, others will not
        /// </summary>
        /// <param name="state">State to apply</param>
        /// <param name="useTransitions">Specify whether to use transitions or not for visible items</param>
        private void ApplyItemsState(LongListMultiSelectorItem.State state, bool useTransitions)
        {
            // Only apply state change if the LLMS has been displayed and items realized
            if (_innerSelector != null)
            {
                LongListMultiSelectorItem item;
                if (useTransitions)
                {
                    List<LongListMultiSelectorItem> invisibleItems = new List<LongListMultiSelectorItem>();
                    GeneralTransform itemTransform;
                    double bottom = _innerSelector.ActualHeight;
                    foreach (var wr in _realizedItems)
                    {
                        if (wr.TryGetTarget(out item))
                        {
                            itemTransform = item.TransformToVisual(_innerSelector);
                            Point pt = itemTransform.Transform(new Point(0.0, 0.0));
                            bool isVisible;
                            if (pt.Y > bottom)
                            {
                                // item's bottom will also be outside of inner lls
                                isVisible = false;
                            }
                            else if (pt.Y >= 0)
                            {
                                // whatever the position of item's botton, its top is visible
                                isVisible = true;
                            }
                            else
                            {
                                // item's bottom is visible if >= 0
                                pt = itemTransform.Transform(new Point(item.ActualHeight, 0));
                                isVisible = pt.Y >= 0;
                            }
                            if (isVisible)
                            {
                                item.GotoState(state, true);
                            }
                            else
                            {
                                invisibleItems.Add(item);
                            }
                        }
                    }
                    // now change asynchronously the state of invisible items
                    Dispatcher.BeginInvoke(() =>
                    {
                        foreach (var invisibleItem in invisibleItems)
                        {
                            invisibleItem.GotoState(state, false);
                        }
                    });
                }
                else
                {
                    foreach (var wr in _realizedItems)
                    {
                        if (wr.TryGetTarget(out item))
                        {
                            item.GotoState(state, false);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Internal event data associated to list changes
        /// </summary>
        private class ClearedChangedArgs : EventArgs
        {
            /// <summary>
            /// Items removed from the list
            /// </summary>
            public IList OldItems { get; private set; }

            /// <summary>
            /// Constructs a NotifyItemsChangedArgs from an action and a list of items
            /// </summary>
            /// <param name="items"></param>
            public ClearedChangedArgs(IList items)
            {
                this.OldItems = items;
            }

        }

        /// <summary>
        /// Collection for holding selected items
        /// It adds CollectionCleared event to the ObservableCollection in order to provide removed items with the event when the collection is cleared
        /// </summary>
        private class SelectedItemsList : ObservableCollection<object>
        {
            /// <summary>
            /// Event indicating that collection has changed
            /// </summary>
            public event EventHandler<ClearedChangedArgs> CollectionCleared;

            /// <summary>
            /// Overrides the base class ClearItems method
            /// </summary>
            protected override void ClearItems()
            {
                // Collection changes only if items were present before...
                if (this.Count > 0)
                {
                    ClearedChangedArgs e = (CollectionCleared != null) ? new ClearedChangedArgs(new List<object>(this)) : null;
                    base.ClearItems();
                    if (CollectionCleared != null)
                    {
                        CollectionCleared(this, e);
                    }
                }
            }
        }
    }
}
