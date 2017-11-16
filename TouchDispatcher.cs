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

            var targets = touchTargets.GetOrCreateValue(view);

            // Up or Cancel actions may have been lost.  If this is the beginning of the gesture, reset the targets.
            if (e.ActionMasked == MotionEventActions.Down)
            {
                targets.Clear();
            }

            // We separate each pointer in a move event so that the PointerMoved event can be fired for each of them.
            if ((e.ActionMasked == MotionEventActions.Move || e.ActionMasked == MotionEventActions.Cancel) && e.PointerCount > 1)
            {
                bool flag = false;
                for (int i = 0; i < e.PointerCount; i++)
                {
                    var props = new MotionEvent.PointerProperties();
                    e.GetPointerProperties(i, props);

                    var coords = new MotionEvent.PointerCoords();
                    e.GetPointerCoords(i, coords);

                    var e2 = MotionEvent.Obtain(e.DownTime, e.EventTime, e.Action, 1, new[] { props }, new[] { coords },
                        e.MetaState, e.ButtonState, e.XPrecision, e.YPrecision, e.DeviceId, e.EdgeFlags, e.Source, e.Flags);

                    if (InternalDispatchTouchEvent(view, e2, targets))
                    {
                        flag = true;
                    }

                    e2.Recycle();
                }

                return flag;
            }

            e = MotionEvent.Obtain(e);
            if (InternalDispatchTouchEvent(view, e, targets))
            {
                e.Recycle();
                return true;
            }
            else
            {
                e.Recycle();
                return false;
            }
        }

        private static bool InternalDispatchTouchEvent(ViewGroup view, MotionEvent e, List<TouchTarget> targets)
        {
            int pointerId = e.GetPointerId(e.ActionIndex);
            var currentTarget = targets.FirstOrDefault(t => t.PointerId == pointerId);

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

                e.OffsetLocation(view.ScrollX - child.Left, view.ScrollY - child.Top);

                var handled = false;
                var transform = child.Animation as TransformAnimation ??
                    (child.Animation as AnimationSet)?.Animations.OfType<TransformAnimation>().FirstOrDefault();

                if (transform != null && !transform.Matrix.IsIdentity)
                {
                    var tEvent = i > 0 ? MotionEvent.Obtain(e) : e;
                    var t = new Transformation();
                    transform.GetTransformation(0, t);

                    var m = new global::Android.Graphics.Matrix();
                    t.Matrix.Invert(m);
                    tEvent.Transform(m);

                    handled = DispatchTransformedTouchEvent(child, tEvent, currentTarget, targets);
                    if (tEvent != e)
                    {
                        tEvent.Recycle();
                    }
                }
                else
                {
                    handled = DispatchTransformedTouchEvent(child, e, currentTarget, targets);
                }

                if (handled)
                {
                    return true;
                }

                e.OffsetLocation(-(view.ScrollX - child.Left), -(view.ScrollY - child.Top));
            }

            return false;
        }

        private static bool DispatchTransformedTouchEvent(View child, MotionEvent e, TouchTarget currentTarget, List<TouchTarget> targets)
        {
            float x = e.GetX(e.ActionIndex);
            float y = e.GetY(e.ActionIndex);

            if (currentTarget != null)
            {
                if (e.ActionMasked == MotionEventActions.Move && x == currentTarget.X && y == currentTarget.Y)
                {
                    return true;
                }

                child.DispatchTouchEvent(e);
                if (e.ActionMasked == MotionEventActions.Up || e.ActionMasked == MotionEventActions.PointerUp ||
                    e.ActionMasked == MotionEventActions.Cancel)
                {
                    targets.Remove(currentTarget);
                }
                return true;
            }

            if ((e.ActionMasked == MotionEventActions.Down || e.ActionMasked == MotionEventActions.PointerDown) &&
                x >= 0 && x <= child.Width && y >= 0 && y <= child.Height)
            {
                targets.Add(new TouchTarget(child, e.GetPointerId(e.ActionIndex), x, y));
                child.DispatchTouchEvent(e);
                return true;
            }

            return false;
        }

        private class TouchTarget
        {
            public int PointerId { get; }

            public View Target { get; }

            public float X { get; set; }

            public float Y { get; set; }

            public TouchTarget(View target, int pointerId, float x, float y)
            {
                Target = target;
                PointerId = pointerId;
                X = x;
                Y = y;
            }
        }
    }
}

