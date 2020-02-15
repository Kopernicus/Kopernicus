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

using System;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;
using System.Threading;
using Kopernicus.ConfigParser.Attributes;
using Kopernicus.ConfigParser.BuiltinTypeParsers;
using Kopernicus.ConfigParser.Enumerations;
using Kopernicus.OnDemand;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Kopernicus.UI
{
    /// <summary>
    /// Exports the celestial body maps of a body
    /// </summary>
    public static class PlanetTextureExporter
    {
        private static readonly Int32 MainTex = Shader.PropertyToID("_MainTex");
        private static readonly Int32 BumpMap = Shader.PropertyToID("_BumpMap");

        [RequireConfigType(ConfigType.Node)]
        [SuppressMessage("ReSharper", "FieldCanBeMadeReadOnly.Global")]
        public class TextureOptions
        {
            [ParserTarget("exportColor")]
            [KittopiaDescription("Whether to generate a color map.")]
            public NumericParser<Boolean> ExportColor;

            [ParserTarget("exportHeight")]
            [KittopiaDescription("Whether to generate a height map.")]
            public NumericParser<Boolean> ExportHeight;

            [ParserTarget("exportNormal")]
            [KittopiaDescription("Whether to generate a normal map. This requires generating a height map.")]
            public NumericParser<Boolean> ExportNormal;

            [ParserTarget("transparentMaps")]
            [KittopiaDescription(
                "The maps of ocean worlds need to have their colors set to transparent when they show land and not water. This setting can disable that, although the produced color map should not be used in-game.")]
            public NumericParser<Boolean> TransparentMaps;

            [ParserTarget("saveToDisk")]
            [KittopiaDescription("Whether to export the new maps to the GameData folder.")]
            public NumericParser<Boolean> SaveToDisk;

            [ParserTarget("applyToScaled")]
            [KittopiaDescription("Whether to update the in-game Scaled Space maps with the new ones.")]
            public NumericParser<Boolean> ApplyToScaled;

            [ParserTarget("normalStrength")]
            public NumericParser<Single> NormalStrength;

            [ParserTarget("resolution")]
            [KittopiaDescription("The width of the final map. The height is automatically set to 1/2 of the width.")]
            public NumericParser<Int32> Resolution;

            [ParserTarget("textureDelta")]
            public NumericParser<Double> TextureDelta;

            public TextureOptions()
            {
                ExportColor = true;
                ExportHeight = true;
                ExportNormal = true;
                TransparentMaps = true;
                SaveToDisk = true;
                ApplyToScaled = true;
                NormalStrength = 10;
                Resolution = 2048;
                TextureDelta = 0;
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
                yield break;
            }

            // Tell the PQS that we are going to build maps
            pqsVersion.SetupExternalRender();

            // Get the mod building methods from the PQS
            Action<PQS.VertexBuildData, Boolean> modOnVertexBuildHeight =
                (Action<PQS.VertexBuildData, Boolean>) Delegate.CreateDelegate(
                    typeof(Action<PQS.VertexBuildData, Boolean>),
                    pqsVersion,
                    typeof(PQS).GetMethod("Mod_OnVertexBuildHeight",
                        BindingFlags.Instance | BindingFlags.NonPublic));
            Action<PQS.VertexBuildData> modOnVertexBuild = (Action<PQS.VertexBuildData>) Delegate.CreateDelegate(
                typeof(Action<PQS.VertexBuildData>),
                pqsVersion,
                typeof(PQS).GetMethod("Mod_OnVertexBuild", BindingFlags.Instance | BindingFlags.NonPublic));

            // Prevent the PQS from updating
            pqsVersion.enabled = false;

            // Create the Textures
            Texture2D colorMap = new Texture2D(options.Resolution, options.Resolution / 2,
                TextureFormat.ARGB32,
                true);
            Texture2D heightMap = new Texture2D(options.Resolution, options.Resolution / 2,
                TextureFormat.RGB24,
                true);

            // Arrays
            Color[] colorMapValues = new Color[options.Resolution * (options.Resolution / 2)];
            Color[] heightMapValues = new Color[options.Resolution * (options.Resolution / 2)];

            // Create a VertexBuildData
            PQS.VertexBuildData data = new PQS.VertexBuildData();

            // Display
            ScreenMessage message = ScreenMessages.PostScreenMessage("Generating terrain data", Single.MaxValue,
                ScreenMessageStyle.UPPER_CENTER);
            yield return null;

            Double[] heightValues = new Double[options.Resolution * (options.Resolution / 2)];

            // Loop through the pixels
            for (Int32 y = 0; y < options.Resolution / 2; y++)
            {
                // Update Message
                Double percent = y / (options.Resolution / 2d) * 100;
                while (CanvasUpdateRegistry.IsRebuildingLayout())
                {
                    Thread.Sleep(10);
                }

                message.textInstance.text.text = "Generating terrain data: " + percent.ToString("0.00") + "%";

                for (Int32 x = 0; x < options.Resolution; x++)
                {
                    // Update the VertexBuildData
                    data.directionFromCenter =
                        QuaternionD.AngleAxis(360d / options.Resolution * x, Vector3d.up) *
                        QuaternionD.AngleAxis(90d - 180d / (options.Resolution / 2f) * y, Vector3d.right)
                        * Vector3d.forward;
                    data.vertHeight = pqsVersion.radius;

                    modOnVertexBuildHeight(data, true);
                    modOnVertexBuild(data);

                    // Cache the results
                    heightValues[y * options.Resolution + x] = data.vertHeight;
                    colorMapValues[y * options.Resolution + x] = data.vertColor;
                }

                yield return null;
            }

            // Update Message
            while (CanvasUpdateRegistry.IsRebuildingLayout())
            {
                Thread.Sleep(10);
            }

            message.textInstance.text.text = "Calculating height difference";

            // Figure out the delta radius ourselves
            Double minHeight = Double.MaxValue;
            Double maxHeight = Double.MinValue;
            for (Int32 i = 0; i < heightValues.Length; i++)
            {
                if (heightValues[i] > maxHeight)
                {
                    maxHeight = heightValues[i];
                }
                else if (heightValues[i] < minHeight)
                {
                    minHeight = heightValues[i];
                }
            }

            Double deltaRadius = maxHeight - minHeight;
            if (options.TextureDelta > 0)
            {
                deltaRadius = options.TextureDelta;
            }

            yield return null;

            // Update Message
            while (CanvasUpdateRegistry.IsRebuildingLayout())
            {
                Thread.Sleep(10);
            }

            message.textInstance.text.text = "Calculating color data";

            // Apply the values
            for (Int32 y = 0; y < options.Resolution / 2; y++)
            {
                // Update Message
                Double percent = y / (options.Resolution / 2d) * 100;
                while (CanvasUpdateRegistry.IsRebuildingLayout())
                {
                    Thread.Sleep(10);
                }

                message.textInstance.text.text = "Calculating color data: " + percent.ToString("0.00") + "%";

                for (Int32 x = 0; x < options.Resolution; x++)
                {
                    // Build from the Mods
                    Double height = heightValues[y * options.Resolution + x] - pqsVersion.radius;
                    if (options.ExportColor)
                    {
                        // Adjust the Color
                        Color color = colorMapValues[y * options.Resolution + x];
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
                        colorMapValues[y * options.Resolution + x] = color;
                    }

                    if (!options.ExportHeight)
                    {
                        continue;
                    }

                    // Adjust the height
                    height /= deltaRadius;
                    if (height < 0)
                    {
                        height = 0;
                    }
                    else if (height > 1)
                    {
                        height = 1;
                    }

                    // Set the Pixels
                    heightMapValues[y * options.Resolution + x] =
                        new Color((Single) height, (Single) height, (Single) height);
                }

                yield return null;
            }

            // Serialize the maps to disk
            String name = "KittopiaTech/PluginData/" + celestialBody.transform.name + "/" +
                          DateTime.Now.ToString("yyyy-MM-dd_hh-mm-ss") + "/";
            String path = KSPUtil.ApplicationRootPath + "/GameData/" + name;
            Directory.CreateDirectory(path);

            // Color map
            if (options.ExportColor)
            {
                // Update Message
                while (CanvasUpdateRegistry.IsRebuildingLayout())
                {
                    Thread.Sleep(10);
                }

                message.textInstance.text.text = "Exporting planet maps: Color";

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
                    ScaledSpaceOnDemand od = celestialBody.scaledBody.GetComponent<ScaledSpaceOnDemand>();
                    if (od != null)
                    {
                        od.texture = colorMap.name;
                        Object.DestroyImmediate(colorMap);

                        if (od.isLoaded)
                        {
                            od.UnloadTextures();
                            od.LoadTextures();
                        }
                    }
                    else
                    {
                        colorMap.Apply();
                        celestialBody.scaledBody.GetComponent<MeshRenderer>().sharedMaterial
                            .SetTexture(MainTex, colorMap);
                    }
                }
                else
                {
                    Object.DestroyImmediate(colorMap);
                }
            }

            if (options.ExportHeight)
            {
                // Update Message
                while (CanvasUpdateRegistry.IsRebuildingLayout())
                {
                    Thread.Sleep(10);
                }

                message.textInstance.text.text = "Exporting planet maps: Height";

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
                    // Update Message
                    while (CanvasUpdateRegistry.IsRebuildingLayout())
                    {
                        Thread.Sleep(10);
                    }

                    message.textInstance.text.text = "Exporting planet maps: Normal";

                    // Bump to Normal Map
                    Texture2D normalMap = Utility.BumpToNormalMap(heightMap, pqsVersion, options.NormalStrength / 10);
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
                        ScaledSpaceOnDemand od = celestialBody.scaledBody.GetComponent<ScaledSpaceOnDemand>();
                        if (od != null)
                        {
                            od.normals = normalMap.name;
                            Object.DestroyImmediate(normalMap);

                            if (od.isLoaded)
                            {
                                od.UnloadTextures();
                                od.LoadTextures();
                            }
                        }
                        else
                        {
                            normalMap.Apply();
                            celestialBody.scaledBody.GetComponent<MeshRenderer>().sharedMaterial
                                .SetTexture(BumpMap, normalMap);
                        }
                    }
                    else
                    {
                        Object.DestroyImmediate(normalMap);
                    }
                }

                Object.DestroyImmediate(heightMap);
            }

            // Close the Renderer
            pqsVersion.enabled = true;
            pqsVersion.CloseExternalRender();

            // Declare that we're done
            ScreenMessages.RemoveMessage(message);
            ScreenMessages.PostScreenMessage(
                "Operation completed in: " + (DateTime.Now - now).TotalMilliseconds + " ms", 2f,
                ScreenMessageStyle.UPPER_CENTER);
        }
    }
}