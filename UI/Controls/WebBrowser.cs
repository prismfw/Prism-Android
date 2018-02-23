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
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Webkit;
using Prism.Input;
using Prism.Native;
using Prism.UI;
using Prism.UI.Controls;
using Prism.Utilities;

namespace Prism.Android.UI.Controls
{
    /// <summary>
    /// Represents an Android implementation of an <see cref="INativeWebBrowser"/>.
    /// </summary>
    [Preserve(AllMembers = true)]
    [Register(typeof(INativeWebBrowser))]
    public class WebBrowser : WebView, INativeWebBrowser, IValueCallback
    {
        /// <summary>
        /// Occurs when this instance has been attached to the visual tree and is ready to be rendered.
        /// </summary>
        public event EventHandler Loaded;

        /// <summary>
        /// Occurs when the web browser has finished loading the document.
        /// </summary>
        public event EventHandler<WebNavigationCompletedEventArgs> NavigationCompleted;

        /// <summary>
        /// Occurs when the web browser has begun navigating to a document.
        /// </summary>
        public event EventHandler<WebNavigationStartingEventArgs> NavigationStarting;

        /// <summary>
        /// Occurs when the system loses track of the pointer for some reason.
        /// </summary>
        public event EventHandler<PointerEventArgs> PointerCanceled;

        /// <summary>
        /// Occurs when the pointer has moved while over the element.
        /// </summary>
        public event EventHandler<PointerEventArgs> PointerMoved;

        /// <summary>
        /// Occurs when the pointer has been pressed while over the element.
        /// </summary>
        public event EventHandler<PointerEventArgs> PointerPressed;

        /// <summary>
        /// Occurs when the pointer has been released while over the element.
        /// </summary>
        public event EventHandler<PointerEventArgs> PointerReleased;

        /// <summary>
        /// Occurs when the value of a property is changed.
        /// </summary>
        public event EventHandler<FrameworkPropertyChangedEventArgs> PropertyChanged;

        /// <summary>
        /// Occurs when a script invocation has completed.
        /// </summary>
        public event EventHandler<WebScriptCompletedEventArgs> ScriptCompleted;

        /// <summary>
        /// Occurs when this instance has been detached from the visual tree.
        /// </summary>
        public event EventHandler Unloaded;

        /// <summary>
        /// Gets or sets a value indicating whether animations are enabled for this instance.
        /// </summary>
        public bool AreAnimationsEnabled
        {
            get { return areAnimationsEnabled; }
            set
            {
                if (value != areAnimationsEnabled)
                {
                    areAnimationsEnabled = value;
                    OnPropertyChanged(Visual.AreAnimationsEnabledProperty);
                }
            }
        }
        private bool areAnimationsEnabled;

        /// <summary>
        /// Gets or sets the method to invoke when this instance requests an arrangement of its children.
        /// </summary>
        public ArrangeRequestHandler ArrangeRequest { get; set; }

