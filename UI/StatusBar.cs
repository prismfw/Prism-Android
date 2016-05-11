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


using System.Threading.Tasks;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Prism.Native;
using Prism.Systems;
using Prism.UI;
using Prism.Utilities;

namespace Prism.Android.UI
{
    /// <summary>
    /// Represents an Android implementation of an <see cref="INativeStatusBar"/>.
    /// </summary>
    [Preserve(AllMembers = true)]
    [Register(typeof(INativeStatusBar))]
    public class StatusBar : INativeStatusBar
    {
        /// <summary>
        /// Gets or sets the background color for the status bar.
        /// </summary>
        public Color BackgroundColor
        {
            get { return new Color(Application.MainActivity.Window.StatusBarColor); }
            set
            {
                if (Build.VERSION.SdkInt >= BuildVersionCodes.Lollipop)
                {
                    Application.MainActivity.Window.SetStatusBarColor(value.GetColor());
                }
                else
                {
                    Logger.Warn("Setting StatusBar BackgroundColor requires Android 5.0 (Lollipop) or later.");
                }
            }
        }

        /// <summary>
        /// Gets a rectangle describing the area that the status bar is consuming.
        /// </summary>
        public Rectangle Frame
        {
            get
            {
                var frame = new global::Android.Graphics.Rect();
                Application.MainActivity.Window.DecorView.GetWindowVisibleDisplayFrame(frame);
                return new Rectangle(0, 0, frame.Right / Device.Current.DisplayScale, frame.Top / Device.Current.DisplayScale);
            }
        }

        /// <summary>
        /// Gets a value indicating whether the status bar is visible.
        /// </summary>
        public bool IsVisible
        {
            get { return ((int)Application.MainActivity.Window.DecorView.SystemUiVisibility & (int)SystemUiFlags.Fullscreen) == 0; }
        }

        /// <summary>
        /// Gets or sets the style of the status bar.
        /// </summary>
        public StatusBarStyle Style
        {
            get { return style; }
            set
            {
                style = value;
                if (IsVisible)
                {
                    SetStyle();
                }
            }
        }
        private StatusBarStyle style;
    
        /// <summary>
        /// Initializes a new instance of the <see cref="StatusBar"/> class.
        /// </summary>
        public StatusBar()
        {
        }
        
        /// <summary>
        /// Hides the status bar from view.
        /// </summary>
        public Task HideAsync()
        {
            Application.MainActivity.Window.DecorView.SystemUiVisibility = (StatusBarVisibility)SystemUiFlags.Fullscreen;
            return Task.CompletedTask;
        }

        /// <summary>
        /// Shows the status bar if it is not visible.
        /// </summary>
        public Task ShowAsync()
        {
            SetStyle();
            return Task.CompletedTask;
        }
        
        private void SetStyle()
        {
            int visibility = (int)SystemUiFlags.Visible;
            if (Style == StatusBarStyle.Default || Style == StatusBarStyle.Dark)
            {
                visibility &= (int)~SystemUiFlags.LightStatusBar;
            }
            else if (Style == StatusBarStyle.Light)
            {
                visibility |= (int)SystemUiFlags.LightStatusBar;
            }
            else
            {
                visibility = (int)Style;
            }
            
            Application.MainActivity.Window.DecorView.SystemUiVisibility = (StatusBarVisibility)visibility;
        }
    }
}