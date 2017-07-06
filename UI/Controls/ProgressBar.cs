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
using Android;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Prism.Input;
using Prism.Native;
using Prism.Systems;
using Prism.UI;
using Prism.UI.Controls;
using Prism.UI.Media;

namespace Prism.Android.UI.Controls
{
    /// <summary>
    /// Represents an Android implementation of an <see cref="INativeProgressBar"/>.
    /// </summary>
    [Preserve(AllMembers = true)]
    [Register(typeof(INativeProgressBar))]
    public class ProgressBar : global::Android.Widget.ProgressBar, INativeProgressBar
    {
        /// <summary>
        /// Occurs when this instance has been attached to the visual tree and is ready to be rendered.
        /// </summary>
        public event EventHandler Loaded;
        
        /// <summary>
        /// Occurs when the system loses track of the pointer for some reason.
        /// </summary>
        public event EventHandler<PointerEventArgs> PointerCanceled;
        
        /// <summary>
        /// Occurs when the pointer has moved while over the element.
        /// </summary>
        public event EventHandler<PointerEventArgs> PointerMoved;

        /// <summary>
        /// Occurs when the pointer has been pressed while over the element.
        /// </summary>
        public event EventHandler<PointerEventArgs> PointerPressed;

        /// <summary>
        /// Occurs when the pointer has been released while over the element.
        /// </summary>
        public event EventHandler<PointerEventArgs> PointerReleased;

        /// <summary>
        /// Occurs when the value of a property is changed.
        /// </summary>
        public event EventHandler<FrameworkPropertyChangedEventArgs> PropertyChanged;

        /// <summary>
        /// Occurs when this instance has been detached from the visual tree.
        /// </summary>
        public event EventHandler Unloaded;

        /// <summary>
        /// Gets or sets a value indicating whether animations are enabled for this instance.
        /// </summary>
        public bool AreAnimationsEnabled
        {
            get { return areAnimationsEnabled; }
            set
            {
                if (value != areAnimationsEnabled)
                {
                    areAnimationsEnabled = value;
                    OnPropertyChanged(Visual.AreAnimationsEnabledProperty);
                }
            }
        }
        private bool areAnimationsEnabled;

        /// <summary>
        /// Gets or sets the method to invoke when this instance requests an arrangement of its children.
        /// </summary>
        public ArrangeRequestHandler ArrangeRequest { get; set; }
        
        /// <summary>
        /// Gets or sets the background of the progress bar.
        /// </summary>
        public new Brush Background
        {
            get { return background; }
            set
            {
                if (value != background)
                {
                    background = value;
                    
                    var drawable = (ProgressDrawable as LayerDrawable)?.FindDrawableByLayerId(Resource.Id.Background);
                    var scb = background as SolidColorBrush;
                    if (scb != null)
                    {
                        drawable?.SetColorFilter(scb.Color.GetColor(), PorterDuff.Mode.SrcIn);
                    }
                    else
                    {
                        drawable?.ClearColorFilter();
                        if (background != null)
                        {
                            Prism.Utilities.Logger.Warn("ProgressBar.Background on Android only supports instances of SolidColorBrush.");
                        }
                    }
                    
                    OnPropertyChanged(Prism.UI.Controls.ProgressBar.BackgroundProperty);
                }
            }
        }
        private Brush background;

        /// <summary>
        /// Gets or sets the <see cref="Brush"/> to apply to the foreground content of the indicator.
        /// </summary>
        public new Brush Foreground
        {
            get { return foreground; }
            set
            {
                if (value != foreground)
                {
                    foreground = value;
                    
                    var drawable = (ProgressDrawable as LayerDrawable)?.FindDrawableByLayerId(Resource.Id.Progress);
                    var scb = foreground as SolidColorBrush;
                    if (scb != null)
                    {
                        drawable?.SetColorFilter(scb.Color.GetColor(), PorterDuff.Mode.SrcIn);
                    }
                    else
                    {
                        drawable?.ClearColorFilter();
                        if (foreground != null)
                        {
                            Prism.Utilities.Logger.Warn("ProgressBar.Foreground on Android only supports instances of SolidColorBrush.");
                        }
                    }
                    
                    OnPropertyChanged(Prism.UI.Controls.ProgressBar.ForegroundProperty);
                }
            }
        }
        private Brush foreground;

