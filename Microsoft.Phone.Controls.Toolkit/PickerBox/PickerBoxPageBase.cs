// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Navigation;
using Microsoft.Phone.Controls.LocalizedResources;
using Microsoft.Phone.Controls.Primitives;
using Microsoft.Phone.Shell;

namespace Microsoft.Phone.Controls
{
    /// <summary>
    /// Represents a base class for pages that work with PickerBox to display the list of items and allows single or multiple selection.
    /// </summary>
    public abstract class PickerBoxPageBase : BasePage, IPickerBoxPage
    {
        private const string StateKey_Value = "PickerBoxPageBase_State_Value";

        private PageOrientation _lastOrientation;

        private IList<WeakReference> _itemsToAnimate;

        /// <summary>
        /// Gets or sets the string of text to display as the header of the page.
        /// </summary>
        public string HeaderText { get; set; }

        /// <summary>
        /// Gets or sets the list of items to display.
        /// </summary>
        public IList Items { get; private set; }

        /// <summary>
        /// Gets or sets the selection mode.
        /// </summary>
        public SelectionMode SelectionMode { get; set; }

        /// <summary>
        /// Gets or sets the selected item.
        /// </summary>
        public object SelectedItem { get; set; }

        /// <summary>
        /// Gets or sets the list of items to select.
        /// </summary>
        public IList SelectedItems { get; private set; }

        /// <summary>
        /// Gets or sets the item template
        /// </summary>
        public DataTemplate ItemTemplate { get; set; }

        /// <summary>
        /// Gets or sets the item container style
        /// </summary>
        public Style ItemContainerStyle { get; set; }

        /// <summary>
        /// Gets or sets the name or path of the property that is displayed for each data item.
        /// </summary>
        public string DisplayMemberPath { get; set; }

        private PickerBoxList Picker { get; set; }

        private TextBlock HeaderTitle { get; set; }

        /// <summary>
        /// Whether the picker page is open or not.
        /// </summary>
        private bool IsOpen
        {
            get { return (bool)GetValue(IsOpenProperty); }
            set { SetValue(IsOpenProperty, value); }
        }

        private static readonly DependencyProperty IsOpenProperty =
            DependencyProperty.Register("IsOpen",
                                        typeof(bool),
                                        typeof(PickerBoxPageBase),
                                        new PropertyMetadata(false, new PropertyChangedCallback(OnIsOpenChanged)));

        private static void OnIsOpenChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            (o as PickerBoxPageBase).OnIsOpenChanged();
        }

        private void OnIsOpenChanged()
        {
            UpdateVisualState(true);
        }

        /// <summary>
        /// Initializes the PickerBoxPageBase class; must be called from the subclass's constructor.
        /// </summary>
        /// <param name="picker">Picker.</param>
        /// <param name="headerTitle">Header title.</param>
        protected void InitializePickerBoxPage(PickerBoxList picker, TextBlock headerTitle)
        {
            if (null == picker)
            {
                throw new ArgumentNullException("picker");
            }
            if (null == headerTitle)
            {
                throw new ArgumentNullException("headerTitle");
            }

            Picker = picker;
            HeaderTitle = headerTitle;

            Picker.ItemClick += OnPickerItemClick;

            Items = new List<object>();
            SelectedItems = new List<object>();

            Loaded += OnLoaded;
            Unloaded += OnUnloaded;

            _lastOrientation = Orientation;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            OrientationChanged += OnOrientationChanged;
            _lastOrientation = Orientation;

            // Customize the ApplicationBar Buttons by providing the right text
            if (null != ApplicationBar)
            {
                foreach (object obj in ApplicationBar.Buttons)
                {
                    IApplicationBarIconButton button = obj as IApplicationBarIconButton;
                    if (null != button)
                    {
                        if ("DONE" == button.Text)
                        {
                            button.Text = ControlResources.DateTimePickerDoneText;
                            button.Click += OnDoneButtonClick;
                        }
                        else if ("CANCEL" == button.Text)
                        {
                            button.Text = ControlResources.DateTimePickerCancelText;
                            button.Click += OnCancelButtonClick;
                        }
                    }
                }
            }

            // Scroll the selected item into view, needs to be done before the
            // animations are setup because the animations are only attached to
            // the items in view.
            if (SelectionMode == SelectionMode.Single)
            {
                Picker.ScrollIntoView(SelectedItem);
            }
            else
            {
                if (SelectedItems.Count > 0)
                {
                    Picker.ScrollIntoView(SelectedItems[0]);
                }
            }

            // Add a projection for each list item and turn it to -90
            // (rotationX) so it is hidden.
            SetupListItems(-90);

            PlaneProjection headerProjection = (PlaneProjection)HeaderTitle.Projection;
            if (null == headerProjection)
            {
                headerProjection = new PlaneProjection();
                HeaderTitle.Projection = headerProjection;
            }
            headerProjection.RotationX = -90;

            Picker.Opacity = 1;

            Dispatcher.BeginInvoke(() =>
            {
                IsOpen = true;
            });
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            OrientationChanged -= OnOrientationChanged;
        }

