// Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;

namespace Microsoft.Phone.Controls
{
    /// <summary>
    /// Class for LongListMultiSelector items
    /// </summary>
    [TemplateVisualState(Name = OpenedStateName, GroupName = HasSelectionStatesesName)]
    [TemplateVisualState(Name = ClosedStateName, GroupName = HasSelectionStatesesName)]
    [TemplateVisualState(Name = ExposedStateName, GroupName = ManipulationStatesName)]
    [TemplateVisualState(Name = SelectedStateName, GroupName = SelectionStatesName)]
    [TemplateVisualState(Name = UnselectedStateName, GroupName = SelectionStatesName)]
    [TemplatePart(Name = ContentContainerName, Type = typeof(ContentControl))]
    [TemplatePart(Name = OuterHintPanelName, Type = typeof(Rectangle))]
    [TemplatePart(Name = InnerHintPanelName, Type = typeof(Rectangle))]
    [TemplatePart(Name = OuterCoverName, Type = typeof(Grid))]
    [TemplatePart(Name = InfoPresenterName, Type = typeof(ContentControl))]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Multi")]
    public class LongListMultiSelectorItem : ContentControl
    {
        private const string HasSelectionStatesesName = "HasSelectionStates";
        private const string OpenedStateName = "Opened";
        private const string ClosedStateName = "Closed";
        private const string ExposedStateName = "Exposed";
        private const string ManipulationStatesName = "ManipulationStates";
        private const string SelectionStatesName = "SelectionStates";
        private const string SelectedStateName = "Selected";
        private const string UnselectedStateName = "Unselected";

        internal enum State { Opened, Exposed, Closed, Selected, Unselected }

        private const string ContentContainerName = "ContentContainer";
        private const string OuterHintPanelName = "OuterHintPanel";
        private const string InnerHintPanelName = "InnerHintPanel";
        private const string OuterCoverName = "OuterCover";
        private const string InfoPresenterName = "InfoPresenter";

        /// <summary>
        /// Limit for the manipulation delta in the Y-axis.
        /// </summary>
        private const double _translationYLimit = 0.4;

        /// <summary>
        /// Outer Hint Panel template part.
        /// </summary>
        Rectangle _outerHintPanel = null;

        /// <summary>
        /// Inner Hint Panel template part.
        /// </summary>
        Rectangle _innerHintPanel = null;

        /// <summary>
        /// Outer Cover template part.
        /// </summary>
        Grid _outerCover = null;

        /// <summary>
        /// Indicator for SelectPanel manipulation : true if still inside acceptable limits and will trigger Selection.
        /// </summary>
        bool _insideAndDown = false;

        /// <summary>
        /// Flag used to restore state when the current style is changed
        /// </summary>
        bool _isOpened = false;

        /// <summary>
        /// Weak Reference used by the container
        /// </summary>
        private WeakReference<LongListMultiSelectorItem> _wr = null;

        /// <summary>
        /// Weak Reference used by the container
        /// </summary>
        internal WeakReference<LongListMultiSelectorItem> WR
        {
            get
            {
                if (_wr == null) _wr = new WeakReference<LongListMultiSelectorItem>(this);
                return _wr;
            }
        }



        #region Dependency Properties
        /// <summary>
        /// Gets or sets the content information.
        /// </summary>
        public object ContentInfo
        {
            get { return (object)GetValue(ContentInfoProperty); }
            set { SetValue(ContentInfoProperty, value); }
        }

        /// <summary>
        /// Identifies the ContentInfo dependency property.
        /// </summary>
        public static readonly DependencyProperty ContentInfoProperty =
            DependencyProperty.Register("ContentInfo", typeof(object), typeof(LongListMultiSelectorItem), new PropertyMetadata(null));

        /// <summary>
        /// Gets or sets the data template that defines
        /// the content information.
        /// </summary>
        public DataTemplate ContentInfoTemplate
        {
            get { return (DataTemplate)GetValue(ContentInfoTemplateProperty); }
            set { SetValue(ContentInfoTemplateProperty, value); }
        }

