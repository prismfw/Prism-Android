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
using Android.Runtime;
using Android.Views;
using Android.Views.Animations;
using Prism.Native;
using Prism.Systems;
using Prism.UI.Media;

namespace Prism.Android.UI.Media
{
    /// <summary>
    /// Represents an Android implementation of an <see cref="INativeTransform"/>.
    /// </summary>
    [Preserve(AllMembers = true)]
    [Register(typeof(INativeTransform))]
    public class Transform : INativeTransform
    {
        /// <summary>
        /// Gets or sets the affine transformation matrix.
        /// </summary>
        public Matrix Value
        {
            get { return matrix; }
            set
            {
                matrix = value;
                
                for (int i = 0; i < views.Count; i++)
                {
                    var view = views[i].Target as View;
                    if (view == null)
                    {
                        views.RemoveAt(i--);
                    }
                    else
                    {
                        SetAnimation(view);
                        view.RequestLayout();
                        view.Invalidate();
                    }
                }
            }
        }
        private Matrix matrix;
        
        private readonly List<WeakReference> views = new List<WeakReference>();
        
        /// <summary>
        /// Adds a view to the transformation listener.
        /// </summary>
        /// <param name="view">The view to be added.</param>
        public void AddView(View view)
        {
            views.Add(new WeakReference(view));
            SetAnimation(view);
            view.RequestLayout();
            view.Invalidate();
        }

        /// <summary>
        /// Removes a view from the transformation listener.
        /// </summary>
        /// <param name="view">The view to be removed.</param>
        public void RemoveView(View view)
        {
            if (views.RemoveAll(r => r.Target == view) > 0)
            {
                var currentSet = view.Animation as AnimationSet;
                if (currentSet != null)
                {
                    var newSet = new AnimationSet(true);
                    foreach (var anim in currentSet.Animations)
                    {
                        if (!(anim is TransformAnimation))
                        {
                            newSet.AddAnimation(anim);
                        }
                    }
                    
                    view.Animation = newSet.Animations.Count > 0 ? newSet : null;
                }
                
                view.RequestLayout();
                view.Invalidate();
            }
        }
        
        private void SetAnimation(View view)
        {
            var newSet = new AnimationSet(true);
            var currentSet = view.Animation as AnimationSet;
            if (currentSet != null)
            {
                foreach (var anim in currentSet.Animations)
                {
                    if (!(anim is TransformAnimation))
                    {
                        newSet.AddAnimation(anim);
                    }
                }
            }
            else if (view.Animation != null)
            {
                newSet.AddAnimation(view.Animation);
            }
            
            newSet.AddAnimation(new TransformAnimation(view, matrix));
            view.Animation = newSet;
        }
    }
    
    /// <summary>
    /// Represents an animation that performs a render transformation on a view.
    /// </summary>
    public class TransformAnimation : Animation
    {
        /// <summary>
        /// Gets the transformation matrix.
        /// </summary>
        public Matrix Matrix { get; }
        
        /// <summary>
        /// Gets the view that is being transformed.
        /// </summary>
        public View View { get; }
    
        /// <summary>
        /// Initializes a new instance of the <see cref="TransformAnimation"/> class.
        /// </summary>
        /// <param name="view">The view that is being transformed..</param>
        /// <param name="matrix">The transformation matrix..</param>
        public TransformAnimation(View view, Matrix matrix)
        {
            Matrix = matrix;
            View = view;
            Duration = 0;
            FillAfter = true;
            RepeatCount = Infinite;
            RepeatMode = RepeatMode.Restart;
        }
    
        /// <summary></summary>
        /// <param name="interpolatedTime"></param>
        /// <param name="t"></param>
        protected override void ApplyTransformation(float interpolatedTime, Transformation t)
        {
            base.ApplyTransformation(interpolatedTime, t);
            t.TransformationType = TransformationTypes.Matrix;
            t.Matrix.Reset();
            
            var width = (float)View.Width / 2;
            var height = (float)View.Height / 2;
            
            // I pulled this code from some dark pit of the interwebs, and in all likelihood it doesn't work in a number of scenarios.
            // It does, however, seem to work in my limited tests, so it's staying here until someone replaces it with better code.
            var matrix = Matrix;
            matrix.OffsetX = (matrix.OffsetX * Device.Current.DisplayScale) - matrix.M11 * width - matrix.M21 * height + width;
            matrix.OffsetY = (matrix.OffsetY * Device.Current.DisplayScale) - matrix.M12 * width - matrix.M11 * height + height;
            t.Matrix.Set(matrix.GetMatrix());
        }
    }
}

