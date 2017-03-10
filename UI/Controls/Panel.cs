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
using System.Collections.Generic;
using System.Linq;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Prism.Input;
using Prism.Native;
using Prism.Systems;
using Prism.UI;
using Prism.UI.Media;

namespace Prism.Android.UI.Controls
{
    /// <summary>
    /// Represents an Android implementation of an <see cref="INativePanel"/>.
    /// </summary>
    [Preserve(AllMembers = true)]
    [Register(typeof(INativePanel))]
    public class Panel : RelativeLayout, INativePanel, ITouchDispatcher
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
        /// Gets or sets the background for the panel.
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
                    base.Background = background.GetDrawable(OnBackgroundImageLoaded);
                    OnPropertyChanged(Prism.UI.Controls.Panel.BackgroundProperty);
                }
            }
        }
        private Brush background;

        /// <summary>
        /// Gets a collection of the child elements that currently belong to this instance.
        /// </summary>
        public IList Children
        {
            get { return children; }
        }
        private readonly PanelChildrenList children;

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

                Measure(MeasureSpec.MakeMeasureSpec(Right - Left, MeasureSpecMode.Exactly),
                    MeasureSpec.MakeMeasureSpec(Bottom - Top, MeasureSpecMode.Exactly));
                Layout(Left, Top, Right, Bottom);
            }
        }
        
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

        private bool touchEventHandledByChildren;

        /// <summary>
        /// Initializes a new instance of the <see cref="Panel"/> class.
        /// </summary>
        public Panel()
            : base(Application.MainActivity)
        {
            children = new PanelChildrenList(this);
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
            touchEventHandledByChildren = this.DispatchTouchEventToChildren(e);
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
        /// Measures the element and returns its desired size.
        /// </summary>
        /// <param name="constraints">The width and height that the element is not allowed to exceed.</param>
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
        
        /// <summary></summary>
        /// <param name="e"></param>
        public override bool OnTouchEvent(MotionEvent e)
        {
            if (!isHitTestVisible)
            {
                return false;
            }
            
            if (!touchEventHandledByChildren)
            {
                if (e.Action == MotionEventActions.Cancel)
                {
                    PointerCanceled(this, e.GetPointerEventArgs(this));
                }
                else if (e.Action == MotionEventActions.Down)
                {
                    PointerPressed(this, e.GetPointerEventArgs(this));
                }
                else if (e.Action == MotionEventActions.Move)
                {
                    PointerMoved(this, e.GetPointerEventArgs(this));
                }
                else if (e.Action == MotionEventActions.Up)
                {
                    PointerReleased(this, e.GetPointerEventArgs(this));
                }
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

            for (int i = 0; i < ChildCount; i++)
            {
                var child = GetChildAt(i);
                child.Layout(child.Left, child.Top, child.Right, child.Bottom);
            }
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
            base.Background = background.GetDrawable(null);
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

        private class PanelChildrenList : IList
        {
            public int Count
            {
                get { return Views.Count(); }
            }

            public bool IsFixedSize
            {
                get { return false; }
            }

            public bool IsReadOnly
            {
                get { return false; }
            }

            public bool IsSynchronized
            {
                get { return false; }
            }

            public object SyncRoot
            {
                get { return null; }
            }

            public object this[int index]
            {
                get { return Views.ElementAt(index); }
                set
                {
                    index = parent.IndexOfChild(Views.ElementAt(index));
                    var child = value as global::Android.Views.View;
                    if (child == null)
                    {
                        throw new ArgumentException("Value must be an object of type View.", nameof(value));
                    }

                    parent.RemoveViewAt(index);
                    parent.AddView(child, index);
                }
            }

            private readonly ViewGroup parent;
            
            public PanelChildrenList(ViewGroup parent)
            {
                this.parent = parent;
            }

            public int Add(object value)
            {
                int count = parent.ChildCount;
                var child = value as global::Android.Views.View;
                if (child == null)
                {
                    throw new ArgumentException("Value must be an object of type View.", nameof(value));
                }

                parent.AddView(child);
                return parent.ChildCount - count;
            }

            public void Clear()
            {
                for (int i = parent.ChildCount - 1; i >= 0; i--)
                {
                    if (parent.GetChildAt(i) is INativeElement)
                    {
                        parent.RemoveViewAt(i);
                    }
                }
            }

            public bool Contains(object value)
            {
                return Views.Any(sv => sv == value);
            }

            public int IndexOf(object value)
            {
                int index = 0;
                foreach (var view in Views.OfType<INativeElement>())
                {
                    if (view == value)
                    {
                        return index;
                    }

                    index++;
                }

                return -1;
            }

            public void Insert(int index, object value)
            {
                var child = value as global::Android.Views.View;
                if (child == null)
                {
                    throw new ArgumentException("Value must be an object of type View.", nameof(value));
                }

                parent.AddView(child, parent.IndexOfChild(Views.ElementAt(index)));
            }

            public void Remove(object value)
            {
                var child = value as global::Android.Views.View;
                if (child != null)
                {
                    parent.RemoveView(child);
                }
            }

            public void RemoveAt(int index)
            {
                parent.RemoveView(Views.ElementAt(index));
            }

            public void CopyTo(Array array, int index)
            {
                Views.ToArray().CopyTo(array, index);
            }

            public IEnumerator GetEnumerator()
            {
                return new PanelChildrenEnumerator(Views.GetEnumerator());
            }

            private IEnumerable<global::Android.Views.View> Views
            {
                get
                {
                    for (int i = 0; i < parent.ChildCount; i++)
                    {
                        var view = parent.GetChildAt(i);
                        if (view is INativeElement)
                        {
                            yield return view;
                        }
                    }
                }
            }

            private class PanelChildrenEnumerator : IEnumerator<INativeElement>, IEnumerator
            {
                public INativeElement Current
                {
                    get { return viewEnumerator.Current as INativeElement; }
                }

                object IEnumerator.Current
                {
                    get { return viewEnumerator.Current; }
                }

                private readonly IEnumerator viewEnumerator;

                public PanelChildrenEnumerator(IEnumerator viewEnumerator)
                {
                    this.viewEnumerator = viewEnumerator;
                }

                public void Dispose()
                {
                    var disposable = viewEnumerator as IDisposable;
                    if (disposable != null)
                    {
                        disposable.Dispose();
                    }
                }

                public bool MoveNext()
                {
                    do
                    {
                        if (!viewEnumerator.MoveNext())
                        {
                            return false;
                        }
                    }
                    while (!(viewEnumerator.Current is INativeElement));

                    return true;
                }

                public void Reset()
                {
                    viewEnumerator.Reset();
                }
            }
        }
    }
}

