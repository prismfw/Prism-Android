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
using Android.Text;
using Android.Util;
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
    /// Represents an Android implementation of an <see cref="INativeButton"/>.
    /// </summary>
    [Preserve(AllMembers = true)]
    [Register(typeof(INativeButton))]
    public class Button : FrameLayout, INativeButton
    {
        /// <summary>
        /// Occurs when the button is clicked or tapped.
        /// </summary>
        public event EventHandler Clicked;

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
                    backgroundDefault.ClearColorFilter();
                    if (background is ImageBrush || background is LinearGradientBrush)
                    {
                        base.Background = background.GetDrawable(OnBackgroundImageLoaded);
                    }
                    else
                    {
                        base.Background = backgroundDefault;
                        
                        var scb = background as SolidColorBrush;
                        if (scb != null)
                        {
                            base.Background.SetColorFilter(scb.Color.GetColor(), PorterDuff.Mode.SrcIn);
                        }
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
        /// Gets or sets the direction in which the button image should be placed in relation to the button title.
        /// </summary>
        public ContentDirection ContentDirection
        {
            get { return contentDirection; }
            set
            {
                if (value != contentDirection)
                {
                    contentDirection = value;
                    OnPropertyChanged(Prism.UI.Controls.Button.ContentDirectionProperty);
                    SetVisuals();
                }
            }
        }
        private ContentDirection contentDirection;

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
                    TextView.SetTypeface(fontFamily.GetTypeface(), TextView.Typeface.Style);
                    TextView.Paint.Flags = fontFamily.Traits;
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
            get { return TextView.TextSize / Device.Current.DisplayScale; }
            set
            {
                if (value * Device.Current.DisplayScale != TextView.TextSize)
                {
                    TextView.SetTextSize(ComplexUnitType.Sp, (float)value);
                    OnPropertyChanged(Control.FontSizeProperty);
                }
            }
        }

        /// <summary>
        /// Gets or sets the style with which to render the text in the control.
        /// </summary>
        public FontStyle FontStyle
        {
            get { return (FontStyle)TextView.Typeface.Style; }
            set
            {
                var style = (TypefaceStyle)value;
                if (style != TextView.Typeface.Style)
                {
                    TextView.SetTypeface(fontFamily.GetTypeface(), style);
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
                    (foreground as ImageBrush).ClearImageHandler(OnForegroundImageLoaded);

                    foreground = value;
                    if (foreground == null)
                    {
                        TextView.Paint.SetShader(null);
                        TextView.SetTextColor(ResourceExtractor.GetColor(global::Android.Resource.Attribute.TextColorPrimary)); 
                    }
                    else
                    {
                        TextView.Paint.SetBrush(foreground, Width, (foreground is ImageBrush) ? Height : (TextView.Paint.FontSpacing + 0.5f), OnForegroundImageLoaded);
                        TextView.SetTextColor(TextView.Paint.Color);
                    }

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

                Measure(MeasureSpec.MakeMeasureSpec(Right - Left, MeasureSpecMode.Exactly),
                    MeasureSpec.MakeMeasureSpec(Bottom - Top, MeasureSpecMode.Exactly));
                Layout(Left, Top, Right, Bottom);
            }
        }

        /// <summary>
        /// Gets or sets an image to display within the button.
        /// </summary>
        public INativeImageSource Image
        {
            get { return image; }
            set
            {
                if (value != image)
                {
                    image.ClearImageHandler(OnImageLoaded);

                    image = value;
                    image.BeginLoadingImage(OnImageLoaded);
                    SetVisuals();
                    OnPropertyChanged(Prism.UI.Controls.Button.ImageProperty);
                }
            }
        }
        private INativeImageSource image;

        /// <summary>
        /// Gets or sets a value indicating whether the user can interact with the control.
        /// </summary>
        public bool IsEnabled
        {
            get { return Enabled; }
            set
            {
                if (value != Enabled)
                {
                    Enabled = value;
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
        /// Gets or sets the inner padding of the element.
        /// </summary>
        public Thickness Padding
        {
            get { return new Thickness(PaddingLeft, PaddingTop, PaddingRight, PaddingBottom) / Device.Current.DisplayScale; }
            set
            {
                value *= Device.Current.DisplayScale;
                if (value != Padding)
                {
                    SetPadding((int)value.Left, (int)value.Top, (int)value.Right, (int)value.Bottom);
                    OnPropertyChanged(Prism.UI.Controls.Button.PaddingProperty);
                }
            }
        }

        /// <summary>
        /// Gets or sets the title of the button.
        /// </summary>
        public string Title
        {
            get { return TextView.Text; }
            set
            {
                if (value != TextView.Text)
                {
                    TextView.Text = value;
                    OnPropertyChanged(Prism.UI.Controls.Button.TitleProperty);
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
        /// Gets the view that displays the image in the button.
        /// </summary>
        protected ImageView ImageView { get; }

        /// <summary>
        /// Gets the view that displays the text in the button.
        /// </summary>
        protected TextView TextView { get; }

        private readonly Drawable backgroundDefault;
        private readonly Paint borderPaint = new Paint();
        private readonly LinearLayout layout;

        /// <summary>
        /// Initializes a new instance of the <see cref="Button"/> class.
        /// </summary>
        public Button()
            : base(Application.MainActivity)
        {
            base.Background = backgroundDefault = new global::Android.Widget.Button(Context).Background;
            Focusable = true;

            TextView = new TextView(Application.MainActivity)
            {
                Ellipsize = TextUtils.TruncateAt.End,
                LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent),
                Typeface = Typeface.Default
            };
            TextView.SetTextColor(ResourceExtractor.GetColor(global::Android.Resource.Attribute.TextColorPrimary));

            ImageView = new ImageView(Application.MainActivity)
            {
                LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent)
            };

            layout = new LinearLayout(Application.MainActivity)
            {
                LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent)  
            };

            AddView(layout, new LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent, GravityFlags.Center));

            Click += (o, e) =>
            {
                Clicked(this, EventArgs.Empty);
            };

            TextView.TextChanged += (sender, e) =>
            {
                if (string.IsNullOrEmpty(TextView.Text) && TextView.Parent == layout)
                {
                    layout.RemoveView(TextView);
                }
                else if (!string.IsNullOrEmpty(TextView.Text) && TextView.Parent == null)
                {
                    layout.AddView(TextView);
                    SetVisuals();
                }
            };
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
            var imageSize = image == null ? Size.Empty : new Size(image.PixelWidth, image.PixelHeight);
            if (contentDirection == ContentDirection.Up || contentDirection == ContentDirection.Down)
            {
                constraints.Height -= imageSize.Height;
            }
            else
            {
                constraints.Width -= imageSize.Width;
            }

            var textSize = Size.Empty;
            if (TextView.Parent != null)
            {
                TextView.Measure(MeasureSpec.MakeMeasureSpec((int)(constraints.Width * Device.Current.DisplayScale), MeasureSpecMode.AtMost),
                    MeasureSpec.MakeMeasureSpec((int)(constraints.Height * Device.Current.DisplayScale), MeasureSpecMode.AtMost));

                textSize = new Size(TextView.MeasuredWidth, TextView.MeasuredHeight);
            }

            var size = new Size(PaddingLeft + PaddingRight + (borderWidth * Device.Current.DisplayScale),
                PaddingTop + PaddingBottom + (borderWidth * Device.Current.DisplayScale));
            
            if (contentDirection == ContentDirection.Up || contentDirection == ContentDirection.Down)
            {
                size.Width += Math.Max(imageSize.Width, textSize.Width);
                size.Height += imageSize.Height + textSize.Height;
            }
            else
            {
                size.Width += imageSize.Width + textSize.Width;
                size.Height += Math.Max(imageSize.Height, textSize.Height);
            }

            return new Size(Math.Min(constraints.Width, size.Width / Device.Current.DisplayScale),
                Math.Min(constraints.Height, size.Height / Device.Current.DisplayScale));
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
                borderPaint.StrokeWidth = (float)(borderWidth * Device.Current.DisplayScale);
                canvas.DrawLines(new float[] { 0, Height, 0, 0, 0, 0, Width, 0 }, borderPaint);

                // the right and bottom borders seem to be drawn thinner than the left and top ones
                borderPaint.StrokeWidth++;
                canvas.DrawLines(new float[] { Width, 0, Width, Height, Width, Height, 0, Height }, borderPaint);
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

            OnPropertyChanged(Control.IsFocusedProperty);
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
            PropertyChanged?.Invoke(this, new FrameworkPropertyChangedEventArgs(pd));
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
            TextView.Paint.SetBrush(foreground, w, (foreground is ImageBrush) ? h : TextView.Paint.FontSpacing + 0.5f, null);
        }

        private void OnBackgroundImageLoaded(object sender, EventArgs e)
        {
            base.Background = background.GetDrawable(null) ?? backgroundDefault;
        }

        private void OnBorderImageLoaded(object sender, EventArgs e)
        {
            Invalidate();
        }

        private void OnForegroundImageLoaded(object sender, EventArgs e)
        {
            TextView.Paint.SetShader(foreground.GetShader(Width, Height, null));
        }

        private void OnImageLoaded(object sender, EventArgs e)
        {
            SetVisuals();
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

        private void SetVisuals()
        {
            ImageView.SetImageBitmap(Image.GetImageSource());

            if (Image != null && ((Image as INativeBitmapImage)?.IsLoaded ?? true) && ImageView.Parent == null)
            {
                layout.AddView(ImageView);
            }
            else if ((Image == null || (!(Image as INativeBitmapImage)?.IsLoaded ?? true)) && ImageView.Parent == layout)
            {
                layout.RemoveView(ImageView);
            }

            if (ImageView.Parent != null && TextView.Parent != null)
            {
                switch (contentDirection)
                {
                    case ContentDirection.Down:
                        layout.Orientation = global::Android.Widget.Orientation.Vertical;
                        layout.RemoveView(ImageView);
                        layout.AddView(ImageView);
                        break;
                    case ContentDirection.Left:
                        layout.Orientation = global::Android.Widget.Orientation.Horizontal;
                        layout.RemoveView(TextView);
                        layout.AddView(TextView);
                        break;
                    case ContentDirection.Right:
                        layout.Orientation = global::Android.Widget.Orientation.Horizontal;
                        layout.RemoveView(ImageView);
                        layout.AddView(ImageView);
                        break;
                    case ContentDirection.Up:
                        layout.Orientation = global::Android.Widget.Orientation.Vertical;
                        layout.RemoveView(TextView);
                        layout.AddView(TextView);
                        break;
                }
            }

            var parent = Parent as global::Android.Views.View;
            if (parent != null)
            {
                Measure(MeasureSpec.MakeMeasureSpec(parent.Right, MeasureSpecMode.Exactly),
                    MeasureSpec.MakeMeasureSpec(parent.Bottom, MeasureSpecMode.Exactly));
            }
        }
    }
}