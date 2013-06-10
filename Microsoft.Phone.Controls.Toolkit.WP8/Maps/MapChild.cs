// ---------------------------------------------------------------------------
// <copyright file="MapChild.cs" company="Microsoft">
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
    using System.ComponentModel;
    using System.Device.Location;
    using System.Diagnostics;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Controls.Primitives;
    using System.Windows.Data;
    using Microsoft.Phone.Maps.Controls;

    /// <summary>
    /// Represents a child of a map, which uses geographic coordinates to position itself.
    /// </summary>
    public static class MapChild
    {
        /// <summary>
        /// Gets or sets the child's geographic coordinate position
        /// </summary>
        public static readonly DependencyProperty GeoCoordinateProperty = DependencyProperty.RegisterAttached(
            "GeoCoordinate",
            typeof(object),
            typeof(MapChild),
            null);

        /// <summary>
        /// Gets or sets the child's position origin.
        /// </summary>
        public static readonly DependencyProperty PositionOriginProperty = DependencyProperty.RegisterAttached(
            "PositionOrigin",
            typeof(Point),
            typeof(MapChild),
            null);

        /// <summary>
        /// Identifies the Microsoft.Phone.Maps.Toolkit.MapChild.ToolkitCreatedProperty attached dependency property.
        /// </summary>
        internal static readonly DependencyProperty ToolkitCreatedProperty = DependencyProperty.RegisterAttached(
            "ToolkitCreated",
            typeof(bool),
            typeof(MapChild),
            null);

        /// <summary>
        /// Gets the geographic coordinate position of the child.
        /// </summary>
        /// <param name="element">The dependency object</param>
        /// <returns>Returns <see cref="GeoCoordinate"/></returns>
        [TypeConverter(typeof(GeoCoordinateConverter))]
        public static GeoCoordinate GetGeoCoordinate(DependencyObject element)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }

            return (GeoCoordinate)element.GetValue(MapChild.GeoCoordinateProperty);
        }

        /// <summary>
        /// Sets the geographic coordinate position of the child. 
        /// </summary>
        /// <param name="element">The dependency object</param>
        /// <param name="value">The coordinate to use to position the child</param>
        public static void SetGeoCoordinate(DependencyObject element, GeoCoordinate value)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }

            element.SetValue(MapChild.GeoCoordinateProperty, value);
        }

        /// <summary>
        /// Gets the position origin of the child.
        /// </summary>
        /// <param name="element">The dependency object</param>
        /// <returns>Returns <see cref="GeoCoordinate"/></returns>
        public static Point GetPositionOrigin(DependencyObject element)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }

            return (Point)element.GetValue(MapChild.PositionOriginProperty);
        }

        /// <summary>
        /// Sets the position origin of the child.
        /// </summary>
        /// <param name="element">The dependency object</param>
        /// <param name="value">The position origin of the child</param>
        public static void SetPositionOrigin(DependencyObject element, Point value)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }

            element.SetValue(MapChild.PositionOriginProperty, value);
        }

        /// <summary>
        /// Creates a MapOverlay with the specified content and content template.
        /// It will have special setup so that later the dependency properties from MapOverlay
        /// and the attached properties from the target UI can be in a binding.
        /// </summary>
        /// <param name="content">Content of the MapOverlay</param>
        /// <param name="contentTemplate">Template to be used in the MapOverlay</param>
        /// <returns>The MapOverlay that was created</returns>
        internal static MapOverlay CreateMapOverlay(object content, DataTemplate contentTemplate)
        {
            MapOverlay mapOverlay;
            MapOverlayItem presenterHelper;

            mapOverlay = new MapOverlay();
            presenterHelper = new MapOverlayItem(content, contentTemplate, mapOverlay);

            // For insertion and removal purposes, we will have a diferentiation between
            // map overlays that are created by this toolkit and when they are not.
            // This will help to determine, when they are removed, to which
            // we will need to to speacial cleaning
            mapOverlay.SetValue(MapChild.ToolkitCreatedProperty, true);
            mapOverlay.Content = presenterHelper;

            // The dependency properties from MapOverlay, will be binded
            // until the content finally has a UI.
            // At that point, we will try to get the actual ui 
            // and bind the attached properties from the ui to the 
            // dependency properties of the map overlay.
            return mapOverlay;
        }

        /// <summary>
        /// Method that takes care of setting the two way bindings for all the attached properties
        /// from MapOverlay to the actual UI (not the intermediary presenter).
        /// </summary>
        /// <remarks>
        /// Even though the MapOverlay is supposed to be the target, it is the source due to issues with
        /// setting a two way binding where the source is an attached property
        /// </remarks>
        /// <param name="mapOverlay">MapOverlay that will be used in the two way binding</param>
        internal static void BindMapOverlayProperties(MapOverlay mapOverlay)
        {
            MapOverlayItem mapOverlayItem;
            DependencyObject targetObject;

            mapOverlayItem = (MapOverlayItem)mapOverlay.Content;

            targetObject = mapOverlayItem.GetVisualChildren().FirstOrDefault();

            if (targetObject == null)
            {
                throw new InvalidOperationException("Could not bind the properties because there was no UI");
            }

            BindMapOverlayProperties(mapOverlay, targetObject);
        }

        /// <summary>
        /// Method that takes care of setting the two way bindings for all the attached properties
        /// from MapOverlay to the actual UI (not the intermediary presenter).
        /// </summary>
        /// <remarks>
        /// Even though the MapOverlay is supposed to be the target, it is the source due to issues with
        /// setting a two way binding where the source is an attached property
        /// </remarks>
        /// <param name="mapOverlay">MapOverlay that will be used in the two way binding</param>
        /// <param name="targetObject">Source object that will be used in the two way binding</param>
        internal static void BindMapOverlayProperties(MapOverlay mapOverlay, DependencyObject targetObject)
        {
            // *** See remarks section ***
            // Bring the value from target object to mapOverlay so that before binding starts, both have the same value.
            // BindMapOverlayProperty will setup binding two way with mapOverlay as source.
            // If value is not set before in MapOverlay, the MapOverlay value will trumpt, that we do not want that.
            Debug.Assert(mapOverlay.GeoCoordinate == null, "Expected to have mapOverlay as null");
            mapOverlay.GeoCoordinate = (GeoCoordinate)targetObject.GetValue(MapChild.GeoCoordinateProperty);
            BindMapOverlayProperty(mapOverlay, "GeoCoordinate", targetObject, MapChild.GeoCoordinateProperty);

            Debug.Assert(mapOverlay.PositionOrigin.X == 0, "Expected to have a default PositionOrigin.X with value of zero");
            Debug.Assert(mapOverlay.PositionOrigin.Y == 0, "Expected to have a default PositionOrigin.X with value of zero");
            mapOverlay.PositionOrigin = (Point)targetObject.GetValue(MapChild.PositionOriginProperty);
            BindMapOverlayProperty(mapOverlay, "PositionOrigin", targetObject, MapChild.PositionOriginProperty);
        }

        /// <summary>
        /// Binds two ways the map overlay to the target dependency property
        /// </summary>
        /// <param name="mapOverlay">MapOverlay that will be used as source</param>
        /// <param name="mapOverlayDependencyProperty">Name of the source dependency property from the MapOverlay</param>
        /// <param name="targetObject">Target object</param>
        /// <param name="targetDependencyProperty">Target dependency property</param>
        internal static void BindMapOverlayProperty(MapOverlay mapOverlay, string mapOverlayDependencyProperty, DependencyObject targetObject, DependencyProperty targetDependencyProperty)
        {
            Binding binding = new Binding()
            {
                Source = mapOverlay,
                Mode = BindingMode.TwoWay,
                Path = new PropertyPath(mapOverlayDependencyProperty)
            };

            BindingOperations.SetBinding(targetObject, targetDependencyProperty, binding);
        }

        /// <summary>
        /// Clear the bindings created when the overlay was created by MapChild
        /// </summary>
        /// <param name="mapOverlay">MapOverlay that was created by MapChild</param>
        internal static void ClearMapOverlayBindings(MapOverlay mapOverlay)
        {
            MapOverlayItem mapOverlayItem;
            DependencyObject targetObject;

            mapOverlayItem = (MapOverlayItem)mapOverlay.Content;

            targetObject = mapOverlayItem.GetVisualChildren().FirstOrDefault();

            // In some cases, the MapOverlay was not presented in the UI.
            // Bindings are create when the MapOverlay is presented in the, so
            // if there is no visual, there is nothing to clear.
            if (targetObject != null)
            {
                ClearMapOverlayBindings(mapOverlay, targetObject);
            }
        }

        /// <summary>
        /// Clear the bindings created when the overlay was created by MapChild
        /// </summary>
        /// <param name="mapOverlay">MapOverlay that was created by MapChild</param>
        /// <param name="targetObject">Target object to be used to clear the bindings</param>
        internal static void ClearMapOverlayBindings(MapOverlay mapOverlay, DependencyObject targetObject)
        {
            GeoCoordinate geoCoordinate;
            Point point;

            // Clear happens always in the target object. 
            // Binding was created with MapOverlay been the source.
            // See code that creates the binding for more info.
            Debug.Assert((bool)mapOverlay.GetValue(MapChild.ToolkitCreatedProperty), "expected that we only get this calls for overlays created by the toolkit");

            // It would be weird that bindings are cleared and out of the sudden the object from the client
            // looses the values he once provided. 
            // They are lost because we setup the binding in a weird way. See code that creates the map overlay.
            // Save them to set them after clearing the binding
            geoCoordinate = (GeoCoordinate)targetObject.GetValue(MapChild.GeoCoordinateProperty);
            point = (Point)targetObject.GetValue(MapChild.PositionOriginProperty);

            targetObject.ClearValue(MapChild.GeoCoordinateProperty);
            targetObject.ClearValue(MapChild.PositionOriginProperty);

            targetObject.SetValue(MapChild.GeoCoordinateProperty, geoCoordinate);
            targetObject.SetValue(MapChild.PositionOriginProperty, point);
        }
    }
}
