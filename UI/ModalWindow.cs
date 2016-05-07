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


#pragma warning disable 1998

using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Prism.Native;
using Prism.Systems;
using Prism.UI;
using Prism.Utilities;

namespace Prism.Android.UI
{
    /// <summary>
    /// Represents an Android implementation for a modal <see cref="INativeWindow"/>.
    /// </summary>
    [Preserve(AllMembers = true)]
    [Register(typeof(INativeWindow), Name = "modal")]
    public class ModalWindow : DialogFragment, INativeWindow
    {
        /// <summary>
        /// Occurs when the window gains focus.
        /// </summary>
        public event EventHandler Activated;

        /// <summary>
        /// Occurs when the window is about to be closed.
        /// </summary>
        public event EventHandler<CancelEventArgs> Closing;

        /// <summary>
        /// Occurs when the window loses focus.
        /// </summary>
        public event EventHandler Deactivated;

        /// <summary>
        /// Occurs when the size of the window has changed.
        /// </summary>
        public event EventHandler<WindowSizeChangedEventArgs> SizeChanged;

        /// <summary>
        /// Gets or sets the object that acts as the content of the window.
        /// This is typically an <see cref="IView"/> or <see cref="INativeViewStack"/> instance.
        /// </summary>
        public object Content
        {
            get { return content; }
            set
            {
                if (value == content)
                {
                    return;
                }

                content = value;

                if (!IsVisible)
                {
                    return;
                }

                var view = value as global::Android.Views.View;
                if (view != null)
                {
                    Dialog?.SetContentView(view);
                    return;
                }

                var fragment = value as Fragment;
                if (fragment != null)
                {
                    var fragmentTransaction = ChildFragmentManager.BeginTransaction();
                    fragmentTransaction.Replace(1, fragment);
                    fragmentTransaction.Commit();
                }
            }
        }
        private object content;

        /// <summary>
        /// Gets the height of the window.
        /// </summary>
        public double Height
        {
            get { return View == null ? 0 : View.Height / Device.Current.DisplayScale; }
            set { Logger.Warn("Setting window height is not supported on this platform.  Ignoring."); }
        }

        /// <summary>
        /// Gets the width of the window.
        /// </summary>
        public double Width
        {
            get { return View == null ? 0 : View.Width / Device.Current.DisplayScale; }
            set { Logger.Warn("Setting window width is not supported on this platform.  Ignoring."); }
        }

        private Size currentSize;

        /// <summary>
        /// Initializes a new instance of the <see cref="ModalWindow"/> class.
        /// </summary>
        public ModalWindow()
        {
            Cancelable = false;
        }

        /// <summary>
        /// Attempts to close the window.
        /// </summary>
        /// <param name="animate">Whether to use any system-defined transition animation.</param>
        public void Close(Animate animate)
        {
            if (!IsVisible)
            {
                return;
            }

            var args = new CancelEventArgs();
            Closing(this, args);

            if (args.Cancel)
            {
                return;
            }

            Dismiss();
        }

        /// <summary>
        /// Displays the window if it is not already visible.
        /// </summary>
        /// <param name="animate">Whether to use any system-defined transition animation.</param>
        public void Show(Animate animate)
        {
            if (IsVisible)
            {
                return;
            }

            base.Show(Application.MainActivity.FragmentManager, null);
            Activated(this, EventArgs.Empty);
        }

        /// <summary>
        /// Captures the contents of the window in an image and returns the result.
        /// </summary>
        public async Task<Prism.UI.Media.Imaging.ImageSource> TakeScreenshotAsync()
        {
            if (View == null)
            {
                return new Prism.UI.Media.Imaging.ImageSource(new byte[0]);
            }

            View.Layout(0, 0, View.LayoutParameters.Width, View.LayoutParameters.Height);
            MemoryStream save;
            using (var bitmap = Bitmap.CreateBitmap(View.Width, View.Height, Bitmap.Config.Argb8888))
            {
                var canvas = new Canvas(bitmap);
                View.Draw(canvas);
                save = new MemoryStream();
                bitmap.Compress(Bitmap.CompressFormat.Png, 100, save);
            }
            save.Position = 0;
            return new Prism.UI.Media.Imaging.ImageSource(save.GetBuffer());
        }

        /// <summary>
        /// Called to have the fragment instantiate its user interface view.
        /// </summary>
        /// <param name="inflater"></param>
        /// <param name="container"></param>
        /// <param name="savedInstanceState"></param>
        public override global::Android.Views.View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var rootView = content as global::Android.Views.View;
            if (rootView?.Parent != null)
            {
                ((ViewGroup)rootView.Parent).RemoveView(rootView);
            }

            container = container ?? new FrameLayout(Activity);
            container.RemoveAllViews();
            container.Id = 1;
            container.LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent);

            container.LayoutChange -= OnLayoutChanged;
            container.LayoutChange += OnLayoutChanged;

            if (rootView != null)
            {
                container.AddView(rootView);
            }
            else
            {
                var fragment = content as Fragment;
                if (fragment != null)
                {
                    var fragmentTransaction = ChildFragmentManager.BeginTransaction();
                    fragmentTransaction.Replace(1, fragment);
                    fragmentTransaction.Commit();
                }
            }

            return container;
        }

        /// <summary>
        /// This method will be invoked when the dialog is dismissed.
        /// </summary>
        /// <param name="dialog"></param>
        public override void OnDismiss(IDialogInterface dialog)
        {
            base.OnDismiss(dialog);
            Deactivated(this, EventArgs.Empty);
        }

        private void OnLayoutChanged(object sender, global::Android.Views.View.LayoutChangeEventArgs e)
        {
            var newSize = new Size(e.Right - e.Left, e.Bottom - e.Top);
            if (currentSize != newSize)
            {
                SizeChanged(this, new WindowSizeChangedEventArgs(currentSize, newSize));
                currentSize = newSize;
            }
        }
    }
}

