// ---------------------------------------------------------------------------
// <copyright file="CollectionChangeListener.cs" company="Microsoft">
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
    using System.Collections.Specialized;
    using System.Diagnostics;
    using System.Windows.Controls.Primitives;

    /// <summary>
    /// CollectionChangedListener class.
    /// Generic template that can used as a middle man of events from a collection to another collection.
    /// It has a CollectionChanged event handler that will forward the calls to concrete implementations
    /// which can in turn apply them into a target collection.
    /// </summary>
    /// <typeparam name="T">Type of element in the collection that implements the INotifyCollectionChanged</typeparam>
    internal abstract class CollectionChangeListener<T>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CollectionChangeListener&lt;T&gt;"/> class
        /// </summary>
        protected CollectionChangeListener()
        {
        }

        /// <summary>
        /// CollectionChanged handler.
        /// Will forward the events to concrete implementations.
        /// </summary>
        /// <param name="sender">the sender value</param>
        /// <param name="e">the e value</param>
        protected void CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e == null)
            {
                throw new ArgumentNullException("e", "NotifyCollectionChangedEventArgs");
            }

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    for (int i = e.NewItems.Count - 1; i >= 0; i--)
                    {
                        this.InsertItemInternal(e.NewStartingIndex, (T)e.NewItems[i]);
                    }

                    break;

                case NotifyCollectionChangedAction.Remove:
                    foreach (object item in e.OldItems)
                    {
                        this.RemoveItemInternal((T)item);
                    }

                    break;

                case NotifyCollectionChangedAction.Replace:
                    foreach (object item in e.OldItems)
                    {
                        this.RemoveItemInternal((T)item);
                    }

                    for (int i = e.NewItems.Count - 1; i >= 0; i--)
                    {
                        this.InsertItemInternal(e.NewStartingIndex, (T)e.NewItems[i]);
                    }

                    break;

                case NotifyCollectionChangedAction.Reset:
                    // e.g., Clear() or the list is resorted.
                    // We need to walk over everything manually.
                    // The list has changed drastically.
                    this.ResetInternal();
                    this.AddRangeInternal((IEnumerable<T>)sender);
                    break;

                case NotifyCollectionChangedAction.Move:
                    Debug.Assert(e.OldItems.Count == 1, "Expecting only one item in the old items collection");
                    Debug.Assert(e.NewItems.Count == 1, "Expecting only one item in the new items collection");

                    this.MoveInternal((T)e.OldItems[0], e.NewStartingIndex);

                    break;

                default:
                    Debug.Assert(false, "Did we miss a new event?");
                    break;
            }
        }

        /// <summary>
        /// Adds Range to the target collection
        /// </summary>
        /// <param name="items">the collection items</param>
        protected void AddRangeInternal(IEnumerable<T> items)
        {
            if (items == null)
            {
                throw new ArgumentNullException("items");
            }

            foreach (T obj in items)
            {
                this.AddInternal(obj);
            }
        }

        /// <summary>
        /// Concrete implementations will have a chance to insert the object at the specified index into a target collection
        /// </summary>
        /// <param name="index">index where the object was inserted</param>
        /// <param name="obj">object to add</param>
        protected abstract void InsertItemInternal(int index, T obj);

        /// <summary>
        /// Concrete implementations will have a chance to remove the object from a target collection
        /// </summary>
        /// <param name="obj">object to remove</param>
        protected abstract void RemoveItemInternal(T obj);

        /// <summary>
        /// Concrete implementations will have a chance to reset the contents target collection
        /// </summary>
        protected abstract void ResetInternal();

        /// <summary>
        /// Concrete implementations will have a chance to add the item into a target collection
        /// </summary>
        /// <param name="obj">object to add</param>
        protected abstract void AddInternal(T obj);

        /// <summary>
        /// Concrete implementations will have a chance to move the end object from the old index to the new index
        /// </summary>
        /// <param name="obj">object to move</param>
        /// <param name="newIndex">new index</param>
        protected abstract void MoveInternal(T obj, int newIndex);
    }
}
