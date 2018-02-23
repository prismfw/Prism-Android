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
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Android.Media;
using Android.Runtime;
using Prism.Native;

namespace Prism.Android.Media
{
    /// <summary>
    /// Represents an Android implementation of an <see cref="INativeMediaPlaybackList"/>.
    /// </summary>
    [Preserve(AllMembers = true)]
    [Register(typeof(INativeMediaPlaybackList))]
    public class MediaPlaybackList : Java.Lang.Object, INativeMediaPlaybackList
    {
        /// <summary>
        /// Occurs when the currently playing item has changed.
        /// </summary>
        public event EventHandler<NativeItemChangedEventArgs> CurrentItemChanged;

        /// <summary>
        /// Occurs when a playback item has failed to open.
        /// </summary>
        public event EventHandler<NativeErrorEventArgs> ItemFailed;

        /// <summary>
        /// Occurs when a playback item has been successfully opened.
        /// </summary>
        public event EventHandler<NativeItemEventArgs> ItemOpened;

        /// <summary>
        /// Occurs when the last time in the playlist has finished playing.
        /// </summary>
        public event EventHandler PlaylistEnded;

        /// <summary>
        /// Gets the zero-based index of the current item in the <see cref="Items"/> collection.
        /// </summary>
        public int CurrentItemIndex { get; private set; } = -1;

        /// <summary>
        /// Gets or sets a value indicating whether the playlist should automatically restart after the last item has finished playing.
        /// </summary>
        public bool IsRepeatEnabled
        {
            get { return isRepeatEnabled; }
            set
            {
                if (value != isRepeatEnabled)
                {
                    isRepeatEnabled = value;
                    
                    if (isActive && CurrentItemIndex >= 0 && (isShuffleEnabled ?
                        shuffledItems.IndexOf(Items[CurrentItemIndex]) == shuffledItems.Count - 1 : CurrentItemIndex == Items.Count - 1))
                    {
                        SetNextItem();
                    }
                }
            }
        }
        private bool isRepeatEnabled;

        /// <summary>
        /// Gets or sets a value indicating whether the items in the playlist should be played in random order.
        /// </summary>
        public bool IsShuffleEnabled
        {
            get { return isShuffleEnabled; }
            set
            {
                if (value == isShuffleEnabled)
                {
                    return;
                }

                isShuffleEnabled = value;
                if (isShuffleEnabled)
                {
                    shuffledItems = new List<object>(Items as IEnumerable<object>);

                    var random = new Random();
                    for (int i = shuffledItems.Count - 1; i > 1; i--)
                    {
                        int index = random.Next(i + 1);
                        var item = shuffledItems[index];
                        shuffledItems[index] = shuffledItems[i];
                        shuffledItems[i] = item;
                    }
                }

                if (isActive && CurrentItemIndex >= 0)
                {
                    SetNextItem();
                }
            }
        }
        private bool isShuffleEnabled;

        /// <summary>
        /// Gets a collection of playback items that make up the playlist.
        /// </summary>
        public IList Items { get; }

        private object activeItem;
        private bool isActive;
        private IList shuffledItems;

        /// <summary>
        /// Initializes a new instance of the <see cref="MediaPlaybackList"/> class.
        /// </summary>
        public MediaPlaybackList()
        {
            Items = new ObservableCollection<object>();
            ((ObservableCollection<object>)Items).CollectionChanged += OnItemsCollectionChanged;
        }

        /// <summary>
        /// Sets the playlist to an active state where it begins preparing and playing media items.
        /// </summary>
        public void Activate()
        {
            isActive = true;
            if (Items.Count > 0)
            {
                CurrentItemIndex = Math.Max(CurrentItemIndex, 0);
                ((isShuffleEnabled ? shuffledItems[CurrentItemIndex] : Items[CurrentItemIndex]) as MediaPlayer)?.PrepareAsync();
            }
        }

