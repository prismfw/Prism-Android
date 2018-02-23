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
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Prism.Native;
using Prism.UI;

namespace Prism.Android.UI
{
    /// <summary>
    /// Represents an Android implementation of an <see cref="INativeSplitView"/>.
    /// </summary>
    [Preserve(AllMembers = true)]
    [Register(typeof(INativeSplitView))]
    public class SplitView : FrameLayout, INativeSplitView, ITouchDispatcher
    {
        /// <summary>
        /// Occurs when this instance has been attached to the visual tree and is ready to be rendered.
        /// </summary>
        public event EventHandler Loaded;

        /// <summary>
        /// Occurs when the value of a property is changed.
        /// </summary>
        public event EventHandler<FrameworkPropertyChangedEventArgs> PropertyChanged;

        /// <summary>
        /// Occurs when this instance has been detached from the visual tree.
        /// </summary>
        public event EventHandler Unloaded;

        /// <summary>
        /// Gets the actual width of the detail pane.
        /// </summary>
        public double ActualDetailWidth { get; private set; }

        /// <summary>
        /// Gets the actual width of the master pane.
        /// </summary>
        public double ActualMasterWidth { get; private set; }

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
        /// Gets or sets the object that acts as the content for the detail pane.
        /// </summary>
        public object DetailContent
        {
            get { return detailContent; }
            set
            {
                if (value != detailContent)
                {
                    detailContent = value;

                    if (DetailLayout.ChildCount > 0)
                    {
                        DetailLayout.RemoveAllViews();
                    }

                    var view = detailContent as global::Android.Views.View;
                    if (view != null)
                    {
                        DetailLayout.AddView(view);
                    }
                }
            }
        }
        private object detailContent;

        /// <summary>
        /// Gets or sets a <see cref="Rectangle"/> that represents
        /// the size and position of the element relative to its parent container.
        /// </summary>
        public Rectangle Frame { get; set; }

        /// <summary>
        /// Gets a value indicating whether this instance is currently dispatching touch events.
        /// </summary>
        public bool IsDispatching { get; private set; }

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
        /// Gets or sets the object that acts as the content for the master pane.
        /// </summary>
        public object MasterContent
        {
            get { return masterContent; }
            set
            {
                if (value != masterContent)
                {
                    masterContent = value;

                    if (MasterLayout.ChildCount > 0)
                    {
                        MasterLayout.RemoveAllViews();
                    }

                    var view = masterContent as global::Android.Views.View;
                    if (view != null)
                    {
                        MasterLayout.AddView(view);
                    }
                }
            }
        }
        private object masterContent;

        /// <summary>
        /// Gets or sets the maximum width of the master pane.
        /// </summary>
        public double MaxMasterWidth
        {
            get { return maxMasterWidth; }
            set
            {
                if (value != maxMasterWidth)
                {
                    maxMasterWidth = value;
                    OnPropertyChanged(Prism.UI.SplitView.MaxMasterWidthProperty);
                    SetMasterWidth();
                }
            }
        }
        private double maxMasterWidth;

        /// <summary>
        /// Gets or sets the method to invoke when this instance requests a measurement of itself and its children.
        /// </summary>
        public MeasureRequestHandler MeasureRequest { get; set; }

        /// <summary>
        /// Gets or sets the minimum width of the master pane.
        /// </summary>
        public double MinMasterWidth
        {
            get { return minMasterWidth; }
            set
            {
                if (value != minMasterWidth)
                {
                    minMasterWidth = value;
                    OnPropertyChanged(Prism.UI.SplitView.MinMasterWidthProperty);
                    SetMasterWidth();
                }
            }
        }
        private double minMasterWidth;

        /// <summary>
        /// Gets or sets the preferred width of the master pane as a percentage of the width of the split view.
        /// Valid values are between 0.0 and 1.0.
        /// </summary>
        public double PreferredMasterWidthRatio
        {
            get { return preferredMasterWidthRatio; }
            set
            {
                if (value != preferredMasterWidthRatio)
                {
                    preferredMasterWidthRatio = value;
                    OnPropertyChanged(Prism.UI.SplitView.PreferredMasterWidthRatioProperty);
                    SetMasterWidth();
                }
            }
        }
        private double preferredMasterWidthRatio;
        
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
        /// Gets the layout that contains the detail content.
        /// </summary>
        protected FrameLayout DetailLayout { get; }

