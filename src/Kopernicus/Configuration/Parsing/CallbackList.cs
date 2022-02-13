/**
 * Kopernicus Planetary System Modifier
 * ------------------------------------------------------------- 
 * This library is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 3 of the License, or (at your option) any later version.
 *
 * This library is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public
 * License along with this library; if not, write to the Free Software
 * Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston,
 * MA 02110-1301  USA
 * 
 * This library is intended to be used as a plugin for Kerbal Space Program
 * which is copyright of TakeTwo Interactive. Your usage of Kerbal Space Program
 * itself is governed by the terms of its EULA, not the license above.
 * 
 * https://kerbalspaceprogram.com
 */

using System;
using System.Collections;
using System.Collections.Generic;

namespace Kopernicus.Configuration.Parsing
{
    /// <summary>
    /// A list that allows us to edit newly added elements
    /// </summary>
    public class CallbackList<T> : IList<T>, IList
    {
        private readonly List<T> _list;
        private readonly Action<T> _callback;

        public CallbackList(Action<T> callback)
        {
            _list = new List<T>();
            _callback = callback;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _list.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(T item)
        {
            _list.Add(item);
            _callback(item);
        }

        public void Add(T item, Boolean call)
        {
            _list.Add(item);
            if (call)
            {
                _callback(item);
            }
        }

        Int32 IList.Add(Object value)
        {
            Add((T)value);
            return IndexOf((T)value);
        }

        public void Clear()
        {
            _list.Clear();
            _callback(default(T));
        }

        public void Clear(Boolean call)
        {
            _list.Clear();
            if (call)
            {
                _callback(default(T));
            }
        }

        Boolean IList.Contains(Object value)
        {
            return Contains((T)value);
        }

        Int32 IList.IndexOf(Object value)
        {
            return IndexOf((T)value);
        }

        void IList.Insert(Int32 index, Object value)
        {
            Insert(index, (T)value);
        }

        void IList.Remove(Object value)
        {
            Remove((T)value);
        }

        public Boolean Contains(T item)
        {
            return _list.Contains(item);
        }

        public void CopyTo(T[] array, Int32 arrayIndex)
        {
            _list.CopyTo(array, arrayIndex);
        }

        public Boolean Remove(T item)
        {
            Boolean val = _list.Remove(item);
            _callback(default(T));
            return val;
        }

        void ICollection.CopyTo(Array array, Int32 index)
        {
            throw new NotImplementedException();
        }

        public Int32 Count
        {
            get { return _list.Count; }
        }

        Boolean ICollection.IsSynchronized
        {
            get { return ((IList)_list).IsSynchronized; }
        }

        Object ICollection.SyncRoot
        {
            get { return ((IList)_list).SyncRoot; }
        }

        public Boolean IsReadOnly
        {
            get { return ((IList<T>)_list).IsReadOnly; }
        }

        Object IList.this[Int32 index]
        {
            get { return _list[index]; }
            set
            {
                _list[index] = (T)value;
                _callback((T)value);
            }
        }

        public Int32 IndexOf(T item)
        {
            return _list.IndexOf(item);
        }

        public void Insert(Int32 index, T item)
        {
            _callback(item);
            _list.Insert(index, item);
        }

        public void RemoveAt(Int32 index)
        {
            _list.RemoveAt(index);
            _callback(default(T));
        }

        Boolean IList.IsFixedSize
        {
            get { return ((IList)_list).IsFixedSize; }
        }

        public T this[Int32 index]
        {
            get { return _list[index]; }
            set
            {
                _list[index] = value;
                _callback(value);
            }
        }
    }
}