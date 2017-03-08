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
using System.Collections.Generic;
using System.Linq;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Prism.Native;
using Prism.Systems;
using Prism.UI;

namespace Prism.Android.UI
{
    /// <summary>
    /// Represents an Android implementation of an <see cref="INativeWindow"/>.
    /// </summary>
    [Preserve(AllMembers = true)]
    [Register(typeof(INativeWindow))]
    public class Window : INativeWindow
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
        /// Occurs when the orientation of the rendered content has changed.
        /// </summary>
        public event EventHandler<DisplayOrientationChangedEventArgs> OrientationChanged;

        /// <summary>
        /// Occurs when the size of the window has changed.
        /// </summary>
        public event EventHandler<WindowSizeChangedEventArgs> SizeChanged;
        
        /// <summary>
        /// Gets or sets the preferred orientations in which to automatically rotate the window in response to orientation changes of the physical device.
        /// </summary>
        public DisplayOrientations AutorotationPreferences
        {
            get { return autorotationPreferences; }
            set
            {
                autorotationPreferences = value;
                Application.MainActivity.RequestedOrientation = autorotationPreferences.GetScreenOrientation();
            }
        }
        private DisplayOrientations autorotationPreferences;

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

                var view = content as global::Android.Views.View ?? Application.MainActivity.FindViewById<FrameLayout>(1);
                if (view != null)
                {
                    view.LayoutChange -= OnLayoutChanged;
                }

                content = value;

                view = value as global::Android.Views.View;
                if (view != null)
                {
                    view.LayoutChange += OnLayoutChanged;
                    Application.MainActivity.SetContentView(view);
                    return;
                }

                var fragment = value as Fragment;
                if (fragment != null)
                {
                    var fl = new FrameLayout(Application.MainActivity)
                    {
                        Id = 1,
                        LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent)
                    };
                    fl.SetFitsSystemWindows(true);
                    fl.LayoutChange += OnLayoutChanged;

                    Application.MainActivity.SetContentView(fl);

                    var fragmentTransaction = Application.MainActivity.FragmentManager.BeginTransaction();
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
            get
            {
                Rect frame = new Rect();
                Application.MainActivity.Window.DecorView.GetWindowVisibleDisplayFrame(frame);
                return frame.Height() / Device.Current.DisplayScale;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is currently visible.
        /// </summary>
        public bool IsVisible
        {
            get { return Application.MainActivity.HasWindowFocus; }
        }
        
        /// <summary>
        /// Gets the current orientation of the rendered content within the window.
        /// </summary>
        public DisplayOrientations Orientation
        {
            get { return Application.MainActivity.Resources.Configuration.Orientation.GetDisplayOrientations(); }
        }
        
        /// <summary>
        /// Gets or sets the style for the window.
        /// </summary>
        public WindowStyle Style { get; set; }

        /// <summary>
        /// Gets the width of the window.
        /// </summary>
        public double Width
        {
            get
            {
                Rect frame = new Rect();
                Application.MainActivity.Window.DecorView.GetWindowVisibleDisplayFrame(frame);
                return frame.Width() / Device.Current.DisplayScale;
            }
        }

        private Size currentSize;

        /// <summary>
        /// Initializes a new instance of the <see cref="Window"/> class.
        /// </summary>
        public Window()
        {
            Application.MainActivity.Window.ClearFlags(WindowManagerFlags.TranslucentStatus);
            Application.MainActivity.Window.AddFlags(WindowManagerFlags.DrawsSystemBarBackgrounds);
        
            Application.MainActivityChanged += (sender, e) =>
            {
                e.OldActivity.Window.Callback = null;
                e.NewActivity.Window.Callback = new MainWindowCallback();
                
                e.NewActivity.RequestedOrientation = autorotationPreferences.GetScreenOrientation();
            };

            Application.MainActivity.Window.Callback = new MainWindowCallback();
        }

        /// <summary>
        /// Attempts to close the window.
        /// </summary>
        public void Close()
        {
            var args = new CancelEventArgs();
            Closing(this, args);

            if (args.Cancel)
                return;

            Application.MainActivity.Finish();
        }
        
        /// <summary>
        /// Sets the preferred minimum size of the window.
        /// </summary>
        /// <param name="minSize">The preferred minimum size.</param>
        public void SetPreferredMinSize(Size minSize) { }

        /// <summary>
        /// Displays the window if it is not already visible.
        /// </summary>
        public void Show()
        {
            Application.MainActivity.StartActivity(new Intent(Application.MainActivity, GetType()));
        }
        
        /// <summary>
        /// Attempts to resize the window to the specified size.
        /// </summary>
        /// <param name="newSize">The width and height at which to size the window.</param>
        /// <returns><c>true</c> if the window was successfully resized; otherwise, <c>false</c>.</returns>
        public bool TryResize(Size newSize)
        {
            return false;
        }

        internal void OnActivated()
        {
            Activated?.Invoke(this, EventArgs.Empty);
        }

        internal void OnDeactivated()
        {
            Deactivated?.Invoke(this, EventArgs.Empty);
        }
        
        internal void OnOrientationChanged(global::Android.Content.Res.Orientation orientation)
        {
            OrientationChanged(this, new DisplayOrientationChangedEventArgs(orientation.GetDisplayOrientations()));
        }

        private void OnLayoutChanged(object sender, global::Android.Views.View.LayoutChangeEventArgs e)
        {
            Rect frame = new Rect();
            Application.MainActivity.Window.DecorView.GetWindowVisibleDisplayFrame(frame);

            var newSize = new Size((frame.Right - frame.Left) / Device.Current.DisplayScale, (frame.Bottom - frame.Top) / Device.Current.DisplayScale);
            if (currentSize != newSize)
            {
                SizeChanged(this, new WindowSizeChangedEventArgs(currentSize, newSize));
                currentSize = newSize;
            }
        }
    }

