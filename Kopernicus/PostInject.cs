using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Kopernicus
{
    [KSPAddon(KSPAddon.Startup.PSystemSpawn, false)]
    public class PostInject : MonoBehaviour
    {
        private const string rootNodeName = "Kopernicus";
        private const string finalizeName = "Finalize";
        private const string bodyNodeName = "Body";

        static private CelestialBody GetBody(string bodyName)
        {
            foreach (CelestialBody b in FlightGlobals.Bodies)
                if (b.name.Equals(bodyName))
                    return b;
            return null;
        }
        private void PatchPQS(PQS pqs, ConfigNode node)
        {
            if (node.HasValue("radius"))
                double.TryParse(node.GetValue("radius"), out pqs.radius);

            PQSMod[] mods = pqs.transform.GetComponentsInChildren<PQSMod>(true).Where(m => m.sphere == pqs) as PQSMod[];
            foreach (var m in mods)
            {
                if (m is PQSCity)
                {
                    PQSCity mod = (PQSCity)m;
                    mod.OnSetup();
                    mod.OnPostSetup();
                    SpaceCenter.Instance.transform.localPosition = mod.transform.localPosition;
                    SpaceCenter.Instance.transform.localRotation = mod.transform.localRotation;
                }
                if (m is PQSMod_MapDecal)
                {
                    PQSMod_MapDecal mod = (PQSMod_MapDecal)m;
                    mod.OnSetup();
                    mod.OnPostSetup();
                }
                if (m is PQSMod_MapDecalTangent)
                {
                    PQSMod_MapDecalTangent mod = (PQSMod_MapDecalTangent)m;
                    mod.OnSetup();
                    mod.OnPostSetup();
                }
            }
            try
            {
                pqs.RebuildSphere();
            }
            catch (Exception e)
            {
                Logger.Default.Log("Rebuild sphere for " + node.name + " failed: " + e.Message);
            }
        }
        private bool PatchBody(ConfigNode node)
        {
            if (node != null && node.HasValue("name"))
            {
                CelestialBody body = GetBody(node.GetValue("name"));
                if(node.HasNode("PQS"))
                    PatchPQS(body.pqsController, node.GetNode("PQS"));
                return true;
            }
            return false;
        }
        private void FinalizeOrbits()
        {
            foreach (CelestialBody body in FlightGlobals.fetch.bodies)
            {
                if (body.orbitDriver != null)
                {
                    if (body.referenceBody != null)
                    {
                        body.hillSphere = body.orbit.semiMajorAxis * (1.0 - body.orbit.eccentricity) * Math.Pow(body.Mass / body.orbit.referenceBody.Mass, 1 / 3);
                        body.sphereOfInfluence = body.orbit.semiMajorAxis * Math.Pow(body.Mass / body.orbit.referenceBody.Mass, 0.4);
                        if (body.sphereOfInfluence < body.Radius * 1.5 || body.sphereOfInfluence < body.Radius + 20000.0)
                            body.sphereOfInfluence = Math.Max(body.Radius * 1.5, body.Radius + 20000.0); // sanity check

                        // period should be (body.Mass + body.referenceBody.Mass) at the end, not just ref body, but KSP seems to ignore that bit so I will too.
                        body.orbit.period = 2 * Math.PI * Math.Sqrt(Math.Pow(body.orbit.semiMajorAxis, 2) / 6.674E-11 * body.orbit.semiMajorAxis / (body.referenceBody.Mass));

                        if (body.orbit.eccentricity <= 1.0)
                        {
                            body.orbit.meanAnomaly = body.orbit.meanAnomalyAtEpoch;
                            body.orbit.orbitPercent = body.orbit.meanAnomalyAtEpoch / (Math.PI * 2);
                            body.orbit.ObTAtEpoch = body.orbit.orbitPercent * body.orbit.period;
                        }
                        else
                        {
                            // ignores this body's own mass for this one...
                            body.orbit.meanAnomaly = body.orbit.meanAnomalyAtEpoch;
                            body.orbit.ObT = Math.Pow(Math.Pow(Math.Abs(body.orbit.semiMajorAxis), 3.0) / body.orbit.referenceBody.gravParameter, 0.5) * body.orbit.meanAnomaly;
                            body.orbit.ObTAtEpoch = body.orbit.ObT;
                        }
                    }
                    else
                    {
                        body.sphereOfInfluence = double.PositiveInfinity;
                        body.hillSphere = double.PositiveInfinity;
                    }
                }
                try
                {
                    body.CBUpdate();
                }
                catch (Exception e)
                {
                    print("CBUpdate for " + body.name + " failed: " + e.Message);
                }
            }
        }

        private void UpdateMenuTex()
        {
            PSystemBody home = Utility.FindHomeBody(PSystemManager.Instance.systemPrefab.rootBody);
            Texture homeMain = home.scaledVersion.renderer.material.GetTexture("_MainTex");
            Texture homeBump = home.scaledVersion.renderer.material.GetTexture("_BumpMap");
            Material[] mats = Resources.FindObjectsOfTypeAll<Material>();
            foreach (Material m in mats)
            {
                if (m.GetTexture("_MainTex") == Templates.instance.origKerbinTex)
                    m.SetTexture("_MainTex", homeMain);
                if (m.GetTexture("_BumpMap") == Templates.instance.origKerbinBump)
                    m.SetTexture("_BumpMap", homeBump);
            }
        }

        private void RemoveUnused()
        {
            Logger.Default.Log("Removing unused MapSOs and textures");
            List<MapSO> usedMaps = new List<MapSO>();
            List<Texture> usedTex = new List<Texture>();
            Templates.GetUsedLists(usedMaps, usedTex, PSystemManager.Instance.systemPrefab.rootBody);

            foreach (MapSO map in Templates.instance.origMapSOs)
                if (!usedMaps.Contains(map))
                {
                    MapSO.DestroyImmediate(map);
                    Logger.Default.Log("Removed MapSO " + map.name + " with mapname " + map.MapName);
                }

            foreach (Texture tex in Templates.instance.origTextures)
                if (!usedTex.Contains(tex))
                {
                    Texture.DestroyImmediate(tex);
                    Logger.Default.Log("Removed Texture " + tex.name);
                }

        }
        public void Start()
        {
            // Get the data node
            ConfigNode rootConfig = GameDatabase.Instance.GetConfigs(rootNodeName)[0].config;

            // Patch bodies
            if (rootConfig != null)
            {
                if (rootConfig.HasNode(finalizeName))
                {
                    Logger.Default.Log("**** Finalizing things");

                    rootConfig = rootConfig.GetNode(finalizeName);
                    bool finalizeOrbits = false;
                    bool removeUnused = false;
                    if (rootConfig.HasValue("finalizeOrbits"))
                        bool.TryParse(rootConfig.GetValue("finalizeOribts"), out finalizeOrbits);
                    if (rootConfig.HasValue("removeUnused"))
                        bool.TryParse(rootConfig.GetValue("removeUnused"), out removeUnused);

                    // Update the bodies
                    foreach (ConfigNode node in rootConfig.GetNodes(bodyNodeName))
                        if (!PatchBody(node))
                        {
                            if(node.HasValue("name"))
                            {
                                Logger.Default.Log("Failed to patch " + bodyNodeName + " " + node.GetValue("name"));
                            }
                            else
                            {
                                Logger.Default.Log("Failed to patch " + bodyNodeName + " node with no name");
                            }
                        }
                    if (finalizeOrbits)
                        FinalizeOrbits();
                    
                    UpdateMenuTex();

                    if(removeUnused)
                        RemoveUnused();

                    Logger.Default.Log("Finalization pass done!");
                }
            }
        }
    }
}
