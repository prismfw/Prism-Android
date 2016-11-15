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
        private const string OverflowButtonKey = "drawable/abc_ic_menu_moreoverflow_mtrl_alpha";
    
        /// <summary>
        /// Occurs when the value of a property is changed.
        /// </summary>
        public event EventHandler<FrameworkPropertyChangedEventArgs> PropertyChanged;
        
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
        /// Gets the amount that the menu is inset on top of its parent view.
        /// </summary>
        public Thickness Insets
        {
            get { return new Thickness(); }
        }

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
                    if (attachedParent != null && (oldValue < Items.Count || maxDisplayItems < Items.Count))
                    {
                        SetButtons();
                    }
                    
                    OnPropertyChanged(Prism.UI.Controls.ActionMenu.MaxDisplayItemsProperty);
                }
            }
        }
        private int maxDisplayItems;

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
                        OverflowButton.SetImageDrawable((Drawable)Prism.Application.Current.Resources[OverflowButtonKey]);
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
        
        private ImageView OverflowButton { get; }
        
        private ViewStackHeader attachedParent;

        /// <summary>
        /// Initializes a new instance of the <see cref="ActionMenu"/> class.
        /// </summary>
        public ActionMenu()
        {
            OverflowButton = new ImageView(Application.MainActivity);
            OverflowButton.SetColorFilter((Color)Prism.Application.Current.Resources[global::Android.Resource.Attribute.TextColorPrimary]);
            OverflowButton.SetImageDrawable((Drawable)Prism.Application.Current.Resources[OverflowButtonKey]);

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
            
                if (attachedParent == null)
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
                        attachedParent.SetMenuButtons(null);
                        break;
                }
            };
        }
        
        /// <summary>
        /// Attaches the menu to the specified parent.
        /// </summary>
        public void Attach(ViewStackHeader parent)
        {
            attachedParent = parent;
            SetButtons();
        }
        
        /// <summary>
        /// Detaches the menu from its current parent.
        /// </summary>
        public void Detach()
        {
            attachedParent?.SetMenuButtons(null);
            attachedParent = null;
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
                    item.SetOnMenuItemClickListener(new PopupMenuItemClickListener(button));
                    
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

        /// <summary>
        /// Called when a property value is changed.
        /// </summary>
        /// <param name="pd">A property descriptor describing the property whose value has been changed.</param>
        protected virtual void OnPropertyChanged(PropertyDescriptor pd)
        {
            PropertyChanged(this, new FrameworkPropertyChangedEventArgs(pd));
        }
        
        private void OnItemPropertyChanged(object sender, FrameworkPropertyChangedEventArgs e)
        {
            if (e.Property == Prism.UI.Controls.MenuItem.ForegroundProperty)
            {
                SetItemForeground(sender as INativeMenuItem);
            }
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
            var items = ((ObservableCollection<INativeMenuItem>)Items).Take(hasOverflow ? maxDisplayItems : Items.Count).OfType<global::Android.Views.View>();
            var itemsEnumerator = items.GetEnumerator();
            
            var buttons = new global::Android.Views.View[items.Count() + (hasOverflow ? 1 : 0)];
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
            
            attachedParent.SetMenuButtons(buttons);
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
            
            public PopupMenuItemClickListener(object button)
            {
                buttonRef = new WeakReference(button);
            }
        
            public bool OnMenuItemClick(IMenuItem item)
            {
                var button = buttonRef.Target as global::Android.Views.View;
                return (button?.CallOnClick()).GetValueOrDefault();
            }
        }
    }
}

