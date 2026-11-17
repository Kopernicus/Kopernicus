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

// KSP Retooled supports a home world that orbits a planet by itself, so none of this is built for it.
#if !KSP_RETOOLED
using System.Collections.Generic;
using Kopernicus.Components;

namespace Kopernicus.Patches;

/// <summary>
/// Walks the tree of celestial bodies for the patches that let the home world be a body at any depth, such as a moon.
/// </summary>
internal static class BodyHierarchy
{
    /// <summary>
    /// Whether the running game lacks its own support for a home world that orbits a planet. That is stock KSP up to
    /// 1.12.5, the last stock release.
    /// </summary>
    public static bool GameNeedsPatches
    {
        get
        {
            if (Versioning.version_major != 1)
            {
                return Versioning.version_major < 1;
            }

            if (Versioning.version_minor != 12)
            {
                return Versioning.version_minor < 12;
            }

            return Versioning.Revision <= 5;
        }
    }

    /// <summary>
    /// Makes <paramref name="body"/> the body the others are placed around, by reversing its orbit driver and the
    /// driver of every body above it.
    /// </summary>
    /// <remarks>
    /// A reversed driver places its reference body instead of its own body. The update walk only reaches a reversed
    /// body through a reversed parent, so the whole chain up to the root has to be reversed together.
    /// </remarks>
    public static void ReverseChain(CelestialBody body)
    {
        OrbitDriver driver = body.orbitDriver;
        while (driver != null)
        {
            driver.reverse = true;
            CelestialBody parent = driver.referenceBody;
            driver = parent != null ? parent.orbitDriver : null;
        }
    }

    /// <summary>
    /// Returns the body the update walk starts from: the deepest body reached from <paramref name="root"/> through
    /// reversed drivers, or <paramref name="root"/> when nothing is reversed.
    /// </summary>
    /// <remarks>
    /// A reversed body under parents that are not reversed would never be updated, so those parents are reversed on
    /// the way down.
    /// </remarks>
    public static CelestialBody FindUpdateRoot(CelestialBody root)
    {
        while (true)
        {
            CelestialBody next = ReversedBranch(root);
            if (next == null)
            {
                return root;
            }

            next.orbitDriver.reverse = true;
            root = next;
        }
    }

    /// <summary>
    /// Returns the orbit <paramref name="body"/> follows around its star: its own orbit for a planet, and the orbit of
    /// the planet it belongs to for a moon. Returns null for a body that orbits nothing.
    /// </summary>
    public static Orbit SolarOrbit(CelestialBody body)
    {
        while (body.orbitDriver != null)
        {
            CelestialBody parent = body.orbitDriver.referenceBody;
            if (parent == null || parent.orbitDriver == null || IsStar(parent))
            {
                break;
            }

            body = parent;
        }

        return body.orbit;
    }

    /// <summary>
    /// Returns the orbit that sets the length of the solar day of <paramref name="body"/>, or null when the body keeps
    /// its sidereal day: it orbits nothing, or it turns once a year or slower.
    /// </summary>
    public static Orbit SolarDayOrbit(CelestialBody body)
    {
        Orbit solarOrbit = SolarOrbit(body);
        if (solarOrbit == null || solarOrbit.period - body.rotationPeriod <= 0.01)
        {
            return null;
        }

        return solarOrbit;
    }

    // A star that orbits another star still counts as the star of the bodies around it
    private static bool IsStar(CelestialBody body)
    {
        List<KopernicusStar> stars = KopernicusStar.Stars;
        if (stars == null)
        {
            return false;
        }

        for (int i = 0; i < stars.Count; i++)
        {
            if (stars[i] != null && stars[i].sun == body)
            {
                return true;
            }
        }

        return false;
    }

    private static CelestialBody ReversedBranch(CelestialBody parent)
    {
        int count = parent.orbitingBodies.Count;
        for (int i = 0; i < count; i++)
        {
            CelestialBody child = parent.orbitingBodies[i];
            if (child.orbitDriver != null && (child.orbitDriver.reverse || ReversedBranch(child) != null))
            {
                return child;
            }
        }

        return null;
    }
}
#endif
