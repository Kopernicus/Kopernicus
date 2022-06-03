using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;
using System.Runtime;
using KSP;
using KSP.IO;
using UnityEngine;

namespace Kopernicus.ShadowMan
{
    public struct EVEClouds2d
    {
        public Material Clouds2dMaterial;
        public MeshRenderer Clouds2dMeshRenderer;
        public Material CloudShadowMaterial;
    }

    public class EVEReflectionHandler
    {
        public Dictionary<String, List<EVEClouds2d> > EVEClouds2dDictionary = new Dictionary<String, List<EVEClouds2d>>();

        //how to make this detect when EVE re-applies though? need some kind of callback, is there one on EVE? maybe there is a C# way to add one?
        //doesn't seem to be a way to do this, just do it in the map eve clouds button
        //also make it so that the map eve clouds button causes the active planet to remap

        //map EVE CloudObjects to planet names
        //CloudObjects in EVE contain the 2d clouds and the volumetrics for a given layer on a given planet
        //Due to the way they are handled in EVE they don't directly reference their parent planet and the volumetrics are only created when the PQS is active
        //Map them here to facilitate accessing the volumetrics later
        public Dictionary<String, List<object>> EVECloudObjects = new Dictionary<String, List<object>>();
        public object EVEinstance;

        public EVEReflectionHandler()
        {
        }

        public void Start()
        {
            MapEVEClouds();
        }

