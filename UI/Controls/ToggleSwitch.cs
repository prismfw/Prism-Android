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
using Android;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using Prism.Input;
using Prism.Native;
using Prism.UI;
using Prism.UI.Controls;
using Prism.UI.Media;

namespace Prism.Android.UI.Controls
{
    /// <summary>
    /// Represents an Android implementation of an <see cref="INativeToggleSwitch"/>.
    /// </summary>
    [Preserve(AllMembers = true)]
    [Register(typeof(INativeToggleSwitch))]
    public class ToggleSwitch : Switch, INativeToggleSwitch
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
        /// Occurs when the value of the toggle switch is changed.
        /// </summary>
        public event EventHandler ValueChanged;

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
                    background = value;

                    if (background is SolidColorBrush)
                    {
                        SetColorLists();
                    }
                    else
                    {
                        Prism.Utilities.Logger.Warn("ToggleSwitch.Background on Android only supports instances of SolidColorBrush.");
                    }

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
                    SetSwitchTypeface(Typeface.Create(fontFamily.Name, Typeface.Style), Typeface.Style);
                    Paint.Flags = fontFamily.Traits;
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
            get { return TextSize; }
            set
            {
                if (value != TextSize)
                {
                    SetTextSize(ComplexUnitType.Sp, (float)value);
                    OnPropertyChanged(Control.FontSizeProperty);
                }
            }
        }

        /// <summary>
        /// Gets or sets the style with which to render the text in the control.
        /// </summary>
        public FontStyle FontStyle
        {
            get { return (FontStyle)Typeface.Style; }
            set
            {
                var style = (TypefaceStyle)value;
                if (style != Typeface.Style)
                {
                    var typeface = Typeface.Create(fontFamily.Name, style);
                    SetTypeface(typeface, style);
                    SetSwitchTypeface(typeface, style);
                    OnPropertyChanged(Control.FontStyleProperty);
                }
            }
        }

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
                    foreground = value;

                    if (foreground is SolidColorBrush)
                    {
                        SetColorLists();
                    }
                    else
                    {
                        Prism.Utilities.Logger.Warn("ToggleSwitch.Foreground on Android only supports instances of SolidColorBrush.");
                    }

                    OnPropertyChanged(Control.ForegroundProperty);
                }
            }
        }
        private Brush foreground;

        /// <summary>
        /// Gets or sets a <see cref="Rectangle"/> that represents the size and position of the element relative to its parent container.
        /// </summary>
        public Rectangle Frame { get; set; }

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
        /// Gets or sets the visual theme that should be used by this instance.
        /// </summary>
        public Theme RequestedTheme { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Brush"/> to apply to the thumb of the control when the control's value is false.
        /// </summary>
        public Brush ThumbOffBrush
        {
            get { return thumbOffBrush; }
            set
            {
                if (value != thumbOffBrush)
                {
                    thumbOffBrush = value;

                    if (thumbOffBrush is SolidColorBrush)
                    {
                        SetColorLists();
                    }
                    else
                    {
                        Prism.Utilities.Logger.Warn("ToggleSwitch.ThumbOffBrush on Android only supports instances of SolidColorBrush.");
                    }

                    OnPropertyChanged(Prism.UI.Controls.ToggleSwitch.ThumbOffBrushProperty);
                }
            }
        }
        private Brush thumbOffBrush;

        /// <summary>
        /// Gets or sets the <see cref="Brush"/> to apply to the thumb of the control when the control's value is false.
        /// </summary>
        public Brush ThumbOnBrush
        {
            get { return thumbOnBrush; }
            set
            {
                if (value != thumbOnBrush)
                {
                    thumbOnBrush = value;

                    if (thumbOffBrush is SolidColorBrush)
                    {
                        SetColorLists();
                    }
                    else
                    {
                        Prism.Utilities.Logger.Warn("ToggleSwitch.ThumbOnBrush on Android only supports instances of SolidColorBrush.");
                    }

                    OnPropertyChanged(Prism.UI.Controls.ToggleSwitch.ThumbOnBrushProperty);
                }
            }
        }
        private Brush thumbOnBrush;

        /// <summary>
        /// Gets or sets the value of the toggle switch.
        /// </summary>
        public bool Value
        {
            get { return base.Checked; }
            set
            {
                if (value != base.Checked)
                {
                    base.Checked = value;
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

        /// <summary>
        /// Initializes a new instance of the <see cref="ToggleSwitch"/> class.
        /// </summary>
        public ToggleSwitch()
            : base(Application.MainActivity)
        {
            Focusable = true;
            Gravity = GravityFlags.Start;
            Typeface = Typeface.Default;

            CheckedChange += (sender, e) =>
            {
                OnPropertyChanged(Prism.UI.Controls.ToggleSwitch.ValueProperty);
                ValueChanged(this, EventArgs.Empty);
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

            var widthSpec = Math.Min(int.MaxValue, constraints.Width.GetScaledInt());
            var heightSpec = Math.Min(int.MaxValue, constraints.Height.GetScaledInt());
            base.OnMeasure(MeasureSpec.MakeMeasureSpec(widthSpec, MeasureSpecMode.AtMost),
                MeasureSpec.MakeMeasureSpec(heightSpec, MeasureSpecMode.AtMost));

            var size = new Size(MeasuredWidth, MeasuredHeight).GetScaledSize();
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

            if (e.ActionMasked == MotionEventActions.Cancel)
            {
                PointerCanceled(this, e.GetPointerEventArgs(this));
                base.OnTouchEvent(e);
                return true;
            }
            if (e.ActionMasked == MotionEventActions.Down || e.ActionMasked == MotionEventActions.PointerDown)
            {
                PointerPressed(this, e.GetPointerEventArgs(this));
                base.OnTouchEvent(e);
                return true;
            }
            if (e.ActionMasked == MotionEventActions.Move)
            {
                PointerMoved(this, e.GetPointerEventArgs(this));
                base.OnTouchEvent(e);
                return true;
            }
            if (e.ActionMasked == MotionEventActions.Up || e.ActionMasked == MotionEventActions.PointerUp)
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

                borderPaint.StrokeWidth = borderWidth.GetScaledFloat();
                canvas.DrawLines(new float[] { 0, Height, 0, 0, 0, 0, Width, 0 }, borderPaint);

                // the right and bottom borders seem to be drawn thinner than the left and top ones
                borderPaint.StrokeWidth = (float)Math.Floor(borderPaint.StrokeWidth + 1);
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
            ArrangeRequest(false, null);

            Left = Frame.Left.GetScaledInt();
            Top = Frame.Top.GetScaledInt();
            Right = Frame.Right.GetScaledInt();
            Bottom = Frame.Bottom.GetScaledInt();

            base.OnLayout(changed, Left, Top, Right, Bottom);
        }

        /// <summary>
        /// Measure the view and its content to determine the measured width and the measured height.
        /// </summary>
        /// <param name="widthMeasureSpec">Horizontal space requirements as imposed by the parent.</param>
        /// <param name="heightMeasureSpec">Vertical space requirements as imposed by the parent.</param>
        public override void OnMeasure(int widthMeasureSpec, int heightMeasureSpec)
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
            Paint.SetBrush(foreground, w, h, null);
        }

        private void OnBorderImageLoaded(object sender, EventArgs e)
        {
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

        private void SetColorLists()
        {
            if (Build.VERSION.SdkInt < BuildVersionCodes.Lollipop)
            {
                Prism.Utilities.Logger.Warn("Setting ToggleSwitch brushes requires Android 5.0 (Lollipop) or later.");
                return;
            }

            int[] colors =
            {
                (background as SolidColorBrush)?.Color.GetHashCode() ?? Android.Resources.GetColor(this, Resource.Attribute.TextColorPrimary),
                (foreground as SolidColorBrush)?.Color.GetHashCode() ?? Android.Resources.GetColor(this, Resource.Attribute.ColorAccent)
            };

            int[][] states =
            {
                new int[] { -Resource.Attribute.StateChecked },
                new int[] { Resource.Attribute.StateChecked },
            };

            ThumbDrawable.SetTintList(new global::Android.Content.Res.ColorStateList(states, new int[]
            {
                (thumbOffBrush as SolidColorBrush)?.Color.GetHashCode() ?? colors[0],
                (thumbOnBrush as SolidColorBrush)?.Color.GetHashCode() ?? colors[1]
            }));

            TrackDrawable.SetTintList(new global::Android.Content.Res.ColorStateList(states, colors));
        }
    }
}

