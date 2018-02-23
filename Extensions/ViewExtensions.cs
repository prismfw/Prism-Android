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
using System.Collections.Generic;
using Android.Views;
using Prism.Android.UI;

namespace Prism.Android
{
    /// <summary>
    /// Provides methods for traversing the Android view hierarchy.
    /// </summary>
    public static class ViewExtensions
    {
        /// <summary>
        /// Walks the view hierarchy and returns the child view that satisfies the specified condition.
        /// </summary>
        /// <param name="viewGroup">The view group.</param>
        /// <param name="predicate">The condition to check each child for.</param>
        public static View GetChild(this ViewGroup viewGroup, Func<View, bool> predicate = null)
        {
            if (viewGroup != null)
            {
                for (int i = 0; i < viewGroup.ChildCount; i++)
                {
                    var child = viewGroup.GetChildAt(i);
                    if (predicate == null || predicate.Invoke(child))
                    {
                        return child;
                    }

                    child = (child as ViewGroup)?.GetChild(predicate);
                    if (child != null)
                    {
                        return child;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Walks the view hierarchy and returns the child view that is of type T and satisfies the specified condition.
        /// </summary>
        /// <typeparam name="T">The type of child to search for.</typeparam>
        /// <param name="viewGroup">The view group.</param>
        /// <param name="predicate">The condition to check each child for.</param>
        public static T GetChild<T>(this ViewGroup viewGroup, Func<T, bool> predicate = null)
            where T : class
        {
            if (viewGroup != null)
            {
                for (int i = 0; i < viewGroup.ChildCount; i++)
                {
                    var child = viewGroup.GetChildAt(i);
                    var tView = child as T;
                    if (tView != null && (predicate == null || predicate.Invoke(tView)))
                    {
                        return tView;
                    }

                    tView = (child as ViewGroup)?.GetChild<T>(predicate);
                    if (tView != null)
                    {
                        return tView;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Walks the view hierarchy and returns all child views that are of type T and satisfy the specified condition.
        /// </summary>
        /// <typeparam name="T">The type of children to search for.</typeparam>
        /// <param name="viewGroup">The view group.</param>
        /// <param name="predicate">The condition to check each child for.</param>
        public static T[] GetChildren<T>(this ViewGroup viewGroup, Func<T, bool> predicate = null)
            where T : class
        {
            var children = new List<T>();
            if (viewGroup != null)
            {
                for (int i = 0; i < viewGroup.ChildCount; i++)
                {
                    var child = viewGroup.GetChildAt(i);
                    var tView = child as T;
                    if (tView != null && (predicate == null || predicate.Invoke(tView)))
                    {
                        children.Add(tView);
                    }

                    children.AddRange((child as ViewGroup).GetChildren<T>(predicate));
                }
            }

            return children.ToArray();
        }

        /// <summary>
        /// Walks the view hierarchy and returns the parent view that is of type T and satisfies the specified condition.
        /// </summary>
        /// <typeparam name="T">The type of parent to search for.</typeparam>
        /// <param name="view">The view.</param>
        /// <param name="predicate">The condition to check each parent for.</param>
        public static T GetParent<T>(this View view, Func<T, bool> predicate = null)
            where T : class
        {
            var fragment = (view as IFragmentView)?.Fragment as T;
            if (fragment != null && (predicate == null || predicate.Invoke(fragment)))
            {
                return fragment;
            }
        
            if (view?.Parent != null)
            {
                var tView = view.Parent as T;
                if (tView != null && (predicate == null || predicate.Invoke(tView)))
                {
                    return tView;
                }

                tView = (view.Parent as View)?.GetParent<T>(predicate);
                if (tView != null)
                {
                    return tView;
                }
            }

            return null;
        }
    }
}