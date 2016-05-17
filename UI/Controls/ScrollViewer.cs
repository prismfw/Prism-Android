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
using Android.Content;
using Android.Graphics;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Prism.Input;
using Prism.Native;
using Prism.Systems;
using Prism.UI;
using Prism.UI.Controls;

namespace Prism.Android.UI.Controls
{
    /// <summary>
    /// Represents an Android implementation of an <see cref="INativeScrollViewer"/>.
    /// </summary>
    [Preserve(AllMembers = true)]
    [Register(typeof(INativeScrollViewer))]
    public class ScrollViewer : ScrollView, INativeScrollViewer
    {
        /// <summary>
        /// Occurs when this instance has been attached to the visual tree and is ready to be rendered.
        /// </summary>
        public event EventHandler Loaded;
        
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
        /// Occurs when the contents of the scroll viewer has been scrolled.
        /// </summary>
        public event EventHandler Scrolled;

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
        /// Gets or sets a value indicating whether the contents can be scrolled horizontally.
        /// </summary>
        public new bool CanScrollHorizontally
        {
            get { return horizontalScrollView.HorizontalScrollBarEnabled; }
            set
            {
                if (value != horizontalScrollView.HorizontalScrollBarEnabled)
                {
                    horizontalScrollView.HorizontalScrollBarEnabled = value;

                    var content = horizontalScrollView.GetChildAt(0);
                    if (content != null)
                    {
                        content.LayoutParameters.Width = value ? ViewGroup.LayoutParams.WrapContent : ViewGroup.LayoutParams.MatchParent;
                    }

                    OnPropertyChanged(Prism.UI.Controls.ScrollViewer.CanScrollHorizontallyProperty);
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the contents can be scrolled vertically.
        /// </summary>
        public new bool CanScrollVertically
        {
            get { return VerticalScrollBarEnabled; }
            set
            {
                if (value != VerticalScrollBarEnabled)
                {
                    VerticalScrollBarEnabled = value;

                    var content = horizontalScrollView.GetChildAt(0);
                    if (content != null)
                    {
                        content.LayoutParameters.Height = value ? ViewGroup.LayoutParams.WrapContent : ViewGroup.LayoutParams.MatchParent;
                    }

                    OnPropertyChanged(Prism.UI.Controls.ScrollViewer.CanScrollVerticallyProperty);
                }
            }
        }

        /// <summary>
        /// Gets or sets the content of the scroll viewer.
        /// </summary>
        public object Content
        {
            get { return horizontalScrollView.GetChildAt(0); }
            set
            {
                horizontalScrollView.RemoveAllViews();

                var view = value as global::Android.Views.View;
                if (view != null)
                {
                    horizontalScrollView.AddView(view, new FrameLayout.LayoutParams(
                        horizontalScrollView.HorizontalScrollBarEnabled ? ViewGroup.LayoutParams.WrapContent : ViewGroup.LayoutParams.MatchParent,
                        VerticalScrollBarEnabled ? ViewGroup.LayoutParams.WrapContent : ViewGroup.LayoutParams.MatchParent));
                }
            }
        }

        /// <summary>
        /// Gets the distance that the contents has been scrolled.
        /// </summary>
        public Point ContentOffset
        {
            get { return contentOffset; }
            private set
            {
                if (value != contentOffset)
                {
                    contentOffset = value;
                    OnPropertyChanged(Prism.UI.Controls.ScrollViewer.ContentOffsetProperty);
                }
            }
        }
        private Point contentOffset;

        /// <summary>
        /// Gets the size of the scrollable area.
        /// </summary>
        public Size ContentSize
        {
            get { return contentSize; }
            private set
            {
                if (value != contentSize)
                {
                    contentSize = value;
                    OnPropertyChanged(Prism.UI.Controls.ScrollViewer.ContentSizeProperty);
                }
            }
        }
        private Size contentSize;

        /// <summary>
        /// Gets or sets a <see cref="Rectangle"/> that represents the size and position of the element relative to its parent container.
        /// </summary>
        public Rectangle Frame
        {
            get
            {
                return new Rectangle(Left / Device.Current.DisplayScale, Top / Device.Current.DisplayScale,
                    Width / Device.Current.DisplayScale, Height / Device.Current.DisplayScale);
            }
            set
            {
                Left = (int)(value.Left * Device.Current.DisplayScale);
                Top = (int)(value.Top * Device.Current.DisplayScale);
                Right = (int)(value.Right * Device.Current.DisplayScale);
                Bottom = (int)(value.Bottom * Device.Current.DisplayScale);

                var widthSpec = MeasureSpec.MakeMeasureSpec(Right - Left, MeasureSpecMode.Unspecified);
                var heightSpec = MeasureSpec.MakeMeasureSpec(Bottom - Top, MeasureSpecMode.Unspecified);
                Measure(widthSpec, heightSpec);
                Layout(Left, Top, Right, Bottom);

                var content = horizontalScrollView.GetChildAt(0);
                if (content != null)
                {
                    content.Measure(CanScrollHorizontally ? widthSpec : MeasureSpec.MakeMeasureSpec(Right - Left, MeasureSpecMode.Exactly),
                        CanScrollVertically ? heightSpec : MeasureSpec.MakeMeasureSpec(Bottom - Top, MeasureSpecMode.Exactly));
                }
            }
        }

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

        private readonly HorizontalScrollViewer horizontalScrollView;

        /// <summary>
        /// Initializes a new instance of the <see cref="ScrollViewer"/> class.
        /// </summary>
        public ScrollViewer()
            : base(Application.MainActivity)
        {
            AddView(horizontalScrollView = new HorizontalScrollViewer(Context, this) { FillViewport = true },
                new FrameLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent));
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
        /// Measures the element and returns its desired size.
        /// </summary>
        /// <param name="constraints">The width and height that the element is not allowed to exceed.</param>
        public Size Measure(Size constraints)
        {
            return constraints;
        }

        /// <summary>
        /// Scrolls the contents to the specified offset.
        /// </summary>
        /// <param name="offset">The position to which to scroll the contents.</param>
        /// <param name="animate">Whether to animate the scrolling.</param>
        public void ScrollTo(Point offset, Animate animate)
        {
            if (animate == Prism.UI.Animate.Off || !areAnimationsEnabled)
            {
                ScrollTo(0, (int)(offset.Y * Device.Current.DisplayScale));
                horizontalScrollView.ScrollTo((int)(offset.X * Device.Current.DisplayScale), 0);
            }
            else
            {
                SmoothScrollTo(0, (int)(offset.Y * Device.Current.DisplayScale));
                horizontalScrollView.SmoothScrollTo((int)(offset.X * Device.Current.DisplayScale), 0);
            }
        }
        
        /// <summary></summary>
        /// <param name="e"></param>
        public override bool OnTouchEvent(MotionEvent e)
        {
            if (!isHitTestVisible)
            {
                return false;
            }
            
            if (e.Action == MotionEventActions.Cancel)
            {
                OnPointerCanceled(e);
                base.OnTouchEvent(e);
                return true;
            }
            if (e.Action == MotionEventActions.Down)
            {
                OnPointerPressed(e);
                base.OnTouchEvent(e);
                return true;
            }
            if (e.Action == MotionEventActions.Move)
            {
                OnPointerMoved(e);
                base.OnTouchEvent(e);
                return true;
            }
            if (e.Action == MotionEventActions.Up)
            {
                OnPointerReleased(e);
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
            MeasureRequest(false, null);
            ArrangeRequest(false, null);
            base.OnLayout(changed, Left, Top, Right, Bottom);

            var content = horizontalScrollView.GetChildAt(0);
            var element = ObjectRetriever.GetAgnosticObject(content) as Element;
            if (element != null)
            {
                right = (int)(element.DesiredSize.Width * Device.Current.DisplayScale);
                bottom = (int)(element.DesiredSize.Height * Device.Current.DisplayScale);
            }

            horizontalScrollView.Measure(MeasureSpec.MakeMeasureSpec(CanScrollHorizontally ? Width : Math.Max(right, Width), MeasureSpecMode.Exactly),
                MeasureSpec.MakeMeasureSpec(CanScrollVertically ? Math.Max(bottom, Height) : Height, MeasureSpecMode.Exactly));

            horizontalScrollView.Layout(0, 0, horizontalScrollView.MeasuredWidth, horizontalScrollView.MeasuredHeight);

            content.Measure(MeasureSpec.MakeMeasureSpec(right, MeasureSpecMode.Exactly),
                MeasureSpec.MakeMeasureSpec(bottom, MeasureSpecMode.Exactly));

            content.Layout(0, 0, right, bottom);

            ContentSize = new Size(horizontalScrollView.HorizontalScrollRange, ComputeVerticalScrollRange()) / Device.Current.DisplayScale;
        }

        /// <summary>
        /// Called when a property value is changed.
        /// </summary>
        /// <param name="pd">A property descriptor describing the property whose value has been changed.</param>
        protected virtual void OnPropertyChanged(PropertyDescriptor pd)
        {
            PropertyChanged(this, new FrameworkPropertyChangedEventArgs(pd));
        }

        /// <summary>
        /// This is called in response to an internal scroll in this view (i.e., the view scrolled its own contents).
        /// </summary>
        /// <param name="l">Current horizontal scroll origin.</param>
        /// <param name="t">Current vertical scroll origin.</param>
        /// <param name="oldl">Previous horizontal scroll origin.</param>
        /// <param name="oldt">Previous vertical scroll origin.</param>
        protected override void OnScrollChanged(int l, int t, int oldl, int oldt)
        {
            base.OnScrollChanged(l, t, oldl, oldt);
            ContentOffset = new Point(contentOffset.X, t / Device.Current.DisplayScale);
            OnScrolled();
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
        
        private void OnPointerCanceled(MotionEvent e)
        {
            PointerCanceled(this, e.GetPointerEventArgs(this));
        }
        
        private void OnPointerMoved(MotionEvent e)
        {
            PointerMoved(this, e.GetPointerEventArgs(this));
        }
        
        private void OnPointerPressed(MotionEvent e)
        {
            PointerPressed(this, e.GetPointerEventArgs(this));
        }
        
        private void OnPointerReleased(MotionEvent e)
        {
            PointerReleased(this, e.GetPointerEventArgs(this));
        }

        private void OnScrolled()
        {
            Scrolled(this, EventArgs.Empty);
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

        private class HorizontalScrollViewer : HorizontalScrollView
        {
            private readonly ScrollViewer parent;

            public int HorizontalScrollRange
            {
                get { return ComputeHorizontalScrollRange(); }
            }

            public int VerticalScrollRange
            {
                get { return ComputeVerticalScrollRange(); }
            }

            public HorizontalScrollViewer(Context context, ScrollViewer parent)
                : base(context)
            {
                this.parent = parent;
            }

            public override bool OnInterceptTouchEvent(MotionEvent ev)
            {
                return !parent.IsHitTestVisible;
            }
            
            public override bool OnTouchEvent(MotionEvent e)
            {
                if (!parent.isHitTestVisible)
                {
                    return false;
                }
                
                var child = GetChildAt(0);
                if (child != null && ((child as INativeElement)?.IsHitTestVisible ?? false))
                {
                    var rect = new Rect();
                    child.GetHitRect(rect);
                    if (rect.Contains((int)e.GetX(), (int)e.GetY()))
                    {
                        return base.OnTouchEvent(e);
                    }
                }
                
                if (e.Action == MotionEventActions.Cancel)
                {
                    parent.OnPointerCanceled(e);
                }
                if (e.Action == MotionEventActions.Down)
                {
                    parent.OnPointerPressed(e);
                }
                if (e.Action == MotionEventActions.Move)
                {
                    parent.OnPointerMoved(e);
                }
                if (e.Action == MotionEventActions.Up)
                {
                    parent.OnPointerReleased(e);
                }
                return base.OnTouchEvent(e);
            }

            protected override void OnScrollChanged(int l, int t, int oldl, int oldt)
            {
                base.OnScrollChanged(l, t, oldl, oldt);
                parent.ContentOffset = new Point(l / Device.Current.DisplayScale, parent.contentOffset.Y);
                parent.OnScrolled();
            }
        }
    }
}

