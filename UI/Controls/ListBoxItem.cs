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
using Android.Graphics.Drawables;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Prism.Input;
using Prism.Native;
using Prism.Systems;
using Prism.UI;
using Prism.UI.Controls;
using Prism.UI.Media;

namespace Prism.Android.UI.Controls
{
    /// <summary>
    /// Represents an Android implementation of an <see cref="INativeListBoxItem"/>.
    /// </summary>
    [Preserve(AllMembers = true)]
    [Register(typeof(INativeListBoxItem))]
    public class ListBoxItem : FrameLayout, INativeListBoxItem, IListBoxChild, ITouchDispatcher
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
        /// Gets or sets the accessory for the item.
        /// </summary>
        public ListBoxItemAccessory Accessory
        {
            get { return accessory; }
            set
            {
                if (value != accessory)
                {
                    accessory = value;
                    OnPropertyChanged(Prism.UI.Controls.ListBoxItem.AccessoryProperty);
                }
            }
        }
        private ListBoxItemAccessory accessory;

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
        /// Gets or sets the background of the item.
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

                    SetBackground();
                    OnPropertyChanged(Prism.UI.Controls.ListBoxItem.BackgroundProperty);
                }
            }
        }
        private Brush background;

        /// <summary>
        /// Gets or sets the panel containing the content to be displayed by the item.
        /// </summary>
        public INativePanel ContentPanel
        {
            get { return GetChildAt(0) as INativePanel; }
            set
            {
                if (value != ContentPanel)
                {
                    RemoveAllViews();
                    AddView(value as global::Android.Views.View);
                    OnPropertyChanged(Prism.UI.Controls.ListBoxItem.ContentPanelProperty);
                }
            }
        }

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
        /// Gets or sets a value indicating whether this instance is selected.
        /// </summary>
        public bool IsSelected
        {
            get { return isSelected; }
            set
            {
                if (value != isSelected)
                {
                    isSelected = value;

                    SetBackground();

                    var labels = this.GetChildren<Label>();
                    foreach (var label in labels)
                    {
                        label.SetForeground(this);
                    }

                    OnPropertyChanged(Prism.UI.Controls.ListBoxItem.IsSelectedProperty);
                }
            }
        }
        private bool isSelected;

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
        /// Gets the containing <see cref="ListBox"/>.
        /// </summary>
        public new ListBox Parent { get; private set; }

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
        /// Gets or sets the background of the item when it is selected.
        /// </summary>
        public Brush SelectedBackground
        {
            get { return selectedBackground; }
            set
            {
                if (value != selectedBackground)
                {
                    (selectedBackground as ImageBrush).ClearImageHandler(OnSelectedBackgroundImageLoaded);

                    selectedBackground = value;

                    SetBackground();
                    OnPropertyChanged(Prism.UI.Controls.ListBoxItem.SelectedBackgroundProperty);
                }
            }
        }
        private Brush selectedBackground;

        /// <summary>
        /// Gets or sets the amount to indent the separator.
        /// </summary>
        public Thickness SeparatorIndentation
        {
            get { return separatorIndentation; }
            set
            {
                if (value != separatorIndentation)
                {
                    separatorIndentation = value;
                    OnPropertyChanged(Prism.UI.Controls.ListBoxItem.SeparatorIndentationProperty);
                }
            }
        }
        private Thickness separatorIndentation;

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

        private Size desiredSize;
        private double? parentWidth;
        private bool touchEventHandledByChildren;

        /// <summary>
        /// Initializes a new instance of the <see cref="ListBoxItem"/> class.
        /// </summary>
        public ListBoxItem()
            : base(Application.MainActivity)
        {
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
            return !IsHitTestVisible;
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
            double? width = Parent?.Width;
            if (width == null)
            {
                width = (ObjectRetriever.GetAgnosticObject(this.GetParent<INativeListBox>()) as Visual)?.RenderSize.Width * Device.Current.DisplayScale;
            }

            ArrangeRequest(Width != width || Height != desiredSize.Height, new Rectangle(0, Top / Device.Current.DisplayScale, (width ?? 0) / Device.Current.DisplayScale, desiredSize.Height));

            parentWidth = width;
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
            double? width = Parent?.Width;
            if (width == null)
            {
                width = (ObjectRetriever.GetAgnosticObject(this.GetParent<INativeListBox>()) as Visual)?.RenderSize.Width * Device.Current.DisplayScale;
            }

            desiredSize = MeasureRequest(width != parentWidth, new Size((width ?? double.PositiveInfinity) / Device.Current.DisplayScale, double.PositiveInfinity));
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
            if (!isSelected && background != null)
            {
                base.Background = background.GetDrawable(null);
            }
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

        private void OnSelectedBackgroundImageLoaded(object sender, EventArgs e)
        {
            if (isSelected && selectedBackground != null)
            {
                base.Background = selectedBackground.GetDrawable(null);
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

        private void SetBackground()
        {
            if (isSelected && selectedBackground != null)
            {
                base.Background = selectedBackground.GetDrawable(OnSelectedBackgroundImageLoaded);
            }
            else if (background != null)
            {
                base.Background = background.GetDrawable(OnBackgroundImageLoaded);
            }
            else
            {
                base.Background = new ColorDrawable(global::Android.Graphics.Color.Transparent);
            }
        }

        void IListBoxChild.SetParent(ListBox parent)
        {
            Parent = parent;
        }
    }
}