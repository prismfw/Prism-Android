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
using System.Linq;
using Android.Support.Design.Widget;
using Prism.Native;

namespace Prism.Android.UI.Controls
{
    internal class TabItemCollection : IList
    {
        public int Count
        {
            get { return tabCollection.Count; }
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

        public TabLayout TabLayout
        {
            get { return tabLayout; }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("TabLayout");
                }

                tabLayout = value;
                for (int i = 0; i < tabCollection.Count; i++)
                {
                    var kvp = tabCollection[i];
                    kvp = new KeyValuePair<object, TabLayout.Tab>(kvp.Key, CreateTab(kvp.Key));
                    tabCollection[i] = kvp;
                    tabLayout.AddTab(kvp.Value);
                }
            }
        }
        private TabLayout tabLayout;

        public object this[int index]
        {
            get
            {
                if (tabCollection.Count <= index)
                {
                    throw new ArgumentOutOfRangeException(nameof(index));
                }

                return tabCollection[index].Key;
            }
            set
            {
                if (tabCollection.Count <= index)
                {
                    throw new ArgumentOutOfRangeException(nameof(index));
                }

                var kvp = tabCollection[index];
                TabLayout?.RemoveTab(kvp.Value);

                kvp = new KeyValuePair<object, TabLayout.Tab>(value, CreateTab(value));
                tabCollection[index] = kvp;
                TabLayout?.AddTab(kvp.Value, index);
            }
        }

        private readonly List<KeyValuePair<object, TabLayout.Tab>> tabCollection;
        
        public TabItemCollection()
        {
            tabCollection = new List<KeyValuePair<object, TabLayout.Tab>>();
        }

        public int Add(object value)
        {
            int count = tabCollection.Count;
            var kvp = new KeyValuePair<object, TabLayout.Tab>(value, CreateTab(value));
            tabCollection.Add(kvp);
            TabLayout?.AddTab(kvp.Value);
            return tabCollection.Count - count;
        }

        public void Clear()
        {
            TabLayout?.RemoveAllTabs();
            tabCollection.Clear();
        }

        public bool Contains(object value)
        {
            return tabCollection.Any(kvp => kvp.Key == value);
        }

        public object GetItemForTab(TabLayout.Tab tab)
        {
            return tabCollection.FirstOrDefault(kvp => kvp.Value == tab).Key;
        }

        public int IndexOf(object value)
        {
            return tabCollection.FindIndex(kvp => kvp.Key == value);
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
                var kvp = new KeyValuePair<object, TabLayout.Tab>(value, CreateTab(value));
                tabCollection.Insert(index, kvp);
                TabLayout?.AddTab(kvp.Value, index);
            }
        }

        public void Remove(object value)
        {
            var kvp = tabCollection.FirstOrDefault(k => k.Key == value);
            tabCollection.Remove(kvp);
            TabLayout?.RemoveTab(kvp.Value);
        }

        public void RemoveAt(int index)
        {
            var kvp = tabCollection[index];
            tabCollection.RemoveAt(index);
            TabLayout?.RemoveTab(kvp.Value);
        }

        public void CopyTo(Array array, int index)
        {
            tabCollection.Select(kvp => kvp.Key).ToArray().CopyTo(array, index);
        }

        public IEnumerator GetEnumerator()
        {
            return new TabItemEnumerator(tabCollection.Select(kvp => kvp.Key).GetEnumerator());
        }

        private TabLayout.Tab CreateTab(object value)
        {
            if (tabLayout == null)
            {
                return null;
            }

            var tab = tabLayout.NewTab();
            tab.SetCustomView(value as global::Android.Views.View);
            return tab;
        }

        private class TabItemEnumerator : IEnumerator<INativeTabItem>, IEnumerator
        {
            public INativeTabItem Current
            {
                get { return tabItemEnumerator.Current as INativeTabItem; }
            }

            object IEnumerator.Current
            {
                get { return tabItemEnumerator.Current; }
            }

            private readonly IEnumerator tabItemEnumerator;

            public TabItemEnumerator(IEnumerator tabItemEnumerator)
            {
                this.tabItemEnumerator = tabItemEnumerator;
            }

            public void Dispose()
            {
                var disposable = tabItemEnumerator as IDisposable;
                if (disposable != null)
                {
                    disposable.Dispose();
                }
            }

            public bool MoveNext()
            {
                do
                {
                    if (!tabItemEnumerator.MoveNext())
                    {
                        return false;
                    }
                }
                while (!(tabItemEnumerator.Current is INativeTabItem));

                return true;
            }

            public void Reset()
            {
                tabItemEnumerator.Reset();
            }
        }
    }
}

