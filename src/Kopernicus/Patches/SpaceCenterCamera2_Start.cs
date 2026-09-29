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
using HarmonyLib;
using UnityEngine;

namespace Kopernicus.Patches;

[HarmonyPatch(typeof(SpaceCenterCamera2), "Start")]
static class SpaceCenterCamera2_Start
{
    static bool Prefix(SpaceCenterCamera2 __instance)
    {
        CelestialBody homeBody = FlightGlobals.GetHomeBody();
        __instance.pqsName = homeBody.pqsController.gameObject.name;
        __instance.pqs = homeBody.pqsController;
        __instance.t = __instance.transform;

        GameEvents.onGameSceneLoadRequested.Add(__instance.OnSceneSwitch);

        PQSCity ksc = homeBody.pqsController.transform.Find("KSC").GetComponent<PQSCity>();
        __instance.altitudeInitial = (float)(ksc.planetRelativePosition.magnitude - homeBody.Radius) * -1f;

        __instance.initialPosition = __instance.pqs.transform.Find(__instance.initialPositionTransformName);
        if (__instance.initialPosition == null)
        {
            Debug.LogError("SpaceCenterCamera: Cannot find transform of name '" + __instance.initialPositionTransformName + "'");
            return false;
        }
        __instance.t.NestToParent(__instance.initialPosition);
        __instance.cameraTransform = new GameObject("CameraTransform").transform;
        __instance.cameraTransform.NestToParent(__instance.transform);
        FlightCamera.fetch.transform.NestToParent(__instance.cameraTransform);
        FlightCamera.fetch.updateActive = false;
        FlightCamera.fetch.gameObject.SetActive(value: true);
        __instance.ResetCamera();
        __instance.srfPivot = SurfaceObject.Create(__instance.initialPosition.gameObject, FlightGlobals.currentMainBody, 3, KFSMUpdateMode.FIXEDUPDATE);
        return false;
    }
}
