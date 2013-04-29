// ---------------------------------------------------------------------------
// <copyright file="MapItemsSourceChangeManager.cs" company="Microsoft">
//     (c) Copyright Microsoft Corporation.
//     This source is subject to the Microsoft Public License (Ms-PL).
//     Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
//     All other rights reserved.
// </copyright>
// ---------------------------------------------------------------------------

namespace Microsoft.Phone.Maps.Toolkit
{
    using System.Collections.Specialized;

    /// <summary>
    /// Class MapItemsSourceChangeManager.
    /// Concrete implementation of <see cref="MapItemsSourceChangeManager"/> that will listen
    /// to change events in the source collection events to the the target Items collection
    /// </summary>
    internal class MapItemsSourceChangeManager : CollectionChangeListener<object>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MapItemsSourceChangeManager"/> class
        /// </summary>
        /// <param name="sourceCollection">Source collection</param>
        public MapItemsSourceChangeManager(INotifyCollectionChanged sourceCollection)
        {
            this.SourceCollection = sourceCollection;
            this.SourceCollection.CollectionChanged += this.CollectionChanged;
        }

        /// <summary>
        /// Gets or sets the target Items Collection of a <see cref="MapItemsControl"/>
        /// </summary>
        public MapChildCollection Items { get; set; }

        /// <summary>
        /// Gets or sets the source collection
        /// </summary>
        private INotifyCollectionChanged SourceCollection { get; set; }

        /// <summary>
        /// Disconnects this manager from the source. No more events will be processed
        /// </summary>
        public void Disconnect()
        {
            this.SourceCollection.CollectionChanged -= this.CollectionChanged;
            this.SourceCollection = null;
        }

        /// <summary>
        /// Inserts the object at the index in the target collection
        /// </summary>
        /// <param name="index">The index at which the object will be inserted</param>
        /// <param name="obj">Object to be inserted</param>
        protected override void InsertItemInternal(int index, object obj)
        {
            this.Items.InsertInternal(index, obj);
        }

        /// <summary>
        /// Removes the object from the target collection
        /// </summary>
        /// <param name="obj">Object to be removed</param>
        protected override void RemoveItemInternal(object obj)
        {
            this.Items.RemoveInternal(this.Items.IndexOf(obj));
        }

        /// <summary>
        /// Clears the target collection
        /// </summary>
        protected override void ResetInternal()
        {
            this.Items.ClearInternal();
        }

        /// <summary>
        /// Adds the object to the target collection
        /// </summary>
        /// <param name="obj">Object to be added</param>
        protected override void AddInternal(object obj)
        {
            this.Items.AddInternal(obj);
        }

        /// <summary>
        /// Moves the item to the new index within the target collection
        /// </summary>
        /// <param name="obj">Object to be moved</param>
        /// <param name="newIndex">New index</param>
        protected override void MoveInternal(object obj, int newIndex)
        {
            this.Items.MoveInternal(this.Items.IndexOf(obj), newIndex);
        }
    }
}
