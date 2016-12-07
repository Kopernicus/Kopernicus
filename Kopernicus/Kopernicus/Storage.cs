/**
 * Kopernicus Planetary System Modifier
 * ====================================
 * Created by: BryceSchroeder and Teknoman117 (aka. Nathaniel R. Lewis)
 * Maintained by: Thomas P., NathanKell and KillAshley
 * Additional Content by: Gravitasi, aftokino, KCreator, Padishar, Kragrathea, OvenProofMars, zengei, MrHappyFace, Sigma88
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
 * which is copyright 2011-2015 Squad. Your usage of Kerbal Space Program
 * itself is governed by the terms of its EULA, not the license above.
 * 
 * https://kerbalspaceprogram.com
 */

using System;
using UnityEngine;

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
            StorageComponent c = body?.gameObject?.AddOrGetComponent<StorageComponent>();
            return c != null ? c.Get<T>(id) : default(T);
        }

        /// <summary>
        /// Gets data from the internal storage of the body
        /// </summary>summary>
        public static T Get<T>(this PSystemBody body, String id)
        {
            return body.celestialBody.Get<T>(id);
        }

        /// <summary>
        /// Returns if the internal storage knows an id
        /// </summary>
        public static Boolean Has(this CelestialBody body, String id)
        {
            StorageComponent c = body?.gameObject?.AddOrGetComponent<StorageComponent>();
            return c?.Has(id) ?? false;
        }

        /// <summary>
        /// Returns if the internal storage knows an id
        /// </summary>
        public static Boolean Has(this PSystemBody body, String id)
        {
            return body.celestialBody.Has(id);
        }

        /// <summary>
        /// Adds an element to the internal storage
        /// </summary>
        public static void Set<T>(this CelestialBody body, String id, T value)
        {
            StorageComponent c = body?.gameObject?.AddOrGetComponent<StorageComponent>();
            c?.Set<T>(id, value);
        }

        /// <summary>
        /// Adds an element to the internal storage
        /// </summary>
        public static void Set<T>(this PSystemBody body, String id, T value)
        {
            body.celestialBody.Set<T>(id, value);
        }

        /// <summary>
        /// Removes an element from the internal storage
        /// </summary>
        public static void Remove(this CelestialBody body, String id)
        {
            StorageComponent c = body?.gameObject?.AddOrGetComponent<StorageComponent>();
            c?.Remove(id);
        }

        /// <summary>
        /// Removes an element from the internal storage
        /// </summary>
        public static void Remove(this PSystemBody body, String id)
        {
            body.celestialBody.Remove(id);
        }
    }
}