        private void SetupListItems(double degree)
        {
            _itemsToAnimate = Picker.GetItemsInViewPort();

            for (int i = 0; i < _itemsToAnimate.Count; i++)
            {
                FrameworkElement item = (FrameworkElement)_itemsToAnimate[i].Target;
                if (null != item)
                {
                    PlaneProjection p = (PlaneProjection)item.Projection;
                    if (null == p)
                    {
                        p = new PlaneProjection();
                        item.Projection = p;
                    }
                    p.RotationX = degree;
                }
            }
        }

        /// <summary>
        /// Called when a page becomes the active page in a frame.
        /// </summary>
        /// <param name="e">An object that contains the event data.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (null == e)
            {
                throw new ArgumentNullException("e");
            }

            base.OnNavigatedTo(e);

            // Restore Value if returning to application (to avoid inconsistent state)
            if (State.ContainsKey(StateKey_Value))
            {
                State.Remove(StateKey_Value);

                // Back out from picker page for consistency with behavior of core pickers in this scenario
                if (NavigationService.CanGoBack)
                {
                    Visibility = Visibility.Collapsed;
                    NavigationService.GoBack();
                    return;
                }
            }

            // Automatically uppercase the text for the header.
            if (null != HeaderText)
            {
                HeaderTitle.Text = HeaderText.ToUpper(CultureInfo.CurrentCulture);
            }

            Picker.DataContext = Items;

            Picker.SelectionMode = SelectionMode;

            if (null != ItemTemplate)
            {
                Picker.ItemTemplate = ItemTemplate;
            }
            else if (null != DisplayMemberPath)
            {
                Picker.DisplayMemberPath = DisplayMemberPath;
            }

            if (null != ItemContainerStyle)
            {
                Picker.ItemContainerStyle = ItemContainerStyle;
            }

