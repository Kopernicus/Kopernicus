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
using System.Linq;
using Kopernicus.Components.ModularScatter;
using Kopernicus.ConfigParser.Attributes;
using Kopernicus.ConfigParser.BuiltinTypeParsers;
using Kopernicus.ConfigParser.Enumerations;
using Kopernicus.Configuration.Parsing;
using Kopernicus.Constants;
using UnityEngine;
using UnityEngine.Rendering;

namespace Kopernicus.Configuration.ModularScatterLoader
{
    /// <summary>
    /// Component that adds a light to scatter objects
    /// </summary>
    [RequireConfigType(ConfigType.Node)]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public class LightEmitter : ComponentLoader<ModularScatter, LightEmitterComponent>
    {
        [ParserTarget("type")]
        public EnumParser<LightType> Type
        {
            get { return Value.Prefab.type; }
            set { Value.Prefab.type = value; }
        }

        [ParserTarget("color")]
        public ColorParser Color
        {
            get { return Value.Prefab.color; }
            set { Value.Prefab.color = value; }
        }

        [ParserTarget("colorTemperature")]
        public NumericParser<Single> ColorTemperature
        {
            get { return Value.Prefab.colorTemperature; }
            set { Value.Prefab.colorTemperature = value; }
        }

        [ParserTarget("intensity")]
        public NumericParser<Single> Intensity
        {
            get { return Value.Prefab.intensity; }
            set { Value.Prefab.intensity = value; }
        }

        [ParserTarget("bounceIntensity")]
        public NumericParser<Single> BounceIntensity
        {
            get { return Value.Prefab.bounceIntensity; }
            set { Value.Prefab.bounceIntensity = value; }
        }

        [ParserTarget("shadows")]
        public EnumParser<LightShadows> Shadows
        {
            get { return Value.Prefab.shadows; }
            set { Value.Prefab.shadows = value; }
        }

        [ParserTarget("shadowStrength")]
        public NumericParser<Single> ShadowStrength
        {
            get { return Value.Prefab.shadowStrength; }
            set { Value.Prefab.shadowStrength = value; }
        }

        [ParserTarget("shadowResolution")]
        public EnumParser<LightShadowResolution> ShadowResolution
        {
            get { return Value.Prefab.shadowResolution; }
            set { Value.Prefab.shadowResolution = value; }
        }

        [ParserTarget("shadowCustomResolution")]
        public NumericParser<Int32> ShadowCustomResolution
        {
            get { return Value.Prefab.shadowCustomResolution; }
            set { Value.Prefab.shadowCustomResolution = value; }
        }

        [ParserTarget("shadowBias")]
        public NumericParser<Single> ShadowBias
        {
            get { return Value.Prefab.shadowBias; }
            set { Value.Prefab.shadowBias = value; }
        }

        [ParserTarget("shadowNormalBias")]
        public NumericParser<Single> ShadowNormalBias
        {
            get { return Value.Prefab.shadowNormalBias; }
            set { Value.Prefab.shadowNormalBias = value; }
        }

        [ParserTarget("shadowNearPlane")]
        public NumericParser<Single> ShadowNearPlane
        {
            get { return Value.Prefab.shadowNearPlane; }
            set { Value.Prefab.shadowNearPlane = value; }
        }

        [ParserTarget("range")]
        public NumericParser<Single> Range
        {
            get { return Value.Prefab.range; }
            set { Value.Prefab.range = value; }
        }

        [ParserTarget("spotAngle")]
        public NumericParser<Single> SpotAngle
        {
            get { return Value.Prefab.spotAngle; }
            set { Value.Prefab.spotAngle = value; }
        }

        [ParserTarget("innerSpotAngle")]
        public NumericParser<Single> InnerSpotAngle
        {
            get { return Value.Prefab.innerSpotAngle; }
            set { Value.Prefab.innerSpotAngle = value; }
        }

        [ParserTarget("cookieSize")]
        public NumericParser<Single> CookieSize
        {
            get { return Value.Prefab.cookieSize; }
            set { Value.Prefab.cookieSize = value; }
        }

        [ParserTarget("cookie")]
        public Texture2DParser Cookie
        {
            get { return Value.Prefab.cookie as Texture2D; }
            set { Value.Prefab.cookie = value; }
        }

        [ParserTarget("flare")]
        public AssetParser<Flare> Flare
        {
            get { return Value.Prefab.flare; }
            set { Value.Prefab.flare = value; }
        }

        [ParserTarget("renderMode")]
        public EnumParser<LightRenderMode> RenderMode
        {
            get { return Value.Prefab.renderMode; }
            set { Value.Prefab.renderMode = value; }
        }

        [ParserTarget("cullingMask")]
        public NumericParser<Int32> CullingMask
        {
            get { return Value.Prefab.cullingMask; }
            set { Value.Prefab.cullingMask = value; }
        }

        [ParserTarget("useBoundingSphereOverride")]
        public NumericParser<Boolean> UseBoundingSphereOverride
        {
            get { return Value.Prefab.useBoundingSphereOverride; }
            set { Value.Prefab.useBoundingSphereOverride = value; }
        }

        [ParserTarget("boundingSphereOverride")]
        public Vector4Parser BoundingSphereOverride
        {
            get { return Value.Prefab.boundingSphereOverride; }
            set { Value.Prefab.boundingSphereOverride = value; }
        }

        [ParserTarget("renderingLayerMask")]
        public NumericParser<Int32> RenderingLayerMask
        {
            get { return Value.Prefab.renderingLayerMask; }
            set { Value.Prefab.renderingLayerMask = value; }
        }

        [ParserTarget("lightShadowCasterMode")]
        public EnumParser<LightShadowCasterMode> LightShadowCasterMode
        {
            get { return Value.Prefab.lightShadowCasterMode; }
            set { Value.Prefab.lightShadowCasterMode = value; }
        }

        [ParserTarget("layerShadowCullDistances")]
        public NumericCollectionParser<Single> LayerShadowCullDistances
        {
            get { return Value.Prefab.layerShadowCullDistances.ToList(); }
            set { Value.Prefab.layerShadowCullDistances = value.Value.ToArray(); }
        }

        [ParserTarget("offset")]
        public Vector3Parser Offset
        {
            get { return Value.Offset; }
            set { Value.Offset = value; }
        }

        public override void Create()
        {
            base.Create();

            // Create the prefab light object
            GameObject prefab = new GameObject();
            prefab.transform.parent = Utility.Deactivator;
            Value.Prefab = prefab.AddComponent<Light>();
            Value.Prefab.cullingMask = 1 << GameLayers.LOCAL_SPACE;
        }
    }
}