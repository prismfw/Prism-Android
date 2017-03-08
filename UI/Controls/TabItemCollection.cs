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
using Android.Views;
using Prism.Native;

namespace Prism.Android.UI.Controls
{
    internal class TabItemCollection : IList
    {
        public int Count
        {
            get { return TabLayout.TabCount; }
        }

        public bool IsFixedSize
        {
            get { return false; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public bool IsSynchronized
        {
            get { return false; }
        }

        public object SyncRoot
        {
            get { return null; }
        }

        public TabLayout TabLayout { get; }

        public object this[int index]
        {
            get
            {
                if (TabLayout.TabCount <= index)
                {
                    throw new ArgumentOutOfRangeException(nameof(index));
                }

                return TabLayout.GetTabAt(index);
            }
            set
            {
                if (TabLayout.TabCount <= index)
                {
                    throw new ArgumentOutOfRangeException(nameof(index));
                }

                TabLayout.RemoveTabAt(index);
                TabLayout.AddTab(value as View, index);
            }
        }
        
        public TabItemCollection(TabLayout layout)
        {
            if (layout == null)
            {
                throw new ArgumentNullException(nameof(layout));
            }
            
            TabLayout = layout;
        }

        public int Add(object value)
        {
            int count = TabLayout.TabCount;
            TabLayout.AddTab(value as View);
            return TabLayout.TabCount - count;
        }

        public void Clear()
        {
            TabLayout.RemoveAllTabs();
        }

        public bool Contains(object value)
        {
            for (int i = 0; i < TabLayout.TabCount; i++)
            {
                if (TabLayout.GetTabAt(i) == value)
                {
                    return true;
                }
            }
            
            return false;
        }

        public int IndexOf(object value)
        {
            for (int i = 0; i < TabLayout.TabCount; i++)
            {
                if (TabLayout.GetTabAt(i) == value)
                {
                    return i;
                }
            }
            
            return -1;
        }

        public void Insert(int index, object value)
        {
            if (index > Count)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            if (index == Count)
            {
                Add(value);
            }
            else
            {
                TabLayout.AddTab(value as View, index);
            }
        }

        public void Remove(object value)
        {
            TabLayout.RemoveTab(value as View);
        }

        public void RemoveAt(int index)
        {
            TabLayout.RemoveTabAt(index);
        }

        public void CopyTo(Array array, int index)
        {
            for (int i = 0; i < TabLayout.TabCount; i++)
            {
                array.SetValue(TabLayout.GetTabAt(i), i + index);
            }
        }

        public IEnumerator GetEnumerator()
        {
            return new TabItemEnumerator(TabLayout);
        }

        private class TabItemEnumerator : IEnumerator<INativeTabItem>, IEnumerator
        {
            private int currentIndex = -1;
            private TabLayout tabLayout;
        
            public INativeTabItem Current
            {
                get { return tabLayout?.GetTabAt(currentIndex) as INativeTabItem; }
            }

            object IEnumerator.Current
            {
                get { return tabLayout?.GetTabAt(currentIndex); }
            }

            public TabItemEnumerator(TabLayout layout)
            {
                tabLayout = layout;
            }

            public void Dispose()
            {
                tabLayout = null;
            }

            public bool MoveNext()
            {
                currentIndex++;
                return currentIndex < tabLayout.TabCount;
            }

            public void Reset()
            {
                currentIndex = -1;
            }
        }
    }
}

