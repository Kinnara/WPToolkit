using System.Windows;

namespace Microsoft.Phone.Controls
{
    /// <summary>
    /// Represents a selectable item inside an ApplicationBarSubmenu.
    /// </summary>
    [TemplateVisualState(GroupName = VisualStates.GroupSelection, Name = VisualStates.StateUnselected)]
    [TemplateVisualState(GroupName = VisualStates.GroupSelection, Name = VisualStates.StateSelected)]
    public class ApplicationBarSubmenuItem : MenuItem
    {
        /// <summary>
        /// Initializes a new instance of the ApplicationBarSubmenuItem class.
        /// </summary>
        public ApplicationBarSubmenuItem()
        {
            DefaultStyleKey = typeof(ApplicationBarSubmenuItem);

            Unloaded += OnUnloaded;
        }

        /// <summary>
        /// Initializes a new instance of the ApplicationBarSubmenuItem class.
        /// </summary>
        public ApplicationBarSubmenuItem(string header)
        {
            DefaultStyleKey = typeof(ApplicationBarSubmenuItem);

            Unloaded += OnUnloaded;
            Header = header;
        }

        #region IsSelected

        private bool IsSelected
        {
            get { return (bool)GetValue(IsSelectedProperty); }
            set { SetValue(IsSelectedProperty, value); }
        }

        private static readonly DependencyProperty IsSelectedProperty = DependencyProperty.Register(
            "IsSelected",
            typeof(bool),
            typeof(ApplicationBarSubmenuItem),
            new PropertyMetadata((d, e) => ((ApplicationBarSubmenuItem)d).OnIsSelectedChanged()));

        private void OnIsSelectedChanged()
        {
            ChangeVisualState(true);
        }

        #endregion

        /// <summary>
        /// Called when a MenuItem is clicked and raises a Click event.
        /// </summary>
        protected override void OnClick()
        {
            IsSelected = true;

            ApplicationBarSubmenu submenu = ParentMenuBase as ApplicationBarSubmenu;
            if (submenu != null)
            {
                submenu.ChildMenuItemClicked();
            }

            base.OnClick();
        }

        /// <summary>
        /// Changes to the correct visual state(s) for the control.
        /// </summary>
        /// <param name="useTransitions">True to use transitions; otherwise false.</param>
        protected override void ChangeVisualState(bool useTransitions)
        {
            base.ChangeVisualState(useTransitions);

            VisualStateManager.GoToState(this, IsSelected ? VisualStates.StateSelected : VisualStates.StateUnselected, useTransitions);
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            ClearValue(IsSelectedProperty);
        }
    }
}
