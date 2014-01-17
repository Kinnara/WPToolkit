// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.Phone.Controls.LocalizedResources;

namespace Microsoft.Phone.Controls
{

    /// <summary>
    /// This control displays the progress and status of a download or upload
    /// </summary>
    [TemplateVisualState(Name = DefaultState, GroupName = ControlStates)]
    [TemplateVisualState(Name = NoProgressBarState, GroupName = ControlStates)]
    [TemplateVisualState(Name = HiddenState, GroupName = ControlStates)]
    [TemplateVisualState(Name = NoTransferState, GroupName = ControlStates)]
    [TemplateVisualState(Name = IconVisibleState, GroupName = IconStates)]
    [TemplateVisualState(Name = IconCollapsedState, GroupName = IconStates)]
    [TemplatePart(Name = ControlPart, Type = typeof(TransferControl))]
    public class TransferControl : Control
    {
        private const string ControlPart = "TransferControl";


        #region visual states

        private const bool UseTransitions = false;

        private const string ControlStates = "ControlStates";
        private const string DefaultState = "Default";
        private const string NoProgressBarState = "NoProgressBar";
        private const string HiddenState = "Hidden";
        private const string NoTransferState = "NoTransfer";

        private const string IconStates = "IconStates";
        private const string IconVisibleState = "IconVisible";
        private const string IconCollapsedState = "IconCollapsed";

        #endregion

        #region AutoHide property

        private static readonly DependencyProperty AutoHideProperty =
            DependencyProperty.Register("AutoHide", typeof(bool), typeof(TransferControl), new PropertyMetadata(false));

        /// <summary>
        /// If set to true, the control will collapse when the transfer completes or is cancelled. False by default.
        /// </summary>
        public bool AutoHide
        {
            get { return (bool)GetValue(AutoHideProperty); }
            set { SetValue(AutoHideProperty, value); }
        }

        #endregion

        #region Header property

        private static readonly DependencyProperty HeaderProperty =
            DependencyProperty.Register("Header", typeof(object), typeof(TransferControl), new PropertyMetadata(null));

        /// <summary>
        /// This controls what appears above the progress bar.
        /// </summary>
        public object Header
        {
            get { return (string)GetValue(HeaderProperty); }
            set { SetValue(HeaderProperty, value); }
        }

        #endregion

        #region HeaderTemplate property

        private static readonly DependencyProperty HeaderTemplateProperty =
            DependencyProperty.Register("HeaderTemplate", typeof(DataTemplate), typeof(TransferControl),
                                        new PropertyMetadata(default(DataTemplate)));

        /// <summary>
        /// This provides the template for the header
        /// </summary>
        public DataTemplate HeaderTemplate
        {
            get { return (DataTemplate)GetValue(HeaderTemplateProperty); }
            set { SetValue(HeaderTemplateProperty, value); }
        }

        #endregion

        #region Monitor property

        private static readonly DependencyProperty MonitorProperty =
            DependencyProperty.Register("Monitor", typeof(TransferMonitor), typeof(TransferControl),
                                        new PropertyMetadata(null, UpdateMonitor));

        /// <summary>
        /// This binds to a TransferMonitor which wraps around a background transfer
        /// </summary>
        public TransferMonitor Monitor
        {
            get { return (TransferMonitor)GetValue(MonitorProperty); }
            set { SetValue(MonitorProperty, value); }
        }

        #endregion

        #region Icon property


        private static readonly DependencyProperty IconProperty =
            DependencyProperty.Register("Icon", typeof(ImageSource), typeof(TransferControl),
                                        new PropertyMetadata((d, e) => ((TransferControl)d).UpdateVisualStates()));

        /// <summary>
        /// This icon can be set to any type of control and appears to the left of the progress bar
        /// </summary>
        public ImageSource Icon
        {
            get { return (ImageSource)GetValue(IconProperty); }
            set { SetValue(IconProperty, value); }
        }

        #endregion

        #region IsContextMenuEnabled property

        private static readonly DependencyProperty IsContextMenuEnabledProperty =
            DependencyProperty.Register("IsContextMenuEnabled", typeof(bool), typeof(TransferControl),
                                        new PropertyMetadata(true));

