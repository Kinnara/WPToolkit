// ---------------------------------------------------------------------------
// <copyright file="MapExtensionsChildrenChangeManager.cs" company="Microsoft">
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
    using System.Collections.ObjectModel;
    using System.Collections.Specialized;
    using System.Diagnostics;
    using System.Windows;
    using Microsoft.Phone.Maps.Controls;

    /// <summary>
    /// Class MapExtensionsChildrenChangedManager.
    /// Concrete implementation of <see cref="MapExtensionsChildrenChangeManager"/> that will listen
    /// to change events in the source collection events to the Map.Layers collection
    /// </summary>
    internal class MapExtensionsChildrenChangeManager : CollectionChangeListener<DependencyObject>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MapExtensionsChildrenChangeManager"/> class
        /// </summary>
        /// <param name="sourceCollection">Source collection that will be managed</param>
        public MapExtensionsChildrenChangeManager(INotifyCollectionChanged sourceCollection)
        {
            if (sourceCollection == null)
            {
                throw new ArgumentNullException("sourceCollection");
            }

            this.ObjectToMapLayerMapping = new Dictionary<DependencyObject, MapLayer>();

            // Start listening to events
            sourceCollection.CollectionChanged += this.CollectionChanged;
        }

        /// <summary>
        /// Gets or sets Map to be used to insert/remove/etc MapLayers per the forwarded events
        /// </summary>
        public Map Map { get; set; }

        /// <summary>
        /// Gets or sets the dictionary to be used to track the map layers created here.
        /// When removing the objects, the appropriate actions will be taken on the target MapLayer.
        /// Each object from the source collection is translated into a MapLayer.
        /// </summary>
        private Dictionary<DependencyObject, MapLayer> ObjectToMapLayerMapping { get; set; }

        /// <summary>
        /// Implements the InsertItemInternal from the parent class.
        /// Will take the object and map into object of MapLayer 
        /// that can be added to Map.Layers
        /// </summary>
        /// <param name="index">The index at which the object was inserted</param>
        /// <param name="obj">Object to be inserted</param>
        protected override void InsertItemInternal(int index, DependencyObject obj)
        {
            MapLayer mapLayer = null;

            if (this.ObjectToMapLayerMapping.ContainsKey(obj))
            {
                throw new InvalidOperationException("Attempted to insert the same object twice");
            }

            mapLayer = GetMapLayerForObject(obj);

            this.ObjectToMapLayerMapping[obj] = mapLayer;
            this.Map.Layers.Insert(index, mapLayer);
        }

        /// <summary>
        /// Implements the RemoveItemInternal from the parent class.
        /// Will take the object and remove the target item from the Map.Layers collection
        /// </summary>
        /// <param name="obj">Object to be removed</param>
        protected override void RemoveItemInternal(DependencyObject obj)
        {
            bool mappingContainsObject;

            mappingContainsObject = this.ObjectToMapLayerMapping.ContainsKey(obj);

            Debug.Assert(mappingContainsObject, "It is expected that there is a mapping for the object");

            if (mappingContainsObject)
            {
                MapLayer mapLayer;

                mapLayer = this.ObjectToMapLayerMapping[obj];
                this.ObjectToMapLayerMapping.Remove(obj);
                this.Map.Layers.Remove(mapLayer);

                if (!(obj is MapItemsControl))
                {
                    Debug.Assert(mapLayer.Count == 1, "Expected that the map overlay once created is still there");
                }

                // Clear the bindings in the map overlays.
                foreach (MapOverlay mapOverlay in mapLayer)
                {
                    MapChild.ClearMapOverlayBindings(mapOverlay);
                }
            }
        }

        /// <summary>
        /// Implements the behavior the target collection will have when the source is reset
        /// </summary>
        protected override void ResetInternal()
        {
            // The source collection has changed drastically enough.
            // Clear all the MapOverlay bindings.
            foreach (MapLayer mapLayer in this.Map.Layers)
            {
                foreach (MapOverlay mapOverlay in mapLayer)
                {
                    MapChild.ClearMapOverlayBindings(mapOverlay);
                }
            }

            this.Map.Layers.Clear();
            this.ObjectToMapLayerMapping.Clear();
        }

        /// <summary>
        /// Implements the AddInternal from the parent class.
        /// Will take the object and map into object of MapLayer
        /// that can be added to Map.Layers
        /// </summary>
        /// <param name="obj">Object to be added</param>
        protected override void AddInternal(DependencyObject obj)
        {
            MapLayer mapLayer = null;

            if (this.ObjectToMapLayerMapping.ContainsKey(obj))
            {
                throw new InvalidOperationException("Attempted to insert the same object twice");
            }

            mapLayer = GetMapLayerForObject(obj);

            this.ObjectToMapLayerMapping[obj] = mapLayer;
            this.Map.Layers.Add(mapLayer);  
        }

        /// <summary>
        /// Moves the specified object from the old index to the new index
        /// </summary>
        /// <param name="obj">Object to be moved</param>
        /// <param name="newIndex">New index</param>
        protected override void MoveInternal(DependencyObject obj, int newIndex)
        {
            bool mappingContainsObject;

            mappingContainsObject = this.ObjectToMapLayerMapping.ContainsKey(obj);
            Debug.Assert(mappingContainsObject, "target object should be in the mapping table");

            if (mappingContainsObject)
            {
                ObservableCollection<MapLayer> layers;

                layers = (ObservableCollection<MapLayer>)this.Map.Layers;
                layers.Move(layers.IndexOf(this.ObjectToMapLayerMapping[obj]), newIndex);                
            }            
        }
        
        /// <summary>
        /// Takes the target object and create the corresponding MapLayer that will be used
        /// to host in MapOverlays all the items provided.
        /// </summary>
        /// <param name="obj">Object from the source collection to be processed</param>
        /// <returns>The MapLayer that will be used to host the items from the source</returns>
        /// <remarks>
        /// It only supports two types of objects 1) MapItemsControl or 2) anything else.
        /// For MapItemsControls, the creation of the MapOverlays will be deferred to the MapItemsControl
        /// </remarks>
        private static MapLayer GetMapLayerForObject(object obj)
        {
            MapLayer mapLayer;
            MapItemsControl mapItemsControl;

            // Only to types of objects supported per se.
            // 1) MapItemsControl
            // 2) Everything else
            mapItemsControl = obj as MapItemsControl;

            if (mapItemsControl != null)
            {
                // MapsItemsControl does their own control of creation of MapOverlays
                // because by the time we are here, items may be there already.
                // For that reason, we only bring the MapLayer an add it.
                mapLayer = mapItemsControl.MapLayer;

                Debug.Assert(mapLayer.Count == mapItemsControl.Items.Count, "MapLayer and MapItemsControl.Items count should match");
            }
            else
            {
                // Only 1 element. Create MapOverlay and insert
                mapLayer = new MapLayer();
                MapOverlay mapOverlay;

                mapOverlay = MapChild.CreateMapOverlay(obj, null);

                mapLayer.Add(mapOverlay);
            }

            return mapLayer;
        }
    }
}
