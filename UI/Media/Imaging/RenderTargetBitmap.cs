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
using Android.App;
using Android.Graphics;
using Android.Runtime;
using Android.Views;
using Prism.Native;
using Prism.Systems;

namespace Prism.Android.UI.Media.Imaging
{
    /// <summary>
    /// Represents an Android implementation of an <see cref="INativeRenderTargetBitmap"/>.
    /// </summary>
    [Preserve(AllMembers = true)]
    [Register(typeof(INativeRenderTargetBitmap))]
    public class RenderTargetBitmap : ImageSource, INativeRenderTargetBitmap
    {
        /// <summary>
        /// Gets the scaling factor of the image.
        /// </summary>
        public override double Scale
        {
            get { return Device.Current.DisplayScale; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RenderTargetBitmap"/> class.
        /// </summary>
        public RenderTargetBitmap()
        {
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

            var oldSource = Source;
            var newSource = Bitmap.CreateBitmap(view.Width, view.Height, Bitmap.Config.Argb8888);
            view.Draw(new Canvas(newSource));
            
            SetSource(Bitmap.CreateScaledBitmap(newSource, width, height, true), true);

            oldSource?.Recycle();
            return Task.CompletedTask;
        }
    }
}

