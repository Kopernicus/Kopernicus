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
using UnityEngine;

namespace Kopernicus
{
    namespace Configuration
    {
        namespace ModLoader
        {
            [RequireConfigType(ConfigType.Node)]
            public class ModLoader<T> : BaseLoader, IModLoader, IPatchable, ICreatable<T>, ITypeParser<T> where T : PQSMod
            {
                // The mod loader must always be able to return a mod
                public T mod { get; set; }

                // The mod loader must always be able to return a mod
                T ITypeParser<T>.Value
                {
                    get { return mod; }
                    set { mod = value; }
                }

                // The mod loader must always be able to return a mod
                PQSMod IModLoader.Mod
                {
                    get { return mod; }
                    set { mod = (T) value; }
                }

                /// <summary>
                /// Returns the currently edited PQS
                /// </summary>
                protected PQS pqsVersion
                {
                    get
                    {
                        try
                        {
                            return Parser.GetState<PQS>("Kopernicus:pqsVersion");
                        }
                        catch
                        {
                            return null;
                        }
                    }
                }

                // Mod loader provides basic PQS mod loading functions
                [ParserTarget("order")]
                public NumericParser<Int32> order
                {
                    get { return mod.order; }
                    set { mod.order = value; }
                }

                // Mod loader provides basic PQS mod loading functions
                [ParserTarget("enabled")]
                public NumericParser<Boolean> enabled
                {
                    get { return mod.modEnabled; }
                    set { mod.modEnabled = value; }
                }

                // The name of the PQSMod
                [ParserTarget("name")]
                public String name
                {
                    get { return mod.name; }
                    set { mod.name = value; }
                }

                // Creates the a PQSMod of type T
                void ICreatable.Create()
                {
                    Create(pqsVersion);
                }

                // Creates the a PQSMod of type T
                void ICreatable<T>.Create(T value)
                {
                    Create(value, pqsVersion);
                }

                // Creates the a PQSMod of type T with given PQS
                public virtual void Create(PQS pqsVersion)
                {
                    mod = new GameObject(typeof(T).Name.Replace("PQSMod_", "").Replace("PQS", "")).AddComponent<T>();
                    mod.transform.parent = pqsVersion.transform;
                    mod.sphere = pqsVersion;
                    mod.gameObject.layer = Constants.GameLayers.LocalSpace;
                }

                // Creates the a PQSMod of type T with given PQS
                void IModLoader.Create(PQSMod _mod, PQS pqsVersion)
                {
                    Create((T)_mod, pqsVersion);
                }

                // Grabs a PQSMod of type T from a parameter with a given PQS
                public virtual void Create(T _mod, PQS pqsVersion)
                {
                    mod = _mod;
                    mod.transform.parent = pqsVersion.transform;
                    mod.sphere = pqsVersion;
                    mod.gameObject.layer = Constants.GameLayers.LocalSpace;
                }
            }
        }
    }
}

