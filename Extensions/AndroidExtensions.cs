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


#pragma warning disable 1574

using System;
using System.Linq;
using Android.Content.PM;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Graphics.Drawables.Shapes;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;
using Prism.Android.UI.Media.Imaging;
using Prism.Input;
using Prism.Native;
using Prism.Systems;
using Prism.UI;
using Prism.UI.Media;
using Prism.Utilities;

namespace Prism.Android
{
    /// <summary>
    /// Provides methods for converting Prism objects to Android objects and vice versa.
    /// </summary>
    public static class AndroidExtensions
    {
        private static readonly WeakEventManager imageLoadedEventManager = new WeakEventManager("ImageLoaded", typeof(INativeBitmapImage));

        /// <summary>
        /// Checks the state of the image brush's image.  If the image is not loaded, the specified handler
        /// is attached to the image's load event and loading is initiated.
        /// </summary>
        /// <param name="brush">The brush.</param>
        /// <param name="handler">The handler to attach to the ImageLoaded event of the brush's image if the image is not already loaded.</param>
        /// <returns>If the image is already loaded, the Bitmap instance; otherwise, <c>null</c>.</returns>
        public static Bitmap BeginLoadingImage(this ImageBrush brush, EventHandler handler)
        {
            return (ObjectRetriever.GetNativeObject(brush?.Image) as INativeImageSource).BeginLoadingImage(handler);
        }

        /// <summary>
        /// Checks the state of the image.  If the image is not loaded, the specified handler
        /// is attached to the image's load event and loading is initiated.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="handler">The handler to attach to the ImageLoaded event of the image if the image is not already loaded.</param>
        /// <returns>If the image is already loaded, the Bitmap instance; otherwise, <c>null</c>.</returns>
        public static Bitmap BeginLoadingImage(this INativeImageSource source, EventHandler handler)
        {
            if (source == null)
            {
                return null;
            }
            
            var bitmapImage = source as INativeBitmapImage;
            if (bitmapImage == null)
            {
                return source.GetImageSource();
            }

            if (handler != null)
            {
                imageLoadedEventManager.RemoveHandler(bitmapImage, handler);
                imageLoadedEventManager.AddHandler(bitmapImage, handler);
            }

            if (bitmapImage.IsLoaded)
            {
                imageLoadedEventManager.RemoveHandler(bitmapImage, handler);
                return bitmapImage.GetImageSource();
            }
            else if (bitmapImage.IsFaulted)
            {
                imageLoadedEventManager.RemoveHandler(bitmapImage, handler);
            }
            else
            {
                (bitmapImage as ILazyLoader)?.LoadInBackground();
            }

            return null;
        }

        /// <summary>
        /// Removes the specified handler from the brush image's load event.
        /// </summary>
        /// <param name="brush">The brush.</param>
        /// <param name="handler">The handler to be removed.</param>
        public static void ClearImageHandler(this ImageBrush brush, EventHandler handler)
        {
            var image = ObjectRetriever.GetNativeObject(brush?.Image) as INativeImageSource;
            if (image != null)
            {
                imageLoadedEventManager.RemoveHandler(image, handler);
            }
        }

        /// <summary>
        /// Removes the specified handler from the image's load event.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="handler">The handler to be removed.</param>
        public static void ClearImageHandler(this INativeImageSource source, EventHandler handler)
        {
            if (source != null)
            {
                imageLoadedEventManager.RemoveHandler(source, handler);
            }
        }
        
        /// <summary>
        /// Gets a <see cref="double"/> from a <see cref="float"/>
        /// that is divided by the device's display scale.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        public static double GetScaledDouble(this float value)
        {
            return value / Device.Current.DisplayScale;
        }
        
        /// <summary>
        /// Gets a <see cref="float"/> from a <see cref="double"/>
        /// that is multiplied by the device's display scale.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        public static float GetScaledFloat(this double value)
        {
            return (float)(value * Device.Current.DisplayScale);
        }

