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
using System.Diagnostics.CodeAnalysis;
using Kopernicus.Components.MaterialWrapper;
using Kopernicus.ConfigParser.Attributes;
using Kopernicus.ConfigParser.BuiltinTypeParsers;
using Kopernicus.ConfigParser.Enumerations;
using Kopernicus.ConfigParser.Interfaces;
using Kopernicus.Configuration.MaterialLoader;
using Kopernicus.Configuration.Parsing;
using UnityEngine;
using static Kopernicus.Configuration.ModLoader.LandControl.LandClassScatterLoader;

namespace Kopernicus.Configuration.ModLoader
{
    [RequireConfigType(ConfigType.Node)]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    public class MeshScatter : ModLoader<PQSMod_MeshScatter>, IParserEventSubscriber
    {
        [PreApply]
        [ParserTarget("materialType")]
        public EnumParser<ScatterMaterialType> MaterialType
        {
            get
            {
                if (CustomMaterial == null)
                {
                    return null;
                }
                if (NormalDiffuse.UsesSameShader(CustomMaterial))
                {
                    return ScatterMaterialType.Diffuse;
                }
                if (NormalBumped.UsesSameShader(CustomMaterial))
                {
                    return ScatterMaterialType.BumpedDiffuse;
                }
                if (NormalDiffuseDetail.UsesSameShader(CustomMaterial))
                {
                    return ScatterMaterialType.DiffuseDetail;
                }
                if (DiffuseWrap.UsesSameShader(CustomMaterial))
                {
                    return ScatterMaterialType.DiffuseWrapped;
                }
                if (AlphaTestDiffuse.UsesSameShader(CustomMaterial))
                {
                    return ScatterMaterialType.CutoutDiffuse;
                }
                if (AerialTransCutout.UsesSameShader(CustomMaterial))
                {
                    return ScatterMaterialType.AerialCutout;
                }
                return null;
            }
            set
            {
                if (value == ScatterMaterialType.Diffuse)
                {
                    CustomMaterial = new NormalDiffuseLoader();
                }
                else if (value == ScatterMaterialType.BumpedDiffuse)
                {
                    CustomMaterial = new NormalBumpedLoader();
                }
                else if (value == ScatterMaterialType.DiffuseDetail)
                {
                    CustomMaterial = new NormalDiffuseDetailLoader();
                }
                else if (value == ScatterMaterialType.DiffuseWrapped)
                {
                    CustomMaterial = new DiffuseWrapLoader();
                }
                else if (value == ScatterMaterialType.CutoutDiffuse)
                {
                    CustomMaterial = new AlphaTestDiffuseLoader();
                }
                else if (value == ScatterMaterialType.AerialCutout)
                {
                    CustomMaterial = new AerialTransCutoutLoader();
                }
            }
        }

        // Should we add colliders to the scatter=
        [ParserTarget("collide")]
        public NumericParser<Boolean> Collide = new NumericParser<Boolean>(false);

        // Should we add Science to the Scatter?
        [ParserTarget("science")]
        public NumericParser<Boolean> Science = new NumericParser<Boolean>(false);

        // ConfigNode that stores the Experiment
        [ParserTarget("Experiment")]
        public ConfigNode Experiment;

        // Custom scatter material                
        [ParserTarget("Material", AllowMerge = true)]
        public Material CustomMaterial { get; set; }

        // The name of the scatter
        [ParserTarget("scatterName")]
        public String ScatterName
        {
            get { return Mod.scatterName; }
            set { Mod.scatterName = value; }
        }

        // The seed that is used to generate the scatters
        [ParserTarget("seed")]
        public NumericParser<Int32> Seed
        {
            get { return Mod.seed; }
            set { Mod.seed = value; }
        }

        // maxCache
        [ParserTarget("maxCache")]
        public NumericParser<Int32> MaxCache
        {
            get { return Mod.maxCache; }
            set { Mod.maxCache = value; }
        }

        // Maximum amount of scatters
        [ParserTarget("maxScatter")]
        public NumericParser<Int32> MaxScatter
        {
            get { return Mod.maxScatter; }
            set { Mod.maxScatter = value; }
        }

        // Stock material
        [ParserTarget("material", AllowMerge = true)]
        public StockMaterialParser Material
        {
            get { return Mod.material; }
            set { Mod.material = value; }
        }

        // A concentration map. white is many, black is less
        [ParserTarget("map")]
        public Texture2DParser ScatterMap
        {
            get { return Mod.scatterMap; }
            set { Mod.scatterMap = Utility.CreateReadable(value); }
        }

        // The mesh
        [ParserTarget("mesh")]
        public MeshParser BaseMesh
        {
            get { return Mod.baseMesh; }
            set { Mod.baseMesh = value; }
        }

        // min subdivision
        [ParserTarget("minSubdivision")]
        public NumericParser<Int32> MinSubdivision
        {
            get { return Mod.minSubdivision; }
            set { Mod.minSubdivision = value; }
        }

        // countPerSqM
        [ParserTarget("countPerSqM")]
        public NumericParser<Single> CountPerSqM
        {
            get { return Mod.countPerSqM; }
            set { Mod.countPerSqM = value; }
        }

        // verticalOffset
        [ParserTarget("verticalOffset")]
        public NumericParser<Single> VerticalOffset
        {
            get { return Mod.verticalOffset; }
            set { Mod.verticalOffset = value; }
        }

        // minimum Scale of the scatter
        [ParserTarget("minScale")]
        public Vector3Parser MinScale
        {
            get { return Mod.minScale; }
            set { Mod.minScale = value; }
        }

        // maximum Scale of the scatter
        [ParserTarget("maxScale")]
        public Vector3Parser MaxScale
        {
            get { return Mod.maxScale; }
            set { Mod.maxScale = value; }
        }

        // castShadows
        [ParserTarget("castShadows")]
        public NumericParser<Boolean> CastShadows
        {
            get { return Mod.castShadows; }
            set { Mod.castShadows = value; }
        }

        // recieveShadows
        [ParserTarget("recieveShadows")]
        public NumericParser<Boolean> RecieveShadows
        {
            get { return Mod.recieveShadows; }
            set { Mod.recieveShadows = value; }
        }

        // Apply event
        void IParserEventSubscriber.Apply(ConfigNode node)
        {
            if (CustomMaterial || !Mod.material)
            {
                return;
            }
            if (Mod.material.shader == new NormalDiffuse().shader)
            {
                CustomMaterial = new NormalDiffuseLoader(Mod.material);
            }
            else if (Mod.material.shader == new NormalBumped().shader)
            {
                CustomMaterial = new NormalBumpedLoader(Mod.material);
            }
            else if (Mod.material.shader == new NormalDiffuseDetail().shader)
            {
                CustomMaterial = new NormalDiffuseDetailLoader(Mod.material);
            }
            else if (Mod.material.shader == new DiffuseWrapLoader().shader)
            {
                CustomMaterial = new DiffuseWrapLoader(Mod.material);
            }
            else if (Mod.material.shader == new AlphaTestDiffuse().shader)
            {
                CustomMaterial = new AlphaTestDiffuseLoader(Mod.material);
            }
            else if (Mod.material.shader == new AerialTransCutout().shader)
            {
                CustomMaterial = new AerialTransCutoutLoader(Mod.material);
            }
        }

        // Post Apply event
        void IParserEventSubscriber.PostApply(ConfigNode node)
        {
            // If defined, use custom material
            if (CustomMaterial != null)
            {
                Mod.material = CustomMaterial;
            }
        }
    }
}

