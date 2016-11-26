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
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Object = System.Object;

namespace Kopernicus
{
    /// <summary>
    /// A component that stores data in a planet
    /// </summary>
    [RequireComponent(typeof(CelestialBody))]
    public class StorageComponent : MonoBehaviour
    {
        /// <summary>
        /// The data stored by the component
        /// </summary>
        private static Dictionary<String, Dictionary<String, System.Object>> data { get; set; }

        static StorageComponent()
        {
            data = new Dictionary<String, Dictionary<String, Object>>();
        }

        private CelestialBody body
        {
            get { return GetComponent<CelestialBody>(); }
        }

        void Awake()
        {
            if (!data.ContainsKey(Unify(body.transform.name)))
                data.Add(Unify(body.transform.name), new Dictionary<String, Object>());
        }

        /// <summary>
        /// Gets data from the storage
        /// </summary>
        public T Get<T>(String id)
        {
            if (!data.ContainsKey(Unify(body.transform.name)))
                data.Add(Unify(body.transform.name), new Dictionary<String, Object>());
            if (data[Unify(body.transform.name)].ContainsKey(id))
                return (T) data[Unify(body.transform.name)][id];
            throw new IndexOutOfRangeException();
        }

        /// <summary>
        /// Whether the storage contains this key
        /// </summary>
        public Boolean Has(String id)
        {
            if (!data.ContainsKey(Unify(body.transform.name)))
                data.Add(Unify(body.transform.name), new Dictionary<String, Object>());
            return data[Unify(body.transform.name)].ContainsKey(id);
        }

        /// <summary>
        /// Writes data into the storage
        /// </summary>
        public void Set<T>(String id, T value)
        {
            if (!data.ContainsKey(Unify(body.transform.name)))
                data.Add(Unify(body.transform.name), new Dictionary<String, Object>());
            if (data[Unify(body.transform.name)].ContainsKey(id))
                data[Unify(body.transform.name)][id] = value;
            else
                data[Unify(body.transform.name)].Add(id, value);
        }

        /// <summary>
        /// Removes data from the storage
        /// </summary>
        public void Remove(String id)
        {
            if (!data.ContainsKey(Unify(body.transform.name)))
                data.Add(Unify(body.transform.name), new Dictionary<String, Object>());
            if(data[Unify(body.transform.name)].ContainsKey(id))
                data[Unify(body.transform.name)].Remove(id);
            else
                throw new IndexOutOfRangeException();
        }

        private String Unify(String id)
        {
            return id.Replace("(Clone)", "").Trim();
        }
    }
}