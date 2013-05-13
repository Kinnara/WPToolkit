// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Navigation;
using Microsoft.Phone.Controls.Primitives;

namespace Microsoft.Phone.Controls
{
    /// <summary>
    /// Picker box is a versatile control that can be used to select either a single item or multiple items.
    /// </summary>
    /// <QualityBand>Preview</QualityBand>
    [TemplatePart(Name = ButtonPartName, Type = typeof(ButtonBase))]
    [TemplateVisualState(GroupName = PickerStatesGroupName, Name = PickerStatesNormalStateName)]
    [TemplateVisualState(GroupName = PickerStatesGroupName, Name = PickerStatesDisabledStateName)]
    public class PickerBox : ItemsControl
    {
        private const string ButtonPartName = "Button";

        private const string PickerStatesGroupName = "PickerStates";
        private const string PickerStatesNormalStateName = "Normal";
        private const string PickerStatesDisabledStateName = "Disabled";

        private ButtonBase _buttonPart;
        private bool _updatingSelection;
        private int _deferredSelectedIndex = -1;
        private object _deferredSelectedItem = null;

        private PickerPageHelper<IPickerBoxPage> _pickerPageHelper;

        /// <summary>
        /// Event that is raised when the selection changes.
        /// </summary>
        public event SelectionChangedEventHandler SelectionChanged;

        /// <summary>
        /// Gets or sets the delegate, which is called to summarize a list of selections into a string.
        /// If not implemented, the default summarizing behavior will be used.
        /// If this delegate is implemented, default summarizing behavior can be achieved by returning
        /// null instead of a string.
        /// </summary>
        public Func<IList, string> SummaryForSelectedItemsDelegate
        {
            get { return (Func<IList, string>)GetValue(SummaryForSelectedItemsDelegateProperty); }
            set { SetValue(SummaryForSelectedItemsDelegateProperty, value); }
        }

        /// <summary>
        /// Identifies the SummaryForSelectedItemsDelegate DependencyProperty.
        /// </summary>
        public static readonly DependencyProperty SummaryForSelectedItemsDelegateProperty =
            DependencyProperty.Register("SummaryForSelectedItemsDelegate", typeof(Func<IList, string>), typeof(PickerBox), null);

        /// <summary>
        /// Gets or sets the PickerBoxMode (ex: Normal/Expanded/Full).
        /// </summary>
        public PickerBoxMode PickerBoxMode
        {
            get { return (PickerBoxMode)GetValue(PickerBoxModeProperty); }
            private set { SetValue(PickerBoxModeProperty, value); }
        }

        /// <summary>
        /// Identifies the PickerBoxMode DependencyProperty.
        /// </summary>
        public static readonly DependencyProperty PickerBoxModeProperty =
            DependencyProperty.Register("PickerBoxMode", typeof(PickerBoxMode), typeof(PickerBox), new PropertyMetadata(PickerBoxMode.Normal, OnPickerBoxModeChanged));

        private static void OnPickerBoxModeChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            ((PickerBox)o).OnPickerBoxModeChanged((PickerBoxMode)e.OldValue, (PickerBoxMode)e.NewValue);
        }

        private void OnPickerBoxModeChanged(PickerBoxMode oldValue, PickerBoxMode newValue)
        {
            if (PickerBoxMode.Full == oldValue)
            {
                _pickerPageHelper.ClosePickerPage();
            }
            if (PickerBoxMode.Full == newValue)
            {
                _pickerPageHelper.OpenPickerPage(PickerPageUri);
            }
        }

