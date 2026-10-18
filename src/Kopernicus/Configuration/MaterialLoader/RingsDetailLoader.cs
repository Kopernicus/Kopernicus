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
using Kopernicus.Configuration.MaterialLoader.Parsing;
using Kopernicus.UI;
using UnityEngine;

namespace Kopernicus.Configuration.MaterialLoader;

/// <summary>
/// One of the ring's two levels of detail noise.
/// </summary>
[RequireConfigType(ConfigType.Node)]
public class RingDetailPassLoader
{
    [ParserTarget("texture")]
    [KittopiaDescription("The texture used for this layer of detail.")]
    public MaterialTextureParser Texture { get; set; }

    [ParserTarget("alphaMin")]
    [KittopiaDescription("A per-channel override of the minimum opacity multiplier.")]
    public Vector4Parser AlphaMin { get; set; } = Vector4.zero;

    [ParserTarget("alphaMax")]
    [KittopiaDescription("A per-channel override of the maximum opacity multiplier.")]
    public Vector4Parser AlphaMax { get; set; } = Vector4.one;

    [ParserTarget("tiling")]
    [KittopiaDescription("Texture tiling multiplier for this level of ring detail.")]
    public Vector2Parser Tiling { get; set; } = Vector2.one;

    [ParserTarget("strength")]
    [KittopiaDescription("The strength of the detail overlay effect.")]
    public NumericParser<float> Strength { get; set; } = 0f;

    [ParserTarget("detailMask")]
    [KittopiaDescription("A per-detail-pass per-texture-channel multiplier.")]
    public Vector4Parser DetailMask { get; set; } = Vector4.one;

    [ParserTarget("fadeInStart")]
    [KittopiaDescription("The distance from the camera that a ring pixel has to be for this detail level to start being blended in.")]
    public NumericParser<float> FadeInStart
    {
        get => FadeParams.x;
        set => FadeParams = new Vector4(value, FadeParams.y, FadeParams.z, FadeParams.w);
    }

    [ParserTarget("fadeInEnd")]
    [KittopiaDescription("The distance from the camera that a ring pixel has to be for this detail level to fully be blended in.")]
    public NumericParser<float> FadeInEnd
    {
        get => FadeParams.y;
        set => FadeParams = new Vector4(FadeParams.x, value, FadeParams.z, FadeParams.w);
    }

    [ParserTarget("fadeOutStart")]
    [KittopiaDescription("The distance from the camera at which this detail level will start being faded out again.")]
    public NumericParser<float> FadeOutStart
    {
        get => FadeParams.z;
        set => FadeParams = new Vector4(FadeParams.x, FadeParams.y, value, FadeParams.w);
    }

    [ParserTarget("fadeOutEnd")]
    [KittopiaDescription("The distance from the camera at which this detail level is again fully ignored.")]
    public NumericParser<float> FadeOutEnd
    {
        get => FadeParams.w;
        set => FadeParams = new Vector4(FadeParams.x, FadeParams.y, FadeParams.z, value);
    }

    public Vector4 FadeParams { get; private set; } = new Vector4(-10f, -8f, -6f, -3f);

    internal void Apply(MaterialLoader loader, string textureKey, string alphaMinKey, string alphaMaxKey,
                        string strengthKey, string maskKey)
    {
        if (Texture != null)
            loader.SetTexture(textureKey, Texture);

        loader.SetVector(alphaMinKey, AlphaMin);
        loader.SetVector(alphaMaxKey, AlphaMax);
        loader.SetFloat(strengthKey, Strength);
        loader.SetVector(maskKey, DetailMask);
    }
}

[RequireConfigType(ConfigType.Node)]
public class RingDetailLoader
{
    [ParserTarget("detailRegionsMask")]
    [KittopiaDescription("A mask that is applied to the detail regions texture mutiplicatively.")]
    public Vector4Parser DetailRegionsMask { get; set; } = Vector4.zero;

    [ParserTarget("detailRegionsTexture")]
    [KittopiaDescription("A texture that defines per-location prominence of the detail noise texture channels.")]
    public MaterialTextureParser DetailRegionsTexture { get; set; }

    [ParserTarget("Coarse", AllowMerge = true)]
    public RingDetailPassLoader Coarse { get; set; } = new RingDetailPassLoader();

    [ParserTarget("Fine", AllowMerge = true)]
    public RingDetailPassLoader Fine { get; set; } = new RingDetailPassLoader();

    internal void Apply(MaterialLoader loader)
    {
        loader.SetVector("detailRegionsMask", DetailRegionsMask);
        if (DetailRegionsTexture != null)
            loader.SetTexture("_DetailRegionsTex", DetailRegionsTexture);

        Coarse.Apply(loader, "_CoarseDetailNoiseTex", "coarseDetailAlphaMin", "coarseDetailAlphaMax",
            "coarseDetailStrength", "coarseDetailMask");
        Fine.Apply(loader, "_FineDetailNoiseTex", "fineDetailAlphaMin", "fineDetailAlphaMax",
            "fineDetailStrength", "fineDetailMask");

        loader.SetVector("detailTiling", new Vector4(
            Coarse.Tiling.Value.x, Coarse.Tiling.Value.y,
            Fine.Tiling.Value.x, Fine.Tiling.Value.y));

        loader.SetVector("detailFade0", new Vector4(
            Coarse.FadeParams.x, Fine.FadeParams.x,
            Coarse.FadeParams.w, Fine.FadeParams.w));

        loader.SetVector("detailFade1", new Vector4(
            Coarse.FadeParams.y, Fine.FadeParams.y,
            Coarse.FadeParams.z, Fine.FadeParams.z));
    }
}
