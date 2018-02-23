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
using Android.Runtime;
using Android.Views;
using Prism.Input;
using Prism.Native;
using Prism.Systems;

namespace Prism.Android.Input
{
    /// <summary>
    /// Represents an Android implementation of an <see cref="INativeHoldingGestureRecognizer"/>.
    /// </summary>
    [Preserve(AllMembers = true)]
    [Register(typeof(INativeHoldingGestureRecognizer))]
    public class HoldingGestureRecognizer : GestureDetector.SimpleOnGestureListener, INativeHoldingGestureRecognizer
    {
        /// <summary>
        /// Occurs when a holding gesture is started, completed, or canceled.
        /// </summary>
        public event EventHandler<HoldingEventArgs> Holding;

        /// <summary>
        /// Occurs when the value of a property is changed.
        /// </summary>
        public event EventHandler<FrameworkPropertyChangedEventArgs> PropertyChanged;

        /// <summary>
        /// Gets or sets the maximum distance a touch can move before the gesture is canceled.
        /// </summary>
        public double MaxMovementTolerance { get; set; } = 10 * Device.Current.DisplayScale;

        /// <summary>
        /// Gets the detector responsible for recognizing the gesture.
        /// </summary>
        protected GestureDetector Detector { get; }

        private bool isActive;
        private float anchorX, anchorY;

        /// <summary>
        /// Initializes a new instance of the <see cref="HoldingGestureRecognizer"/> class.
        /// </summary>
        public HoldingGestureRecognizer()
        {
            Detector = new GestureDetector(this);
        }

        /// <summary>
        /// Removes the specified object as the target of the gesture recognizer.
        /// </summary>
        /// <param name="target">The object to clear as the target.</param>
        public void ClearTarget(object target)
        {
            var view = target as View;
            if (view != null)
            {
                view.Touch -= OnTargetTouch;
            }
        }

        /// <summary></summary>
        /// <param name="e"></param>
        public override void OnLongPress(MotionEvent e)
        {
            if (!isActive)
            {
                anchorX = e.GetX();
                anchorY = e.GetY();

                OnHolding(e, HoldingState.Started);
                isActive = true;
            }

            base.OnLongPress(e);
        }

        /// <summary>
        /// Sets the specified object as the target of the gesture recognizer.
        /// </summary>
        /// <param name="target">The object to set as the target.</param>
        public void SetTarget(object target)
        {
            var view = target as View;
            if (view != null)
            {
                view.Touch -= OnTargetTouch;
                view.Touch += OnTargetTouch;
            }
        }

        /// <summary>
        /// Called when a property value is changed.
        /// </summary>
        /// <param name="pd">A property descriptor describing the property whose value has been changed.</param>
        protected virtual void OnPropertyChanged(PropertyDescriptor pd)
        {
            PropertyChanged(this, new FrameworkPropertyChangedEventArgs(pd));
        }

        private void OnHolding(MotionEvent e, HoldingState state)
        {
            var pointerType = e.GetToolType(e.ActionIndex).GetPointerType();

            // coincides with UWP behavior for touch input
            double x = (state == HoldingState.Canceled && pointerType == PointerType.Touch ? anchorX : e.GetX()).GetScaledDouble();
            double y = (state == HoldingState.Canceled && pointerType == PointerType.Touch ? anchorY : e.GetY()).GetScaledDouble();

            Holding(this, new HoldingEventArgs(pointerType, new Point(x, y), state));
            isActive = false;
        }

        private void OnTargetTouch(object sender, View.TouchEventArgs e)
        {
            e.Handled = false;

            if (isActive)
            {
                if (e.Event.ActionMasked == MotionEventActions.Up)
                {
                    OnHolding(e.Event, HoldingState.Completed);
                }
                else if (e.Event.ActionMasked == MotionEventActions.Move)
                {
                    float x = Math.Abs(anchorX - e.Event.GetX());
                    float y = Math.Abs(anchorY - e.Event.GetY());
                    if (Math.Sqrt(x * x + y * y) > MaxMovementTolerance)
                    {
                        OnHolding(e.Event, HoldingState.Canceled);
                    }
                }
                else
                {
                    OnHolding(e.Event, HoldingState.Canceled);
                }
            }
            else
            {
                Detector.OnTouchEvent(e.Event);
            }
        }
    }
}

