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
 * which is copyright 2011-2017 Squad. Your usage of Kerbal Space Program
 * itself is governed by the terms of its EULA, not the license above.
 * 
 * https://kerbalspaceprogram.com
 */

using System;
using System.Collections;
using System.Collections.Generic;

namespace Kopernicus
{
    namespace Configuration
    {
        /// <summary>
        /// A list that allows us to edit newly added elements
        /// </summary>
        public class CallbackList<T> : IList<T>
        {
            private List<T> _list;
            private Action<T> callback;

            public CallbackList(Action<T> callback)
            {
                _list = new List<T>();
                this.callback = callback;
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
                callback(item);
                _list.Add(item);
            }

            public void Clear()
            {
                _list.Clear();
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
                return _list.Remove(item);
            }

            public Int32 Count
            {
                get { return _list.Count; }
            }

            public Boolean IsReadOnly
            {
                get { return ((IList<T>) _list).IsReadOnly; }
            }

            public Int32 IndexOf(T item)
            {
                return _list.IndexOf(item);
            }

            public void Insert(Int32 index, T item)
            {
                _list.Insert(index, item);
            }

            public void RemoveAt(Int32 index)
            {
                _list.RemoveAt(index);
            }

            public T this[Int32 index]
            {
                get { return _list[index]; }
                set { _list[index] = value; }
            }
        }
    }
}