        public void MapEVEClouds()
        {
            Utils.LogDebug("mapping EVE clouds");
            EVEClouds2dDictionary.Clear();
            EVECloudObjects.Clear();

            //find EVE base type
            Type EVEType = ReflectionUtils.getType("Atmosphere.CloudsManager");

            if (EVEType == null)
            {
                Utils.LogDebug("Eve assembly type not found");
                return;
            }
            else
            {
                Utils.LogDebug("Eve assembly type found");
            }

            Utils.LogDebug("Eve assembly version: " + EVEType.Assembly.GetName().ToString());

            const BindingFlags flags =  BindingFlags.FlattenHierarchy |  BindingFlags.NonPublic | BindingFlags.Public |
                BindingFlags.Instance | BindingFlags.Static;

            try
            {
                EVEinstance = EVEType.GetField("instance", flags).GetValue(null);
            }
            catch (Exception)
            {
                Utils.LogDebug("No EVE Instance found");
                return;
            }
            if (EVEinstance == null)
            {
                Utils.LogError("Failed grabbing EVE Instance");
                return;
            }
            else
            {
                Utils.LogInfo("Successfully grabbed EVE Instance");
            }

            IList objectList = EVEType.GetField ("ObjectList", flags).GetValue (EVEinstance) as IList;

            foreach (object _obj in objectList)
            {
#pragma warning disable REFL009 // The referenced member is not known to exist
                String body = _obj.GetType().GetField("body", flags).GetValue(_obj) as String;
#pragma warning restore REFL009 // The referenced member is not known to exist

                if (EVECloudObjects.ContainsKey(body))
                {
                    EVECloudObjects[body].Add(_obj);
                }
                else
                {
                    List<object> objectsList = new List<object>();
                    objectsList.Add(_obj);
                    EVECloudObjects.Add(body, objectsList);
                }

                object cloud2dObj;
                if (HighLogic.LoadedScene == GameScenes.MAINMENU)
                {
#pragma warning disable REFL009 // The referenced member is not known to exist
                    object cloudsPQS = _obj.GetType().GetField("cloudsPQS", flags).GetValue(_obj) as object;
#pragma warning restore REFL009 // The referenced member is not known to exist

                    if (cloudsPQS == null)
                    {
                        Utils.LogDebug("cloudsPQS not found for layer on planet :" + body);
                        continue;
                    }
#pragma warning disable REFL009 // The referenced member is not known to exist
                    cloud2dObj = cloudsPQS.GetType().GetField("mainMenuLayer", flags).GetValue(cloudsPQS) as object;
#pragma warning restore REFL009 // The referenced member is not known to exist
                }
                else
                {
#pragma warning disable REFL009 // The referenced member is not known to exist
                    cloud2dObj = _obj.GetType().GetField("layer2D", flags).GetValue(_obj) as object;
#pragma warning restore REFL009 // The referenced member is not known to exist
                }

                if (cloud2dObj == null)
                {
                    Utils.LogDebug("layer2d not found for layer on planet :" + body);
                    continue;
                }

#pragma warning disable REFL009 // The referenced member is not known to exist
                GameObject cloudmesh = cloud2dObj.GetType().GetField("CloudMesh", flags).GetValue(cloud2dObj) as GameObject;
#pragma warning restore REFL009 // The referenced member is not known to exist
                if (cloudmesh == null)
                {
                    Utils.LogDebug("cloudmesh null");
                    return;
                }

                Material shadowMaterial = null;

                //first try to get screenSpace shadow, if it doesn't work means we are on old EVE and grab shadowProjector
                //make sure to try this on old EVE

                object screenSpaceShadow = null;

                try
                {
#pragma warning disable REFL009 // The referenced member is not known to exist
                    screenSpaceShadow = cloud2dObj.GetType().GetField("screenSpaceShadow", flags).GetValue(cloud2dObj) as object;
#pragma warning restore REFL009 // The referenced member is not known to exist
                }
                catch (Exception) { }

                if (screenSpaceShadow != null)
                {
#pragma warning disable REFL009 // The referenced member is not known to exist
                    shadowMaterial = screenSpaceShadow.GetType().GetField("material", flags).GetValue(screenSpaceShadow) as Material;
#pragma warning restore REFL009 // The referenced member is not known to exist
                }
                else
                {
#pragma warning disable REFL009 // The referenced member is not known to exist
                    Projector shadowProjector = cloud2dObj.GetType().GetField("ShadowProjector", flags).GetValue(cloud2dObj) as Projector;
#pragma warning restore REFL009 // The referenced member is not known to exist

                    if (shadowProjector != null && shadowProjector.material != null)
                    {
                        shadowMaterial = shadowProjector.material;
                    }
                }

                if (EVEClouds2dDictionary.ContainsKey(body))
                {
                    EVEClouds2d clouds2d = new EVEClouds2d();
                    clouds2d.Clouds2dMeshRenderer = cloudmesh.GetComponent<MeshRenderer>();
                    clouds2d.Clouds2dMaterial = clouds2d.Clouds2dMeshRenderer.material;
                    clouds2d.CloudShadowMaterial = shadowMaterial;

                    EVEClouds2dDictionary[body].Add(clouds2d);

                    clouds2d.Clouds2dMaterial.renderQueue = 2999; //we might as well fix the EVE clouds renderqueue for 1.9 until official EVE fix
                }
                else
                {
                    List<EVEClouds2d> cloudsList = new List<EVEClouds2d>();

                    EVEClouds2d clouds2d = new EVEClouds2d();
                    clouds2d.Clouds2dMeshRenderer = cloudmesh.GetComponent<MeshRenderer>();
                    clouds2d.Clouds2dMaterial = clouds2d.Clouds2dMeshRenderer.material;
                    clouds2d.CloudShadowMaterial = shadowMaterial;

                    cloudsList.Add(clouds2d);
                    cloudmesh.GetComponent<MeshRenderer>().material.renderQueue = 2999;

                    EVEClouds2dDictionary.Add(body, cloudsList);
                }
                Utils.LogDebug("Detected EVE 2d cloud layer for planet: " + body);
            }
        }

