using Microsoft.Phone.Controls.Primitives;
using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Controls;

namespace Microsoft.Phone.Controls
{
    /// <summary>
    /// A group of radio buttons.
    /// </summary>
    public class RadioButtonGroup : TemplatedItemsControl<RadioButton>, ISupportInitialize, ISelector<RadioButton>
    {
        private readonly string _groupName = Guid.NewGuid().ToString();

        /// <summary>
        /// Initializes a new instance of the
        /// <see cref="T:Microsoft.Phone.Controls.RadioButtonGroup" /> class.
        /// </summary>
        public RadioButtonGroup()
        {
            DefaultStyleKey = typeof(RadioButtonGroup);

            SelectorHelper = new SelectorHelper<RadioButton>(this);
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
        /// Identifies the <see cref="P:Microsoft.Phone.Controls.RadioButtonGroup.SelectedIndex"/> dependency property.
        /// </summary>
        /// 
        /// <returns>
        /// The identifier for the <see cref="P:Microsoft.Phone.Controls.RadioButtonGroup.SelectedIndex"/> dependency property.
        /// </returns>
        public static readonly DependencyProperty SelectedIndexProperty = DependencyProperty.Register(
            "SelectedIndex",
            typeof(int),
            typeof(RadioButtonGroup),
            new PropertyMetadata(-1, (d, e) => ((RadioButtonGroup)d).OnSelectedIndexChanged(e)));

        private void OnSelectedIndexChanged(DependencyPropertyChangedEventArgs e)
        {
            SelectorHelper.OnSelectedIndexChanged((int)e.OldValue, (int)e.NewValue);
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
        /// Identifies the <see cref="P:Microsoft.Phone.Controls.RadioButtonGroup.SelectedItem"/> dependency property.
        /// </summary>
        /// 
        /// <returns>
        /// The identifier for the <see cref="P:Microsoft.Phone.Controls.RadioButtonGroup.SelectedItem"/> dependency property.
        /// </returns>
        public static readonly DependencyProperty SelectedItemProperty = DependencyProperty.Register(
            "SelectedItem",
            typeof(object),
            typeof(RadioButtonGroup),
            new PropertyMetadata((d, e) => ((RadioButtonGroup)d).OnSelectedItemChanged(e)));

        private void OnSelectedItemChanged(DependencyPropertyChangedEventArgs e)
        {
            SelectorHelper.OnSelectedItemChanged(e.OldValue, e.NewValue);
        }

        #endregion

        #region Header

        /// <summary>
        /// Gets or sets the content for the header of the control.
        /// </summary>
        /// <value>
        /// The content for the header of the control. The default value is
        /// null.
        /// </value>
        public object Header
        {
            get { return (object)GetValue(HeaderProperty); }
            set { SetValue(HeaderProperty, value); }
        }

        /// <summary>
        /// Identifies the
        /// <see cref="P:Microsoft.Phone.Controls.RadioButtonGroup.Header" />
        /// dependency property.
        /// </summary>
        /// <value>
        /// The identifier for the
        /// <see cref="P:Microsoft.Phone.Controls.RadioButtonGroup.Header" />
        /// dependency property.
        /// </value>
        public static readonly DependencyProperty HeaderProperty = DependencyProperty.Register(
            "Header",
            typeof(object),
            typeof(RadioButtonGroup),
            null);

        #endregion

        #region HeaderTemplate

        /// <summary>
        /// Gets or sets the template that is used to display the content of the
        /// control's header.
        /// </summary>
        /// <value>
        /// The template that is used to display the content of the control's
        /// header. The default is null.
        /// </value>
        public DataTemplate HeaderTemplate
        {
            get { return (DataTemplate)GetValue(HeaderTemplateProperty); }
            set { SetValue(HeaderTemplateProperty, value); }
        }

        /// <summary>
        /// Identifies the
        /// <see cref="P:Microsoft.Phone.Controls.RadioButtonGroup.HeaderTemplate" />
        /// dependency property.
        /// </summary>
        /// <value>
        /// The identifier for the
        /// <see cref="P:Microsoft.Phone.Controls.RadioButtonGroup.HeaderTemplate" />
        /// dependency property.
        /// </value>
        public static readonly DependencyProperty HeaderTemplateProperty = DependencyProperty.Register(
            "HeaderTemplate",
            typeof(DataTemplate),
            typeof(RadioButtonGroup),
            null);

        #endregion

        private SelectorHelper<RadioButton> SelectorHelper { get; set; }

        /// <summary>
        /// Occurs when the currently selected item changes.
        /// </summary>
        public event SelectionChangedEventHandler SelectionChanged;

        /// <summary>
        /// Provides handling for the <see cref="E:System.Windows.Controls.ItemContainerGenerator.ItemsChanged"/> event.
        /// </summary>
        /// <param name="e">A <see cref="T:System.Collections.Specialized.NotifyCollectionChangedEventArgs"/> that contains the event data.</param>
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Justification = "Standard pattern.")]
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