        /// <summary>
        /// Gets the layout that contains the master content.
        /// </summary>
        protected FrameLayout MasterLayout { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SplitView"/> class.
        /// </summary>
        public SplitView()
            : base(Application.MainActivity)
        {
            minMasterWidth = 280;
            maxMasterWidth = 360;
            preferredMasterWidthRatio = 0.35;

            MasterLayout = new FrameLayout(Context) { Id = 1 };
            AddView(MasterLayout, new ViewGroup.LayoutParams(LayoutParams.MatchParent, LayoutParams.MatchParent));

            DetailLayout = new FrameLayout(Context) { Id = 2 };
            AddView(DetailLayout, new ViewGroup.LayoutParams(LayoutParams.MatchParent, LayoutParams.MatchParent));

            LayoutParameters = new ViewGroup.LayoutParams(LayoutParams.MatchParent, LayoutParams.MatchParent);
        }

        /// <summary></summary>
        /// <param name="e"></param>
        public override bool DispatchTouchEvent(MotionEvent e)
        {
            var parent = Parent as ITouchDispatcher;
            if (parent != null && !parent.IsDispatching)
            {
                return false;
            }

            if (OnInterceptTouchEvent(e))
            {
                return true;
            }

            IsDispatching = true;
            if (this.DispatchTouchEventToChildren(e))
            {
                IsDispatching = false;
                return true;
            }

            IsDispatching = false;
            return base.DispatchTouchEvent(e);
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
        /// Measures the object and returns its desired size.
        /// </summary>
        /// <param name="constraints">The width and height that the object is not allowed to exceed.</param>
        /// <returns>The desired size as a <see cref="Size"/> instance.</returns>
        public Size Measure(Size constraints)
        {
            return constraints;
        }

        /// <summary>
        /// Implement this method to intercept all touch screen motion events.
        /// </summary>
        /// <param name="ev">The motion event being dispatched down the hierarchy.</param>
        public override bool OnInterceptTouchEvent(MotionEvent ev)
        {
            return !isHitTestVisible;
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

            int width = ActualMasterWidth.GetScaledInt();
            MasterLayout.Layout(0, 0, width, Bottom);
            DetailLayout.Layout(width, 0, Width, Bottom);
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

        /// <summary>
        /// This is called during layout when the size of this view has changed.
        /// </summary>
        /// <param name="w">Current width of this view.</param>
        /// <param name="h">Current height of this view.</param>
        /// <param name="oldw">Old width of this view.</param>
        /// <param name="oldh">Old height of this view.</param>
        protected override void OnSizeChanged(int w, int h, int oldw, int oldh)
        {
            base.OnSizeChanged(w, h, oldw, oldh);
            SetMasterWidth();
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

        private void OnUnloaded()
        {
            if (IsLoaded)
            {
                IsLoaded = false;
                OnPropertyChanged(Visual.IsLoadedProperty);
                Unloaded(this, EventArgs.Empty);
            }
        }

        private void SetMasterWidth()
        {
            var width = (int)Math.Max(minMasterWidth, Math.Min(maxMasterWidth, Width.GetScaledDouble() * preferredMasterWidthRatio));
            if (width != ActualMasterWidth)
            {
                ActualMasterWidth = width;
                OnPropertyChanged(Prism.UI.SplitView.ActualMasterWidthProperty);
            }

            var detailWidth = Math.Max(0, Width.GetScaledDouble() - width);
            if (detailWidth != ActualDetailWidth)
            {
                ActualDetailWidth = detailWidth;
                OnPropertyChanged(Prism.UI.SplitView.ActualDetailWidthProperty);
                
                if (IsInLayout)
                {
                    ArrangeRequest(true, null);
                }
            }

            MasterLayout.Right = width.GetScaledInt();
        }
    }
}

