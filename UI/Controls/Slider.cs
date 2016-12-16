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
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Prism.Input;
using Prism.Native;
using Prism.Systems;
using Prism.UI;
using Prism.UI.Controls;
using Prism.UI.Media;

namespace Prism.Android.UI.Controls
{
    /// <summary>
    /// Represents an Android implementation of an <see cref="INativeSlider"/>.
    /// </summary>
    [Preserve(AllMembers = true)]
    [Register(typeof(INativeSlider))]
    public class Slider : SeekBar, INativeSlider
    {
        /// <summary>
        /// Occurs when the control receives focus.
        /// </summary>
        public event EventHandler GotFocus;

        /// <summary>
        /// Occurs when this instance has been attached to the visual tree and is ready to be rendered.
        /// </summary>
        public event EventHandler Loaded;

        /// <summary>
        /// Occurs when the control loses focus.
        /// </summary>
        public event EventHandler LostFocus;
        
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
        /// Occurs when the <see cref="P:Value"/> property has changed.
        /// </summary>
        public event EventHandler<ValueChangedEventArgs<double>> ValueChanged;

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
        /// Gets or sets the background for the control.
        /// </summary>
        public new Brush Background
        {
            get { return background; }
            set
            {
                if (value != background)
                {
                    (background as ImageBrush).ClearImageHandler(OnBackgroundImageLoaded);

                    background = value;
                    trackDrawable.MaximumDrawable = background.GetDrawable(OnBackgroundImageLoaded);
                    OnPropertyChanged(Control.BackgroundProperty);
                }
            }
        }
        private Brush background;

        /// <summary>
        /// Gets or sets the <see cref="Brush"/> to apply to the border of the control.
        /// </summary>
        public Brush BorderBrush
        {
            get { return borderBrush; }
            set
            {
                if (value != borderBrush)
                {
                    (borderBrush as ImageBrush).ClearImageHandler(OnBorderImageLoaded);

                    borderBrush = value;
                    borderPaint.SetBrush(borderBrush, Width, Height, OnBorderImageLoaded);
                    OnPropertyChanged(Control.BorderBrushProperty);
                    Invalidate();
                }
            }
        }
        private Brush borderBrush;

        /// <summary>
        /// Gets or sets the width of the border around the control.
        /// </summary>
        public double BorderWidth
        {
            get { return borderWidth; }
            set
            {
                if (value != borderWidth)
                {
                    borderWidth = value;
                    OnPropertyChanged(Control.BorderWidthProperty);
                    Invalidate();
                }
            }
        }
        private double borderWidth;

        /// <summary>
        /// Gets or sets the font to use for displaying the text in the control.
        /// </summary>
        public object FontFamily
        {
            get { return fontFamily; }
            set
            {
                if (value != fontFamily)
                {
                    fontFamily = value as Media.FontFamily;
                    OnPropertyChanged(Control.FontFamilyProperty);
                }
            }
        }
        private Media.FontFamily fontFamily;

        /// <summary>
        /// Gets or sets the size of the text in the control.
        /// </summary>
        public double FontSize
        {
            get { return fontSize; }
            set
            {
                if (value != fontSize)
                {
                    fontSize = value;
                    OnPropertyChanged(Control.FontSizeProperty);
                }
            }
        }
        private double fontSize;

        /// <summary>
        /// Gets or sets the style with which to render the text in the control.
        /// </summary>
        public FontStyle FontStyle
        {
            get { return fontStyle; }
            set
            {
                if (value != fontStyle)
                {
                    fontStyle = value;
                    OnPropertyChanged(Control.FontStyleProperty);
                }
            }
        }
        private FontStyle fontStyle;

        /// <summary>
        /// Gets or sets the <see cref="Brush"/> to apply to the foreground content of the control.
        /// </summary>
        public new Brush Foreground
        {
            get { return foreground; }
            set
            {
                if (value != foreground)
                {
                    (foreground as ImageBrush).ClearImageHandler(OnForegroundImageLoaded);

                    foreground = value;
                    trackDrawable.MinimumDrawable = foreground.GetDrawable(OnForegroundImageLoaded);
                    OnPropertyChanged(Control.ForegroundProperty);
                }
            }
        }
        private Brush foreground;

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

