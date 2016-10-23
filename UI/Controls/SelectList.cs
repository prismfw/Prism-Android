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
    /// Represents an Android implementation of an <see cref="INativeSelectList"/>.
    /// </summary>
    [Preserve(AllMembers = true)]
    [Register(typeof(INativeSelectList))]
    public class SelectList : Spinner, INativeSelectList, AdapterView.IOnItemSelectedListener
    {
        /// <summary>
        /// Occurs when the control receives focus.
        /// </summary>
        public event EventHandler GotFocus;

        /// <summary>
        /// Occurs when this instance has been attached to the visual tree and is ready to be rendered.
        /// </summary>
        public event EventHandler Loaded;

        /// <summary>
        /// Occurs when the control loses focus.
        /// </summary>
        public event EventHandler LostFocus;
        
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
        /// Occurs when the selection of the select list is changed.
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
        /// Gets or sets the background for the control.
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
                    OnPropertyChanged(Control.BackgroundProperty);
                }
            }
        }
        private Brush background;

        /// <summary>
        /// Gets or sets the <see cref="Brush"/> to apply to the border of the control.
        /// </summary>
        public Brush BorderBrush
        {
            get { return borderBrush; }
            set
            {
                if (value != borderBrush)
                {
                    (borderBrush as ImageBrush).ClearImageHandler(OnBorderImageLoaded);

                    borderBrush = value;
                    borderPaint.SetBrush(borderBrush, Width, Height, OnBorderImageLoaded);
                    OnPropertyChanged(Control.BorderBrushProperty);
                    Invalidate();
                }
            }
        }
        private Brush borderBrush;

        /// <summary>
        /// Gets or sets the width of the border around the control.
        /// </summary>
        public double BorderWidth
        {
            get { return borderWidth; }
            set
            {
                if (value != borderWidth)
                {
                    borderWidth = value;
                    SetPadding((int)borderWidth, (int)borderWidth, (int)borderWidth, 7);
                    OnPropertyChanged(Control.BorderWidthProperty);
                    Invalidate();
                }
            }
        }
        private double borderWidth;

        /// <summary>
        /// Gets or sets the method to invoke when this instance requests a display item for the select list.
        /// </summary>
        public SelectListDisplayItemRequestHandler DisplayItemRequest { get; set; }

        /// <summary>
        /// Gets or sets the font to use for displaying the text in the control.
        /// </summary>
        public object FontFamily
        {
            get { return fontFamily; }
            set
            {
                if (value != fontFamily)
                {
                    fontFamily = value as Media.FontFamily;
                    OnPropertyChanged(Control.FontFamilyProperty);
                }
            }
        }
        private Media.FontFamily fontFamily;

        /// <summary>
        /// Gets or sets the size of the text in the control.
        /// </summary>
        public double FontSize
        {
            get { return fontSize; }
            set
            {
                if (value != fontSize)
                {
                    fontSize = value;
                    OnPropertyChanged(Control.FontSizeProperty);
                }
            }
        }
        private double fontSize;

        /// <summary>
        /// Gets or sets the style with which to render the text in the control.
        /// </summary>
        public FontStyle FontStyle
        {
            get { return fontStyle; }
            set
            {
                if (value != fontStyle)
                {
                    fontStyle = value;
                    OnPropertyChanged(Control.FontStyleProperty);
                }
            }
        }
        private FontStyle fontStyle;

        /// <summary>
        /// Gets or sets the <see cref="Brush"/> to apply to the foreground content of the control.
        /// </summary>
        public new Brush Foreground
        {
            get { return foreground; }
            set
            {
                if (value != foreground)
                {
                    var displayObj = this.GetChild<INativeVisual>();
                    var panel = displayObj as INativePanel;
                    if (panel != null)
                    {
                        foreach (var child in panel.Children)
                        {
                            var label = child as INativeLabel;
                            if (label != null && (label.Foreground == null || label.Foreground == foreground))
                            {
                                label.Foreground = value;
                            }
                            else
                            {
                                var control = child as INativeControl;
                                if (control != null && (control.Foreground == null || control.Foreground == foreground))
                                {
                                    control.Foreground = value;
                                }
                            }
                        }
                    }
                    else
                    {
                        var label = displayObj as INativeLabel;
                        if (label != null && (label.Foreground == null || label.Foreground == foreground))
                        {
                            label.Foreground = value;
                        }
                        else
                        {
                            var control = displayObj as INativeControl;
                            if (control != null && (control.Foreground == null || control.Foreground == foreground))
                            {
                                control.Foreground = value;
                            }
                        }
                    }
                
                    foreground = value;
                    var scb = foreground as SolidColorBrush;
                    if (scb == null)
                    {
                        foregroundDrawable.ClearColorFilter();
                    }
                    else
                    {
                        foregroundDrawable.SetColorFilter(scb.Color.GetColor(), PorterDuff.Mode.SrcIn);
                    }
                    
                    OnPropertyChanged(Control.ForegroundProperty);
                    Invalidate();
                }
            }
        }
        private Brush foreground;

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
        /// Gets or sets a value indicating whether the user can interact with the control.
        /// </summary>
        public bool IsEnabled
        {
            get { return Enabled; }
            set
            {
                if (value != Enabled)
                {
                    Enabled = value;
                    OnPropertyChanged(Control.IsEnabledProperty);
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
        /// Gets or sets a value indicating whether the picker is open.
        /// </summary>
        public bool IsOpen
        {
            get { return isOpen; }
            set
            {
                if (value != isOpen)
                {
                    if (value)
                    {
                        PerformClick();
                    }
                    else
                    {
                        base.OnDetachedFromWindow();
                    }
                }
            }
        }
        private bool isOpen;

        /// <summary>
        /// Gets or sets a list of the items that make up the selection list.
        /// </summary>
        public IList Items
        {
            get { return items; }
            set
            {
                if (value != items)
                {
                    items = value;
                    OnPropertyChanged(Prism.UI.Controls.SelectList.ItemsProperty);
                    Adapter = new SelectListAdapter(this);
                }
            }
        }
        private IList items;

        /// <summary>
        /// Gets or sets the method to invoke when this instance requests a list item for an object in the select list.
        /// </summary>
        public SelectListListItemRequestHandler ListItemRequest { get; set; }

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
        /// Gets or sets the zero-based index of the selected item.
        /// </summary>
        public int SelectedIndex
        {
            get { return base.SelectedItemPosition; }
            set { SetSelection(value, areAnimationsEnabled); }
        }

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

        private readonly Drawable foregroundDrawable;
        private readonly Paint borderPaint = new Paint();
        private int currentIndex;

        /// <summary>
        /// Initializes a new instance of the <see cref="SelectList"/> class.
        /// </summary>
        public SelectList()
            : base(Application.MainActivity, SpinnerMode.Dropdown)
        {
            foregroundDrawable = base.Background;
            base.Background = null;
        
            Focusable = true;
            OnItemSelectedListener = this;
            SetPadding(0, 0, 0, 8);
            SetWillNotDraw(false);

            ChildViewAdded += (sender, e) =>
            {
                e.Child.LayoutParameters.Width = ViewGroup.LayoutParams.MatchParent;
                e.Child.LayoutParameters.Height = ViewGroup.LayoutParams.MatchParent;
                Measure(MeasureSpec.MakeMeasureSpec(e.Parent.Width, MeasureSpecMode.Exactly), MeasureSpec.MakeMeasureSpec(e.Parent.Height, MeasureSpecMode.Exactly));
            };

            ChildViewRemoved += (sender, e) =>
            {
                RequestLayout();
            };
        }

        /// <summary></summary>
        /// <param name="e"></param>
        public override bool DispatchTouchEvent(MotionEvent e)
        {
            var parent = Parent as ITouchDispatcher;
            if (parent == null || parent.IsDispatching)
            {
                return base.DispatchTouchEvent(e);
            }

            return false;
        }

        /// <summary>
        /// Attempts to set focus to the control.
        /// </summary>
        public void Focus()
        {
            if (!IsFocused && !RequestFocus())
            {
                RequestFocusFromTouch();
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
            return constraints;
        }

        /// <summary></summary>
        /// <param name="parent"></param>
        /// <param name="view"></param>
        /// <param name="position"></param>
        /// <param name="id"></param>
        public void OnItemSelected(AdapterView parent, global::Android.Views.View view, int position, long id)
        {
            if (currentIndex != position)
            {
                OnPropertyChanged(Prism.UI.Controls.SelectList.SelectedIndexProperty);
                SelectionChanged(this, new SelectionChangedEventArgs(items[position], items[currentIndex]));
                currentIndex = position;
            }
        }

        /// <summary></summary>
        /// <param name="parent"></param>
        public void OnNothingSelected(AdapterView parent)
        {
        }
        
        /// <summary></summary>
        /// <param name="e"></param>
        public override bool OnTouchEvent(MotionEvent e)
        {
            if (!isHitTestVisible)
            {
                return false;
            }
            
            if (e.Action == MotionEventActions.Cancel)
            {
                PointerCanceled(this, e.GetPointerEventArgs(this));
                base.OnTouchEvent(e);
                return true;
            }
            if (e.Action == MotionEventActions.Down)
            {
                PointerPressed(this, e.GetPointerEventArgs(this));
                base.OnTouchEvent(e);
                return true;
            }
            if (e.Action == MotionEventActions.Move)
            {
                PointerMoved(this, e.GetPointerEventArgs(this));
                base.OnTouchEvent(e);
                return true;
            }
            if (e.Action == MotionEventActions.Up)
            {
                PointerReleased(this, e.GetPointerEventArgs(this));
                base.OnTouchEvent(e);
                return true;
            }
            return base.OnTouchEvent(e);
        }

        /// <summary>
        /// Called when the window containing this view gains or loses focus.
        /// </summary>
        /// <param name="hasWindowFocus">True if the window containing this view now has focus, false otherwise.</param>
        public override void OnWindowFocusChanged(bool hasWindowFocus)
        {
            base.OnWindowFocusChanged(hasWindowFocus);
            if (isOpen && hasWindowFocus)
            {
                isOpen = false;
                OnPropertyChanged(Prism.UI.Controls.SelectList.IsOpenProperty);
            }
        }

        /// <summary>
        /// Call this view's OnClickListener, if it is defined.
        /// </summary>
        public override bool PerformClick()
        {
            if (!isOpen)
            {
                isOpen = true;
                OnPropertyChanged(Prism.UI.Controls.SelectList.IsOpenProperty);
            }

            return base.Enabled && base.PerformClick();
        }

        /// <summary>
        /// Forces a refresh of the display item.
        /// </summary>
        public void RefreshDisplayItem()
        {
            int index = SelectedItemPosition;
            SetSelection(index == 0 ? 1 : 0, false);
            SetSelection(index, false);
        }

        /// <summary>
        /// Forces a refresh of the items in the selection list.
        /// </summary>
        public void RefreshListItems()
        {
            (Adapter as ArrayAdapter)?.NotifyDataSetChanged();
        }

        /// <summary>
        /// Attempts to remove focus from the control.
        /// </summary>
        public void Unfocus()
        {
            ClearFocus();
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

        /// <summary></summary>
        /// <param name="canvas"></param>
        protected override void DispatchDraw(global::Android.Graphics.Canvas canvas)
        {
            base.DispatchDraw(canvas);
            
            if (foregroundDrawable != null)
            {
                foregroundDrawable.SetBounds(0, 0, Width, Height);
                foregroundDrawable.Draw(canvas);
            }

            if (borderBrush != null && borderWidth > 0)
            {
                borderPaint.StrokeWidth = (float)(borderWidth * Device.Current.DisplayScale);
                canvas.DrawLines(new float[] { 0, Height, 0, 0, 0, 0, Width, 0 }, borderPaint);

                // the right and bottom borders seem to be drawn thinner than the left and top ones
                borderPaint.StrokeWidth++;
                canvas.DrawLines(new float[] { Width, 0, Width, Height, Width, Height, 0, Height }, borderPaint);
            }
        }

        /// <summary>
        /// Called by the view system when the focus state of this view changes.
        /// </summary>
        /// <param name="gainFocus">True if the View has focus; false otherwise.</param>
        /// <param name="direction"></param>
        /// <param name="previouslyFocusedRect"></param>
        protected override void OnFocusChanged(bool gainFocus, FocusSearchDirection direction, Rect previouslyFocusedRect)
        {
            base.OnFocusChanged(gainFocus, direction, previouslyFocusedRect);

            OnPropertyChanged(Prism.UI.Controls.Control.IsFocusedProperty);
            if (gainFocus)
            {
                GotFocus(this, EventArgs.Empty);
            }
            else
            {
                LostFocus(this, EventArgs.Empty);
            }
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
            // SelectLists have a problem with their parent not rearranging them on item selection, so we're forcing a rearrangement for now
            var parent = (ObjectRetriever.GetAgnosticObject(this) as Visual)?.Parent as Visual;
            if (parent != null && (!parent.IsMeasureValid || !parent.IsArrangeValid))
            {
                (ObjectRetriever.GetNativeObject(parent) as INativeVisual)?.ArrangeRequest(true, null);
            }
            else
            {
                MeasureRequest(false, null);
                ArrangeRequest(false, null);
            }
            base.OnLayout(changed, Left, Top, Right, Bottom);
        }

        /// <summary>
        /// Called when a property value is changed.
        /// </summary>
        /// <param name="pd">A property descriptor describing the property whose value has been changed.</param>
        protected virtual void OnPropertyChanged(PropertyDescriptor pd)
        {
            PropertyChanged(this, new FrameworkPropertyChangedEventArgs(pd));
        }

        /// <summary>
        /// This is called during layout when the size of this view has changed.
        /// </summary>
        /// <param name="w">Current width of this view.</param>
        /// <param name="h">Current height of this view.</param>
        /// <param name="oldw">Old width of this view.</param>
        /// <param name="oldh">Old height of this view.</param>
        protected override void OnSizeChanged(int w, int h, int oldw, int oldh)
        {
            base.OnSizeChanged(w, h, oldw, oldh);
            borderPaint.SetBrush(borderBrush, w, h, null);
        }

        private void OnBackgroundImageLoaded(object sender, EventArgs e)
        {
            base.Background = background.GetDrawable(null);
        }

        private void OnBorderImageLoaded(object sender, EventArgs e)
        {
            Invalidate();
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

        private class SelectListAdapter : ArrayAdapter
        {
            private readonly WeakReference selectList;

            public SelectListAdapter(SelectList parent)
                : base(parent.Context, global::Android.Resource.Layout.SimpleSpinnerItem, parent.Items)
            {
                selectList = new WeakReference(parent);
                SetDropDownViewResource(global::Android.Resource.Layout.SimpleSpinnerDropDownItem);
            }

            public override global::Android.Views.View GetView(int position, global::Android.Views.View convertView, ViewGroup parent)
            {
                var sl = selectList.Target as SelectList;
                var obj = sl?.DisplayItemRequest();
                return obj as global::Android.Views.View ?? new TextView(Context) { Text = obj?.ToString() };
            }

            public override global::Android.Views.View GetDropDownView(int position, global::Android.Views.View convertView, ViewGroup parent)
            {
                var sl = selectList.Target as SelectList;
                var obj = sl?.ListItemRequest(sl.Items[position]);
                var view = obj as global::Android.Views.View ?? new TextView(Context) { Text = obj?.ToString() };
                var visual = obj as INativeVisual;
                if (visual != null)
                {
                    visual.MeasureRequest(false, null);
                    visual.ArrangeRequest(false, null);

                    view.SetMinimumHeight(view.Height);
                    view.SetMinimumWidth(view.Width);
                }

                return view;
            }
        }
    }
}