        /// <summary>
        /// Enabled property changed
        /// </summary>
        private void OnIsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            UpdateVisualStates(true);
        }

        /// <summary>
        /// Gets or sets the index of the selected item.
        /// </summary>
        public int SelectedIndex
        {
            get { return (int)GetValue(SelectedIndexProperty); }
            set { SetValue(SelectedIndexProperty, value); }
        }

        /// <summary>
        /// Identifies the SelectedIndex DependencyProperty.
        /// </summary>
        public static readonly DependencyProperty SelectedIndexProperty =
            DependencyProperty.Register("SelectedIndex", typeof(int), typeof(PickerBox), new PropertyMetadata(-1, OnSelectedIndexChanged));

        private static void OnSelectedIndexChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            ((PickerBox)o).OnSelectedIndexChanged((int)e.OldValue, (int)e.NewValue);
        }

        [SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "SelectedIndex", Justification = "Property name.")]
        private void OnSelectedIndexChanged(int oldValue, int newValue)
        {
            // Validate new value
            if ((Items.Count <= newValue) ||
                ((0 < Items.Count) && (newValue < 0)) ||
                ((0 == Items.Count) && (newValue != -1)))
            {
                if ((null == Template) && (0 <= newValue))
                {
                    // Can't set the value now; remember it for later
                    _deferredSelectedIndex = newValue;
                    return;
                }
                throw new InvalidOperationException(Properties.Resources.InvalidSelectedIndex);
            }

            // Synchronize SelectedItem property
            if (!_updatingSelection)
            {
                _updatingSelection = true;
                SelectedItem = (-1 != newValue) ? Items[newValue] : null;
                _updatingSelection = false;
            }
        }

        /// <summary>
        /// Gets or sets the selected item.
        /// </summary>
        public object SelectedItem
        {
            get { return (object)GetValue(SelectedItemProperty); }
            set { SetValue(SelectedItemProperty, value); }
        }

        /// <summary>
        /// Identifies the SelectedItem DependencyProperty.
        /// </summary>
        public static readonly DependencyProperty SelectedItemProperty =
            DependencyProperty.Register("SelectedItem", typeof(object), typeof(PickerBox), new PropertyMetadata(null, OnSelectedItemChanged));

        private static void OnSelectedItemChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            ((PickerBox)o).OnSelectedItemChanged(e.OldValue, e.NewValue);
        }

        [SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "SelectedItem", Justification = "Property name.")]
        private void OnSelectedItemChanged(object oldValue, object newValue)
        {
            if (newValue != null && (null == Items || Items.Count == 0))
            {
                if (null == Template)
                {
                    // Can't set the value now; remember it for later
                    _deferredSelectedItem = newValue;
                    return;
                }
                else
                {
                    throw new InvalidOperationException(Properties.Resources.InvalidSelectedItem);
                }
            }

            // Validate new value
            int newValueIndex = (null != newValue) ? Items.IndexOf(newValue) : -1;

            if ((-1 == newValueIndex) && (0 < Items.Count))
            {
                throw new InvalidOperationException(Properties.Resources.InvalidSelectedItem);
            }

            // Synchronize SelectedIndex property
            if (!_updatingSelection)
            {
                _updatingSelection = true;
                SelectedIndex = newValueIndex;
                _updatingSelection = false;
            }

            // Switch to Normal mode or size for current item
            if (PickerBoxMode.Normal != PickerBoxMode)
            {
                PickerBoxMode = PickerBoxMode.Normal;
            }

            // Fire SelectionChanged event
            var handler = SelectionChanged;
            if (null != handler)
            {
                IList removedItems = (null == oldValue) ? new object[0] : new object[] { oldValue };
                IList addedItems = (null == newValue) ? new object[0] : new object[] { newValue };
                handler(this, new SelectionChangedEventArgs(removedItems, addedItems));
            }

            UpdateButtonContent();
        }

        /// <summary>
        /// Gets or sets the DataTemplate used to display each item when PickerBoxMode is set to Full.
        /// </summary>
        public DataTemplate FullModeItemTemplate
        {
            get { return (DataTemplate)GetValue(FullModeItemTemplateProperty); }
            set { SetValue(FullModeItemTemplateProperty, value); }
        }

        /// <summary>
        /// Identifies the FullModeItemTemplate DependencyProperty.
        /// </summary>
        public static readonly DependencyProperty FullModeItemTemplateProperty =
            DependencyProperty.Register("FullModeItemTemplate", typeof(DataTemplate), typeof(PickerBox), null);

        /// <summary>
        /// Gets or sets the header of the control.
        /// </summary>
        public object Header
        {
            get { return (object)GetValue(HeaderProperty); }
            set { SetValue(HeaderProperty, value); }
        }

        /// <summary>
        /// Identifies the Header DependencyProperty.
        /// </summary>
        public static readonly DependencyProperty HeaderProperty =
            DependencyProperty.Register("Header", typeof(object), typeof(PickerBox), null);

        /// <summary>
        /// Gets or sets the template used to display the control's header.
        /// </summary>
        public DataTemplate HeaderTemplate
        {
            get { return (DataTemplate)GetValue(HeaderTemplateProperty); }
            set { SetValue(HeaderTemplateProperty, value); }
        }

        /// <summary>
        /// Identifies the HeaderTemplate DependencyProperty.
        /// </summary>
        public static readonly DependencyProperty HeaderTemplateProperty =
            DependencyProperty.Register("HeaderTemplate", typeof(DataTemplate), typeof(PickerBox), null);

        /// <summary>
        /// Gets or sets the header to use when PickerBoxMode is set to Full.
        /// </summary>
        public object FullModeHeader
        {
            get { return (object)GetValue(FullModeHeaderProperty); }
            set { SetValue(FullModeHeaderProperty, value); }
        }

        /// <summary>
        /// Identifies the FullModeHeader DependencyProperty.
        /// </summary>
        public static readonly DependencyProperty FullModeHeaderProperty =
            DependencyProperty.Register("FullModeHeader", typeof(object), typeof(PickerBox), null);

        /// <summary>
        /// Gets or sets the Uri to use for loading the IPickerBoxPage instance when the control is tapped.
        /// </summary>
        public Uri PickerPageUri
        {
            get { return (Uri)GetValue(PickerPageUriProperty); }
            set { SetValue(PickerPageUriProperty, value); }
        }

        /// <summary>
        /// Identifies the PickerPageUri DependencyProperty.
        /// </summary>
        public static readonly DependencyProperty PickerPageUriProperty = DependencyProperty.Register(
            "PickerPageUri", typeof(Uri), typeof(PickerBox), null);

        /// <summary>
        /// Gets or sets the SelectionMode. Extended is treated as Multiple.
        /// Single by default.
        /// </summary>
        public SelectionMode SelectionMode
        {
            get { return (SelectionMode)GetValue(SelectionModeProperty); }
            set { SetValue(SelectionModeProperty, value); }
        }

        /// <summary>
        /// Identifies the SelectionMode DependencyProperty.
        /// </summary>
        public static readonly DependencyProperty SelectionModeProperty = DependencyProperty.Register(
            "SelectionMode",
            typeof(SelectionMode),
            typeof(PickerBox),
            new PropertyMetadata(SelectionMode.Single, OnSelectionModeChanged)
            );

        private static void OnSelectionModeChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            ((PickerBox)o).OnSelectionModeChanged((SelectionMode)e.NewValue);
        }

        private void OnSelectionModeChanged(SelectionMode newValue)
        {
            UpdateButtonContent();
        }

        /// <summary>
        /// Gets the selected items.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification="Want to allow this to be bound to.")]
        public IList SelectedItems
        {
            get { return (IList)GetValue(SelectedItemsProperty); }
            set { SetValue(SelectedItemsProperty, value); }
        }

        /// <summary>
        /// Identifies the SelectedItems DependencyProperty.
        /// </summary>
        public static readonly DependencyProperty SelectedItemsProperty = DependencyProperty.Register(
            "SelectedItems",
            typeof(IList),
            typeof(PickerBox),
            new PropertyMetadata(OnSelectedItemsChanged)
            );

        private static void OnSelectedItemsChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            ((PickerBox)o).OnSelectedItemsChanged((IList)e.OldValue, (IList)e.NewValue);
        }

        private void OnSelectedItemsChanged(IList oldValue, IList newValue)
        {
            UpdateSummary(newValue);

            // Fire SelectionChanged event
            var handler = SelectionChanged;
            if (null != handler)
            {
                IList removedItems = new List<object>();
                if (null != oldValue)
                {
                    foreach (object o in oldValue)
                    {
                        if (null == newValue || !newValue.Contains(o))
                        {
                            removedItems.Add(o);
                        }
                    }
                }
                IList addedItems = new List<object>();
                if (null != newValue)
                {
                    foreach (object o in newValue)
                    {
                        if (null == oldValue || !oldValue.Contains(o))
                        {
                            addedItems.Add(o);
                        }
                    }
                }

                handler(this, new SelectionChangedEventArgs(removedItems, addedItems));
            }
        }

        /// <summary>
        /// Initializes a new instance of the PickerBox class.
        /// </summary>
        public PickerBox()
        {
            DefaultStyleKey = typeof(PickerBox);

            IsEnabledChanged += OnIsEnabledChanged;

            Loaded += OnLoaded;

            _pickerPageHelper = new PickerPageHelper<IPickerBoxPage>(this, OnPickerPageOpening, OnPickerPageClosed, ClosePickerPage);
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            UpdateVisualStates (true);
        }

        /// <summary>
        /// Builds the visual tree for the control when a new template is applied.
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            // Unhook from old template
            if (null != _buttonPart)
            {
                _buttonPart.Click -= OnButtonClick;
            }

            base.OnApplyTemplate();

            // Hook up to new template
            _buttonPart = GetTemplateChild(ButtonPartName) as ButtonBase;
            if (null != _buttonPart)
            {
                _buttonPart.Click += OnButtonClick;
            }

            // Commit deferred SelectedIndex (if any)
            if (-1 != _deferredSelectedIndex)
            {
                SelectedIndex = _deferredSelectedIndex;
                _deferredSelectedIndex = -1;
            }
            if (null != _deferredSelectedItem)
            {
                SelectedItem = _deferredSelectedItem;
                _deferredSelectedItem = null;
            }

            OnSelectionModeChanged(SelectionMode);
            OnSelectedItemsChanged(SelectedItems, SelectedItems);
        }

        /// <summary>
        /// Provides handling for the ItemContainerGenerator.ItemsChanged event.
        /// </summary>
        /// <param name="e">A NotifyCollectionChangedEventArgs that contains the event data.</param>
        protected override void OnItemsChanged(NotifyCollectionChangedEventArgs e)
        {
            base.OnItemsChanged(e);

            if ((0 < Items.Count) && (null == SelectedItem))
            {
                // Nothing selected (and no pending Binding); select the first item
                if ((null == GetBindingExpression(SelectedIndexProperty)) &&
                    (null == GetBindingExpression(SelectedItemProperty)))
                {
                    SelectedIndex = 0;
                }
            }
            else if (0 == Items.Count)
            {
                // No items; select nothing
                SelectedIndex = -1;
                PickerBoxMode = PickerBoxMode.Normal;
            }
            else if (Items.Count <= SelectedIndex)
            {
                // Selected item no longer present; select the last item
                SelectedIndex = Items.Count - 1;
            }
            else
            {
                // Re-synchronize SelectedIndex with SelectedItem if necessary
                if (!object.Equals(Items[SelectedIndex], SelectedItem))
                {
                    int selectedItemIndex = Items.IndexOf(SelectedItem);
                    if (-1 == selectedItemIndex)
                    {
                        SelectedItem = Items[0];
                    }
                    else
                    {
                        SelectedIndex = selectedItemIndex;
                    }
                }
            }
        }

        /// <summary>
        /// Opens the picker for selection into Full mode.
        /// </summary>
        /// <returns>Whether the picker was succesfully opened.</returns>
        public bool Open()
        {
            // On interaction, switch to Full mode
            if ((PickerBoxMode.Normal == PickerBoxMode))
            {
                PickerBoxMode = PickerBoxMode.Full;
                return true;
            }

            return false;
        }

        private void UpdateVisualStates(bool useTransitions)
        {
            if (!IsEnabled)
            {
                VisualStateManager.GoToState(this, PickerStatesDisabledStateName, useTransitions);
            }
            else
            {
                VisualStateManager.GoToState(this, PickerStatesNormalStateName, useTransitions);
            }
        }

        /// <summary>
        /// Updates the summary of the selected items to be displayed in the PickerBox.
        /// </summary>
        /// <param name="newValue">The list selected items</param>
        private void UpdateSummary(IList newValue)
        {
            const string space = " ";
            string summary = null;

            if (null != SummaryForSelectedItemsDelegate)
            {
                // Ask the delegate to sumarize the selected items.
                summary = SummaryForSelectedItemsDelegate(newValue);
            }

            if (summary == null)
            {
                // No summary was provided, so by default, show only the first item in the selection list.
                if (null == newValue || newValue.Count == 0)
                {
                    // In the case that there were no selected items, show the empty string.
                    summary = space;
                }
                else
                {
                    summary = newValue[0].ToString();
                }
            }

            // The display does not size correctly if the empty string is used.
            if (String.IsNullOrEmpty(summary))
            {
                summary = space;
            }

            if (SelectionMode != SelectionMode.Single)
            {
                if (null != _buttonPart)
                {
                    _buttonPart.Content = summary;
                }
            }
        }

        private void OnPickerPageOpening(IPickerBoxPage pickerPage)
        {
            // Sets the flow direction.
            pickerPage.FlowDirection = this.FlowDirection;

            // Set up the list picker page with the necesarry fields.
            if (null != FullModeHeader)
            {
                pickerPage.HeaderText = (string)FullModeHeader;
            }
            else
            {
                pickerPage.HeaderText = (string)Header;
            }

            pickerPage.FullModeItemTemplate = FullModeItemTemplate;

            pickerPage.Items.Clear();
            if (null != Items)
            {
                foreach (var element in Items)
                {
                    pickerPage.Items.Add(element);
                }
            }

            pickerPage.SelectionMode = SelectionMode;

            if (SelectionMode == SelectionMode.Single)
            {
                pickerPage.SelectedItem = SelectedItem;
            }
            else
            {
                pickerPage.SelectedItems.Clear();
                if (null != SelectedItems)
                {
                    foreach (var element in SelectedItems)
                    {
                        pickerPage.SelectedItems.Add(element);
                    }
                }
            }
        }

        private void ClosePickerPage()
        {
            PickerBoxMode = PickerBoxMode.Normal;
        }

        private void OnPickerPageClosed(IPickerBoxPage pickerPage)
        {
            if (SelectionMode == SelectionMode.Single && null != pickerPage.SelectedItem)
            {
                SelectedItem = pickerPage.SelectedItem;
            }
            else if ((SelectionMode == SelectionMode.Multiple || SelectionMode == SelectionMode.Extended) && null != pickerPage.SelectedItems)
            {
                SelectedItems = pickerPage.SelectedItems;
            }
        }

        private void OnButtonClick(object sender, RoutedEventArgs e)
        {
            Open();
        }

        private void UpdateButtonContent()
        {
            if (null != _buttonPart)
            {
                if (SelectionMode == SelectionMode.Single)
                {
                    _buttonPart.Content = SelectedItem;
                }
                else
                {
                    UpdateSummary(SelectedItems);
                }
            }
        }
    }
}
