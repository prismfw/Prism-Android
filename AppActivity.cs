/*
Copyright (C) 2017  Prism Framework Team

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
using System.Collections.Generic;
using Android.App;
using Android.Content.PM;
using Android.Content.Res;
using Android.OS;
using Prism.Native;

namespace Prism.Android
{
    /// <summary>
    /// Represents a basic activity that responds to framework-generated events.
    /// </summary>
    [Activity(ConfigurationChanges = ConfigChanges.KeyboardHidden | ConfigChanges.Orientation | ConfigChanges.ScreenSize)]
    public class AppActivity : Activity
    {
        private readonly Dictionary<int, IPermissionRequestCallback> pendingPermissionRequests = new Dictionary<int, IPermissionRequestCallback>();
        
        /// <summary>
        /// Called when permissions have been requested of the user.
        /// </summary>
        /// <param name="requestCode">The request code for the specific request.</param>
        /// <param name="permissions">The names of the permissions that have beeen requested.</param>
        /// <param name="grantResults">The results of each permission request.</param>
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
        {
            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            
            IPermissionRequestCallback callback;
            if (pendingPermissionRequests.TryGetValue(requestCode, out callback))
            {
                pendingPermissionRequests.Remove(requestCode);
            }
            
            callback?.OnPermissionRequestCompleted(permissions, grantResults);
        }
        
        /// <summary>
        /// Requests from the user the specified permissions.
        /// </summary>
        /// <param name="permissions">The names of the permissions to request.</param>
        /// <param name="callback">A callback to call when the request is complete.</param>
        public void RequestPermissions(string[] permissions, IPermissionRequestCallback callback)
        {
            if (permissions == null)
            {
                throw new ArgumentNullException(nameof(permissions));
            }
        
            int requestCode = 0;
            foreach (var permission in permissions)
            {
                requestCode ^= permission.GetHashCode();
            }
        
            RequestPermissions(permissions, Math.Abs(requestCode), callback);
        }
        
        /// <summary>
        /// Requests from the user the specified permissions.
        /// </summary>
        /// <param name="permissions">The names of the permissions to request.</param>
        /// <param name="requestCode">The request code to use for the request.</param>
        /// <param name="callback">A callback to call when the request is complete.</param>
        public void RequestPermissions(string[] permissions, int requestCode, IPermissionRequestCallback callback)
        {
            if (permissions == null)
            {
                throw new ArgumentNullException(nameof(permissions));
            }
        
            pendingPermissionRequests[requestCode] = callback;
            RequestPermissions(permissions, requestCode);
        }
        
        /// <summary>
        /// Sets the activity content to an explicit view.
        /// </summary>
        /// <param name="view">The desired content to display.</param>
        public override void SetContentView(global::Android.Views.View view)
        {
            base.SetContentView(view);
            if ((view as INativeContentView)?.Background == null)
            {
                view.Background = Android.Resources.GetDrawable(null, global::Android.Resource.Attribute.WindowBackground);
            }
        }
        
        /// <summary>
        /// Called on creation of the activity.
        /// </summary>
        /// <param name="savedInstanceState">Saved instance state.</param>
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            
            Android.Application.MainActivity = this;
            AndroidInitializer.Initialize();
        }
        
        /// <summary>
        /// Called by the system when the device configuration changes while your activity is running.
        /// </summary>
        /// <param name="newConfig"></param>
        public override void OnConfigurationChanged(Configuration newConfig)
        {
            base.OnConfigurationChanged(newConfig);
            (ObjectRetriever.GetNativeObject(Prism.UI.Window.Current) as UI.Window)?.OnConfigurationChanged(newConfig);
        }
    }
}

