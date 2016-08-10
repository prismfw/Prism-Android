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
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Prism.Input;
using Prism.Native;
using Prism.Systems;
using Prism.UI;
using Prism.UI.Controls;

namespace Prism.Android.UI.Controls
{
    /// <summary>
    /// Represents an Android implementation of an <see cref="INativeImage"/>.
    /// </summary>
    [Preserve(AllMembers = true)]
    [Register(typeof(INativeImage))]
    public class Image : ImageView, INativeImage
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
        /// Gets or sets a <see cref="Rectangle"/> that represents the size and position of the element relative to its parent container.
        /// </summary>
        public Rectangle Frame
        {
            get
            {
                return new Rectangle(Left / Device.Current.DisplayScale, Top / Device.Current.DisplayScale,
                    Width / Device.Current.DisplayScale, Height / Device.Current.DisplayScale);
            }
            set
            {
                Left = (int)(value.Left * Device.Current.DisplayScale);
                Top = (int)(value.Top * Device.Current.DisplayScale);
                Right = (int)(value.Right * Device.Current.DisplayScale);
                Bottom = (int)(value.Bottom * Device.Current.DisplayScale);

                Measure(MeasureSpec.MakeMeasureSpec(Right - Left, MeasureSpecMode.Exactly),
                    MeasureSpec.MakeMeasureSpec(Bottom - Top, MeasureSpecMode.Exactly));
                Layout(Left, Top, Right, Bottom);
            }
        }

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
        /// Gets or sets the <see cref="INativeImageSource"/> object that contains the image data for the element.
        /// </summary>
        public INativeImageSource Source
        {
            get { return source; }
            set
            {
                if (value != source)
                {
                    source.ClearImageHandler(OnImageLoaded);

                    source = value;
                    if (source != null && ((source as INativeBitmapImage)?.IsLoaded ?? true))
                    {
                        OnImageLoaded(null, null);
                    }
                    else
                    {
                        SetImageBitmap(source.BeginLoadingImage(OnImageLoaded));
                    }
                    OnPropertyChanged(Prism.UI.Controls.Image.SourceProperty);
                }
            }
        }
        private INativeImageSource source;

        /// <summary>
        /// Gets or sets the display state of the element.
        /// </summary>
        public Stretch Stretch
        {
            get { return GetScaleType().GetStretch(); }
            set
            {
                var scaleType = value.GetScaleType();
                if (scaleType != GetScaleType())
                {
                    SetScaleType(scaleType);
                    OnPropertyChanged(Prism.UI.Controls.Image.StretchProperty);
                }
            }
        }

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
        /// Initializes a new instance of the <see cref="Image"/> class.
        /// </summary>
        public Image()
            : base(Application.MainActivity)
        {
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
            if (source == null || !((source as INativeBitmapImage)?.IsLoaded ?? true))
            {
                return Size.Empty;
            }

            double scale = 1;
            double width = source.PixelWidth / source.Scale;
            double height = source.PixelHeight / source.Scale;
            if (constraints.Width < width)
            {
                scale = constraints.Width / width;
            }
            if (constraints.Height < height)
            {
                scale = Math.Min(scale, constraints.Height / height);
            }

            return new Size(width * scale, height * scale);
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
                base.OnTouchEvent(e);
                return true;
            }
            if (e.Action == MotionEventActions.Down)
            {
                PointerPressed(this, e.GetPointerEventArgs(this));
                base.OnTouchEvent(e);
                return true;
            }
            if (e.Action == MotionEventActions.Move)
            {
                PointerMoved(this, e.GetPointerEventArgs(this));
                base.OnTouchEvent(e);
                return true;
            }
            if (e.Action == MotionEventActions.Up)
            {
                PointerReleased(this, e.GetPointerEventArgs(this));
                base.OnTouchEvent(e);
                return true;
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
            MeasureRequest(false, null);
            ArrangeRequest(false, null);
            base.OnLayout(changed, Left, Top, Right, Bottom);
        }

        /// <summary>
        /// Called when a property value is changed.
        /// </summary>
        /// <param name="pd">A property descriptor describing the property whose value has been changed.</param>
        protected virtual void OnPropertyChanged(PropertyDescriptor pd)
        {
            PropertyChanged(this, new FrameworkPropertyChangedEventArgs(pd));
        }

        private void OnImageLoaded(object sender, EventArgs e)
        {
            SetImageBitmap(source.GetImageSource());
            Right = Left + (int)(source.PixelWidth / source.Scale);
            Bottom = Top + (int)(source.PixelHeight / source.Scale);

            source.GetImageSource().PrepareToDraw();
            Invalidate();
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

