// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using Microsoft.Phone.Controls.Primitives;

namespace Microsoft.Phone.Controls
{
    /// <summary>
    /// Class that implements a container for the ListPicker control.
    /// </summary>
    /// <QualityBand>Preview</QualityBand>
    public class ListPickerItem : SimpleSelectorItem
    {
        /// <summary>
        /// Initializes a new instance of the ListPickerItem class.
        /// </summary>
        public ListPickerItem()
        {
            DefaultStyleKey = typeof(ListPickerItem);

            TiltEffect.SetSuppressTilt(this, false);
        }
    }
}