        public void invokeClouds2dReassign(string celestialBodyName)
        {
            const BindingFlags flags = BindingFlags.FlattenHierarchy | BindingFlags.NonPublic | BindingFlags.Public |
                BindingFlags.Instance | BindingFlags.Static;

            foreach (object _obj in ShadowMan.Instance.eveReflectionHandler.EVECloudObjects[celestialBodyName])
            {
#pragma warning disable REFL009 // The referenced member is not known to exist
                object cloud2dObj = _obj.GetType ().GetField ("layer2D", flags).GetValue (_obj) as object;
#pragma warning restore REFL009 // The referenced member is not known to exist
                if (cloud2dObj == null)
                {
                    Utils.LogDebug(" layer2d not found for layer on planet: " + celestialBodyName);
                    continue;
                }

#pragma warning disable REFL009 // The referenced member is not known to exist
                bool cloud2dScaled = (bool)cloud2dObj.GetType ().GetField ("isScaled", flags).GetValue (cloud2dObj);
#pragma warning restore REFL009 // The referenced member is not known to exist

#pragma warning disable REFL009 // The referenced member is not known to exist
                MethodInfo scaledGetter = cloud2dObj.GetType ().GetProperty ("Scaled").GetGetMethod ();
#pragma warning restore REFL009 // The referenced member is not known to exist
#pragma warning disable REFL009 // The referenced member is not known to exist
                MethodInfo scaledSetter = cloud2dObj.GetType ().GetProperty ("Scaled").GetSetMethod ();
#pragma warning restore REFL009 // The referenced member is not known to exist

                //if in scaled mode, switch it to local then back to scaled, to set all the properties
                if (cloud2dScaled)
                    scaledSetter.Invoke(cloud2dObj, new object[] { !cloud2dScaled });

                scaledSetter.Invoke(cloud2dObj, new object[] { cloud2dScaled });

                //set the radius for use in the scatterer shader to have smooth scattering
#pragma warning disable REFL009 // The referenced member is not known to exist
                float radius = (float) cloud2dObj.GetType ().GetField ("radius", flags).GetValue (cloud2dObj);
#pragma warning restore REFL009 // The referenced member is not known to exist
#pragma warning disable REFL009 // The referenced member is not known to exist
                GameObject cloudmesh = cloud2dObj.GetType().GetField("CloudMesh", flags).GetValue(cloud2dObj) as GameObject;
#pragma warning restore REFL009 // The referenced member is not known to exist
                cloudmesh.GetComponent<MeshRenderer>().material.SetFloat("_Radius", radius);

            }
        }


        public void mapEVEVolumetrics(string celestialBodyName, List<Material> EVEvolumetrics)
        {
            Utils.LogDebug(" Mapping EVE volumetrics for planet: " + celestialBodyName);

            EVEvolumetrics.Clear();

            const BindingFlags flags =  BindingFlags.FlattenHierarchy |  BindingFlags.NonPublic | BindingFlags.Public |
                BindingFlags.Instance | BindingFlags.Static;

            if (EVECloudObjects.ContainsKey(celestialBodyName)) //EVECloudObjects contain both the 2d clouds and the volumetrics, here we extract the volumetrics
            {
                List<object> cloudObjs = ShadowMan.Instance.eveReflectionHandler.EVECloudObjects [celestialBodyName];

                foreach (object _obj in cloudObjs)
                {
                    try
                    {
#pragma warning disable REFL009 // The referenced member is not known to exist
                        object cloudsPQS = _obj.GetType ().GetField ("cloudsPQS", flags).GetValue (_obj) as object;
#pragma warning restore REFL009 // The referenced member is not known to exist
#pragma warning disable REFL009 // The referenced member is not known to exist
                        object layerVolume = cloudsPQS.GetType ().GetField ("layerVolume", flags).GetValue (cloudsPQS) as object;
#pragma warning restore REFL009 // The referenced member is not known to exist
                        if (ReferenceEquals(layerVolume, null))
                        {
                            Utils.LogDebug(" No volumetric cloud for layer on planet: " + celestialBodyName);
                            continue;
                        }

#pragma warning disable REFL009 // The referenced member is not known to exist
                        Material ParticleMaterial = layerVolume.GetType ().GetField ("ParticleMaterial", flags).GetValue (layerVolume) as Material;
#pragma warning restore REFL009 // The referenced member is not known to exist

                        if (ReferenceEquals(layerVolume, null))
                        {
                            Utils.LogDebug(" Volumetric cloud has no material on planet: " + celestialBodyName);
                            continue;
                        }

                        EVEvolumetrics.Add(ParticleMaterial);
                    }
                    catch (Exception stupid)
                    {
                        Utils.LogDebug(" Volumetric clouds error on planet: " + celestialBodyName + stupid.ToString());
                    }
                }
                Utils.LogDebug(" Detected " + EVEvolumetrics.Count + " EVE volumetric layers for planet: " + celestialBodyName);
            }
            else
            {
                Utils.LogDebug(" No cloud objects for planet: " + celestialBodyName);
            }
        }
    }
}