        /// <summary>
        /// Sets the playlist to a deactivated state where it no longer prepares or plays media items.
        /// </summary>
        public void Deactivate()
        {
            isActive = false;
            if (CurrentItemIndex >= 0)
            {
                (Items[CurrentItemIndex] as MediaPlayer)?.Stop();
            }
        }

        /// <summary>
        /// Moves to the next item in the playlist.
        /// </summary>
        public void MoveNext()
        {
            if (CurrentItemIndex < 0)
            {
                return;
            }

            var currentItem = Items[CurrentItemIndex];
            if (Items.Count == 1)
            {
                CurrentItemChanged(this, new NativeItemChangedEventArgs(currentItem, currentItem));
                (currentItem as MediaPlayer)?.Start();
            }
            else
            {
                int index = CurrentItemIndex;
                if (isShuffleEnabled)
                {
                    index = shuffledItems.IndexOf(currentItem) + 1;
                    if (index == shuffledItems.Count)
                    {
                        index = Items.IndexOf(shuffledItems[0]);
                    }
                }
                else
                {
                    index++;
                    if (index == Items.Count)
                    {
                        index = 0;
                    }
                }

                (Items[index] as MediaPlayer)?.Start();
            }
        }

        /// <summary>
        /// Moves to the previous item in the playlist.
        /// </summary>
        public void MovePrevious()
        {
            if (CurrentItemIndex < 0)
            {
                return;
            }

            var currentItem = Items[CurrentItemIndex];
            if (Items.Count == 1)
            {
                CurrentItemChanged(this, new NativeItemChangedEventArgs(currentItem, currentItem));
                (currentItem as MediaPlayer)?.Start();
            }
            else
            {
                int index = CurrentItemIndex;
                if (isShuffleEnabled)
                {
                    index = shuffledItems.IndexOf(currentItem) - 1;
                    if (index < 0)
                    {
                        index = Items.IndexOf(shuffledItems[shuffledItems.Count - 1]);
                    }
                }
                else
                {
                    index--;
                    if (index < 0)
                    {
                        index = Items.Count - 1;
                    }
                }

                (Items[index] as MediaPlayer)?.Start();
            }
        }

        /// <summary>
        /// Moves to the item in the playlist that is located at the specified index.
        /// </summary>
        /// <param name="itemIndex">The zero-based index of the item to move to.</param>
        public void MoveTo(int itemIndex)
        {
            if (isActive)
            {
                CurrentItemIndex = itemIndex;
                if (activeItem == null)
                {   (Items[itemIndex] as MediaPlayer)?.PrepareAsync();
                
                }
                else
                {
                    (Items[itemIndex] as MediaPlayer)?.Start();
                }
            }
        }

        private void OnItemsCollectionChanged(object o, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Move)
            {
                // Not supporting Move actions since they shouldn't ever occur.
                return;
            }

            if (e.OldItems != null)
            {
                for (int i = 0; i < e.OldItems.Count; i++)
                {
                    var oldItem = e.OldItems[i] as MediaPlayer;
                    if (oldItem != null)
                    {
                        oldItem.SetNextMediaPlayer(null);

                        var mpi = oldItem as MediaPlaybackItem;
                        if (mpi != null)
                        {
                            mpi.StatusChanged -= OnItemStatusChanged;
                        }
                    }
                    
                }
            }

            if (e.NewItems != null)
            {
                for (int i = 0; i < e.NewItems.Count; i++)
                {
                    var newItem = e.NewItems[i] as MediaPlaybackItem;
                    if (newItem != null)
                    {
                        newItem.StatusChanged -= OnItemStatusChanged;
                        newItem.StatusChanged += OnItemStatusChanged;
                    }
                }
            }

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    if (e.NewStartingIndex <= CurrentItemIndex)
                    {
                        CurrentItemIndex += e.NewItems.Count;
                    }

