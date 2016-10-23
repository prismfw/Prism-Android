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
    /// Represents an Android implementation of an <see cref="INativeTabView"/>.
    /// </summary>
    [Preserve(AllMembers = true)]
    [Register(typeof(INativeTabView))]
    public class TabView : Fragment, INativeTabView
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

        /// <summary>
        /// Gets or sets the visual theme that should be used by this instance.
        /// </summary>
        public Theme RequestedTheme { get; set; }

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
        /// Gets the size and location of the bar that contains the tab items.
        /// </summary>
        public Rectangle TabBarFrame
        {
            get
            {
                if (contentContainer == null)
                {
                    return new Rectangle();
                }

                return new Rectangle(contentContainer.TabLayout.Left / Device.Current.DisplayScale,
                    contentContainer.TabLayout.Top / Device.Current.DisplayScale,
                    contentContainer.TabLayout.Width / Device.Current.DisplayScale,
                    contentContainer.TabLayout.Height / Device.Current.DisplayScale);
            }
        }

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
        /// Initializes a new instance of the <see cref="TabView"/> class.
        /// </summary>
        public TabView()
        {
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
        /// Called when the fragment's activity has been created and this fragment's view hierarchy instantiated.
        /// </summary>
        /// <param name="savedInstanceState"></param>
        public override void OnActivityCreated(Bundle savedInstanceState)
        {
            try
            {
                // There appears to be a known bug in Android where fragments with a child fragment manager may throw
                // a 'No Activity' exception when the fragment's activity is created.
                // The suggested fix is to nullify the child fragment manager before activity creation.
                var field = Class.Superclass.GetDeclaredField("mChildFragmentManager");
                field.Accessible = true;
                field.Set(this, null);
            }
            catch
            {
                Prism.Utilities.Logger.Warn("Unable to perform fragment cleanup.  This may lead to unexpected runtime errors.");
            }

            base.OnActivityCreated(savedInstanceState);
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
                return (contentContainer = new ViewContentContainer(this));
            }

            (contentContainer.Parent as ViewGroup)?.RemoveView(contentContainer);
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

            contentContainer?.SetContent((newTab as INativeTabItem)?.Content);
        }

        private void OnLoaded()
        {
            contentContainer?.SetContent((tabItems[SelectedIndex] as INativeTabItem)?.Content);
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
        
        private class ViewContentContainer : LinearLayout, IFragmentView, ITouchDispatcher
        {
            public new Brush Background
            {
                get { return background; }
                set
                {
                    (background as ImageBrush).ClearImageHandler(OnBackgroundImageLoaded);

                    background = value;
                    TabLayout.Background = background.GetDrawable(OnBackgroundImageLoaded) ??
                        ResourceExtractor.GetDrawable(global::Android.Resource.Attribute.Background);
                }
            }
            private Brush background;
            
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
                    else
                    {
                        TabLayout.SetSelectedTabIndicatorColor(ResourceExtractor.GetColor(global::Android.Resource.Attribute.ColorAccent));
                        if (foreground != null)
                        {
                            Prism.Utilities.Logger.Warn("TabView.Foreground on Android only supports instances of SolidColorBrush.");
                        }
                    }
                }
            }
            private Brush foreground;

            public Fragment Fragment
            {
                get { return TabView; }
            }
            
            public FrameLayout FrameLayout { get; }
            
            public bool IsDispatching { get; private set; }
            
            public TabLayout TabLayout { get; }
            
            public TabView TabView { get; }

            public ViewContentContainer(TabView tabView)
                : base(Application.MainActivity)
            {
                TabView = tabView;
                TabLayout = new TabLayout(Context);
                FrameLayout = new FrameLayout(Context) { Id = 1 };

                Background = tabView.Background;
                Focusable = true;
                FocusableInTouchMode = true;
                Foreground = tabView.Foreground;
                LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent);
                Orientation = global::Android.Widget.Orientation.Vertical;

                AddView(TabLayout, new LinearLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent));
                AddView(FrameLayout, new LinearLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent));
                SetFrame();
                SetTransform();

                TabLayout.TabSelected += TabView.OnTabSelected;
                TabView.tabItems.TabLayout = TabLayout;
                TabLayout.TabMode = TabLayout.TabCount > 4 ? 0 : 1;
                TabLayout.GetTabAt(TabView.SelectedIndex)?.Select();
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
                return TabView != null && !TabView.IsHitTestVisible;
            }

            public void SetContent(object content)
            {
                FrameLayout.RemoveAllViews();
                var view = content as global::Android.Views.View;
                if (view != null)
                {
                    FrameLayout.AddView(view);
                }
                else
                {
                    var fragment = content as Fragment;
                    if (fragment != null)
                    {
                        var transaction = TabView.ChildFragmentManager.BeginTransaction();
                        transaction.Replace(1, fragment);
                        transaction.Commit();
                    }
                }
            }

            public void SetFrame()
            {
                Left = (int)(TabView.frame.Left * Device.Current.DisplayScale);
                Top = (int)(TabView.frame.Top * Device.Current.DisplayScale);
                Right = (int)(TabView.frame.Right * Device.Current.DisplayScale);
                Bottom = (int)(TabView.frame.Bottom * Device.Current.DisplayScale);

                Measure(MeasureSpec.MakeMeasureSpec(Right - Left, MeasureSpecMode.Exactly),
                    MeasureSpec.MakeMeasureSpec(Bottom - Top, MeasureSpecMode.Exactly));
                Layout(Left, Top, Right, Bottom);
            }
            
            public void SetTransform()
            {
                var transform = TabView.renderTransform as Media.Transform;
                if (transform == null)
                {
                    Animation = TabView.renderTransform as global::Android.Views.Animations.Animation;
                }
                else
                {
                    transform.AddView(this);
                }
            }

            protected override void OnAttachedToWindow()
            {
                base.OnAttachedToWindow();
                TabView.OnLoaded();
            }

            protected override void OnDetachedFromWindow()
            {
                base.OnDetachedFromWindow();
                TabView.OnUnloaded();
            }

            protected override void OnLayout(bool changed, int left, int top, int right, int bottom)
            {
                TabView.MeasureRequest(false, null);
                TabView.ArrangeRequest(false, null);

                base.OnLayout(changed, left, top, right, bottom);
            }

            private void OnBackgroundImageLoaded(object sender, EventArgs e)
            {
                TabLayout.Background = background.GetDrawable(null) ??
                    ResourceExtractor.GetDrawable(global::Android.Resource.Attribute.Background);
            }
        }
    }
}

