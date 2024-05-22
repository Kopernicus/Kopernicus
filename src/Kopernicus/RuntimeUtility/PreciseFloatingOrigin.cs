using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Kopernicus.RuntimeUtility
{
    /// <summary>
    /// On all cases where the floating origin should be set back to the KSC (main menu, space center, tracking station and editor scenes),
    /// KSP is calling FloatingOrigin.SetOffset() with the float backed position of the KSC transform, resulting in a potentially very large
    /// offset from (0, 0, 0) when the floating origin was previously far from the KSC, in turn resulting in various rendering and physics
    /// glitches (most notable being the "garbled KSC effect").
    /// We fix that by using the double backed body position and KSC position to find the precise position of the KSC from anywhere.
    /// </summary>
    [HarmonyPatch]
    static class PreciseKSCFloatingOrigin
    {
        [HarmonyTargetMethods]
        static IEnumerable<MethodBase> TargetMethods()
        {
            yield return AccessTools.Method(typeof(PSystemSetup), "SetMainMenu");
            yield return AccessTools.Method(typeof(PSystemSetup), "SetSpaceCentre");
            yield return AccessTools.Method(typeof(PSystemSetup), "SetTrackingStation");
            yield return AccessTools.Method(typeof(PSystemSetup), "SetEditor");
        }

        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            MethodInfo m_FloatingOrigin_SetOffset = AccessTools.Method(typeof(FloatingOrigin), nameof(FloatingOrigin.SetOffset));
            MethodInfo m_SetFloatingOriginToKSC = AccessTools.Method(typeof(PreciseKSCFloatingOrigin), nameof(SetFloatingOriginToKSC));
            foreach (CodeInstruction instruction in instructions)
            {
                if (instruction.Calls(m_FloatingOrigin_SetOffset))
                    instruction.operand = m_SetFloatingOriginToKSC;

                yield return instruction;
            }
        }

        static void SetFloatingOriginToKSC(Vector3d notPrecisePos)
        {
            CelestialBody homeBody = FlightGlobals.GetHomeBody();
            PQSCity ksc = homeBody.pqsController.transform.Find("KSC").GetComponent<PQSCity>();
            Vector3d kscPos = homeBody.position + homeBody.rotation * ksc.planetRelativePosition;
            FloatingOrigin.SetOffset(kscPos);
        }
    }

    /// <summary>
    /// A similar issue occurs when switching to an existing vessel. FlightDriver.Start() update the orbit of every vessel,
    /// then position the floating origin to the transform position of the (future) active vessel. Due to float imprecision,
    /// if the offset between the current origin and the vessel position is large, this will result in the floating origin
    /// being initially positioned very far from the vessel (between 50 and 250 km for switching from the KSC to a vessel in
    /// Eeloo orbit). This will be corrected on the next frame and (as far as my tests have shown) before anythings physics
    /// related has been engaged, but I suspect this still is the source of some issues, especially with terrain/PQS.
    /// To get the precise vessel position, we use the same code as in OrbitDriver.updateFromParameters().
    /// </summary>
    [HarmonyPatch(typeof(FlightDriver), "Start")]
    static class PreciseFlightFloatingOrigin
    {
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            MethodInfo m_FloatingOrigin_SetOffset = AccessTools.Method(typeof(FloatingOrigin), nameof(FloatingOrigin.SetOffset));
            MethodInfo m_SetFloatingOriginToKSC = AccessTools.Method(typeof(PreciseFlightFloatingOrigin), nameof(SetFloatingOriginToActiveVessel));
            foreach (CodeInstruction instruction in instructions)
            {
                if (instruction.Calls(m_FloatingOrigin_SetOffset))
                    instruction.operand = m_SetFloatingOriginToKSC;

                yield return instruction;
            }
        }

        static void SetFloatingOriginToActiveVessel(Vector3d notPrecisePos)
        {
            OrbitDriver orbit = FlightGlobals.Vessels[FlightDriver.FocusVesselAfterLoad].orbitDriver;
            Vector3d comOffset = (QuaternionD)orbit.driverTransform.rotation * orbit.vessel.localCoM;
            Vector3d vesselPos = orbit.referenceBody.position + orbit.pos - comOffset;
            FloatingOrigin.SetOffset(vesselPos);
        }
    }
}