        /// <summary>
        /// Gets an <see cref="ActionKeyType"/> from an <see cref="ImeAction"/>.
        /// </summary>
        /// <param name="action">The action.</param>
        public static ActionKeyType GetActionKeyType(this ImeAction action)
        {
            switch (action)
            {
                case ImeAction.Done:
                    return ActionKeyType.Done;
                case ImeAction.Go:
                    return ActionKeyType.Go;
                case ImeAction.Next:
                    return ActionKeyType.Next;
                case ImeAction.Search:
                    return ActionKeyType.Search;
                default:
                    return ActionKeyType.Default;
            }
        }

        /// <summary>
        /// Gets a <see cref="Android.Graphics.Color"/> from a <see cref="Prism.UI.Color"/>.
        /// </summary>
        /// <param name="color">The color.</param>
        public static global::Android.Graphics.Color GetColor(this Prism.UI.Color color)
        {
            return new global::Android.Graphics.Color(color.R, color.G, color.B, color.A);
        }

        /// <summary>
        /// Gets a <see cref="Prism.UI.Color"/> from a <see cref="Android.Graphics.Color"/>.
        /// </summary>
        /// <param name="color">The color.</param>
        public static Prism.UI.Color GetColor(this global::Android.Graphics.Color color)
        {
            return new Prism.UI.Color(color.A, color.R, color.G, color.B);
        }
        
        /// <summary>
        /// Gets a <see cref="DisplayOrientations"/> from an <see cref="Android.Content.Res.Orientation"/>.
        /// </summary>
        /// <param name="orientation">The orientation.</param>
        public static DisplayOrientations GetDisplayOrientations(this global::Android.Content.Res.Orientation orientation)
        {
            return orientation == global::Android.Content.Res.Orientation.Landscape ?
                DisplayOrientations.Landscape : DisplayOrientations.Portrait;
        }

        /// <summary>
        /// Gets a <see cref="Drawable"/> from a <see cref="Brush"/>.
        /// </summary>
        /// <param name="brush">The brush.</param>
        /// <param name="handler">The event handler to invoke when an image brush's image has loaded.</param>
        public static Drawable GetDrawable(this Brush brush, EventHandler handler)
        {
            var dataBrush = brush as DataBrush;
            if (dataBrush != null)
            {
                if (dataBrush.Data is global::Android.Graphics.Color)
                {
                    return new ColorDrawable((global::Android.Graphics.Color)dataBrush.Data);
                }
                return dataBrush.Data as Drawable;
            }
            
            var solidColor = brush as SolidColorBrush;
            if (solidColor != null)
            {
                return new ColorDrawable(solidColor.Color.GetColor());
            }

            var imageBrush = brush as ImageBrush;
            if (imageBrush != null)
            {
                var bitmap = imageBrush.BeginLoadingImage(handler);
                if (bitmap == null)
                {
                    return null;
                }

                var shape = new ShapeDrawable(new RectShape());
                shape.SetShaderFactory(new BrushShaderFactory(imageBrush));
                return shape;
            }

            var linearBrush = brush as LinearGradientBrush;
            if (linearBrush != null)
            {
                if (linearBrush.Colors.Count == 1)
                {
                    return new ColorDrawable(linearBrush.Colors[0].GetColor());
                }
                else if (linearBrush.Colors.Count > 1)
                {
                    var shape = new ShapeDrawable(new RectShape());
                    shape.SetShaderFactory(new BrushShaderFactory(linearBrush));
                    return shape;
                }
            }

            return null;
        }

        /// <summary>
        /// Gets a <see cref="GravityFlags"/> from a <see cref="Prism.UI.TextAlignment"/>.
        /// </summary>
        /// <param name="alignment">The text alignment.</param>
        public static GravityFlags GetGravity(this Prism.UI.TextAlignment alignment)
        {
            switch (alignment)
            {
                case Prism.UI.TextAlignment.Center:
                    return GravityFlags.Center;
                case Prism.UI.TextAlignment.Justified:
                    return GravityFlags.FillHorizontal;
                case Prism.UI.TextAlignment.Right:
                    return GravityFlags.Right;
                default:
                    return GravityFlags.Left;
            }
        }

        /// <summary>
        /// Gets a <see cref="Bitmap"/> from an <see cref="INativeImageSource"/>.
        /// </summary>
        /// <param name="source">The image.</param>
        public static Bitmap GetImageSource(this INativeImageSource source)
        {
            var image = source as IImageSource;
            return image == null ? (object)source as Bitmap : image.Source;
        }

