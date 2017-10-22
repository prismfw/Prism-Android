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


using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Android.Views;
using Android.Views.Animations;
using Prism.Android.UI.Media;
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
        private static readonly ConditionalWeakTable<View, List<TouchTarget>> touchTargets = new ConditionalWeakTable<View, List<TouchTarget>>();

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

            int pointerId = e.GetPointerId(e.ActionIndex);
            var targets = touchTargets.GetOrCreateValue(view);
            var currentTarget = targets.FirstOrDefault(t => t.PointerId == pointerId);

            // Up or Cancel actions may have been lost.  If this is the beginning of the gesture, reset the target.
            if (e.Action == MotionEventActions.Down && currentTarget != null)
            {
                targets.Remove(currentTarget);
                currentTarget = null;
            }

            for (int i = view.ChildCount - 1; i >= 0; i--)
            {
                var child = view.GetChildAt(i);
                if (currentTarget != null && currentTarget.Target != child)
                {
                    continue;
                }

                if (!((child as INativeVisual)?.IsHitTestVisible ?? true))
                {
                    targets.Remove(currentTarget);
                    continue;
                }

                var e2 = MotionEvent.Obtain(e);
                e2.SetLocation(e.GetX() + view.ScrollX - child.Left, e.GetY() + view.ScrollY - child.Top);

                var transform = child.Animation as TransformAnimation ??
                    (child.Animation as AnimationSet)?.Animations.OfType<TransformAnimation>().FirstOrDefault();

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

                if (currentTarget != null)
                {
                    child.DispatchTouchEvent(e2);
                    if (e2.ActionMasked == MotionEventActions.Up || e2.ActionMasked == MotionEventActions.Cancel)
                    {
                        targets.Remove(currentTarget);
                    }
                    return true;
                }
                else if (e2.ActionMasked == MotionEventActions.Down && x >= 0 && x <= child.Width && y >= 0 && y <= child.Height)
                {
                    child.DispatchTouchEvent(e2);
                    targets.Add(new TouchTarget(child, pointerId));
                    return true;
                }

                e2.Recycle();
            }

            return false;
        }

        private class TouchTarget
        {
            public int PointerId { get; }

            public View Target { get; }

            public TouchTarget(View target, int pointerId)
            {
                Target = target;
                PointerId = pointerId;
            }
        }
    }
}

