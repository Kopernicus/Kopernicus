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
