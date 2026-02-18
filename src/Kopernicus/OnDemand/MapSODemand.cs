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
using Kopernicus.Components;
using KSPTextureLoader;
using Unity.Burst;
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
        enum MapState : byte
        {
            Unloaded = 0,
            Loaded,
            Error
        }

        // Representation of the map
        private CPUTextureHandle Handle { get; set; }

        private MapState State { get; set; }

        // This exists solely for backwards compatibility, so that code compiled
        // against the old version of kopernicus continues to work.
        public new MapDepth Depth
        {
            get => base.Depth;
            set => base.Depth = value;
        }

        // States
        public bool IsLoaded
        {
            get => State == MapState.Loaded;

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
            Handle = TextureLoader.LoadCPUTexture(Path, options);
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
                Handle = TextureLoader.LoadCPUTexture(Path, options);
            }

            // Load the Map
            CPUTexture2D map;
            try
            {
                map = Handle.GetTexture();
            }
            catch (Exception e)
            {
                Debug.Log($"[OD] ERROR: Failed to load map {name} at path {Path}");
                Debug.LogException(e);

                Handle?.Dispose();
                Handle = null;
                State = MapState.Error;
                return;
            }

            // CreateMap may unload the texture so we should get the asset bundle path first
            var assetBundle = Handle?.AssetBundle;

            // Make sure we're in an error state if CreateMap throws.
            State = MapState.Error;

            // If the map isn't null
            CreateMap(Depth, map);
            State = MapState.Loaded;
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
            // Clear the texture handle regardless of whether we are loaded or not.
            Handle?.Dispose();
            Handle = null;
            base.Unload();

            // We can only destroy the map, if it is loaded
            bool loaded = IsLoaded;
            State = MapState.Unloaded;

            if (!loaded)
                return;

            // Event
            Events.OnMapSOUnload.Fire(this);

            // Log
            Debug.Log("[OD] <--- Map " + name + " disabling self. Path = " + Path);
        }

        private bool EnsureLoaded()
        {
            // We failed to load once, prevent spamming the log and/or emitting NREs repeatedly.
            if (State == MapState.Error)
                return false;

            if (OnDemandStorage.OnDemandLogOnMissing)
                Debug.Log($"[OD] ERROR: read from unloaded map {name} with path {Path}, autoload = {AutoLoad}");

            if (AutoLoad)
                Load();

            return IsLoaded;
        }

        #region GetPixelByte
        public override byte GetPixelByte(int x, int y)
        {
            if (!IsLoaded)
            {
                if (!EnsureLoaded())
                    return 0;
            }

            return base.GetPixelByte(x, y);
        }
        #endregion

        #region GetPixelColor
        public override Color GetPixelColor(int x, int y)
        {
            if (!IsLoaded)
            {
                if (!EnsureLoaded())
                    return Color.black;
            }

            return base.GetPixelColor(x, y);
        }

        public override Color GetPixelColor(double x, double y)
        {
            if (!IsLoaded)
            {
                if (!EnsureLoaded())
                    return Color.black;
            }

            return base.GetPixelColor(x, y);
        }

        public override Color GetPixelColor(Single x, Single y)
        {
            if (!IsLoaded)
            {
                if (!EnsureLoaded())
                    return Color.black;
            }

            return base.GetPixelColor(x, y);
        }
        #endregion

        #region GetPixelColor32
        public override Color32 GetPixelColor32(int x, int y)
        {
            if (!IsLoaded)
            {
                if (!EnsureLoaded())
                    return default;
            }

            return base.GetPixelColor32(x, y);
        }

        // Honestly Squad, why are they named GetPixelColor32, but return normal Colors instead of Color32?
        public override Color GetPixelColor32(Double x, Double y)
        {
            if (!IsLoaded)
            {
                if (!EnsureLoaded())
                    return default;
            }

            return base.GetPixelColor32(x, y);
        }

        public override Color GetPixelColor32(Single x, Single y)
        {
            if (!IsLoaded)
            {
                if (!EnsureLoaded())
                    return default;
            }

            return base.GetPixelColor32(x, y);
        }
        #endregion

        #region GetPixelFloat
        public override float GetPixelFloat(int x, int y)
        {
            if (!IsLoaded)
            {
                if (!EnsureLoaded())
                    return 0f;
            }

            return base.GetPixelFloat(x, y);
        }

        public override float GetPixelFloat(double x, double y)
        {
            if (!IsLoaded)
            {
                if (!EnsureLoaded())
                    return 0f;
            }

            return base.GetPixelFloat(x, y);
        }

        public override float GetPixelFloat(float x, float y)
        {
            if (!IsLoaded)
            {
                if (!EnsureLoaded())
                    return 0f;
            }

            return base.GetPixelFloat(x, y);
        }
        #endregion

        #region GetPixelHeightAlpha
        public override HeightAlpha GetPixelHeightAlpha(int x, int y)
        {
            if (!IsLoaded)
            {
                if (!EnsureLoaded())
                    return new HeightAlpha(0f, 0f);
            }

            return base.GetPixelHeightAlpha(x, y);
        }

        public override HeightAlpha GetPixelHeightAlpha(double x, double y)
        {
            if (!IsLoaded)
            {
                if (!EnsureLoaded())
                    return new HeightAlpha(0f, 0f);
            }

            return base.GetPixelHeightAlpha(x, y);
        }

        public override HeightAlpha GetPixelHeightAlpha(float x, float y)
        {
            if (!IsLoaded)
            {
                if (!EnsureLoaded())
                    return new HeightAlpha(0f, 0f);
            }

            return base.GetPixelHeightAlpha(x, y);
        }
        #endregion

        #region GreyByte
        public override byte GreyByte(int x, int y)
        {
            if (!IsLoaded)
            {
                if (!EnsureLoaded())
                    return 0;
            }

            return base.GreyByte(x, y);
        }
        #endregion

        #region GreyFloat
        public override float GreyFloat(int x, int y)
        {
            if (!IsLoaded)
            {
                if (!EnsureLoaded())
                    return 0f;
            }

            return base.GreyFloat(x, y);
        }
        #endregion

        #region PixelByte
        public override byte[] PixelByte(int x, int y)
        {
            if (!IsLoaded)
            {
                if (!EnsureLoaded())
                    return new byte[_bpp];
            }

            return base.PixelByte(x, y);
        }
        #endregion

        // CompileToTexture
        public override Texture2D CompileToTexture(byte filter)
        {
            if (!IsLoaded)
            {
                if (!EnsureLoaded())
                    return new Texture2D(_width, _height);
            }

            return base.CompileToTexture(filter);
        }

        // Generate a greyscale texture from the stored data
        public override Texture2D CompileGreyscale()
        {
            if (!IsLoaded)
            {
                if (!EnsureLoaded())
                    return new Texture2D(_width, _height);
            }

            return base.CompileGreyscale();
        }

        // Generate a height/alpha texture from the stored data
        public override Texture2D CompileHeightAlpha()
        {
            if (!IsLoaded)
            {
                if (!EnsureLoaded())
                    return new Texture2D(_width, _height);
            }

            return base.CompileHeightAlpha();
        }

        // Generate an RGB texture from the stored data
        public override Texture2D CompileRGB()
        {
            if (!IsLoaded)
            {
                if (!EnsureLoaded())
                    return new Texture2D(_width, _height);
            }

            return base.CompileRGB();
        }

        // Generate an RGBA texture from the stored data
        public override Texture2D CompileRGBA()
        {
            if (!IsLoaded)
            {
                if (!EnsureLoaded())
                    return new Texture2D(_width, _height);
            }

            return base.CompileRGBA();
        }
    }
}
