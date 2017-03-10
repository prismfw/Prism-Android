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
using System.Linq;
using System.Reflection;
using Android.App;
using Prism.Native;

namespace Prism.Android
{
    /// <summary>
    /// Represents a platform initializer for Android.
    /// </summary>
    public sealed class AndroidInitializer : PlatformInitializer
    {
        private static Prism.Application application;
    
        private AndroidInitializer()
        {
        }

        /// <summary>
        /// Initializes the platform and loads the specified <see cref="Prism.Application"/> instance.
        /// </summary>
        /// <param name="appInstance">The application instance to be loaded.</param>
        /// <param name="activity">The activity instance that manages views and fragments.</param>
        public static void Initialize(Prism.Application appInstance, Activity activity)
        {
            if (activity == null)
            {
                throw new ArgumentNullException(nameof(activity));
            }
        
            application = appInstance;
            
            if (activity is AppActivity)
            {
                Application.MainActivity = activity;
                Initialize();
            }
            else
            {
                activity.StartActivity(typeof(AppActivity));
                activity.Finish();
            }
        }

        internal static void Initialize()
        {
            List<Assembly> appAssemblies = null;
            if (!HasInitialized)
            {
                appAssemblies = new List<Assembly>(AppDomain.CurrentDomain.GetAssemblies().Where(a => a.GetType() != typeof(System.Reflection.Emit.AssemblyBuilder)));

                var exeAssembly = Assembly.GetExecutingAssembly();
                FilterAssemblies(exeAssembly.GetName(), appAssemblies);
                appAssemblies.Insert(0, exeAssembly);
            }

            Initialize(application, appAssemblies?.ToArray());
            application = null;
        }
        
        private static void FilterAssemblies(AssemblyName name, ICollection<Assembly> loadedAssemblies)
        {
            var assembly = loadedAssemblies.FirstOrDefault(a => a.FullName == name.FullName);
            if (assembly == null) return;
            loadedAssemblies.Remove(assembly);
            foreach (var refAssembly in assembly.GetReferencedAssemblies())
            {
                FilterAssemblies(refAssembly, loadedAssemblies);
            }
        }
    }
}