    /// <summary>
    /// Provides methods for intercepting events within the application's main window.
    /// </summary>
    public class MainWindowCallback : Java.Lang.Object, global::Android.Views.Window.ICallback
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MainWindowCallback"/> class.
        /// </summary>
        public MainWindowCallback()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MainWindowCallback"/> class.
        /// </summary>
        /// <param name="handle">An <see cref="IntPtr"/> containing a Java Native Interface (JNI) object reference.</param>
        /// <param name="transfer">A <see cref="JniHandleOwnership"/> indicating how to handle <paramref name="handle"/>.</param>
        public MainWindowCallback(IntPtr handle, JniHandleOwnership transfer)
            : base(handle, transfer)
        {
        }

        /// <summary></summary>
        /// <param name="e"></param>
        public virtual bool DispatchGenericMotionEvent(global::Android.Views.MotionEvent e)
        {
            return Application.MainActivity.Window.SuperDispatchGenericMotionEvent(e);
        }

        /// <summary></summary>
        /// <param name="e"></param>
        public virtual bool DispatchKeyEvent(global::Android.Views.KeyEvent e)
        {
            if ((e.KeyCode == Keycode.Back || (e.Flags & KeyEventFlags.VirtualHardKey) != 0) && e.Action == KeyEventActions.Up)
            {
                var appContent = ObjectRetriever.GetNativeObject(Prism.UI.Window.Current.Content);

                var stack = appContent as INativeViewStack;
                if (stack == null)
                {
                    var splitView = appContent as INativeSplitView;
                    if (splitView != null)
                    {
                        stack = splitView.DetailContent as INativeViewStack;
                        if (stack == null || !stack.IsBackButtonEnabled || stack.Views.Count() < 2)
                        {
                            stack = splitView.MasterContent as INativeViewStack;
                        }
                    }
                    else
                    {
                        var tabView = appContent as INativeTabView;
                        if (tabView != null)
                        {
                            stack = (tabView.TabItems[tabView.SelectedIndex] as INativeTabItem)?.Content as INativeViewStack;
                        }
                    }
                }
                
                if (stack != null && stack.IsBackButtonEnabled && stack.Views.Count() > 1)
                {
                    stack.PopView(Animate.Default);
                }
                else
                {
                    Prism.UI.Window.Current.Close();
                }
            }
        
            return Application.MainActivity.Window.SuperDispatchKeyEvent(e);
        }

        /// <summary></summary>
        /// <param name="e"></param>
        public virtual bool DispatchKeyShortcutEvent(global::Android.Views.KeyEvent e)
        {
            return Application.MainActivity.Window.SuperDispatchKeyShortcutEvent(e);
        }

