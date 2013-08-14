using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Controls;

namespace Microsoft.Phone.Controls.Primitives
{
    /// <summary>
    /// Represents a control that allows a user to select an item from a collection of items.
    /// </summary>
    public abstract class SimpleSelector : ItemsControl, ISupportInitialize, ISelector<SimpleSelectorItem>
    {
        internal SimpleSelector()
        {
            SelectorHelper = new SelectorHelper<SimpleSelectorItem>(this);
        }

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
        /// Identifies the <see cref="P:Microsoft.Phone.Controls.Primitives.SimpleSelector.SelectedIndex"/> dependency property.
        /// </summary>
        /// 
        /// <returns>
        /// The identifier for the <see cref="P:Microsoft.Phone.Controls.Primitives.SimpleSelector.SelectedIndex"/> dependency property.
        /// </returns>
        public static readonly DependencyProperty SelectedIndexProperty = DependencyProperty.Register(
            "SelectedIndex", typeof(int), typeof(SimpleSelector), new PropertyMetadata(-1, OnSelectedIndexChanged));

        private static void OnSelectedIndexChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((SimpleSelector)d).OnSelectedIndexChanged((int)e.OldValue, (int)e.NewValue);
        }

        private void OnSelectedIndexChanged(int oldIndex, int newIndex)
        {
            SelectorHelper.OnSelectedIndexChanged(oldIndex, newIndex);
        }

        /// <summary>
        /// Gets or sets the selected item.
        /// </summary>
        /// 
        /// <returns>
        /// The selected item. The default is null.
        /// </returns>
        public object SelectedItem
        {
            get { return GetValue(SelectedItemProperty); }
            set { SetValue(SelectedItemProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="P:Microsoft.Phone.Controls.Primitives.SimpleSelector.SelectedItem"/> dependency property.
        /// </summary>
        /// 
        /// <returns>
        /// The identifier for the <see cref="P:Microsoft.Phone.Controls.Primitives.SimpleSelector.SelectedItem"/> dependency property.
        /// </returns>
        public static readonly DependencyProperty SelectedItemProperty = DependencyProperty.Register(
            "SelectedItem", typeof(object), typeof(SimpleSelector), new PropertyMetadata(OnSelectedItemChanged));

        private static void OnSelectedItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((SimpleSelector)d).OnSelectedItemChanged(e.OldValue, e.NewValue);
        }

        private void OnSelectedItemChanged(object oldValue, object newValue)
        {
            SelectorHelper.OnSelectedItemChanged(oldValue, newValue);
        }

        internal SelectorHelper<SimpleSelectorItem> SelectorHelper { get; private set; }

        internal virtual bool CanSelectMultiple
        {
            get { return false; }
        }

        /// <summary>
        /// Occurs when the currently selected item changes.
        /// </summary>
        public event SelectionChangedEventHandler SelectionChanged;

        internal virtual void OnSelectionChanged(SelectionChangedEventArgs e)
        {
            if (SelectionChanged != null)
            {
                SelectionChanged(this, e);
            }
        }

        internal virtual void OnSelectionChanged(int oldIndex, int newIndex, object oldValue, object newValue)
        {
        }

        /// <summary>
        /// Provides handling for the <see cref="E:System.Windows.Controls.ItemContainerGenerator.ItemsChanged"/> event.
        /// </summary>
        /// <param name="e">A <see cref="T:System.Collections.Specialized.NotifyCollectionChangedEventArgs"/> that contains the event data.</param>
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Justification = "Standard pattern.")]
        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        protected override void OnItemsChanged(NotifyCollectionChangedEventArgs e)
        {
            base.OnItemsChanged(e);

            SelectorHelper.OnItemsChanged(e);
        }

        /// <summary>
        /// Prepares the specified element to display the specified item.
        /// </summary>
        /// <param name="element">The element used to display the specified item.</param>
        /// <param name="item">The item to display</param>
        protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
        {
            base.PrepareContainerForItemOverride(element, item);

            SimpleSelectorItem selectorItem = (SimpleSelectorItem)element;
            selectorItem._parentSelector = this;
            if (!ReferenceEquals(element, item))
            {
                selectorItem.Item = item;
            }

            int index = ItemContainerGenerator.IndexFromContainer(element);
            if (index != -1)
            {
                selectorItem.IsSelected = SelectorHelper._selectedItems.Contains(item);
            }
        }

