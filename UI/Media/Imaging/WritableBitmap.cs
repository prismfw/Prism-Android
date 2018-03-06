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


using System.Threading.Tasks;
using Android.Graphics;
using Android.Runtime;
using Prism.Native;

namespace Prism.Android.UI.Media.Imaging
{
    /// <summary>
    /// Represents an Android implementation of an <see cref="INativeWritableBitmap"/>.
    /// </summary>
    [Preserve(AllMembers = true)]
    [Register(typeof(INativeWritableBitmap))]
    public class WritableBitmap : ImageSource, INativeWritableBitmap
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WritableBitmap"/> class.
        /// </summary>
        /// <param name="pixelWidth">The number of pixels along the image's X-axis.</param>
        /// <param name="pixelHeight">The number of pixels along the image's Y-axis.</param>
        public WritableBitmap(int pixelWidth, int pixelHeight)
        {
            SetSource(Bitmap.CreateBitmap(pixelWidth, pixelHeight, Bitmap.Config.Argb8888), false);
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

            OnSourceChanged();
        }
    }
}

