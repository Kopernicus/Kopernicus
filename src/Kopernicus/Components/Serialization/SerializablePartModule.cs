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
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Kopernicus.Components.Serialization
{
    [Serializable]
    public class SerializablePartModule : PartModule, ISerializationCallbackReceiver
    {
        // static values
        private static readonly Dictionary<String, Dictionary<String, System.Object>> Properties =
            new Dictionary<String, Dictionary<String, System.Object>>();

        // this is because SerializationID is not public
        [SerializeField]
        protected String serializationId;

        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
            // Create a new ID if it is null
            if (String.IsNullOrEmpty(serializationId))
            {
                serializationId = Guid.NewGuid().ToString();
            }

            // If there is no entry, add it
            if (!Properties.ContainsKey(serializationId))
            {
                Properties.Add(serializationId, new Dictionary<String, System.Object>());
            }

            Type ownType = GetType();

            // Serialize all fields into the dictionary
            FieldInfo[] fields =
                ownType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            foreach (FieldInfo info in fields)
            {
                // If it is public, store it
                if (info.IsPublic)
                {
                    if (Properties[serializationId].ContainsKey(info.Name))
                    {
                        Properties[serializationId][info.Name] = info.GetValue(this);
                    }
                    else
                    {
                        Properties[serializationId].Add(info.Name, info.GetValue(this));
                    }
                }
                else
                {
                    // Does the field have a SerializeField attribute?
                    if (!(info.GetCustomAttributes(typeof(SerializeField),
                            false) is SerializeField[] serializeFields) || serializeFields.Length <= 0)
                    {
                        continue;
                    }

                    if (Properties[serializationId].ContainsKey(info.Name))
                    {
                        Properties[serializationId][info.Name] = info.GetValue(this);
                    }
                    else
                    {
                        Properties[serializationId].Add(info.Name, info.GetValue(this));
                    }
                }
            }
        }

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            // Create a new ID if it is null
            if (String.IsNullOrEmpty(serializationId))
            {
                serializationId = Guid.NewGuid().ToString();
            }

            // If there is no entry, add it
            if (!Properties.ContainsKey(serializationId))
            {
                Properties.Add(serializationId, new Dictionary<String, System.Object>());
            }

            Type ownType = GetType();

            // Serialize all fields from the dictionary
            FieldInfo[] fields =
                ownType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            foreach (FieldInfo info in fields)
            {
                // If it is public, store it
                if (info.IsPublic)
                {
                    if (Properties[serializationId].ContainsKey(info.Name))
                    {
                        info.SetValue(this, Properties[serializationId][info.Name]);
                    }
                }
                else
                {
                    // Does the field have a SerializeField attribute?

                    if (!(info.GetCustomAttributes(typeof(SerializeField),
                            false) is SerializeField[] serializeFields) || serializeFields.Length <= 0)
                    {
                        continue;
                    }

                    if (Properties[serializationId].ContainsKey(info.Name))
                    {
                        info.SetValue(this, Properties[serializationId][info.Name]);
                    }
                }
            }
        }
    }
}