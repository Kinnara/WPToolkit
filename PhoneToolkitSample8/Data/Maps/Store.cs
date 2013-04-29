// ---------------------------------------------------------------------------
// <copyright file="Store.cs" company="Microsoft">
//     (c) Copyright Microsoft Corporation.
//     This source is subject to the Microsoft Public License (Ms-PL).
//     Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
//     All other rights reserved.
// </copyright>
// ---------------------------------------------------------------------------

namespace PhoneToolkitSample.Data.Maps
{
    using System.ComponentModel;
    using System.Device.Location;
    using System.Runtime.CompilerServices;
    using System.Windows;
    using Microsoft.Phone.Maps.Controls;

    /// <summary>
    /// Store data
    /// </summary>
    public class Store : INotifyPropertyChanged
    {
        /// <summary>
        /// Address of the store
        /// </summary>
        private string address;

        /// <summary>
        /// GeoCoordinate of the store
        /// </summary>
        private GeoCoordinate geoCoordinate;

        /// <summary>
        /// Whether the store is visible or not in the map
        /// </summary>
        private Visibility visibility;

        /// <summary>
        /// Event to be raised when a property value has changed
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Gets or sets the GeoCoordinate of the store
        /// </summary>
        [TypeConverter(typeof(GeoCoordinateConverter))]
        public GeoCoordinate GeoCoordinate
        {
            get
            {
                return this.geoCoordinate;
            }

            set
            {
                if (this.geoCoordinate != value)
                {
                    this.geoCoordinate = value;
                    this.NotifyPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets the address of the store
        /// </summary>
        public string Address
        {
            get
            {
                return this.address;
            }

            set
            {
                if (this.address != value)
                {
                    this.address = value;
                    this.NotifyPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets whether the store Pushpin is visible or not in the map
        /// </summary>
        public Visibility Visibility
        {
            get
            {
                return this.visibility;
            }

            set
            {
                if (this.visibility != value)
                {
                    this.visibility = value;
                    this.NotifyPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Generic NotifyPropertyChanged
        /// </summary>
        /// <param name="propertyName">Name of the property</param>
        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
