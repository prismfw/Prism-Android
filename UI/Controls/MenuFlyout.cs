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
using Android.Graphics.Drawables;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Prism.Native;
using Prism.Systems;
using Prism.UI;
using Prism.UI.Controls;
using Prism.UI.Media;

using View = global::Android.Views.View;

namespace Prism.Android.UI.Controls
{
    /// <summary>
    /// Represents an Android implementation for an <see cref="INativeMenuFlyout"/>.
    /// </summary>
    [Preserve(AllMembers = true)]
    [Register(typeof(INativeMenuFlyout))]
    public class MenuFlyout : ListPopupWindow, INativeMenuFlyout, IVisualTreeObject, PopupWindow.IOnDismissListener, AdapterView.IOnItemClickListener, View.IOnAttachStateChangeListener, View.IOnLayoutChangeListener, View.IOnTouchListener
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
        /// Gets or sets the <see cref="Brush"/> to apply to the foreground content of the menu.
        /// </summary>
        public Brush Foreground
        {
            get { return foreground; }
            set
            {
                if (value != foreground)
                {
                    foreground = value;
                    OnPropertyChanged(Prism.UI.Controls.MenuFlyout.ForegroundProperty);
                }
            }
        }
        private Brush foreground;

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
        /// Gets a value indicating whether this instance has been loaded and is ready for rendering.
        /// </summary>
        public bool IsLoaded { get; private set; }

        /// <summary>
        /// Gets a collection of the items within the menu.
        /// </summary>
        public IList Items { get; }

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
                    if (ListView != null)
                    {
                        (renderTransform as Media.Transform)?.RemoveView(ListView);
                    }

                    renderTransform = value;

                    if (ListView != null)
                    {
                        SetTransform();
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
            get { return ListView == null ? null : new[] { ListView }; }
        }

        object IVisualTreeObject.Parent
        {
            get { return null; }
        }

        private bool isOffsetValid;

        /// <summary>
        /// Initializes a new instance of the <see cref="MenuFlyout"/> class.
        /// </summary>
        public MenuFlyout()
            : base(Application.MainActivity)
        {
            Items = new List<INativeMenuItem>();
            Modal = true;

            SetOnDismissListener(this);
            SetOnItemClickListener(this);
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
            ListView?.RequestLayout();
        }

        /// <summary>
        /// Invalidates the measurement of this instance and its children.
        /// </summary>
        public void InvalidateMeasure()
        {
            ListView?.RequestLayout();
        }

        /// <summary>
        /// Measures the object and returns its desired size.
        /// </summary>
        /// <param name="constraints">The width and height that the object is not allowed to exceed.</param>
        public Size Measure(Size constraints)
        {
            return ListView == null ? Size.Empty : new Size(ListView.Width, ListView.Height) / Device.Current.DisplayScale;
        }

        /// <summary></summary>
        public void OnDismiss()
        {
            Closed(this, EventArgs.Empty);
        }

        /// <summary></summary>
        /// <param name="parent"></param>
        /// <param name="view"></param>
        /// <param name="position"></param>
        /// <param name="id"></param>
        public void OnItemClick(AdapterView parent, View view, int position, long id)
        {
            var item = Items[position] as INativeMenuButton;
            if (item != null)
            {
                item.Action?.Invoke();
            }

            Dismiss();
        }

        /// <summary></summary>
        /// <param name="v"></param>
        /// <param name="left"></param>
        /// <param name="top"></param>
        /// <param name="right"></param>
        /// <param name="bottom"></param>
        /// <param name="oldLeft"></param>
        /// <param name="oldTop"></param>
        /// <param name="oldRight"></param>
        /// <param name="oldBottom"></param>
        public void OnLayoutChange(View v, int left, int top, int right, int bottom, int oldLeft, int oldTop, int oldRight, int oldBottom)
        {
            var frame = Frame;
            MeasureRequest(true, null);
            ArrangeRequest(true, null);

            if ((!isOffsetValid || frame.Size != Frame.Size) && IsShowing)
            {
                isOffsetValid = true;
                SetOffset();
                Show();
            }
        }

        /// <summary></summary>
        /// <param name="v"></param>
        /// <param name="e"></param>
        public bool OnTouch(View v, MotionEvent e)
        {
            return !isHitTestVisible;
        }

        /// <summary></summary>
        /// <param name="attachedView"></param>
        public void OnViewAttachedToWindow(View attachedView)
        {
            OnLoaded();
        }

