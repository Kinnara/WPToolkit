// ---------------------------------------------------------------------------
// <copyright file="UserLocationMarker.cs" company="Microsoft">
//     (c) Copyright Microsoft Corporation.
//     This source is subject to the Microsoft Public License (Ms-PL).
//     Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
//     All other rights reserved.
// </copyright>
// ---------------------------------------------------------------------------

namespace Microsoft.Phone.Maps.Toolkit
{
    using System.Windows;
    using System.Windows.Markup;
    using System.Windows.Media;

    /// <summary>
    /// Represents a marker for the location of a user on the map.
    /// </summary>
    [ContentProperty("Content")]
    public sealed class UserLocationMarker : MapChildControl
    {
        #region Public Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="UserLocationMarker"/> class.
        /// </summary>
        public UserLocationMarker()
        {
            this.DefaultStyleKey = typeof(UserLocationMarker);
        }

        #endregion
    }
}