        /// <summary>
        /// Gets an <see cref="ImeAction"/> from an <see cref="ActionKeyType"/>.
        /// </summary>
        /// <param name="keyType">The key type.</param>
        public static ImeAction GetImeAction(this ActionKeyType keyType)
        {
            switch (keyType)
            {
                case ActionKeyType.Done:
                    return ImeAction.Done;
                case ActionKeyType.Go:
                    return ImeAction.Go;
                case ActionKeyType.Next:
                    return ImeAction.Next;
                case ActionKeyType.Search:
                    return ImeAction.Search;
                default:
                    return ImeAction.Unspecified;
            }
        }
        
        /// <summary>
        /// Gets a <see cref="LineCap"/> from a <see cref="Paint.Cap"/>.
        /// </summary>
        /// <param name="cap">The paint cap.</param>
        public static LineCap GetLineCap(this Paint.Cap cap)
        {
            if (cap == Paint.Cap.Square)
                return LineCap.Square;
            if (cap == Paint.Cap.Round)
                return LineCap.Round;
            return LineCap.Flat;
        }

        /// <summary>
        /// Gets a <see cref="LineJoin"/> from a <see cref="Paint.Join"/>.
        /// </summary>
        /// <param name="join">The paint join.</param>
        public static LineJoin GetLineJoin(this Paint.Join join)
        {
            if (join == Paint.Join.Bevel)
                return LineJoin.Bevel;
            if (join == Paint.Join.Round)
                return LineJoin.Round;
            return LineJoin.Miter;
        }

        /// <summary>
        /// Gets a <see cref="Android.Graphics.Matrix"/> from a <see cref="Prism.UI.Media.Matrix"/>.
        /// </summary>
        /// <param name="matrix">The matrix.</param>
        public static global::Android.Graphics.Matrix GetMatrix(this Prism.UI.Media.Matrix matrix)
        {
            var retVal = new global::Android.Graphics.Matrix();
            retVal.SetValues(new[] { (float)matrix.M11, (float)matrix.M21, (float)matrix.OffsetX, (float)matrix.M12, (float)matrix.M22, (float)matrix.OffsetY, 0f, 0f, 1f });
            return retVal;
        }
        
        /// <summary>
        /// Gets a <see cref="Paint.Cap"/> from a <see cref="LineCap"/>.
        /// </summary>
        /// <param name="lineCap">The line cap.</param>
        public static Paint.Cap GetPaintCap(this LineCap lineCap)
        {
            switch (lineCap)
            {
                case LineCap.Square:
                    return Paint.Cap.Square;
                case LineCap.Round:
                    return Paint.Cap.Round;
                default:
                    return Paint.Cap.Butt;
            }
        }

        /// <summary>
        /// Gets a <see cref="Paint.Join"/> from a <see cref="LineJoin"/>.
        /// </summary>
        /// <param name="lineJoin">The line join.</param>
        public static Paint.Join GetPaintJoin(this LineJoin lineJoin)
        {
            switch (lineJoin)
            {
                case LineJoin.Bevel:
                    return Paint.Join.Bevel;
                case LineJoin.Round:
                    return Paint.Join.Round;
                default:
                    return Paint.Join.Miter;
            }
        }

        /// <summary>
        /// Generates a <see cref="PointerEventArgs"/> from a <see cref="MotionEvent"/>.
        /// </summary>
        /// <param name="evt">The event.</param>
        /// <param name="source">The source of the event.</param>
        public static PointerEventArgs GetPointerEventArgs(this MotionEvent evt, object source)
        {
            return new PointerEventArgs(source, evt.GetToolType(evt.ActionIndex).GetPointerType(),
                new Point(evt.GetX() / Device.Current.DisplayScale, evt.GetY() / Device.Current.DisplayScale), evt.Pressure, evt.EventTime);
        }
        
