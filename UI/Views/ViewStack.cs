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
using System.Collections.Generic;
using System.Linq;
using Android.App;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Prism.Android.UI.Controls;
using Prism.Native;
using Prism.Systems;
using Prism.UI;

namespace Prism.Android.UI
{
    /// <summary>
    /// Represents an Android implementation of an <see cref="INativeViewStack"/>.
    /// </summary>
    [Preserve(AllMembers = true)]
    [Register(typeof(INativeViewStack))]
    public class ViewStack : Fragment, INativeViewStack
    {
        /// <summary>
        /// Occurs when this instance has been attached to the visual tree and is ready to be rendered.
        /// </summary>
        public event EventHandler Loaded;

        /// <summary>
        /// Occurs when a view is being popped off of the view stack.
        /// </summary>
        public event EventHandler<NativeViewStackPoppingEventArgs> Popping;

        /// <summary>
        /// Occurs when the value of a property is changed.
        /// </summary>
        public event EventHandler<FrameworkPropertyChangedEventArgs> PropertyChanged;

        /// <summary>
        /// Occurs when this instance has been detached from the visual tree.
        /// </summary>
        public event EventHandler Unloaded;

        /// <summary>
        /// Occurs when the current view of the view stack has changed.
        /// </summary>
        public event EventHandler ViewChanged;

        /// <summary>
        /// Occurs when the current view of the view stack is being replaced by a different view.
        /// </summary>
        public event EventHandler<NativeViewStackViewChangingEventArgs> ViewChanging;

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
        /// Gets the view that is currently on top of the stack.
        /// </summary>
        public object CurrentView
        {
            get { return views.LastOrDefault(); }
        }

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
        /// Gets the header for the view stack.
        /// </summary>
        public INativeViewStackHeader Header
        {
            get { return header; }
        }
        private readonly ViewStackHeader header;

        /// <summary>
        /// Gets or sets a value indicating whether the back button is enabled.
        /// </summary>
        public bool IsBackButtonEnabled
        {
            get { return isBackButtonEnabled; }
            set
            {
                isBackButtonEnabled = value;
                header.SetBackButtonVisibility(contentContainer?.GetParent<INativeSplitView>()?.DetailContent == this || !value ? ViewStates.Gone : ViewStates.Visible);
            }
        }
        private bool isBackButtonEnabled;

