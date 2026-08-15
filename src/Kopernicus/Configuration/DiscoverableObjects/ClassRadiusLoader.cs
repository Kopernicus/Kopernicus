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

using System.Diagnostics.CodeAnalysis;
using Kopernicus.ConfigParser.Attributes;
using Kopernicus.ConfigParser.Enumerations;

namespace Kopernicus.Configuration.DiscoverableObjects
{
    // Radius configuration for asteroid size classes.
    //
    // Normally, the asteroid size multipliers is controlled by the stock prefab for each individual
    // size class. This config loader allows you to specify what radii you want for each asteroid
    // class, then Kopernicus take care of making that apply to any asteroids spawned from that
    // class.
    //
    // Actually picking a number is still done by stock code and unset classes keep the same scaling as stock.
    [RequireConfigType(ConfigType.Node)]
    [SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    public class ClassRadiusLoader
    {
        [ParserTarget("A")]
        public Location.RandomRangeLoader A { get; set; }

        [ParserTarget("B")]
        public Location.RandomRangeLoader B { get; set; }

        [ParserTarget("C")]
        public Location.RandomRangeLoader C { get; set; }

        [ParserTarget("D")]
        public Location.RandomRangeLoader D { get; set; }

        [ParserTarget("E")]
        public Location.RandomRangeLoader E { get; set; }

        [ParserTarget("F")]
        public Location.RandomRangeLoader F { get; set; }

        [ParserTarget("G")]
        public Location.RandomRangeLoader G { get; set; }

        [ParserTarget("H")]
        public Location.RandomRangeLoader H { get; set; }

        [ParserTarget("I")]
        public Location.RandomRangeLoader I { get; set; }

        // The range configured for a class, or null if that class wasn't given one
        public Location.RandomRangeLoader Get(UntrackedObjectClass sizeClass)
        {
            switch (sizeClass)
            {
                case UntrackedObjectClass.A:
                    return A;
                case UntrackedObjectClass.B:
                    return B;
                case UntrackedObjectClass.C:
                    return C;
                case UntrackedObjectClass.D:
                    return D;
                case UntrackedObjectClass.E:
                    return E;
                case UntrackedObjectClass.F:
                    return F;
                case UntrackedObjectClass.G:
                    return G;
                case UntrackedObjectClass.H:
                    return H;
                case UntrackedObjectClass.I:
                    return I;
                default:
                    return null;
            }
        }
    }
}
