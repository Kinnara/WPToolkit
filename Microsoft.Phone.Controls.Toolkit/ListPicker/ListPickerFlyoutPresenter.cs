using Microsoft.Phone.Controls.Primitives;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Microsoft.Phone.Controls
{
    /// <summary>
    /// Represents a control that allows a user to pick one or more items from a list.
    /// </summary>
    [TemplatePart(Name = TitlePresenterName, Type = typeof(TextBlock))]
    [TemplatePart(Name = ItemsHostPanelName, Type = typeof(Grid))]
    public sealed class ListPickerFlyoutPresenter : Control
    {
        private const string TitlePresenterName = "TitlePresenter";
        private const string ItemsHostPanelName = "ItemsHostPanel";

        private ListPickerFlyout _flyout;
        private bool _templateApplied;
        private OrientationHelper _orientationHelper;

        internal ListPickerFlyoutPresenter(ListPickerFlyout flyout)
        {
            _flyout = flyout;
            _flyout.Opening += OnFlyoutOpening;
            _flyout.Confirmed += OnFlyoutConfirmed;
            _flyout.Closing += OnFlyoutClosing;

            DefaultStyleKey = typeof(ListPickerFlyoutPresenter);

            _orientationHelper = new OrientationHelper(this);

            Picker = new PickerBoxList();
            Picker.SetBinding(PickerBoxList.SelectedIndexProperty, new Binding("SelectedIndex") { Source = _flyout, Mode = BindingMode.TwoWay });
            Picker.SetBinding(PickerBoxList.SelectedItemProperty, new Binding("SelectedItem") { Source = _flyout, Mode = BindingMode.TwoWay });
            Picker.ItemClick += OnPickerItemClick;

            Loaded += OnLoaded;
        }

        internal PickerBoxList Picker { get; private set; }

        private TextBlock TitlePresenter { get; set; }

        private Grid ItemsHostPanel { get; set; }

        internal event EventHandler ItemPicked;

        /// <summary>
        /// Builds the visual tree for the control when a new template is applied.
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (TitlePresenter != null)
            {
                TitlePresenter.ClearValue(ReaderboardEffect.RowIndexProperty);
            }

            if (ItemsHostPanel != null)
            {
                ItemsHostPanel.Children.Remove(Picker);
                Picker.ClearValue(ReaderboardEffect.RowIndexProperty);
            }

            TitlePresenter = GetTemplateChild(TitlePresenterName) as TextBlock;
            ItemsHostPanel = GetTemplateChild(ItemsHostPanelName) as Grid;

            if (TitlePresenter != null)
            {
                UpdateTitlePresenter();
                ReaderboardEffect.SetRowIndex(TitlePresenter, 0);
            }

            if (ItemsHostPanel != null)
            {
                ItemsHostPanel.Children.Add(Picker);
                ReaderboardEffect.SetRowIndex(Picker, 1);
            }

            SetFlowDirection();

            _orientationHelper.OnApplyTemplate();

            _templateApplied = true;
        }

        private void SetFlowDirection()
        {
            FrameworkElement root = VisualStates.GetImplementationRoot(this);
            if (root != null)
            {
                if (root.ReadLocalValue(FlowDirectionProperty) == DependencyProperty.UnsetValue)
                {
                    root.FlowDirection = this.GetUsefulFlowDirection();
                }
            }
        }

        private void UpdateTitlePresenter()
        {
            if (TitlePresenter != null)
            {
                TitlePresenter.Text = PickerFlyoutBase.GetTitle(_flyout);
            }
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            ApplyTemplate();

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
                    Picker.ScrollIntoView(Picker.SelectedItem);
                }
                else
                {
                    if (Picker.SelectedItems.Count > 0)
                    {
                        Picker.ScrollIntoView(Picker.SelectedItems[0]);
                    }
                }
            }
        }

        private void Commit()
        {
            // Commit the value and close
            _flyout.InternalHide(true);
        }

        private void OnPickerItemClick(object sender, EventArgs e)
        {
            // We listen to the ItemClick event because SelectionChanged does not fire if the user picks the already selected item.

            // Only close the flyout in Single Selection mode.
            if (Picker.SelectionMode == SelectionMode.Single)
            {
                // Commit the value and close
                Commit();
                SafeRaise.Raise(ItemPicked, this);
            }
        }

        private void OnFlyoutOpening(object sender, EventArgs e)
        {
            if (_templateApplied)
            {
                UpdateTitlePresenter();
            }
        }

        private void OnFlyoutConfirmed(object sender, EventArgs e)
        {
            Commit();
        }

        private void OnFlyoutClosing(object sender, EventArgs e)
        {
            // If the Picker is scrolling stop it from moving, this is both
            // consistant with Metro and allows for attaching the animations
            // to the correct, in view items.
            ScrollViewer scrollViewer = Picker.GetVisualChildren().OfType<ScrollViewer>().FirstOrDefault();
            if (scrollViewer != null)
            {
                scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset);
            }
        }
    }
}
