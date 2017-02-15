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
using Prism.Native;
using Prism.Systems;
using Prism.UI.Media;
using Prism.UI.Media.Imaging;

namespace Prism.Android.UI.Controls
{
    /// <summary>
    /// Represents an Android implementation of an <see cref="INativeMenuButton"/>.
    /// </summary>
    [Preserve(AllMembers = true)]
    [Register(typeof(INativeMenuButton))]
    public class MenuButton : FrameLayout, INativeMenuButton
    {
        /// <summary>
        /// Occurs when the value of a property is changed.
        /// </summary>
        public event EventHandler<FrameworkPropertyChangedEventArgs> PropertyChanged;

        /// <summary>
        /// Gets or sets the action to perform when the button is pressed by the user.
        /// </summary>
        public Action Action { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Brush"/> to apply to the foreground content of the menu item.
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
                    SetForeground(foreground ?? enabledForeground, true);
                    OnPropertyChanged(Prism.UI.Controls.MenuItem.ForegroundProperty);
                }
            }
        }
        private Brush foreground;
        
        /// <summary>
        /// Gets or sets the <see cref="Uri"/> of the image to display within the button.
        /// </summary>
        public Uri ImageUri
        {
            get { return imageUri; }
            set
            {
                if (value != imageUri)
                {
                    imageUri = value;
                    if (imageUri == null)
                    {
                        ImageView.Visibility = ViewStates.Gone;
                        TextView.Visibility = ViewStates.Visible;
                    }
                    else
                    {
                        TextView.Visibility = ViewStates.Gone;
                        ImageView.Visibility = ViewStates.Visible;
                        
                        var source = (INativeImageSource)ObjectRetriever.GetNativeObject(new BitmapImage(imageUri));
                        ImageView.SetImageBitmap(source.BeginLoadingImage(OnImageLoaded));
                    }
                    
                    OnPropertyChanged(Prism.UI.Controls.MenuButton.ImageUriProperty);
                }
            }
        }
        private Uri imageUri;

        /// <summary>
        /// Gets or sets a value indicating whether the button is enabled and should respond to user interaction.
        /// </summary>
        public bool IsEnabled
        {
            get { return base.Enabled; }
            set
            {
                if (value != Enabled)
                {
                    Enabled = value;
                    ImageView.Enabled = value;
                    TextView.Enabled = value;
                    
                    if (Enabled)
                    {
                        SetForeground(enabledForeground, false);
                    }
                    else
                    {
                        ImageView.SetColorFilter(new global::Android.Graphics.Color(110, 110, 110));
                        TextView.SetTextColor(new global::Android.Graphics.Color(110, 110, 110));
                        TextView.Paint.SetShader(null);
                    }
                    
                    OnPropertyChanged(Prism.UI.Controls.MenuButton.IsEnabledProperty);
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
                    OnPropertyChanged(Prism.UI.Controls.MenuButton.TitleProperty);
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
        
        private Brush enabledForeground;

        /// <summary>
        /// Initializes a new instance of the <see cref="MenuButton"/> class.
        /// </summary>
        public MenuButton()
            : base(Application.MainActivity)
        {
            SetMinimumWidth((int)(40 * Device.Current.DisplayScale));
            SetMinimumHeight(MinimumWidth);
            
            ImageView = new ImageView(Context);
            AddView(ImageView, new LayoutParams(LayoutParams.WrapContent, LayoutParams.WrapContent, GravityFlags.CenterVertical | GravityFlags.CenterHorizontal));
            
            TextView = new TextView(Context);
            AddView(TextView, new LayoutParams(LayoutParams.WrapContent, LayoutParams.WrapContent, GravityFlags.CenterVertical | GravityFlags.CenterHorizontal));
        
            Click += (o, e) => Action();
        }

        /// <summary>
        /// Called when a property value is changed.
        /// </summary>
        /// <param name="pd">A property descriptor describing the property whose value has been changed.</param>
        protected virtual void OnPropertyChanged(PropertyDescriptor pd)
        {
            PropertyChanged(this, new FrameworkPropertyChangedEventArgs(pd));
        }
        
        internal void SetForeground(Brush foreground, bool listen)
        {
            enabledForeground = foreground;
            if (!Enabled)
            {
                return;
            }
            
            if (foreground == null)
            {
                TextView.Paint.SetShader(null);
                TextView.SetTextColor(Android.Resources.GetColor(this, global::Android.Resource.Attribute.TextColorPrimary)); 
                ImageView.SetColorFilter(Android.Resources.GetColor(this, global::Android.Resource.Attribute.TextColorPrimary));
            }
            else
            {
                TextView.Paint.SetBrush(foreground, Width, (foreground is ImageBrush) ? Height : (TextView.Paint.FontSpacing + 0.5f),
                    listen ? OnForegroundImageLoaded : (EventHandler)null);
                
                TextView.SetTextColor(TextView.Paint.Color);
                ImageView.SetColorFilter(TextView.Paint.Color);
            }
        }
        
        private void OnForegroundImageLoaded(object sender, EventArgs e)
        {
            TextView.Paint.SetShader(foreground.GetShader(Width, Height, null));
            TextView.Invalidate();
        }
        
        private void OnImageLoaded(object sender, EventArgs e)
        {
            var source = sender as INativeBitmapImage;
            if (source != null && source.SourceUri == imageUri)
            {
                ImageView.SetImageBitmap(source.GetImageSource());
            }
        }
    }
}

