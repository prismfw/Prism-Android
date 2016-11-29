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
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Runtime;
using Android.Text;
using Android.Text.Style;
using Android.Views;
using Android.Widget;
using Prism.Native;
using Prism.Systems;
using Prism.UI.Media;
using Prism.UI.Media.Imaging;

namespace Prism.Android.UI.Controls
{
    /// <summary>
    /// Represents an Android implementation of an <see cref="INativeActionMenu"/>.
    /// </summary>
    [Preserve(AllMembers = true)]
    [Register(typeof(INativeActionMenu))]
    public class ActionMenu : INativeActionMenu
    {
        /// <summary>
        /// Occurs when this instance has been attached to the visual tree and is ready to be rendered.
        /// </summary>
        public event EventHandler Loaded;
        
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
                    OnPropertyChanged(Prism.UI.Visual.AreAnimationsEnabledProperty);
                }
            }
        }
        private bool areAnimationsEnabled;

        /// <summary>
        /// Gets or sets the method to invoke when this instance requests an arrangement of its children.
        /// </summary>
        public ArrangeRequestHandler ArrangeRequest { get; set; }
        
        /// <summary>
        /// Gets or sets the background for the menu.
        /// </summary>
        public Brush Background
        {
            get { return background; }
            set
            {
                if (value != background)
                {
                    background = value;
                    OnPropertyChanged(Prism.UI.Controls.ActionMenu.BackgroundProperty);
                }
            }
        }
        private Brush background;
        
        /// <summary>
        /// Gets or sets the title of the menu's Cancel button, if one exists.
        /// </summary>
        public string CancelButtonTitle
        {
            get { return cancelButtonTitle; }
            set
            {
                if (value != cancelButtonTitle)
                {
                    cancelButtonTitle = value;
                    OnPropertyChanged(Prism.UI.Controls.ActionMenu.CancelButtonTitleProperty);
                }
            }
        }
        private string cancelButtonTitle;
        
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
                    
                    var scb = foreground as SolidColorBrush;
                    if (scb == null)
                    {
                        OverflowButton.SetColorFilter(ResourceExtractor.GetColor(global::Android.Resource.Attribute.TextColorPrimary));
                    }
                    else
                    {
                        OverflowButton.SetColorFilter(scb.Color.GetColor());
                    }
                    
                    foreach (var item in Items.OfType<INativeMenuItem>())
                    {
                        SetItemForeground(item);
                    }
                    
                    OnPropertyChanged(Prism.UI.Controls.ActionMenu.ForegroundProperty);
                }
            }
        }
        private Brush foreground;
        
        /// <summary>
        /// Gets or sets a <see cref="Rectangle"/> that represents the size and position of the element relative to its parent container.
        /// </summary>
        public Rectangle Frame { get; set; }

        /// <summary>
        /// Gets the amount that the menu is inset on top of its parent view.
        /// </summary>
        public Thickness Insets
        {
            get { return new Thickness(); }
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
                    
                    OverflowButton.Clickable = isHitTestVisible;
                    foreach (var button in Items.OfType<View>())
                    {
                        button.Clickable = isHitTestVisible;
                    }

                    OnPropertyChanged(Prism.UI.Visual.IsHitTestVisibleProperty);
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
        /// Gets or sets the maximum number of menu items that can be displayed before they are placed into an overflow menu.
        /// </summary>
        public int MaxDisplayItems
        {
            get { return maxDisplayItems; }
            set
            {
                if (value != maxDisplayItems)
                {
                    int oldValue = maxDisplayItems;
                    maxDisplayItems = value;
                    if (Parent != null && (oldValue < Items.Count || maxDisplayItems < Items.Count))
                    {
                        SetButtons();
                    }
                    
                    OnPropertyChanged(Prism.UI.Controls.ActionMenu.MaxDisplayItemsProperty);
                }
            }
        }
        private int maxDisplayItems;
        
        /// <summary>
        /// Gets or sets the method to invoke when this instance requests a measurement of itself and its children.
        /// </summary>
        public MeasureRequestHandler MeasureRequest { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Uri"/> of the image to use for representing the overflow menu when one is present.
        /// </summary>
        public Uri OverflowImageUri
        {
            get { return overflowImageUri; }
            set
            {
                if (value != overflowImageUri)
                {
                    overflowImageUri = value;
                    if (overflowImageUri == null)
                    {
                        OverflowButton.SetImageDrawable(null);
                    }
                    else
                    {
                        var source = (INativeImageSource)ObjectRetriever.GetNativeObject(new BitmapImage(overflowImageUri));
                        OverflowButton.SetImageBitmap(source.BeginLoadingImage(OnOverflowImageLoaded));
                    }
                    
                    OnPropertyChanged(Prism.UI.Controls.ActionMenu.OverflowImageUriProperty);
                }
            }
        }
        private Uri overflowImageUri;
        
        /// <summary>
        /// Gets the parent object for this instance.
        /// </summary>
        public IViewParent Parent { get; private set; }
        
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
                    var transform = renderTransform as Media.Transform;
                    if (transform != null)
                    {
                        transform.RemoveView(OverflowButton);
                        foreach (var button in Items.OfType<View>())
                        {
                            transform.RemoveView(button);
                        }
                    }
                    
                    renderTransform = value;
                    
                    transform = renderTransform as Media.Transform;
                    if (transform != null)
                    {
                        transform.AddView(OverflowButton);
                        foreach (var button in Items.OfType<View>())
                        {
                            transform.AddView(button);
                        }
                    }

                    OnPropertyChanged(Prism.UI.Visual.RenderTransformProperty);
                }
            }
        }
        private INativeTransform renderTransform;

        /// <summary>
        /// Gets or sets the visual theme that should be used by this instance.
        /// </summary>
        public Prism.UI.Theme RequestedTheme { get; set; }
        
        internal ImageView OverflowButton { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ActionMenu"/> class.
        /// </summary>
        public ActionMenu()
        {
            OverflowButton = new ActionMenuOverflowButton(Application.MainActivity);
            OverflowButton.Click += OnOverflowButtonClicked;
            
            Items = new ObservableCollection<INativeMenuItem>();
            ((ObservableCollection<INativeMenuItem>)Items).CollectionChanged += (o, e) =>
            {
                if (e.OldItems != null)
                {
                    foreach (var item in e.OldItems.OfType<INativeMenuItem>())
                    {
                        item.PropertyChanged -= OnItemPropertyChanged;
                    }
                }
            
                if (e.NewItems != null)
                {
                    foreach (var item in e.NewItems.OfType<INativeMenuItem>())
                    {
                        item.PropertyChanged -= OnItemPropertyChanged;
                        item.PropertyChanged += OnItemPropertyChanged;
                        
                        SetItemForeground(item);
                    }
                }
            
                if (Parent == null)
                {
                    return;
                }
            
                switch (e.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                        if (e.NewStartingIndex < maxDisplayItems || Items.Count > maxDisplayItems && (Items.Count - e.NewItems.Count <= maxDisplayItems))
                        {
                            SetButtons();
                        }
                        break;
                    case NotifyCollectionChangedAction.Move:
                        if (e.NewStartingIndex < maxDisplayItems || e.OldStartingIndex < maxDisplayItems)
                        {
                            SetButtons();
                        }
                        break;
                    case NotifyCollectionChangedAction.Remove:
                        if (e.OldStartingIndex < maxDisplayItems)
                        {
                            SetButtons();
                        }
                        break;
                    case NotifyCollectionChangedAction.Replace:
                        if (e.NewStartingIndex < maxDisplayItems)
                        {
                            SetButtons();
                        }
                        break;
                    case NotifyCollectionChangedAction.Reset:
                        SetButtons(null);
                        break;
                }
            };
        }
        
        /// <summary>
        /// Attaches the menu to the specified parent.
        /// </summary>
        public void Attach(IViewParent parent)
        {
            if (Parent != null && parent != null && Parent != parent)
            {
                throw new InvalidOperationException("Menu instance is already assigned to another object.");
            }
        
            Parent = parent;
            if (Parent == null)
            {
                Detach();
            }
            else
            {
                SetButtons();
                if ((parent as INativeVisual)?.IsLoaded ?? (parent.Parent != null))
                {
                    OnLoaded();
                }
            }
        }
        
        /// <summary>
        /// Detaches the menu from its current parent.
        /// </summary>
        public void Detach()
        {
            SetButtons(null);
            Parent = null;
            
            OnUnloaded();
        }
        
        /// <summary>
        /// Invalidates the arrangement of this instance's children.
        /// </summary>
        public void InvalidateArrange()
        {
            ArrangeRequest(false, null);
        }

        /// <summary>
        /// Invalidates the measurement of this instance and its children.
        /// </summary>
        public void InvalidateMeasure()
        {
            MeasureRequest(false, null);
        }
        
        /// <summary>
        /// Measures the element and returns its desired size.
        /// </summary>
        /// <param name="constraints">The width and height that the element is not allowed to exceed.</param>
        public Size Measure(Size constraints)
        {
            var viewGroup = Parent as ViewGroup;
            if (viewGroup == null)
            {
                return Size.Empty;
            }
            
            var size = new Size();
            for (int i = 0; i < viewGroup.ChildCount; i++)
            {
                var child = viewGroup.GetChildAt(i);
                if (child == OverflowButton || Items.Contains(child))
                {
                    child.Measure(View.MeasureSpec.MakeMeasureSpec(-1, MeasureSpecMode.Unspecified),
                        View.MeasureSpec.MakeMeasureSpec(-1, MeasureSpecMode.Unspecified));
                        
                    size.Width += child.MeasuredWidth;
                    size.Height = Math.Max(size.Height, child.MeasuredHeight);
                }
            }

            return size / Device.Current.DisplayScale;
        }

        /// <summary>
        /// Called when a property value is changed.
        /// </summary>
        /// <param name="pd">A property descriptor describing the property whose value has been changed.</param>
        protected virtual void OnPropertyChanged(PropertyDescriptor pd)
        {
            PropertyChanged(this, new FrameworkPropertyChangedEventArgs(pd));
        }
        
        internal void OnLoaded()
        {
            if (!IsLoaded)
            {
                IsLoaded = true;
                OnPropertyChanged(Prism.UI.Visual.IsLoadedProperty);
                Loaded(this, EventArgs.Empty);
            }
        }

        internal void OnUnloaded()
        {
            if (IsLoaded)
            {
                IsLoaded = false;
                OnPropertyChanged(Prism.UI.Visual.IsLoadedProperty);
                Unloaded(this, EventArgs.Empty);
            }
        }
        
        private void OnItemPropertyChanged(object sender, FrameworkPropertyChangedEventArgs e)
        {
            if (e.Property == Prism.UI.Controls.MenuItem.ForegroundProperty)
            {
                SetItemForeground(sender as INativeMenuItem);
            }
        }
        
        private void OnOverflowButtonClicked(object sender, EventArgs e)
        {
            var popup = new PopupMenu(OverflowButton.Context, OverflowButton);
            for (int i = maxDisplayItems; i < Items.Count; i++)
            {
                var button = Items[i] as INativeMenuButton;
                if (button != null)
                {
                    var item = popup.Menu.Add(new Java.Lang.String(button.Title));
                    item.SetEnabled(button.IsEnabled);
                    item.SetOnMenuItemClickListener(new PopupMenuItemClickListener(this, button));
                    
                    var scb = (button.Foreground as SolidColorBrush) ?? Foreground as SolidColorBrush;
                    if (scb != null)
                    {
                        var span = new SpannableString(button.Title);
                        span.SetSpan(new ForegroundColorSpan(scb.Color.GetColor()), 0, button.Title.Length, 0);
                        item.SetTitle(span);
                    }
                }
            }

            popup.Show();
        }
        
        private void OnOverflowImageLoaded(object sender, EventArgs e)
        {
            var source = sender as INativeBitmapImage;
            if (source != null && source.SourceUri == overflowImageUri)
            {
                OverflowButton.SetImageBitmap(source.GetImageSource());
            }
        }
        
        private void SetButtons()
        {
            bool hasOverflow = maxDisplayItems < Items.Count;
            var items = ((ObservableCollection<INativeMenuItem>)Items).Take(hasOverflow ? maxDisplayItems : Items.Count).OfType<View>();
            var itemsEnumerator = items.GetEnumerator();
            
            var buttons = new View[items.Count() + (hasOverflow ? 1 : 0)];
            for (int i = 0; i < buttons.Length; i++)
            {
                if (i == buttons.Length - 1 && hasOverflow)
                {
                    buttons[0] = OverflowButton;
                }
                else if (itemsEnumerator.MoveNext())
                {
                    buttons[(buttons.Length - 1) - i] = itemsEnumerator.Current;
                }
            }
            
            SetButtons(buttons);
            
            MeasureRequest(false, null);
            ArrangeRequest(false, null);
        }
        
        private void SetButtons(View[] buttons)
        {
            var viewGroup = Parent as ViewGroup;
            if (viewGroup == null)
            {
                return;
            }
            
            for (int i = viewGroup.ChildCount - 1; i >= 0; i--)
            {
                var child = viewGroup.GetChildAt(i);
                if (child == OverflowButton || Items.Contains(child))
                {
                    viewGroup.RemoveView(child);
                    (renderTransform as Media.Transform)?.RemoveView(child);
                }
            }
            
            if (buttons != null)
            {
                foreach (var button in buttons)
                {
                    viewGroup.AddView(button, new FrameLayout.LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent));
                    button.Clickable = isHitTestVisible;
                    (renderTransform as Media.Transform)?.AddView(button);
                }
            }
        }
        
        private void SetItemForeground(INativeMenuItem item)
        {
            if (item != null && item.Foreground == null)
            {
                var button = item as MenuButton;
                if (button != null)
                {
                    button.SetForeground(foreground, false);
                }
            }
        }
        
        private class PopupMenuItemClickListener : Java.Lang.Object, IMenuItemOnMenuItemClickListener
        {
            private readonly WeakReference buttonRef;
            private readonly WeakReference menuRef;
            
            public PopupMenuItemClickListener(ActionMenu menu, object button)
            {
                buttonRef = new WeakReference(button);
                menuRef = new WeakReference(menu);
            }
        
            public bool OnMenuItemClick(IMenuItem item)
            {
                var menu = menuRef.Target as ActionMenu;
                var button = buttonRef.Target as View;
                return menu != null && menu.IsHitTestVisible && button != null && button.CallOnClick();
            }
        }
    }
    
    internal sealed class ActionMenuOverflowButton : ImageView
    {
        private const string OverflowButtonKey = "drawable/abc_ic_menu_moreoverflow_mtrl_alpha";
        
        public ActionMenuOverflowButton(global::Android.Content.Context context)
            : base(context)
        {
            SetColorFilter((Color)Prism.Application.Current.Resources[global::Android.Resource.Attribute.TextColorPrimary]);
            SetImageDrawable((Drawable)Prism.Application.Current.Resources[OverflowButtonKey]);
        }
        
        public override void SetImageDrawable(Drawable drawable)
        {
            if (drawable == null)
            {
                base.SetImageDrawable((Drawable)Prism.Application.Current.Resources[OverflowButtonKey]);
            }
            else
            {
                base.SetImageDrawable(drawable);
            }
        }
    }
}

