using Microsoft.Phone.Controls.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Microsoft.Phone.Controls
{
    internal abstract class DateTimePickerFlyoutPresenterHelperBase
    {
        private const string VisibilityGroupName = "VisibilityStates";
        private const string OpenVisibilityStateName = "Open";
        private const string ClosedVisibilityStateName = "Closed";

        private PickerFlyoutBase _flyout;
        private Control _presenter;

        protected DateTimePickerFlyoutPresenterHelperBase(PickerFlyoutBase flyout, Control presenter)
        {
            _flyout = flyout;
            _flyout.Opening += OnFlyoutOpening;
            _flyout.Closing += OnFlyoutClosing;

            _presenter = presenter;
            _presenter.Loaded += OnPresenterLoaded;
            _presenter.Unloaded += OnPresenterUnloaded;

            FirstPicker = CreatePicker();
            SecondPicker = CreatePicker();
            ThirdPicker = CreatePicker();

            InitializePickers();

            // Hook up to interesting events
            FirstPicker.DataSource.SelectionChanged += OnDataSourceSelectionChanged;
            SecondPicker.DataSource.SelectionChanged += OnDataSourceSelectionChanged;
            ThirdPicker.DataSource.SelectionChanged += OnDataSourceSelectionChanged;
            FirstPicker.IsExpandedChanged += OnSelectorIsExpandedChanged;
            SecondPicker.IsExpandedChanged += OnSelectorIsExpandedChanged;
            ThirdPicker.IsExpandedChanged += OnSelectorIsExpandedChanged;

            // Hide all selectors
            FirstPicker.Visibility = Visibility.Collapsed;
            SecondPicker.Visibility = Visibility.Collapsed;
            ThirdPicker.Visibility = Visibility.Collapsed;

            // Position and reveal the culture-relevant selectors
            int column = 0;
            foreach (LoopingSelector selector in GetSelectorsOrderedByCulturePattern())
            {
                Grid.SetColumn(selector, column);
                selector.Visibility = Visibility.Visible;
                column++;
            }
        }

        internal DateTime Value
        {
            get { return _value; }
            set
            {
                _value = value;
                DateTimeWrapper wrapper = new DateTimeWrapper(value);
                FirstPicker.DataSource.SelectedItem = wrapper;
                SecondPicker.DataSource.SelectedItem = wrapper;
                ThirdPicker.DataSource.SelectedItem = wrapper;
            }
        }
        private DateTime _value;

        internal LoopingSelector FirstPicker { get; private set; }

        internal LoopingSelector SecondPicker { get; private set; }

        internal LoopingSelector ThirdPicker { get; private set; }

        private VisualStateGroup VisibilityStates { get; set; }

        private bool IsOpen { get; set; }

        protected abstract void InitializePickers();

        protected abstract void CommitCore(DateTime value);

        internal void BeforeOnApplyTemplate()
        {
            if (VisibilityStates != null)
            {
                VisibilityStates.CurrentStateChanged -= OnVisibilityStatesCurrentStateChanged;
            }
        }

        internal void AfterOnApplyTemplate()
        {
            FrameworkElement templateRoot = VisualStates.GetImplementationRoot(_presenter);
            if (templateRoot != null)
            {
                FirstPicker.ItemTemplate = TryFindResource<DataTemplate>(templateRoot, "FirstPickerItemTemplate");
                SecondPicker.ItemTemplate = TryFindResource<DataTemplate>(templateRoot, "SecondPickerItemTemplate");
                ThirdPicker.ItemTemplate = TryFindResource<DataTemplate>(templateRoot, "ThirdPickerItemTemplate");
            }

            VisibilityStates = VisualStates.TryGetVisualStateGroup(_presenter, VisibilityGroupName);

            if (VisibilityStates != null)
            {
                VisibilityStates.CurrentStateChanged += OnVisibilityStatesCurrentStateChanged;
            }

            UpdateVisualStates(false);
        }

        internal void Commit()
        {
            CommitCore(((DateTimeWrapper)FirstPicker.DataSource.SelectedItem).DateTime);
            CloseFlyout(true);
        }

        internal void SetIsHitTestVisible(bool value)
        {
            FirstPicker.IsHitTestVisible = value;
            SecondPicker.IsHitTestVisible = value;
            ThirdPicker.IsHitTestVisible = value;
        }

        private void OnDataSourceSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Push the selected item to all selectors
            DataSource dataSource = (DataSource)sender;
            FirstPicker.DataSource.SelectedItem = dataSource.SelectedItem;
            SecondPicker.DataSource.SelectedItem = dataSource.SelectedItem;
            ThirdPicker.DataSource.SelectedItem = dataSource.SelectedItem;
        }

        private void OnSelectorIsExpandedChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue)
            {
                // Ensure that only one picker is expanded at a time
                FirstPicker.IsExpanded = (sender == FirstPicker);
                SecondPicker.IsExpanded = (sender == SecondPicker);
                ThirdPicker.IsExpanded = (sender == ThirdPicker);
            }
        }

        private void OnFlyoutOpening(object sender, EventArgs e)
        {
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
            else
            {
                if (e.IsCancelable)
                {
                    e.Cancel = true;
                }
                else
                {
                    UpdateVisualStates(false);
                }
            }
        }

        private void OnPresenterLoaded(object sender, RoutedEventArgs e)
        {
            _presenter.Dispatcher.BeginInvoke(() =>
            {
                IsOpen = true;
                UpdateVisualStates(true);
            });
        }

        private void OnPresenterUnloaded(object sender, RoutedEventArgs e)
        {
            FirstPicker.IsExpanded = false;
            SecondPicker.IsExpanded = false;
            ThirdPicker.IsExpanded = false;
        }

        private void OnVisibilityStatesCurrentStateChanged(object sender, VisualStateChangedEventArgs e)
        {
            if ((e.OldState != null && e.OldState.Name == OpenVisibilityStateName) &&
                (e.NewState != null && e.NewState.Name == ClosedVisibilityStateName))
            {
                // Close the picker flyout
                _flyout.InternalHide(false);
            }
        }

        private void UpdateVisualStates(bool useTransitions)
        {
            SetIsHitTestVisible(IsOpen);

            VisualStateManager.GoToState(_presenter, IsOpen ? OpenVisibilityStateName : ClosedVisibilityStateName, useTransitions);
        }

        private void CloseFlyout(bool useTransitions)
        {
            IsOpen = false;
            UpdateVisualStates(useTransitions);
        }

        private LoopingSelector CreatePicker()
        {
            return new LoopingSelector
            {
                Width = 148,
                ItemSize = new Size(148, 148),
                ItemMargin = new Thickness(6)
            };
        }

        /// <summary>
        /// Gets a sequence of LoopingSelector parts ordered according to culture string for date/time formatting.
        /// </summary>
        /// <returns>LoopingSelectors ordered by culture-specific priority.</returns>
        protected abstract IEnumerable<LoopingSelector> GetSelectorsOrderedByCulturePattern();

        /// <summary>
        /// Gets a sequence of LoopingSelector parts ordered according to culture string for date/time formatting.
        /// </summary>
        /// <param name="pattern">Culture-specific date/time format string.</param>
        /// <param name="patternCharacters">Date/time format string characters for the primary/secondary/tertiary LoopingSelectors.</param>
        /// <param name="selectors">Instances for the primary/secondary/tertiary LoopingSelectors.</param>
        /// <returns>LoopingSelectors ordered by culture-specific priority.</returns>
        internal static IEnumerable<LoopingSelector> GetSelectorsOrderedByCulturePattern(string pattern, char[] patternCharacters, LoopingSelector[] selectors)
        {
            if (null == pattern)
            {
                throw new ArgumentNullException("pattern");
            }
            if (null == patternCharacters)
            {
                throw new ArgumentNullException("patternCharacters");
            }
            if (null == selectors)
            {
                throw new ArgumentNullException("selectors");
            }
            if (patternCharacters.Length != selectors.Length)
            {
                throw new ArgumentException("Arrays must contain the same number of elements.");
            }

            // Create a list of index and selector pairs
            List<Tuple<int, LoopingSelector>> pairs = new List<Tuple<int, LoopingSelector>>(patternCharacters.Length);
            for (int i = 0; i < patternCharacters.Length; i++)
            {
                pairs.Add(new Tuple<int, LoopingSelector>(pattern.IndexOf(patternCharacters[i]), selectors[i]));
            }

            // Return the corresponding selectors in order
            return pairs.Where(p => -1 != p.Item1).OrderBy(p => p.Item1).Select(p => p.Item2).Where(s => null != s);
        }

        private static T TryFindResource<T>(FrameworkElement element, object resourceKey)
        {
            if (element.Resources.Contains(resourceKey))
            {
                object value = element.Resources[resourceKey];
                if (value is T)
                {
                    return (T)value;
                }
            }

            return default(T);
        }
    }
}