            if (SelectionMode == SelectionMode.Single)
            {
                if (ApplicationBar != null)
                {
                    ApplicationBar.IsVisible = false;
                }

                Picker.SelectedItem = SelectedItem;
            }
            else
            {
                if (ApplicationBar != null)
                {
                    ApplicationBar.IsVisible = true;
                }

                foreach (object item in Items)
                {
                    if (null != SelectedItems && SelectedItems.Contains(item))
                    {
                        Picker.SelectedItems.Add(item);
                    }
                }
            }
        }

        private void OnDoneButtonClick(object sender, EventArgs e)
        {
            // Commit the value and close
            SelectedItem = Picker.SelectedItem;
            SelectedItems = Picker.SelectedItems;
            ClosePickerPage();
        }

        private void OnCancelButtonClick(object sender, EventArgs e)
        {
            // Close without committing a value
            SelectedItem = null;
            SelectedItems = null;
            ClosePickerPage();
        }

        /// <summary>
        /// Called when the Back key is pressed.
        /// </summary>
        /// <param name="e">Event arguments.</param>
        protected override void OnBackKeyPress(CancelEventArgs e)
        {
            if (null == e)
            {
                throw new ArgumentNullException("e");
            }

            // Cancel back action so we can play the Close state animation (then go back)
            e.Cancel = true;
            SelectedItem = null;
            SelectedItems = null;
            ClosePickerPage();
        }

        private void ClosePickerPage()
        {
            // Prevent user from selecting an item as the picker is closing,
            // disabling the control would cause the UI to change so instead
            // it's hidden from hittesting.
            Picker.IsHitTestVisible = false;

            IsOpen = false;
        }

        private void OnClosedStoryboardCompleted(object sender, EventArgs e)
        {
            // Close the picker page
            if (NavigationService.CanGoBack)
            {
                NavigationService.GoBack();
            }
        }

        /// <summary>
        /// Called when a page is no longer the active page in a frame.
        /// </summary>
        /// <param name="e">An object that contains the event data.</param>
        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            if (null == e)
            {
                throw new ArgumentNullException("e");
            }

            base.OnNavigatedFrom(e);

            // Save Value if navigating away from application
            if (e.Uri.IsExternalNavigation())
            {
                State[StateKey_Value] = StateKey_Value;
            }
        }

        private void OnOrientationChanged(object sender, OrientationChangedEventArgs e)
        {
            PageOrientation newOrientation = e.Orientation;

            RotateTransition transitionElement = new RotateTransition();

            switch (newOrientation)
            {
                case PageOrientation.Portrait:
                case PageOrientation.PortraitUp:
                    transitionElement.Mode = (_lastOrientation == PageOrientation.LandscapeLeft) ?
                    RotateTransitionMode.In90Counterclockwise : RotateTransitionMode.In90Clockwise;

                    break;
                case PageOrientation.Landscape:
                case PageOrientation.LandscapeLeft:
                    transitionElement.Mode = (_lastOrientation == PageOrientation.LandscapeRight) ?
                    RotateTransitionMode.In180Counterclockwise : RotateTransitionMode.In90Clockwise;
                    break;
                case PageOrientation.LandscapeRight:
                    transitionElement.Mode = (_lastOrientation == PageOrientation.PortraitUp) ?
                    RotateTransitionMode.In90Counterclockwise : RotateTransitionMode.In180Clockwise;
                    break;
            }

            PhoneApplicationPage phoneApplicationPage = (PhoneApplicationPage)(((PhoneApplicationFrame)Application.Current.RootVisual)).Content;
            ITransition transition = transitionElement.GetTransition(phoneApplicationPage);
            transition.Completed += delegate
            {
                transition.Stop();
            };
            transition.Begin();

            _lastOrientation = newOrientation;
        }

        private void UpdateVisualState(bool useTransitions)
        {
            if (useTransitions)
            {
                // If the Picker is scrolling stop it from moving, this is both
                // consistant with Metro and allows for attaching the animations
                // to the correct, in view items.
                ScrollViewer scrollViewer = Picker.GetVisualChildren().OfType<ScrollViewer>().FirstOrDefault();
                if (scrollViewer != null)
                {
                    scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset);
                }

                if (!IsOpen)
                {
                    SetupListItems(0);
                }

                Storyboard mainBoard = new Storyboard();

                Storyboard headerBoard = AnimationForElement(HeaderTitle, 0);
                mainBoard.Children.Add(headerBoard);

                for (int i = 0; i < _itemsToAnimate.Count; i++)
                {
                    FrameworkElement element = (FrameworkElement)_itemsToAnimate[i].Target;
                    Storyboard board = AnimationForElement(element, i + 1);
                    mainBoard.Children.Add(board);
                }

                if (!IsOpen)
                {
                    mainBoard.Completed += OnClosedStoryboardCompleted;
                }

                mainBoard.Begin();
            }
            else if (!IsOpen)
            {
                OnClosedStoryboardCompleted(null, null);
            }
        }

        /// <summary>Gets the animation for the specified element.</summary>
        /// <param name="element">The element.</param>
        /// <param name="index">The index of the element.</param>
        /// <returns>The Storyboard for the specified element.</returns>
        protected virtual Storyboard AnimationForElement(FrameworkElement element, int index)
        {
            double delay = 30;
            double duration = (IsOpen) ? 350 : 250;
            double from = (IsOpen) ? -45 : 0;
            double to = (IsOpen) ? 0 : 90;
            ExponentialEase ee = new ExponentialEase()
            {
                EasingMode = (IsOpen) ? EasingMode.EaseOut : EasingMode.EaseIn,
                Exponent = 5,
            };

            DoubleAnimation anim = new DoubleAnimation()
            {
                Duration = new Duration(TimeSpan.FromMilliseconds(duration)),
                From = from,
                To = to,
                EasingFunction = ee,
            };

            Storyboard.SetTarget(anim, element);
            Storyboard.SetTargetProperty(anim, new PropertyPath("(UIElement.Projection).(PlaneProjection.RotationX)"));

            Storyboard board = new Storyboard();
            board.BeginTime = TimeSpan.FromMilliseconds(delay * index);
            board.Children.Add(anim);

            return board;
        }

        private void OnPickerItemClick(object sender, EventArgs e)
        {
            // We listen to the ItemClick event because SelectionChanged does not fire if the user picks the already selected item.

            // Only close the page in Single Selection mode.
            if (SelectionMode == SelectionMode.Single)
            {
                // Commit the value and close
                SelectedItem = Picker.SelectedItem;
                ClosePickerPage();
            }
        }
    }
}