// ---------------------------------------------------------------------------
// <copyright file="MapItemsControl.cs" company="Microsoft">
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
    using System.Collections.Specialized;
    using System.Diagnostics;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Markup;
    using Microsoft.Phone.Maps.Controls;

    /// <summary>
    /// Represents a control that can be used to present a collection of items on a map.
    /// </summary>
    [ContentProperty("Items")]
    public sealed class MapItemsControl : DependencyObject
    {
        /// <summary>
        /// Identifies the <see cref="ItemsSource"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register(
            "ItemsSource",
            typeof(IEnumerable),
            typeof(MapItemsControl),
            new PropertyMetadata(OnItemsSourceChanged));

        /// <summary>
        /// Identifies the <see cref="ItemTemplate"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ItemTemplateProperty = DependencyProperty.Register(
            "ItemTemplate",
            typeof(DataTemplate),
            typeof(MapItemsControl),
            new PropertyMetadata(OnItemTemplateChanged));

        /// <summary>
        /// Identifies the <see cref="Name"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty NameProperty = DependencyProperty.Register(
            "ItemTemplate",
            typeof(string),
            typeof(MapItemsControl),
            null);

        /// <summary>
        /// Initializes a new instance of the <see cref="MapItemsControl"/> class
        /// </summary>
        public MapItemsControl()
        {
            this.MapLayer = new MapLayer();
            this.Items = new MapChildCollection();
            this.ItemsChangeManager = new MapItemsControlChangeManager((INotifyCollectionChanged)this.Items)
            {
                MapLayer = this.MapLayer
            };

            this.ItemsSource = null;
            this.ItemTemplate = null;
        }

        /// <summary>
        /// Gets the collection used to generate the content of the control.
        /// </summary>
        public MapChildCollection Items { get; private set; }

        /// <summary>
        /// Gets or sets the name
        /// </summary>
        public string Name
        {
            get { return (string)this.GetValue(MapItemsControl.NameProperty); }
            set { this.SetValue(MapItemsControl.NameProperty, value); }
        }

        /// <summary>
        /// Gets or sets a collection used to generate the content of the <see cref="ItemsControl"/>. 
        /// </summary>
        public IEnumerable ItemsSource
        {
            get { return (IEnumerable)this.GetValue(MapItemsControl.ItemsSourceProperty); }
            set { this.SetValue(MapItemsControl.ItemsSourceProperty, value); }
        }

        /// <summary>
        /// Gets or sets the <see cref="DataTemplate"/> used to display each item. 
        /// </summary>
        public DataTemplate ItemTemplate
        {
            get { return (DataTemplate)this.GetValue(MapItemsControl.ItemTemplateProperty); }
            set { this.SetValue(MapItemsControl.ItemTemplateProperty, value); }
        }

        /// <summary>
        /// Gets the <see cref="MapItemsControlChangeManager"/> used in the backend
        /// </summary>
        internal MapItemsControlChangeManager ItemsChangeManager { get; private set; }

        /// <summary>
        /// Gets the <see cref="ItemsSourceChangeManager"/> used in the backend
        /// </summary>
        internal MapItemsSourceChangeManager ItemsSourceChangeManager { get; private set; }

        /// <summary>
        /// Gets or sets the MapLayer used to map the input
        /// </summary>
        internal MapLayer MapLayer { get; set; }

        /// <summary>
        /// Will handle the item template change
        /// </summary>
        /// <param name="d">DependencyObject that triggers the event.</param>
        /// <param name="e">Event args</param>
        private static void OnItemTemplateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            MapItemsControl mapsItemControl;
            DataTemplate dataTemplate;

            mapsItemControl = (MapItemsControl)d;
            dataTemplate = (DataTemplate)e.NewValue;

            mapsItemControl.ItemsChangeManager.ItemTemplate = dataTemplate;

            foreach (MapOverlay mapOverlay in mapsItemControl.MapLayer)
            {
                // New template, so there will be a new ui element created
                MapChild.ClearMapOverlayBindings(mapOverlay);

                MapOverlayItem mapOverlayPresenterHelper = (MapOverlayItem)mapOverlay.Content;
                mapOverlayPresenterHelper.ContentTemplate = dataTemplate;
            }
        }

        /// <summary>
        /// Will handle the items source change
        /// </summary>
        /// <param name="d">D9ependencyObject that triggers the event.</param>
        /// <param name="e">Event args</param>
        private static void OnItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            MapItemsControl mapItemsControl;
            IEnumerable newItemsSource;

            mapItemsControl = (MapItemsControl)d;
            newItemsSource = (IEnumerable)e.NewValue;

            // Following behavior from ItemsControl.ItemsSource
            if (mapItemsControl.Items.Count > 0)
            {
                throw new InvalidOperationException("Items must be empty before using Items Source");
            }

            if (newItemsSource != null)
            {
                INotifyCollectionChanged itemSourceAsINotifyCollectionChanged;

                if (mapItemsControl.ItemsSourceChangeManager != null)
                {
                    mapItemsControl.ItemsSourceChangeManager.Disconnect();
                    mapItemsControl.ItemsSourceChangeManager = null;
                }
                
                // Sync both lists
                Debug.Assert(mapItemsControl.Items.Count == 0, "Expected MapItemsControl.Items.Count == 0");
                mapItemsControl.Items.AddRange(newItemsSource);

                itemSourceAsINotifyCollectionChanged = newItemsSource as INotifyCollectionChanged;
                if (itemSourceAsINotifyCollectionChanged != null)
                {
                    mapItemsControl.ItemsSourceChangeManager = new MapItemsSourceChangeManager(itemSourceAsINotifyCollectionChanged)
                    {
                        Items = mapItemsControl.Items
                    };
                }
            }

            // In ItemsControl.Items world, it behaves as ReadOnly when there is an ItemsSource
            // NOTE: This needs to happen after all the Items collection has sucesfully been updated.
            mapItemsControl.Items.IsReadOnly = !(newItemsSource == null);
        }
    }
}
