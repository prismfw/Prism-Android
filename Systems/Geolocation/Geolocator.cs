/*
Copyright (C) 2018  Prism Framework Team

This file is part of the Prism Framework.

The Prism Framework is free software; you can redistribute it and/or
modify it under the terms of the GNU General Public License
as published by the Free Software Foundation; either version 2
of the License, or (at your option) any later version.

The Prism Framework is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program; if not, write to the Free Software
Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.
*/


using System;
using System.Threading;
using System.Threading.Tasks;
using Android;
using Android.Content;
using Android.Content.PM;
using Android.Locations;
using Android.OS;
using Android.Runtime;
using Prism.Native;
using Prism.Systems.Geolocation;

namespace Prism.Android.Systems.Geolocation
{
    /// <summary>
    /// Represents an Android implementation of an <see cref="INativeGeolocator"/>.
    /// </summary>
    [Preserve(AllMembers = true)]
    [Register(typeof(INativeGeolocator), IsSingleton = true)]
    public class Geolocator : Java.Lang.Object, INativeGeolocator, ILocationListener, IPermissionRequestCallback
    {
        /// <summary>
        /// Occurs when the location is updated.
        /// </summary>
        public event EventHandler<GeolocationUpdatedEventArgs> LocationUpdated;

        /// <summary>
        /// Gets or sets the desired level of accuracy when reading geographic coordinates.
        /// </summary>
        public GeolocationAccuracy DesiredAccuracy
        {
            get { return criteria.Accuracy == Accuracy.Fine ? GeolocationAccuracy.Precise : GeolocationAccuracy.Approximate; }
            set { criteria.Accuracy = value == GeolocationAccuracy.Precise ? Accuracy.Fine : Accuracy.Coarse; }
        }

        /// <summary>
        /// Gets or sets the minimum distance, in meters, that should be covered before the location is updated again.
        /// </summary>
        public double DistanceThreshold
        {
            get { return distanceThreshold; }
            set
            {
                distanceThreshold = value;
                if (isActive)
                {
                    StartListening();
                }
            }
        }
        private double distanceThreshold = double.NaN;

        /// <summary>
        /// Gets or sets the amount of time, in milliseconds, that should pass before the location is updated again.
        /// </summary>
        public double UpdateInterval
        {
            get { return updateInterval; }
            set
            {
                updateInterval = value;
                if (isActive)
                {
                    StartListening();
                }
            }
        }
        private double updateInterval = double.NaN;
        
        private readonly Criteria criteria;
        private readonly LocationManager locator;
        
        private bool isActive;
        private Location lastLocation;
        private ManualResetEventSlim authorizer, retriever;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="Geolocator"/> class.
        /// </summary>
        public Geolocator()
        {
            criteria = new Criteria();
            locator = (LocationManager)Application.MainActivity.GetSystemService(Context.LocationService);
        }

        /// <summary>
        /// Signals the geolocation service to begin listening for location updates.
        /// </summary>
        public void BeginLocationUpdates()
        {
            StartListening();
        }

        /// <summary>
        /// Signals the geolocation service to stop listening for location updates.
        /// </summary>
        public void EndLocationUpdates()
        {
            locator.RemoveUpdates(this);
            isActive = false;
        }

        /// <summary>
        /// Makes a singular request to the geolocation service for the current location.
        /// </summary>
        /// <returns>A <see cref="Coordinate"/> representing the current location.</returns>
        public Task<Coordinate> GetCoordinateAsync()
        {
            return Task.Run(() =>
            {
                retriever = new ManualResetEventSlim(false);
                locator.RequestSingleUpdate(criteria, this, Looper.MainLooper);
                retriever?.Wait(10000);
                return BuildCoordinate(lastLocation);
            });
        }
        
        /// <summary></summary>
        /// <param name="location"></param>
        public void OnLocationChanged(Location location)
        {
            if (location != null)
            {
                lastLocation = location;
            }
            
            if (lastLocation == null)
            {
                return;
            }
            
            if (retriever != null)
            {
                retriever.Set();
                retriever = null;
                return;
            }
            
            LocationUpdated(this, new GeolocationUpdatedEventArgs(BuildCoordinate(lastLocation)));
            lastLocation = null;
        }
        
        /// <summary></summary>
        /// <param name="permissions"></param>
        /// <param name="results"></param>
        public void OnPermissionRequestCompleted(string[] permissions, Permission[] results)
        {
            authorizer?.Set();
        }
        
        /// <summary></summary>
        /// <param name="provider"></param>
        public void OnProviderDisabled(string provider)
        {
        }
        
        /// <summary></summary>
        /// <param name="provider"></param>
        public void OnProviderEnabled(string provider)
        {
        }
        
        /// <summary></summary>
        /// <param name="provider"></param>
        /// <param name="status"></param>
        /// <param name="extras"></param>
        public void OnStatusChanged(string provider, Availability status, Bundle extras)
        {
        }

        /// <summary>
        /// Requests access to the device's geolocation service.
        /// </summary>
        /// <returns><c>true</c> if access is granted; otherwise, <c>false</c>.</returns>
        public Task<bool> RequestAccessAsync()
        {
            return Task.Run(() =>
            {
                if (locator.GetProviders(criteria, true) == null)
                {
                    return false;
                }
            
                if (Application.MainActivity.PackageManager.CheckPermission(Manifest.Permission.AccessFineLocation, Application.MainActivity.PackageName) == Permission.Granted)
                {
                    return true;
                }
                
                if (Application.MainActivity.PackageManager.CheckPermission(Manifest.Permission.AccessCoarseLocation, Application.MainActivity.PackageName) == Permission.Granted)
                {
                    return true;
                }
                
                if (Build.VERSION.SdkInt >= BuildVersionCodes.M)
                {
                    var appActivity = Application.MainActivity as AppActivity;
                    if (appActivity != null)
                    {
                        authorizer = new ManualResetEventSlim(false);
                        appActivity.RequestPermissions(new[] { Manifest.Permission.AccessCoarseLocation, Manifest.Permission.AccessFineLocation }, this);
                        authorizer.Wait();
                        authorizer = null;
                        
                        return appActivity.CheckSelfPermission(Manifest.Permission.AccessFineLocation) == Permission.Granted ||
                            appActivity.CheckSelfPermission(Manifest.Permission.AccessCoarseLocation) == Permission.Granted;
                    }
                }
                
                return false;
            });
        }
        
        private Coordinate BuildCoordinate(Location location)
        {
            if (location == null)
            {
                return null;
            }
            
            return new Coordinate(new DateTime(1970, 1, 1).AddMilliseconds(location.Time).ToLocalTime(), location.Latitude, location.Longitude,
                location.Altitude, location.HasBearing ? (double?)location.Bearing : null, location.HasSpeed ? (double?)location.Speed : null,
                location.HasAccuracy ? (double?)location.Accuracy : null, null);
        }
        
        private void StartListening()
        {
            if (isActive)
            {
                locator.RemoveUpdates(this);
            }
            
            locator.RequestLocationUpdates(double.IsNaN(updateInterval) ? 0 : (long)updateInterval,
                double.IsNaN(distanceThreshold) ? 0 : (float)distanceThreshold, criteria, this, Looper.MainLooper);
            
            isActive = true;
        }
    }
}

