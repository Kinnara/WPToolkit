// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System.Collections;
using System.Windows;
using System.Windows.Controls;

namespace Microsoft.Phone.Controls.Primitives
{
    /// <summary>
    /// Represents an interface for PickerBox to use for communicating with a picker page.
    /// </summary>
    public interface IPickerBoxPage
    {
        /// <summary>
        /// Gets or sets the string of text to display as the header of the page.
        /// </summary>
        string HeaderText { get; set; }

        /// <summary>
        /// Gets the list of items to display.
        /// </summary>
        IList Items { get; }

        /// <summary>
        /// Gets or sets the selection mode.
        /// </summary>
        SelectionMode SelectionMode { get; set; }

        /// <summary>
        /// Gets or sets the selected item.
        /// </summary>
        object SelectedItem { get; set; }

        /// <summary>
        /// Gets the list of items to select.
        /// </summary>
        IList SelectedItems { get; }

        /// <summary>
        /// Gets or sets the item template
        /// </summary>
        DataTemplate ItemTemplate { get; set; }

        /// <summary>
        /// Gets or sets the item container style
        /// </summary>
        Style ItemContainerStyle { get; set; }

        /// <summary>
        /// Gets or sets the name or path of the property that is displayed for each data item.
        /// </summary>
        string DisplayMemberPath { get; set; }

        /// <summary>
        /// Gets or sets the direction that text and other user interface elements flow
        /// within any parent element that controls their layout.
        /// </summary>
        FlowDirection FlowDirection { get; set; }
    }
}
