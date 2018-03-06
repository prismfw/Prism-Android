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
using Prism.UI.Media.Imaging;

namespace Prism.Android.UI.Media.Imaging
{
    /// <summary>
    /// Represents the base class for image sources.  This class is abstract.
    /// </summary>
    public class ImageSource
    {
        /// <summary>
        /// Occurs when the underlying image data has changed.
        /// </summary>
        public event EventHandler SourceChanged;

        /// <summary>
        /// Gets the number of pixels along the image's Y-axis.
        /// </summary>
        public virtual int PixelHeight
        {
            get { return Source?.Height ?? 0; }
        }

        /// <summary>
        /// Gets the number of pixels along the image's X-axis.
        /// </summary>
        public virtual int PixelWidth
        {
            get { return Source?.Width ?? 0; }
        }

        /// <summary>
        /// Gets the scaling factor of the image.
        /// </summary>
        public virtual double Scale
        {
            get { return 1; }
        }

        /// <summary>
        /// Gets the image source instance.
        /// </summary>
        public Bitmap Source { get; private set; }

        /// <summary>
        /// Gets the data for the image source as a byte array.
        /// </summary>
        /// <returns>The image data as an <see cref="Array"/> of bytes.</returns>
        public Task<byte[]> GetPixelsAsync()
        {
            return Task.Run(() =>
            {
                if (Source == null)
                {
                    return new byte[0];
                }

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

        /// <summary>
        /// Called when the image source changes significantly.
        /// </summary>
        protected void OnSourceChanged()
        {
            SourceChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Sets the image source.
        /// </summary>
        /// <param name="source">The new image source.</param>
        /// <param name="notify">A value indicating whether to trigger the SourceChanged event.</param>
        protected void SetSource(Bitmap source, bool notify)
        {
            if (Source != source)
            {
                Source = source;
                if (notify)
                {
                    OnSourceChanged();
                }
            }
        }
    }
}

