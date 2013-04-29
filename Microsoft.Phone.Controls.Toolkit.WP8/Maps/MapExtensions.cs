// ---------------------------------------------------------------------------
// <copyright file="MapExtensions.cs" company="Microsoft">
//     (c) Copyright Microsoft Corporation.
//     This source is subject to the Microsoft Public License (Ms-PL).
//     Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
//     All other rights reserved.
// </copyright>
// ---------------------------------------------------------------------------

namespace Microsoft.Phone.Maps.Toolkit
{
    using System;
    using System.Collections.ObjectModel;
    using System.Device.Location;
    using System.Windows;
    using Microsoft.Phone.Maps.Controls;

    /// <summary>
    /// Represents a class that can extend the capabilities of the <see cref="Map"/> class.
    /// </summary>
    public static class MapExtensions
    {
        /// <summary>
        /// Identifies the Children dependency property.
        /// </summary>
        public static readonly DependencyProperty ChildrenProperty = DependencyProperty.RegisterAttached(
            "Children",
            typeof(ObservableCollection<DependencyObject>),
            typeof(MapExtensions),
            null);

        /// <summary>
        /// Identifies the Microsoft.Phone.Maps.Toolkit.MapExtensions.ChildrenChangedManagerProperty attached dependency property.
        /// </summary>
        private static readonly DependencyProperty ChildrenChangedManagerProperty = DependencyProperty.RegisterAttached(
            "ChildrenChangedManager",
            typeof(MapExtensionsChildrenChangeManager),
            typeof(MapExtensions),
            null);

        /// <summary>
        /// Gets the Children collection of a map.
        /// </summary>
        /// <param name="element">The dependency object</param>
        /// <returns>Returns <see cref="ObservableCollection&lt;DependencyObject&gt;"/></returns>
        public static ObservableCollection<DependencyObject> GetChildren(Map element)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }

            ObservableCollection<DependencyObject> childrenCollection = (ObservableCollection<DependencyObject>)element.GetValue(MapExtensions.ChildrenProperty);

            if (childrenCollection == null)
            {
                MapExtensionsChildrenChangeManager childrenChangeManager;

                childrenCollection = new ObservableCollection<DependencyObject>();
                childrenChangeManager = new MapExtensionsChildrenChangeManager(childrenCollection)
                {
                    Map = element
                };

                element.SetValue(MapExtensions.ChildrenProperty, childrenCollection);
                element.SetValue(MapExtensions.ChildrenChangedManagerProperty, childrenChangeManager);
            }

            return childrenCollection;
        }

        /// <summary>
        /// Adds a dependency object to the map at the specified location. 
        /// </summary>
        /// <param name="childrenCollection">An <see cref="ObservableCollection&lt;DependencyObject&gt;"/> to add to.</param>
        /// <param name="dependencyObject">The dependency object to add.</param>
        /// <param name="geoCoordinate">The geographic coordinate at which to add the dependency object.</param>
        public static void Add(this ObservableCollection<DependencyObject> childrenCollection, DependencyObject dependencyObject, GeoCoordinate geoCoordinate)
        {
            if (childrenCollection == null)
            {
                throw new ArgumentNullException("childrenCollection");
            }

            if (dependencyObject == null)
            {
                throw new ArgumentNullException("dependencyObject");
            }

            if (geoCoordinate == null)
            {
                throw new ArgumentNullException("geoCoordinate");
            }

            dependencyObject.SetValue(MapChild.GeoCoordinateProperty, geoCoordinate);
            childrenCollection.Add(dependencyObject);
        }

        /// <summary>
        /// Adds a dependency object to the map at the specified location. 
        /// </summary>
        /// <param name="childrenCollection">An <see cref="ObservableCollection&lt;DependencyObject&gt;"/> to add to.</param>
        /// <param name="dependencyObject">The dependency object to add.</param>
        /// <param name="geoCoordinate">The geographic coordinate at which to add the dependency object.</param>
        /// <param name="positionOrigin">The position origin to use.</param>
        public static void Add(this ObservableCollection<DependencyObject> childrenCollection, DependencyObject dependencyObject, GeoCoordinate geoCoordinate, Point positionOrigin)
        {
            if (childrenCollection == null)
            {
                throw new ArgumentNullException("childrenCollection");
            }

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
            childrenCollection.Add(dependencyObject);
        }
    }
}