        /// <summary></summary>
        /// <param name="e"></param>
        public virtual bool DispatchPopulateAccessibilityEvent(global::Android.Views.Accessibility.AccessibilityEvent e)
        {
            return true;
        }

        /// <summary></summary>
        /// <param name="e"></param>
        public virtual bool DispatchTouchEvent(global::Android.Views.MotionEvent e)
        {
            return Application.MainActivity.Window.SuperDispatchTouchEvent(e);
        }

        /// <summary></summary>
        /// <param name="e"></param>
        public virtual bool DispatchTrackballEvent(global::Android.Views.MotionEvent e)
        {
            return Application.MainActivity.Window.SuperDispatchTrackballEvent(e);
        }

        /// <summary></summary>
        /// <param name="mode"></param>
        public virtual void OnActionModeFinished(global::Android.Views.ActionMode mode)
        {
        }

        /// <summary></summary>
        /// <param name="mode"></param>
        public virtual void OnActionModeStarted(global::Android.Views.ActionMode mode)
        {
        }

        /// <summary></summary>
        public virtual void OnAttachedToWindow()
        {
        }

        /// <summary></summary>
        public virtual void OnContentChanged()
        {
        }

        /// <summary></summary>
        /// <param name="featureId"></param>
        /// <param name="menu"></param>
        public virtual bool OnCreatePanelMenu(int featureId, global::Android.Views.IMenu menu)
        {
            return true;
        }

        /// <summary></summary>
        /// <param name="featureId"></param>
        public virtual global::Android.Views.View OnCreatePanelView(int featureId)
        {
            return null;
        }

        /// <summary></summary>
        public virtual void OnDetachedFromWindow()
        {
        }

        /// <summary></summary>
        /// <param name="featureId"></param>
        /// <param name="item"></param>
        public virtual bool OnMenuItemSelected(int featureId, global::Android.Views.IMenuItem item)
        {
            return true;
        }

        /// <summary></summary>
        /// <param name="featureId"></param>
        /// <param name="menu"></param>
        public virtual bool OnMenuOpened(int featureId, global::Android.Views.IMenu menu)
        {
            return true;
        }

        /// <summary></summary>
        /// <param name="featureId"></param>
        /// <param name="menu"></param>
        public virtual void OnPanelClosed(int featureId, global::Android.Views.IMenu menu)
        {
        }

        /// <summary></summary>
        /// <param name="featureId"></param>
        /// <param name="view"></param>
        /// <param name="menu"></param>
        public virtual bool OnPreparePanel(int featureId, global::Android.Views.View view, global::Android.Views.IMenu menu)
        {
            return true;
        }

        /// <summary></summary>
        /// <param name="data"></param>
        /// <param name="menu"></param>
        /// <param name="deviceId"></param>
        public void OnProvideKeyboardShortcuts(IList<KeyboardShortcutGroup> data, IMenu menu, int deviceId)
        {
        }

        /// <summary></summary>
        public virtual bool OnSearchRequested()
        {
            return true;
        }

        /// <summary></summary>
        public virtual bool OnSearchRequested(global::Android.Views.SearchEvent searchEvent)
        {
            return true;
        }

        /// <summary></summary>
        /// <param name="attrs"></param>
        public virtual void OnWindowAttributesChanged(WindowManagerLayoutParams attrs)
        {
        }

        /// <summary></summary>
        /// <param name="hasFocus"></param>
        public virtual void OnWindowFocusChanged(bool hasFocus)
        {
            var window = ObjectRetriever.GetNativeObject(Prism.UI.Window.Current) as Window;
            if (window != null)
            {
                if (hasFocus)
                {
                    window.OnActivated();
                }
                else
                {
                    window.OnDeactivated();
                }
            }
        }

        /// <summary></summary>
        /// <param name="callback"></param>
        public virtual ActionMode OnWindowStartingActionMode(ActionMode.ICallback callback)
        {
            return null;
        }

        /// <summary></summary>
        /// <param name="callback"></param>
        /// <param name="type"></param>
        public virtual ActionMode OnWindowStartingActionMode(ActionMode.ICallback callback, ActionModeType type)
        {
            return null;
        }
    }
}