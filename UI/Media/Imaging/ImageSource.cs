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
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Android.Graphics;
using Android.Runtime;
using Prism.Native;
using Prism.UI.Media.Imaging;

namespace Prism.Android.UI.Media.Imaging
{
    /// <summary>
    /// Represents an Android implementation of an <see cref="INativeImageSource"/>.
    /// </summary>
    [Preserve(AllMembers = true)]
    [Register(typeof(INativeImageSource))]
    public class ImageSource : INativeImageSource, ILazyLoader
    {
        /// <summary>
        /// Occurs when the image fails to load.
        /// </summary>
        public event EventHandler<ErrorEventArgs> ImageFailed;

        /// <summary>
        /// Occurs when the image has been loaded into memory.
        /// </summary>
        public event EventHandler ImageLoaded;

        /// <summary>
        /// Gets a value indicating whether the image has encountered an error during loading.
        /// </summary>
        public bool IsFaulted { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the image has been loaded into memory.
        /// </summary>
        public bool IsLoaded { get; private set; }

        /// <summary>
        /// Gets the number of pixels along the image's Y-axis.
        /// </summary>
        public int PixelHeight
        {
            get { return Bitmap?.Height ?? 0; }
        }

        /// <summary>
        /// Gets the number of pixels along the image's X-axis.
        /// </summary>
        public int PixelWidth
        {
            get { return Bitmap?.Width ?? 0; }
        }

        /// <summary>
        /// Gets the URI of the source file containing the image data.
        /// </summary>
        public Uri SourceUri { get; private set; }

        /// <summary>
        /// Gets the <see cref="Bitmap"/> containing the image data.
        /// </summary>
        public Bitmap Bitmap { get; private set; }

        private byte[] imageBytes;

        /// <summary>
        /// Initializes a new instance of the <see cref="ImageSource"/> class.
        /// </summary>
        /// <param name="sourceUri">The URI of the source file containing the image data.</param>
        /// <param name="cachedImage">The image that was pulled from the image cache, or <c>null</c> if nothing was pulled from the cache.</param>
        public ImageSource(Uri sourceUri, INativeImageSource cachedImage)
        {
            SourceUri = sourceUri;

            var cached = cachedImage as ImageSource;
            if (cached != null)
            {
                Bitmap = cached.Bitmap;
                IsLoaded = cached.IsLoaded;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ImageSource"/> class.
        /// </summary>
        /// <param name="imageData">The byte array containing the data for the image.</param>
        public ImageSource(byte[] imageData)
        {
            imageBytes = imageData;
        }

        /// <summary>
        /// Loads the contents of the object in a background thread.
        /// </summary>
        public void LoadInBackground()
        {
            var context = SynchronizationContext.Current ?? new SynchronizationContext();
            ThreadPool.QueueUserWorkItem((o) =>
            {
                lock (this)
                {
                    if (Bitmap == null && !IsFaulted && (imageBytes != null || SourceUri != null))
                    {
                        try
                        {
                            if (imageBytes != null)
                            {
                                Bitmap = BitmapFactory.DecodeByteArray(imageBytes, 0, imageBytes.Length);
                                imageBytes = null;
                            }
                            else if (!SourceUri.IsAbsoluteUri || SourceUri.IsFile)
                            {
                                Bitmap = BitmapFactory.DecodeFile(SourceUri.OriginalString);
                                if (Bitmap == null && SourceUri.OriginalString.StartsWith(Prism.IO.Directory.AssetDirectory))
                                {
                                    using (var stream = Application.MainActivity.Assets.Open(SourceUri.OriginalString.Remove(0, Prism.IO.Directory.AssetDirectory.Length)))
                                    {
                                        Bitmap = BitmapFactory.DecodeStream(stream);
                                    }
                                }
                            }
                            else
                            {
                                var url = new Java.Net.URL(SourceUri.OriginalString);
                                using (var stream = url.OpenStream())
                                {
                                    Bitmap = BitmapFactory.DecodeStream(stream);
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            context.Post((obj) => OnImageFailed(e), null);
                            return;
                        }

                        if (Bitmap == null)
                        {
                            context.Post((obj) => OnImageFailed(null), null);
                            return;
                        }
                    }

                    if (!IsLoaded)
                    {
                        context.Post((obj) => OnImageLoaded(), null);
                    }
                }
            }, this);
        }

        /// <summary>
        /// Saves the image data to a file at the specified path using the specified file format.
        /// </summary>
        /// <param name="filePath">The path to the file in which to save the image data.</param>
        /// <param name="fileFormat">The file format to with which to save the image data.</param>
        public async Task SaveAsync(string filePath, ImageFileFormat fileFormat)
        {
            using (var stream = new MemoryStream())
            {
                if (fileFormat == ImageFileFormat.Jpeg)
                {
                    await Bitmap.CompressAsync(Bitmap.CompressFormat.Jpeg, 100, stream);
                }
                else
                {
                    await Bitmap.CompressAsync(Bitmap.CompressFormat.Png, 100, stream);
                }

                stream.Position = 0;
                await Prism.IO.File.WriteAllBytesAsync(filePath, stream.GetBuffer());
            }
        }

        /// <summary>
        /// Raises the image failed event.
        /// </summary>
        /// <param name="e">The exception that describes the failure.</param>
        protected void OnImageFailed(Exception e)
        {
            IsFaulted = true;
            ImageFailed(this, new ErrorEventArgs(e));
        }

        /// <summary>
        /// Raises the image loaded event.
        /// </summary>
        protected void OnImageLoaded()
        {
            IsLoaded = true;
            ImageLoaded(this, EventArgs.Empty);
        }
    }
}

