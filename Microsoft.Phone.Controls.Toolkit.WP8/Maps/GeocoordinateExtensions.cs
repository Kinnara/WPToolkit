// ---------------------------------------------------------------------------
// <copyright file="GeocoordinateExtensions.cs" company="Microsoft">
//     (c) Copyright Microsoft Corporation.
//     This source is subject to the Microsoft Public License (Ms-PL).
//     Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
//     All other rights reserved.
// </copyright>
// ---------------------------------------------------------------------------

namespace Windows.Devices.Geolocation
{
    using global::System.Device.Location;
    using global::System.Diagnostics.CodeAnalysis;
    using global::System.Security;

    /// <summary>
    /// Represents a class that extends Geocoordinate.
    /// </summary>
    [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Geocoordinate", Justification = "Geocoordinate is a valid word in the Windows.Devices.Geolocation namespace")]
    public static class GeocoordinateExtensions
    {
        /// <summary>
        /// Creates a <see cref="GeoCoordinate"/> from a <see cref="Geocoordinate"/>.
        /// </summary>
        /// <param name="geocoordinate">A <see cref="Geocoordinate"/> to create a <see cref="GeoCoordinate"/> from.</param>
        /// <returns>Returns <see cref="GeoCoordinate"/></returns>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "geocoordinate", Justification = "Geocoordinate is a valid word in the Windows.Devices.Geolocation namespace")]
        public static GeoCoordinate ToGeoCoordinate(this Geocoordinate geocoordinate)
        {
            if (geocoordinate == null)
            {
                return null;
            }

            GeoCoordinate geoCoordinate = new GeoCoordinate()
            {
                // Per MSDN, http://msdn.microsoft.com/en-us/library/ee808821.aspx (not in the invidual properties, but rather this constructor)
                // Altitude, Course and Speed and VerticalAccuracy, if unknown, they are supposed to be NaN.
                Altitude = geocoordinate.Altitude ?? double.NaN,
                Course = geocoordinate.Heading ?? double.NaN,
                HorizontalAccuracy = geocoordinate.Accuracy,
                Latitude = geocoordinate.Latitude,
                Longitude = geocoordinate.Longitude,
                Speed = geocoordinate.Speed ?? double.NaN,
                VerticalAccuracy = geocoordinate.AltitudeAccuracy ?? double.NaN,
            };

            return geoCoordinate;            
        }
    }
}