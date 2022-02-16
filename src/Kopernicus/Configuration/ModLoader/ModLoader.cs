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
using Kopernicus.ConfigParser;
using Kopernicus.ConfigParser.Attributes;
using Kopernicus.ConfigParser.BuiltinTypeParsers;
using Kopernicus.ConfigParser.Enumerations;
using Kopernicus.ConfigParser.Interfaces;
using Kopernicus.Configuration.Parsing;
using Kopernicus.Constants;
using Kopernicus.UI;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Kopernicus.Configuration.ModLoader
{
    [RequireConfigType(ConfigType.Node)]
    [SuppressMessage("ReSharper", "MemberCanBeProtected.Global")]
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public abstract class ModLoader<T> : BaseLoader, IModLoader, IPatchable, ICreatable<PQSMod>,
        ICreatable<CelestialBody>, ITypeParser<T> where T : PQSMod
    {
        // The mod loader must always be able to return a mod
        public T Mod { get; set; }

        // The mod loader must always be able to return a mod
        T ITypeParser<T>.Value
        {
            get { return Mod; }
            set { Mod = value; }
        }

        // The mod loader must always be able to return a mod
        PQSMod IModLoader.Mod
        {
            get { return Mod; }
            set { Mod = (T)value; }
        }

        /// <summary>
        /// Returns the currently edited PQS
        /// </summary>
        protected PQS PqsVersion
        {
            get
            {
                if (_pqsVersionOverride != null)
                {
                    return _pqsVersionOverride;
                }

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

        private PQS _pqsVersionOverride;

        // Mod loader provides basic PQS mod loading functions
        [ParserTarget("order")]
        public NumericParser<Int32> Order
        {
            get { return Mod.order; }
            set { Mod.order = value; }
        }

        // Mod loader provides basic PQS mod loading functions
        [ParserTarget("enabled")]
        public NumericParser<Boolean> Enabled
        {
            get { return Mod.modEnabled; }
            set { Mod.modEnabled = value; }
        }

        // The name of the PQSMod
        [ParserTarget("name")]
        public String name
        {
            get { return Mod.name; }
            set { Mod.name = value; }
        }

        [KittopiaDestructor]
        public void Destroy()
        {
            Object.Destroy(Mod);
        }

        // Creates the a PQSMod of type T
        void ICreatable.Create()
        {
            try
            {
                Create(PqsVersion);
            }
            catch
            {

            }
        }

        // Creates the a PQSMod of type T
        void ICreatable<PQSMod>.Create(PQSMod value)
        {
            try
            {
                Create((T)value, PqsVersion);
            }
            catch
            {

            }
        }

        // Creates the a PQSMod from the specified body
        void ICreatable<CelestialBody>.Create(CelestialBody value)
        {
            _pqsVersionOverride = value.pqsController;
            Create(PqsVersion);
        }

        // Creates the a PQSMod of type T with given PQS
        public virtual void Create(PQS pqsVersion)
        {
            Mod = new GameObject(typeof(T).Name.Replace("PQSMod_", "").Replace("PQS", "")).AddComponent<T>();
            Mod.transform.parent = pqsVersion.transform;
            Mod.sphere = pqsVersion;
            Mod.gameObject.layer = GameLayers.LOCAL_SPACE;
        }

        // Creates the a PQSMod of type T with given PQS
        void IModLoader.Create(PQSMod mod, PQS pqsVersion)
        {
            Create((T)mod, pqsVersion);
        }

        // Grabs a PQSMod of type T from a parameter with a given PQS
        public virtual void Create(T mod, PQS pqsVersion)
        {
            Mod = mod;
            Mod.transform.parent = pqsVersion.transform;
            Mod.sphere = pqsVersion;
            Mod.gameObject.layer = GameLayers.LOCAL_SPACE;
        }
    }
}