            RadioButton container = element as RadioButton;
            if (container != null)
            {
                container.GroupName = _groupName;
                container.IsChecked = InternalUtils.AreValuesEqual(item, SelectedItem);
                container.Checked += OnItemContainerChecked;
                container.Unchecked += OnItemContainerUnchecked;
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

            RadioButton container = element as RadioButton;
            if (container != null)
            {
                container.Checked -= OnItemContainerChecked;
                container.Unchecked -= OnItemContainerUnchecked;
            }
        }

        private void OnItemContainerChecked(object sender, RoutedEventArgs e)
        {
            OnItemContainerIsCheckedChanged((RadioButton)sender);
        }

        private void OnItemContainerUnchecked(object sender, RoutedEventArgs e)
        {
            OnItemContainerIsCheckedChanged((RadioButton)sender);
        }

        private void OnItemContainerIsCheckedChanged(RadioButton container)
        {
            if (SelectorHelper._selectionChanger.IsActive)
            {
                return;
            }

            int index = ItemContainerGenerator.IndexFromContainer(container);
            if (index >= 0 && index < Items.Count)
            {
                object item = GetItem(container);
                try
                {
                    SelectorHelper._selectionChanger.Begin();

                    if (container.IsChecked == true)
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

        bool ISelector<RadioButton>.CanSelectMultiple
        {
            get { return false; }
        }

        ItemCollection ISelector<RadioButton>.Items
        {
            get { return Items; }
        }

        DataTemplate ISelector<RadioButton>.ItemTemplate
        {
            get { return ItemTemplate; }
        }

        int ISelector<RadioButton>.SelectedIndex
        {
            get { return SelectedIndex; }
            set { SelectedIndex = value; }
        }

        object ISelector<RadioButton>.SelectedItem
        {
            get { return SelectedItem; }
            set { SelectedItem = value; }
        }

        void ISelector<RadioButton>.OnSelectionChanged(SelectionChangedEventArgs e)
        {
            if (SelectionChanged != null)
            {
                SelectionChanged(this, e);
            }
        }

        void ISelector<RadioButton>.OnSelectionChanged(int oldIndex, int newIndex, object oldValue, object newValue)
        {
        }

        void ISelector<RadioButton>.SetItemIsSelected(object item, bool value)
        {
            RadioButton container = item as RadioButton ?? ItemContainerGenerator.ContainerFromItem(item) as RadioButton;
            if (container != null)
            {
                container.IsChecked = value;
            }
        }

        bool ISelector<RadioButton>.GetIsSelected(RadioButton element)
        {
            return element.IsChecked == true;
        }

        RadioButton ISelector<RadioButton>.GetSelectorItem(int index)
        {
            if (index < 0 || Items.Count <= index)
            {
                return null;
            }
            return GetContainer(Items[index]);
        }

        #endregion
    }
}