        /// <summary>
        /// Defines if the context menu is enabled
        /// </summary>
        public bool IsContextMenuEnabled
        {
            get { return (bool)GetValue(IsContextMenuEnabledProperty); }
            private set { SetValue(IsContextMenuEnabledProperty, value); }
        }

        #endregion

        #region ProgressBarStyle property

        private static readonly DependencyProperty ProgressBarStyleProperty =
            DependencyProperty.Register("ProgressBarStyle", typeof(Style), typeof(TransferControl),
                                        new PropertyMetadata(default(Style)));

        /// <summary>
        /// The style used for the progress bar
        /// </summary>
        public Style ProgressBarStyle
        {
            get { return (Style)GetValue(ProgressBarStyleProperty); }
            set { SetValue(ProgressBarStyleProperty, value); }
        }

        #endregion

        #region StatusTextBrush property

        private static readonly DependencyProperty StatusTextBrushProperty =
            DependencyProperty.Register("StatusTextBrush", typeof(Brush), typeof(TransferControl),
                                        new PropertyMetadata(Application.Current.Resources["PhoneForegroundBrush"]));

        /// <summary>
        /// The foreground brush used to the color the status text
        /// </summary>
        public Brush StatusTextBrush
        {
            get { return (Brush)GetValue(StatusTextBrushProperty); }
            set { SetValue(StatusTextBrushProperty, value); }
        }

        #endregion

        static TransferControl()
        {
            TiltEffect.TiltableItems.Add(typeof(TransferControl));
        }

        /// <summary>
        /// Initializes a new instance of the TransferControl class.
        /// </summary>
        public TransferControl()
        {
            DefaultStyleKey = typeof(TransferControl);
        }

        /// <summary>
        /// This function is called when the helper is set or updated. It passes the new helper into non-static context.
        /// </summary>
        private static void UpdateMonitor(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as TransferControl;
            if (control != null)
                control.UpdateMonitor(e);
        }

        /// <summary>
        /// This updates the helper by subscribing to events and assigning the filename.
        /// </summary>
        /// <param name="e"></param>
        private void UpdateMonitor(DependencyPropertyChangedEventArgs e)
        {
            var monitor = e.NewValue as TransferMonitor;
            if (monitor == null)
                return;

            monitor.PropertyChanged -= OnMonitorPropertyChanged;
            monitor.PropertyChanged += OnMonitorPropertyChanged;

            if (Header == null && monitor.Name != null)
            {
                Header = monitor.Name;
            }

            UpdateVisualStates();
        }

        private void OnMonitorPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "State")
            {
                UpdateVisualStates();
            }
        }

        private void UpdateVisualStates()
        {
            string newState;

            if (Monitor != null)
            {
                switch (Monitor.State)
                {
                    case TransferRequestState.Failed:
                        IsContextMenuEnabled = false;
                        newState = NoProgressBarState;
                        break;
                    case TransferRequestState.Pending:
                    case TransferRequestState.Waiting:
                        IsContextMenuEnabled = true;
                        newState = NoProgressBarState;
                        break;
                    case TransferRequestState.Uploading:
                    case TransferRequestState.Paused:
                    case TransferRequestState.Downloading:
                        IsContextMenuEnabled = true;
                        newState = DefaultState;
                        break;
                    case TransferRequestState.Unknown:
                    case TransferRequestState.Complete:
                        IsContextMenuEnabled = false;
                        newState = AutoHide ? HiddenState : DefaultState;
                        break;
                    default:
                        IsContextMenuEnabled = false;
                        newState = NoProgressBarState;
                        break;
                }
            }
            else
            {
                IsContextMenuEnabled = false;
                newState = NoTransferState;
            }

            VisualStateManager.GoToState(this, newState, UseTransitions);
            VisualStateManager.GoToState(this, Icon != null ? IconVisibleState : IconCollapsedState, UseTransitions);
        }

        /// <summary>
        /// Applies the context menu actions
        /// </summary>
        public override void OnApplyTemplate()
        {
            var cancelAction = GetTemplateChild("ContextMenuCancel") as MenuItem;
            if (cancelAction != null)
            {
                cancelAction.Click += (sender, args) => { if (this.Monitor != null) Monitor.RequestCancel(); };
                cancelAction.Header = ControlResources.Cancel;
            }
            base.OnApplyTemplate();
            UpdateVisualStates();
        }

    }
}