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
using KSPTextureLoader;
using Unity.Collections;
using UnityEngine;

namespace Kopernicus.Components
{
    /// <summary>
    /// MapSO replacement that stores pixel data in a <see cref="CPUTexture2D"/>
    /// instead of the stock byte array.
    /// </summary>
    public class KopernicusMapSO : MapSO
    {
        // A non-allocating HeightAlpha for internal use.
        struct ValueHeightAlpha
        {
            public float height;
            public float alpha;

            public ValueHeightAlpha(float height, float alpha)
            {
                this.height = height;
                this.alpha = alpha;
            }

            public static implicit operator HeightAlpha(ValueHeightAlpha ha) => new HeightAlpha(ha.height, ha.alpha);
        }

        // Representation of the map
        public CPUTexture2D Texture { get; private set; }

        // MapDepth
        public new MapDepth Depth { get; protected set; }

        #region CreateMap
        /// <summary>
        /// Initializes the map from an existing <see cref="CPUTexture2D"/>.
        /// </summary>
        /// <param name="depth"></param>
        /// <param name="tex"></param>
        /// 
        /// <remarks>
        /// This does not take ownership of the <see cref="CPUTexture2D"/>.
        /// If you ever intend to unload this MapSO you will need to manage its
        /// lifetime yourself.
        /// </remarks>
        public void CreateMap(MapDepth depth, CPUTexture2D tex)
        {
            if (tex is null)
                throw new ArgumentNullException(nameof(tex));

            _width = tex.Width;
            _height = tex.Height;
            _bpp = (int)depth;

            if (tex.Format == TextureFormat.R16 && depth == MapDepth.HeightAlpha)
                Texture = new RA16Texture(tex);
            else
                Texture = tex;

            _isCompiled = true;
        }

        /// <summary>
        /// Create a map from a Texture2D.
        /// </summary>
        /// 
        /// <remarks>
        /// This method assumes the texture remains live for as long as the map
        /// needs it to be (i.e. until unload is called or the map is no longer
        /// needed). Take care to ensure that is the case.
        /// </remarks>
        public override void CreateMap(MapDepth depth, Texture2D tex)
        {
            if (tex == null)
                throw new ArgumentNullException(nameof(tex));

            CreateMap(Depth, CPUTexture2D.Create(tex));
        }

        //
        public void Unload()
        {
            Texture = null;
        }
        #endregion

        #region GetPixelByte
        public override byte GetPixelByte(int x, int y)
        {
            if (Texture is null)
                return 0;

            var pixel = Texture.GetPixel32(x, y);
            if (Texture.Format == TextureFormat.Alpha8)
                return pixel.a;
            else
                return pixel.r;
        }
        #endregion

        #region GetPixelColor
        public override Color GetPixelColor(int x, int y)
        {
            if (Texture is null)
                return Color.black;

            return Texture.GetPixel(x, y);
        }
        #endregion

        #region GetPixelColor32
        public override Color32 GetPixelColor32(int x, int y)
        {
            if (Texture is null)
                return new Color32(0, 0, 0, 255);

            return Texture.GetPixel32(x, y);
        }
        #endregion

        #region GetPixelFloat
        public override float GetPixelFloat(int x, int y)
        {
            if (Texture is null)
                return 0f;

            var pixel = Texture.GetPixel(x, y);

            if (Texture.Format == TextureFormat.Alpha8)
                return pixel.a;

            switch (Depth)
            {
                case MapDepth.Greyscale:
                    return pixel.r;
                case MapDepth.HeightAlpha:
                    return 0.5f * (pixel.r + pixel.a);
                case MapDepth.RGB:
                    return (1f / 3f) * (pixel.r + pixel.g + pixel.b);
                case MapDepth.RGBA:
                    return 0.25f * (pixel.r + pixel.g + pixel.b + pixel.a);
                default:
                    return 0f;
            }
        }
        #endregion

        #region GetPixelHeightAlpha
        ValueHeightAlpha GetPixelValueHeightAlpha(int x, int y)
        {
            if (Texture is null)
                return new ValueHeightAlpha(0f, 0f);

            var pixel = Texture.GetPixel(x, y);
            switch (Depth)
            {
                case MapDepth.HeightAlpha:
                case MapDepth.RGBA:
                    return new ValueHeightAlpha(pixel.r, pixel.a);

                default:
                    return new ValueHeightAlpha(pixel.r, 1f);
            }
        }

