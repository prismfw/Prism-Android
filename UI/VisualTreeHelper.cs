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
using System.Linq;
using Android.App;
using Android.Runtime;
using Android.Views;
using Prism.Android.UI.Controls;
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
            int count = 0;
            var vto = reference as IVisualTreeObject;
            if (vto != null)
            {
                count = vto.Children?.Length ?? 0;
            }

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
                        count++;
                    }

                    var viewStack = reference as INativeViewStack;
                    if (viewStack != null)
                    {
                        count += viewStack.Views.Count(o => o != viewStack.CurrentView);
                    }

                    return count + (fragment.View == null ? 0 : 1);
                }
            }

            return (view == null ? 0 : view.ChildCount) + count;
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

                    object child = fragment.ChildFragmentManager.FindFragmentById(childIndex);
                    if (child == null)
                    {
                        var viewStack = reference as INativeViewStack;
                        if (viewStack != null)
                        {
                            int id = childIndex - 1;
                            while (fragment.ChildFragmentManager.FindFragmentById(id) == null && --id > 0) ;

                            childIndex -= (id + 1);
                            child = viewStack.Views.Where(o => o != viewStack.CurrentView).ElementAtOrDefault(childIndex);
                            if (child == null)
                            {
                                childIndex -= viewStack.Views.Count(o => o != viewStack.CurrentView);
                                child = (reference as IVisualTreeObject)?.Children?.ElementAtOrDefault(childIndex);
                            }
                        }
                    }

                    return child;
                }
                else
                {
                    view = (reference as ListBox.ListBoxViewHolder)?.ItemView as ViewGroup;
                }
            }

            if (view == null)
            {
                return (reference as IVisualTreeObject)?.Children?.ElementAtOrDefault(childIndex);
            }

            if (childIndex < view.ChildCount)
            {
                return view.GetChildAt(childIndex);
            }

            var vto = reference as IVisualTreeObject;
            return vto?.Children?.ElementAtOrDefault(childIndex - view.ChildCount);
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

            var vto = reference as IVisualTreeObject;
            if (vto?.Parent != null)
            {
                return vto.Parent;
            }

            var view = reference as View;
            if (view == null)
            {
                view = (reference as ListBox.ListBoxViewHolder)?.ItemView;
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
                if (frag.ParentFragment != null && frag.Activity == frag.ParentFragment.Activity)
                {
                    return frag.ParentFragment;
                }

                view = frag.Activity?.FindViewById(frag.Id);
                if (view != null)
                {
                    return view;
                }
            }

            return (reference as IViewStackChild)?.ViewStack;
        }
    }
}