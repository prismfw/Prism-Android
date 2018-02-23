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
using Android.Graphics.Drawables;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Prism.Native;
using Prism.UI;
using Prism.UI.Controls;
using Prism.UI.Media;

namespace Prism.Android.UI.Controls
{
    /// <summary>
    /// Represents an Android implementation for an <see cref="INativeFlyout"/>.
    /// </summary>
    [Preserve(AllMembers = true)]
    [Register(typeof(INativeFlyout))]
    public class Flyout : PopupWindow, INativeFlyout, PopupWindow.IOnDismissListener, IVisualTreeObject
    {
        /// <summary>
        /// Occurs when the flyout has been closed.
        /// </summary>
        public event EventHandler Closed;

        /// <summary>
        /// Occurs when this instance has been attached to the visual tree and is ready to be rendered.
        /// </summary>
        public event EventHandler Loaded;

        /// <summary>
        /// Occurs when the flyout has been opened.
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
            get { return AnimationStyle != 0; }
            set
            {
                if (value != AreAnimationsEnabled)
                {
                    AnimationStyle = value ? -1 : 0;
                    OnPropertyChanged(Visual.AreAnimationsEnabledProperty);
                }
            }
        }

        /// <summary>
        /// Gets or sets the method to invoke when this instance requests an arrangement of its children.
        /// </summary>
        public ArrangeRequestHandler ArrangeRequest { get; set; }

