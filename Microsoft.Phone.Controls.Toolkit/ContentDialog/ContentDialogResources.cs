using System.Windows;
using System.Windows.Media;

namespace Microsoft.Phone.Controls
{
    /// <summary>
    /// Provides access to the theme resources used by the ContentDialog.
    /// </summary>
    public sealed class ContentDialogResources
    {
        private SolidColorBrush _contentDialogDimmingThemeBrush;

        /// <summary>
        /// Gets the ContentDialogDimmingThemeBrush.
        /// </summary>
        public SolidColorBrush ContentDialogDimmingThemeBrush
        {
            get
            {
                if (_contentDialogDimmingThemeBrush == null)
                {
                    SolidColorBrush phoneBackgroundBrush = (SolidColorBrush)Application.Current.Resources["PhoneBackgroundBrush"];
                    Color backgroundColor = phoneBackgroundBrush.Color;
                    backgroundColor.A = 0x99;
                    _contentDialogDimmingThemeBrush = new SolidColorBrush(backgroundColor);
                }

                return _contentDialogDimmingThemeBrush;
            }
        }
    }
}
