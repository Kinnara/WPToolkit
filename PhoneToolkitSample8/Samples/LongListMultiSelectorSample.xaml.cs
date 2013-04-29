// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using PhoneToolkitSample.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace PhoneToolkitSample.Samples
{
    /// <summary>
    /// Sample code for LongListMultiSelector
    /// </summary>
    public partial class LongListMultiSelectorSample : PhoneApplicationPage
    {
        class PivotCallbacks
        {
            public Action Init {get;set;}
            public Action OnActivated{get;set;}
            public Action<CancelEventArgs> OnBackKeyPress {get;set;}
        }

        Dictionary<object, PivotCallbacks> _callbacks;

        /// <summary>
        /// Initializes the dictionary of delegates to call when each pivot is selected
        /// </summary>
        public LongListMultiSelectorSample()
        {
            InitializeComponent();

            this.Loaded += LongListMultiSelectorSample_Loaded;

            
            _callbacks = new Dictionary<object,PivotCallbacks>();
            _callbacks[MultiselectLbxItem] = new PivotCallbacks {
                Init = CreateEmailApplicationBarItems,
                OnActivated = OnEmailPivotItemActivated,
                OnBackKeyPress = OnEmailBackKeyPressed
            };
            _callbacks[BuddiesPivotItem] = new PivotCallbacks
            {
                OnActivated = SetupBuddiesApplicationBar,
                OnBackKeyPress = OnBuddiesBackKeyPressed,
            };
            _callbacks[GridModeItem] = new PivotCallbacks
            {
                Init = CreatePicturesApplicationBarItems,
                OnActivated = OnPicturesPivotItemActivated,
                OnBackKeyPress = OnGridBackKeyPressed
            };
            _callbacks[DataboundPivotItem] = new PivotCallbacks
            {
                OnActivated = SetupBoundBuddiesApplicationBar
            };
            foreach (var callbacks in _callbacks.Values)
            {
                if (callbacks.Init != null)
                {
                    callbacks.Init();
                }
            }
        }

        void LongListMultiSelectorSample_Loaded(object sender, RoutedEventArgs e)
        {
            if (this.NavigationContext.QueryString.ContainsKey("multiselect"))
            {
                MessageBox.Show(
    @"The MultiSelectList has been deprecated in Windows Phone 8 in favor of LongListMultiSelector which is built on top of the more performant LongListSelector.
This sample and the sample code demonstrates how to use the new LongListMultiSelector control.");
            }
        }

        /// <summary>
        /// Setup the application bar buttons according to the active Pivot Item
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnPivotSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            PivotCallbacks callbacks;
            if (_callbacks.TryGetValue(SamplePivot.SelectedItem, out callbacks) && (callbacks.OnActivated != null))
            {
                callbacks.OnActivated();
            }
        }

        /// <summary>
        /// Defers back treatment to active pivot function
        /// </summary>
        /// <param name="e"></param>
        protected override void OnBackKeyPress(CancelEventArgs e)
        {
            base.OnBackKeyPress(e);
            if (CurrentPicture != null)
            {
                CurrentPicture = null;
                e.Cancel = true;
            }
            else
            {
                PivotCallbacks callbacks;
                if (_callbacks.TryGetValue(SamplePivot.SelectedItem, out callbacks) && (callbacks.OnBackKeyPress != null))
                {
                    callbacks.OnBackKeyPress(e);
                }
            }
        }

        /// <summary>
        /// Resets the application bar
        /// </summary>
        void ClearApplicationBar()
        {
            while (ApplicationBar.Buttons.Count > 0)
            {
                ApplicationBar.Buttons.RemoveAt(0);
            }

            while (ApplicationBar.MenuItems.Count > 0)
            {
                ApplicationBar.MenuItems.RemoveAt(0);
            }
        }

        #region MultiselectListbox item
        ApplicationBarIconButton select;
        ApplicationBarIconButton delete;
        ApplicationBarMenuItem markAsRead;
        ApplicationBarMenuItem markAsUnread;

        /// <summary>
        /// Creates ApplicationBar items for email list
        /// </summary>
        private void CreateEmailApplicationBarItems()
        {
            select = new ApplicationBarIconButton();
            select.IconUri = new Uri("/Toolkit.Content/ApplicationBar.Select.png", UriKind.RelativeOrAbsolute);
            select.Text = "select";
            select.Click += OnSelectClick;

            delete = new ApplicationBarIconButton();
            delete.IconUri = new Uri("/Toolkit.Content/ApplicationBar.Delete.png", UriKind.RelativeOrAbsolute);
            delete.Text = "delete";
            delete.Click += OnDeleteClick;

            markAsRead = new ApplicationBarMenuItem();
            markAsRead.Text = "mark as read";
            markAsRead.Click += OnMarkAsReadClick;

            markAsUnread = new ApplicationBarMenuItem();
            markAsUnread.Text = "mark as unread";
            markAsUnread.Click += OnMarkAsUnreadClick;
        }

        /// <summary>
        /// Called when Email pivot item is activated : makes sure that selection is disabled and updates the application bar
        /// </summary>
        void OnEmailPivotItemActivated()
        {
            if (EmailList.IsSelectionEnabled)
            {
                EmailList.IsSelectionEnabled = false; // Will update the application bar too
            }
            else
            {
                SetupEmailApplicationBar();
            }
        }

        /// <summary>
        /// Configure ApplicationBar items for email list
        /// </summary>
        private void SetupEmailApplicationBar()
        {
            ClearApplicationBar();

            if (EmailList.IsSelectionEnabled)
            {
                ApplicationBar.Buttons.Add(delete);
                ApplicationBar.MenuItems.Add(markAsRead);
                ApplicationBar.MenuItems.Add(markAsUnread);
                UpdateEmailApplicationBar();
            }
            else
            {
                ApplicationBar.Buttons.Add(select);
            }
            ApplicationBar.IsVisible = true;
        }

        /// <summary>
        /// Updates the email Application bar items depending on selection
        /// </summary>
        private void UpdateEmailApplicationBar()
        {
            if (EmailList.IsSelectionEnabled)
            {
                bool hasSelection = ((EmailList.SelectedItems != null) && (EmailList.SelectedItems.Count > 0));
                delete.IsEnabled = hasSelection;
                markAsRead.IsEnabled = hasSelection;
                markAsUnread.IsEnabled = hasSelection;
            }
        }

        /// <summary>
        /// Back Key Pressed = leaves the selection mode
        /// </summary>
        /// <param name="e"></param>
        protected void OnEmailBackKeyPressed(CancelEventArgs e)
        {
            if (EmailList.IsSelectionEnabled)
            {
                EmailList.IsSelectionEnabled = false;
                e.Cancel = true;
            }
        }

        /// <summary>
        /// Passes the email list in selection mode
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void OnSelectClick(object sender, EventArgs e)
        {
            EmailList.IsSelectionEnabled = true;
        }

        /// <summary>
        /// Deletes selected items
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void OnDeleteClick(object sender, EventArgs e)
        {
            IList source = EmailList.ItemsSource as IList;
            while (EmailList.SelectedItems.Count > 0)
            {
                source.Remove((EmailObject)EmailList.SelectedItems[0]);
            }
        }

        /// <summary>
        /// Mark all items as read
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void OnMarkAsReadClick(object sender, EventArgs e)
        {
            foreach (EmailObject obj in EmailList.SelectedItems)
            {
                obj.Unread = false;
            }

            EmailList.IsSelectionEnabled = false;
        }

        /// <summary>
        /// Mark all items as unread
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void OnMarkAsUnreadClick(object sender, EventArgs e)
        {
            foreach (EmailObject obj in EmailList.SelectedItems)
            {
                obj.Unread = true;
            }

            EmailList.IsSelectionEnabled = false;
        }

        /// <summary>
        /// Adjusts the user interface according to the number of selected emails
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnEmailListSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateEmailApplicationBar();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnEmailListIsSelectionEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            SetupEmailApplicationBar();
        }



        /// <summary>
        /// Tap on an item : depending on the selection state, either unselect it or consider it as read
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnItemContentTap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            EmailObject item = ((FrameworkElement)sender).DataContext as EmailObject;
            if (item != null)
            {
                item.Unread = false;
            }
        }

        #endregion

        #region Buddies
        /// <summary>
        /// Setup the application bar for the Buddies Pivot items : simply hide it in this sample
        /// </summary>
        void SetupBuddiesApplicationBar()
        {
            ApplicationBar.IsVisible = false;
            buddies.IsSelectionEnabled = false;
        }


        /// <summary>
        /// Back Key Pressed = leaves the selection mode
        /// </summary>
        /// <param name="e"></param>
        protected void OnBuddiesBackKeyPressed(CancelEventArgs e)
        {
            if (buddies.IsSelectionEnabled)
            {
                buddies.IsSelectionEnabled = false;
                e.Cancel = true;
            }
        }
        #endregion

        #region Grid Mode
        ApplicationBarIconButton picturesEnableSelect;
        ApplicationBarIconButton picturesUpload;
        /// <summary>
        ///  One time initialization for grid Pivot
        /// </summary>
        private void CreatePicturesApplicationBarItems()
        {
            picturesEnableSelect = new ApplicationBarIconButton();
            picturesEnableSelect.IconUri = new Uri("/Toolkit.Content/ApplicationBar.Select.png", UriKind.RelativeOrAbsolute);
            picturesEnableSelect.Text = "select";
            picturesEnableSelect.Click += OnPicturesEnableSelectClick;

            picturesUpload = new ApplicationBarIconButton();
            picturesUpload.IconUri = new Uri("/Toolkit.Content/ApplicationBar.Upload.png", UriKind.RelativeOrAbsolute);
            picturesUpload.Text = "upload";
            picturesUpload.Click += OnPicturesUploadClick;
        }

        /// <summary>
        /// Called when Picture pivot item is activated : makes sure that selection is disabled and updates the application bar
        /// </summary>
        private void OnPicturesPivotItemActivated()
        {
            if (GridSelector.IsSelectionEnabled)
            {
                GridSelector.IsSelectionEnabled = false; // Will also update the Application Bar
            }
            else
            {
                SetupPicturesApplicationBar();
            }
        }

        /// <summary>
        /// Setups the application bar for the Pivot
        /// </summary>
        private void SetupPicturesApplicationBar()
        {
            ClearApplicationBar();
            if (GridSelector.IsSelectionEnabled)
            {
                ApplicationBar.Buttons.Add(picturesUpload);
                UpdatePicturesApplicationBar();
            }
            else
            {
                ApplicationBar.Buttons.Add(picturesEnableSelect);
            }
            ApplicationBar.IsVisible = true;
        }

        /// <summary>
        /// Enables / disables the Upload button according to selection
        /// </summary>
        void UpdatePicturesApplicationBar()
        {
            picturesUpload.IsEnabled = ((GridSelector.SelectedItems != null) && (GridSelector.SelectedItems.Count > 0));
        }
        /// <summary>
        /// Opens the selection
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void OnPicturesEnableSelectClick(object sender, EventArgs e)
        {
            GridSelector.EnforceIsSelectionEnabled = true;
        }

        /// <summary>
        /// Back Key Pressed = leaves the selection mode
        /// </summary>
        /// <param name="e"></param>
        protected void OnGridBackKeyPressed(CancelEventArgs e)
        {
            if (GridSelector.IsSelectionEnabled)
            {
                GridSelector.EnforceIsSelectionEnabled = false;
                e.Cancel = true;
            }
        }

        /// <summary>
        /// Updates the application bar when the selection is opened or closed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnGridSelectorIsSelectionEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            SetupPicturesApplicationBar();
        }

        /// <summary>
        /// Simulates upload of the pictures
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void OnPicturesUploadClick(object sender, EventArgs e)
        {
            StringBuilder builder = new StringBuilder("Uploading:\n");
            foreach (Picture picture in GridSelector.SelectedItems)
            {
                builder.AppendLine(picture.City);
            }
            MessageBox.Show(builder.ToString());
        }

        /// <summary>
        /// Updates the application bar when the picture selection changes
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnGridSelectorSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdatePicturesApplicationBar();
        }

        /// <summary>
        /// Called when a picture is tapped : open it
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnPictureItemTap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            FrameworkElement fe = sender as FrameworkElement;
            if (fe != null)
            {
                CurrentPicture = fe.DataContext as Picture;
            }
        }
        #endregion

        #region Picture display
        Picture _currentPicture = null;

        /// <summary>
        /// Sets or gets the current displayed picture
        /// </summary>
        public Picture CurrentPicture
        {
            set
            {
                _currentPicture = value;
                ZoomGrid.DataContext = _currentPicture;
                bool hasPicture = (_currentPicture != null);
                ZoomGrid.Visibility = hasPicture ? Visibility.Visible : Visibility.Collapsed;
                ApplicationBar.IsVisible = !hasPicture;
                SamplePivot.IsEnabled = !hasPicture;
            }
            get { return _currentPicture; }
        }

        /// <summary>
        /// Tap on the picture : hide it and revert to grid mode
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnZoomGridTap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            CurrentPicture = null;
        }
        #endregion

        #region Databinding
        /// <summary>
        /// Hide the application bar
        /// </summary>
        void SetupBoundBuddiesApplicationBar()
        {
            ApplicationBar.IsVisible = false;
        }


       #endregion
    }
}