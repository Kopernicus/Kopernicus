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
    // Physical radius ranges per size class, in meters.
    //
    // Stock derives the radius of a spawned object from the ProceduralAsteroid prefab that its
    // UntrackedObjectClass maps to - Resources.Load("Procedural/PA_<class>") - scaled by the
    // minRadiusMultiplier/maxRadiusMultiplier fields on ModuleAsteroid. Those multipliers live on
    // the part, so every class shares them, and the radii baked into the prefabs (~3.2m for class
    // A up to ~193m for class I) are the only thing separating one class from another. Once the
    // multiplier spread is wider than the ~1.67x gap between adjacent classes, the classes overlap
    // and the class letter stops saying anything useful about the object's size.
    //
    // Giving a class a range here lets a config state the radius it actually wants. Kopernicus
    // divides out the prefab radius and hands stock the multipliers that produce that range, so
    // the roll still happens in stock code off the persisted seed. Classes left unset are not
    // touched at all and keep the stock scaling.
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
