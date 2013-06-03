using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Microsoft.Phone.Controls.Primitives
{
    /// <summary>
    /// Implements a custom ListBox for the PickerBox control.
    /// </summary>
    public sealed class PickerBoxListBox : ListBox
    {
        /// <summary>
        /// Gets or sets the default DataTemplate used to display each item in single selection mode.
        /// </summary>
        /// 
        /// <returns>
        /// The template that specifies the visualization of the data objects. The default is null.
        /// </returns>
        public DataTemplate DefaultSingleSelectionItemTemplate
        {
            get { return (DataTemplate)GetValue(DefaultSingleSelectionItemTemplateProperty); }
            set { SetValue(DefaultSingleSelectionItemTemplateProperty, value); }
        }

        /// <summary>
        /// Identifies the DefaultSingleSelectionItemTemplate dependency property.
        /// </summary>
        /// 
        /// <returns>
        /// The identifier for the DefaultSingleSelectionItemTemplate dependency property.
        /// </returns>
        public static readonly DependencyProperty DefaultSingleSelectionItemTemplateProperty = DependencyProperty.Register(
            "DefaultSingleSelectionItemTemplate",
            typeof(DataTemplate),
            typeof(PickerBoxListBox),
            null);

        /// <summary>
        /// Gets or sets the default DataTemplate used to display each item in multi selection mode.
        /// </summary>
        /// 
        /// <returns>
        /// The template that specifies the visualization of the data objects. The default is null.
        /// </returns>
        public DataTemplate DefaultMultiSelectionItemTemplate
        {
            get { return (DataTemplate)GetValue(DefaultMultiSelectionItemTemplateProperty); }
            set { SetValue(DefaultMultiSelectionItemTemplateProperty, value); }
        }

        /// <summary>
        /// Identifies the DefaultMultiSelectionItemTemplate dependency property.
        /// </summary>
        /// 
        /// <returns>
        /// The identifier for the DefaultMultiSelectionItemTemplate dependency property.
        /// </returns>
        public static readonly DependencyProperty DefaultMultiSelectionItemTemplateProperty = DependencyProperty.Register(
            "DefaultMultiSelectionItemTemplate",
            typeof(DataTemplate),
            typeof(PickerBoxListBox),
            null);

        /// <summary>
        /// Prepares the specified element to display the specified item.
        /// </summary>
        /// <param name="element">
        /// The container element used to display the specified item.
        /// </param>
        /// <param name="item">The content to display.</param>
        protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
        {
            base.PrepareContainerForItemOverride(element, item);

            if (ItemTemplate == null)
            {
                DataTemplate defaultItemTemplate = null;

                if (SelectionMode == SelectionMode.Single)
                {
                    defaultItemTemplate = DefaultSingleSelectionItemTemplate;
                }
                else
                {
                    defaultItemTemplate = DefaultMultiSelectionItemTemplate;
                }

                if (defaultItemTemplate != null)
                {
                    ListBoxItem container = (ListBoxItem)element;

                    if (DisplayMemberPath != null)
                    {
                        container.SetBinding(ListBoxItem.ContentProperty, new Binding(DisplayMemberPath));
                    }

                    container.ContentTemplate = defaultItemTemplate;
                }
            }
        }
    }
}
