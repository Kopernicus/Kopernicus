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

// At some point (seems to be in KSP 1.4), a call to PQS.ResetLaunchsitePlacementRender() was added in PQS.ResetSphere().
// I believe this was left in the stock code by mistake, maybe as a leftover of some reverted debug or in-progress change, because :
// - The method has a `SetupLaunchsitePlacementRender()` counterpart that is never called
// - Some of what it does just doesn't make  sense, and does make sense is already done elsewhere in a more logical codepath.
// - There is no relation whatsoever between what its name suggest it should be doing, and what it is actually doing.
// But, notably, that method always force PQSLandControl.createColors and PQSLandControl.createScatter to true
// Which result in those values being set to false in a Kopernicus body config to be ignored, notably resulting in
// LandClass.color values to be applied when they shouldn't, resulting in arbitrary coloring of the terrain.

// There is very long history of "poke at it with a stick" workarounds to try to fix that in Kopernicus :
// - Release 1.9.1-1 (2018 !) : https://github.com/Kopernicus/Kopernicus/commit/3ceffb3ba6cde4b9357b5b027ceb04c5763652f8
// - Release 118 : https://github.com/Kopernicus/Kopernicus/blob/06bd3bf9d188a19fccd87339a241e933bdc8d99e/src/Kopernicus/Components/PQSLandControlFixer.cs#L64
// - Release 132 : https://github.com/Kopernicus/Kopernicus/blame/a61070896306527b5604796710af8b27f712481f/build/KSP19PLUS/GameData/Kopernicus/Config/ColorFix.cfg
// - Release 144 : https://github.com/Kopernicus/Kopernicus/blame/e74d43c3dd0208ee69fbf29f5690e9f6abd2501f/src/Kopernicus/Configuration/ModLoader/LandControl.cs#L734-L755
// - Release 155 : https://github.com/Kopernicus/Kopernicus/commit/18bdfa9215ea917b8db16e248d818bd330a4b11c

// This patch is a fix to the root issue by simply removing the call to PQS.ResetLaunchsitePlacementRender() in PQS.ResetSphere().
// Contrary to previous solutions, it prevent PQSLandControl.OnVertexBuild() from running a quite slow lerp and simplex noise sampling
// for every PQ vertex when createColors is set to false, which is usually the case on modded bodies.

using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace Kopernicus
{
    [HarmonyPatch(typeof(PQS), nameof(PQS.ResetSphere))]
    static class PQSLandControlFixer
    {
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            MethodInfo target = AccessTools.Method(typeof(PQS), "ResetLaunchsitePlacementRender");

            foreach (CodeInstruction il in instructions)
            {
                if (il.opcode == OpCodes.Call && ReferenceEquals(il.operand, target))
                {
                    il.opcode = OpCodes.Pop;
                    il.operand = null;
                }
                yield return il;
            }
        }
    }
}
