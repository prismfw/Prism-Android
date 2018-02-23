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
using Android.OS;
using Android.Runtime;
using Android.Views;
using Prism.Input;
using Prism.Native;
using Prism.Systems;

namespace Prism.Android.Input
{
    /// <summary>
    /// Represents an Android implementation of an <see cref="INativeTapGestureRecognizer"/>.
    /// </summary>
    [Preserve(AllMembers = true)]
    [Register(typeof(INativeTapGestureRecognizer))]
    public class TapGestureRecognizer : GestureDetector.SimpleOnGestureListener, INativeTapGestureRecognizer
    {
        /// <summary>
        /// Occurs when a double tap gesture is recognized.
        /// </summary>
        public event EventHandler<TappedEventArgs> DoubleTapped;

        /// <summary>
        /// Occurs when the value of a property is changed.
        /// </summary>
        public event EventHandler<FrameworkPropertyChangedEventArgs> PropertyChanged;

        /// <summary>
        /// Occurs when a right tap gesture (or long press gesture for touch input) is recognized.
        /// </summary>
        public event EventHandler<TappedEventArgs> RightTapped;

        /// <summary>
        /// Occurs when a single tap gesture is recognized.
        /// </summary>
        public event EventHandler<TappedEventArgs> Tapped;

        /// <summary>
        /// Gets or sets a value indicating whether double tap gestures should be recognized.
        /// </summary>
        public bool IsDoubleTapEnabled
        {
            get { return isDoubleTapEnabled; }
            set
            {
                if (value != isDoubleTapEnabled)
                {
                    isDoubleTapEnabled = value;
                    OnPropertyChanged(Prism.Input.TapGestureRecognizer.IsDoubleTapEnabledProperty);
                }
            }
        }
        private bool isDoubleTapEnabled;

        /// <summary>
        /// Gets or sets a value indicating whether right tap gestures (long press gestures for touch input) should be recognized.
        /// </summary>
        public bool IsRightTapEnabled
        {
            get { return Detector.IsLongpressEnabled; }
            set
            {
                if (value != Detector.IsLongpressEnabled)
                {
                    Detector.IsLongpressEnabled = value;
                    OnPropertyChanged(Prism.Input.TapGestureRecognizer.IsRightTapEnabledProperty);
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether single tap gestures should be recognized.
        /// </summary>
        public bool IsTapEnabled
        {
            get { return isTapEnabled; }
            set
            {
                if (value != isTapEnabled)
                {
                    isTapEnabled = value;
                    OnPropertyChanged(Prism.Input.TapGestureRecognizer.IsTapEnabledProperty);
                }
            }
        }
        private bool isTapEnabled;

        /// <summary>
        /// Gets or sets the maximum distance a touch can move before an active right tap gesture is aborted.
        /// </summary>
        public double MaxMovementTolerance { get; set; } = 10 * Device.Current.DisplayScale;

        /// <summary>
        /// Gets the detector responsible for recognizing the gesture.
        /// </summary>
        protected GestureDetector Detector { get; }

        private bool isLongPressActive;
        private float longPressAnchorX, longPressAnchorY;

        /// <summary>
        /// Initializes a new instance of the <see cref="TapGestureRecognizer"/> class.
        /// </summary>
        public TapGestureRecognizer()
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

                if (Build.VERSION.SdkInt >= BuildVersionCodes.M)
                {
                    view.GenericMotion -= OnGenericMotionEvent;
                }
            }
        }

        /// <summary></summary>
        /// <param name="e"></param>
        public override bool OnContextClick(MotionEvent e)
        {
            if (Detector.IsLongpressEnabled)
            {
                RightTapped(this, new TappedEventArgs(e.GetToolType(e.ActionIndex).GetPointerType(),
                    new Point(e.GetX().GetScaledDouble(), e.GetY().GetScaledDouble()), 1));
            }

            return base.OnContextClick(e);
        }

        /// <summary></summary>
        /// <param name="e"></param>
        public override bool OnDoubleTap(MotionEvent e)
        {
            if (isDoubleTapEnabled)
            {
                DoubleTapped(this, new TappedEventArgs(e.GetToolType(e.ActionIndex).GetPointerType(),
                    new Point(e.GetX().GetScaledDouble(), e.GetY().GetScaledDouble()), 2));
            }
            else if (isTapEnabled)
            {
                Tapped(this, new TappedEventArgs(e.GetToolType(e.ActionIndex).GetPointerType(),
                    new Point(e.GetX().GetScaledDouble(), e.GetY().GetScaledDouble()), 1));
            }

            return base.OnDoubleTap(e);
        }

        /// <summary></summary>
        /// <param name="e"></param>
        public override void OnLongPress(MotionEvent e)
        {
            if (e.GetToolType(e.ActionIndex) == MotionEventToolType.Finger)
            {
                longPressAnchorX = e.GetX();
                longPressAnchorY = e.GetY();
                
                isLongPressActive = true;
            }

            base.OnLongPress(e);
        }

        /// <summary></summary>
        /// <param name="e"></param>
        public override bool OnSingleTapUp(MotionEvent e)
        {
            if (isTapEnabled)
            {
                Tapped(this, new TappedEventArgs(e.GetToolType(e.ActionIndex).GetPointerType(),
                    new Point(e.GetX().GetScaledDouble(), e.GetY().GetScaledDouble()), 1));
            }

            return base.OnSingleTapUp(e);
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

                if (Build.VERSION.SdkInt >= BuildVersionCodes.M)
                {
                    view.GenericMotion -= OnGenericMotionEvent;
                    view.GenericMotion += OnGenericMotionEvent;
                }
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

        private void OnGenericMotionEvent(object sender, View.GenericMotionEventArgs e)
        {
            if (Detector.IsLongpressEnabled)
            {
                Detector.OnGenericMotionEvent(e.Event);
            }
        }

        private void OnTargetTouch(object sender, View.TouchEventArgs e)
        {
            e.Handled = false;

            if (isLongPressActive)
            {
                if (e.Event.ActionMasked == MotionEventActions.Up)
                {
                    RightTapped(this, new TappedEventArgs(e.Event.GetToolType(e.Event.ActionIndex).GetPointerType(),
                        new Point(e.Event.GetX().GetScaledDouble(), e.Event.GetY().GetScaledDouble()), 1));

                    isLongPressActive = false;
                }
                else if (e.Event.ActionMasked == MotionEventActions.Move)
                {
                    float x = Math.Abs(longPressAnchorX - e.Event.GetX());
                    float y = Math.Abs(longPressAnchorY - e.Event.GetY());
                    if (Math.Sqrt(x * x + y * y) > MaxMovementTolerance)
                    {
                        isLongPressActive = false;
                    }
                }
                else
                {
                    isLongPressActive = false;
                }
            }
            Detector.OnTouchEvent(e.Event);
        }
    }
}

