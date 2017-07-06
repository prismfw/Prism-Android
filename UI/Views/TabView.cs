/*
Copyright (C) 2017  Prism Framework Team

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
using System.Collections;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Prism.Android.UI.Controls;
using Prism.Native;
using Prism.Systems;
using Prism.UI;
using Prism.UI.Media;

namespace Prism.Android.UI
{
    /// <summary>
    /// Represents an Android implementation of an <see cref="INativeTabView"/>.
    /// </summary>
    [Preserve(AllMembers = true)]
    [Register(typeof(INativeTabView))]
    public class TabView : FrameLayout, INativeTabView, ITouchDispatcher
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
        /// Occurs when a tab item is selected.
        /// </summary>
        public event EventHandler<NativeItemChangedEventArgs> TabItemSelected;

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
        /// Gets or sets the background for the view.
        /// </summary>
        public new Brush Background
        {
            get { return background; }
            set
            {
                if (value != background)
                {
                    (background as ImageBrush).ClearImageHandler(OnBackgroundImageLoaded);

                    background = value;
                    TabLayout.Background = background.GetDrawable(OnBackgroundImageLoaded) ??
                        Android.Resources.GetDrawable(this, global::Android.Resource.Attribute.Background);

                    OnPropertyChanged(Prism.UI.TabView.BackgroundProperty);
                }
            }
        }
        private Brush background;

        /// <summary>
        /// Gets or sets the <see cref="Brush"/> to apply to the selected tab item.
        /// </summary>
        public new Brush Foreground
        {
            get { return TabLayout.SelectionBrush; }
            set
            {
                if (value != TabLayout.SelectionBrush)
                {
                    TabLayout.SelectionBrush = value;
                    OnPropertyChanged(Prism.UI.TabView.ForegroundProperty);
                }
            }
        }

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
        /// Gets or sets the method to invoke when this instance requests a measurement of itself and its children.
        /// </summary>
        public MeasureRequestHandler MeasureRequest { get; set; }

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
        /// Gets or sets the zero-based index of the selected tab item.
        /// </summary>
        public int SelectedIndex
        {
            get { return TabLayout.SelectedTabIndex; }
            set
            {
                if (value != selectedIndex)
                {
                    selectedIndex = value;
                    TabLayout.SelectTabAt(selectedIndex);
                }
            }
        }
        private int selectedIndex;

        /// <summary>
        /// Gets the size and location of the bar that contains the tab items.
        /// </summary>
        public Rectangle TabBarFrame
        {
            get
            {
                return new Rectangle(TabLayout.Left / Device.Current.DisplayScale,
                    TabLayout.Top / Device.Current.DisplayScale,
                    TabLayout.Width / Device.Current.DisplayScale,
                    TabLayout.Height / Device.Current.DisplayScale);
            }
        }

        /// <summary>
        /// Gets a list of the tab items that are a part of the view.
        /// </summary>
        public IList TabItems
        {
            get { return tabItems; }
        }
        private readonly TabItemCollection tabItems;

        private FrameLayout FrameLayout { get; }

        private TabLayout TabLayout { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="TabView"/> class.
        /// </summary>
        public TabView()
            : base(Application.MainActivity)
        {
            TabLayout = new TabLayout(Context);
            FrameLayout = new FrameLayout(Context) { Id = 1 };

            tabItems = new TabItemCollection(TabLayout);

            Focusable = true;
            FocusableInTouchMode = true;
            LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent);

            AddView(TabLayout, new LinearLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent));
            AddView(FrameLayout, new LinearLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent));

            TabLayout.TabSelected += OnTabSelected;
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
            TabLayout.Layout(0, 0, right - left, TabLayout.MeasuredHeight);

            ArrangeRequest(false, null);

            Left = (int)Math.Ceiling(Frame.Left * Device.Current.DisplayScale);
            Top = (int)Math.Ceiling(Frame.Top * Device.Current.DisplayScale);
            Right = (int)Math.Ceiling(Frame.Right * Device.Current.DisplayScale);
            Bottom = (int)Math.Ceiling(Frame.Bottom * Device.Current.DisplayScale);

            FrameLayout.Layout(0, TabLayout.Bottom, Width, Height);
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

        private void OnBackgroundImageLoaded(object sender, EventArgs e)
        {
            TabLayout.Background = background.GetDrawable(null) ??
                Android.Resources.GetDrawable(this, global::Android.Resource.Attribute.Background);
        }

        private void OnLoaded()
        {
            SetContent((tabItems[SelectedIndex] as INativeTabItem)?.Content);
            if (!IsLoaded)
            {
                IsLoaded = true;
                OnPropertyChanged(Visual.IsLoadedProperty);
                Loaded(this, EventArgs.Empty);
            }
        }

        private void OnTabSelected(object sender, TabLayout.TabSelectedEventArgs e)
        {
            bool changed = tabItems.IndexOf(e.OldTab) != tabItems.IndexOf(e.NewTab);
            if (changed)
            {
                OnPropertyChanged(Prism.UI.TabView.SelectedIndexProperty);
            }

            TabItemSelected(this, new NativeItemChangedEventArgs(e.OldTab, e.NewTab));

            if (changed || FrameLayout.ChildCount == 0)
            {
                SetContent((e.NewTab as INativeTabItem)?.Content);
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

        private void SetContent(object content)
        {
            FrameLayout.RemoveAllViews();

            var view = content as global::Android.Views.View;
            if (view != null)
            {
                FrameLayout.AddView(view);
            }
        }
    }
}

