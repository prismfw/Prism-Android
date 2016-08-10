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
using Android.App;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Prism.Native;
using Prism.Systems;
using Prism.UI;

namespace Prism.Android.UI
{
    /// <summary>
    /// Represents an Android implementation of an <see cref="INativeSplitView"/>.
    /// </summary>
    [Preserve(AllMembers = true)]
    [Register(typeof(INativeSplitView))]
    public class SplitView : Fragment, INativeSplitView
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
                    contentContainer?.SetDetailView();
                }
            }
        }
        private object detailContent;

        /// <summary>
        /// Gets or sets a <see cref="Rectangle"/> that represents
        /// the size and position of the element relative to its parent container.
        /// </summary>
        public Rectangle Frame
        {
            get { return frame; }
            set
            {
                frame = value;
                contentContainer?.SetFrame();
            }
        }
        private Rectangle frame;

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
                    contentContainer?.SetMasterView();
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
                    SetMasterWidth();
                    OnPropertyChanged(Prism.UI.SplitView.MaxMasterWidthProperty);
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
                    SetMasterWidth();
                    OnPropertyChanged(Prism.UI.SplitView.MinMasterWidthProperty);
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
                    SetMasterWidth();
                    OnPropertyChanged(Prism.UI.SplitView.PreferredMasterWidthRatioProperty);
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
                    if (contentContainer != null)
                    {
                        (renderTransform as Media.Transform)?.RemoveView(contentContainer);
                    }
                    
                    renderTransform = value;
                    contentContainer?.SetTransform();
                    OnPropertyChanged(Visual.RenderTransformProperty);
                }
            }
        }
        private INativeTransform renderTransform;

        private ViewContentContainer contentContainer;

        /// <summary>
        /// Initializes a new instance of the <see cref="SplitView"/> class.
        /// </summary>
        public SplitView()
        {
            minMasterWidth = 360;
            maxMasterWidth = 360;
            preferredMasterWidthRatio = 0.3;
        }

        /// <summary>
        /// Invalidates the arrangement of this instance's children.
        /// </summary>
        public void InvalidateArrange()
        {
            contentContainer?.RequestLayout();
        }

        /// <summary>
        /// Invalidates the measurement of this instance and its children.
        /// </summary>
        public void InvalidateMeasure()
        {
            contentContainer?.RequestLayout();
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
        /// Called to have the fragment instantiate its user interface view.
        /// </summary>
        /// <param name="inflater"></param>
        /// <param name="container"></param>
        /// <param name="savedInstanceState"></param>
        public override global::Android.Views.View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            if (contentContainer?.Parent != null)
            {
                (contentContainer.Parent as ViewGroup)?.RemoveView(contentContainer);
                return contentContainer;
            }

            contentContainer = new ViewContentContainer(this);
            SetMasterWidth();
            return contentContainer;
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
            if (contentContainer != null)
            {
                var width = (int)Math.Max(minMasterWidth, Math.Min(maxMasterWidth, (contentContainer.Width / Device.Current.DisplayScale) * preferredMasterWidthRatio));
                if (width != ActualMasterWidth)
                {
                    ActualMasterWidth = width;
                    OnPropertyChanged(Prism.UI.SplitView.ActualMasterWidthProperty);
                }

                var detailWidth = Math.Max(0, (contentContainer.Width / Device.Current.DisplayScale) - width);
                if (detailWidth != ActualDetailWidth)
                {
                    ActualDetailWidth = detailWidth;
                    OnPropertyChanged(Prism.UI.SplitView.ActualDetailWidthProperty);
                }

                contentContainer.MasterLayout.Right = (int)(width * Device.Current.DisplayScale);
            }
        }

        private class ViewContentContainer : FrameLayout, IFragmentView, ITouchDispatcher
        {
            public FrameLayout DetailLayout { get; }

            public Fragment Fragment
            {
                get { return SplitView; }
            }
            
            public bool IsDispatching { get; private set; }

            public FrameLayout MasterLayout { get; }

            public SplitView SplitView { get; }

            public ViewContentContainer(SplitView splitView)
                : base(Application.MainActivity)
            {
                SplitView = splitView;

                MasterLayout = new FrameLayout(Context) { Id = 1 };
                AddView(MasterLayout, new ViewGroup.LayoutParams(LayoutParams.MatchParent, LayoutParams.MatchParent));

                DetailLayout = new FrameLayout(Context) { Id = 2 };
                AddView(DetailLayout, new ViewGroup.LayoutParams(LayoutParams.MatchParent, LayoutParams.MatchParent));

                LayoutParameters = new ViewGroup.LayoutParams(LayoutParams.MatchParent, LayoutParams.MatchParent);

                SetFrame();
                SetTransform();
                SetMasterView();
                SetDetailView();
            }
            
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

            public override bool OnInterceptTouchEvent(MotionEvent ev)
            {
                return SplitView != null && !SplitView.IsHitTestVisible;
            }

            public void SetDetailView()
            {
                var detailFrag = SplitView.DetailContent as Fragment;
                if (detailFrag != null)
                {
                    var transaction = SplitView.ChildFragmentManager.BeginTransaction();
                    transaction.Replace(2, detailFrag);
                    transaction.Commit();
                }
                else
                {
                    if (DetailLayout.ChildCount > 0)
                    {
                        DetailLayout.RemoveAllViews();
                    }

                    var view = SplitView.DetailContent as global::Android.Views.View;
                    if (view != null)
                    {
                        DetailLayout.AddView(view);
                    }
                }
            }

            public void SetFrame()
            {
                Left = (int)(SplitView.frame.Left * Device.Current.DisplayScale);
                Top = (int)(SplitView.frame.Top * Device.Current.DisplayScale);
                Right = (int)(SplitView.frame.Right * Device.Current.DisplayScale);
                Bottom = (int)(SplitView.frame.Bottom * Device.Current.DisplayScale);
            }

            public void SetMasterView()
            {
                var masterFrag = SplitView.MasterContent as Fragment;
                if (masterFrag != null)
                {
                    var transaction = SplitView.ChildFragmentManager.BeginTransaction();
                    transaction.Replace(1, masterFrag);
                    transaction.Commit();
                }
                else
                {
                    if (MasterLayout.ChildCount > 0)
                    {
                        MasterLayout.RemoveAllViews();
                    }

                    var view = SplitView.MasterContent as global::Android.Views.View;
                    if (view != null)
                    {
                        MasterLayout.AddView(view);
                    }
                }
            }
            
            public void SetTransform()
            {
                var transform = SplitView.renderTransform as Media.Transform;
                if (transform == null)
                {
                    Animation = SplitView.renderTransform as global::Android.Views.Animations.Animation;
                }
                else
                {
                    transform.AddView(this);
                }
            }

            protected override void OnAttachedToWindow()
            {
                base.OnAttachedToWindow();
                SplitView.OnLoaded();
            }

            protected override void OnDetachedFromWindow()
            {
                base.OnDetachedFromWindow();
                SplitView.OnUnloaded();
            }

            protected override void OnLayout(bool changed, int left, int top, int right, int bottom)
            {
                SplitView.MeasureRequest(false, null);
                SplitView.ArrangeRequest(false, null);

                int width = (int)(left + SplitView.ActualMasterWidth * Device.Current.DisplayScale);
                MasterLayout.Layout(left, top, width, bottom);
                DetailLayout.Layout(width, top, right, bottom);
            }

            protected override void OnSizeChanged(int w, int h, int oldw, int oldh)
            {
                base.OnSizeChanged(w, h, oldw, oldh);
                SplitView.SetMasterWidth();
            }
        }
    }
}

