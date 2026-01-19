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
using System.Runtime.CompilerServices;
using Kopernicus.Components;
using KSPTextureLoader;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using UnityEngine;

namespace Kopernicus.OnDemand
{
    /// <summary>
    /// MapSO Replacement to support Texture streaming
    /// </summary>
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    [SuppressMessage("ReSharper", "SwitchStatementMissingSomeCases")]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    [BurstCompile]
    public class MapSODemand : KopernicusMapSO, ILoadOnDemand, IPreloadOnDemand
    {
        private bool error = false;

        // States
        public new bool IsLoaded
        {
            get => base.IsLoaded;

            // Setting this externally never worked, but we need to keep it for back-compat.
            [Obsolete("Setting IsLoaded is not supported and is now a no-op")]
            set
            {
                Debug.LogWarning($"[Kopernicus] Setting MapSODemand.IsLoaded externally is not supported");
            }
        }
        public bool AutoLoad { get; set; }

        // Path of the Texture
        public string Path { get; set; }

        // MapDepth
        // This got moved to KopernicusMapSO but we need to keep the accessor here
        // for backwards compatibility.
        public new MapDepth Depth
        {
            get => base.Depth;
            set => base.Depth = value;
        }

        // Name
        string ILoadOnDemand.Name
        {
            get { return name; }
            set { name = value; }
        }

        public void Preload()
        {
            if (IsLoaded)
                return;

            // We already have a handle, loading is already in progress or completed.
            if (Handle != null)
                return;

            var options = new TextureLoadOptions
            {
                Hint = TextureLoadHint.BatchSynchronous,
                Unreadable = false
            };
            Handle = TextureLoader.LoadTexture<Texture2D>(Path, options);
        }

        /// <summary>
        /// Load the Map
        /// </summary>
        public void Load()
        {
            // Check if the Map is already loaded
            if (IsLoaded)
                return;

            if (Handle is null)
            {
                var options = new TextureLoadOptions
                {
                    Hint = TextureLoadHint.Synchronous,
                    Unreadable = false
                };
                Handle = TextureLoader.LoadTexture<Texture2D>(Path, options);
            }

            CreateMap(Depth, Handle);
            if (!IsLoaded)
            {
                error = true;
                return;
            }

            // CreateMap may unload the texture so we should get the asset bundle path first
            var assetBundle = Handle?.AssetBundle;
            Events.OnMapSOLoad.Fire(this);
            if (assetBundle is null)
                Debug.Log($"[OD] ---> Map {name} enabling self. Path = {Path}");
            else
                Debug.Log($"[OD] ---> Map {name} enabling self. Path = {Path}, Asset Bundle = {assetBundle}");
        }

        /// <summary>
        /// Unload the map
        /// </summary>
        public new void Unload()
        {
            error = false;
            if (!IsLoaded)
                return;

            base.Unload();
            error = false;

            // Event
            Events.OnMapSOUnload.Fire(this);

            // Log
            Debug.Log("[OD] <--- Map " + name + " disabling self. Path = " + Path);
        }

        #region CreateMap
        /// <summary>
        /// Create a map from a Texture2D
        /// </summary>
        public override void CreateMap(MapDepth depth, Texture2D tex) =>
            base.CreateMap(depth, tex);
        #endregion

