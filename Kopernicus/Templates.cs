using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Kopernicus
{
    public class Templates
    {
        static public Templates instance = null;

        static public bool loadFinished = false;

        public static Mesh refGeosphere = null;

        public static bool finalizeOrbits = false;
        public static double SOIMinRadiusMult = 2.0d;
        public static double SOIMinAltitude = 40000d;
        
        // for loading only one each
        public Dictionary<string, MapSO> mapsGray;
        public Dictionary<string, MapSO> mapsRGB;
        public List<MapSO> origMapSOs;
        public List<Texture> origTextures;

        public Texture origKerbinDiff;
        public Texture origKerbinBump;
        public Texture origMunDiff;
        public Texture origMunBump;

        public double epoch = double.NaN;

        public List<string> finalizeBodies;
        
        public Templates()
        {
            instance = this;

            mapsGray = new Dictionary<string, MapSO>();
            mapsRGB = new Dictionary<string, MapSO>();
            origMapSOs = new List<MapSO>();
            origTextures = new List<Texture>();
            GetUsedLists(origMapSOs, origTextures, PSystemManager.Instance.systemPrefab.rootBody);

            PSystemBody kerbin = Utility.FindBody(PSystemManager.Instance.systemPrefab.rootBody, "Kerbin");
            if (kerbin.scaledVersion != null)
            {
                origKerbinDiff = kerbin.scaledVersion.renderer.material.GetTexture("_MainTex");
                origKerbinBump = kerbin.scaledVersion.renderer.material.GetTexture("_BumpMap");
            }

            PSystemBody mun = Utility.FindBody(PSystemManager.Instance.systemPrefab.rootBody, "Mun");
            if (mun.scaledVersion != null)
            {
                origMunDiff = mun.scaledVersion.renderer.material.GetTexture("_MainTex");
                origMunBump = mun.scaledVersion.renderer.material.GetTexture("_BumpMap");
            }

            // get reference geosphere
            // We need to get the body for Jool (to steal it's mesh)
            PSystemBody Jool = Utility.FindBody(PSystemManager.Instance.systemPrefab.rootBody, "Jool");

            // Return it's mesh
            refGeosphere = Jool.scaledVersion.GetComponent<MeshFilter>().sharedMesh;



            finalizeBodies = new List<string>();
        }
        static public void GetUsedLists(List<MapSO> mapList, List<Texture> texList, PSystemBody body)
        {
            // get the biome map
            if (body.celestialBody.BiomeMap != null)
                mapList.Add(body.celestialBody.BiomeMap);

            // get any MapSOs in PQSs
            if (body.pqsVersion != null)
            {
                AddMapSOs(mapList, body.pqsVersion); // main PQS

                // get other PQSs (like ocean)
                PQS[] pqss = body.pqsVersion.GetComponentsInChildren<PQS>(true);
                foreach(PQS p in pqss)
                    if(p != body.pqsVersion)
                        AddMapSOs(mapList, p);

            }
            if (body.scaledVersion != null)
                AddTexes(texList, body.scaledVersion);

            // Recurse
            foreach (PSystemBody child in body.children)
                GetUsedLists(mapList, texList, child);
        }

        private static void AddMapSOs(List<MapSO> list, PQS pqs)
        {
            PQSMod[] mods = pqs.GetComponentsInChildren<PQSMod>(true) as PQSMod[];
            foreach (PQSMod m in mods)
            {
                foreach (FieldInfo fi in m.GetType().GetFields())
                {
                    // this _should_ get everything derived from it.
                    MapSO val = fi.GetValue(m) as MapSO;
                    if(val != null)
                        if(!list.Contains(val))
                            list.Add(val);
                }
            }
        }
        private static void AddTexes(List<Texture> list, GameObject scaledVersion)
        {
            Texture tex = null;
            /*tex = scaledVersion.renderer.material.GetTexture("_ResourceMap");
            if (tex != null && !origTextures.Contains(tex))
                list.Add(tex);*/
            tex = scaledVersion.renderer.material.GetTexture("_MainTex");
            if (tex != null && !list.Contains(tex))
                list.Add(tex);
            tex = scaledVersion.renderer.material.GetTexture("_BumpMap");
            if (tex != null && !list.Contains(tex))
                list.Add(tex);
            // ignore PQS Texture2Ds.
        }
    }
}
