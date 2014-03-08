using Microsoft.Phone.Controls.Primitives;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Microsoft.Phone.Controls
{
    /// <summary>
    /// Represents a control that allows a user to pick one or more items from a list.
    /// </summary>
    public sealed class ListPickerFlyoutPresenter : Control
    {
        private const string ElementTitlePresenterName = "TitlePresenter";
        private const string ElementItemsHostPanelName = "ItemsHostPanel";

        private ListPickerFlyout _flyout;
        private IList<WeakReference> _itemsToAnimate;
        private bool _templateApplied;
        private OrientationHelper _orientationHelper;
        private Storyboard _openedStoryboard;
        private Storyboard _closedStoryboard;

        internal ListPickerFlyoutPresenter(ListPickerFlyout flyout)
        {
            _flyout = flyout;
            _flyout.Opening += OnFlyoutOpening;
            _flyout.Confirmed += OnFlyoutConfirmed;
            _flyout.Closing += OnFlyoutClosing;

            DefaultStyleKey = typeof(ListPickerFlyoutPresenter);

            _orientationHelper = new OrientationHelper(this);

            Picker = new PickerBoxList();
            Picker.ItemClick += OnPickerItemClick;

            Loaded += OnLoaded;
            Unloaded += ListPickerFlyoutPresenter_Unloaded;
        }

        private void ListPickerFlyoutPresenter_Unloaded(object sender, RoutedEventArgs e)
        {
            ResetListItems();
        }

        private PickerBoxList Picker { get; set; }

        private TextBlock ElementTitlePresenter { get; set; }

        private Grid ElementItemsHostPanel { get; set; }

        #region private bool IsOpen

        private bool IsOpen
        {
            get { return (bool)GetValue(IsOpenProperty); }
            set { SetValue(IsOpenProperty, value); }
        }

        private static readonly DependencyProperty IsOpenProperty = DependencyProperty.Register(
            "IsOpen",
            typeof(bool),
            typeof(ListPickerFlyoutPresenter),
            null);

        #endregion

        #region internal DataTemplate DefaultSingleSelectItemTemplat

        internal DataTemplate DefaultSingleSelectItemTemplate
        {
            get { return (DataTemplate)GetValue(DefaultSingleSelectItemTemplateProperty); }
            set { SetValue(DefaultSingleSelectItemTemplateProperty, value); }
        }

        internal static readonly DependencyProperty DefaultSingleSelectItemTemplateProperty = DependencyProperty.Register(
            "DefaultSingleSelectItemTemplate",
            typeof(DataTemplate),
            typeof(ListPickerFlyoutPresenter),
            null);

        #endregion

        #region internal DataTemplate DefaultMultiSelectItemTemplate

        internal DataTemplate DefaultMultiSelectItemTemplate
        {
            get { return (DataTemplate)GetValue(DefaultMultiSelectItemTemplateProperty); }
            set { SetValue(DefaultMultiSelectItemTemplateProperty, value); }
        }

        internal static readonly DependencyProperty DefaultMultiSelectItemTemplateProperty = DependencyProperty.Register(
            "DefaultMultiSelectItemTemplate",
            typeof(DataTemplate),
            typeof(ListPickerFlyoutPresenter),
            null);

        #endregion

        #region internal Style DefaultSingleSelectItemContainerStyle

        internal Style DefaultSingleSelectItemContainerStyle
        {
            get { return (Style)GetValue(DefaultSingleSelectItemContainerStyleProperty); }
            set { SetValue(DefaultSingleSelectItemContainerStyleProperty, value); }
        }

        internal static readonly DependencyProperty DefaultSingleSelectItemContainerStyleProperty = DependencyProperty.Register(
            "DefaultSingleSelectItemContainerStyle",
            typeof(Style),
            typeof(ListPickerFlyoutPresenter),
            null);

        #endregion

        #region internal Style DefaultMultiSelectItemContainerStyle

        internal Style DefaultMultiSelectItemContainerStyle
        {
            get { return (Style)GetValue(DefaultMultiSelectItemContainerStyleProperty); }
            set { SetValue(DefaultMultiSelectItemContainerStyleProperty, value); }
        }

        internal static readonly DependencyProperty DefaultMultiSelectItemContainerStyleProperty = DependencyProperty.Register(
            "DefaultMultiSelectItemContainerStyle",
            typeof(Style),
            typeof(ListPickerFlyoutPresenter),
            null);

        #endregion

        internal event EventHandler ItemPicked;

        /// <summary>
        /// Builds the visual tree for the control when a new template is applied.
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (ElementItemsHostPanel != null)
            {
                ElementItemsHostPanel.Children.Remove(Picker);
            }

            ElementTitlePresenter = GetTemplateChild(ElementTitlePresenterName) as TextBlock;
            ElementItemsHostPanel = GetTemplateChild(ElementItemsHostPanelName) as Grid;

            SetupTitle();
            UpdateTitlePresenter();

            if (DefaultSingleSelectItemTemplate != null)
            {
                Picker.DefaultSingleSelectItemTemplate = DefaultSingleSelectItemTemplate;
            }

            if (DefaultMultiSelectItemTemplate != null)
            {
                Picker.DefaultMultiSelectItemTemplate = DefaultMultiSelectItemTemplate;
            }

            if (DefaultSingleSelectItemContainerStyle != null)
            {
                Picker.DefaultSingleSelectItemContainerStyle = DefaultSingleSelectItemContainerStyle;
            }

            if (DefaultMultiSelectItemContainerStyle != null)
            {
                Picker.DefaultMultiSelectItemContainerStyle = DefaultMultiSelectItemContainerStyle;
            }

            UpdatePicker();

            if (ElementItemsHostPanel != null)
            {
                ElementItemsHostPanel.Children.Add(Picker);
            }

            _orientationHelper.OnApplyTemplate();

            _templateApplied = true;
        }

        private void UpdateTitlePresenter()
        {
            if (ElementTitlePresenter != null)
            {
                // Automatically uppercase the text for the title.
                string title = ListPickerFlyout.GetTitle(_flyout);
                if (title != null)
                {
                    ElementTitlePresenter.Text = title.ToUpper(CultureInfo.CurrentCulture);
                }
                else
                {
                    ElementTitlePresenter.Text = string.Empty;
                }
            }
        }

        private void UpdatePicker()
        {
            Picker.SelectionMode = ToSelectionMode(_flyout.SelectionMode);

            if (_flyout.ItemTemplate != null)
            {
                Picker.DisplayMemberPath = null;
                Picker.ItemTemplate = _flyout.ItemTemplate;
            }
            else if (_flyout.DisplayMemberPath != null)
            {
                Picker.DisplayMemberPath = _flyout.DisplayMemberPath;
                Picker.ItemTemplate = null;
            }
            else
            {
                Picker.DisplayMemberPath = null;
                Picker.ItemTemplate = null;
            }

            Picker.ItemsSource = _flyout.ItemsSource;

            if (_flyout.SelectionMode == ListPickerFlyoutSelectionMode.Single)
            {
                Picker.SelectedItem = _flyout.SelectedItem;
            }
            else if (!Picker.SelectedItems.Cast<object>().SequenceEqual(_flyout.SelectedItems.Cast<object>()))
            {
                if (Picker.SelectedItems.Count > 0)
                {
                    Picker.SelectedItems.Clear();
                }

                foreach (object item in _flyout.SelectedItems)
                {
                    Picker.SelectedItems.Add(item);
                }
            }
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            Picker.UpdateLayout();

            if (Picker.SelectedItems.Count == 0 && Picker.Items.Count > 0)
            {
                Picker.ScrollIntoView(Picker.Items[0]);
            }
            else
            {
                // Scroll the selected item into view, needs to be done before the
                // animations are setup because the animations are only attached to
                // the items in view.
                if (Picker.SelectionMode == SelectionMode.Single)
                {
                    Picker.ScrollIntoView(_flyout.SelectedItem);
                }
                else
                {
                    if (Picker.SelectedItems.Count > 0)
                    {
                        Picker.ScrollIntoView(Picker.SelectedItems[0]);
                    }
                }
            }

            // Add a projection for each list item and turn it to -90
            // (rotationX) so it is hidden.
            SetupListItems(-90);

            Picker.Opacity = 1;

            Dispatcher.BeginInvoke(() =>
            {
                IsOpen = true;
                UpdateVisualStates(true);
            });
        }

        private void SetupTitle()
        {
            if (ElementTitlePresenter != null)
            {
                PlaneProjection titleProjection = (PlaneProjection)ElementTitlePresenter.Projection;
                if (null == titleProjection)
                {
                    titleProjection = new PlaneProjection();
                    ElementTitlePresenter.Projection = titleProjection;
                }
                titleProjection.RotationX = -90;
            }
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

        private void ResetListItems()
        {
            //SetupListItems(0);
        }

        private void Commit()
        {
            // Commit the value and close
            _flyout.SelectedItem = Picker.SelectedItem;
            _flyout.SelectedItems.Clear();
            foreach (object item in Picker.SelectedItems)
            {
                _flyout.SelectedItems.Add(item);
            }
            CloseFlyout(true);
        }

        private void CloseFlyout(bool useTransitions)
        {
            if (_openedStoryboard != null)
            {
                _openedStoryboard.SkipToFill();
            }

            // Prevent user from selecting an item as the picker is closing,
            // disabling the control would cause the UI to change so instead
            // it's hidden from hittesting.
            Picker.IsHitTestVisible = false;

            IsOpen = false;
            UpdateVisualStates(useTransitions);
        }

        private void OnClosedStoryboardCompleted(object sender, EventArgs e)
        {
            _closedStoryboard.Stop();
            _closedStoryboard = null;

            // Close the picker flyout
            _flyout.InternalHide(false);
        }

        private void UpdateVisualStates(bool useTransitions)
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

                Storyboard titleBoard = AnimationForElement(ElementTitlePresenter, 0);
                mainBoard.Children.Add(titleBoard);

                for (int i = 0; i < _itemsToAnimate.Count; i++)
                {
                    FrameworkElement element = (FrameworkElement)_itemsToAnimate[i].Target;
                    Storyboard board = AnimationForElement(element, i + 1);
                    mainBoard.Children.Add(board);
                }

                if (!IsOpen)
                {
                    _closedStoryboard = mainBoard;
                    _closedStoryboard.Completed += OnClosedStoryboardCompleted;
                }
                else
                {
                    _openedStoryboard = mainBoard;
                    _openedStoryboard.Completed += delegate
                    {
                        _openedStoryboard = null;
                    };
                }

                mainBoard.Begin();
            }
            else if (!IsOpen)
            {
                OnClosedStoryboardCompleted(null, null);
            }
        }

        /// <summary>
        /// Gets the animation for the specified element.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <param name="index">The index of the element.</param>
        /// <returns>The Storyboard for the specified element.</returns>
        private Storyboard AnimationForElement(FrameworkElement element, int index)
        {
            double delay = 20;
            double duration = (IsOpen) ? 300 : 128;
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

            // Only close the flyout in Single Selection mode.
            if (_flyout.SelectionMode == ListPickerFlyoutSelectionMode.Single)
            {
                // Commit the value and close
                Commit();
                SafeRaise.Raise(ItemPicked, this);
            }
        }

        private void OnFlyoutOpening(object sender, EventArgs e)
        {
            Picker.IsHitTestVisible = true;
            Picker.Opacity = 0;

            if (_templateApplied)
            {
                SetupTitle();
                UpdateTitlePresenter();
                UpdatePicker();
                //ResetListItems();
            }
        }

        private void OnFlyoutConfirmed(object sender, EventArgs e)
        {
            Commit();
        }

        private void OnFlyoutClosing(object sender, FlyoutClosingEventArgs e)
        {
            if (IsOpen)
            {
                if (e.IsCancelable)
                {
                    e.Cancel = true;
                    CloseFlyout(true);
                }
                else
                {
                    CloseFlyout(false);
                }
            }
            else if (_closedStoryboard != null)
            {
                if (e.IsCancelable)
                {
                    e.Cancel = true;
                }
                else
                {
                    _closedStoryboard.SkipToFill();
                }
            }
        }

        private static SelectionMode ToSelectionMode(ListPickerFlyoutSelectionMode selectionMode)
        {
            switch (selectionMode)
            {
                case ListPickerFlyoutSelectionMode.Single:
                    return SelectionMode.Single;
                case ListPickerFlyoutSelectionMode.Multiple:
                    return SelectionMode.Multiple;
                default:
                    throw new ArgumentOutOfRangeException("selectionMode");
            }
        }
    }
}
