// ---------------------------------------------------------------------------
// <copyright file="MapsViewModel.cs" company="Microsoft">
//     (c) Copyright Microsoft Corporation.
//     This source is subject to the Microsoft Public License (Ms-PL).
//     Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
//     All other rights reserved.
// </copyright>
// ---------------------------------------------------------------------------

namespace PhoneToolkitSample.Data.Maps
{
    /// <summary>
    /// Maps Main View Model used in the map sample page
    /// </summary>
    public class MapsViewModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MapsViewModel"/> class
        /// </summary>
        public MapsViewModel()
        {
            this.StoreList = new StoreList();
        }

        /// <summary>
        /// Gets or sets the list of stores
        /// </summary>
        public StoreList StoreList { get; set; }
    }
}