        /// <summary>
        /// Gets a <see cref="PointerType"/> from a <see cref="MotionEventToolType"/>.
        /// </summary>
        /// <param name="type">The tool type.</param>
        public static PointerType GetPointerType(this MotionEventToolType type)
        {
            switch (type)
            {
                case MotionEventToolType.Finger:
                    return PointerType.Touch;
                case MotionEventToolType.Stylus:
                    return PointerType.Stylus;
                case MotionEventToolType.Mouse:
                    return PointerType.Mouse;
                case MotionEventToolType.Eraser:
                    return PointerType.Other;
                default:
                    return PointerType.Unknown;
            }
        }
        
        /// <summary>
        /// Gets a <see cref="Rectangle"/> from a <see cref="RectF"/>.
        /// </summary>
        /// <param name="rect">The rectangle.</param>
        public static Rectangle GetRectangle(this RectF rect)
        {
            return new Rectangle(rect.Left / Device.Current.DisplayScale, rect.Top / Device.Current.DisplayScale,
                (rect.Right - rect.Left) / Device.Current.DisplayScale, (rect.Bottom - rect.Top) / Device.Current.DisplayScale);
        }

        /// <summary>
        /// Gets an <see cref="ImageView.ScaleType"/> from a <see cref="Stretch"/>.
        /// </summary>
        /// <param name="stretch">The stretch.</param>
        public static ImageView.ScaleType GetScaleType(this Stretch stretch)
        {
            switch (stretch)
            {
                case Stretch.Fill:
                    return ImageView.ScaleType.FitXy;
                case Stretch.Uniform:
                    return ImageView.ScaleType.FitCenter;
                case Stretch.UniformToFill:
                    return ImageView.ScaleType.CenterCrop;
                default:
                    return ImageView.ScaleType.Center;
            }
        }
        
        /// <summary>
        /// Gets a <see cref="ScreenOrientation"/> from a <see cref="DisplayOrientations"/>.
        /// </summary>
        /// <param name="orientation">The orientation.</param>
        public static ScreenOrientation GetScreenOrientation(this DisplayOrientations orientation)
        {
            if (orientation.HasFlag(DisplayOrientations.Portrait) && !orientation.HasFlag(DisplayOrientations.Landscape))
            {
                return ScreenOrientation.SensorPortrait;
            }
            
            if (orientation.HasFlag(DisplayOrientations.Landscape) && !orientation.HasFlag(DisplayOrientations.Portrait))
            {
                return ScreenOrientation.SensorLandscape;
            }
            
            return ScreenOrientation.Unspecified;
        }

        /// <summary>
        /// Gets a <see cref="Shader"/> for the specified width and height.
        /// </summary>
        /// <param name="brush">The brush.</param>
        /// <param name="width">The width of the area to which the shader will be applied.</param>
        /// <param name="height">The height of the area to which the shader will be applied.</param>
        /// <param name="handler">The event handler to invoke when an image brush's image has loaded.</param>
        public static Shader GetShader(this Brush brush, float width, float height, EventHandler handler)
        {
            var imageBrush = brush as ImageBrush;
            if (imageBrush != null)
            {
                var bitmap = imageBrush.BeginLoadingImage(handler);
                if (bitmap == null)
                {
                    return null;
                }

                var shader = new BitmapShader(bitmap, Shader.TileMode.Repeat, Shader.TileMode.Repeat);
                var matrix = new global::Android.Graphics.Matrix();
                switch (imageBrush.Stretch)
                {
                    case Stretch.Fill:
                        matrix.SetScale(width / bitmap.Width, height / bitmap.Height);
                        break;
                    case Stretch.Uniform:
                        float scale = Math.Min(width / bitmap.Width, height / bitmap.Height);
                        matrix.SetScale(scale, scale);
                        break;
                    case Stretch.UniformToFill:
                        scale = Math.Max(width / bitmap.Width, height / bitmap.Height);
                        matrix.SetScale(scale, scale);
                        break;
                    default:
                        matrix.Reset();
                        break;
                }

                shader.SetLocalMatrix(matrix);
                return shader;
            }

            var linearBrush = brush as LinearGradientBrush;
            if (linearBrush != null)
            {
                int[] colors = linearBrush.Colors.Count == 1 ?
                    new[] { linearBrush.Colors[0].GetHashCode(), 0 } :
                    linearBrush.Colors.Select(c => c.GetHashCode()).ToArray();

                return new LinearGradient(
                    (float)linearBrush.StartPoint.X * width,
                    (float)linearBrush.StartPoint.Y * height,
                    (float)linearBrush.EndPoint.X * width,
                    (float)linearBrush.EndPoint.Y * height,
                    colors, null, Shader.TileMode.Repeat);
            }

            return null;
        }

