/*
Copyright (C) 2016  Prism Framework Team

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
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Android;
using Android.App;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Util;
using Android.Views;

namespace Prism.Android
{
    /// <summary>
    /// Represents a utility for extracting colors and drawables from theme-based resources.
    /// </summary>
    public static class ResourceExtractor
    {
        private static readonly Type resourceClass;
        private static readonly Dictionary<string, int> resourceIds = new Dictionary<string, int>();

        static ResourceExtractor()
        {
            resourceClass = AppDomain.CurrentDomain.GetAssemblies().SelectMany(a => a.GetExportedTypes())
                .FirstOrDefault(t => t.Name == "Resource" && t.IsDefined(typeof(GeneratedCodeAttribute)));
        }
        
        /// <summary>
        /// Gets the color associated with the specified <see cref="Resource.Attribute"/> identifier.
        /// </summary>
        public static Color GetColor(int id, Activity activity = null)
        {
            return (activity ?? Application.MainActivity).Theme.ObtainStyledAttributes(Resource.Style.Theme, new int[] { id }).GetColor(0, 0);
        }
        
        /// <summary>
        /// Gets the color associated with the specified <see cref="Resource.Attribute"/> identifier.
        /// </summary>
        public static Color GetColor(int id, View view)
        {
#pragma warning disable 0618 // deprecated method is called for pre-lollipop devices
            return Build.VERSION.SdkInt >= BuildVersionCodes.Lollipop ?
                view.Resources.GetColor(id, Application.MainActivity.Theme) : view.Resources.GetColor(id);
#pragma warning restore 0618
        }
        
        /// <summary>
        /// Gets the drawable associated with the specified <see cref="Resource.Attribute"/> identifier.
        /// </summary>
        public static Drawable GetDrawable(int id, Activity activity = null)
        {
            var array = (activity ?? Application.MainActivity).Theme.ObtainStyledAttributes(Resource.Style.Theme, new int[] { id });
            var value = array?.PeekValue(0);
            if (value == null)
            {
                return null;
            }
            
            if (value.Type >= DataType.FirstColorInt && value.Type <= DataType.LastColorInt)
            {
                return new ColorDrawable(array.GetColor(0, 0));
            }
            
            try
            {
                return array.GetDrawable(0);
            }
            catch
            {
                return new ColorDrawable(array.GetColor(0, 0));
            }
        }
        
        /// <summary>
        /// Gets the drawable associated with the specified <see cref="Resource.Attribute"/> identifier.
        /// </summary>
        public static Drawable GetDrawable(int id, View view)
        {
#pragma warning disable 0618 // deprecated method is called for pre-lollipop devices
            return Build.VERSION.SdkInt >= BuildVersionCodes.Lollipop ?
                view.Resources.GetDrawable(id, Application.MainActivity.Theme) : view.Resources.GetDrawable(id);
#pragma warning restore 0618
        }
        
        /// <summary>
        /// Gets the resource identifier that corresponds to either of the specified resource names.
        /// </summary>
        /// <param name="names">The possible names of the resource whose identifier is to be retrieved.</param>
        public static int GetResourceId(params string[] names)
        {
            int id;
            for (int i = 0; i < names.Length; i++)
            {
                if (resourceIds.TryGetValue(names[i], out id))
                {
                    return id;
                }
            }
        
            if (resourceClass != null)
            {
                foreach (var field in resourceClass.GetNestedTypes(BindingFlags.DeclaredOnly | BindingFlags.Public).SelectMany(t => t.GetRuntimeFields()))
                {
                    int index = Array.IndexOf(names, field.Name);
                    if (index >= 0)
                    {
                        return (resourceIds[names[index]] = (int)field.GetValue(null));
                    }
                }
            }
            
            return 0;
        }
    }
}

