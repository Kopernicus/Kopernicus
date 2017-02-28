/**
 * Kopernicus Planetary System Modifier
 * ====================================
 * Created by: BryceSchroeder and Teknoman117 (aka. Nathaniel R. Lewis)
 * Maintained by: Thomas P., NathanKell and KillAshley
 * Additional Content by: Gravitasi, aftokino, KCreator, Padishar, Kragrathea, OvenProofMars, zengei, MrHappyFace, Sigma88
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
 * which is copyright 2011-2015 Squad. Your usage of Kerbal Space Program
 * itself is governed by the terms of its EULA, not the license above.
 * 
 * https://kerbalspaceprogram.com
 */

using System;
using Kopernicus.MaterialWrapper;
using UnityEngine;
using static Kopernicus.Configuration.ModLoader.LandControl.LandClassScatterLoader;

namespace Kopernicus
{
    namespace Configuration
    {
        namespace ModLoader
        {
            [RequireConfigType(ConfigType.Node)]
            public class MeshScatter : ModLoader<PQSMod_MeshScatter>, IParserEventSubscriber
            {
                [PreApply]
                [ParserTarget("materialType")]
                public EnumParser<ScatterMaterialType> materialType
                {
                    get
                    {
                        if (customMaterial == null)
                            return null;
                        if (NormalDiffuse.UsesSameShader(customMaterial))
                            return ScatterMaterialType.Diffuse;
                        else if (NormalBumped.UsesSameShader(customMaterial))
                            return ScatterMaterialType.BumpedDiffuse;
                        else if (NormalDiffuseDetail.UsesSameShader(customMaterial))
                            return ScatterMaterialType.DiffuseDetail;
                        else if (DiffuseWrap.UsesSameShader(customMaterial))
                            return ScatterMaterialType.DiffuseWrapped;
                        else if (AlphaTestDiffuse.UsesSameShader(customMaterial))
                            return ScatterMaterialType.CutoutDiffuse;
                        else if (AerialTransCutout.UsesSameShader(customMaterial))
                            return ScatterMaterialType.AerialCutout;
                        return null;
                    }
                    set
                    {
                        if (value.value == ScatterMaterialType.Diffuse)
                            customMaterial = new NormalDiffuseLoader();
                        else if (value.value == ScatterMaterialType.BumpedDiffuse)
                            customMaterial = new NormalBumpedLoader();
                        else if (value.value == ScatterMaterialType.DiffuseDetail)
                            customMaterial = new NormalDiffuseDetailLoader();
                        else if (value.value == ScatterMaterialType.DiffuseWrapped)
                            customMaterial = new DiffuseWrapLoader();
                        else if (value.value == ScatterMaterialType.CutoutDiffuse)
                            customMaterial = new AlphaTestDiffuseLoader();
                        else if (value.value == ScatterMaterialType.AerialCutout)
                            customMaterial = new AerialTransCutoutLoader();
                    }
                }

                // Should we add colliders to the scatter=
                [ParserTarget("collide")]
                public NumericParser<bool> collide = new NumericParser<bool>(false);

                // Should we add Science to the Scatter?
                [ParserTarget("science")]
                public NumericParser<bool> science = new NumericParser<bool>(false);

                // ConfigNode that stores the Experiment
                [ParserTarget("Experiment")]
                public ConfigNode experiment;

                // Custom scatter material                
                [ParserTarget("Material")]
                public Material customMaterial;

                // The name of the scatter
                [ParserTarget("scatterName")]
                public string scatterName
                {
                    get { return mod.scatterName; }
                    set { mod.scatterName = value; }
                }

                // The seed that is used to generate the scatters
                [ParserTarget("seed")]
                public NumericParser<int> seed
                {
                    get { return mod.seed; }
                    set { mod.seed = value; }
                }

                // maxCache
                [ParserTarget("maxCache")]
                public NumericParser<int> maxCache
                {
                    get { return mod.maxCache; }
                    set { mod.maxCache = value; }
                }

                // Maximum amount of scatters
                [ParserTarget("maxScatter")]
                public NumericParser<int> maxScatter
                {
                    get { return mod.maxScatter; }
                    set { mod.maxScatter = value; }
                }

                // Stock material
                [ParserTarget("material")]
                public StockMaterialParser material
                {
                    get { return mod.material; }
                    set { mod.material = value; }
                }

                // A concentration map. white is many, black is less
                [ParserTarget("map")]
                public Texture2DParser scatterMap
                {
                    get { return mod.scatterMap; }
                    set { mod.scatterMap = value; }
                }

                // The mesh
                [ParserTarget("mesh")]
                public MeshParser baseMesh
                {
                    get { return mod.baseMesh; }
                    set { mod.baseMesh = value; }
                }

                // min subdivision
                [ParserTarget("minSubdivision")]
                public NumericParser<int> minSubdivision
                {
                    get { return mod.minSubdivision; }
                    set { mod.minSubdivision = value; }
                }

                // countPerSqM
                [ParserTarget("countPerSqM")]
                public NumericParser<float> countPerSqM
                {
                    get { return mod.countPerSqM; }
                    set { mod.countPerSqM = value; }
                }

                // verticalOffset
                [ParserTarget("verticalOffset")]
                public NumericParser<float> verticalOffset
                {
                    get { return mod.verticalOffset; }
                    set { mod.verticalOffset = value; }
                }

                // minimum Scale of the scatter
                [ParserTarget("minScale")]
                public Vector3Parser minScale
                {
                    get { return mod.minScale; }
                    set { mod.minScale = value; }
                }

                // maximum Scale of the scatter
                [ParserTarget("maxScale")]
                public Vector3Parser maxScale
                {
                    get { return mod.maxScale; }
                    set { mod.maxScale = value; }
                }

                // castShadows
                [ParserTarget("castShadows")]
                public NumericParser<bool> castShadows
                {
                    get { return mod.castShadows; }
                    set { mod.castShadows = value; }
                }

                // recieveShadows
                [ParserTarget("recieveShadows")]
                public NumericParser<bool> recieveShadows
                {
                    get { return mod.recieveShadows; }
                    set { mod.recieveShadows = value; }
                }

                // Apply event
                void IParserEventSubscriber.Apply(ConfigNode node)
                {
                    if (!customMaterial && mod.material)
                    {
                        if (mod.material.shader == new NormalDiffuse().shader)
                            customMaterial = new NormalDiffuseLoader(mod.material);
                        else if (mod.material.shader == new NormalBumped().shader)
                            customMaterial = new NormalBumpedLoader(mod.material);
                        else if (mod.material.shader == new NormalDiffuseDetail().shader)
                            customMaterial = new NormalDiffuseDetailLoader(mod.material);
                        else if (mod.material.shader == new DiffuseWrapLoader().shader)
                            customMaterial = new DiffuseWrapLoader(mod.material);
                        else if (mod.material.shader == new AlphaTestDiffuse().shader)
                            customMaterial = new AlphaTestDiffuseLoader(mod.material);
                        else if (mod.material.shader == new AerialTransCutout().shader)
                            customMaterial = new AerialTransCutoutLoader(mod.material);
                    }
                }

                // Post Apply event
                void IParserEventSubscriber.PostApply(ConfigNode node)
                {
                    // If defined, use custom material
                    if (customMaterial != null) mod.material = customMaterial;
                }
            }
        }
    }
}

