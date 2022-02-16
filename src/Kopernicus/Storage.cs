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
using Kopernicus.Components;

namespace Kopernicus
{
    /// <summary>
    /// Extension methods for storing data
    /// </summary>
    public static class Storage
    {
        /// <summary>
        /// Gets data from the internal storage of the body
        /// </summary>
        public static T Get<T>(this CelestialBody body, String id)
        {
            if (!body)
            {
                return default(T);
            }

            StorageComponent c = body.gameObject.AddOrGetComponent<StorageComponent>();
            return !c ? default(T) : c.Get<T>(id);
        }

        /// <summary>
        /// Gets data from the internal storage of the body
        /// </summary>summary>
        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        public static T Get<T>(this PSystemBody body, String id)
        {
            return body.celestialBody.Get<T>(id);
        }

        /// <summary>
        /// Returns if the internal storage knows an id
        /// </summary>
        public static Boolean Has(this CelestialBody body, String id)
        {
            if (!body)
            {
                return false;
            }

            StorageComponent c = body.gameObject.AddOrGetComponent<StorageComponent>();
            return c && c.Has(id);
        }

        /// <summary>
        /// Returns if the internal storage knows an id
        /// </summary>
        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        public static Boolean Has(this PSystemBody body, String id)
        {
            return body.celestialBody.Has(id);
        }

        /// <summary>
        /// Adds an element to the internal storage
        /// </summary>
        public static void Set<T>(this CelestialBody body, String id, T value)
        {
            if (!body)
            {
                return;
            }

            StorageComponent c = body.gameObject.AddOrGetComponent<StorageComponent>();
            if (!c)
            {
                return;
            }
            c.Set(id, value);
        }

        /// <summary>
        /// Adds an element to the internal storage
        /// </summary>
        public static void Set<T>(this PSystemBody body, String id, T value)
        {
            body.celestialBody.Set(id, value);
        }

        /// <summary>
        /// Removes an element from the internal storage
        /// </summary>
        [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
        public static void Remove(this CelestialBody body, String id)
        {
            if (!body)
            {
                return;
            }

            StorageComponent c = body.gameObject.AddOrGetComponent<StorageComponent>();
            if (!c)
            {
                return;
            }
            c.Remove(id);
        }

        /// <summary>
        /// Removes an element from the internal storage
        /// </summary>
        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        public static void Remove(this PSystemBody body, String id)
        {
            body.celestialBody.Remove(id);
        }

        /// <summary>
        /// Returns a value or a default one if the element doesn't exist
        /// </summary>
        public static T Get<T>(this CelestialBody body, String id, T defaultValue)
        {
            return body.Has(id) ? body.Get<T>(id) : defaultValue;
        }

        /// <summary>
        /// Returns a value or a default one if the element doesn't exist
        /// </summary>
        public static T Get<T>(this PSystemBody body, String id, T defaultValue)
        {
            return body.celestialBody.Get(id, defaultValue);
        }
    }
}