        /// <summary>
        /// Identifies the ContentInfoTemplate dependency property.
        /// </summary>
        public static readonly DependencyProperty ContentInfoTemplateProperty =
            DependencyProperty.Register("ContentInfoTemplate", typeof(DataTemplate), typeof(LongListMultiSelectorItem), new PropertyMetadata(null));

        /// <summary>
        /// Gets or sets the flag indicating that the item is selected
        /// </summary>
        public bool IsSelected
        {
            get { return (bool)GetValue(IsSelectedProperty); }
            set { SetValue(IsSelectedProperty, value); }
        }

        /// <summary>
        ///    Identifies the IsSelected dependency property.
        /// </summary>
        public static readonly DependencyProperty IsSelectedProperty =
            DependencyProperty.Register("IsSelected", typeof(bool), typeof(LongListMultiSelectorItem), new PropertyMetadata(false, OnIsSelectedPropertyChanged));

        /// <summary>
        /// Called then the property is changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        static void OnIsSelectedPropertyChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            LongListMultiSelectorItem This = sender as LongListMultiSelectorItem;
            if (This != null)
            {
                This.OnIsSelectedChanged();
            }
        }

        /// <summary>
        /// Gets or sets the height of the hint panel.
        /// </summary>
        public double HintPanelHeight
        {
            get { return (double)GetValue(HintPanelHeightProperty); }
            set { SetValue(HintPanelHeightProperty, value); }
        }

        /// <summary>
        /// Identifies the HintPanelHeight dependency property.
        /// </summary>
        public static readonly DependencyProperty HintPanelHeightProperty =
            DependencyProperty.Register("HintPanelHeight", typeof(double), typeof(LongListMultiSelectorItem), new PropertyMetadata(Double.NaN, OnHintPanelHeightPropertyChanged));