        public override HeightAlpha GetPixelHeightAlpha(int x, int y) => GetPixelValueHeightAlpha(x, y);
        #endregion

        #region GreyByte
        public override byte GreyByte(int x, int y) => GetPixelByte(x, y);
        #endregion

        #region GreyFloat
        public override float GreyFloat(int x, int y)
        {
            if (Texture is null)
                return 0f;

            var pixel = Texture.GetPixel(x, y);
            if (Texture.Format == TextureFormat.Alpha8)
                return pixel.a;
            return pixel.r;
        }
        #endregion

        #region PixelByte
        public override byte[] PixelByte(int x, int y)
        {
            var c = GetPixelColor32(x, y);

            switch (Depth)
            {
                case MapDepth.Greyscale:
                    return new[] { c.r };
                case MapDepth.HeightAlpha:
                    return new[] { c.r, c.a };
                case MapDepth.RGB:
                    return new[] { c.r, c.g, c.b };
                default:
                    return new[] { c.r, c.g, c.b, c.a };
            }
        }
        #endregion

        #region Compile
        // CompileToTexture
        public override Texture2D CompileToTexture(byte filter)
        {
            if (Texture is null)
                return new Texture2D(_width, _height);

            var data = Texture.GetRawTextureData<byte>();
            var texture = new Texture2D(Width, Height, Texture.Format, Texture.MipCount, false);
            texture.LoadRawTextureData(data);
            texture.Apply(false, true);
            return texture;
        }

        // Generate a greyscale texture from the stored data
        public override Texture2D CompileGreyscale()
        {
            if (Texture is null)
                return new Texture2D(_width, _height);

            var color32 = new Color32[Size];
            for (int i = 0, y = 0; y < _height; ++y)
            {
                for (int x = 0; x < _width; ++x, ++i)
                {
                    var v = GetPixelByte(x, y);
                    color32[i] = new Color32(v, v, v, byte.MaxValue);
                }
            }

            Texture2D compiled = new Texture2D(Width, Height, TextureFormat.RGB24, false);
            compiled.SetPixels32(color32);
            compiled.Apply(false, true);
            return compiled;
        }

        // Generate a height/alpha texture from the stored data
        public override Texture2D CompileHeightAlpha()
        {
            if (Texture is null)
                return new Texture2D(_width, _height);

            var color32 = new Color32[Size];
            for (int i = 0, y = 0; y < _height; ++y)
            {
                for (int x = 0; x < _width; ++x, ++i)
                {
                    var ha = GetPixelValueHeightAlpha(x, y);
                    color32[i] = new Color(ha.height, ha.height, ha.height, ha.alpha);
                }
            }

            Texture2D compiled = new Texture2D(Width, Height, TextureFormat.RGBA32, false);
            compiled.SetPixels32(color32);
            compiled.Apply(false, true);
            return compiled;
        }

        // Generate an RGB texture from the stored data
        public override Texture2D CompileRGB()
        {
            if (Texture is null)
                return new Texture2D(_width, _height);

            var color32 = new Color32[Size];
            for (int i = 0, y = 0; y < _height; ++y)
            {
                for (int x = 0; x < _width; ++x, ++i)
                    color32[i] = GetPixelColor32(x, y);
            }

            Texture2D compiled = new Texture2D(Width, Height, TextureFormat.RGB24, false);
            compiled.SetPixels32(color32);
            compiled.Apply(false, true);
            return compiled;
        }

        // Generate an RGBA texture from the stored data
        public override Texture2D CompileRGBA()
        {
            if (Texture is null)
                return new Texture2D(_width, _height);

            var color32 = new Color32[Size];
            for (int i = 0, y = 0; y < _height; ++y)
            {
                for (int x = 0; x < _width; ++x, ++i)
                    color32[i] = GetPixelColor32(x, y);
            }

            Texture2D compiled = new Texture2D(Width, Height, TextureFormat.RGBA32, false);
            compiled.SetPixels32(color32);
            compiled.Apply(false, true);
            return compiled;
        }
        #endregion

        #region RA16Texture
        private sealed class RA16Texture(CPUTexture2D tex) :
            CPUTexture2D<CPUTexture2D.RA16>(new(tex.GetRawTextureData<byte>(), tex.Width, tex.Height, tex.MipCount))
        {
            readonly CPUTexture2D tex = tex;

            public override void Dispose()
            {
                base.Dispose();
                tex.Dispose();
            }
        }
        #endregion
    }
}
