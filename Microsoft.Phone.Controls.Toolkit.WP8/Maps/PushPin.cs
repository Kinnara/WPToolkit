// ---------------------------------------------------------------------------
// <copyright file="Pushpin.cs" company="Microsoft">
//     (c) Copyright Microsoft Corporation.
//     This source is subject to the Microsoft Public License (Ms-PL).
//     Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
//     All other rights reserved.
// </copyright>
// ---------------------------------------------------------------------------

namespace Microsoft.Phone.Maps.Toolkit
{
    using System.Windows.Markup;

    /// <summary>
    /// Represents a pushpin on the map.
    /// </summary>
    [ContentProperty("Content")]
    public sealed class Pushpin : MapChildControl
    {
        #region Public Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="Pushpin"/> class.
        /// </summary>
        public Pushpin()
        {
            this.DefaultStyleKey = typeof(Pushpin);
        }

        #endregion
    }
}