        /// <summary>
        /// Removes any bindings and templates applied to the item container for the specified content.
        /// </summary>
        /// <param name="element">The combo box item used to display the specified content.</param>
        /// <param name="item">The item content.</param>
        protected override void ClearContainerForItemOverride(DependencyObject element, object item)
        {
            base.ClearContainerForItemOverride(element, item);

            SimpleSelectorItem selectorItem = (SimpleSelectorItem)element;
            if (!selectorItem.Equals(item))
            {
                selectorItem.ClearValue(ContentControl.ContentProperty);
            }
            selectorItem.Item = null;
        }

        internal SimpleSelectorItem GetSelectorItem(int index)
        {
            if (index < 0 || Items.Count <= index)
            {
                return null;
            }
            else
            {
                return Items[index] as SimpleSelectorItem ?? ItemContainerGenerator.ContainerFromIndex(index) as SimpleSelectorItem;
            }
        }

        internal virtual bool OnSelectorItemClicked(SimpleSelectorItem item)
        {
            item.IsSelected = !item.IsSelected;
            return true;
        }

        internal virtual void NotifySelectorItemSelected(SimpleSelectorItem selectorItem, bool isSelected)
        {
            if (SelectorHelper._selectionChanger.IsActive)
            {
                return;
            }

            int index = ItemContainerGenerator.IndexFromContainer(selectorItem);
            if (index >= 0 && index < Items.Count)
            {
                object item = selectorItem.Item ?? selectorItem;
                try
                {
                    SelectorHelper._selectionChanger.Begin();

                    if (isSelected)
                    {
                        SelectorHelper._selectionChanger.Select(index, item);
                    }
                    else
                    {
                        SelectorHelper._selectionChanger.Unselect(item);
                    }
                }
                finally
                {
                    SelectorHelper._selectionChanger.End();
                }
            }
        }

        #region ISupportInitialize

        void ISupportInitialize.BeginInit()
        {
            SelectorHelper.BeginInit();
        }

        void ISupportInitialize.EndInit()
        {
            SelectorHelper.EndInit();
        }

        #endregion

        #region ISelector

        bool ISelector<SimpleSelectorItem>.CanSelectMultiple
        {
            get { return CanSelectMultiple; }
        }

        ItemCollection ISelector<SimpleSelectorItem>.Items
        {
            get { return Items; }
        }

        DataTemplate ISelector<SimpleSelectorItem>.ItemTemplate
        {
            get { return ItemTemplate; }
        }

        int ISelector<SimpleSelectorItem>.SelectedIndex
        {
            get { return SelectedIndex; }
            set { SelectedIndex = value; }
        }

        object ISelector<SimpleSelectorItem>.SelectedItem
        {
            get { return SelectedItem; }
            set { SelectedItem = value; }
        }

        void ISelector<SimpleSelectorItem>.OnSelectionChanged(SelectionChangedEventArgs e)
        {
            OnSelectionChanged(e);
        }

        void ISelector<SimpleSelectorItem>.OnSelectionChanged(int oldIndex, int newIndex, object oldValue, object newValue)
        {
            OnSelectionChanged(oldIndex, newIndex, oldValue, newValue);
        }

        void ISelector<SimpleSelectorItem>.SetItemIsSelected(object item, bool value)
        {
            SimpleSelectorItem selectorItem = item as SimpleSelectorItem ?? ItemContainerGenerator.ContainerFromItem(item) as SimpleSelectorItem;
            if (selectorItem != null)
            {
                selectorItem.IsSelected = value;
            }
        }

        bool ISelector<SimpleSelectorItem>.GetIsSelected(SimpleSelectorItem element)
        {
            return element.IsSelected;
        }

        SimpleSelectorItem ISelector<SimpleSelectorItem>.GetSelectorItem(int index)
        {
            return GetSelectorItem(index);
        }

        #endregion
    }
}