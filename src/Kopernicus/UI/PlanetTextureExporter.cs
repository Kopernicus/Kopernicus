using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

namespace Kopernicus
{
    namespace UI
    {
        /// <summary>
        /// Exports the celestial body maps of a body
        /// </summary>
        public class PlanetTextureExporter
        {
            [RequireConfigType(ConfigType.Node)]
            public class TextureOptions
            {
                [ParserTarget("exportColor")] 
                public NumericParser<Boolean> ExportColor;

                [ParserTarget("exportHeight")] 
                public NumericParser<Boolean> ExportHeight;

                [ParserTarget("exportNormal")] 
                public NumericParser<Boolean> ExportNormal;

                [ParserTarget("transparentMaps")] 
                public NumericParser<Boolean> TransparentMaps;

                [ParserTarget("saveToDisk")] 
                public NumericParser<Boolean> SaveToDisk;

                [ParserTarget("applyToScaled")] 
                public NumericParser<Boolean> ApplyToScaled;

                [ParserTarget("normalStrength")] 
                public NumericParser<Single> NormalStrength;

                public TextureOptions()
                {
                    ExportColor = true;
                    ExportHeight = true;
                    ExportNormal = true;
                    TransparentMaps = true;
                    SaveToDisk = true;
                    ApplyToScaled = true;
                    NormalStrength = 7;
                }
            }