        /// <summary>
        /// Gets a value indicating whether the web browser has at least one document in its back navigation history.
        /// </summary>
        public new bool CanGoBack { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the web browser has at least one document in its forward navigation history.
        /// </summary>
        public new bool CanGoForward { get; private set; }

        /// <summary>
        /// Gets or sets a <see cref="Rectangle"/> that represents the size and position of the element relative to its parent container.
        /// </summary>
        public Rectangle Frame { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance can be considered a valid result for hit testing.
        /// </summary>
        public bool IsHitTestVisible
        {
            get { return isHitTestVisible; }
            set
            {
                if (value != isHitTestVisible)
                {
                    isHitTestVisible = value;
                    OnPropertyChanged(Visual.IsHitTestVisibleProperty);
                }
            }
        }
        private bool isHitTestVisible = true;

        /// <summary>
        /// Gets a value indicating whether this instance has been loaded and is ready for rendering.
        /// </summary>
        public bool IsLoaded { get; private set; }

        /// <summary>
        /// Gets or sets the method to invoke when this instance requests a measurement of itself and its children.
        /// </summary>
        public MeasureRequestHandler MeasureRequest { get; set; }

        /// <summary>
        /// Gets or sets the level of opacity for the element.
        /// </summary>
        public double Opacity
        {
            get { return Alpha; }
            set
            {
                if (value != Alpha)
                {
                    Alpha = (float)value;
                    OnPropertyChanged(Element.OpacityProperty);
                }
            }
        }

        /// <summary>
        /// Gets or sets transformation information that affects the rendering position of this instance.
        /// </summary>
        public INativeTransform RenderTransform
        {
            get { return renderTransform; }
            set
            {
                if (value != renderTransform)
                {
                    (renderTransform as Media.Transform)?.RemoveView(this);
                    renderTransform = value;

                    var transform = renderTransform as Media.Transform;
                    if (transform == null)
                    {
                        Animation = renderTransform as global::Android.Views.Animations.Animation;
                    }
                    else
                    {
                        transform.AddView(this);
                    }

                    OnPropertyChanged(Visual.RenderTransformProperty);
                }
            }
        }
        private INativeTransform renderTransform;

        /// <summary>
        /// Gets or sets the visual theme that should be used by this instance.
        /// </summary>
        public Theme RequestedTheme { get; set; }

        /// <summary>
        /// Gets the title of the current document.
        /// </summary>
        public new string Title { get; private set; }

        /// <summary>
        /// Gets the URI of the current document.
        /// </summary>
        public Uri Uri { get; private set; }

        /// <summary>
        /// Gets or sets the display state of the element.
        /// </summary>
        public new Visibility Visibility
        {
            get { return base.Visibility.GetVisibility(); }
            set
            {
                var visibility = value.GetViewStates();
                if (visibility != base.Visibility)
                {
                    base.Visibility = visibility;
                    OnPropertyChanged(Element.VisibilityProperty);
                }
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WebBrowser"/> class.
        /// </summary>
        public WebBrowser()
            : base(Application.MainActivity)
        {
            SetWebViewClient(new WebBrowserClient());
        }

        /// <summary></summary>
        /// <param name="e"></param>
        public override bool DispatchTouchEvent(MotionEvent e)
        {
            var parent = Parent as ITouchDispatcher;
            if (parent == null || parent.IsDispatching)
            {
                return base.DispatchTouchEvent(e);
            }

            return false;
        }

        /// <summary>
        /// Invalidates the arrangement of this instance's children.
        /// </summary>
        public void InvalidateArrange()
        {
            RequestLayout();
        }

        /// <summary>
        /// Invalidates the measurement of this instance and its children.
        /// </summary>
        public void InvalidateMeasure()
        {
            RequestLayout();
        }

        /// <summary>
        /// Executes a script function that is implemented by the current document.
        /// </summary>
        /// <param name="scriptName">The name of the script function to execute.</param>
        public void InvokeScript(string scriptName)
        {
            if (Build.VERSION.SdkInt >= BuildVersionCodes.Kitkat)
            {
                EvaluateJavascript(scriptName, this);
            }
            else
            {
                Logger.Warn("InvokeScript requires Android 4.4 (KitKat) or later.");
            }
        }

        /// <summary>
        /// Measures the element and returns its desired size.
        /// </summary>
        /// <param name="constraints">The width and height that the element is not allowed to exceed.</param>
        public Size Measure(Size constraints)
        {
            return constraints;
        }

        /// <summary>
        /// Navigates to the specified <see cref="Uri"/>.
        /// </summary>
        /// <param name="uri">The URI to navigate to.</param>
        public void Navigate(Uri uri)
        {
            LoadUrl(uri.OriginalString);
        }

        /// <summary>
        /// Navigates to the specified <see cref="String"/> containing the content for a document.
        /// </summary>
        /// <param name="html">The string containing the content for a document.</param>
        public void NavigateToString(string html)
        {
            LoadData(html, "text/html", null);
        }

        /// <summary></summary>
        /// <param name="value"></param>
        public void OnReceiveValue(Java.Lang.Object value)
        {
            ScriptCompleted(this, new WebScriptCompletedEventArgs(value?.ToString()));
        }

        /// <summary>
        /// Reloads the current document.
        /// </summary>
        public void Refresh()
        {
            Reload();
        }

        /// <summary></summary>
        /// <param name="e"></param>
        public override bool OnTouchEvent(MotionEvent e)
        {
            if (!isHitTestVisible)
            {
                return false;
            }

            if (e.ActionMasked == MotionEventActions.Cancel)
            {
                PointerCanceled(this, e.GetPointerEventArgs(this));
                base.OnTouchEvent(e);
                return true;
            }
            if (e.ActionMasked == MotionEventActions.Down || e.ActionMasked == MotionEventActions.PointerDown)
            {
                PointerPressed(this, e.GetPointerEventArgs(this));
                base.OnTouchEvent(e);
                return true;
            }
            if (e.ActionMasked == MotionEventActions.Move)
            {
                PointerMoved(this, e.GetPointerEventArgs(this));
                base.OnTouchEvent(e);
                return true;
            }
            if (e.ActionMasked == MotionEventActions.Up || e.ActionMasked == MotionEventActions.PointerUp)
            {
                PointerReleased(this, e.GetPointerEventArgs(this));
                base.OnTouchEvent(e);
                return true;
            }
            return base.OnTouchEvent(e);
        }

        /// <summary>
        /// This is called when the view is attached to a window.
        /// </summary>
        protected override void OnAttachedToWindow()
        {
            base.OnAttachedToWindow();
            OnLoaded();
        }

        /// <summary>
        /// This is called when the view is detached from a window.
        /// </summary>
        protected override void OnDetachedFromWindow()
        {
            base.OnDetachedFromWindow();
            OnUnloaded();
        }

        /// <summary>
        /// Called from layout when this view should assign a size and position to each of its children.
        /// </summary>
        /// <param name="changed"></param>
        /// <param name="left">Left position, relative to parent.</param>
        /// <param name="top">Top position, relative to parent.</param>
        /// <param name="right">Right position, relative to parent.</param>
        /// <param name="bottom">Bottom position, relative to parent.</param>
        protected override void OnLayout(bool changed, int left, int top, int right, int bottom)
        {
            ArrangeRequest(false, null);

            Left = Frame.Left.GetScaledInt();
            Top = Frame.Top.GetScaledInt();
            Right = Frame.Right.GetScaledInt();
            Bottom = Frame.Bottom.GetScaledInt();

            base.OnLayout(changed, Left, Top, Right, Bottom);
        }

        /// <summary>
        /// Measure the view and its content to determine the measured width and the measured height.
        /// </summary>
        /// <param name="widthMeasureSpec">Horizontal space requirements as imposed by the parent.</param>
        /// <param name="heightMeasureSpec">Vertical space requirements as imposed by the parent.</param>
        protected override void OnMeasure(int widthMeasureSpec, int heightMeasureSpec)
        {
            MeasureRequest(false, null);
            base.OnMeasure(widthMeasureSpec, heightMeasureSpec);
        }

        /// <summary>
        /// Called when a property value is changed.
        /// </summary>
        /// <param name="pd">A property descriptor describing the property whose value has been changed.</param>
        protected virtual void OnPropertyChanged(PropertyDescriptor pd)
        {
            PropertyChanged(this, new FrameworkPropertyChangedEventArgs(pd));
        }

        private void OnLoaded()
        {
            if (!IsLoaded)
            {
                IsLoaded = true;
                OnPropertyChanged(Visual.IsLoadedProperty);
                Loaded(this, EventArgs.Empty);
            }
        }

        private void OnNavigationCompleted(string url)
        {
            if (CanGoBack != base.CanGoBack())
            {
                CanGoBack = base.CanGoBack();
                OnPropertyChanged(Prism.UI.Controls.WebBrowser.CanGoBackProperty);
            }

            if (CanGoForward != base.CanGoForward())
            {
                CanGoForward = base.CanGoForward();
                OnPropertyChanged(Prism.UI.Controls.WebBrowser.CanGoForwardProperty);
            }

            try
            {
                if (Uri == null || Uri.OriginalString != url)
                {
                    Uri = new Uri(url);
                    OnPropertyChanged(Prism.UI.Controls.WebBrowser.UriProperty);
                }
            }
            catch { }

            if (Title != base.Title)
            {
                Title = base.Title;
                OnPropertyChanged(Prism.UI.Controls.WebBrowser.TitleProperty);
            }

            NavigationCompleted(this, new WebNavigationCompletedEventArgs(Uri));
        }

        private bool OnNavigationStarted(string url)
        {
            var args = new WebNavigationStartingEventArgs(new Uri(url));
            NavigationStarting(this, args);
            if (args.Cancel)
            {
                StopLoading();
                return false;
            }

            return true;
        }

        private void OnUnloaded()
        {
            if (IsLoaded)
            {
                IsLoaded = false;
                OnPropertyChanged(Visual.IsLoadedProperty);
                Unloaded(this, EventArgs.Empty);
            }
        }

        private class WebBrowserClient : WebViewClient
        {
            public override void OnPageFinished(WebView view, string url)
            {
                base.OnPageFinished(view, url);
                (view as WebBrowser)?.OnNavigationCompleted(url);
            }

            public override void OnPageStarted(WebView view, string url, Bitmap favicon)
            {
                var webBrowser = view as WebBrowser;
                if (webBrowser == null || webBrowser.OnNavigationStarted(url))
                {
                    base.OnPageStarted(view, url, favicon);
                }
            }

            [Obsolete]
            public override void OnReceivedError(WebView view, ClientError errorCode, string description, string failingUrl)
            {
                Logger.Error("Error received from web client.  Code: {0} - Description: {1}", errorCode, description);
                base.OnReceivedError(view, errorCode, description, failingUrl);
            }
        }
    }
}

