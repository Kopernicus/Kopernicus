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

using Kopernicus.ConfigParser.Attributes;
using Kopernicus.ConfigParser.BuiltinTypeParsers;
using Kopernicus.ConfigParser.Enumerations;
using Kopernicus.Configuration.Attributes;
using Kopernicus.Configuration.MaterialLoader.Parsing;
using Kopernicus.Configuration.Parsing;
using UnityEngine;

namespace Kopernicus.Configuration.MaterialLoader
{
    /// <summary>
    /// Loader for the <c>Terrain/Gas Giant</c> shader used by Jool. The shader
    /// blends two levels of detail, so most properties come in a "far" and a
    /// "detail" pair. Animation is driven at runtime by
    /// <c>GasGiantMaterialControls</c>, which writes <c>_GasGiantTime</c> every
    /// frame and clamps the speed properties.
    /// </summary>
    [RequireConfigType(ConfigType.Node)]
    [MaterialLoader(GasGiantLoader.SHADER_NAME)]
    public class GasGiantLoader : BaseMaterialLoader
    {
        public const string SHADER_NAME = "Terrain/Gas Giant";
        private static readonly Shader Shader = UnityEngine.Shader.Find(SHADER_NAME);
        public static bool UsesSameShader(Material m) => m != null && m.shader.name == SHADER_NAME;

        // Movement control texture. Red/green channels drive left/right band
        // movement speed, blue blends between the two band texture reads, and
        // alpha controls the swirl pan speed.
        [ParserTarget("movementControlTexture")]
        public MaterialTextureParser MovementTextureSetter
        {
            get => null;
            set => SetTexture("_MovementTexture", value);
        }

        [ParserTarget("movementControlTextureScale")]
        public Vector2Parser MovementTextureScaleSetter
        {
            get => GetTextureScale("_MovementTexture");
            set => SetTextureScale("_MovementTexture", value);
        }

        [ParserTarget("movementControlTextureOffset")]
        public Vector2Parser MovementTextureOffsetSetter
        {
            get => GetTextureOffset("_MovementTexture");
            set => SetTextureOffset("_MovementTexture", value);
        }

        // Swirl control texture. Red/green channels are the uv position of the
        // centre of rotation, blue is the swirl rotation amount.
        [ParserTarget("swirlControlTexture")]
        public MaterialTextureParser SwirlRotationControlTextureSetter
        {
            get => null;
            set => SetTexture("_SwirlRotationControlTexture", value);
        }

        [ParserTarget("swirlControlTextureScale")]
        public Vector2Parser SwirlRotationControlTextureScaleSetter
        {
            get => GetTextureScale("_SwirlRotationControlTexture");
            set => SetTextureScale("_SwirlRotationControlTexture", value);
        }

        [ParserTarget("swirlControlTextureOffset")]
        public Vector2Parser SwirlRotationControlTextureOffsetSetter
        {
            get => GetTextureOffset("_SwirlRotationControlTexture");
            set => SetTextureOffset("_SwirlRotationControlTexture", value);
        }

        // Far-texturing colour ramp. Sampled with the cloud pattern's red
        // channel, then blended with cloudColorMap2 by the pattern's green
        // channel.
        [ParserTarget("colorMap")]
        public MaterialTextureParser CloudColorMapSetter
        {
            get => null;
            set => SetTexture("_CloudColorMap", value);
        }

        [ParserTarget("colorMapScale")]
        public Vector2Parser CloudColorMapScaleSetter
        {
            get => GetTextureScale("_CloudColorMap");
            set => SetTextureScale("_CloudColorMap", value);
        }

        [ParserTarget("colorMapOffset")]
        public Vector2Parser CloudColorMapOffsetSetter
        {
            get => GetTextureOffset("_CloudColorMap");
            set => SetTextureOffset("_CloudColorMap", value);
        }

