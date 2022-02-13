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
using Kopernicus.ConfigParser.Attributes;
using Kopernicus.ConfigParser.Enumerations;
using Kopernicus.ConfigParser.Interfaces;
using LibNoise;

namespace Kopernicus.Configuration.NoiseLoader
{
    [RequireConfigType(ConfigType.Node)]
    [SuppressMessage("ReSharper", "MemberCanBeProtected.Global")]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public class NoiseLoader<T> : INoiseLoader, ICreatable<T>, ITypeParser<T> where T : IModule
    {
        // The noise we are loading
        public T Noise { get; set; }

        // The noise we are loading
        T ITypeParser<T>.Value
        {
            get { return Noise; }
            set { Noise = value; }
        }

        // The noise we are loading
        IModule INoiseLoader.Noise
        {
            get { return Noise; }
            set { Noise = (T)value; }
        }

        [SuppressMessage("ReSharper", "UnusedParameter.Global")]
        public virtual void Apply(ConfigNode node)
        {
            Noise = Activator.CreateInstance<T>();
        }

        [SuppressMessage("ReSharper", "UnusedParameter.Global")]
        public virtual void PostApply(ConfigNode node)
        {

        }

        public void Create(T value)
        {
            Noise = value;
        }

        public void Create()
        {
            Apply(null);
        }

        public void Create(IModule value)
        {
            Create((T)value);
        }
    }
}