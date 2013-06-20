using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Controls;

namespace Microsoft.Phone.Controls.Primitives
{
    /// <summary>
    /// Provides a base class for item types in a selection control.
    /// </summary>
    [TemplateVisualState(GroupName = VisualStates.GroupCommon, Name = VisualStates.StateNormal)]
    [TemplateVisualState(GroupName = VisualStates.GroupCommon, Name = VisualStates.StateDisabled)]
    [TemplateVisualState(GroupName = VisualStates.GroupSelection, Name = VisualStates.StateUnselected)]
    [TemplateVisualState(GroupName = VisualStates.GroupSelection, Name = VisualStates.StateSelected)]
    public abstract class SimpleSelectorItem : ContentControl
    {
        internal SimpleSelector _parentSelector;

        /// <summary>
        /// Provides base class initialization behavior for
        /// <see cref="T:Microsoft.Phone.Controls.Primitives.SimpleSelectorItem" />-derived classes.
        /// </summary>
        protected SimpleSelectorItem()
        {
            Loaded += delegate { ChangeVisualState(false); };
            IsEnabledChanged += delegate { ChangeVisualState(); };
            DefaultStyleKey = typeof(SimpleSelectorItem);
        }

        /// <summary>
        /// Gets or sets a value that indicates whether the item is selected in a selector.
        /// </summary>
        /// 
        /// <returns>
        /// True if the item is selected; otherwise, false.
        /// </returns>
        public bool IsSelected
        {
            get { return (bool)GetValue(IsSelectedProperty); }
            set { SetValue(IsSelectedProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="P:Microsoft.Phone.Controls.Primitives.SimpleSelectorItem.IsSelected"/> dependency property.
        /// </summary>
        /// 
        /// <returns>
        /// The identifier for the IsSelected dependency property.
        /// </returns>
        public static readonly DependencyProperty IsSelectedProperty = DependencyProperty.RegisterAttached(
            "IsSelected", typeof(bool), typeof(SimpleSelectorItem), new PropertyMetadata(OnIsSelectedChanged));

        private static void OnIsSelectedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((SimpleSelectorItem)d).OnIsSelectedChanged((bool)e.OldValue, (bool)e.NewValue);
        }

        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "oldValue")]
        private void OnIsSelectedChanged(bool oldValue, bool newValue)
        {
            if (_parentSelector != null)
            {
                _parentSelector.NotifySelectorItemSelected(this, newValue);
            }

            ChangeVisualState();
        }

        internal object Item { get; set; }

        /// <summary>
        /// Builds the visual tree for the
        /// <see cref="T:Microsoft.Phone.Controls.Primitives.SimpleSelectorItem" /> control
        /// when a new template is applied.
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            ChangeVisualState(false);
        }

        /// <summary>
        /// Provides handling for the
        /// <see cref="E:System.Windows.UIElement.Tap" /> event.
        /// </summary>
        /// <param name="e">A <see cref="T:System.Windows.Input.GestureEventArgs" />
        /// that contains the event data.</param>
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Justification = "Standard pattern.")]
        protected override void OnTap(System.Windows.Input.GestureEventArgs e)
        {
            base.OnTap(e);

            if (_parentSelector != null)
            {
                e.Handled = _parentSelector.OnSelectorItemClicked(this);
            }
        }

        internal void ChangeVisualState()
        {
            ChangeVisualState(true);
        }

        internal void ChangeVisualState(bool useTransitions)
        {
            if (_parentSelector != null)
            {
                if (!IsEnabled)
                {
                    GoToState(useTransitions, Content is Control ? VisualStates.StateNormal : VisualStates.StateDisabled);
                }
                else
                {
                    GoToState(useTransitions, VisualStates.StateNormal);
                }

                if (IsSelected)
                {
                    GoToState(useTransitions, VisualStates.StateSelected);
                }
                else
                {
                    GoToState(useTransitions, VisualStates.StateUnselected);
                }
            }
        }

        private bool GoToState(bool useTransitions, string stateName)
        {
            return VisualStateManager.GoToState(this, stateName, useTransitions);
        }
    }
}