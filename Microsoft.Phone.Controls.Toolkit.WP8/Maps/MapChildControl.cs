// ---------------------------------------------------------------------------
// <copyright file="MapChildControl.cs" company="Microsoft">
//     (c) Copyright Microsoft Corporation.
//     This source is subject to the Microsoft Public License (Ms-PL).
//     Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
//     All other rights reserved.
// </copyright>
// ---------------------------------------------------------------------------

namespace Microsoft.Phone.Maps.Toolkit
{
    using System.ComponentModel;
    using System.Device.Location;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Markup;
    using Microsoft.Phone.Maps.Controls;

    /// <summary>
    /// Represents a child of a map, which uses geographic coordinates to position itself.
    /// </summary>
    [ContentProperty("Content")]
    public class MapChildControl : ContentControl
    {
        #region Dependency properties

        /// <summary>
        /// Identifies the <see cref="GeoCoordinate"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty GeoCoordinateProperty = DependencyProperty.Register(
            "GeoCoordinate",
            typeof(GeoCoordinate),
            typeof(MapChildControl),
            new PropertyMetadata(OnGeoCoordinateChangedCallback));

        /// <summary>
        /// Identifies the <see cref="PositionOrigin"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty PositionOriginProperty = DependencyProperty.Register(
            "PositionOrigin",
            typeof(Point),
            typeof(MapChildControl),
            new PropertyMetadata(OnPositionOriginChangedCallback));

        #endregion

        #region Public properties

        /// <summary>
        /// Gets or sets the geographic coordinate of the control on the map.
        /// </summary>
        [TypeConverter(typeof(GeoCoordinateConverter))]
        public GeoCoordinate GeoCoordinate
        {
            get { return (GeoCoordinate)this.GetValue(GeoCoordinateProperty); }
            set { this.SetValue(GeoCoordinateProperty, value); }
        }

        /// <summary>
        /// Gets or sets the position origin of the control, which defines the position on the control to anchor to the map.
        /// </summary>
        public Point PositionOrigin
        {
            get { return (Point)this.GetValue(PositionOriginProperty); }
            set { this.SetValue(PositionOriginProperty, value); }
        }

        #endregion

        #region Private static handlers for the dependency properties

        /// <summary>
        /// Callback method on object when GeoCoordinate is changed. 
        /// </summary>
        /// <param name="d">dependency object</param>
        /// <param name="e">event args</param>
        private static void OnGeoCoordinateChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            d.SetValue(MapChild.GeoCoordinateProperty, e.NewValue);
        }

        /// <summary>
        /// Callback method on object when PositionOrigin is changed. 
        /// </summary>
        /// <param name="d">dependency object</param>
        /// <param name="e">event args</param>
        private static void OnPositionOriginChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            d.SetValue(MapChild.PositionOriginProperty, e.NewValue);
        }

        #endregion
    }
}
