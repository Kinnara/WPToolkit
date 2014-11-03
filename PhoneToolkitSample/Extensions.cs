// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System;
using System.Collections.Generic;

namespace PhoneToolkitSample
{
    public static class Extensions
    {
        /// <summary>
        /// Return a random item from a list.
        /// </summary>
        /// <typeparam name="T">The item type.</typeparam>
        /// <param name="rnd">The Random instance.</param>
        /// <param name="list">The list to choose from.</param>
        /// <returns>A randomly selected item from the list.</returns>
        public static T Next<T>(this Random rnd, IList<T> list)
        {
            return list[rnd.Next(list.Count)];
        }
    }
}