// ---------------------------------------------------------------------------
// <copyright file="MapChildCollection.cs" company="Microsoft">
//     (c) Copyright Microsoft Corporation.
//     This source is subject to the Microsoft Public License (Ms-PL).
//     Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
//     All other rights reserved.
// </copyright>
// ---------------------------------------------------------------------------

namespace Microsoft.Phone.Maps.Toolkit
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Device.Location;
    using System.Windows;

    /// <summary>
    /// Holds the list of items that represent the content of a <see cref="MapItemsControl"/>.
    /// </summary>
    public sealed class MapChildCollection : ObservableCollection<object>, IList, ICollection<object>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MapChildCollection"/> class
        /// </summary>
        public MapChildCollection()
        {
            this.IsInternalCall = false;
        }

        /// <summary>
        /// Gets a value indicating whether the IList is read-only. 
        /// </summary>
        bool IList.IsReadOnly
        {
            get { return this.IsReadOnly; }
        }

        /// <summary>
        /// Gets a value indicating whether the ICollection is read-only.
        /// </summary>
        bool ICollection<object>.IsReadOnly
        {
            get { return this.IsReadOnly; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the MapChildCollection is read-only. 
        /// </summary>
        internal bool IsReadOnly { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the next call should be treated as internal.
        /// Used to determine whether the operation/call is done from internal types.
        /// When done so, it will bypass the read only checks that are only used for client calls.
        /// </summary>
        private bool IsInternalCall { get; set; }
        
        /// <summary>
        /// Adds an object at the specified GeoCoordinate. 
        /// </summary>
        /// <param name="dependencyObject">The dependency object to add.</param>
        /// <param name="geoCoordinate">The geographic coordinate at which to add the dependency object.</param>
        public void Add(DependencyObject dependencyObject, GeoCoordinate geoCoordinate)
        {
            if (dependencyObject == null)
            {
                throw new ArgumentNullException("dependencyObject");
            }

            if (geoCoordinate == null)
            {
                throw new ArgumentNullException("geoCoordinate");
            }

            dependencyObject.SetValue(MapChild.GeoCoordinateProperty, geoCoordinate);
            this.Add(dependencyObject);
        }

        /// <summary>
        /// Adds an object at the specified GeoCoordinate and PositionOrigin 
        /// </summary>
        /// <param name="dependencyObject">The dependency object to add.</param>
        /// <param name="geoCoordinate">The geographic coordinate at which to add the dependency object.</param>
        /// <param name="positionOrigin">The position origin to use.</param>
        public void Add(DependencyObject dependencyObject, GeoCoordinate geoCoordinate, Point positionOrigin)
        {
            if (dependencyObject == null)
            {
                throw new ArgumentNullException("dependencyObject");
            }

            if (geoCoordinate == null)
            {
                throw new ArgumentNullException("geoCoordinate");
            }

            dependencyObject.SetValue(MapChild.GeoCoordinateProperty, geoCoordinate);
            dependencyObject.SetValue(MapChild.PositionOriginProperty, positionOrigin);
            this.Add(dependencyObject);
        }

        /// <summary>
        /// Copy the elements from the source
        /// </summary>
        /// <param name="source">Source enumerable collection</param>
        internal void AddRange(IEnumerable source)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            foreach (object item in source)
            {
                this.Add(item);
            }
        }

        #region Internal calls with bypass for read only

        /// <summary>
        /// Add the item to the collection.
        /// Read only check will be bypassed
        /// </summary>
        /// <param name="item">Item to be added</param>
        internal void AddInternal(object item)
        {
            this.IsInternalCall = true;
            this.Add(item);
            this.IsInternalCall = false;
        }

        /// <summary>
        /// Insert the item at the specified index.
        /// Read only check will be bypassed
        /// </summary>
        /// <param name="index">Index at which item should be inserted</param>
        /// <param name="item">Item to be inserted</param>
        internal void InsertInternal(int index, object item)
        {
            this.IsInternalCall = true;
            base.InsertItem(index, item);
            this.IsInternalCall = false;
        }

        /// <summary>
        /// Move the item at the old index to the new index within the collection
        /// Read only check will be bypassed
        /// </summary>
        /// <param name="oldIndex">Old index of the item to be moved</param>
        /// <param name="newIndex">New index at which the item should be moved</param>
        internal void MoveInternal(int oldIndex, int newIndex)
        {
            this.IsInternalCall = true;
            base.MoveItem(oldIndex, newIndex);
            this.IsInternalCall = false;
        }

        /// <summary>
        /// Remove the item at the specific index
        /// Read only check will be bypassed
        /// </summary>
        /// <param name="index">Index of the element to be removed</param>
        internal void RemoveInternal(int index)
        {
            this.IsInternalCall = true;
            base.RemoveItem(index);
            this.IsInternalCall = false;
        }

        /// <summary>
        /// Clears the collection
        /// Read only check will be bypassed
        /// </summary>
        internal void ClearInternal()
        {
            this.IsInternalCall = true;
            base.ClearItems();
            this.IsInternalCall = false;
        }

        #endregion

        #region Overrides from ObservableCollection<object>

        /// <summary>
        /// Insert the item at the specified index.
        /// Read only check will be honored
        /// </summary>
        /// <param name="index">Index at which item should be inserted</param>
        /// <param name="item">Item to be inserted</param>
        protected override void InsertItem(int index, object item)
        {
            this.CheckCanWriteAndRaiseExceptionIfNecessary();

            base.InsertItem(index, item);
        }

        /// <summary>
        /// Move the item at the old index to the new index within the collection
        /// Read only check will be honored
        /// </summary>
        /// <param name="oldIndex">Old index of the item to be moved</param>
        /// <param name="newIndex">New index at which the item should be moved</param>
        protected override void MoveItem(int oldIndex, int newIndex)
        {
            this.CheckCanWriteAndRaiseExceptionIfNecessary();

            base.MoveItem(oldIndex, newIndex);
        }

        /// <summary>
        /// Remove the item at the specific index
        /// Read only check will be honored
        /// </summary>
        /// <param name="index">Index of the element to be removed</param>
        protected override void RemoveItem(int index)
        {
            this.CheckCanWriteAndRaiseExceptionIfNecessary();

            base.RemoveItem(index);
        }

        /// <summary>
        /// Set the item at the specified index
        /// Read only check will be honored
        /// </summary>
        /// <param name="index">Index of the element to be set</param>
        /// <param name="item">Item to be set</param>
        protected override void SetItem(int index, object item)
        {
            this.CheckCanWriteAndRaiseExceptionIfNecessary();

            base.SetItem(index, item);
        }

        /// <summary>
        /// Clears the collection.
        /// Read only check will be honored
        /// </summary>
        protected override void ClearItems()
        {
            this.CheckCanWriteAndRaiseExceptionIfNecessary();

            base.ClearItems();
        }

        #endregion

        /// <summary>
        /// Will check the internal state, including writeable property and whether 
        /// the call comes from an internal call. It will throw if necessary
        /// </summary>
        private void CheckCanWriteAndRaiseExceptionIfNecessary()
        {
            if (this.IsReadOnly && !this.IsInternalCall)
            {
                throw new InvalidOperationException("Collection is in non writeable mode");
            }
        }
    }
}
