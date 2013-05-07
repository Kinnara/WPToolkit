using Microsoft.Phone.Shell;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;

namespace Microsoft.Phone.Controls
{
    /// <summary>
    /// Provides methods and properties for interacting with the wait cursor on an application page.
    /// </summary>
    public class WaitCursor : DependencyObject
    {
        private PhoneApplicationPage _owner;
        private IApplicationBar _storedApplicationBar;
        private bool _storedApplicationBarIsVisible;

        #region public bool IsVisible

        /// <summary>
        /// Gets or sets the visibility of wait cursor on the current application page.
        /// </summary>
        /// 
        /// <returns>
        /// true if the wait cursor is visible; otherwise, false.
        /// </returns>
        public bool IsVisible
        {
            get { return (bool)GetValue(IsVisibleProperty); }
            set { SetValue(IsVisibleProperty, value); }
        }

        /// <summary>
        /// The dependency property for <see cref="P:Microsoft.Phone.Controls.WaitCursor.IsVisible"/>.
        /// </summary>
        public static readonly DependencyProperty IsVisibleProperty = DependencyProperty.Register(
            "IsVisible",
            typeof(bool),
            typeof(WaitCursor),
            new PropertyMetadata(OnIsVisibleChanged));

        private static void OnIsVisibleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((WaitCursor)d).OnIsVisibleChanged(e);
        }

        private void OnIsVisibleChanged(DependencyPropertyChangedEventArgs e)
        {
            if (DesignerProperties.IsInDesignTool)
            {
                return;
            }

            Apply();
        }

        #endregion

        #region public string Text

        /// <summary>
        /// Gets or sets the text of the wait cursor on the current application page.
        /// </summary>
        /// 
        /// <returns>
        /// The text of the wait cursor.
        /// </returns>
        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        /// <summary>
        /// The dependency property for <see cref="P:Microsoft.Phone.Controls.WaitCursor.Text"/>.
        /// </summary>
        public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
            "Text",
            typeof(string),
            typeof(WaitCursor),
            new PropertyMetadata(string.Empty, OnPropertyChanged));

        #endregion

        #region public Brush Background

        /// <summary>
        /// Gets or sets the background brush of the wait cursor.
        /// </summary>
        /// 
        /// <returns>
        /// The background brush of the wait cursor.
        /// </returns>
        public Brush Background
        {
            get { return (Brush)GetValue(BackgroundProperty); }
            set { SetValue(BackgroundProperty, value); }
        }

        /// <summary>
        /// The dependency property for <see cref="P:Microsoft.Phone.Controls.WaitCursor.Background"/>.
        /// </summary>
        public static readonly DependencyProperty BackgroundProperty = DependencyProperty.Register(
            "Background",
            typeof(Brush),
            typeof(WaitCursor),
            new PropertyMetadata(OnPropertyChanged));

        #endregion

        #region public Brush Foreground

        /// <summary>
        /// Gets or sets the foreground brush of the wait cursor.
        /// </summary>
        /// 
        /// <returns>
        /// The foreground brush of the wait cursor.
        /// </returns>
        public Brush Foreground
        {
            get { return (Brush)GetValue(ForegroundProperty); }
            set { SetValue(ForegroundProperty, value); }
        }

        /// <summary>
        /// The dependency property for <see cref="P:Microsoft.Phone.Controls.WaitCursor.Foreground"/>.
        /// </summary>
        public static readonly DependencyProperty ForegroundProperty = DependencyProperty.Register(
            "Foreground",
            typeof(Brush),
            typeof(WaitCursor),
            new PropertyMetadata(OnPropertyChanged));

        #endregion

        internal PhoneApplicationPage Owner
        {
            get { return _owner; }
            set
            {
                if (_owner != value)
                {
                    if (_owner != null)
                    {
                        _owner = null;
                        UpdateOwner();
                    }

                    _owner = value;

                    if (_owner != null)
                    {
                        Apply();
                    }
                }
            }
        }

        private bool IsActive
        {
            get { return Owner != null && Owner == WaitCursorService.GetActivePage(); }
        }

        internal void UpdateOwner()
        {
            if (IsVisible)
            {
                if (_storedApplicationBar == null && Owner != null)
                {
                    _storedApplicationBar = Owner.ApplicationBar;
                    if (_storedApplicationBar != null)
                    {
                        _storedApplicationBarIsVisible = _storedApplicationBar.IsVisible;
                        _storedApplicationBar.IsVisible = false;
                    }
                }
            }
            else
            {
                if (_storedApplicationBar != null)
                {
                    _storedApplicationBar.IsVisible = _storedApplicationBarIsVisible;
                    _storedApplicationBar = null;
                }
            }
        }

        private void Apply()
        {
            if (IsActive)
            {
                if (IsVisible)
                {
                    if (WaitCursorService.Frame != null)
                    {
                        WaitCursorService.Frame.Focus();
                    }
                }

                WaitCursorService.UpdateControl();
            }

            UpdateOwner();
        }

        private static void OnPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (DesignerProperties.IsInDesignTool)
            {
                return;
            }

            if (((WaitCursor)d).IsActive)
            {
                WaitCursorService.UpdateControl();
            }
        }
    }
}