        private bool EnsureLoaded()
        {
            // We failed to load once, prevent spamming the log and/or emitting NREs repeatedly.
            if (error)
                return false;

            if (OnDemandStorage.OnDemandLogOnMissing)
                Debug.Log($"[OD] ERROR: read from unloaded map {name} with path {Path}, autoload = {AutoLoad}");

            if (AutoLoad)
                Load();

            return IsLoaded;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int WrapWidth(int x)
        {
            if (x < 0)
            {
                // TODO: This seems wrong?
                x = Width - x;
            }
            else if (x >= Width)
            {
                x -= Width;
            }

            return x;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int WrapHeight(int y)
        {
            if (y < 0)
            {
                y = Height - y;
            }
            else if (y >= Height)
            {
                y -= Height;
            }

            return y;
        }

        #region GetPixelByte
        public override byte GetPixelByte(int x, int y)
        {
            if (!IsLoaded)
                EnsureLoaded();

            return base.GetPixelByte(x, y);
        }
        #endregion

        #region GetPixelColor
        public override Color GetPixelColor(int x, int y)
        {
            if (!IsLoaded)
                EnsureLoaded();

            return base.GetPixelColor(x, y);
        }

        public override Color GetPixelColor(double x, double y)
        {
            if (!IsLoaded)
                EnsureLoaded();

            return base.GetPixelColor(x, y);
        }

        public override Color GetPixelColor(Single x, Single y)
        {
            if (!IsLoaded)
                EnsureLoaded();

            return base.GetPixelColor(x, y);
        }
        #endregion

        #region GetPixelColor32
        public override Color32 GetPixelColor32(int x, int y)
        {
            if (!IsLoaded)
                EnsureLoaded();

            return base.GetPixelColor32(x, y);
        }

        // Honestly Squad, why are they named GetPixelColor32, but return normal Colors instead of Color32?
        public override Color GetPixelColor32(Double x, Double y)
        {
            if (!IsLoaded)
                EnsureLoaded();

            return base.GetPixelColor32(x, y);
        }

        public override Color GetPixelColor32(Single x, Single y)
        {
            if (!IsLoaded)
                EnsureLoaded();

            return base.GetPixelColor32(x, y);
        }
        #endregion

        #region GetPixelFloat
        public override float GetPixelFloat(int x, int y)
        {
            if (!IsLoaded)
                EnsureLoaded();

            return base.GetPixelFloat(x, y);
        }

        public override float GetPixelFloat(double x, double y)
        {
            if (!IsLoaded)
                EnsureLoaded();

            return base.GetPixelFloat(x, y);
        }

        public override float GetPixelFloat(float x, float y)
        {
            if (!IsLoaded)
                EnsureLoaded();

            return base.GetPixelFloat(x, y);
        }
        #endregion

        #region GetPixelHeightAlpha
        public override HeightAlpha GetPixelHeightAlpha(int x, int y)
        {
            if (!IsLoaded)
                EnsureLoaded();

            return base.GetPixelHeightAlpha(x, y);
        }


        public override HeightAlpha GetPixelHeightAlpha(double x, double y)
        {
            if (!IsLoaded)
                EnsureLoaded();

            return base.GetPixelHeightAlpha(x, y);
        }

        public override HeightAlpha GetPixelHeightAlpha(float x, float y)
        {
            if (!IsLoaded)
                EnsureLoaded();

            return base.GetPixelHeightAlpha(x, y);
        }
        #endregion

        #region GreyByte
        public override byte GreyByte(int x, int y)
        {
            if (!IsLoaded)
                EnsureLoaded();

            return base.GreyByte(x, y);
        }
        #endregion


        #region GreyFloat
        public override float GreyFloat(int x, int y)
        {
            if (!IsLoaded)
                EnsureLoaded();

            return base.GreyFloat(x, y);
        }
        #endregion

        #region PixelByte
        public override byte[] PixelByte(int x, int y)
        {
            if (!IsLoaded)
                EnsureLoaded();

            return base.PixelByte(x, y);
        }
        #endregion

        // CompileToTexture
        public override Texture2D CompileToTexture(byte filter)
        {
            if (!IsLoaded)
                EnsureLoaded();

            return base.CompileToTexture(filter);
        }

        // Generate a greyscale texture from the stored data
        public override Texture2D CompileGreyscale()
        {
            if (!IsLoaded)
                EnsureLoaded();

            return base.CompileGreyscale();
        }

        // Generate a height/alpha texture from the stored data
        public override Texture2D CompileHeightAlpha()
        {
            if (!IsLoaded)
                EnsureLoaded();

            return base.CompileHeightAlpha();
        }

        // Generate an RGB texture from the stored data
        public override Texture2D CompileRGB()
        {
            if (!IsLoaded)
                EnsureLoaded();

            return base.CompileRGB();
        }

        // Generate an RGBA texture from the stored data
        public override Texture2D CompileRGBA()
        {
            if (!IsLoaded)
                EnsureLoaded();

            return base.CompileRGBA();
        }
    }
}