        /// <summary>
        /// Gets or sets a <see cref="Rectangle"/> that represents the size and position of the element relative to its parent container.
        /// </summary>
        public Rectangle Frame { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance can be considered a valid result for hit testing.
        /// </summary>
        public bool IsHitTestVisible
        {
            get { return isHitTestVisible; }
            set
            {
                if (value != isHitTestVisible)
                {
                    isHitTestVisible = value;
                    OnPropertyChanged(Visual.IsHitTestVisibleProperty);
                }
            }
        }
        private bool isHitTestVisible = true;

        /// <summary>
        /// Gets a value indicating whether this instance has been loaded and is ready for rendering.
        /// </summary>
        public bool IsLoaded { get; private set; }

        /// <summary>
        /// Gets or sets the method to invoke when this instance requests a measurement of itself and its children.
        /// </summary>
        public MeasureRequestHandler MeasureRequest { get; set; }

        /// <summary>
        /// Gets or sets the level of opacity for the element.
        /// </summary>
        public double Opacity
        {
            get { return Alpha; }
            set
            {
                if (value != Alpha)
                {
                    Alpha = (float)value;
                    OnPropertyChanged(Element.OpacityProperty);
                }
            }
        }
        
        /// <summary>
        /// Gets or sets the level of progress as a value between 0.0 and 1.0.
        /// </summary>
        public new double Progress
        {
            get { return base.Progress / (double)Max; }
            set
            {
                var progress = (int)(value * Max);
                if (progress != base.Progress)
                {
                    if (Build.VERSION.SdkInt >= BuildVersionCodes.N)
                    {
                        SetProgress(progress, areAnimationsEnabled);
                    }
                    else
                    {
                        base.Progress = progress;
                    }
                    
                    OnPropertyChanged(Prism.UI.Controls.ProgressBar.ProgressProperty);
                }
            }
        }

        /// <summary>
        /// Gets or sets transformation information that affects the rendering position of this instance.
        /// </summary>
        public INativeTransform RenderTransform
        {
            get { return renderTransform; }
            set
            {
                if (value != renderTransform)
                {
                    (renderTransform as Media.Transform)?.RemoveView(this);
                    renderTransform = value;
                    
                    var transform = renderTransform as Media.Transform;
                    if (transform == null)
                    {
                        Animation = renderTransform as global::Android.Views.Animations.Animation;
                    }
                    else
                    {
                        transform.AddView(this);
                    }

                    OnPropertyChanged(Visual.RenderTransformProperty);
                }
            }
        }
        private INativeTransform renderTransform;

        /// <summary>
        /// Gets or sets the visual theme that should be used by this instance.
        /// </summary>
        public Theme RequestedTheme { get; set; }

        /// <summary>
        /// Gets or sets the display state of the element.
        /// </summary>
        public new Visibility Visibility
        {
            get { return base.Visibility.GetVisibility(); }
            set
            {
                var visibility = value.GetViewStates();
                if (visibility != base.Visibility)
                {
                    base.Visibility = visibility;
                    OnPropertyChanged(Element.VisibilityProperty);
                }
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProgressBar"/> class.
        /// </summary>
        public ProgressBar()
            : base(Application.MainActivity, null, Resource.Attribute.ProgressBarStyleHorizontal)
        {
            Max = 1000;
        }

        /// <summary></summary>
        /// <param name="e"></param>
        public override bool DispatchTouchEvent(MotionEvent e)
        {
            var parent = Parent as ITouchDispatcher;
            if (parent == null || parent.IsDispatching)
            {
                return base.DispatchTouchEvent(e);
            }

            return false;
        }

        /// <summary>
        /// Invalidates the arrangement of this instance's children.
        /// </summary>
        public void InvalidateArrange()
        {
            RequestLayout();
        }

        /// <summary>
        /// Invalidates the measurement of this instance and its children.
        /// </summary>
        public void InvalidateMeasure()
        {
            RequestLayout();
        }

        /// <summary>
        /// Measures the element and returns its desired size.
        /// </summary>
        /// <param name="constraints">The width and height that the element is not allowed to exceed.</param>
        public Size Measure(Size constraints)
        {
            int width = MeasuredWidth;
            int height = MeasuredHeight;

            var widthSpec = (int)Math.Min(int.MaxValue, constraints.Width * Device.Current.DisplayScale);
            var heightSpec = (int)Math.Min(int.MaxValue, constraints.Height * Device.Current.DisplayScale);
            base.OnMeasure(MeasureSpec.MakeMeasureSpec(widthSpec, MeasureSpecMode.AtMost),
                MeasureSpec.MakeMeasureSpec(heightSpec, MeasureSpecMode.AtMost));

            var size = new Size(MeasuredWidth, MeasuredHeight) / Device.Current.DisplayScale;
            SetMeasuredDimension(width, height);

            return new Size(Math.Min(constraints.Width, size.Width), Math.Min(constraints.Height, size.Height));
        }
        
        /// <summary></summary>
        /// <param name="e"></param>
        public override bool OnTouchEvent(MotionEvent e)
        {
            if (!isHitTestVisible)
            {
                return false;
            }
            
            if (e.Action == MotionEventActions.Cancel)
            {
                PointerCanceled(this, e.GetPointerEventArgs(this));
            }
            if (e.Action == MotionEventActions.Down)
            {
                PointerPressed(this, e.GetPointerEventArgs(this));
            }
            if (e.Action == MotionEventActions.Move)
            {
                PointerMoved(this, e.GetPointerEventArgs(this));
            }
            if (e.Action == MotionEventActions.Up)
            {
                PointerReleased(this, e.GetPointerEventArgs(this));
            }
            return base.OnTouchEvent(e);
        }

        /// <summary>
        /// This is called when the view is attached to a window.
        /// </summary>
        protected override void OnAttachedToWindow()
        {
            base.OnAttachedToWindow();
            OnLoaded();
        }

        /// <summary>
        /// This is called when the view is detached from a window.
        /// </summary>
        protected override void OnDetachedFromWindow()
        {
            base.OnDetachedFromWindow();
            OnUnloaded();
        }

        /// <summary>
        /// Called from layout when this view should assign a size and position to each of its children.
        /// </summary>
        /// <param name="changed"></param>
        /// <param name="left">Left position, relative to parent.</param>
        /// <param name="top">Top position, relative to parent.</param>
        /// <param name="right">Right position, relative to parent.</param>
        /// <param name="bottom">Bottom position, relative to parent.</param>
        protected override void OnLayout(bool changed, int left, int top, int right, int bottom)
        {
            ArrangeRequest(false, null);

            Left = (int)Math.Ceiling(Frame.Left * Device.Current.DisplayScale);
            Top = (int)Math.Ceiling(Frame.Top * Device.Current.DisplayScale);
            Right = (int)Math.Ceiling(Frame.Right * Device.Current.DisplayScale);
            Bottom = (int)Math.Ceiling(Frame.Bottom * Device.Current.DisplayScale);

            base.OnLayout(changed, Left, Top, Right, Bottom);
        }

        /// <summary>
        /// Measure the view and its content to determine the measured width and the measured height.
        /// </summary>
        /// <param name="widthMeasureSpec">Horizontal space requirements as imposed by the parent.</param>
        /// <param name="heightMeasureSpec">Vertical space requirements as imposed by the parent.</param>
        protected override void OnMeasure(int widthMeasureSpec, int heightMeasureSpec)
        {
            MeasureRequest(false, null);
            base.OnMeasure(widthMeasureSpec, heightMeasureSpec);
        }

        /// <summary>
        /// Called when a property value is changed.
        /// </summary>
        /// <param name="pd">A property descriptor describing the property whose value has been changed.</param>
        protected virtual void OnPropertyChanged(PropertyDescriptor pd)
        {
            PropertyChanged(this, new FrameworkPropertyChangedEventArgs(pd));
        }

        private void OnLoaded()
        {
            if (!IsLoaded)
            {
                IsLoaded = true;
                OnPropertyChanged(Visual.IsLoadedProperty);
                Loaded(this, EventArgs.Empty);
            }
        }

        private void OnUnloaded()
        {
            if (IsLoaded)
            {
                IsLoaded = false;
                OnPropertyChanged(Visual.IsLoadedProperty);
                Unloaded(this, EventArgs.Empty);
            }
        }
    }
}

