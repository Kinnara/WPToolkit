// ---------------------------------------------------------------------------
// <copyright file="MapsSample.xaml.cs" company="Microsoft">
//     (c) Copyright Microsoft Corporation.
//     This source is subject to the Microsoft Public License (Ms-PL).
//     Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
//     All other rights reserved.
// </copyright>
// ---------------------------------------------------------------------------

namespace PhoneToolkitSample.Samples
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Device.Location;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;
    using System.Windows;
    using Microsoft.Phone.Controls;
    using Microsoft.Phone.Maps.Controls;
    using Microsoft.Phone.Maps.Services;
    using Microsoft.Phone.Maps.Toolkit;
    using Microsoft.Phone.Shell;
    using Microsoft.Phone.Tasks;
    using PhoneToolkitSample.Data.Maps;
    using Windows.Devices.Geolocation;
    using Windows.Foundation;

    #region Enums

    /// <summary>
    /// Map Mode
    /// </summary>
    public enum MapMode
    {
        /// <summary>
        /// Stores are displayed in the map
        /// </summary>
        Stores,

        /// <summary>
        /// Map is showing directions using a Windows Phone Task
        /// </summary>
        Directions,

        /// <summary>
        /// Map is showing a route in the map
        /// </summary>
        Route
    }

    #endregion

    /// <summary>
    /// Maps sample page
    /// </summary>
    public partial class MapsSample : PhoneApplicationPage
    {
        /// <summary>
        /// Main view model used in this page
        /// </summary>
        private readonly MapsViewModel mainViewModel = new MapsViewModel();
        
        /// <summary>
        /// Seattle's GeoCoordinate
        /// </summary>
        private readonly GeoCoordinate seattleGeoCoordinate = new GeoCoordinate(47.60097, -122.3331);
        
        /// <summary>
        /// Zoom level to be used when showing the user location
        /// </summary>
        private readonly double userLocationMarkerZoomLevel = 16;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="MapsSample"/> class
        /// </summary>
        public MapsSample()
        {
            this.InitializeComponent();
            this.MapExtensionsSetup(this.Map);

            this.DataContext = this.mainViewModel;

            this.HideStoresIconMenuItem = (ApplicationBarMenuItem)ApplicationBar.MenuItems[0];
            this.ShowStoresIconMenuItem = (ApplicationBarMenuItem)ApplicationBar.MenuItems[1];

            this.Loaded += this.OnPageLoaded;
        }

        /// <summary>
        /// Gets or sets the current Map Mode used in the page
        /// </summary>
        private MapMode Mode { get; set; }

        /// <summary>
        /// Gets or sets the route displayed in the map
        /// </summary>
        private MapRoute MapRoute { get; set; }

        /// <summary>
        /// Gets or sets the application bar menu item used to hide the stores displayed in the map
        /// </summary>
        private ApplicationBarMenuItem HideStoresIconMenuItem { get; set; }

        /// <summary>
        /// Gets or sets the application bar menu item used to show the stores displayed in the map
        /// </summary>
        private ApplicationBarMenuItem ShowStoresIconMenuItem { get; set; }

        #region Event Handlers

        /// <summary>
        /// Event handler to be called when the page has been loaded
        /// </summary>
        /// <param name="sender">Sender of the event</param>
        /// <param name="e">Event arguments</param>
        private void OnPageLoaded(object sender, RoutedEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(this.MapFlightToSeattle));

            this.StoresMapItemsControl.ItemsSource = this.mainViewModel.StoreList;
        }

        /// <summary>
        /// Event handler for the Me button. It will show the user location marker and set the view on the map
        /// </summary>
        /// <param name="sender">Sender of the event</param>
        /// <param name="e">Event arguments</param>
        private async void OnMe(object sender, EventArgs e)
        {
            await this.ShowUserLocation();

            this.Map.SetView(this.UserLocationMarker.GeoCoordinate, this.userLocationMarkerZoomLevel);
        }

        /// <summary>
        /// Event handler called when the user tap and hold in the map
        /// </summary>
        /// <param name="sender">Sender of the event</param>
        /// <param name="e">Event arguments</param>
        private async void OnMapHold(object sender, System.Windows.Input.GestureEventArgs e)
        {
            ReverseGeocodeQuery query;
            List<MapLocation> mapLocations;
            string pushpinContent;
            MapLocation mapLocation;

            query = new ReverseGeocodeQuery();
            query.GeoCoordinate = this.Map.ConvertViewportPointToGeoCoordinate(e.GetPosition(this.Map));

            mapLocations = (List<MapLocation>)await query.GetMapLocationsAsync();
            mapLocation = mapLocations.FirstOrDefault();

            if (mapLocation != null)
            {
                this.RouteDirectionsPushPin.GeoCoordinate = mapLocation.GeoCoordinate;

                pushpinContent = mapLocation.Information.Name;
                pushpinContent = string.IsNullOrEmpty(pushpinContent) ? mapLocation.Information.Description : null;
                pushpinContent = string.IsNullOrEmpty(pushpinContent) ? string.Format("{0} {1}", mapLocation.Information.Address.Street, mapLocation.Information.Address.City) : null;

                this.RouteDirectionsPushPin.Content = pushpinContent.Trim();
                this.RouteDirectionsPushPin.Visibility = Visibility.Visible;
            }
        }

        /// <summary>
        /// Event handler for the directions button
        /// </summary>
        /// <param name="sender">Sender of the event</param>
        /// <param name="e">Event arguments</param>
        private void OnDirections(object sender, EventArgs e)
        {
            if (this.RouteDirectionsPushPin.GeoCoordinate == null || this.RouteDirectionsPushPin.Visibility == Visibility.Collapsed)
            {
                MessageBox.Show("Tap and hold somewhere in the map to show a Pushpin. After that you can get directions to there");
            }
            else
            {
                BingMapsDirectionsTask directionsTask;

                directionsTask = new BingMapsDirectionsTask();

                directionsTask.End = new LabeledMapLocation()
                {
                    Label = (string)this.RouteDirectionsPushPin.Content,
                    Location = this.RouteDirectionsPushPin.GeoCoordinate
                };

                this.ChangeMode(MapMode.Directions);

                directionsTask.Show();
            }
        }

        /// <summary>
        /// Event handler for the show route button
        /// </summary>
        /// <param name="sender">Sender of the event</param>
        /// <param name="e">Event arguments</param>
        private async void OnShowRoute(object sender, EventArgs e)
        {
            if (this.RouteDirectionsPushPin.GeoCoordinate == null || this.RouteDirectionsPushPin.Visibility == Visibility.Collapsed)
            {
                MessageBox.Show("Tap and hold somewhere in the map to show a Pushpin. After that you can show route from your location to the destination");
            }
            else
            {
                RouteQuery query;
                List<GeoCoordinate> wayPoints;
                Route route;

                this.ShowStores(false);
                if (this.MapRoute != null)
                {
                    this.Map.RemoveRoute(this.MapRoute);
                }

                await this.ShowUserLocation();

                query = new RouteQuery();
                wayPoints = new List<GeoCoordinate>();

                wayPoints.Add(this.UserLocationMarker.GeoCoordinate);
                wayPoints.Add(this.RouteDirectionsPushPin.GeoCoordinate);

                query.Waypoints = wayPoints;

                route = await query.GetRouteAsync();
                this.MapRoute = new MapRoute(route);

                this.Map.SetView(route.BoundingBox);
                this.Map.AddRoute(this.MapRoute);

                this.ChangeMode(MapMode.Route);
            }
        }

        /// <summary>
        /// Event handler for the Hide Stores menu item
        /// </summary>
        /// <param name="sender">Sender of the events</param>
        /// <param name="e">Event arguments</param>
        private void OnHideStores(object sender, EventArgs e)
        {
            this.ShowStores(false);
        }

        /// <summary>
        /// Event handler for the Show Stores menu item
        /// </summary>
        /// <param name="sender">Sender of the events</param>
        /// <param name="e">Event arguments</param>
        private void OnShowStores(object sender, EventArgs e)
        {
            this.ShowStores(true);
        }

        /// <summary>
        /// Toggles the visuals to show or hide the stores
        /// </summary>
        /// <param name="show">boolean whether the stores should be displayed or not</param>
        private void ShowStores(bool show)
        {
            this.ShowStoresIconMenuItem.IsEnabled = !show;
            this.HideStoresIconMenuItem.IsEnabled = show;

            foreach (Store store in this.mainViewModel.StoreList)
            {
                store.Visibility = show ? Visibility.Visible : Visibility.Collapsed;
            }

            if (show)
            {
                this.ChangeMode(MapMode.Stores);

                this.MapFlightToSeattle();
            }
        }

        #endregion

        #region Helpers

        /// <summary>
        /// Will set view in the map to an area that will display all the stores in the map
        /// </summary>
        private void MapFlightToSeattle()
        {
            LocationRectangle locationRectangle;

            locationRectangle = LocationRectangle.CreateBoundingRectangle(from store in this.mainViewModel.StoreList select store.GeoCoordinate);

            this.Map.SetView(locationRectangle, new Thickness(20, 20, 20, 20));
        }

        /// <summary>
        /// Show the user location in the map
        /// </summary>
        /// <returns>Task that can used to await</returns>
        private async Task ShowUserLocation()
        {
            Geolocator geolocator;
            Geoposition geoposition;

            this.UserLocationMarker = (UserLocationMarker)this.FindName("UserLocationMarker");

            geolocator = new Geolocator();

            geoposition = await geolocator.GetGeopositionAsync();

            this.UserLocationMarker.GeoCoordinate = geoposition.Coordinate.ToGeoCoordinate();
            this.UserLocationMarker.Visibility = System.Windows.Visibility.Visible;
        }

        /// <summary>
        /// Setup the map extensions objects.
        /// All named objects inside the map extensions will have its references properly set
        /// </summary>
        /// <param name="map">The map that uses the map extensions</param>
        private void MapExtensionsSetup(Map map)
        {
            ObservableCollection<DependencyObject> children = MapExtensions.GetChildren(map);
            var runtimeFields = this.GetType().GetRuntimeFields();

            foreach (DependencyObject i in children)
            {
                var info = i.GetType().GetProperty("Name");

                if (info != null)
                {
                    string name = (string)info.GetValue(i);

                    if (name != null)
                    {
                        foreach (FieldInfo j in runtimeFields)
                        {
                            if (j.Name == name)
                            {
                                j.SetValue(this, i);
                                break;
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Get the current user location
        /// </summary>
        /// <returns>IAsyncOperation caller can await</returns>
        private IAsyncOperation<Geoposition> GetUserLocation()
        {
            Geolocator geolocator;

            geolocator = new Geolocator();

            return geolocator.GetGeopositionAsync();
        }

        /// <summary>
        /// Changes the effective map mode. Will switch visuals state it changed
        /// </summary>
        /// <param name="mode">New map mode</param>
        private void ChangeMode(MapMode mode)
        {
            if (this.Mode != mode)
            {
                this.Mode = mode;

                switch (this.Mode)
                {
                    case MapMode.Stores:
                        this.ShowStores(true);
                        this.RouteDirectionsPushPin.Visibility = Visibility.Collapsed;                        
                        if (this.MapRoute != null)
                        {
                            this.Map.RemoveRoute(this.MapRoute);
                        }

                        break;

                    case MapMode.Route:
                        this.ShowStores(false);
                        break;

                    case MapMode.Directions:
                        this.ShowStores(false);
                        if (this.MapRoute != null)
                        {
                            this.Map.RemoveRoute(this.MapRoute);
                        }

                        break;
                }
            }
        }
        #endregion
    }
}