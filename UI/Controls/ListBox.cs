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
using System.Collections.Specialized;
using System.Linq;
using Android.Graphics;
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
    /// Represents an Android implementation of an <see cref="INativeListBox"/>.
    /// </summary>
    [Preserve(AllMembers = true)]
    [Register(typeof(INativeListBox))]
    public class ListBox : ListView, INativeListBox, ITouchDispatcher, AbsListView.IOnScrollListener
    {
        /// <summary>
        /// Occurs when an accessory in a list box item is clicked or tapped.
        /// </summary>
        public event EventHandler<AccessoryClickedEventArgs> AccessoryClicked;

        /// <summary>
        /// Occurs when an item in the list box is clicked or tapped.
        /// </summary>
        public event EventHandler<ItemClickedEventArgs> ItemClicked;

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
        /// Occurs when the selection of the list box is changed.
        /// </summary>
        public event EventHandler<SelectionChangedEventArgs> SelectionChanged;

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
        /// Gets or sets the background of the list box.
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
                    OnPropertyChanged(Prism.UI.Controls.ListBox.BackgroundProperty);
                }
            }
        }
        private Brush background;

        /// <summary>
        /// Gets or sets a value indicating whether the contents of the list box can be scrolled horizontally (not used).
        /// </summary>
        public new bool CanScrollHorizontally
        {
            get { return false; }
            set { }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the contents of the list box can be scrolled vertically (not used).
        /// </summary>
        public new bool CanScrollVertically
        {
            get { return true; }
            set { }
        }

        /// <summary>
        /// Gets the distance that the contents has been scrolled.
        /// </summary>
        public Point ContentOffset
        {
            get { return contentOffset; }
            private set
            {
                if (value != contentOffset)
                {
                    contentOffset = value;
                    OnPropertyChanged(Prism.UI.Controls.ListBox.ContentOffsetProperty);
                }
            }
        }
        private Point contentOffset;

        /// <summary>
        /// Gets the size of the scrollable area.
        /// </summary>
        public Size ContentSize
        {
            get { return contentSize; }
            private set
            {
                if (value != contentSize)
                {
                    contentSize = value;
                    OnPropertyChanged(Prism.UI.Controls.ListBox.ContentSizeProperty);
                }
            }
        }
        private Size contentSize;

        /// <summary>
        /// Gets or sets a <see cref="Rectangle"/> that represents the size and position of the element relative to its parent container.
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
        /// Gets or sets a value indicating whether each object in the <see cref="P:Items"/> collection represents a different section in the list.
        /// When <c>true</c>, objects that implement <see cref="IList"/> will have each of their items represent a different entry in the same section.
        /// </summary>
        public bool IsSectioningEnabled
        {
            get { return isSectioningEnabled; }
            set
            {
                if (value != isSectioningEnabled)
                {
                    isSectioningEnabled = value;
                    OnPropertyChanged(Prism.UI.Controls.ListBox.IsSectioningEnabledProperty);

                    if (items != null && items.Count > 0)
                    {
                        GetAdapter()?.NotifyDataSetChanged();
                    }
                }
            }
        }
        private bool isSectioningEnabled;

        /// <summary>
        /// Gets or sets the method to be used for retrieving reuse identifiers for items in the list box.
        /// </summary>
        public ItemIdRequestHandler ItemIdRequest { get; set; }

        /// <summary>
        /// Gets or sets the method to be used for retrieving display items for items in the list box.
        /// </summary>
        public ListBoxItemRequestHandler ItemRequest { get; set; }

        /// <summary>
        /// Gets or sets the items that make up the contents of the list box.
        /// </summary>
        public IList Items
        {
            get { return items; }
            set
            {
                if (value != items)
                {
                    var notifier = items as INotifyCollectionChanged;
                    if (notifier != null)
                    {
                        notifier.CollectionChanged -= OnItemsCollectionChanged;
                        foreach (var item in items.OfType<INotifyCollectionChanged>())
                        {
                            item.CollectionChanged -= OnItemsSubcollectionChanged;
                        }
                    }

                    notifier = value as INotifyCollectionChanged;
                    if (notifier != null)
                    {
                        notifier.CollectionChanged -= OnItemsCollectionChanged;
                        notifier.CollectionChanged += OnItemsCollectionChanged;
                    }

                    if (value != null)
                    {
                        foreach (var item in value.OfType<INotifyCollectionChanged>())
                        {
                            item.CollectionChanged -= OnItemsSubcollectionChanged;
                            item.CollectionChanged += OnItemsSubcollectionChanged;
                        }
                    }

                    items = value;
                    OnPropertyChanged(Prism.UI.Controls.ListBox.ItemsProperty);
                    GetAdapter()?.NotifyDataSetChanged();
                }
            }
        }
        private IList items;

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
        /// Gets or sets the method to be used for retrieving section headers in the list box.
        /// </summary>
        public ListBoxSectionHeaderRequestHandler SectionHeaderRequest { get; set; }

        /// <summary>
        /// Gets or sets the method to be used for retrieving reuse identifiers for section headers.
        /// </summary>
        public ItemIdRequestHandler SectionHeaderIdRequest { get; set; }

        /// <summary>
        /// Gets the currently selected items.
        /// </summary>
        public IList SelectedItems
        {
            get { return selectedIndices.Select(i => GetItemAtPosition(i)).ToList().AsReadOnly(); }
        }

        /// <summary>
        /// Gets or sets the selection behavior for the list box.
        /// </summary>
        public SelectionMode SelectionMode
        {
            get { return selectionMode; }
            set
            {
                if (value != selectionMode)
                {
                    selectionMode = value;
                    if (selectionMode != SelectionMode.Multiple)
                    {
                        var removedItems = selectedIndices.Count == 0 ? null : selectedIndices.Select(i => GetItemAtPosition(i)).ToArray();
                        if (removedItems != null)
                        {
                            selectedIndices.Clear();
                            for (int i = 0; i < ChildCount; i++)
                            {
                                var child = GetChildAt(i) as ListBoxItem;
                                if (child != null)
                                {
                                    child.IsSelected = false;
                                }
                            }

                            SelectionChanged(this, new SelectionChangedEventArgs(null, removedItems));
                        }
                    }

                    OnPropertyChanged(Prism.UI.Controls.ListBox.SelectionModeProperty);
                }
            }
        }
        private SelectionMode selectionMode;

        /// <summary>
        /// Gets or sets the <see cref="Brush"/> to apply to the separators between each item in the list.
        /// </summary>
        public Brush SeparatorBrush
        {
            get { return separatorBrush; }
            set
            {
                if (value != separatorBrush)
                {
                    (separatorBrush as ImageBrush).ClearImageHandler(OnSeparatorImageLoaded);

                    separatorBrush = value;
                    separatorDrawable = separatorBrush.GetDrawable(OnSeparatorImageLoaded) ??
                        Android.Resources.GetDrawable(this, global::Android.Resource.Attribute.TextColorPrimary);

                    OnPropertyChanged(Prism.UI.Controls.ListBox.SeparatorBrushProperty);
                    DividerHeight = 1;
                }
            }
        }
        private Brush separatorBrush;

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

        private readonly List<int> selectedIndices;
        private Drawable separatorDrawable;
        private bool touchEventHandledByChildren;

        /// <summary>
        /// Initializes a new instance of the <see cref="ListBox"/> class.
        /// </summary>
        public ListBox(ListBoxStyle style)
            : base(Application.MainActivity)
        {
            selectedIndices = new List<int>();
            DescendantFocusability = DescendantFocusability.BeforeDescendants;

            Divider = new ListBoxDividerDrawable(this);
            base.Adapter = new ListBoxAdapter(this);
            base.SetOnScrollListener(this);
        }

        /// <summary>
        /// Deselects the specified item.
        /// </summary>
        /// <param name="item">The item within the <see cref="P:Items"/> collection to deselect.</param>
        /// <param name="animate">Whether to animate the deselection.</param>
        public void DeselectItem(object item, Animate animate)
        {
            int index;
            if (items != null && (index = GetPositionForItem(item)) >= 0 && selectedIndices.Remove(index))
            {
                for (int i = 0; i < ChildCount; i++)
                {
                    var child = GetChildAt(i) as ListBoxItem;
                    if (child != null && GetChildAdapterPosition(child) == index)
                    {
                        child.IsSelected = false;
                        break;
                    }
                }

                OnPropertyChanged(Prism.UI.Controls.ListBox.SelectedItemsProperty);
                SelectionChanged(this, new SelectionChangedEventArgs(null, item));
            }
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
        /// Returns a collection of the <see cref="INativeListBoxItem"/> objects that are in the list.
        /// </summary>
        public IEnumerable<INativeListBoxItem> GetChildItems()
        {
            for (int i = 0; i < ChildCount; i++)
            {
                var item = GetChildAt(i) as INativeListBoxItem;
                if (item != null)
                {
                    yield return item;
                }
            }
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
            int width = MeasuredWidth;
            int height = MeasuredHeight;

            var widthSpec = (int)Math.Min(int.MaxValue, constraints.Width * Device.Current.DisplayScale);
            var heightSpec = (int)Math.Min(int.MaxValue, constraints.Height * Device.Current.DisplayScale);
            base.OnMeasure(MeasureSpec.MakeMeasureSpec(widthSpec, widthSpec == int.MaxValue ? MeasureSpecMode.AtMost : MeasureSpecMode.Exactly),
                MeasureSpec.MakeMeasureSpec(heightSpec, heightSpec == int.MaxValue ? MeasureSpecMode.AtMost : MeasureSpecMode.Unspecified));

            var size = new Size(MeasuredWidth, MeasuredHeight) / Device.Current.DisplayScale;
            SetMeasuredDimension(width, height);

            return new Size(Math.Min(constraints.Width, size.Width), Math.Min(constraints.Height, size.Height));
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
        /// Callback method to be invoked when the list or grid has been scrolled. This will be called after the scroll has completed
        /// </summary>
        /// <param name="view">The view whose scroll state is being reported</param>
        /// <param name="firstVisibleItem">the index of the first visible cell (ignore if visibleItemCount == 0)</param>
        /// <param name="visibleItemCount">the number of visible cells</param>
        /// <param name="totalItemCount">the number of items in the list adapter</param>
        public void OnScroll(AbsListView view, int firstVisibleItem, int visibleItemCount, int totalItemCount)
        {
            ContentOffset = new Point(ComputeHorizontalScrollOffset() / Device.Current.DisplayScale, ComputeVerticalScrollOffset() / Device.Current.DisplayScale);
        }

        /// <summary>
        /// Callback method to be invoked while the list view or grid view is being scrolled. If the view is
        /// being scrolled, this method will be called before the next frame of the scroll is rendered. In particular,
        /// it will be called before any calls to GetView(int, View, ViewGroup)
        /// </summary>
        /// <param name="view">The view whose scroll state is being reported</param>
        /// <param name="scrollState">The current scroll state. One of <see cref="ScrollState.TouchScroll"/> or <see cref="ScrollState.Idle"/>.</param>
        public void OnScrollStateChanged(AbsListView view, ScrollState scrollState) { }

        /// <summary>
        /// Forces a reload of the list box's contents.
        /// </summary>
        public void Reload()
        {
            GetAdapter()?.NotifyDataSetChanged();
        }

        private ListBoxAdapter GetAdapter()
        {
            return Adapter as ListBoxAdapter;
        }

        private int GetChildAdapterPosition(ListBoxItem child)
        {
            return (child.Tag as ListBoxViewHolder)?.AdapterPosition ?? -1;
        }

        /// <summary>
        /// Scrolls to the specified item.
        /// </summary>
        /// <param name="item">The item within the <see cref="P:Items"/> collection to which the list box should scroll.</param>
        /// <param name="animate">Whether to animate the scrolling.</param>
        public void ScrollTo(object item, Animate animate)
        {
            int index;
            if (items == null || (index = GetPositionForItem(item)) < 0)
            {
                return;
            }

            if (animate == Prism.UI.Animate.Off || !areAnimationsEnabled)
            {
                SmoothScrollToPositionFromTop(index, 0, 0);
            }
            else
            {
                SmoothScrollToPosition(index);
            }
        }

        /// <summary>
        /// Scrolls the contents within the list box to the specified offset.
        /// </summary>
        /// <param name="offset">The position to which to scroll the contents.</param>
        /// <param name="animate">Whether to animate the scrolling.</param>
        public void ScrollTo(Point offset, Animate animate)
        {
            if (animate == Prism.UI.Animate.Off || !areAnimationsEnabled)
            {
                ScrollBy((int)(offset.X * Device.Current.DisplayScale - contentOffset.X), (int)(offset.Y * Device.Current.DisplayScale - contentOffset.Y));
            }
            else
            {
                SmoothScrollBy((int)(offset.X * Device.Current.DisplayScale - contentOffset.X), (int)(offset.Y * Device.Current.DisplayScale - contentOffset.Y));
            }
        }

        /// <summary>
        /// Selects the specified item.
        /// </summary>
        /// <param name="item">The item within the <see cref="P:Items"/> collection to select.</param>
        /// <param name="animate">Whether to animate the selection.</param>
        public void SelectItem(object item, Animate animate)
        {
            int index;
            if (selectionMode != SelectionMode.None && items != null && (index = GetPositionForItem(item)) >= 0 && !selectedIndices.Contains(index))
            {
                object[] removedItems = null;
                if (selectionMode == SelectionMode.Single && selectedIndices.Count > 0)
                {
                    for (int i = 0; i < ChildCount; i++)
                    {
                        var child = GetChildAt(i) as ListBoxItem;
                        if (child != null)
                        {
                            child.IsSelected = GetChildAdapterPosition(child) == index;
                        }
                    }

                    removedItems = selectedIndices.Select(i => GetItemAtPosition(i)).ToArray();
                    selectedIndices.Clear();
                }

                selectedIndices.Add(index);

                OnPropertyChanged(Prism.UI.Controls.ListBox.SelectedItemsProperty);
                SelectionChanged(this, new SelectionChangedEventArgs(new object[] { item }, removedItems));
            }
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

            Left = (int)Math.Ceiling(Frame.Left * Device.Current.DisplayScale);
            Top = (int)Math.Ceiling(Frame.Top * Device.Current.DisplayScale);
            Right = (int)Math.Ceiling(Frame.Right * Device.Current.DisplayScale);
            Bottom = (int)Math.Ceiling(Frame.Bottom * Device.Current.DisplayScale);

            base.OnLayout(changed, Left, Top, Right, Bottom);

            ContentSize = new Size(Width, ComputeVerticalScrollRange()) / Device.Current.DisplayScale;
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

        private new object GetItemAtPosition(int position)
        {
            if (!isSectioningEnabled)
            {
                return items[position];
            }

            for (int i = 0; i < items.Count; i++)
            {
                var list = items[i] as IList;
                if (list != null)
                {
                    if (position >= list.Count)
                    {
                        position -= list.Count;
                    }
                    else
                    {
                        return list[position];
                    }
                }
            }

            throw new ArgumentOutOfRangeException(nameof(position));
        }

        private int GetPositionForItem(object item)
        {
            if (!isSectioningEnabled)
            {
                return items.IndexOf(item);
            }

            for (int i = 0; i < items.Count; i++)
            {
                var list = items[i] as IList;
                if (list != null)
                {
                    int index = list.IndexOf(item);
                    if (index >= 0)
                    {
                        return index;
                    }
                }
            }

            return -1;
        }

        private void OnAccessoryClicked(int index)
        {
            AccessoryClicked(this, new AccessoryClickedEventArgs(items?[index]));
        }

        private void OnBackgroundImageLoaded(object sender, EventArgs e)
        {
            base.Background = background.GetDrawable(null);
        }

        private void OnSeparatorImageLoaded(object sender, EventArgs e)
        {
            separatorDrawable = separatorBrush.GetDrawable(null);
            DividerHeight = 1;
        }

        private void OnItemClicked(global::Android.Views.View view, int position)
        {
            if (selectionMode == SelectionMode.None)
            {
                return;
            }

            var item = GetItemAtPosition(position);
            // this check must be done before ItemClicked is fired
            if (selectedIndices.Contains(position))
            {
                ItemClicked(this, new ItemClickedEventArgs(item));
                if (selectionMode == SelectionMode.Multiple)
                {
                    DeselectItem(item, Prism.UI.Animate.Default);
                }
            }
            else
            {
                ItemClicked(this, new ItemClickedEventArgs(item));
                SelectItem(item, Prism.UI.Animate.Default);
            }
        }

        private void OnItemsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action != NotifyCollectionChangedAction.Move)
            {
                if (e.OldItems != null)
                {
                    foreach (var item in e.OldItems.OfType<INotifyCollectionChanged>())
                    {
                        item.CollectionChanged -= OnItemsSubcollectionChanged;
                    }
                }

                if (e.NewItems != null)
                {
                    foreach (var item in e.NewItems.OfType<INotifyCollectionChanged>())
                    {
                        item.CollectionChanged += OnItemsSubcollectionChanged;
                    }
                }
            }

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    for (int i = 0; i < selectedIndices.Count; i++)
                    {
                        int index = selectedIndices[i];
                        if (index >= e.NewStartingIndex)
                        {
                            selectedIndices[i] = index + e.NewItems.Count;
                        }
                    }

                    GetAdapter().NotifyDataSetChanged();
                    break;
                case NotifyCollectionChangedAction.Move:
                    int startIndex = Math.Min(e.OldStartingIndex, e.NewStartingIndex);
                    int movedCount = Math.Abs(e.OldStartingIndex - e.NewStartingIndex) + e.OldItems.Count;
                    for (int i = 0; i < selectedIndices.Count; i++)
                    {
                        int index = selectedIndices[i];
                        if (index >= startIndex && index < startIndex + movedCount)
                        {
                            if (index >= e.OldStartingIndex && index < e.OldStartingIndex + e.OldItems.Count)
                            {
                                selectedIndices[i] = e.NewStartingIndex + (index - e.OldStartingIndex);
                            }
                            else if (e.OldStartingIndex > e.NewStartingIndex)
                            {
                                selectedIndices[i] = index + e.NewItems.Count;
                            }
                            else
                            {
                                selectedIndices[i] = index - e.NewItems.Count;
                            }
                        }
                    }

                    GetAdapter().NotifyDataSetChanged();
                    break;
                case NotifyCollectionChangedAction.Remove:
                    for (int i = selectedIndices.Count - 1; i >= 0; i--)
                    {
                        int index = selectedIndices[i];
                        if (index >= e.OldStartingIndex)
                        {
                            if (index < e.OldStartingIndex + e.OldItems.Count)
                            {
                                selectedIndices.RemoveAt(i);
                            }
                            else
                            {
                                selectedIndices[i] = index - e.OldItems.Count;
                            }
                        }
                    }
                    GetAdapter().NotifyDataSetChanged();
                    break;
                case NotifyCollectionChangedAction.Replace:
                    GetAdapter().NotifyDataSetChanged();
                    break;
                case NotifyCollectionChangedAction.Reset:
                    selectedIndices.Clear();
                    GetAdapter().NotifyDataSetChanged();
                    break;
            }
        }

        private void OnItemsSubcollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (!IsSectioningEnabled)
            {
                return;
            }

            int sectionIndex = items.IndexOf(sender);
            int newStartingIndex = e.NewStartingIndex;
            int oldStartingIndex = e.OldStartingIndex;
            for (int i = 0; i < sectionIndex; i++)
            {
                var list = items[i] as IList;
                if (list != null)
                {
                    newStartingIndex += list.Count;
                    oldStartingIndex += list.Count;
                }
            }

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    for (int i = 0; i < selectedIndices.Count; i++)
                    {
                        int index = selectedIndices[i];
                        if (index >= newStartingIndex)
                        {
                            selectedIndices[i] = index + e.NewItems.Count;
                        }
                    }

                    GetAdapter().NotifyDataSetChanged();
                    break;
                case NotifyCollectionChangedAction.Move:
                    int startIndex = Math.Min(oldStartingIndex, newStartingIndex);
                    int movedCount = Math.Abs(oldStartingIndex - newStartingIndex) + e.OldItems.Count;
                    for (int i = 0; i < selectedIndices.Count; i++)
                    {
                        int index = selectedIndices[i];
                        if (index >= startIndex && index < startIndex + movedCount)
                        {
                            if (index >= oldStartingIndex && index < oldStartingIndex + e.OldItems.Count)
                            {
                                selectedIndices[i] = newStartingIndex + (index - oldStartingIndex);
                            }
                            else if (oldStartingIndex > newStartingIndex)
                            {
                                selectedIndices[i] = index + e.NewItems.Count;
                            }
                            else
                            {
                                selectedIndices[i] = index - e.NewItems.Count;
                            }
                        }
                    }

                    GetAdapter().NotifyDataSetChanged();
                    break;
                case NotifyCollectionChangedAction.Remove:
                    for (int i = selectedIndices.Count - 1; i >= 0; i--)
                    {
                        int index = selectedIndices[i];
                        if (index >= oldStartingIndex)
                        {
                            if (index < oldStartingIndex + e.OldItems.Count)
                            {
                                selectedIndices.RemoveAt(i);
                            }
                            else
                            {
                                selectedIndices[i] = index - e.OldItems.Count;
                            }
                        }
                    }
                    GetAdapter().NotifyDataSetChanged();
                    break;
                case NotifyCollectionChangedAction.Replace:
                    GetAdapter().NotifyDataSetChanged();
                    break;
                case NotifyCollectionChangedAction.Reset:
                    selectedIndices.Clear();
                    GetAdapter().NotifyDataSetChanged();
                    break;
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

        private void OnUnloaded()
        {
            if (IsLoaded)
            {
                IsLoaded = false;
                OnPropertyChanged(Visual.IsLoadedProperty);
                Unloaded(this, EventArgs.Empty);
            }
        }


        private class ListBoxAdapter : BaseAdapter
        {
            public override int Count
            {
                get
                {
                    if (listBox.Items == null || listBox.Items.Count == 0)
                    {
                        return 0;
                    }

                    if (listBox.IsSectioningEnabled)
                    {
                        int count = 0;
                        foreach (var item in listBox.Items)
                        {
                            var list = item as IList;
                            count += list == null ? 0 : list.Count;
                        }

                        return count;
                    }

                    return listBox.Items.Count;
                }
            }

            private readonly Dictionary<string, int> itemIds;
            private readonly ListBox listBox;

            public ListBoxAdapter(ListBox parent)
            {
                itemIds = new Dictionary<string, int>();
                listBox = parent;
            }

            public override int GetItemViewType(int position)
            {
                string idString = listBox.ItemIdRequest(listBox.GetItemAtPosition(position));
                int idInt = 0;
                if (itemIds.TryGetValue(idString, out idInt))
                {
                    return idInt;
                }

                itemIds.Add(idString, itemIds.Count);
                return itemIds.Count - 1;
            }

            public override global::Android.Views.View GetView(int position, global::Android.Views.View convertView, ViewGroup parent)
            {
                var holder = convertView?.Tag as ListBoxViewHolder ?? new ListBoxViewHolder();
                holder.AdapterPosition = position;
                var item = holder.ItemView as ListBoxItem;
                if (item != null)
                {
                    item.IsSelected = false;
                }

                holder.ItemView?.SetOnClickListener(null);

                var obj = listBox.ItemRequest(listBox.GetItemAtPosition(position), holder.ItemView as INativeListBoxItem);
                (obj as IListBoxChild)?.SetParent(listBox);

                var view = obj as global::Android.Views.View;
                if (view != null)
                {
                    view.Layout(0, 0, listBox.Width, view.Bottom);
                    view.SetMinimumHeight(view.Height);
                }

                holder.ItemView = view ?? new TextView(listBox.Context) { Text = obj?.ToString() };
                holder.ItemView.Tag = holder;
                holder.ItemView.SetOnClickListener(holder);

                item = holder.ItemView as ListBoxItem;
                if (item != null)
                {
                    item.IsSelected = listBox.selectedIndices.Contains(position);
                }
                return holder.ItemView;
            }

            public override Java.Lang.Object GetItem(int position)
            {
                return position;
            }

            public override long GetItemId(int position)
            {
                return position;
            }
        }

        internal class ListBoxViewHolder : Java.Lang.Object, IOnClickListener
        {
            public int AdapterPosition { get; set; }
            public global::Android.Views.View ItemView { get; set; }

            public void OnClick(global::Android.Views.View v)
            {
                var parent = v.GetParent<ListBox>();
                if (parent != null && parent.IsHitTestVisible)
                {
                    parent.OnItemClicked(v, AdapterPosition);
                }
            }
        }

        private class ListBoxDividerDrawable : Drawable
        {
            private readonly ListBox listBox;

            public ListBoxDividerDrawable(ListBox parent)
            {
                listBox = parent;
            }

            public override void Draw(global::Android.Graphics.Canvas cValue)
            {
                for (int i = 0; i < listBox.ChildCount - 1; i++)
                {
                    int left, top, right, bottom;
                    int height = (int)Math.Max(1, 0.5 * Device.Current.DisplayScale);

                    var child = listBox.GetChildAt(i);
                    var item = child as INativeListBoxItem;
                    if (item != null)
                    {
                        var indentation = item.SeparatorIndentation * Device.Current.DisplayScale;
                        left = (int)(child.Left + indentation.Left);
                        top = (int)(child.Bottom + indentation.Top - indentation.Bottom - height);
                        right = (int)(child.Right - indentation.Right);
                        bottom = (int)(top + height);
                    }
                    else
                    {
                        left = child.Left;
                        top = (child.Bottom - height);
                        right = child.Right;
                        bottom = child.Bottom;
                    }

                    listBox.separatorDrawable.SetBounds(left, top, right, bottom);
                    listBox.separatorDrawable.Draw(cValue);
                }

                listBox.Invalidate();
            }

            public override int Opacity { get { return listBox.separatorDrawable?.Opacity ?? (int)Format.Opaque; } }

            public override void SetAlpha(int alpha)
            {
                listBox.separatorDrawable?.SetAlpha(alpha);
            }

            public override void SetColorFilter(ColorFilter colorFilter)
            {
                listBox.separatorDrawable?.SetColorFilter(colorFilter);
            }
        }
    }
}