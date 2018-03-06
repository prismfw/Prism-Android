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
using Android.Graphics;
using Android.Runtime;
using Prism.Native;
using Prism.Systems;
using Prism.UI.Media.Imaging;

namespace Prism.Android.UI.Media.Imaging
{
    /// <summary>
    /// Represents an Android implementation of an <see cref="INativeWritableBitmap"/>.
    /// </summary>
    [Preserve(AllMembers = true)]
    [Register(typeof(INativeWritableBitmap))]
    public class WritableBitmap : INativeWritableBitmap, IImageSource
    {
        /// <summary>
        /// Occurs when the underlying image data has changed.
        /// </summary>
        public event EventHandler SourceChanged;

        /// <summary>
        /// Gets the number of pixels along the image's Y-axis.
        /// </summary>
        public int PixelHeight
        {
            get { return Source.Height; }
        }

        /// <summary>
        /// Gets the number of pixels along the image's X-axis.
        /// </summary>
        public int PixelWidth
        {
            get { return Source.Width; }
        }
        
        /// <summary>
        /// Gets the scaling factor of the image.
        /// </summary>
        public double Scale
        {
            get { return 1; }
        }
        
        /// <summary>
        /// Gets the image source instance.
        /// </summary>
        public Bitmap Source { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="WritableBitmap"/> class.
        /// </summary>
        /// <param name="pixelWidth">The number of pixels along the image's X-axis.</param>
        /// <param name="pixelHeight">The number of pixels along the image's Y-axis.</param>
        public WritableBitmap(int pixelWidth, int pixelHeight)
        {
            Source = Bitmap.CreateBitmap(pixelWidth, pixelHeight, Bitmap.Config.Argb8888);
        }
        
        /// <summary>
        /// Gets the data for the captured image as a byte array.
        /// </summary>
        /// <returns>The image data as an <see cref="Array"/> of bytes.</returns>
        public Task<byte[]> GetPixelsAsync()
        {
            return Task.Run(() =>
            {
                var retVal = new byte[Source.Width * Source.Height * 4];
                var pixels = new int[Source.Width * Source.Height];
                Source.GetPixels(pixels, 0, Source.Width, 0, 0, Source.Width, Source.Height);
                for (int i = 0; i < retVal.Length; i += 4)
                {
                    int argb = pixels[i / 4];
                    retVal[i] = (byte)(argb >> 24 & 0xFF);
                    retVal[i + 1] = (byte)(argb >> 16 & 0xFF);
                    retVal[i + 2] = (byte)(argb >> 8 & 0xFF);
                    retVal[i + 3] = (byte)(argb & 0xFF);
                }

                return retVal;
            });
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
                    await Source.CompressAsync(Bitmap.CompressFormat.Jpeg, 100, stream);
                }
                else
                {
                    await Source.CompressAsync(Bitmap.CompressFormat.Png, 100, stream);
                }

                stream.Position = 0;
                await Prism.IO.File.WriteAllBytesAsync(filePath, stream.GetBuffer());
            }
        }

        /// <summary>
        /// Sets the pixel data of the bitmap to the specified byte array.
        /// </summary>
        /// <param name="pixelData">The byte array containing the pixel data.</param>
        public async Task SetPixelsAsync(byte[] pixelData)
        {
            await Task.Run(() =>
            {
                var colors = new int[pixelData.Length / 4];
                for (int i = 0; i < pixelData.Length; i += 4)
                {
                    colors[i / 4] = (pixelData[i] << 24 | pixelData[i + 1] << 16 | pixelData[i + 2] << 8 | pixelData[i + 3]);
                }

                Source.SetPixels(colors, 0, Source.Width, 0, 0, Source.Width, Source.Height);
            });

            SourceChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}