        /// <summary>
        /// Gets a <see cref="Stretch"/> from an <see cref="ImageView.ScaleType"/>.
        /// </summary>
        /// <param name="scaleType">The scale type.</param>
        public static Stretch GetStretch(this ImageView.ScaleType scaleType)
        {
            if (scaleType == ImageView.ScaleType.FitXy)
                return Stretch.Fill;

            if (scaleType == ImageView.ScaleType.FitCenter)
                return Stretch.Uniform;

            if (scaleType == ImageView.ScaleType.CenterCrop)
                return Stretch.UniformToFill;

            return Stretch.None;
        }

        /// <summary>
        /// Gets a <see cref="Prism.UI.TextAlignment"/> from a <see cref="GravityFlags"/>.
        /// </summary>
        /// <param name="gravity">The gravity.</param>
        public static Prism.UI.TextAlignment GetTextAlignment(this GravityFlags gravity)
        {
            switch (gravity)
            {
                case GravityFlags.Center:
                case GravityFlags.CenterHorizontal:
                    return Prism.UI.TextAlignment.Center;
                case GravityFlags.FillHorizontal:
                    return Prism.UI.TextAlignment.Justified;
                case GravityFlags.Right:
                    return Prism.UI.TextAlignment.Right;
                default:
                    return Prism.UI.TextAlignment.Left;
            }
        }

        /// <summary>
        /// Gets a <see cref="ViewStates"/> from a <see cref="Visibility"/>.
        /// </summary>
        /// <param name="visibility">The visibility.</param>
        public static ViewStates GetViewStates(this Visibility visibility)
        {
            switch (visibility)
            {
                case Visibility.Collapsed:
                    return ViewStates.Gone;
                case Visibility.Hidden:
                    return ViewStates.Invisible;
                default:
                    return ViewStates.Visible;
            }
        }

        /// <summary>
        /// Gets a <see cref="Visibility"/> from a <see cref="ViewStates"/>.
        /// </summary>
        /// <param name="state">The state.</param>
        public static Visibility GetVisibility(this ViewStates state)
        {
            switch (state)
            {
                case ViewStates.Gone:
                    return Visibility.Collapsed;
                case ViewStates.Invisible:
                    return Visibility.Hidden;
                default:
                    return Visibility.Visible;
            }
        }

        /// <summary>
        /// Applies the properties of the specified <see cref="Brush"/> to the paint instance.
        /// </summary>
        /// <param name="paint">The paint.</param>
        /// <param name="brush">The brush whose properties are to be applied.</param>
        /// <param name="width">The width of the paint area.</param>
        /// <param name="height">The height of the paint area.</param>
        /// <param name="handler">The event handler to invoke when an image brush's image has loaded.</param>
        public static void SetBrush(this Paint paint, Brush brush, float width, float height, EventHandler handler)
        {
            var dataBrush = brush as DataBrush;
            if (dataBrush != null)
            {
                if (dataBrush.Data is global::Android.Graphics.Color)
                {
                    paint.SetShader(null);
                    paint.Color = ((global::Android.Graphics.Color)dataBrush.Data);
                }
                else
                {
                    paint.Color = global::Android.Graphics.Color.Black;
                    paint.SetShader(dataBrush.Data as Shader);
                }
                return;
            }
            
            var solidColor = brush as SolidColorBrush;
            if (solidColor != null)
            {
                paint.SetShader(null);
                paint.Color = solidColor.Color.GetColor();
                return;
            }

            paint.Color = global::Android.Graphics.Color.Black;
            paint.SetShader(brush.GetShader(width, height, handler));
        }
    }

    internal class BrushShaderFactory : ShapeDrawable.ShaderFactory
    {
        private readonly Brush brush;

        public BrushShaderFactory(Brush brush)
        {
            this.brush = brush;
        }

        public override Shader Resize(int width, int height)
        {
            return brush.GetShader(width, height, null);
        }
    }
}