            public static IEnumerator UpdateTextures(CelestialBody celestialBody, TextureOptions options)
            {
                // Get time
                DateTime now = DateTime.Now;
                
                // If the user wants to export normals, we need height too
                if (options.ExportNormal)
                {
                    options.ExportHeight = true;
                }

                // Prepare the PQS
                PQS pqsVersion = celestialBody.pqsController;

                // If the PQS is null, abort
                if (pqsVersion == null)
                {
                    throw new InvalidOperationException();
                }

                // Tell the PQS that we are going to build maps
                pqsVersion.SetupExternalRender();

                // Get the mod building methods from the PQS
                #if !KSP131
                Action<PQS.VertexBuildData, Boolean> modOnVertexBuildHeight = 
                    (Action<PQS.VertexBuildData, Boolean>) Delegate.CreateDelegate(
                        typeof(Action<PQS.VertexBuildData, Boolean>),
                        pqsVersion,
                        typeof(PQS).GetMethod("Mod_OnVertexBuildHeight",
                            BindingFlags.Instance | BindingFlags.NonPublic));
                #else
                Action<PQS.VertexBuildData> modOnVertexBuildHeight =
                    (Action<PQS.VertexBuildData>) Delegate.CreateDelegate(
                        typeof(Action<PQS.VertexBuildData>),
                        pqsVersion,
                        typeof(PQS).GetMethod("Mod_OnVertexBuildHeight",
                            BindingFlags.Instance | BindingFlags.NonPublic));
                #endif
                Action<PQS.VertexBuildData> modOnVertexBuild = (Action<PQS.VertexBuildData>) Delegate.CreateDelegate(
                    typeof(Action<PQS.VertexBuildData>),
                    pqsVersion,
                    typeof(PQS).GetMethod("Mod_OnVertexBuild", BindingFlags.Instance | BindingFlags.NonPublic));

                // Get all mods the PQS is connected to
                PQSMod[] mods = pqsVersion.GetComponentsInChildren<PQSMod>()
                    .Where(m => m.sphere == pqsVersion && m.modEnabled).OrderBy(m => m.order).ToArray();
                
                // Prevent the PQS from updating
                pqsVersion.enabled = false;

                // Create the Textures
                Texture2D colorMap = new Texture2D(pqsVersion.mapFilesize, pqsVersion.mapFilesize / 2,
                    TextureFormat.ARGB32,
                    true);
                Texture2D heightMap = new Texture2D(pqsVersion.mapFilesize, pqsVersion.mapFilesize / 2,
                    TextureFormat.RGB24,
                    true);

                // Arrays
                Color[] colorMapValues = new Color[pqsVersion.mapFilesize * (pqsVersion.mapFilesize / 2)];
                Color[] heightMapValues = new Color[pqsVersion.mapFilesize * (pqsVersion.mapFilesize / 2)];

                // Create a VertexBuildData
                PQS.VertexBuildData data = new PQS.VertexBuildData();
                
                // Display
                ScreenMessage message = ScreenMessages.PostScreenMessage("Generating Planet-Maps", Single.MaxValue, ScreenMessageStyle.UPPER_CENTER);
                yield return null;

                // Loop through the pixels
                for (Int32 y = 0; y < pqsVersion.mapFilesize / 2; y++)
                {
                    for (Int32 x = 0; x < pqsVersion.mapFilesize; x++)
                    {
                        // Update Message
                        Double percent = (Double) (y * pqsVersion.mapFilesize + x) /
                                         (pqsVersion.mapFilesize / 2 * pqsVersion.mapFilesize) * 100;
                        while (CanvasUpdateRegistry.IsRebuildingLayout()) Thread.Sleep(10);
                        message.textInstance.text.text = "Generating Planet-Maps: " + percent.ToString("0.00") + "%";

                        // Update the VertexBuildData
                        data.directionFromCenter =
                            QuaternionD.AngleAxis(360d / pqsVersion.mapFilesize * x, Vector3d.up) *
                            QuaternionD.AngleAxis(90d - 180d / (pqsVersion.mapFilesize / 2f) * y, Vector3d.right)
                            * Vector3d.forward;
                        data.vertHeight = pqsVersion.radius;

                        // Build from the Mods 
                        Double height = Double.MinValue;
                        if (options.ExportHeight)
                        {
                            #if !KSP131
                            modOnVertexBuildHeight(data, true);
                            #else
                            modOnVertexBuildHeight(data);
                            #endif

                            // Adjust the height
                            height = (data.vertHeight - pqsVersion.radius) * (1d / pqsVersion.mapMaxHeight);
                            if (height < 0)
                            {
                                height = 0;
                            }
                            else if (height > 1)
                            {
                                height = 1;
                            }

                            // Set the Pixels
                            heightMapValues[y * pqsVersion.mapFilesize + x] =
                                new Color((Single) height, (Single) height, (Single) height);
                        }

                        if (options.ExportColor)
                        {
                            modOnVertexBuild(data);

                            // Adjust the Color
                            Color color = data.vertColor;
                            if (!pqsVersion.mapOcean)
                            {
                                color.a = 1f;
                            }
                            else if (height > pqsVersion.mapOceanHeight)
                            {
                                color.a = options.TransparentMaps ? 0f : 1f;
                            }
                            else
                            {
                                color = pqsVersion.mapOceanColor.A(1f);
                            }

                            // Set the Pixels
                            colorMapValues[y * pqsVersion.mapFilesize + x] = color;
                        }
                    }
                        
                    yield return null;
                }

                // Serialize the maps to disk
                String name = "KittopiaTech/PluginData/" + celestialBody.transform.name + "/";
                String path = KSPUtil.ApplicationRootPath + "/GameData/" + name;
                Directory.CreateDirectory(path);

                // Colormap
                if (options.ExportColor)
                {
                    // Save it
                    colorMap.SetPixels(colorMapValues);
                    yield return null;
                    
                    if (options.SaveToDisk)
                    {
                        File.WriteAllBytes(path + celestialBody.transform.name + "_Color.png", colorMap.EncodeToPNG());
                        colorMap.name = name + celestialBody.transform.name + "_Color.png";
                        yield return null;
                    }

                    // Apply it
                    if (options.ApplyToScaled)
                    {
                        colorMap.Apply();
                        celestialBody.scaledBody.GetComponent<MeshRenderer>().material.SetTexture("_MainTex", colorMap);
                    }
                }

                if (options.ExportHeight)
                {
                    heightMap.SetPixels(heightMapValues);
                    yield return null;
                    
                    if (options.SaveToDisk)
                    {
                        File.WriteAllBytes(path + celestialBody.transform.name + "_Height.png",
                            heightMap.EncodeToPNG());
                        yield return null;
                    }

                    if (options.ExportNormal)
                    {
                        // Bump to Normal Map
                        Texture2D normalMap = Utility.BumpToNormalMap(heightMap, pqsVersion.radius, options.NormalStrength);
                        yield return null;
                        
                        if (options.SaveToDisk)
                        {
                            File.WriteAllBytes(path + celestialBody.transform.name + "_Normal.png",
                                normalMap.EncodeToPNG());
                            normalMap.name = name + celestialBody.transform.name + "_Normal.png";
                            yield return null;
                        }

                        // Apply it
                        if (options.ApplyToScaled)
                        {
                            normalMap.Apply();
                            celestialBody.scaledBody.GetComponent<MeshRenderer>().material
                                .SetTexture("_BumpMap", normalMap);
                        }
                    }
                }

                // Close the Renderer
                pqsVersion.enabled = true;
                pqsVersion.CloseExternalRender();
                
                // Declare that we're done
                ScreenMessages.RemoveMessage(message);
                ScreenMessages.PostScreenMessage("Operation completed in: " + (DateTime.Now - now).TotalMilliseconds + " ms", 2f, ScreenMessageStyle.UPPER_CENTER);
            }
        }
    }
}