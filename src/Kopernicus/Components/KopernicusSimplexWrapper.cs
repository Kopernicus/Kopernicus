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
using System.Reflection;

namespace Kopernicus.Components
{
    /// <summary>
    /// A wrapper for SimplexWrapper to provide a storage for the seed
    /// </summary>
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public class KopernicusSimplexWrapper : PQSMod_VertexPlanet.SimplexWrapper
    {
        private Int32 _seed;

        public Int32 Seed
        {
            get { return _seed; }
            set
            {
                _seed = value;
                Setup(_seed);
            }
        }

        private readonly PropertyInfo _simplex = typeof(PQSMod_VertexPlanet.SimplexWrapper).GetProperty(nameof(simplex), BindingFlags.Public
            | BindingFlags.Instance
            | BindingFlags.DeclaredOnly);

        public new Simplex simplex
        {
            get { return (Simplex)_simplex.GetValue(this, null); }
            set { _simplex.SetValue(this, value, null); }
        }

        public KopernicusSimplexWrapper(PQSMod_VertexPlanet.SimplexWrapper copyFrom) : base(copyFrom)
        {
            simplex = copyFrom.simplex;
        }

        public KopernicusSimplexWrapper(Double deformity, Double octaves, Double persistance, Double frequency) :
            base(deformity, octaves, persistance, frequency)
        {
        }
    }
}
