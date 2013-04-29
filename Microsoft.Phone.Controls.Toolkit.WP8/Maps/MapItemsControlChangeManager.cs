// ---------------------------------------------------------------------------
// <copyright file="MapItemsControlChangeManager.cs" company="Microsoft">
//     (c) Copyright Microsoft Corporation.
//     This source is subject to the Microsoft Public License (Ms-PL).
//     Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
//     All other rights reserved.
// </copyright>
// ---------------------------------------------------------------------------

namespace Microsoft.Phone.Maps.Toolkit
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Diagnostics;
    using System.Windows;
    using Microsoft.Phone.Maps.Controls;

    /// <summary>
    /// Class MapItemsControlChangedManager.
    /// Works as a middle man between the internal ItemsControl in MapItemsControl and a MapLayer
    /// </summary>
    internal class MapItemsControlChangeManager : CollectionChangeListener<object>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MapItemsControlChangeManager"/> class
        /// </summary>
        /// <param name="sourceCollection">Source collection that will be managed</param>
        public MapItemsControlChangeManager(INotifyCollectionChanged sourceCollection)
        {
            if (sourceCollection == null)
            {
                throw new ArgumentNullException("sourceCollection");
            }

            this.ObjectToMapOverlayMapping = new Dictionary<object, MapOverlay>();

            // Start listening to events
            sourceCollection.CollectionChanged += this.CollectionChanged;
        }

        /// <summary>
        /// Gets or sets the ItemTemplate to be used when creating MapOverlays
        /// </summary>
        public DataTemplate ItemTemplate { get; set; }

        /// <summary>
        /// Gets or sets the MapLayer used to host all the MapOverlays created here
        /// </summary>
        public MapLayer MapLayer { get; set; }

        /// <summary>
        /// Gets or sets the dictionary that maps the objects to a MapOverlay
        /// </summary>
        private System.Collections.Generic.Dictionary<object, MapOverlay> ObjectToMapOverlayMapping { get; set; }

        /// <summary>
        /// Inserts the item at the specified index in the target collection
        /// </summary>
        /// <param name="index">Index at which object will be inserted</param>
        /// <param name="obj">Object to be inserted</param>
        protected override void InsertItemInternal(int index, object obj)
        {
            MapOverlay mapOverlay;

            if (this.ObjectToMapOverlayMapping.ContainsKey(obj))
            {
                throw new InvalidOperationException("Attempted to insert the same object twice");
            }

            mapOverlay = MapChild.CreateMapOverlay(obj, this.ItemTemplate);

            this.MapLayer.Insert(index, mapOverlay);
            this.ObjectToMapOverlayMapping.Add(obj, mapOverlay);
        }

        /// <summary>
        /// Remove the specified item from the target collection
        /// </summary>
        /// <param name="obj">Object to be removed</param>
        protected override void RemoveItemInternal(object obj)
        {
            bool mappingContainsObject;

            mappingContainsObject = this.ObjectToMapOverlayMapping.ContainsKey(obj);

            Debug.Assert(mappingContainsObject, "expected to have a mapping for the object");

            if (mappingContainsObject)
            {
                MapOverlay mapOverlay;

                mapOverlay = this.ObjectToMapOverlayMapping[obj];
                this.ObjectToMapOverlayMapping.Remove(obj);
                this.MapLayer.Remove(mapOverlay);

                MapChild.ClearMapOverlayBindings(mapOverlay);
            }
        }

        /// <summary>
        /// Resets the target collection and internal state
        /// </summary>
        protected override void ResetInternal()
        {
            foreach (MapOverlay mapOverlay in this.MapLayer)
            {
                MapChild.ClearMapOverlayBindings(mapOverlay);
            }

            this.MapLayer.Clear();
            this.ObjectToMapOverlayMapping.Clear();
        }

        /// <summary>
        /// Adds the object to the target collection
        /// </summary>
        /// <param name="obj">Object to be added</param>
        protected override void AddInternal(object obj)
        {
            MapOverlay mapOverlay;

            if (this.ObjectToMapOverlayMapping.ContainsKey(obj))
            {
                throw new InvalidOperationException("Attempted to insert the same object twice");
            }

            mapOverlay = MapChild.CreateMapOverlay(obj, this.ItemTemplate);

            this.ObjectToMapOverlayMapping[obj] = mapOverlay;
            this.MapLayer.Add(mapOverlay);
        }

        /// <summary>
        /// Moves the specified object from the old index to the new index
        /// </summary>
        /// <param name="obj">Object to be moved</param>
        /// <param name="newIndex">New index</param>
        protected override void MoveInternal(object obj, int newIndex)
        {
            bool mappingContainsObject;

            mappingContainsObject = this.ObjectToMapOverlayMapping.ContainsKey(obj);
            Debug.Assert(mappingContainsObject, "target object should be in the mapping table");

            if (mappingContainsObject)
            {
                this.MapLayer.Move(this.MapLayer.IndexOf(this.ObjectToMapOverlayMapping[obj]), newIndex);
            }  
        }
    }
}