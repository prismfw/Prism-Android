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
using Android.Content;
using Android.Graphics;
using Android.Views;
using Android.Widget;
using Prism.Systems;
using Prism.UI.Media;

namespace Prism.Android.UI.Controls
{
    internal class TabLayout : HorizontalScrollView
    {
        public event EventHandler<TabSelectedEventArgs> TabSelected;

        public Brush SelectionBrush
        {
            get { return selectionBrush; }
            set
            {
                if (value != selectionBrush)
                {
                    (selectionBrush as ImageBrush).ClearImageHandler(OnSelectionBrushLoaded);

                    selectionBrush = value;
                    selectionPaint.SetBrush(selectionBrush, innerLayout.GetChildAt(SelectedTabIndex)?.Width ?? 0, selectionBarHeight, OnSelectionBrushLoaded);
                    Invalidate();
                }
            }
        }
        private Brush selectionBrush;

        public int SelectedTabIndex { get; private set; }

        public int TabCount
        {
            get { return innerLayout.ChildCount; }
        }

        private int currentScrollX;
        private bool isSelecting;
        private readonly LinearLayout innerLayout;
        private readonly int selectionBarHeight = 3;
        private readonly Paint selectionPaint = new Paint();

        public TabLayout(Context context)
            : base(context)
        {
            HorizontalScrollBarEnabled = false;

            base.AddView(innerLayout = new LinearLayout(context), new LayoutParams(LayoutParams.MatchParent, LayoutParams.MatchParent));

            innerLayout.ChildViewAdded += (sender, e) =>
            {
                e.Child.Click -= OnTabSelected;
                e.Child.Click += OnTabSelected;
            };

            innerLayout.ChildViewRemoved += (sender, e) =>
            {
                e.Child.Click -= OnTabSelected;
            };
        }

        public void AddTab(View child)
        {
            innerLayout.AddView(child);
        }

        public void AddTab(View child, int index)
        {
            innerLayout.AddView(child, index);
        }

        public View GetTabAt(int index)
        {
            return innerLayout.GetChildAt(index);
        }

        public void RemoveAllTabs()
        {
            innerLayout.RemoveAllViews();
        }

        public void RemoveTabAt(int index)
        {
            innerLayout.RemoveViewAt(index);
        }

        public void RemoveTab(View view)
        {
            innerLayout.RemoveView(view);
        }

        public void SelectTabAt(int index)
        {
            if (index >= 0 && index < innerLayout.ChildCount)
            {
                OnTabSelected(innerLayout.GetChildAt(index), EventArgs.Empty);
            }
        }

        protected override void OnDraw(Canvas canvas)
        {
            base.OnDraw(canvas);
            isSelecting = false;

            var selectedTab = innerLayout.GetChildAt(SelectedTabIndex);
            var rect = new Rect(selectedTab.Left, (int)Math.Ceiling(selectedTab.Bottom - selectionBarHeight * Device.Current.DisplayScale),
                selectedTab.Right, selectedTab.Bottom);

            OffsetDescendantRectToMyCoords(innerLayout, rect);
            canvas.DrawRect(rect, selectionPaint);
        }

        protected override void OnLayout(bool changed, int left, int top, int right, int bottom)
        {
            // Selection might cause a layout that resets the scroll position.  This is very noticeable to the user, and it
            // isn't even necessary to do, so we skip the potential layout that occurs after selection to avoid visual oddities.
            if (!isSelecting)
            {
                base.OnLayout(changed, left, top, right, bottom);
                ScrollX = Math.Min(Math.Max(0, ComputeHorizontalScrollRange() - ComputeHorizontalScrollExtent()), currentScrollX);

                if (right - left < innerLayout.MeasuredWidth)
                {
                    innerLayout.Layout(0, 0, innerLayout.MeasuredWidth, bottom - top);
                }
                else
                {
                    left = ((right - left) - innerLayout.MeasuredWidth) / 2;
                    innerLayout.Layout(left, 0, left + innerLayout.MeasuredWidth, bottom - top);
                }
            }
        }

        protected override void OnScrollChanged(int l, int t, int oldl, int oldt)
        {
            base.OnScrollChanged(l, t, oldl, oldt);
            if (!IsInLayout)
            {
                currentScrollX = l;
            }
        }

        private void OnSelectionBrushLoaded(object sender, EventArgs e)
        {
            var tab = innerLayout.GetChildAt(SelectedTabIndex);
            selectionPaint.SetBrush(selectionBrush, tab?.Width ?? 0, selectionBarHeight, null);
        }

        private void OnTabSelected(object sender, EventArgs e)
        {
            var tab = sender as View;

            int index = innerLayout.IndexOfChild(tab);
            if (index >= 0)
            {
                int oldIndex = SelectedTabIndex;
                SelectedTabIndex = index;

                isSelecting = oldIndex != index;

                Invalidate();
                TabSelected?.Invoke(this, new TabSelectedEventArgs(oldIndex < 0 ? null : innerLayout.GetChildAt(oldIndex), tab));
            }
        }

        public class TabSelectedEventArgs : EventArgs
        {
            public object NewTab { get; }

            public object OldTab { get; }

            public TabSelectedEventArgs(object oldTab, object newTab)
            {
                NewTab = newTab;
                OldTab = oldTab;
            }
        }
    }
}

