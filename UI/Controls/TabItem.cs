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
using System.Linq;
using Android.Graphics;
using Android.Runtime;
using Android.Support.Design.Widget;
using Android.Text;
using Android.Util;
using Android.Widget;
using Prism.Native;
using Prism.Systems;
using Prism.UI.Media;
using Android.App;

namespace Prism.Android.UI.Controls
{
    /// <summary>
    /// Represents an Android implementation of an <see cref="INativeTabItem"/>.
    /// </summary>
    [Preserve(AllMembers = true)]
    [Register(typeof(INativeTabItem))]
    public class TabItem : LinearLayout, INativeTabItem
    {
        /// <summary>
        /// Occurs when the value of a property is changed.
        /// </summary>
        public event EventHandler<FrameworkPropertyChangedEventArgs> PropertyChanged;

        /// <summary>
        /// Gets or sets the object that acts as the content of the item.
        /// This is typically an <see cref="IView"/> or <see cref="INativeViewStack"/> instance.
        /// </summary>
        public object Content
        {
            get { return content; }
            set
            {
                content = value;

                var tabView = ObjectRetriever.GetNativeObject(Prism.UI.Window.Current.Content) as INativeTabView;
                if (tabView != null)
                {
                    var view = value as global::Android.Views.View;
                    if (view == null)
                    {
                        var fragment = value as Fragment;
                        if (fragment != null)
                        {
                            var transaction = (tabView as Fragment)?.ChildFragmentManager.BeginTransaction();
                            transaction?.Replace(1, fragment);
                            transaction?.Commit();
                        }
                    }
                    else
                    {
                        var contentViewer = Prism.UI.VisualTreeHelper.GetChild<FrameLayout>(tabView, f => f.Id == 1);
                        if (contentViewer != null && tabView.TabItems[tabView.SelectedIndex] == this)
                        {
                            contentViewer.RemoveAllViews();
                            contentViewer.AddView(view);
                        }
                    }
                }
            }
        }
        private object content;

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
                    OnPropertyChanged(Prism.UI.Controls.TabItem.FontFamilyProperty);
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
                    OnPropertyChanged(Prism.UI.Controls.TabItem.FontSizeProperty);
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
                    OnPropertyChanged(Prism.UI.Controls.TabItem.FontStyleProperty);
                }
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="Brush"/> to apply to the title.
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
                        TextView.Paint.SetBrush(foreground, TextView.Width, (foreground is ImageBrush) ? TextView.Height :
                            (TextView.Paint.FontSpacing + 0.5f), OnForegroundImageLoaded);
                        TextView.SetTextColor(TextView.Paint.Color);
                    }

                    OnPropertyChanged(Prism.UI.Controls.TabItem.ForegroundProperty);
                }
            }
        }
        private Brush foreground;

        /// <summary>
        /// Gets or sets an <see cref="INativeImageSource"/> for an image to display with the item.
        /// </summary>
        public INativeImageSource ImageSource
        {
            get { return imageSource; }
            set
            {
                if (value != imageSource)
                {
                    imageSource.ClearImageHandler(OnImageLoaded);

                    imageSource = value;
                    if (imageSource != null && imageSource.IsLoaded)
                    {
                        OnImageLoaded(null, null);
                    }
                    else
                    {
                        ImageView.SetImageBitmap(imageSource.BeginLoadingImage(OnImageLoaded));
                    }
                    OnPropertyChanged(Prism.UI.Controls.TabItem.ImageSourceProperty);
                }
            }
        }
        private INativeImageSource imageSource;

        /// <summary>
        /// Gets or sets the title for the item.
        /// </summary>
        public string Title
        {
            get { return TextView.Text; }
            set
            {
                if (value != TextView.Text)
                {
                    TextView.Text = value;
                    OnPropertyChanged(Prism.UI.Controls.TabItem.TitleProperty);
                }
            }
        }

        /// <summary>
        /// Gets the view that displays the image of the tab.
        /// </summary>
        protected ImageView ImageView { get; }

        /// <summary>
        /// Gets the view that displays the title text of the tab.
        /// </summary>
        protected TextView TextView { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="TabItem"/> class.
        /// </summary>
        public TabItem()
            : base(Application.MainActivity)
        {
            ImageView = new ImageView(Context);
            TextView = new TextView(Context)
            {
                Ellipsize = TextUtils.TruncateAt.End,
                Gravity = global::Android.Views.GravityFlags.CenterHorizontal,
                Typeface = Typeface.Default
            };
            TextView.SetMaxLines(2);

            Orientation = Orientation.Vertical;
            AddView(ImageView, new LinearLayout.LayoutParams(LayoutParams.MatchParent, LayoutParams.WrapContent));
            AddView(TextView, new LinearLayout.LayoutParams(LayoutParams.MatchParent, LayoutParams.WrapContent));
            SetMinimumWidth(200);
        }

        /// <summary>
        /// Called when a property value is changed.
        /// </summary>
        /// <param name="pd">A property descriptor describing the property whose value has been changed.</param>
        protected virtual void OnPropertyChanged(PropertyDescriptor pd)
        {
            PropertyChanged(this, new FrameworkPropertyChangedEventArgs(pd));
        }

        private void OnForegroundImageLoaded(object sender, EventArgs e)
        {
            TextView.Paint.SetShader(foreground.GetShader(TextView.Width, TextView.Height, null));
        }

        private void OnImageLoaded(object sender, EventArgs e)
        {
            ImageView.SetImageBitmap(imageSource.GetImage());
            imageSource.GetImage().PrepareToDraw();
            ImageView.Invalidate();
        }
    }
}

