using System;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Microsoft.Phone.Controls.Primitives
{
    /// <summary>
    /// Implements a custom ListBox for the PickerBox control.
    /// </summary>
    public class PickerBoxList : ListBox
    {
        /// <summary>
        /// Gets or sets the default DataTemplate used to display each item in single select mode.
        /// </summary>
        /// 
        /// <returns>
        /// The template that specifies the visualization of the data objects. The default is null.
        /// </returns>
        public DataTemplate DefaultSingleSelectItemTemplate
        {
            get { return (DataTemplate)GetValue(DefaultSingleSelectItemTemplateProperty); }
            set { SetValue(DefaultSingleSelectItemTemplateProperty, value); }
        }

        /// <summary>
        /// Identifies the DefaultSingleSelectItemTemplate dependency property.
        /// </summary>
        /// 
        /// <returns>
        /// The identifier for the DefaultSingleSelectItemTemplate dependency property.
        /// </returns>
        public static readonly DependencyProperty DefaultSingleSelectItemTemplateProperty = DependencyProperty.Register(
            "DefaultSingleSelectItemTemplate",
            typeof(DataTemplate),
            typeof(PickerBoxList),
            null);

        /// <summary>
        /// Gets or sets the default DataTemplate used to display each item in multi select mode.
        /// </summary>
        /// 
        /// <returns>
        /// The template that specifies the visualization of the data objects. The default is null.
        /// </returns>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Multi")]
        public DataTemplate DefaultMultiSelectItemTemplate
        {
            get { return (DataTemplate)GetValue(DefaultMultiSelectItemTemplateProperty); }
            set { SetValue(DefaultMultiSelectItemTemplateProperty, value); }
        }

        /// <summary>
        /// Identifies the DefaultMultiSelectItemTemplate dependency property.
        /// </summary>
        /// 
        /// <returns>
        /// The identifier for the DefaultMultiSelectItemTemplate dependency property.
        /// </returns>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Multi")]
        public static readonly DependencyProperty DefaultMultiSelectItemTemplateProperty = DependencyProperty.Register(
            "DefaultMultiSelectItemTemplate",
            typeof(DataTemplate),
            typeof(PickerBoxList),
            null);

        /// <summary>
        /// Gets or sets the default style that is used when rendering the item containers in single select mode.
        /// </summary>
        /// 
        /// <returns>
        /// The style applied to the item containers. The default is null.
        /// </returns>
        public Style DefaultSingleSelectItemContainerStyle
        {
            get { return (Style)GetValue(DefaultSingleSelectItemContainerStyleProperty); }
            set { SetValue(DefaultSingleSelectItemContainerStyleProperty, value); }
        }

        /// <summary>
        /// Identifies the DefaultSingleSelectItemContainerStyle dependency property.
        /// </summary>
        /// 
        /// <returns>
        /// The identifier for the DefaultSingleSelectItemContainerStyle dependency property.
        /// </returns>
        public static readonly DependencyProperty DefaultSingleSelectItemContainerStyleProperty = DependencyProperty.Register(
            "DefaultSingleSelectItemContainerStyle",
            typeof(Style),
            typeof(PickerBoxList),
            null);

        /// <summary>
        /// Gets or sets the default style that is used when rendering the item containers in multi select mode.
        /// </summary>
        /// 
        /// <returns>
        /// The style applied to the item containers. The default is null.
        /// </returns>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Multi")]
        public Style DefaultMultiSelectItemContainerStyle
        {
            get { return (Style)GetValue(DefaultMultiSelectItemContainerStyleProperty); }
            set { SetValue(DefaultMultiSelectItemContainerStyleProperty, value); }
        }

        /// <summary>
        /// Identifies the DefaultMultiSelectItemContainerStyle dependency property.
        /// </summary>
        /// 
        /// <returns>
        /// The identifier for the DefaultMultiSelectItemContainerStyle dependency property.
        /// </returns>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Multi")]
        public static readonly DependencyProperty DefaultMultiSelectItemContainerStyleProperty = DependencyProperty.Register(
            "DefaultMultiSelectItemContainerStyle",
            typeof(Style),
            typeof(PickerBoxList),
            null);

        /// <summary>
        /// Occurs when an item receives an interaction.
        /// </summary>
        public event EventHandler ItemClick;

        /// <summary>
        /// Prepares the specified element to display the specified item.
        /// </summary>
        /// <param name="element">The container element used to display the specified item.</param>
        /// <param name="item">The content to display.</param>
        protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
        {
            base.PrepareContainerForItemOverride(element, item);

            PickerBoxListItem container = (PickerBoxListItem)element;

            if (ItemContainerStyle == null)
            {
                Style defaultItemContainerStyle = null;

                if (SelectionMode == SelectionMode.Single)
                {
                    defaultItemContainerStyle = DefaultSingleSelectItemContainerStyle;
                }
                else
                {
                    defaultItemContainerStyle = DefaultMultiSelectItemContainerStyle;
                }

                if (defaultItemContainerStyle != null)
                {
                    container.Style = defaultItemContainerStyle;
                }
            }

            if (ItemTemplate == null)
            {
                DataTemplate defaultItemTemplate = null;

                if (SelectionMode == SelectionMode.Single)
                {
                    defaultItemTemplate = DefaultSingleSelectItemTemplate;
                }
                else
                {
                    defaultItemTemplate = DefaultMultiSelectItemTemplate;
                }

                if (defaultItemTemplate != null)
                {
                    if (DisplayMemberPath != null)
                    {
                        container.SetBinding(ListBoxItem.ContentProperty, new Binding(DisplayMemberPath));
                    }

                    container.ContentTemplate = defaultItemTemplate;
                }
            }

            container.Click += OnItemClick;
        }

        /// <summary>
        /// Removes any bindings and templates applied to the item container for the specified content.
        /// </summary>
        /// <param name="element">The container element used to display the specified item.</param>
        /// <param name="item">The content to display.</param>
        protected override void ClearContainerForItemOverride(DependencyObject element, object item)
        {
            base.ClearContainerForItemOverride(element, item);

            PickerBoxListItem container = (PickerBoxListItem)element;

            container.Click -= OnItemClick;
        }

        /// <summary>
        /// Determines if the specified item is (or is eligible to be) its own item container.
        /// </summary>
        /// <param name="item">The specified item.</param>
        /// <returns>true if the item is its own item container; otherwise, false.</returns>
        protected override bool IsItemItsOwnContainerOverride(object item)
        {
            return item is PickerBoxListItem;
        }

        /// <summary>Creates or identifies the element used to display a specified item.</summary>
        /// <returns>
        /// A <see cref="T:Microsoft.Phone.Controls.Primitives.PickerBoxList" /> corresponding to a specified item.
        /// </returns>
        protected override DependencyObject GetContainerForItemOverride()
        {
            PickerBoxListItem container = new PickerBoxListItem();
            if (ItemContainerStyle != null)
            {
                container.Style = ItemContainerStyle;
            }
            return container;
        }

        private void OnItemClick(object sender, EventArgs e)
        {
            PickerBoxListItem item = (PickerBoxListItem)sender;

            if (SelectionMode == SelectionMode.Single)
            {
                if (!item.IsSelected)
                {
                    item.IsSelected = true;
                }
            }
            else
            {
                item.IsSelected = !item.IsSelected;
            }

            SafeRaise.Raise(ItemClick, this);
        }
    }
}
