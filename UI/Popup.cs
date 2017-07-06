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
using Android.App;
using Android.Content;
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
    /// Represents an Android implementation for an <see cref="INativePopup"/>.
    /// </summary>
    [Preserve(AllMembers = true)]
    [Register(typeof(INativePopup))]
    public class Popup : DialogFragment, INativePopup
    {
        /// <summary>
        /// Occurs when the popup has been closed.
        /// </summary>
        public event EventHandler Closed;

        /// <summary>
        /// Occurs when this instance has been attached to the visual tree and is ready to be rendered.
        /// </summary>
        public event EventHandler Loaded;

        /// <summary>
        /// Occurs when the popup has been opened.
        /// </summary>
        public event EventHandler Opened;

        /// <summary>
        /// Occurs when a property value changes.
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
        /// Gets or sets the object that acts as the content of the popup.
        /// This is typically an <see cref="IView"/> or <see cref="INativeViewStack"/> instance.
        /// </summary>
        public object Content
        {
            get { return content; }
            set
            {
                content = value;
                contentContainer?.SetContent();
            }
        }
        private object content;
        
        /// <summary>
        /// Gets or sets a <see cref="Rectangle"/> that represents the size and position of the object relative to its parent container.
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
        /// Gets or sets a value indicating whether the popup can be dismissed by pressing outside of its bounds.
        /// </summary>
        public bool IsLightDismissEnabled
        {
            get { return Cancelable; }
            set
            {
                if (value != Cancelable)
                {
                    Cancelable = value;
                    Dialog?.SetCancelable(value);
                    Dialog?.SetCanceledOnTouchOutside(value);
                    OnPropertyChanged(Prism.UI.Popup.IsLightDismissEnabledProperty);
                }
            }
        }
        
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

        private ViewContentContainer contentContainer;

        /// <summary>
        /// Initializes a new instance of the <see cref="Popup"/> class.
        /// </summary>
        public Popup()
        {
            SetStyle(DialogFragmentStyle.NoFrame, 0);
        }

        /// <summary>
        /// Closes the popup.
        /// </summary>
        public void Close()
        {
            if (!IsVisible)
            {
                return;
            }

            Dismiss();
        }

        /// <summary>
        /// Invalidates the arrangement of this instance's children.
        /// </summary>
        public void InvalidateArrange()
        {
            View?.RequestLayout();
        }

        /// <summary>
        /// Invalidates the measurement of this instance and its children.
        /// </summary>
        public void InvalidateMeasure()
        {
            View?.RequestLayout();
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
        /// Opens the popup using the specified presenter and presentation style.
        /// </summary>
        /// <param name="presenter">The object responsible for presenting the popup.</param>
        /// <param name="style">The style in which to present the popup.</param>
        public void Open(object presenter, PopupPresentationStyle style)
        {
            if (IsVisible)
            {
                return;
            }

            base.Show((presenter as Fragment)?.FragmentManager ?? (presenter as Activity)?.FragmentManager ?? Application.MainActivity.FragmentManager, null);
            Opened(this, EventArgs.Empty);
        }

        /// <summary>
        /// Called to have the fragment instantiate its user interface view.
        /// </summary>
        /// <param name="inflater"></param>
        /// <param name="container"></param>
        /// <param name="savedInstanceState"></param>
        public override global::Android.Views.View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            Dialog.SetCancelable(Cancelable);
            Dialog.SetCanceledOnTouchOutside(Cancelable);
        
            if (contentContainer?.Parent != null)
            {
                (contentContainer.Parent as ViewGroup)?.RemoveView(contentContainer);
                return contentContainer;
            }

            return (contentContainer = new ViewContentContainer(this));
        }

        /// <summary>
        /// This method will be invoked when the dialog is dismissed.
        /// </summary>
        /// <param name="dialog"></param>
        public override void OnDismiss(IDialogInterface dialog)
        {
            base.OnDismiss(dialog);
            Closed(this, EventArgs.Empty);
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

        private class ViewContentContainer : FrameLayout, IFragmentView, ITouchDispatcher, global::Android.Views.View.IOnClickListener
        {            
            public Popup Popup { get; }

            public Fragment Fragment
            {
                get { return Popup; }
            }
            
            public bool IsDispatching { get; private set; }

            public ViewContentContainer(Popup popup)
                : base(popup.Activity)
            {
                Background = Android.Resources.GetDrawable(this, global::Android.Resource.Attribute.WindowBackground);
                Popup = popup;
                Id = 1;
                LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent);

                SetOnClickListener(this);
                SetContent();
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
                this.DispatchTouchEventToChildren(e);
                IsDispatching = false;
                return base.DispatchTouchEvent(e);
            }
            
            public void OnClick(global::Android.Views.View v)
            {
                if (v != this && v != null && v.Clickable)
                {
                    v.PerformClick();
                }
            }

            public override bool OnInterceptTouchEvent(MotionEvent ev)
            {
                return Popup != null && !Popup.IsHitTestVisible;
            }
            
            public void SetContent()
            {
                var rootView = Popup.Content as global::Android.Views.View;
                if (rootView?.Parent != null)
                {
                    ((ViewGroup)rootView.Parent).RemoveView(rootView);
                }
    
                RemoveAllViews();
    
                if (rootView != null)
                {
                    AddView(rootView);
                }
                else
                {
                    var fragment = Popup.Content as Fragment;
                    if (fragment != null)
                    {
                        var fragmentTransaction = Popup.ChildFragmentManager.BeginTransaction();
                        fragmentTransaction.Replace(1, fragment);
                        fragmentTransaction.Commit();
                    }
                }
            }
            
            public void SetTransform()
            {
                var transform = Popup.renderTransform as Media.Transform;
                if (transform == null)
                {
                    Animation = Popup.renderTransform as global::Android.Views.Animations.Animation;
                }
                else
                {
                    transform.AddView(this);
                }
            }

            protected override void OnAttachedToWindow()
            {
                base.OnAttachedToWindow();
                Popup.OnLoaded();
            }

            protected override void OnDetachedFromWindow()
            {
                base.OnDetachedFromWindow();
                Popup.OnUnloaded();
            }

            protected override void OnLayout(bool changed, int left, int top, int right, int bottom)
            {
                Popup.ArrangeRequest(false, null);

                Right = (int)Math.Ceiling(Popup.Frame.Width * Device.Current.DisplayScale);
                Bottom = (int)Math.Ceiling(Popup.Frame.Height * Device.Current.DisplayScale);
                Left = 0;
                Top = 0;

                Popup.Dialog.Window.SetLayout(Width, Height);
                Popup.Dialog.Window.SetGravity(GravityFlags.CenterHorizontal | GravityFlags.CenterVertical);

                base.OnLayout(changed, Left, Top, Right, Bottom);
            }

            protected override void OnMeasure(int widthMeasureSpec, int heightMeasureSpec)
            {
                Popup.MeasureRequest(false, null);
                base.OnMeasure(widthMeasureSpec, heightMeasureSpec);
            }
        }
    }
}

