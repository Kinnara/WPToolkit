using Microsoft.Phone.Controls.LocalizedResources;
using Microsoft.Phone.Controls.Primitives;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Microsoft.Phone.Controls
{
    /// <summary>
    /// Represents a control that allows a user to pick one or more items from a list.
    /// </summary>
    public sealed class PickerBoxFlyout : PickerFlyoutBase
    {
        private PickerBoxFlyoutPresenter _presenter;
        private PickerBoxList _picker;
        private PickerFlyoutHelper<IList> _helper;

        private List<object> _selectedItemsWhenOpened;

        /// <summary>
        /// Initializes a new instance of the PickerBoxFlyout class.
        /// </summary>
        public PickerBoxFlyout()
        {
            _presenter = new PickerBoxFlyoutPresenter(this);
            _presenter.ItemPicked += OnItemPicked;
            _picker = _presenter.Picker;
            _helper = new PickerFlyoutHelper<IList>(this);

            Transition = new FlyoutTransition
            {
                In = new ReaderboardTransition { Mode = ReaderboardTransitionMode.In, BeginTime = TimeSpan.FromMilliseconds(50) },
                Out = new ReaderboardTransition { Mode = ReaderboardTransitionMode.Out },
            };
        }

        #region public IEnumerable ItemsSource

        /// <summary>
        /// Gets or sets a collection used to generate the content of the control.
        /// </summary>
        /// 
        /// <returns>
        /// The object that is used to generate the content of the control. The default is null.
        /// </returns>
        public IEnumerable ItemsSource
        {
            get { return (IEnumerable)GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }

        /// <summary>
        /// Identifies the ItemsSource dependency property.
        /// </summary>
        /// 
        /// <returns>
        /// The identifier for the ItemsSource dependency property.
        /// </returns>
        public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register(
            "ItemsSource",
            typeof(IEnumerable),
            typeof(PickerBoxFlyout),
            new PropertyMetadata((d, e) => ((PickerBoxFlyout)d).OnItemsSourceChanged()));

        private void OnItemsSourceChanged()
        {
            _picker.ItemsSource = ItemsSource;
        }

        #endregion

        #region public DataTemplate ItemTemplate

        /// <summary>
        /// Gets or sets the DataTemplate used to display each item.
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
        /// Identifies the ItemTemplate dependency property.
        /// </summary>
        /// 
        /// <returns>
        /// The identifier for the ItemTemplate dependency property.
        /// </returns>
        public static readonly DependencyProperty ItemTemplateProperty = DependencyProperty.Register(
            "ItemTemplate",
            typeof(DataTemplate),
            typeof(PickerBoxFlyout),
            new PropertyMetadata((d, e) => ((PickerBoxFlyout)d).OnItemTemplateChanged()));

        private void OnItemTemplateChanged()
        {
            _picker.ItemTemplate = ItemTemplate;
        }

        #endregion

        #region public string DisplayMemberPath

        /// <summary>
        /// Gets or sets the name or path of the property that is displayed for each data item.
        /// </summary>
        /// 
        /// <returns>
        /// The name or path of the property that is displayed for each the data item in the control. The default is null.
        /// </returns>
        public string DisplayMemberPath
        {
            get { return (string)GetValue(DisplayMemberPathProperty); }
            set { SetValue(DisplayMemberPathProperty, value); }
        }

        /// <summary>
        /// Identifies the DisplayMemberPath dependency property.
        /// </summary>
        /// 
        /// <returns>
        /// The identifier for the DisplayMemberPath dependency property.
        /// </returns>
        public static readonly DependencyProperty DisplayMemberPathProperty = DependencyProperty.Register(
            "DisplayMemberPath",
            typeof(string),
            typeof(PickerBoxFlyout),
            new PropertyMetadata((d, e) => ((PickerBoxFlyout)d).OnDisplayMemberPathChanged()));

        private void OnDisplayMemberPathChanged()
        {
            _picker.DisplayMemberPath = DisplayMemberPath;
        }

        #endregion

        #region public int SelectedIndex

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
        /// Identifies the SelectedIndex dependency property.
        /// </summary>
        /// 
        /// <returns>
        /// The identifier for the SelectedIndex dependency property.
        /// </returns>
        public static readonly DependencyProperty SelectedIndexProperty = DependencyProperty.Register(
            "SelectedIndex",
            typeof(int),
            typeof(PickerBoxFlyout),
            new PropertyMetadata(-1));

        #endregion

        #region public object SelectedItem

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
        /// Identifies the SelectedItem dependency property.
        /// </summary>
        /// 
        /// <returns>
        /// The identifier for the SelectedItem dependency property.
        /// </returns>
        public static readonly DependencyProperty SelectedItemProperty = DependencyProperty.Register(
            "SelectedItem",
            typeof(object),
            typeof(PickerBoxFlyout),
            null);

        #endregion

        /// <summary>
        /// Gets the list of currently selected items.
        /// </summary>
        /// 
        /// <returns>
        /// The list of currently selected items.
        /// </returns>
        public IList SelectedItems
        {
            get { return _picker.SelectedItems; }
        }

        #region public SelectionMode SelectionMode

        /// <summary>
        /// Gets or sets the selection behavior for the control.
        /// </summary>
        /// 
        /// <returns>
        /// One of the SelectionMode values.
        /// </returns>
        public SelectionMode SelectionMode
        {
            get { return (SelectionMode)GetValue(SelectionModeProperty); }
            set { SetValue(SelectionModeProperty, value); }
        }

        /// <summary>
        /// Identifies the SelectionMode dependency property.
        /// </summary>
        /// 
        /// <returns>
        /// The identifier for the SelectionMode dependency property.
        /// </returns>
        public static readonly DependencyProperty SelectionModeProperty = DependencyProperty.Register(
            "SelectionMode",
            typeof(SelectionMode),
            typeof(PickerBoxFlyout),
            new PropertyMetadata((d, e) => ((PickerBoxFlyout)d).OnSelectionModeChanged()));

        private void OnSelectionModeChanged()
        {
            _picker.SelectionMode = SelectionMode;
        }

        #endregion

        /// <summary>
        /// Occurs when the user has selected items.
        /// </summary>
        public event SelectionChangedEventHandler ItemsPicked;

        /// <summary>
        /// Begins an asynchronous operation to show the flyout placed in relation to the specified element.
        /// </summary>
        /// <returns>
        /// An asynchronous operation.
        /// </returns>
        /// <param name="target">The element to use as the flyout's placement target.</param>
        public Task<IList> ShowAtAsync(FrameworkElement target)
        {
            return _helper.ShowAsync();
        }

        /// <summary>
        /// Initializes a control to show the flyout content.
        /// </summary>
        /// <returns>
        /// The control that displays the content of the flyout.
        /// </returns>
        protected override Control CreatePresenter()
        {
            return _presenter;
        }

        protected override void OnConfirmed()
        {
            RaiseItemsPicked();

            base.OnConfirmed();
        }

        protected override bool ShouldShowConfirmationButtons()
        {
            return SelectionMode != SelectionMode.Single;
        }

        internal override void OnOpening()
        {
            if (string.IsNullOrEmpty(GetTitle(this)))
            {
                SetTitle(this, SelectionMode == SelectionMode.Single ? ControlResources.ChooseAnItem : ControlResources.ChooseItems);
            }

            base.OnOpening();
        }

        internal override void OnOpened()
        {
            base.OnOpened();

            _selectedItemsWhenOpened = SelectedItems.Cast<object>().ToList();
        }

        internal override void OnClosed()
        {
            if (_selectedItemsWhenOpened != null)
            {
                if (SelectionMode != SelectionMode.Single)
                {
                    SelectedItems.Clear();

                    foreach (object item in _selectedItemsWhenOpened)
                    {
                        SelectedItems.Add(item);
                    }
                }

                _selectedItemsWhenOpened = null;
            }

            _helper.CompleteShowAsync(null);

            base.OnClosed();
        }

        internal override void RequestHide()
        {
            if (_helper.IsAsyncOperationInProgress)
            {
                return;
            }

            base.RequestHide();
        }

        private void OnItemPicked(object sender, EventArgs e)
        {
            OnConfirmed();
        }

        private void RaiseItemsPicked()
        {
            _helper.CompleteShowAsync(SelectedItems);

            var handler = ItemsPicked;
            if (handler != null)
            {
                var oldSelectedItems = _selectedItemsWhenOpened;
                var newSelectedItems = SelectedItems;

                var removedItems = new List<object>();
                var addedItems = new List<object>();

                foreach (object item in oldSelectedItems)
                {
                    if (!newSelectedItems.Contains(item))
                    {
                        removedItems.Add(item);
                    }
                }

                foreach (object item in newSelectedItems)
                {
                    if (!oldSelectedItems.Contains(item))
                    {
                        addedItems.Add(item);
                    }
                }

                handler(this, new SelectionChangedEventArgs(removedItems, addedItems));
            }

            _selectedItemsWhenOpened = null;
        }
    }
}