                Measure(MeasureSpec.MakeMeasureSpec(Right - Left, global::Android.Views.MeasureSpecMode.AtMost),
                    MeasureSpec.MakeMeasureSpec(Bottom - Top, global::Android.Views.MeasureSpecMode.AtMost));
                Layout(Left, Top, Right, Bottom);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the user can interact with the control.
        /// </summary>
        public bool IsEnabled
        {
            get { return base.Enabled; }
            set
            {
                if (value != base.Enabled)
                {
                    base.Enabled = value;
                    OnPropertyChanged(Control.IsEnabledProperty);
                }
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
        /// Gets or sets a value indicating whether the thumb should automatically move to the closest step.
        /// </summary>
        public bool IsSnapToStepEnabled
        {
            get { return isSnapToStepEnabled; }
            set
            {
                if (value != isSnapToStepEnabled)
                {
                    isSnapToStepEnabled = value;
                    OnPropertyChanged(Prism.UI.Controls.Slider.IsSnapToStepEnabledProperty);

                    if (isSnapToStepEnabled)
                    {
                        Progress = (int)(((int)(Progress / stepFrequency + 0.5)) * stepFrequency);
                    }
                }
            }
        }
        private bool isSnapToStepEnabled;

        /// <summary>
        /// Gets or sets the maximum value that the control is allowed to have.
        /// </summary>
        public double MaxValue
        {
            get { return maxValue; }
            set
            {
                if (value != maxValue)
                {
                    maxValue = (int)value;
                    Max = maxValue - minValue;
                    OnPropertyChanged(Prism.UI.Controls.Slider.MaxValueProperty);
                }
            }
        }
        private int maxValue;

        /// <summary>
        /// Gets or sets the method to invoke when this instance requests a measurement of itself and its children.
        /// </summary>
        public MeasureRequestHandler MeasureRequest { get; set; }

        /// <summary>
        /// Gets or sets the minimum value that the control is allowed to have.
        /// </summary>
        public double MinValue
        {
            get { return minValue; }
            set
            {
                if (value != minValue)
                {
                    int oldValue = minValue;
                    minValue = (int)value;
                    Max = maxValue - minValue;
                    Progress = Math.Max(0, Progress + (oldValue - minValue));
                    OnPropertyChanged(Prism.UI.Controls.Slider.MinValueProperty);
                }
            }
        }
        private int minValue;

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
        /// Gets or sets the visual theme that should be used by this instance.
        /// </summary>
        public Theme RequestedTheme { get; set; }

        /// <summary>
        /// Gets or sets the interval between steps along the track.
        /// </summary>
        public double StepFrequency
        {
            get { return stepFrequency; }
            set
            {
                if (value != stepFrequency)
                {
                    stepFrequency = value;
                    OnPropertyChanged(Prism.UI.Controls.Slider.StepFrequencyProperty);

                    if (isSnapToStepEnabled)
                    {
                        Progress = (int)(((int)(Progress / stepFrequency + 0.5)) * stepFrequency);
                    }
                }
            }
        }
        private double stepFrequency;

        /// <summary>
        /// Gets or sets the <see cref="Brush"/> to apply to the thumb of the control.
        /// </summary>
        public Brush ThumbBrush
        {
            get { return thumbBrush; }
            set
            {
                if (value != thumbBrush)
                {
                    (thumbBrush as ImageBrush).ClearImageHandler(OnThumbImageLoaded);

                    thumbBrush = value;
                    
                    var scb = thumbBrush as SolidColorBrush;
                    if (scb == null)
                    {
                        SetThumb(thumbBrush.GetDrawable(OnThumbImageLoaded) ?? Android.Resources.GetDrawable(this, SystemResources.SliderThumbBrushKey));
                        Thumb.ClearColorFilter();
                    }
                    else
                    {
                        var thumbDrawable = Android.Resources.GetDrawable(this, SystemResources.SliderThumbBrushKey);
                        thumbDrawable.SetColorFilter(scb.Color.GetColor(), PorterDuff.Mode.SrcIn);
                        SetThumb(thumbDrawable);
                    }
                    
                    OnPropertyChanged(Prism.UI.Controls.Slider.ThumbBrushProperty);
                }
            }
        }
        private Brush thumbBrush;

        /// <summary>
        /// Gets or sets the current value of the control.
        /// </summary>
        public double Value
        {
            get { return Progress + minValue; }
            set
            {
                if (value != Progress + minValue)
                {
                    Progress = (int)(value - minValue);
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

        private readonly Paint borderPaint = new Paint();
        private double currentValue;
        private readonly TrackDrawable trackDrawable;

        /// <summary>
        /// Initializes a new instance of the <see cref="Slider"/> class.
        /// </summary>
        public Slider()
            : base(Application.MainActivity)
        {
            trackDrawable = new TrackDrawable(this);

            Focusable = true;
            ProgressDrawable = trackDrawable;

            ProgressChanged += (sender, e) =>
            {
                if (isSnapToStepEnabled)
                {
                    Progress = (int)(((int)(e.Progress / stepFrequency + 0.5)) * stepFrequency);
                }

                if (Value != currentValue)
                {
                    OnPropertyChanged(Prism.UI.Controls.Slider.ValueProperty);
                    ValueChanged(this, new ValueChangedEventArgs<double>(currentValue, Value));
                    currentValue = Value;
                }
            };
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
        /// Attempts to set focus to the control.
        /// </summary>
        public void Focus()
        {
            if (!IsFocused && !RequestFocus())
            {
                RequestFocusFromTouch();
            }
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
        /// Attempts to remove focus from the control.
        /// </summary>
        public void Unfocus()
        {
            ClearFocus();
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
        /// Implement this to do your drawing.
        /// </summary>
        /// <param name="canvas"></param>
        protected override void OnDraw(global::Android.Graphics.Canvas canvas)
        {
            base.OnDraw(canvas);

            if (borderBrush != null && borderWidth > 0)
            {
                canvas.Save();

                borderPaint.StrokeWidth = (float)(borderWidth * Device.Current.DisplayScale);
                canvas.DrawLines(new float[] { 0, Height, 0, 0, 0, 0, Width, 0 }, borderPaint);

                // the right and bottom borders seem to be drawn thinner than the left and top ones
                borderPaint.StrokeWidth++;
                canvas.DrawLines(new float[] { Width, 0, Width, Height, Width, Height, 0, Height }, borderPaint);

                canvas.Restore();
            }
        }

        /// <summary>
        /// Called by the view system when the focus state of this view changes.
        /// </summary>
        /// <param name="gainFocus">True if the View has focus; false otherwise.</param>
        /// <param name="direction"></param>
        /// <param name="previouslyFocusedRect"></param>
        protected override void OnFocusChanged(bool gainFocus, FocusSearchDirection direction, Rect previouslyFocusedRect)
        {
            base.OnFocusChanged(gainFocus, direction, previouslyFocusedRect);

            OnPropertyChanged(Prism.UI.Controls.Control.IsFocusedProperty);
            if (gainFocus)
            {
                GotFocus(this, EventArgs.Empty);
            }
            else
            {
                LostFocus(this, EventArgs.Empty);
            }
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

        /// <summary>
        /// This is called during layout when the size of this view has changed.
        /// </summary>
        /// <param name="w">Current width of this view.</param>
        /// <param name="h">Current height of this view.</param>
        /// <param name="oldw">Old width of this view.</param>
        /// <param name="oldh">Old height of this view.</param>
        protected override void OnSizeChanged(int w, int h, int oldw, int oldh)
        {
            base.OnSizeChanged(w, h, oldw, oldh);
            borderPaint.SetBrush(borderBrush, w, h, null);
        }

        private void OnBackgroundImageLoaded(object sender, EventArgs e)
        {
            trackDrawable.MaximumDrawable = background.GetDrawable(null);
        }

        private void OnBorderImageLoaded(object sender, EventArgs e)
        {
            Invalidate();
        }

        private void OnForegroundImageLoaded(object sender, EventArgs e)
        {
            trackDrawable.MinimumDrawable = foreground.GetDrawable(null);
        }

        private void OnThumbImageLoaded(object sender, EventArgs e)
        {
            SetThumb(thumbBrush.GetDrawable(null) ?? Android.Resources.GetDrawable(this, SystemResources.SliderThumbBrushKey));
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

        private class TrackDrawable : Drawable
        {
            public Drawable MaximumDrawable
            {
                get { return maxDrawable; }
                set { maxDrawable = value ?? new ColorDrawable(global::Android.Graphics.Color.Rgb(33, 36, 40)); }
            }
            private Drawable maxDrawable;

            public Drawable MinimumDrawable
            {
                get { return minDrawable; }
                set { minDrawable = value ?? new ColorDrawable(global::Android.Graphics.Color.Rgb(38, 169, 216)); }
            }
            private Drawable minDrawable;

            public override int Opacity
            {
                get { return (int)Format.Opaque; }
            }

            private readonly Slider slider;

            public TrackDrawable(Slider parent)
            {
                slider = parent;

                MaximumDrawable = null;
                MinimumDrawable = null;
            }

            public override void Draw(global::Android.Graphics.Canvas canvas)
            {
                int width = slider.Width - (slider.PaddingLeft + slider.PaddingRight);
                int height = (int)Math.Max(1, (Device.Current.DisplayScale / 2));
                int vCenter = (slider.Height - (slider.PaddingTop + slider.PaddingBottom)) / 2;
                int progress = (int)((slider.Progress / (double)slider.Max) * width);

                minDrawable.SetBounds(0, vCenter - height, progress, vCenter + height);
                minDrawable.Draw(canvas);

                maxDrawable.SetBounds(progress, vCenter - height, width, vCenter + height);
                maxDrawable.Draw(canvas);
            }

            public override void SetAlpha(int alpha)
            {
                minDrawable.SetAlpha(alpha);
                maxDrawable.SetAlpha(alpha);
            }

            public override void SetColorFilter(ColorFilter cf)
            {
                minDrawable.SetColorFilter(cf);
                maxDrawable.SetColorFilter(cf);
            }
        }
    }
}