        /// <summary></summary>
        /// <param name="detachedView"></param>
        public void OnViewDetachedFromWindow(View detachedView)
        {
            OnUnloaded();
        }

        /// <summary>
        /// Presents the flyout and positions it relative to the specified placement target.
        /// </summary>
        /// <param name="placementTarget">The object to use as the flyout's placement target.</param>
        public void ShowAt(object placementTarget)
        {
            var view = placementTarget as View;
            if (view != null)
            {
                AnchorView = view;

                SetAdapter(new MenuAdapter(this));
                if (ListView != null)
                {
                    SetOffset();
                }
                else
                {
                    isOffsetValid = false;
                }

                Show();

                ListView.RemoveOnAttachStateChangeListener(this);
                ListView.AddOnAttachStateChangeListener(this);
                ListView.RemoveOnLayoutChangeListener(this);
                ListView.AddOnLayoutChangeListener(this);
                ListView.SetOnTouchListener(this);

                (renderTransform as Media.Transform)?.RemoveView(ListView);
                if (renderTransform != null)
                {
                    SetTransform();
                }

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

        private void SetOffset()
        {
            double anchorWidth = AnchorView.Width;
            double anchorHeight = AnchorView.Height;
            double x, y;

            switch (placement)
            {
                case FlyoutPlacement.Bottom:
                    y = 0;
                    x = anchorWidth / 2d - ListView.Width / 2d;
                    break;
                case FlyoutPlacement.Left:
                    y = -anchorHeight / 2d - ListView.Height / 2d;
                    x = -ListView.Width;
                    break;
                case FlyoutPlacement.Right:
                    y = -anchorHeight / 2d - ListView.Height / 2d;
                    x = anchorWidth;
                    break;
                case FlyoutPlacement.Top:
                    y = -anchorHeight - ListView.Height;
                    x = anchorWidth / 2d - ListView.Width / 2d;
                    break;
                default:
                    x = 0;
                    y = 0;
                    break;
            }

            HorizontalOffset = (int)x;
            VerticalOffset = (int)y;
        }

        private void SetTransform()
        {
            var transform = renderTransform as Media.Transform;
            if (transform == null)
            {
                ListView.Animation = renderTransform as global::Android.Views.Animations.Animation;
            }
            else
            {
                transform.AddView(ListView);
            }
        }

        private class MenuAdapter : BaseAdapter
        {
            public override int Count
            {
                get { return (flyoutRef.Target as INativeMenuFlyout)?.Items.Count ?? 0; }
            }

            private readonly WeakReference flyoutRef;

            public MenuAdapter(MenuFlyout flyout)
            {
                flyoutRef = new WeakReference(flyout);
            }

            public override Java.Lang.Object GetItem(int position)
            {
                return position;
            }

            public override long GetItemId(int position)
            {
                return position;
            }

            public override View GetView(int position, View convertView, ViewGroup parent)
            {
                View view = null;
                var flyout = flyoutRef.Target as INativeMenuFlyout;
                var item = flyout.Items[position];

                var button = item as INativeMenuButton;
                if (button != null)
                {
                    var textView = convertView as TextView ?? new TextView(Application.MainActivity);
                    textView.Background = null;
                    textView.Text = button.Title;

                    if (button.IsEnabled)
                    {
                        textView.Paint.SetBrush(button.Foreground ?? flyout.Foreground, parent.Width, textView.Paint.FontSpacing + 0.5f, null);
                        textView.SetTextColor(textView.Paint.Color);
                    }
                    else
                    {
                        textView.Paint.SetShader(null);
                        textView.SetTextColor(new global::Android.Graphics.Color(110, 110, 110));
                    }

                    view = textView;
                }
                else
                {
                    var separator = item as INativeMenuSeparator;
                    if (separator != null)
                    {
                        view = new View(Application.MainActivity);
                        view.Background = (separator.Foreground ?? flyout.Foreground).GetDrawable(null);
                        view.SetMinimumHeight((int)(1 * Device.Current.DisplayScale));
                        view.SetMinimumWidth(parent.Width);
                    }
                }

                view.SetPadding((int)(6 * Device.Current.DisplayScale), (int)(10 * Device.Current.DisplayScale),
                        (int)(6 * Device.Current.DisplayScale), (int)(10 * Device.Current.DisplayScale));

                view.LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);

                return view;
            }

            public override bool IsEnabled(int position)
            {
                return ((flyoutRef.Target as INativeMenuFlyout)?.Items[position] as INativeMenuButton)?.IsEnabled ?? false;
            }
        }
    }
}

