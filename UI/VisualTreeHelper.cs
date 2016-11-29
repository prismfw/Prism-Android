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
using Android.App;
using Android.Runtime;
using Android.Support.V7.Widget;
using Android.Views;
using Prism.Native;

namespace Prism.Android.UI
{
    /// <summary>
    /// Represents an Android implementation of an <see cref="INativeVisualTreeHelper"/>.
    /// </summary>
    [Preserve(AllMembers = true)]
    [Register(typeof(INativeVisualTreeHelper), IsSingleton = true)]
    public class VisualTreeHelper : INativeVisualTreeHelper
    {
        /// <summary>
        /// Returns the number of children in the specified object's child collection.
        /// </summary>
        /// <param name="reference">The parent object.</param>
        /// <returns>The number of children in the parent object's child collection.</returns>
        public int GetChildrenCount(object reference)
        {
            var view = reference as ViewGroup;
            if (view == null)
            {
                var window = reference as INativeWindow;
                if (window != null)
                {
                    return window.Content == null ? 0 : 1;
                }

                var fragment = reference as Fragment;
                if (fragment != null)
                {
                    int id = 1;
                    while (fragment.ChildFragmentManager.FindFragmentById(id) != null)
                    {
                        id++;
                    }

                    return id - 1 + (fragment.View == null ? 0 : 1);
                }
            }
            
            var vsh = reference as Controls.ViewStackHeader;
            if (vsh != null)
            {
                return vsh.ChildCount + (vsh.Menu == null ? 0 : 1);
            }

            return view == null ? 0 : view.ChildCount;
        }

        /// <summary>
        /// Returns the child that is located at the specified index in the child collection of the specified object.
        /// </summary>
        /// <param name="reference">The parent object.</param>
        /// <param name="childIndex">The zero-based index of the child to return.</param>
        /// <returns>The child at the specified index.</returns>
        public object GetChild(object reference, int childIndex)
        {
            var view = reference as ViewGroup;
            if (view == null)
            {
                var window = reference as INativeWindow;
                if (window != null && childIndex == 0)
                {
                    return window.Content;
                }

                var fragment = reference as Fragment;
                if (fragment != null)
                {
                    if (fragment.View == null)
                    {
                        childIndex++;
                    }
                    else if (childIndex == 0)
                    {
                        return fragment.View;
                    }

                    return fragment.ChildFragmentManager.FindFragmentById(childIndex);
                }
                else
                {
                    view = (reference as RecyclerView.ViewHolder)?.ItemView as ViewGroup;
                }
            }
            
            var vsh = reference as Controls.ViewStackHeader;
            if (vsh != null)
            {
                return childIndex == vsh.ChildCount ? (object)vsh.Menu : vsh.GetChildAt(childIndex);
            }

            return view == null ? null : view.GetChildAt(childIndex);
        }

        /// <summary>
        /// Returns the parent of the specified object.
        /// </summary>
        /// <param name="reference">The child object.</param>
        /// <returns>The parent.</returns>
        public object GetParent(object reference)
        {
            var window = ObjectRetriever.GetNativeObject(Prism.UI.Window.Current) as INativeWindow;
            if (window != null && window.Content == reference)
            {
                return window;
            }

            var view = reference as View;
            if (view == null)
            {
                view = (reference as RecyclerView.ViewHolder)?.ItemView;
            }

            if (view != null)
            {
                var fv = view as IFragmentView;
                if (fv != null)
                {
                    return fv.Fragment;
                }

                int? id = (view.Parent as View)?.Id;
                if (id.HasValue && id.Value > 0)
                {
                    var fragment = Application.MainActivity.FragmentManager.FindFragmentById(id.Value);
                    if (fragment != null && fragment.View == view)
                    {
                        return fragment;
                    }
                }

                return view.Parent;
            }

            var frag = reference as Fragment;
            if (frag != null)
            {
                return frag.ParentFragment ?? (object)frag.Activity?.FindViewById(frag.Id);
            }

            return (reference as Controls.ActionMenu)?.Parent;
        }
    }
}