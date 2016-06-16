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
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Runtime;
using Android.Support.V7.Widget;
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
    public class ListBox : RecyclerView, INativeListBox
    {
        /// <summary>
        /// Occurs when an accessory in a list box item is selected.
        /// </summary>
        public event EventHandler<AccessorySelectedEventArgs> AccessorySelected;

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
                    if (value != background)
                {
                    (background as ImageBrush).ClearImageHandler(OnBackgroundImageLoaded);

                    background = value;
                    base.Background = background.GetDrawable(OnBackgroundImageLoaded);
                        OnPropertyChanged(Prism.UI.Controls.ListBox.BackgroundProperty);
                }
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
            get { return selectedIndices.Count == 0 ? null : selectedIndices.Select(i => GetItemAtPosition(i)).ToList().AsReadOnly(); }
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
                        ResourceExtractor.GetDrawable(global::Android.Resource.Attribute.TextColorPrimary);

                    OnPropertyChanged(Prism.UI.Controls.ListBox.SeparatorBrushProperty);
                    InvalidateItemDecorations();
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

        /// <summary>
        /// Initializes a new instance of the <see cref="ListBox"/> class.
        /// </summary>
        public ListBox(ListBoxStyle style)
            : base(Application.MainActivity)
        {
            selectedIndices = new List<int>();
            separatorDrawable = ResourceExtractor.GetDrawable(global::Android.Resource.Attribute.TextColorPrimary);
            DescendantFocusability = DescendantFocusability.BeforeDescendants;

            AddItemDecoration(new ListBoxItemDecoration(this));
            SetAdapter(new ListBoxAdapter(this));
            SetLayoutManager(new LinearLayoutManager(Context));
        }

        /// <summary>
        /// Deselects the specified item.
        /// </summary>
        /// <param name="item">The item within the <see cref="P:Items"/> collection to deselect.</param>
        /// <param name="animate">Whether to animate the deselection.</param>
        public void DeselectItem(object item, Animate animate)
        {
            int index;
            if (items == null || (index = GetPositionForItem(item)) < 0)
            {
                return;
            }

            if (selectedIndices.Remove(index))
            {
                for (int i = 0; i < ChildCount; i++)
                {
                    var child = GetChildAt(i) as ListBoxItem;
                    if (child != null && GetChildAdapterPosition(child) == index)
                    {
                        child.IsSelected = false;
                    }
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

            base.OnMeasure(MeasureSpec.MakeMeasureSpec((int)(constraints.Width * Device.Current.DisplayScale), MeasureSpecMode.AtMost),
                MeasureSpec.MakeMeasureSpec((int)(constraints.Height * Device.Current.DisplayScale), MeasureSpecMode.AtMost));

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
            if (!isHitTestVisible)
            {
                return true;
            }
        
            return base.OnInterceptTouchEvent(ev);
        }
        
        /// <summary></summary>
        /// <param name="e"></param>
        public override bool OnTouchEvent(MotionEvent e)
        {
            if (!isHitTestVisible)
            {
                return false;
            }
            
            for (int i = 0; i < ChildCount; i++)
            {
                var child = GetChildAt(0);
                if (child != null && ((child as INativeElement)?.IsHitTestVisible ?? false))
                {
                    var rect = new Rect();
                    child.GetHitRect(rect);
                    if (rect.Contains((int)e.GetX(), (int)e.GetY()))
                    {
                        return base.OnTouchEvent(e);
                    }
                }
            }
            
            if (e.Action == MotionEventActions.Cancel)
            {
                PointerCanceled(this, e.GetPointerEventArgs(this));
            }
            if (e.Action == MotionEventActions.Down)
            {
                PointerPressed(this, e.GetPointerEventArgs(this));
            }
            if (e.Action == MotionEventActions.Move)
            {
                PointerMoved(this, e.GetPointerEventArgs(this));
            }
            if (e.Action == MotionEventActions.Up)
            {
                PointerReleased(this, e.GetPointerEventArgs(this));
            }
            return base.OnTouchEvent(e);
        }

        /// <summary></summary>
        /// <param name="dx"></param>
        /// <param name="dy"></param>
        public override void OnScrolled(int dx, int dy)
        {
            base.OnScrolled(dx, dy);
            ContentOffset = new Point(ComputeHorizontalScrollOffset() / Device.Current.DisplayScale, ComputeVerticalScrollOffset() / Device.Current.DisplayScale);
        }

        /// <summary>
        /// Forces a reload of the list box's contents.
        /// </summary>
        public void Reload()
        {
            GetAdapter()?.NotifyDataSetChanged();
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
                ScrollToPosition(index);
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
            if (selectionMode == SelectionMode.None || items == null || (index = GetPositionForItem(item)) < 0)
            {
                return;
            }

            if (!selectedIndices.Contains(index))
            {
                if (selectionMode == SelectionMode.Single)
                {
                    selectedIndices.Clear();
                }
                
                selectedIndices.Add(index);
                for (int i = 0; i < ChildCount; i++)
                {
                    var child = GetChildAt(i) as ListBoxItem;
                    if (child != null)
                    {
                        child.IsSelected = selectedIndices.Contains(GetChildAdapterPosition(child));
                    }
                }
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
            MeasureRequest(false, null);
            ArrangeRequest(false, null);
            base.OnLayout(changed, Left, Top, Right, Bottom);

            ContentSize = new Size(Width, ComputeVerticalScrollRange()) / Device.Current.DisplayScale;
        }

        /// <summary>
        /// Called when a property value is changed.
        /// </summary>
        /// <param name="pd">A property descriptor describing the property whose value has been changed.</param>
        protected virtual void OnPropertyChanged(PropertyDescriptor pd)
        {
            PropertyChanged(this, new FrameworkPropertyChangedEventArgs(pd));
        }

        private object GetItemAtPosition(int position)
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

        private void OnAccessorySelected(int index)
        {
            AccessorySelected(this, new AccessorySelectedEventArgs(items?[index]));
        }

        private void OnBackgroundImageLoaded(object sender, EventArgs e)
        {
            base.Background = background.GetDrawable(null);
        }

        private void OnSeparatorImageLoaded(object sender, EventArgs e)
        {
            separatorDrawable = separatorBrush.GetDrawable(null);
            InvalidateItemDecorations();
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

                    GetAdapter().NotifyItemRangeInserted(e.NewStartingIndex, e.NewItems.Count);
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

                    var adapter = GetAdapter();
                    for (int i = 0; i < e.NewItems.Count; i++)
                    {
                        adapter.NotifyItemMoved(e.OldStartingIndex + i, e.NewStartingIndex + i);
                    }
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
                    GetAdapter().NotifyItemRangeRemoved(e.OldStartingIndex, e.OldItems.Count);
                    break;
                case NotifyCollectionChangedAction.Replace:
                    GetAdapter().NotifyItemRangeChanged(e.OldStartingIndex, e.OldItems.Count);
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

                    GetAdapter().NotifyItemRangeInserted(newStartingIndex, e.NewItems.Count);
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

                    var adapter = GetAdapter();
                    for (int i = 0; i < e.NewItems.Count; i++)
                    {
                        adapter.NotifyItemMoved(oldStartingIndex + i, newStartingIndex + i);
                    }
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
                    GetAdapter().NotifyItemRangeRemoved(oldStartingIndex, e.OldItems.Count);
                    break;
                case NotifyCollectionChangedAction.Replace:
                    GetAdapter().NotifyItemRangeChanged(oldStartingIndex, e.OldItems.Count);
                    break;
                case NotifyCollectionChangedAction.Reset:
                    selectedIndices.Clear();
                    GetAdapter().NotifyDataSetChanged();
                    break;
            }
        }

        private void OnItemSelected(global::Android.Views.View view, int position)
        {
            if (selectionMode == SelectionMode.None)
            {
                return;
            }

            var listBoxItem = view as ListBoxItem;
            if (selectedIndices.Contains(position))
            {
                if (selectionMode == SelectionMode.Multiple)
                {
                    if (listBoxItem != null)
                    {
                        listBoxItem.IsSelected = false;
                    }

                    selectedIndices.Remove(position);
                    OnPropertyChanged(Prism.UI.Controls.ListBox.SelectedItemsProperty);
                    SelectionChanged(this, new SelectionChangedEventArgs(null, GetItemAtPosition(position)));
                }
            }
            else
            {
                object[] removedItems = null;
                if (selectionMode == SelectionMode.Single && selectedIndices.Count > 0)
                {
                    for (int i = 0; i < ChildCount; i++)
                    {
                        var child = GetChildAt(i) as ListBoxItem;
                        if (child != null)
                        {
                            child.IsSelected = false;
                        }
                    }

                    removedItems = selectedIndices.Select(i => GetItemAtPosition(i)).ToArray();
                    selectedIndices.Clear();
                }

                if (listBoxItem != null)
                {
                    listBoxItem.IsSelected = true;
                }

                selectedIndices.Add(position);
                OnPropertyChanged(Prism.UI.Controls.ListBox.SelectedItemsProperty);
                SelectionChanged(this, new SelectionChangedEventArgs(new object[] { GetItemAtPosition(position) }, removedItems));
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

        private class ListBoxAdapter : Adapter
        {
            public override int ItemCount
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

            public override void OnBindViewHolder(ViewHolder holder, int position)
            {
                var item = holder.ItemView as ListBoxItem;
                if (item != null)
                {
                    item.IsSelected = false;
                }

                holder.ItemView?.SetOnClickListener(null);

                var obj = listBox.ItemRequest(listBox.GetItemAtPosition(position), holder.ItemView as INativeListBoxItem);
                (obj as IRecyclerViewChild)?.SetParent(listBox);

                var view = obj as global::Android.Views.View;
                if (view != null)
                {
                    view.Layout(0, 0, listBox.Width, view.Bottom);
                    view.SetMinimumHeight(view.Height);
                }

                holder.ItemView = view ?? new TextView(listBox.Context) { Text = obj?.ToString() };
                holder.ItemView.SetOnClickListener(holder as IOnClickListener);

                item = holder.ItemView as ListBoxItem;
                if (item != null)
                {
                    item.IsSelected = listBox.selectedIndices.Contains(position);
                }
            }

            public override ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
            {
                return new ListBoxViewHolder(listBox.Context);
            }
        }

        private class ListBoxViewHolder : ViewHolder, IOnClickListener
        {
            public ListBoxViewHolder(Context context)
                : base(new global::Android.Views.View(context))
            {
            }

            public void OnClick(global::Android.Views.View v)
            {
                var parent = v.GetParent<ListBox>();
                if (parent != null && parent.IsHitTestVisible)
                {
                    parent.OnItemSelected(v, AdapterPosition);
                }
            }
        }

        private class ListBoxItemDecoration : ItemDecoration
        {
            private readonly ListBox listBox;

            public ListBoxItemDecoration(ListBox parent)
            {
                listBox = parent;
            }

            public override void OnDrawOver(global::Android.Graphics.Canvas cValue, RecyclerView parent, State state)
            {
                base.OnDrawOver(cValue, parent, state);

                for (int i = 0; i < parent.ChildCount - 1; i++)
                {
                    int left, top, right, bottom;
                    int height = (int)Math.Max(1, 0.5 * Device.Current.DisplayScale);

                    var child = parent.GetChildAt(i);
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
                        top = (int)(child.Bottom - height);
                        right = child.Right;
                        bottom = child.Bottom;
                    }

                    listBox.separatorDrawable.SetBounds(left, top, right, bottom);
                    listBox.separatorDrawable.Draw(cValue);
                }

                parent.Invalidate();
            }
        }
    }
}