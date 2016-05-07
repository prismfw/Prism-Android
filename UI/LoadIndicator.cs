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
using Android.App;
using Android.Graphics;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using Prism.Native;
using Prism.Systems;
using Prism.UI.Media;

namespace Prism.Android.UI
{
    /// <summary>
    /// Represents an Android implementation for an <see cref="INativeLoadIndicator"/>.
    /// </summary>
    [Preserve(AllMembers = true)]
    [Register(typeof(INativeLoadIndicator))]
    public class LoadIndicator : ProgressDialog, INativeLoadIndicator
    {
        /// <summary>
        /// Occurs when the value of a property is changed.
        /// </summary>
        public event EventHandler<FrameworkPropertyChangedEventArgs> PropertyChanged;
        
        /// <summary>
        /// Gets or sets the background of the indicator.
        /// </summary>
        public Brush Background
        {
            get { return background; }
            set
            {
                if (value != background)
                {
                    (background as ImageBrush).ClearImageHandler(OnBackgroundImageLoaded);

                    background = value;
                    layout.Background = (background.GetDrawable(OnBackgroundImageLoaded) ??
                        ResourceExtractor.GetDrawable(global::Android.Resource.Attribute.Background));
                    
                    OnPropertyChanged(Prism.UI.LoadIndicator.BackgroundProperty);
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
                    TextView.SetTypeface(fontFamily.GetTypeface(), TextView.Typeface.Style);
                    TextView.Paint.Flags = fontFamily.Traits;
                    OnPropertyChanged(Prism.UI.LoadIndicator.FontFamilyProperty);
                }
            }
        }
        private Media.FontFamily fontFamily;

        /// <summary>
        /// Gets or sets the size of the title text.
        /// </summary>
        public double FontSize
        {
            get { return TextView.TextSize / Device.Current.DisplayScale; }
            set
            {
                if (value * Device.Current.DisplayScale != TextView.TextSize)
                {
                    TextView.SetTextSize(ComplexUnitType.Sp, (float)value);
                    OnPropertyChanged(Prism.UI.LoadIndicator.FontSizeProperty);
                }
            }
        }

        /// <summary>
        /// Gets or sets the style with which to render the title text.
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
                    OnPropertyChanged(Prism.UI.LoadIndicator.FontStyleProperty);
                }
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="Brush"/> to apply to the title text.
        /// </summary>
        public Brush Foreground
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
                        ProgressIndicator.IndeterminateDrawable.ClearColorFilter();
                    }
                    else
                    {
                        TextView.Paint.SetBrush(foreground, TextView.Width, (foreground is ImageBrush) ? TextView.Height :
                            (TextView.Paint.FontSpacing + 0.5f), OnForegroundImageLoaded);
                        TextView.SetTextColor(TextView.Paint.Color);
                        ProgressIndicator.IndeterminateDrawable.SetColorFilter(TextView.Paint.Color, PorterDuff.Mode.SrcIn);
                    }
                    
                    OnPropertyChanged(Prism.UI.LoadIndicator.ForegroundProperty);
                }
            }
        }
        private Brush foreground;

        /// <summary>
        /// Gets a value indicating whether this instance is currently visible.
        /// </summary>
        public bool IsVisible
        {
            get { return IsShowing; }
        }

        /// <summary>
        /// Gets or sets the title text of the indicator.
        /// </summary>
        public string Title
        {
            get { return TextView.Text; }
            set
            {
                if (value != TextView.Text)
                {
                    TextView.Text = value;
                    OnPropertyChanged(Prism.UI.LoadIndicator.TitleProperty);
                }
            }
        }

        /// <summary>
        /// Gets the view that displays the progress of the load.
        /// </summary>
        protected ProgressBar ProgressIndicator { get; }

        /// <summary>
        /// Gets the view that shows the title text of the indicator.
        /// </summary>
        protected TextView TextView { get; }

        private readonly LinearLayout layout;

        /// <summary>
        /// Initializes a new instance of the <see cref="LoadIndicator"/> class.
        /// </summary>
        public LoadIndicator()
            : base(Application.MainActivity)
        {
            layout = new LinearLayout(Context);
            ProgressIndicator = new ProgressBar(Context) { Indeterminate = true };
            TextView = new TextView(Context) { Typeface = Typeface.Default };

            layout.LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent);
            layout.Orientation = global::Android.Widget.Orientation.Horizontal;
            layout.AddView(ProgressIndicator);
            layout.AddView(TextView);
            layout.SetVerticalGravity(GravityFlags.CenterVertical);
        }

        /// <summary>
        /// Removes the indicator from view.
        /// </summary>
        public new void Hide()
        {
            Dismiss();
        }

        /// <summary>
        /// Displays the indicator.
        /// </summary>
        public override void Show()
        {
            base.Show();
            SetContentView(layout);
        }

        /// <summary>
        /// Called when a property value is changed.
        /// </summary>
        /// <param name="pd">A property descriptor describing the property whose value has been changed.</param>
        protected virtual void OnPropertyChanged(PropertyDescriptor pd)
        {
            PropertyChanged(this, new FrameworkPropertyChangedEventArgs(pd));
        }

        private void OnBackgroundImageLoaded(object sender, EventArgs e)
        {
            layout.Background = background.GetDrawable(null);
        }
        
        private void OnForegroundImageLoaded(object sender, EventArgs e)
        {
            TextView.Paint.SetShader(foreground.GetShader(TextView.Width, TextView.Height, null));
        }
    }
}