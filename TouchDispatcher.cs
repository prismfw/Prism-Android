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


using Android.Views;
using Android.Views.Animations;
using Prism.Native;

namespace Prism.Android
{
    /// <summary>
    /// Defines an object that dispatches touch events.
    /// </summary>
    public interface ITouchDispatcher
    {
        /// <summary>
        /// Gets a value indicating whether this instance is currently dispatching touch events.
        /// </summary>
        bool IsDispatching { get; }
    }
    
    /// <summary>
    /// Provides methods for the <see cref="ITouchDispatcher"/> interface.
    /// </summary>
    public static class TouchDispatcherExtensions
    {
        /// <summary>
        /// Dispatchs the provided touch event to the children of the view.
        /// </summary>
        /// <param name="dispatcher">The dispatcher.</param>
        /// <param name="e">The touch event.</param>
        public static bool DispatchTouchEventToChildren(this ITouchDispatcher dispatcher, MotionEvent e)
        {
            var view = dispatcher as ViewGroup;
            if (view == null)
            {
                return (dispatcher as View)?.DispatchTouchEvent(e) ?? false;
            }
            
            for (int i = view.ChildCount - 1; i >= 0; i--)
            {
                var child = view.GetChildAt(i);
                var e2 = MotionEvent.Obtain(e);
                e2.SetLocation(e.GetX() - child.Left, e.GetY() - child.Top);
                
                var transform = child.Animation as UI.Media.TransformAnimation;
                if (transform != null && !transform.Matrix.IsIdentity)
                {
                    var t = new Transformation();
                    transform.GetTransformation(0, t);
                    
                    var m = new global::Android.Graphics.Matrix();
                    t.Matrix.Invert(m);
                    e2.Transform(m);
                }
                
                float x = e2.GetX();
                float y = e2.GetY();
                
                if (x >=  0 && x <= child.Width && y >= 0 && y <= child.Height && ((child as INativeVisual)?.IsHitTestVisible ?? true))
                {
                    child.DispatchTouchEvent(e2);
                    return true;
                }
                
                e2.Recycle();
            }
            
            return false;
        }
    }
}

