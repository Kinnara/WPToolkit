// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

namespace Microsoft.Phone.Controls
{
    /// <summary>
    /// Displays the list of items and allows single or multiple selection.
    /// </summary>
    public partial class PickerBoxPage : PickerBoxPageBase
    {
        /// <summary>
        /// Creates a picker box page.
        /// </summary>
        public PickerBoxPage()
        {
            InitializeComponent();

            InitializePickerBoxPage(Picker, HeaderTitle);
        }
    }
}