        /// <summary>
        /// Gets or sets a value indicating whether the header is hidden.
        /// </summary>
        public bool IsHeaderHidden
        {
            get { return (Header as global::Android.Views.View).Visibility == ViewStates.Gone; }
            set
            {
                var headerView = Header as global::Android.Views.View;
                if (value != (headerView.Visibility == ViewStates.Gone))
                {
                    headerView.Visibility = value ? ViewStates.Gone : ViewStates.Visible;
                    OnPropertyChanged(Prism.UI.ViewStack.IsHeaderHiddenProperty);
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
        /// Gets a collection of the views that are currently a part of the stack.
        /// </summary>
        public IEnumerable<object> Views
        {
            get { return views.AsReadOnly(); }
        }

        private ViewContentContainer contentContainer;
        private bool isPopping;
        private readonly List<object> views;

        /// <summary>
        /// Initializes a new instance of the <see cref="ViewStack"/> class.
        /// </summary>
        public ViewStack()
        {
            views = new List<object>();
            header = new ViewStackHeader(Application.MainActivity);
        }

        /// <summary>
        /// Inserts the specified view into the stack at the specified index.
        /// </summary>
        /// <param name="view">The view to be inserted.</param>
        /// <param name="index">The zero-based index of the location in the stack in which to insert the view.</param>
        /// <param name="animate">Whether to use any system-defined transition animation.</param>
        public void InsertView(object view, int index, Animate animate)
        {
            views.Insert(index, view);
            if (views.Last() == view)
            {
                ViewChanging(this, new NativeViewStackViewChangingEventArgs(views.Count > 1 ? views[views.Count - 2] : null, view));
                ChangeChild(view);
            }
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

            if (views.Count > 0)
            {
                ChangeChild(views.Last());
            }
        }

        /// <summary>
        /// Called to have the fragment instantiate its user interface view.
        /// </summary>
        /// <param name="inflater"></param>
        /// <param name="container"></param>
        /// <param name="savedInstanceState"></param>
        public override global::Android.Views.View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            if (contentContainer == null)
            {
                return (contentContainer = new ViewContentContainer(this));
            }

            (contentContainer.Parent as ViewGroup)?.RemoveView(contentContainer);
            return contentContainer;
        }

        /// <summary>
        /// Removes the top view from the stack.
        /// </summary>
        /// <param name="animate">Whether to use any system-defined transition animation.</param>
        /// <returns>The view that was removed from the stack.</returns>
        public object PopView(Animate animate)
        {
            if (views.Count < 2)
            {
                return null;
            }

            var last = views.Last();
            if (!isPopping)
            {
                isPopping = true;
                var args = new NativeViewStackPoppingEventArgs(last);
                Popping(this, args);
                isPopping = false;
                if (args.Cancel)
                {
                    return null;
                }
            }

            views.RemoveAt(views.Count - 1);
            ViewChanging(this, new NativeViewStackViewChangingEventArgs(last, views.Last()));
            ChangeChild(views.Last());
            return last;
        }

        /// <summary>
        /// Removes every view from the stack except for the root view.
        /// </summary>
        /// <param name="animate">Whether to use any system-defined transition animation.</param>
        /// <returns>An <see cref="Array"/> containing the views that were removed from the stack.</returns>
        public object[] PopToRoot(Animate animate)
        {
            if (views.Count < 2)
            {
                return null;
            }

            var last = views.Last();
            if (!isPopping)
            {
                isPopping = true;
                var args = new NativeViewStackPoppingEventArgs(last);
                Popping(this, args);
                isPopping = false;
                if (args.Cancel)
                {
                    return null;
                }
            }

            var popped = views.Skip(1);
            views.RemoveRange(1, views.Count - 1);

            ViewChanging(this, new NativeViewStackViewChangingEventArgs(last, views.Last()));
            ChangeChild(views.Last());
            return popped.ToArray();
        }

        /// <summary>
        /// Removes from the stack every view on top of the specified view.
        /// </summary>
        /// <param name="view">The view to pop to.</param>
        /// <param name="animate">Whether to use any system-defined transition animation.</param>
        /// <returns>An <see cref="Array"/> containing the views that were removed from the stack.</returns>
        public object[] PopToView(object view, Animate animate)
        {
            var last = views.Last();
            if (last == view)
            {
                return null;
            }

            if (!isPopping)
            {
                isPopping = true;
                var args = new NativeViewStackPoppingEventArgs(last);
                Popping(this, args);
                isPopping = false;
                if (args.Cancel)
                {
                    return null;
                }
            }

            int index = views.IndexOf(view);

            var popped = views.Skip(++index);
            views.RemoveRange(index, views.Count - index);

            ViewChanging(this, new NativeViewStackViewChangingEventArgs(last, view));
            ChangeChild(view);
            return popped.ToArray();
        }

        /// <summary>
        /// Pushes the specified view onto the top of the stack.
        /// </summary>
        /// <param name="view">The view to push to the top of the stack.</param>
        /// <param name="animate">Whether to use any system-defined transition animation.</param>
        public void PushView(object view, Animate animate)
        {
            views.Add(view);
            ViewChanging(this, new NativeViewStackViewChangingEventArgs(views.Count > 1 ? views[views.Count - 2] : null, view));
            ChangeChild(view);
        }

        /// <summary>
        /// Replaces a view that is currently on the stack with the specified view.
        /// </summary>
        /// <param name="oldView">The view to be replaced.</param>
        /// <param name="newView">The view with which to replace the old view.</param>
        /// <param name="animate">Whether to use any system-defined transition animation.</param>
        public void ReplaceView(object oldView, object newView, Animate animate)
        {
            int index = views.IndexOf(oldView);

            views[index] = newView;
            if (index == views.Count - 1)
            {
                ViewChanging(this, new NativeViewStackViewChangingEventArgs(oldView, newView));
                ChangeChild(newView);
            }
        }

        /// <summary>
        /// Swaps out the current child for the specified one.
        /// </summary>
        /// <param name="newChild">The new child.</param>
        protected void ChangeChild(object newChild)
        {
            if (Activity == null)
            {
                return;
            }

            var contentView = newChild as INativeContentView;
            if (contentView != null)
            {
                Header.Title = contentView.Title;
            }

            var transaction = ChildFragmentManager.IsDestroyed ? null : ChildFragmentManager.BeginTransaction();
            var fragment = newChild as Fragment;
            if (transaction != null && fragment != null)
            {
                transaction.Replace(1, fragment);
                transaction.Commit();
            }
            else
            {
                var view = newChild as global::Android.Views.View;
                if (view != null)
                {
                    var oldFrag = ChildFragmentManager.FindFragmentById(1);
                    if (oldFrag != null)
                    {
                        transaction.Remove(oldFrag);
                        transaction.Commit();
                    }

                    if (contentContainer != null)
                    {
                        (view.Parent as ViewGroup)?.RemoveView(view);
                        if (contentContainer.ChildCount > 2)
                        {
                            contentContainer.RemoveViewAt(1);
                            
                            var vsh = Header as ViewStackHeader;
                            if (vsh != null)
                            {
                                var menu = vsh.Menu as ActionMenu;
                                vsh.Menu = null;
                                menu?.OnUnloaded();
                            }
                        }

                        contentContainer.AddView(view, 1);
                    }
                }
            }

            ViewChanged(this, EventArgs.Empty);
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
            if (views.Count > 0 && contentContainer.ChildCount < 2)
            {
                ChangeChild(views.Last());
            }

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

        private class ViewContentContainer : FrameLayout, IFragmentView, ITouchDispatcher
        {
            public Fragment Fragment
            {
                get { return ViewStack; }
            }
            
            public bool IsDispatching { get; private set; }

            public ViewStack ViewStack { get; }

            public ViewContentContainer(ViewStack viewStack)
                : base(Application.MainActivity)
            {
                ViewStack = viewStack;

                Id = 1;
                LayoutParameters = new LinearLayout.LayoutParams(LayoutParams.MatchParent, LayoutParams.MatchParent);

                var headerView = viewStack.Header as global::Android.Views.View;
                (headerView.Parent as ViewGroup)?.RemoveView(headerView);
                AddView(headerView, new ViewGroup.LayoutParams(LayoutParams.MatchParent, LayoutParams.MatchParent));

                SetFrame();
                SetTransform();

                ChildViewAdded += (sender, e) =>
                {
                    BringChildToFront(ViewStack.Header as global::Android.Views.View);
                };
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
                return ViewStack != null && !ViewStack.IsHitTestVisible;
            }

            public void SetFrame()
            {
                Left = (int)(ViewStack.frame.Left * Device.Current.DisplayScale);
                Top = (int)(ViewStack.frame.Top * Device.Current.DisplayScale);
                Right = (int)(ViewStack.frame.Right * Device.Current.DisplayScale);
                Bottom = (int)(ViewStack.frame.Bottom * Device.Current.DisplayScale);
            }
            
            public void SetTransform()
            {
                var transform = ViewStack.renderTransform as Media.Transform;
                if (transform == null)
                {
                    Animation = ViewStack.renderTransform as global::Android.Views.Animations.Animation;
                }
                else
                {
                    transform.AddView(this);
                }
            }

            protected override void OnAttachedToWindow()
            {
                base.OnAttachedToWindow();
                ViewStack.OnLoaded();
            }

            protected override void OnDetachedFromWindow()
            {
                base.OnDetachedFromWindow();
                ViewStack.OnUnloaded();
            }

            protected override void OnLayout(bool changed, int left, int top, int right, int bottom)
            {
                ViewStack.MeasureRequest(false, null);
                ViewStack.ArrangeRequest(false, null);

                int width = (int)(ViewStack.frame.Width * Device.Current.DisplayScale);
                var headerView = ViewStack.Header as global::Android.Views.View;
                headerView.Layout(0, 0, width, headerView.Height);
                    
                var content = GetChildAt(0);
                if (content != null && content != headerView)
                {
                    content.Layout(0, headerView.Bottom, width, bottom);
                }
            }
        }
    }
}