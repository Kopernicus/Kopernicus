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
using Kopernicus.Components.ModularComponentSystem;
using Kopernicus.ConfigParser.Attributes;
using Kopernicus.ConfigParser.Enumerations;
using Kopernicus.ConfigParser.Interfaces;

namespace Kopernicus.Configuration.Parsing
{
    [RequireConfigType(ConfigType.Node)]
    public abstract class ComponentLoader<T> : ICreatable<IComponent<T>>, ITypeParser<IComponent<T>>
        where T : IComponentSystem<T>
    {
        /// <summary>
        /// The component that is being loaded
        /// </summary>
        public IComponent<T> Value { get; set; }

        public virtual void Create(IComponent<T> value)
        {
            Value = value;
        }

        public abstract void Create();
    }

    [RequireConfigType(ConfigType.Node)]
    public class ComponentLoader<TSystem, TComponent> : ComponentLoader<TSystem>, ICreatable<TComponent>,
        ITypeParser<TComponent>
        where TSystem : IComponentSystem<TSystem>
        where TComponent : IComponent<TSystem>
    {
        public new TComponent Value
        {
            get { return (TComponent)base.Value; }
            set { base.Value = value; }
        }

        public void Create(TComponent value)
        {
            Value = value;
        }

        public override void Create()
        {
            Value = Activator.CreateInstance<TComponent>();
        }
    }
}