        // Second far-texturing colour ramp; blended against colorMap.
        [ParserTarget("colorMap2")]
        public MaterialTextureParser CloudColorMap2Setter
        {
            get => null;
            set => SetTexture("_CloudColorMap2", value);
        }

        [ParserTarget("colorMap2Scale")]
        public Vector2Parser CloudColorMap2ScaleSetter
        {
            get => GetTextureScale("_CloudColorMap2");
            set => SetTextureScale("_CloudColorMap2", value);
        }

        [ParserTarget("colorMap2Offset")]
        public Vector2Parser CloudColorMap2OffsetSetter
        {
            get => GetTextureOffset("_CloudColorMap2");
            set => SetTextureOffset("_CloudColorMap2", value);
        }

        // Near-texturing colour ramp; sampled with the detail cloud pattern.
        [ParserTarget("detailColorMap")]
        public MaterialTextureParser DetailCloudColorMapSetter
        {
            get => null;
            set => SetTexture("_DetailCloudColorMap", value);
        }

        [ParserTarget("detailColorMapScale")]
        public Vector2Parser DetailCloudColorMapScaleSetter
        {
            get => GetTextureScale("_DetailCloudColorMap");
            set => SetTextureScale("_DetailCloudColorMap", value);
        }

        [ParserTarget("detailColorMapOffset")]
        public Vector2Parser DetailCloudColorMapOffsetSetter
        {
            get => GetTextureOffset("_DetailCloudColorMap");
            set => SetTextureOffset("_DetailCloudColorMap", value);
        }

        // Far-texturing pattern. Red channel is the pattern, green channel
        // controls the blend between colorMap and colorMap2.
        [ParserTarget("cloudPatternMap")]
        public MaterialTextureParser CloudPatternTextureSetter
        {
            get => null;
            set => SetTexture("_CloudPatternTexture", value);
        }

        [ParserTarget("cloudPatternMapScale")]
        public Vector2Parser CloudPatternTextureScaleSetter
        {
            get => GetTextureScale("_CloudPatternTexture");
            set => SetTextureScale("_CloudPatternTexture", value);
        }

        [ParserTarget("cloudPatternMapOffset")]
        public Vector2Parser CloudPatternTextureOffsetSetter
        {
            get => GetTextureOffset("_CloudPatternTexture");
            set => SetTextureOffset("_CloudPatternTexture", value);
        }

        // Near-texturing pattern. Only the red channel is read.
        [ParserTarget("detailCloudPatternMap")]
        public MaterialTextureParser DetailCloudPatternTextureSetter
        {
            get => null;
            set => SetTexture("_DetailCloudPatternTexture", value);
        }

        [ParserTarget("detailCloudPatternMapScale")]
        public Vector2Parser DetailCloudPatternTextureScaleSetter
        {
            get => GetTextureScale("_DetailCloudPatternTexture");
            set => SetTextureScale("_DetailCloudPatternTexture", value);
        }

        [ParserTarget("detailCloudPatternMapOffset")]
        public Vector2Parser DetailCloudPatternTextureOffsetSetter
        {
            get => GetTextureOffset("_DetailCloudPatternTexture");
            set => SetTextureOffset("_DetailCloudPatternTexture", value);
        }

        // Far normal map.
        [ParserTarget("normalMap")]
        [ParserTarget("normals")]
        [ParserTarget("bumpMap")]
        public MaterialTextureParser NormalMapSetter
        {
            get => null;
            set => SetTexture("_NormalMap", value);
        }

        [ParserTarget("normalMapScale")]
        public Vector2Parser NormalMapScaleSetter
        {
            get => GetTextureScale("_NormalMap");
            set => SetTextureScale("_NormalMap", value);
        }

        [ParserTarget("normalMapOffset")]
        public Vector2Parser NormalMapOffsetSetter
        {
            get => GetTextureOffset("_NormalMap");
            set => SetTextureOffset("_NormalMap", value);
        }

