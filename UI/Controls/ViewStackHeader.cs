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
using Android.Content;
using Android.Graphics;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using Prism.Native;
using Prism.Systems;
using Prism.UI;
using Prism.UI.Media;

namespace Prism.Android.UI.Controls
{
    /// <summary>
    /// Represents an Android implementation of an <see cref="INativeViewStackHeader"/>.
    /// </summary>
    [Preserve(AllMembers = true)]
    public sealed class ViewStackHeader : FrameLayout, INativeViewStackHeader
    {
        private const string BackButtonKey = "abc_ic_ab_back_mtrl_am_alpha";
        private const string BackButtonAltKey = "abc_ic_ab_back_material";

        /// <summary>
        /// Occurs when this instance has been attached to the visual tree and is ready to be rendered.
        /// </summary>
        public event EventHandler Loaded;

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
        /// Gets or sets the background for the header.
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
                    base.Background = background.GetDrawable(OnBackgroundImageLoaded) ?? ResourceExtractor.GetDrawable(global::Android.Resource.Attribute.HeaderBackground);
                    OnPropertyChanged(Prism.UI.Controls.ViewStackHeader.BackgroundProperty);
                }
            }
        }
        private Brush background;

        /// <summary>
        /// Gets or sets the font to use for displaying the title text.
        /// </summary>
        public object FontFamily
        {
            get { return fontFamily; }
            set
            {
                if (value != fontFamily)
                {
                    fontFamily = value as Media.FontFamily;
                    TitleView.SetTypeface(fontFamily.GetTypeface(), TitleView.Typeface.Style);
                    TitleView.Paint.Flags = fontFamily.Traits;
                    OnPropertyChanged(Prism.UI.Controls.ViewStackHeader.FontFamilyProperty);
                }
            }
        }
        private Media.FontFamily fontFamily;

        /// <summary>
        /// Gets or sets the size of the title text.
        /// </summary>
        public double FontSize
        {
            get { return TitleView.TextSize / Device.Current.DisplayScale; }
            set
            {
                if (value * Device.Current.DisplayScale != TitleView.TextSize)
                {
                    TitleView.SetTextSize(ComplexUnitType.Sp, (float)value);
                    OnPropertyChanged(Prism.UI.Controls.ViewStackHeader.FontSizeProperty);
                }
            }
        }

        /// <summary>
        /// Gets or sets the style with which to render the title text.
        /// </summary>
        public FontStyle FontStyle
        {
            get { return (FontStyle)TitleView.Typeface.Style; }
            set
            {
                var style = (TypefaceStyle)value;
                if (style != TitleView.Typeface.Style)
                {
                    TitleView.SetTypeface(fontFamily.GetTypeface(), style);
                    OnPropertyChanged(Prism.UI.Controls.ViewStackHeader.FontStyleProperty);
                }
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="Brush"/> to apply to the foreground content of the header.
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
                        TitleView.Paint.SetShader(null);
                        TitleView.SetTextColor(ResourceExtractor.GetColor(global::Android.Resource.Attribute.TextColorPrimary));
                        BackButton.SetColorFilter(ResourceExtractor.GetColor(global::Android.Resource.Attribute.TextColorPrimary));
                    }
                    else
                    {
                        TitleView.Paint.SetBrush(foreground, Width, (foreground is ImageBrush) ? Height : (TitleView.Paint.FontSpacing + 0.5f), OnForegroundImageLoaded);
                        TitleView.SetTextColor(TitleView.Paint.Color);
                        BackButton.SetColorFilter(TitleView.Paint.Color);
                    }

                    OnPropertyChanged(Prism.UI.Controls.ViewStackHeader.ForegroundProperty);
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
        /// Gets a value indicating whether the header is inset on top of the view stack content.
        /// A value of <c>false</c> indicates that the header offsets the view stack content.
        /// </summary>
        public bool IsInset
        {
            get { return false; }
        }

        /// <summary>
        /// Gets or sets the method to invoke when this instance requests a measurement of itself and its children.
        /// </summary>
        public MeasureRequestHandler MeasureRequest { get; set; }

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
        /// Gets or sets the title for the header.
        /// </summary>
        public string Title
        {
            get { return TitleView.Text; }
            set
            {
                value = value ?? string.Empty;
                if (value != TitleView.Text)
                {
                    TitleView.Text = value;
                    OnPropertyChanged(Prism.UI.Controls.ViewStackHeader.TitleProperty);
                }
            }
        }

        private ImageView BackButton { get; }
        
        private TextView TitleView { get; }
        
        private global::Android.Views.View[] menuButtons;

        internal ViewStackHeader(Context context)
            : base(context)
        {
            Focusable = true;
        
            BackButton = new ImageView(context) { Clickable = true, Focusable = true };
            BackButton.SetColorFilter(ResourceExtractor.GetColor(global::Android.Resource.Attribute.TextColorPrimary));
            BackButton.SetImageDrawable(ResourceExtractor.GetDrawable(ResourceExtractor.GetResourceId(BackButtonKey, BackButtonAltKey), BackButton));
            BackButton.Visibility = ViewStates.Gone;
            AddView(BackButton, new LayoutParams(LayoutParams.WrapContent, LayoutParams.WrapContent));

            TitleView = new TextView(context) { Typeface = Typeface.Default };
            TitleView.SetTextColor(ResourceExtractor.GetColor(global::Android.Resource.Attribute.TextColorPrimary));
            AddView(TitleView, new LayoutParams(LayoutParams.WrapContent, LayoutParams.WrapContent));
            
            BackButton.Click += (o, e) =>
            {
                this.GetParent<INativeViewStack>()?.PopView(Prism.UI.Animate.Default);
            };
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
            int height = 0;
            if (Visibility == ViewStates.Visible)
            {
                if (Device.Current.FormFactor == FormFactor.Phone)
                {
                    height = Resources.Configuration.Orientation == global::Android.Content.Res.Orientation.Landscape ? 48 : 56;
                }
                else
                {
                    height = 64;
                }
            }
            return new Size(constraints.Width, height);
        }

        /// <summary>
        /// Implement this method to intercept all touch screen motion events.
        /// </summary>
        /// <param name="ev">The motion event being dispatched down the hierarchy.</param>
        public override bool OnInterceptTouchEvent(MotionEvent ev)
        {
            return !IsHitTestVisible;
        }

        /// <summary>
        /// Sets the visibility of the header's back button.
        /// </summary>
        /// <param name="visibility">The visibility value to set on the back button.</param>
        public void SetBackButtonVisibility(ViewStates visibility)
        {
            BackButton.Visibility = visibility;
            SetItemPositions();
        }
        
        /// <summary>
        /// Sets the views to use for the menu buttons.
        /// </summary>
        /// <param name="buttons">The buttons that make up the menu.</param>
        public void SetMenuButtons(global::Android.Views.View[] buttons)
        {
            if (menuButtons != null)
            {
                foreach (var button in menuButtons)
                {
                    RemoveView(button);
                }
            }
            
            menuButtons = buttons;
            if (menuButtons != null)
            {
                foreach (var button in menuButtons)
                {
                    AddView(button, new LayoutParams(LayoutParams.WrapContent, LayoutParams.WrapContent));
                }
            }
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
        protected override void OnLayout (bool changed, int left, int top, int right, int bottom)
        {
            MeasureRequest(false, null);
            ArrangeRequest(false, null);

            base.OnLayout(changed, left, top, right, bottom);
            SetItemPositions();
            for (int i = 0; i < ChildCount; i++)
            {
                var child = GetChildAt(i);
                child.Layout(child.Left, child.Top, child.Left + child.MeasuredWidth, child.Top + child.MeasuredHeight);
            }
        }
        
        /// <summary>
        /// Measure the view and its content to determine the measured width and the measured height.
        /// </summary>
        /// <param name="widthMeasureSpec">Horizontal space requirements as imposed by the parent.</param>
        /// <param name="heightMeasureSpec">Vertical space requirements as imposed by the parent.</param>
        protected override void OnMeasure (int widthMeasureSpec, int heightMeasureSpec)
        {
            base.OnMeasure (widthMeasureSpec, heightMeasureSpec);
            
            for (int i = 0; i < ChildCount; i++)
            {
                var child = GetChildAt(i);
                child.Measure(MeasureSpec.MakeMeasureSpec(1, MeasureSpecMode.Unspecified), MeasureSpec.MakeMeasureSpec(1, MeasureSpecMode.Unspecified));
            }
        }

        private void OnBackgroundImageLoaded(object sender, EventArgs e)
        {
            base.Background = background.GetDrawable(null);
        }

        private void OnForegroundImageLoaded(object sender, EventArgs e)
        {
            TitleView.Paint.SetShader(foreground.GetShader(Width, Height, null));
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

        private void OnPropertyChanged(PropertyDescriptor pd)
        {
            PropertyChanged(this, new FrameworkPropertyChangedEventArgs(pd));
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

        private void SetItemPositions()
        {
            double scale = Device.Current.DisplayScale;
            if (Height >= (int)(64 * scale))
            {
                BackButton.Left = (int)(24 * scale);
                BackButton.Top = Math.Min((int)(20 * scale), (Height - BackButton.Height) / 2);
                TitleView.Left = (int)((BackButton.Visibility == ViewStates.Visible ? 72 : 24) * scale);
                TitleView.Top = Math.Max(Height - (TitleView.Height + (int)(24 * scale)), (Height - TitleView.Height) / 2);
                
                if (menuButtons != null)
                {
                    int right = Width - BackButton.Left;
                    foreach (var button in menuButtons)
                    {
                        button.Left = right - button.Width;
                        button.Top = Math.Min((int)(20 * scale), (Height - button.Height) / 2);
                        
                        right = Math.Max(button.Left - (int)(20 * scale), TitleView.Right);
                    }
                }
            }
            else
            {
                BackButton.Left = (int)(16 * scale);
                BackButton.Top = Math.Min(BackButton.Left, (Height - BackButton.Height) / 2);
                TitleView.Left = (int)((BackButton.Visibility == ViewStates.Visible ? 64 : 16) * scale);
                TitleView.Top = Math.Max(Height - (TitleView.Height + (int)(20 * scale)), (Height - TitleView.Height) / 2);
                
                if (menuButtons != null)
                {
                    int right = Width - BackButton.Left;
                    foreach (var button in menuButtons)
                    {
                        button.Left = right - button.Width;
                        button.Top = Math.Min(BackButton.Left, (Height - button.Height) / 2);
                        
                        right = Math.Max(button.Left - (int)(20 * scale), TitleView.Right);
                    }
                }
            }
        }
    }
}

