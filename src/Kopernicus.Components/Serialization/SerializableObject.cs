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
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Kopernicus
{
    namespace Components
    {
        [Serializable]
        public class SerializableObject : ISerializationCallbackReceiver
        {
            // static values
            private static Dictionary<String, Dictionary<String, System.Object>> _properties =
                new Dictionary<String, Dictionary<String, System.Object>>();

            // this is because SerializationID is not public
            [SerializeField] 
            protected String SerializationID;

            void ISerializationCallbackReceiver.OnBeforeSerialize()
            {
                // Create a new ID if it is null
                if (String.IsNullOrEmpty(SerializationID))
                {
                    SerializationID = Guid.NewGuid().ToString();
                }

                // If there is no entry, add it
                if (!_properties.ContainsKey(SerializationID))
                {
                    _properties.Add(SerializationID, new Dictionary<String, System.Object>());
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
                        if (_properties[SerializationID].ContainsKey(info.Name))
                        {
                            _properties[SerializationID][info.Name] = info.GetValue(this);
                        }
                        else
                        {
                            _properties[SerializationID].Add(info.Name, info.GetValue(this));
                        }
                    }
                    else
                    {
                        // Does the field have a SerializeField attribute?
                        SerializeField[] serializeFields =
                            info.GetCustomAttributes(typeof(SerializeField), false) as SerializeField[];
                        if (serializeFields != null && serializeFields.Length > 0)
                        {
                            if (_properties[SerializationID].ContainsKey(info.Name))
                            {
                                _properties[SerializationID][info.Name] = info.GetValue(this);
                            }
                            else
                            {
                                _properties[SerializationID].Add(info.Name, info.GetValue(this));
                            }
                        }
                    }
                }
            }

            void ISerializationCallbackReceiver.OnAfterDeserialize()
            {
                // Create a new ID if it is null
                if (String.IsNullOrEmpty(SerializationID))
                {
                    SerializationID = Guid.NewGuid().ToString();
                }

                // If there is no entry, add it
                if (!_properties.ContainsKey(SerializationID))
                {
                    _properties.Add(SerializationID, new Dictionary<String, System.Object>());
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
                        if (_properties[SerializationID].ContainsKey(info.Name))
                        {
                            info.SetValue(this, _properties[SerializationID][info.Name]);
                        }
                    }
                    else
                    {
                        // Does the field have a SerializeField attribute?
                        SerializeField[] serializeFields =
                            info.GetCustomAttributes(typeof(SerializeField), false) as SerializeField[];

                        if (serializeFields == null || serializeFields.Length <= 0) continue;

                        if (_properties[SerializationID].ContainsKey(info.Name))
                        {
                            info.SetValue(this, _properties[SerializationID][info.Name]);
                        }
                    }
                }
            }
        }
    }
}