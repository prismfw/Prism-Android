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
using Prism.UI.Media;

namespace Prism.Android.UI
{
    /// <summary>
    /// Represents an Android implementation of an <see cref="INativeContentView"/>.
    /// </summary>
    [Preserve(AllMembers = true)]
    [Register(typeof(INativeContentView))]
    public class ContentView : Fragment, INativeContentView
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

                    OnPropertyChanged(Prism.UI.ContentView.BackgroundProperty);
                }
            }
        }
        private Brush background;

        /// <summary>
        /// Gets or sets the content to be displayed by the view.
        /// </summary>
        public object Content
        {
            get { return content; }
            set
            {
                content = value;
                if (contentContainer != null)
                {
                    contentContainer.RemoveAllViews();

                    var view = content as global::Android.Views.View;
                    if (view != null)
                    {
                        contentContainer.AddView(view, new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent));
                    }
                }
            }
        }
        private object content;

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
        /// Gets or sets a value indicating whether the back button of an <see cref="INativeViewStack"/>
        /// is enabled when this view is the visible view of the stack.
        /// </summary>
        public bool IsBackButtonEnabled
        {
            get { return isBackButtonEnabled; }
            set
            {
                if (value != isBackButtonEnabled)
                {
                    isBackButtonEnabled = value;
                    OnPropertyChanged(Prism.UI.ContentView.IsBackButtonEnabledProperty);
                    
                    var stack = contentContainer?.GetParent<INativeViewStack>();
                    if (stack != null && stack.CurrentView == this && contentContainer?.GetParent<INativeSplitView>()?.DetailContent != stack)
                    {
                        (stack.Header as Controls.ViewStackHeader)?.SetBackButtonVisibility(isBackButtonEnabled ? ViewStates.Visible : ViewStates.Gone);
                    }
                }
            }
        }
        private bool isBackButtonEnabled;

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
        /// Gets or sets the action menu for the view.
        /// </summary>
        public INativeActionMenu Menu
        {
            get { return menu; }
            set
            {
                if (value != menu)
                {
                    (menu as Controls.ActionMenu)?.Detach();
                    
                    menu = value;
                    
                    var header = (ParentFragment as INativeViewStack)?.Header as Controls.ViewStackHeader;
                    if (header != null)
                    {
                        (menu as Controls.ActionMenu).Attach(header);
                    }
                    OnPropertyChanged(Prism.UI.ContentView.MenuProperty);
                }
            }
        }
        private INativeActionMenu menu;

        /// <summary>
        /// Gets or sets the title of the view.
        /// </summary>
        public string Title
        {
            get { return title; }
            set
            {
                if (value != title)
                {
                    title = value;

                    var stack = ParentFragment as INativeViewStack;
                    if (stack != null)
                    {
                        stack.Header.Title = title;
                    }

                    OnPropertyChanged(Prism.UI.ContentView.TitleProperty);
                }
            }
        }
        private string title;

        private ViewContentContainer contentContainer;

        /// <summary>
        /// Initializes a new instance of the <see cref="ContentView"/> class.
        /// </summary>
        public ContentView()
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
        public override global::Android.Views.View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            if (contentContainer?.Parent != null)
            {
                (contentContainer.Parent as ViewGroup)?.RemoveView(contentContainer);
                return contentContainer;
            }

            return (contentContainer = new ViewContentContainer(this));
        }

        /// <summary>
        /// Called when a property value is changed.
        /// </summary>
        /// <param name="pd">A property descriptor describing the property whose value has been changed.</param>
        protected virtual void OnPropertyChanged(PropertyDescriptor pd)
        {
            PropertyChanged.Invoke(this, new FrameworkPropertyChangedEventArgs(pd));
        }

        private void OnLoaded()
        {
            var header = (ParentFragment as INativeViewStack)?.Header as Controls.ViewStackHeader;
            if (header != null)
            {
                (menu as Controls.ActionMenu).Attach(header);
            }
        
            if (!IsLoaded)
            {
                IsLoaded = true;
                OnPropertyChanged(Visual.IsLoadedProperty);
                Loaded.Invoke(this, EventArgs.Empty);
            }
        }

        private void OnUnloaded()
        {
            if (IsLoaded)
            {
                IsLoaded = false;
                OnPropertyChanged(Visual.IsLoadedProperty);
                Unloaded.Invoke(this, EventArgs.Empty);
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
                    base.Background = background.GetDrawable(OnBackgroundImageLoaded) ??
                        ResourceExtractor.GetDrawable(global::Android.Resource.Attribute.WindowBackground);
                }
            }
            private Brush background;
            
            public ContentView ContentView { get; }

            public Fragment Fragment
            {
                get { return ContentView; }
            }

            public ViewContentContainer(ContentView contentView)
                : base(Application.MainActivity)
            {
                ContentView = contentView;

                Background = contentView.Background;
                Focusable = true;
                FocusableInTouchMode = true;
                LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent);

                var view = contentView.Content as global::Android.Views.View;
                if (view != null)
                {
                    (view.Parent as ViewGroup)?.RemoveView(view);
                    AddView(view);
                }

                SetFrame();
            }

            public override bool OnInterceptTouchEvent(MotionEvent ev)
            {
                return ContentView != null && !ContentView.IsHitTestVisible;
            }

            public void SetFrame()
            {
                Left = (int)(ContentView.frame.Left * Device.Current.DisplayScale);
                Top = (int)(ContentView.frame.Top * Device.Current.DisplayScale);
                Right = (int)(ContentView.frame.Right * Device.Current.DisplayScale);
                Bottom = (int)(ContentView.frame.Bottom * Device.Current.DisplayScale);

                Measure(MeasureSpec.MakeMeasureSpec(Right - Left, MeasureSpecMode.Exactly),
                    MeasureSpec.MakeMeasureSpec(Bottom - Top, MeasureSpecMode.Exactly));
                Layout(Left, Top, Right, Bottom);
            }

            protected override void OnAttachedToWindow()
            {
                base.OnAttachedToWindow();
                ContentView.OnLoaded();
            }

            protected override void OnDetachedFromWindow()
            {
                base.OnDetachedFromWindow();
                ContentView.OnUnloaded();
            }

            protected override void OnLayout(bool changed, int left, int top, int right, int bottom)
            {
                ContentView.MeasureRequest(false, null);
                ContentView.ArrangeRequest(false, null);

                for (int i = 0; i < ChildCount; i++)
                {
                    var child = GetChildAt(i);
                    child.Layout(child.Left, child.Top, child.Right, child.Bottom);
                }
            }

            private void OnBackgroundImageLoaded(object sender, EventArgs e)
            {
                base.Background = background.GetDrawable(null) ??
                    ResourceExtractor.GetDrawable(global::Android.Resource.Attribute.Background);
            }
        }
    }
}