        /// <summary>
        /// Handles the change of the HintPanelHeight property
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void OnHintPanelHeightPropertyChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            LongListMultiSelectorItem This = sender as LongListMultiSelectorItem;
            if (This != null)
            {
                This.OnHintPanelHeightChanged();
            }
        }

        #endregion

        #region Events
        /// <summary>
        /// Triggered when the IsSelected property has changed
        /// </summary>
        public event EventHandler IsSelectedChanged;
        #endregion

        /// <summary>
        /// Constructor
        /// </summary>
        public LongListMultiSelectorItem()
        {
            this.DefaultStyleKey = typeof(LongListMultiSelectorItem);
        }


        /// <summary>
        /// Template application : hooks events which need to be handled
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            if (_outerHintPanel != null)
            {
                _outerHintPanel.ManipulationStarted -= OnSelectPanelManipulationStarted;
                _outerHintPanel.ManipulationDelta -= OnSelectPanelManipulationDelta;
                _outerHintPanel.ManipulationCompleted -= OnSelectPanelManipulationCompleted;
            }
            if (_innerHintPanel != null)
            {
                _innerHintPanel.ManipulationStarted -= OnSelectPanelManipulationStarted;
                _innerHintPanel.ManipulationDelta -= OnSelectPanelManipulationDelta;
                _innerHintPanel.ManipulationCompleted -= OnSelectPanelManipulationCompleted;
            }
            if (_outerCover != null)
            {
                _outerCover.Tap -= OnCoverTap;
            }

            _outerHintPanel = GetTemplateChild(OuterHintPanelName) as Rectangle;
            if (_outerHintPanel != null)
            {
                _outerHintPanel.ManipulationStarted += OnSelectPanelManipulationStarted;
                _outerHintPanel.ManipulationDelta += OnSelectPanelManipulationDelta;
                _outerHintPanel.ManipulationCompleted += OnSelectPanelManipulationCompleted;
            }
            _innerHintPanel = GetTemplateChild(InnerHintPanelName) as Rectangle;
            if (_innerHintPanel != null)
            {
                _innerHintPanel.ManipulationStarted += OnSelectPanelManipulationStarted;
                _innerHintPanel.ManipulationDelta += OnSelectPanelManipulationDelta;
                _innerHintPanel.ManipulationCompleted += OnSelectPanelManipulationCompleted;
            }
            _outerCover = GetTemplateChild(OuterCoverName) as Grid;
            if (_outerCover != null)
            {
                _outerCover.Tap += OnCoverTap;
            }

            OnHintPanelHeightChanged();
            GotoState(_isOpened ? State.Opened : State.Closed, false);
            GotoState(IsSelected ? State.Selected : State.Unselected, false);
        }

        /// <summary>
        /// Updates the visual state of the item
        /// </summary>
        protected virtual void OnIsSelectedChanged()
        {
            GotoState(IsSelected ? State.Selected : State.Unselected, true);
            if (IsSelectedChanged != null)
            {
                IsSelectedChanged(this, null);
            }
        }

        /// <summary>
        /// Tap on the cover grid : switch the selected state
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void OnCoverTap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            IsSelected = !IsSelected;
        }

        /// <summary>
        /// Triggers a visual transition to the Exposed visual state
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void OnSelectPanelManipulationStarted(object sender, System.Windows.Input.ManipulationStartedEventArgs e)
        {
            _insideAndDown = true;
            GotoState(State.Exposed, true);
        }

        /// <summary>
        /// Checks that the manipulation is still in correct bounds
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void OnSelectPanelManipulationDelta(object sender, System.Windows.Input.ManipulationDeltaEventArgs e)
        {
            if ((e.DeltaManipulation.Translation.X != 0.0) || (e.DeltaManipulation.Translation.Y <= -_translationYLimit) || (e.DeltaManipulation.Translation.Y >= _translationYLimit))
            {
                _insideAndDown = false;
                GotoState(State.Closed, true);
            }
        }

        /// <summary>
        /// End of the manipulation, select the item
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void OnSelectPanelManipulationCompleted(object sender, System.Windows.Input.ManipulationCompletedEventArgs e)
        {
            if (_insideAndDown)
            {
                _insideAndDown = false;
                IsSelected = true;
            }
        }

        /// <summary>
        /// Changes the visual state of the item
        /// </summary>
        /// <param name="state">New state</param>
        /// <param name="useTransitions">Indicates whether display or not transitions between states</param>
        internal void GotoState(State state, bool useTransitions = false)
        {
            string stateName;
            switch (state)
            {
                default:
                case State.Closed:
                    _isOpened = false;
                    stateName = ClosedStateName;
                    break;
                case State.Opened:
                    _isOpened = true;
                    stateName = OpenedStateName;
                    break;
                case State.Exposed:
                    stateName = ExposedStateName;
                    break;
                case State.Selected:
                    stateName = SelectedStateName;
                    break;
                case State.Unselected:
                    stateName = UnselectedStateName;
                    break;
            }
            VisualStateManager.GoToState(this, stateName, useTransitions);
        }

        /// <summary>
        /// Sets the vertical alignment of the hint panels to stretch if the
        /// height is not manually set. If it is, the alignment is set to top.
        /// </summary>
        protected virtual void OnHintPanelHeightChanged()
        {
            if (_outerHintPanel != null)
            {
                _outerHintPanel.VerticalAlignment = ((double.IsNaN(HintPanelHeight) ? VerticalAlignment.Stretch : VerticalAlignment.Top));
            }
            if (_innerHintPanel != null)
            {
                _innerHintPanel.VerticalAlignment = ((double.IsNaN(HintPanelHeight) ? VerticalAlignment.Stretch : VerticalAlignment.Top));
            }
        }

        /// <summary>
        /// Called when content is changed. This is a good place to get the style (which depends on the LLMS layout)
        /// because the control template has not yet been expanded.
        /// </summary>
        /// <param name="oldContent"></param>
        /// <param name="newContent"></param>
        protected override void OnContentChanged(object oldContent, object newContent)
        {
            // Finds the LongListMultiSelector to which the LongListMultiSelectorItem belongs
            LongListMultiSelector llms = this.GetParentByType<LongListMultiSelector>();
            if (llms != null)
            {
                llms.ConfigureItem(this);
            }
            base.OnContentChanged(oldContent, newContent);
        }
    }
}
