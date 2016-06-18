/**
 * Kopernicus Planetary System Modifier
 * ====================================
 * Created by: BryceSchroeder and Teknoman117 (aka. Nathaniel R. Lewis)
 * Maintained by: Thomas P., NathanKell and KillAshley
 * Additional Content by: Gravitasi, aftokino, KCreator, Padishar, Kragrathea, OvenProofMars, zengei, MrHappyFace
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
 
using UnityEngine;

namespace Kopernicus
{
    namespace Configuration
    {
        namespace ModLoader
        {
            [RequireConfigType(ConfigType.Node)]
            public class ModLoader<T> : BaseLoader where T : PQSMod
            {
                // The mod loader must always be able to return a mod
                public T mod { get; set; }

                // Mod loader provides basic PQS mod loading functions
                [ParserTarget("order")]
                public NumericParser<int> order
                {
                    get { return mod.order; }
                    set { mod.order = value; }
                }

                // Mod loader provides basic PQS mod loading functions
                [ParserTarget("enabled")]
                public NumericParser<bool> enabled
                {
                    get { return mod.modEnabled; }
                    set { mod.modEnabled = value; }
                }

                // Creates the a PQSMod of type T
                public virtual void Create()
                {
                    mod = new GameObject(typeof(T).Name.Replace("PQSMod_", "").Replace("PQS", "")).AddComponent<T>();
                    mod.transform.parent = generatedBody.pqsVersion.transform;
                    mod.sphere = generatedBody.pqsVersion;
                    mod.gameObject.layer = Constants.GameLayers.LocalSpace;
                }

                // Grabs a PQSMod of type T from a parameter
                public virtual void Create(T _mod)
                {
                    mod = _mod;
                    mod.transform.parent = generatedBody.pqsVersion.transform;
                    mod.sphere = generatedBody.pqsVersion;
                    mod.gameObject.layer = Constants.GameLayers.LocalSpace;
                }

                // Creates the a PQSMod of type T with given PQS
                public virtual void Create(PQS pqsVersion)
                {
                    mod = new GameObject(typeof(T).Name.Replace("PQSMod_", "").Replace("PQS", "")).AddComponent<T>();
                    mod.transform.parent = pqsVersion.transform;
                    mod.sphere = pqsVersion;
                    mod.gameObject.layer = Constants.GameLayers.LocalSpace;
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

