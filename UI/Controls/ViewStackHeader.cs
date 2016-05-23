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
        /// Occurs when the value of a property is changed.
        /// </summary>
        public event EventHandler<FrameworkPropertyChangedEventArgs> PropertyChanged;

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
        /// Called from layout when this view should assign a size and position to each of its children.
        /// </summary>
        /// <param name="changed"></param>
        /// <param name="left">Left position, relative to parent.</param>
        /// <param name="top">Top position, relative to parent.</param>
        /// <param name="right">Right position, relative to parent.</param>
        /// <param name="bottom">Bottom position, relative to parent.</param>
        protected override void OnLayout (bool changed, int left, int top, int right, int bottom)
        {
            base.OnLayout (changed, left, top, right, bottom);
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

        private void OnPropertyChanged(PropertyDescriptor pd)
        {
            PropertyChanged(this, new FrameworkPropertyChangedEventArgs(pd));
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