        // Near normal map.
        [ParserTarget("detailNormalMap")]
        public MaterialTextureParser DetailNormalMapSetter
        {
            get => null;
            set => SetTexture("_DetailNormalMap", value);
        }

        [ParserTarget("detailNormalMapScale")]
        public Vector2Parser DetailNormalMapScaleSetter
        {
            get => GetTextureScale("_DetailNormalMap");
            set => SetTextureScale("_DetailNormalMap", value);
        }

        [ParserTarget("detailNormalMapOffset")]
        public Vector2Parser DetailNormalMapOffsetSetter
        {
            get => GetTextureOffset("_DetailNormalMap");
            set => SetTextureOffset("_DetailNormalMap", value);
        }

        // UV scale applied to the far uvs to produce the detail uvs.
        [ParserTarget("detailTiling")]
        public NumericParser<float> DetailTilingSetter
        {
            get => GetFloat("_DetailTiling");
            set => SetFloat("_DetailTiling", value);
        }

        // Camera distance at which detail texturing is at full nearDetail
        // strength.
        [ParserTarget("nearDetailDistance")]
        public NumericParser<float> NearDistanceForDetailSetter
        {
            get => GetFloat("_NearDistanceForDetail");
            set => SetFloat("_NearDistanceForDetail", value);
        }

        // Camera distance at which detail texturing is at full farDetail
        // strength. Detail blends between the two distances.
        [ParserTarget("farDetailDistance")]
        public NumericParser<float> FarDistanceForDetailSetter
        {
            get => GetFloat("_FarDistanceForDetail");
            set => SetFloat("_FarDistanceForDetail", value);
        }

        // Maximum strength of the detail texturing when zoomed in.
        [ParserTarget("nearDetailStrength")]
        public NumericParser<float> NearDetailSetter
        {
            get => GetFloat("_NearDetail");
            set => SetFloat("_NearDetail", value);
        }

        // Maximum strength of the detail texturing when zoomed out.
        [ParserTarget("farDetailStrength")]
        public NumericParser<float> FarDetailSetter
        {
            get => GetFloat("_FarDetail");
            set => SetFloat("_FarDetail", value);
        }

        // Band scroll speed. GasGiantMaterialControls clamps this to [-10, 10].
        [ParserTarget("bandMovementSpeed")]
        public NumericParser<float> BandMovementSpeedSetter
        {
            get => GetFloat("_BandMovementSpeed");
            set => SetFloat("_BandMovementSpeed", value);
        }

        // Swirl rotation rate. GasGiantMaterialControls clamps this to [0, 10].
        [ParserTarget("swirlRotationSpeed")]
        public NumericParser<float> SwirlRotationSpeedSetter
        {
            get => GetFloat("_SwirlRotationSpeed");
            set => SetFloat("_SwirlRotationSpeed", value);
        }

        // Final extent of the swirl rotation. Clamped to [-10, 10] at runtime.
        [ParserTarget("swirlRotationSwirliness")]
        public NumericParser<float> SwirlRotationSwirlinessSetter
        {
            get => GetFloat("_SwirlRotationSwirliness");
            set => SetFloat("_SwirlRotationSwirliness", value);
        }

        // Swirl pan speed. Clamped to [-10, 10] at runtime.
        [ParserTarget("swirlPanSpeed")]
        public NumericParser<float> SwirlPanSpeedSetter
        {
            get => GetFloat("_SwirlPanSpeed");
            set => SetFloat("_SwirlPanSpeed", value);
        }

        public override ShaderParser ShaderParser { get; set; } = Shader;

        public override void OnParentApply(GameObject scaledBody)
        {
            base.OnParentApply(scaledBody);

            if (Value == null || scaledBody == null)
                return;

            scaledBody.AddOrGetComponent<GasGiantMaterialControls>();
        }

        public GasGiantLoader(Material material = null) => Value = material;
    }
}
