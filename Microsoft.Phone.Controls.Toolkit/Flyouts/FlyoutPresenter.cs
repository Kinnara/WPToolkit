using System;
using System.Windows;
using System.Windows.Controls;

namespace Microsoft.Phone.Controls
{
    /// <summary>
    /// Display the content of a Flyout.
    /// </summary>
    [TemplateVisualState(GroupName = GroupOpen, Name = StateClosed)]
    [TemplateVisualState(GroupName = GroupOpen, Name = StateOpen)]
    public class FlyoutPresenter : ContentControl
    {
        private const string GroupOpen = "OpenStates";
        private const string StateClosed = "Closed";
        private const string StateOpen = "Open";

        private bool _isOpening;
        private OrientationHelper _orientationHelper;

        /// <summary>
        /// Initializes a new instance of the FlyoutPresenter class.
        /// </summary>
        public FlyoutPresenter()
        {
            DefaultStyleKey = typeof(FlyoutPresenter);

            _orientationHelper = new OrientationHelper(this);

            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
        }

        private VisualStateGroup OpenStates { get; set; }

        internal event EventHandler Closed;

        /// <summary>
        /// Builds the visual tree for the control when a new template is applied.
        /// </summary>
        public override void OnApplyTemplate()
        {
            if (OpenStates != null)
            {
                OpenStates.CurrentStateChanged -= OnOpenStatesCurrentStateChanged;
            }

            base.OnApplyTemplate();

            OpenStates = VisualStates.TryGetVisualStateGroup(this, GroupOpen);

            if (OpenStates != null)
            {
                OpenStates.CurrentStateChanged += OnOpenStatesCurrentStateChanged;
            }

            Hide(false);

            _orientationHelper.OnApplyTemplate();
        }

        internal bool Hide(bool useTransitions)
        {
            _isOpening = false;
            return VisualStateManager.GoToState(this, StateClosed, useTransitions);
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            _isOpening = true;
            AnimationHelper.InvokeOnSecondRendering(() =>
                {
                    if (_isOpening)
                    {
                        _isOpening = false;
                        VisualStateManager.GoToState(this, StateOpen, true);
                    }
                });
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            Hide(false);
        }

        private void OnOpenStatesCurrentStateChanged(object sender, VisualStateChangedEventArgs e)
        {
            if (e.OldState != null && e.OldState.Name == StateOpen && e.NewState != null && e.NewState.Name == StateClosed)
            {
                SafeRaise.Raise(Closed, this);
            }
        }
    }
}
