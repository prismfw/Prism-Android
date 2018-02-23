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
using System.IO;
using System.Threading.Tasks;
using Android.App;
using Android.Graphics;
using Android.Runtime;
using Android.Views;
using Prism.Native;
using Prism.Systems;
using Prism.UI.Media.Imaging;

namespace Prism.Android.UI.Media.Imaging
{
    /// <summary>
    /// Represents an Android implementation of an <see cref="INativeRenderTargetBitmap"/>.
    /// </summary>
    [Preserve(AllMembers = true)]
    [Register(typeof(INativeRenderTargetBitmap))]
    public class RenderTargetBitmap : INativeRenderTargetBitmap, IImageSource
    {
        /// <summary>
        /// Gets the number of pixels along the image's Y-axis.
        /// </summary>
        public int PixelHeight
        {
            get { return Source?.Height ?? 0; }
        }

        /// <summary>
        /// Gets the number of pixels along the image's X-axis.
        /// </summary>
        public int PixelWidth
        {
            get { return Source?.Width ?? 0; }
        }
        
        /// <summary>
        /// Gets the scaling factor of the image.
        /// </summary>
        public double Scale
        {
            get { return Device.Current.DisplayScale; }
        }
        
        /// <summary>
        /// Gets the image source instance.
        /// </summary>
        public Bitmap Source { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="RenderTargetBitmap"/> class.
        /// </summary>
        public RenderTargetBitmap()
        {
        }
        
        /// <summary>
        /// Gets the data for the captured image as a byte array.
        /// </summary>
        /// <returns>The image data as an <see cref="Array"/> of bytes.</returns>
        public async Task<byte[]> GetPixelsAsync()
        {
            if (Source == null)
            {
                return new byte[0];
            }
            
            using (var stream = new MemoryStream())
            {
                await Source.CompressAsync(Bitmap.CompressFormat.Png, 100, stream);
                return stream.ToArray();
            }
        }

        /// <summary>
        /// Renders a snapshot of the specified visual object.
        /// </summary>
        /// <param name="target">The visual object to render.    This value can be <c>null</c> to render the entire visual tree.</param>
        /// <param name="width">The width of the snapshot.</param>
        /// <param name="height">The height of the snapshot.</param>
        public Task RenderAsync(INativeVisual target, int width, int height)
        {
            width = width.GetScaledInt();
            height = height.GetScaledInt();
        
            var view = target as View ?? (target as Fragment)?.View ?? Application.MainActivity.Window.DecorView;
            view.Layout(view.Left, view.Top, view.Right, view.Bottom);
            
            Source = Bitmap.CreateBitmap(view.Width, view.Height, Bitmap.Config.Argb8888);
            view.Draw(new Canvas(Source));
            
            Source = Bitmap.CreateScaledBitmap(Source, width, height, true);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Saves the image data to a file at the specified path using the specified file format.
        /// </summary>
        /// <param name="filePath">The path to the file in which to save the image data.</param>
        /// <param name="fileFormat">The file format in which to save the image data.</param>
        public async Task SaveAsync(string filePath, ImageFileFormat fileFormat)
        {
            using (var stream = new MemoryStream())
            {
                if (fileFormat == ImageFileFormat.Jpeg)
                {
                    await Source?.CompressAsync(Bitmap.CompressFormat.Jpeg, 100, stream);
                }
                else
                {
                    await Source?.CompressAsync(Bitmap.CompressFormat.Png, 100, stream);
                }

                stream.Position = 0;
                await Prism.IO.File.WriteAllBytesAsync(filePath, stream.GetBuffer());
            }
        }
    }
}