        /// <summary>
        /// Gets or sets the background for the flyout.
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
                    SetBackgroundDrawable(background.GetDrawable(OnBackgroundImageLoaded));
                    OnPropertyChanged(FlyoutBase.BackgroundProperty);
                }
            }
        }
        private Brush background;

        /// <summary>
        /// Gets or sets the element that serves as the content of the flyout.
        /// </summary>
        public object Content
        {
            get { return content; }
            set
            {
                if (value != content)
                {
                    content = value;

                    var layout = ContentView as FlyoutLayout;
                    layout.RemoveAllViews();

                    var view = content as global::Android.Views.View;
                    if (view != null)
                    {
                        layout.AddView(view, new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent));
                    }
    
                    OnPropertyChanged(Prism.UI.Controls.Flyout.ContentProperty);
                }
            }
        }
        private object content;

        /// <summary>
        /// Gets or sets a <see cref="Rectangle"/> that represents the size and position of the object relative to its parent container.
        /// </summary>
        public Rectangle Frame
        {
            get { return new Rectangle(0, 0, Width.GetScaledDouble(), Height.GetScaledDouble()); }
            set
            {
                int width = value.Width.GetScaledInt();
                int height = value.Height.GetScaledInt();

                if (Width != width || Height != height)
                {
                    Width = width;
                    Height = height;

                    if (IsShowing)
                    {
                        var offset = GetOffset();
                        Update(anchorView, (int)offset.X, (int)offset.Y, width, height);
                    }
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance can be considered a valid result for hit testing.
        /// </summary>
        public bool IsHitTestVisible
        {
            get { return Touchable; }
            set
            {
                if (value != Touchable)
                {
                    Touchable = value;
                    OnPropertyChanged(Visual.IsHitTestVisibleProperty);
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
        /// Gets or sets the placement of the flyout in relation to its placement target.
        /// </summary>
        public FlyoutPlacement Placement
        {
            get { return placement; }
            set
            {
                if (value != placement)
                {
                    placement = value;
                    OnPropertyChanged(FlyoutBase.PlacementProperty);
                }
            }
        }
        private FlyoutPlacement placement;

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
                    (renderTransform as Media.Transform)?.RemoveView(ContentView);
                    renderTransform = value;

                    var transform = renderTransform as Media.Transform;
                    if (transform == null)
                    {
                        ContentView.Animation = renderTransform as global::Android.Views.Animations.Animation;
                    }
                    else
                    {
                        transform.AddView(ContentView);
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

        object[] IVisualTreeObject.Children
        {
            get { return ContentView == null ? null : new[] { ContentView }; }
        }

        object IVisualTreeObject.Parent
        {
            get { return null; }
        }

        private global::Android.Views.View anchorView;

        /// <summary>
        /// Initializes a new instance of the <see cref="Flyout"/> class.
        /// </summary>
        public Flyout()
        {
            ContentView = new FlyoutLayout(this);
            Focusable = true;
            Touchable = true;
            OutsideTouchable = true;
            SetBackgroundDrawable(new ColorDrawable());
            SetOnDismissListener(this);
        }

        /// <summary>
        /// Dismisses the flyout.
        /// </summary>
        public void Hide()
        {
            Dismiss();
        }

        /// <summary>
        /// Invalidates the arrangement of this instance's children.
        /// </summary>
        public void InvalidateArrange()
        {
            ContentView.RequestLayout();
        }

        /// <summary>
        /// Invalidates the measurement of this instance and its children.
        /// </summary>
        public void InvalidateMeasure()
        {
            ContentView.RequestLayout();
        }

        /// <summary>
        /// Measures the object and returns its desired size.
        /// </summary>
        /// <param name="constraints">The width and height that the object is not allowed to exceed.</param>
        public Size Measure(Size constraints)
        {
            return constraints;
        }

        /// <summary></summary>
        public void OnDismiss()
        {
            Closed(this, EventArgs.Empty);
        }

        /// <summary>
        /// Presents the flyout and positions it relative to the specified placement target.
        /// </summary>
        /// <param name="placementTarget">The object to use as the flyout's placement target.</param>
        public void ShowAt(object placementTarget)
        {
            var view = placementTarget as global::Android.Views.View;
            if (view != null)
            {
                anchorView = view;
                var offset = GetOffset();

                ShowAsDropDown(anchorView, (int)offset.X, (int)offset.Y);
                Opened(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Called when a property value is changed.
        /// </summary>
        /// <param name="pd">A property descriptor describing the property whose value has been changed.</param>
        protected virtual void OnPropertyChanged(PropertyDescriptor pd)
        {
            PropertyChanged.Invoke(this, new FrameworkPropertyChangedEventArgs(pd));
        }

        private void OnBackgroundImageLoaded(object sender, EventArgs e)
        {
            SetBackgroundDrawable(background.GetDrawable(null));
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

        private Point GetOffset()
        {
            double anchorWidth = anchorView.Width;
            double anchorHeight = anchorView.Height;
            double x, y;

            switch (placement)
            {
                case FlyoutPlacement.Bottom:
                    y = 0;
                    x = anchorWidth / 2 - Width / 2;
                    break;
                case FlyoutPlacement.Left:
                    y = -anchorHeight / 2 - Height / 2;
                    x = -Width;
                    break;
                case FlyoutPlacement.Right:
                    y = -anchorHeight / 2 - Height / 2;
                    x = anchorWidth;
                    break;
                case FlyoutPlacement.Top:
                    y = -anchorHeight - Height;
                    x = anchorWidth / 2 - Width / 2;
                    break;
                default:
                    x = 0;
                    y = 0;
                    break;
            }

            return new Point(x, y);
        }

        private class FlyoutLayout : FrameLayout, IVisualTreeObject
        {
            private readonly WeakReference flyoutRef;

            public FlyoutLayout(Flyout flyout)
                : base(Application.MainActivity)
            {
                flyoutRef = new WeakReference(flyout);
            }

            object[] IVisualTreeObject.Children
            {
                get { return null; }
            }

            object IVisualTreeObject.Parent
            {
                get { return flyoutRef.Target; }
            }

            /// <summary>
            /// This is called when the view is attached to a window.
            /// </summary>
            protected override void OnAttachedToWindow()
            {
                base.OnAttachedToWindow();
                (flyoutRef.Target as Flyout)?.OnLoaded();
            }

            /// <summary>
            /// This is called when the view is detached from a window.
            /// </summary>
            protected override void OnDetachedFromWindow()
            {
                base.OnDetachedFromWindow();
                (flyoutRef.Target as Flyout)?.OnUnloaded();
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
                (flyoutRef.Target as Flyout)?.ArrangeRequest(false, null);

                Left = 0;
                Top = 0;

                var flyout = flyoutRef.Target as Flyout;
                Right = flyout?.Width ?? 0;
                Bottom = flyout?.Height ?? 0;

                base.OnLayout(changed, Left, Top, Right, Bottom);
            }

            /// <summary>
            /// Measure the view and its content to determine the measured width and the measured height.
            /// </summary>
            /// <param name="widthMeasureSpec">Horizontal space requirements as imposed by the parent.</param>
            /// <param name="heightMeasureSpec">Vertical space requirements as imposed by the parent.</param>
            protected override void OnMeasure(int widthMeasureSpec, int heightMeasureSpec)
            {
                (flyoutRef.Target as Flyout)?.MeasureRequest(false, null);
                base.OnMeasure(widthMeasureSpec, heightMeasureSpec);
            }
        }
    }
}

