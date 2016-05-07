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
using System.Collections;
using Android.App;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Runtime;
using Android.Support.Design.Widget;
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
    /// Represents an Android implementation of an <see cref="INativeTabbedSplitView"/>.
    /// </summary>
    [Preserve(AllMembers = true)]
    [Register(typeof(INativeTabbedSplitView))]
    public class TabbedSplitView : Fragment, INativeTabbedSplitView
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
        public event EventHandler<NativeItemSelectedEventArgs> TabItemSelected;

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
        /// Gets or sets the background for the view.
        /// </summary>
        public Brush Background
        {
            get { return background; }
            set
            {
                if (value != background)
                {
                    background = value;
                    if (contentContainer != null)
                    {
                        contentContainer.Background = background;
                    }

                    OnPropertyChanged(Prism.UI.TabView.BackgroundProperty);
                }
            }
        }
        private Brush background;

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
                    contentContainer?.SetDetailContent(detailContent);
                }
            }
        }
        private object detailContent;
        
        /// <summary>
        /// Gets or sets the <see cref="Brush"/> to apply to the selected tab item.
        /// </summary>
        public Brush Foreground
        {
            get { return foreground; }
            set
            {
                if (value != foreground)
                {
                    foreground = value;
                    if (contentContainer != null)
                    {
                        contentContainer.Foreground = foreground;
                    }

                    OnPropertyChanged(Prism.UI.TabView.ForegroundProperty);
                }
            }
        }
        private Brush foreground;
        
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
                    OnPropertyChanged(Prism.UI.TabbedSplitView.MaxMasterWidthProperty);
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
                    OnPropertyChanged(Prism.UI.TabbedSplitView.MinMasterWidthProperty);
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
                    OnPropertyChanged(Prism.UI.TabbedSplitView.PreferredMasterWidthRatioProperty);
                }
            }
        }
        private double preferredMasterWidthRatio;
        
        /// <summary>
        /// Gets or sets the zero-based index of the selected tab item.
        /// </summary>
        public int SelectedIndex
        {
            get { return contentContainer?.TabLayout.SelectedTabPosition ?? selectedIndex; }
            set
            {
                if (value != selectedIndex)
                {
                    selectedIndex = value;
                    contentContainer?.TabLayout.GetTabAt(value).Select();
                }
            }
        }
        private int selectedIndex;

        /// <summary>
        /// Gets a list of the tab items that are a part of the view.
        /// </summary>
        public IList TabItems
        {
            get { return tabItems; }
        }
        private readonly TabItemCollection tabItems = new TabItemCollection();
        
        private ViewContentContainer contentContainer;
        private object currentTab;

        /// <summary>
        /// Initializes a new instance of the <see cref="TabbedSplitView"/> class.
        /// </summary>
        public TabbedSplitView()
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
        public override global::Android.Views.View OnCreateView (LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            if (contentContainer == null)
            {
                contentContainer = new ViewContentContainer(this);
            }

            (contentContainer.Parent as ViewGroup)?.RemoveView(contentContainer);
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

        /// <summary>
        /// Called when a tab is selected.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event arguments.</param>
        protected void OnTabSelected(object sender, TabLayout.TabSelectedEventArgs e)
        {
            var newTab = tabItems.GetItemForTab(e.Tab);
            TabItemSelected(this, new NativeItemSelectedEventArgs(currentTab, newTab));
            currentTab = newTab;

            contentContainer?.SetMasterContent((newTab as INativeTabItem)?.Content);
        }

        private void OnLoaded()
        {
            contentContainer?.SetMasterContent((tabItems[SelectedIndex] as INativeTabItem)?.Content);
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
        
        private class ViewContentContainer : FrameLayout, IFragmentView
        {
            public new Brush Background
            {
                get { return background; }
                set
                {
                    (background as ImageBrush).ClearImageHandler(OnBackgroundImageLoaded);

                    background = value;
                    TabLayout.Background = background.GetDrawable(OnBackgroundImageLoaded) ??
                        ResourceExtractor.GetDrawable(global::Android.Resource.Attribute.ColorAccent);
                }
            }
            private Brush background;

            public FrameLayout DetailLayout { get; }
            
            public new Brush Foreground
            {
                get { return foreground; }
                set
                {
                    foreground = value;

                    var scb = foreground as SolidColorBrush;
                    if (scb != null)
                    {
                        TabLayout.SetSelectedTabIndicatorColor(scb.Color.GetHashCode());
                    }
                    else if (foreground != null)
                    {
                        Prism.Utilities.Logger.Warn("TabbedSplitView.Foreground on Android only supports instances of SolidColorBrush.");
                    }
                }
            }
            private Brush foreground;

            public Fragment Fragment
            {
                get { return TabbedSplitView; }
            }

            public FrameLayout MasterLayout { get; }
            
            public TabLayout TabLayout { get; }
            
            public TabbedSplitView TabbedSplitView { get; }

            public ViewContentContainer(TabbedSplitView tabbedSplitView)
                : base(Application.MainActivity)
            {
                TabbedSplitView = tabbedSplitView;
                TabLayout = new TabLayout(Context);
                MasterLayout = new FrameLayout(Context) { Id = 1 };
                DetailLayout = new FrameLayout(Context) { Id = 2 };

                Background = TabbedSplitView.Background;
                Focusable = true;
                FocusableInTouchMode = true;
                Foreground = TabbedSplitView.Foreground;
                LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent);

                AddView(TabLayout, new FrameLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent));
                AddView(MasterLayout, new FrameLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent));
                AddView(DetailLayout, new FrameLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent));
                SetFrame();

                TabLayout.TabSelected += TabbedSplitView.OnTabSelected;
                TabbedSplitView.tabItems.TabLayout = TabLayout;
                TabLayout.TabMode = TabLayout.TabCount > 4 ? 0 : 1;
                TabLayout.GetTabAt(TabbedSplitView.SelectedIndex)?.Select();
                SetDetailContent(TabbedSplitView.DetailContent);
            }

            public override bool OnInterceptTouchEvent(MotionEvent ev)
            {
                return TabbedSplitView != null && !TabbedSplitView.IsHitTestVisible;
            }

            public void SetDetailContent(object content)
            {
                DetailLayout.RemoveAllViews();
                var view = content as global::Android.Views.View;
                if (view != null)
                {
                    DetailLayout.AddView(view);
                }
                else
                {
                    var fragment = content as Fragment;
                    if (fragment != null)
                    {
                        var transaction = TabbedSplitView.ChildFragmentManager.BeginTransaction();
                        transaction.Replace(2, fragment);
                        transaction.Commit();
                    }
                }
            }

            public void SetFrame()
            {
                Left = (int)(TabbedSplitView.frame.Left * Device.Current.DisplayScale);
                Top = (int)(TabbedSplitView.frame.Top * Device.Current.DisplayScale);
                Right = (int)(TabbedSplitView.frame.Right * Device.Current.DisplayScale);
                Bottom = (int)(TabbedSplitView.frame.Bottom * Device.Current.DisplayScale);

                Measure(MeasureSpec.MakeMeasureSpec(Right - Left, MeasureSpecMode.Exactly),
                    MeasureSpec.MakeMeasureSpec(Bottom - Top, MeasureSpecMode.Exactly));
                Layout(Left, Top, Right, Bottom);
            }

            public void SetMasterContent(object content)
            {
                MasterLayout.RemoveAllViews();
                var view = content as global::Android.Views.View;
                if (view != null)
                {
                    MasterLayout.AddView(view);
                }
                else
                {
                    var fragment = content as Fragment;
                    if (fragment != null)
                    {
                        var transaction = TabbedSplitView.ChildFragmentManager.BeginTransaction();
                        transaction.Replace(1, fragment);
                        transaction.Commit();
                    }
                }
            }

            protected override void OnAttachedToWindow()
            {
                base.OnAttachedToWindow();
                TabbedSplitView.OnLoaded();
            }

            protected override void OnDetachedFromWindow()
            {
                base.OnDetachedFromWindow();
                TabbedSplitView.OnUnloaded();
            }

            protected override void OnLayout(bool changed, int left, int top, int right, int bottom)
            {
                TabbedSplitView.MeasureRequest(false, null);
                TabbedSplitView.ArrangeRequest(false, null);

                int tabBottom = TabLayout.MeasuredHeight;
                int width = (int)(left + TabbedSplitView.ActualMasterWidth * Device.Current.DisplayScale);
                TabLayout.Layout(left, top, right, tabBottom);
                MasterLayout.Layout(left, tabBottom, width, bottom);
                DetailLayout.Layout(width, tabBottom, right, bottom);
            }

            protected override void OnSizeChanged(int w, int h, int oldw, int oldh)
            {
                base.OnSizeChanged(w, h, oldw, oldh);
                TabbedSplitView.SetMasterWidth();
            }

            private void OnBackgroundImageLoaded(object sender, EventArgs e)
            {
                TabLayout.Background = background.GetDrawable(null) ??
                    ResourceExtractor.GetDrawable(global::Android.Resource.Attribute.ColorAccent);
            }
        }
    }
}

