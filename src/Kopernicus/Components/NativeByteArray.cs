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
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace Kopernicus.Components
{
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    [SuppressMessage("ReSharper", "PossibleNullReferenceException")]
    public unsafe class NativeByteArray
    {
        /// <summary>
        /// The size of the array
        /// </summary>
        public Int32 Size { get; private set; }

        /// <summary>
        /// The address of our array in memory
        /// </summary>
        private IntPtr _items;

        /// <summary>
        /// Create a new Array using unmanaged memory
        /// </summary>
        /// <param name="size"></param>
        public NativeByteArray(Int32 size)
        {
            Size = size;

            // Allocate a pointer to the custom array
            _items = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(Byte)) * Size);
        }

        /// <summary>
        /// Disposes the memory used by the array
        /// </summary>
        public void Free()
        {
            Marshal.FreeHGlobal(_items);
        }

        /// <summary>
        /// Changes the size of the native array
        /// </summary>
        /// <param name="newSize"></param>
        public void Resize(Int32 newSize)
        {
            // Allocate a new array
            IntPtr newItems = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(Byte)) * newSize);

            // Copy the old one
            for (Int32 i = 0; i < Size; i++)
            {
                ((Byte*)newItems)[i] = this[i];
            }

            // Free the old memory
            Marshal.FreeHGlobal(_items);

            // Assign the new values
            Size = newSize;
            _items = newItems;
        }

        public ref Byte this[Int32 index]
        {
            get { return ref ((Byte*)_items)[index]; }
        }
    }
}