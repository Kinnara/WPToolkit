using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Microsoft.Phone.Controls
{
    /// <summary>
    /// Represents the container for an item in a <see cref="T:Microsoft.Phone.Controls.ListView"/> control.
    /// </summary>
    [TemplateVisualState(GroupName = VisualStates.GroupCommon, Name = VisualStates.StateNormal)]
    [TemplateVisualState(GroupName = VisualStates.GroupCommon, Name = VisualStates.StateDisabled)]
    [TemplateVisualState(GroupName = VisualStates.GroupSelection, Name = VisualStates.StateUnselected)]
    [TemplateVisualState(GroupName = VisualStates.GroupSelection, Name = VisualStates.StateSelected)]
    public class ListViewItem : ContentControl
    {
        /// <summary>
        /// Initializes a new instance of the ListViewItem class.
        /// </summary>
        public ListViewItem()
        {
            DefaultStyleKey = typeof(ListViewItem);

            IsEnabledChanged += OnIsEnabledChanged;
        }

        #region IsSelected

        /// <summary>
        /// Gets or sets a value that indicates whether a <see cref="T:Microsoft.Phone.Controls.ListViewItem"/> is selected.
        /// </summary>
        /// 
        /// <returns>
        /// true if the item is selected; otherwise, false. The default is false.
        /// </returns>
        internal bool IsSelected
        {
            get { return (bool)GetValue(IsSelectedProperty); }
            set { SetValue(IsSelectedProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="P:Microsoft.Phone.Controls.ListViewItem.IsSelected"/> dependency property.
        /// </summary>
        /// 
        /// <returns>
        /// The identifier for the <see cref="P:Microsoft.Phone.Controls.ListViewItem.IsSelected"/> dependency property.
        /// </returns>
        private static readonly DependencyProperty IsSelectedProperty = DependencyProperty.Register(
            "IsSelected",
            typeof(bool),
            typeof(ListViewItem),
            new PropertyMetadata(false, (d, e) => ((ListViewItem)d).OnIsSelectedChanged()));

        private void OnIsSelectedChanged()
        {
            ChangeVisualState();
        }

        #endregion

        internal object Item { get; set; }

        internal bool IsStyleSetFromListView { get; set; }

        /// <summary>
        /// Builds the visual tree for the <see cref="T:Microsoft.Phone.Controls.ListViewItem"/> control when a new template is applied.
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            ChangeVisualState(false);
        }

        /// <summary>
        /// Finds the ListView to which the ListViewItem belongs
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        protected static ListView FindContainer(DependencyObject item)
        {
            while (item != null)
            {
                item = System.Windows.Media.VisualTreeHelper.GetParent(item);
                ListView listView = item as ListView;
                if (listView != null)
                {
                    return listView;
                }
            }
            return null;
        }

        /// <summary>
        /// Called when content is changed. This is a good place to get the style (which depends on the ListView layout)
        /// because the control template has not yet been expanded
        /// </summary>
        /// <param name="oldContent"></param>
        /// <param name="newContent"></param>
        protected override void OnContentChanged(object oldContent, object newContent)
        {
            ListView listView = FindContainer(this);
            if (listView != null)
            {
                listView.ConfigureItem(this);
            }
            base.OnContentChanged(oldContent, newContent);
        }

        private void ChangeVisualState()
        {
            ChangeVisualState(true);
        }

        private void ChangeVisualState(bool useTransitions)
        {
            GoToState(useTransitions, IsEnabled ? VisualStates.StateNormal : VisualStates.StateDisabled);
            GoToState(useTransitions, IsSelected ? VisualStates.StateSelected : VisualStates.StateUnselected);
        }

        private bool GoToState(bool useTransitions, string stateName)
        {
            return VisualStateManager.GoToState(this, stateName, useTransitions);
        }

        private void OnIsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            ChangeVisualState();
        }
    }
}