                    if (isShuffleEnabled)
                    {
                        bool setNextItem = false;
                        var random = new Random();
                        var currentIndex = CurrentItemIndex >= 0 ? shuffledItems.IndexOf(Items[CurrentItemIndex]) : -1;
                        for (int i = 0; i < e.NewItems.Count; i++)
                        {
                            var newItem = e.NewItems[i] as MediaPlaybackItem;
                            if (newItem != null)
                            {
                                int index = random.Next(0, shuffledItems.Count);
                                shuffledItems.Insert(index, newItem);
                                if (isActive && (index == currentIndex + 1 ||
                                    (isRepeatEnabled && currentIndex == shuffledItems.Count - 1 && index == 0)))
                                {
                                    setNextItem = true;
                                }
                            }
                        }

                        if (isActive && setNextItem)
                        {
                            SetNextItem();
                        }
                    }
                    else if (isActive && (e.NewStartingIndex == CurrentItemIndex + 1 ||
                        (isRepeatEnabled && CurrentItemIndex == Items.Count - 1 && e.NewStartingIndex == 0)))
                    {
                        SetNextItem();
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    bool resetNextItem = false;
                    if (isShuffleEnabled)
                    {
                        var nextItem = GetNextItem();
                        for (int i = 0; i < e.OldItems.Count; i++)
                        {
                            var oldItem = e.OldItems[i];
                            shuffledItems.Remove(e.OldItems[i]);
                            resetNextItem = resetNextItem || oldItem == nextItem;
                        }
                    }

                    if (isActive)
                    {
                        int oldCurrentIndex = CurrentItemIndex;
                        if (CurrentItemIndex >= e.OldStartingIndex)
                        {
                            CurrentItemIndex -= Math.Min((CurrentItemIndex - e.OldStartingIndex), e.OldItems.Count);
                            if (CurrentItemIndex >= Items.Count)
                            {
                                CurrentItemIndex = Items.Count - 1;
                            }
                        }

                        if (oldCurrentIndex >= e.OldStartingIndex && oldCurrentIndex < e.OldStartingIndex + e.OldItems.Count)
                        {
                            var newItem = Items[CurrentItemIndex] as MediaPlayer;
                            var oldItem = e.OldItems[oldCurrentIndex - e.OldStartingIndex] as MediaPlayer;
                            CurrentItemChanged(this, new NativeItemChangedEventArgs(oldItem, newItem));

                            activeItem = newItem;
                            if (oldItem != null && oldItem.IsPlaying)
                            {
                                oldItem.Stop();
                                newItem?.Start();
                            }

                            resetNextItem = true;
                        }

                        if (resetNextItem || oldCurrentIndex + 1 == e.OldStartingIndex ||
                            (isRepeatEnabled && e.OldStartingIndex == 0 && oldCurrentIndex == Items.Count + e.OldItems.Count - 1))
                        {
                            SetNextItem();
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Replace:
                    if (isShuffleEnabled)
                    {
                        for (int i = 0; i < e.OldItems.Count; i++)
                        {
                            int index = shuffledItems.IndexOf(e.OldItems[i]);
                            if (index >= 0)
                            {
                                shuffledItems[index] = e.NewItems[i];
                            }
                        }
                    }

                    if (isActive)
                    {
                        if (CurrentItemIndex >= e.OldStartingIndex && CurrentItemIndex < e.OldStartingIndex + e.OldItems.Count)
                        {
                            var newItem = Items[CurrentItemIndex] as MediaPlayer;
                            var oldItem = e.OldItems[CurrentItemIndex - e.OldStartingIndex] as MediaPlayer;
                            CurrentItemChanged(this, new NativeItemChangedEventArgs(oldItem, newItem));

                            activeItem = newItem;
                            if (oldItem != null && oldItem.IsPlaying)
                            {
                                oldItem.Stop();
                                newItem?.Start();
                            }

                            SetNextItem();
                        }
                        else if (e.NewItems.Contains(GetNextItem()))
                        {
                            SetNextItem();
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Reset:
                    shuffledItems?.Clear();
                    CurrentItemIndex = -1;
                    if (activeItem != null)
                    {
                        (activeItem as MediaPlayer)?.Stop();
                        CurrentItemChanged(this, new NativeItemChangedEventArgs(activeItem, null));
                        activeItem = null;
                    }
                    break;
            }
        }

        private void OnItemStatusChanged(object sender, EventArgs e)
        {
            var item = sender as MediaPlaybackItem;
            if (item != null)
            {
                switch (item.Status)
                {
                    case MediaPlayerStatus.Error:
                        ItemFailed(this, new NativeErrorEventArgs(item, new Exception(item.Error.ToString())));
                        break;
                    case MediaPlayerStatus.Finished:
                        if (isRepeatEnabled)
                        {
                            if (Items.Count == 1)
                            {
                                item.Start();
                                return;
                            }
                        }
                        else if ((isShuffleEnabled ? shuffledItems : Items)[Items.Count - 1] == item)
                        {
                            PlaylistEnded?.Invoke(this, EventArgs.Empty);
                        }
                        break;
                    case MediaPlayerStatus.Prepared:
                        if (!item.IsOpen)
                        {
                            ItemOpened(this, new NativeItemEventArgs(item));
                        }

                        if (activeItem == null && (isShuffleEnabled ? shuffledItems : Items)[CurrentItemIndex] == item)
                        {
                            activeItem = item;
                            CurrentItemIndex = Items.IndexOf(item);
                            CurrentItemChanged(this, new NativeItemChangedEventArgs(null, item));
                            SetNextItem();
                        }

                        var currentItem = Items[CurrentItemIndex];
                        if (currentItem != item && GetNextItem() == item)
                        {
                            (currentItem as MediaPlayer)?.SetNextMediaPlayer(item);
                        }
                        break;
                    case MediaPlayerStatus.Started:
                        if (activeItem != item)
                        {
                            var oldItem = activeItem;
                            activeItem = item;
                            CurrentItemIndex = Items.IndexOf(item);
                            CurrentItemChanged(this, new NativeItemChangedEventArgs(oldItem, item));
                            (oldItem as MediaPlayer)?.Stop();
                            SetNextItem();
                        }
                        break;
                }
            }
        }

        private object GetNextItem()
        {
            if (Items.Count == 0)
            {
                return null;
            }

            if (Items.Count == 1)
            {
                return Items[0];
            }

            if (isShuffleEnabled)
            {
                int index = shuffledItems.IndexOf(Items[CurrentItemIndex]);
                return index < shuffledItems.Count - 1 ? shuffledItems[index + 1] : (isRepeatEnabled ? shuffledItems[0] : null);
            }

            return CurrentItemIndex < Items.Count - 1 ? Items[CurrentItemIndex + 1] : (isRepeatEnabled ? Items[0] : null);
        }

        private void SetNextItem()
        {
            if (Items.Count <= 1)
            {
                return;
            }

            var currentItem = CurrentItemIndex >= 0 ? Items[CurrentItemIndex] : null;
            if (currentItem != null)
            {
                int index = CurrentItemIndex;
                var items = Items;
                if (isShuffleEnabled)
                {
                    index = shuffledItems.IndexOf(currentItem);
                    items = shuffledItems;
                }

                var nextItem = (index < items.Count - 1 ? items[index + 1] : (isRepeatEnabled ? items[0] : null)) as MediaPlayer;
                if (nextItem == null)
                {
                    (currentItem as MediaPlayer)?.SetNextMediaPlayer(null);
                }
                else
                {
                    nextItem.PrepareAsync();
                    if ((nextItem as MediaPlaybackItem)?.Status == MediaPlayerStatus.Prepared)
                    {
                        (currentItem as MediaPlayer)?.SetNextMediaPlayer(nextItem);
                    }
                }
            }
        }
    }
}