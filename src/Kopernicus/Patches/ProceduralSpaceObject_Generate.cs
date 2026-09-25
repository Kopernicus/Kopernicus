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
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;

namespace Kopernicus.Patches;

// Records the physical radius of the space object being generated so the collider patches can see
// how big it actually is.
[HarmonyPatch]
static class ProceduralSpaceObject_Generate
{
    static IEnumerable<MethodBase> TargetMethods()
    {
        yield return AccessTools.Method(typeof(ProceduralAsteroid), nameof(ProceduralAsteroid.Generate));
        yield return AccessTools.Method(typeof(ProceduralComet), nameof(ProceduralComet.Generate));
    }

    static void Prefix(float radius)
    {
        SpaceObjectColliderUtil.GeneratingRadius = radius;
    }

    static void Finalizer()
    {
        SpaceObjectColliderUtil.GeneratingRadius = 0f;
    }
}
