/**
 * Kopernicus Planetary System Modifier
 * ====================================
 * Created by: BryceSchroeder and Teknoman117 (aka. Nathaniel R. Lewis)
 * Maintained by: Thomas P., NathanKell and KillAshley
 * Additional Content by: Gravitasi, aftokino, KCreator, Padishar, Kragrathea, OvenProofMars, zengei, MrHappyFace
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
using System.Collections.Generic;
using System.Linq;
using System.Text;

using KSP;
using UnityEngine;
using System.IO;

namespace Kopernicus
{
    namespace OnDemand
    {
        public class CBAttributeMapSODemand : CBAttributeMapSO, ILoadOnDemand
        {
            // BitPerPixels is always 4
            protected new const int _bpp = 4;

            // Representation of the map
            protected new Texture2D _data { get; set; }

            // States
            public bool IsLoaded { get; set; }
            public bool AutoLoad { get; set; }

            // Path of the Texture
            public string Path { get; set; }

            // MapDepth
            public new MapDepth Depth { get; set; }

            // Load the Map
            public bool Load()
            {
                // Check if the Map is already loaded
                if (IsLoaded)
                    return false;

                // Load the Map
                Texture2D map = OnDemandStorage.LoadTexture(Path, false, false, false);

                // If the map isn't null
                if (map != null)
                {
                    CreateMap(Depth, map);
                    IsLoaded = true;
                    Debug.Log("[OD] CBmap " + name + " enabling self. Path = " + Path);
                    return true;
                }

                // Return nothing
                Debug.Log("[OD] ERROR: Failed to load CBmap " + name + " at path " + Path);
                return false;
            }

            // Unload the map
            public bool Unload()
            {
                // We can only destroy the map, if it is loaded and initialized
                if (!IsLoaded || !string.IsNullOrEmpty(Path))
                    return false;

                // Nuke the map
                DestroyImmediate(_data);

                // Set flags
                IsLoaded = false;

                // Log
                Debug.Log("[OD] CBmap " + name + " disabling self. Path = " + Path);

                // We're done here
                return true;
            }

            // Create a map from a Texture2D
            public override void CreateMap(MapDepth depth, Texture2D tex)
            {
                // If the Texture is null, abort
                if (tex == null)
                {
                    Debug.Log("[OD] ERROR: Failed to load map");
                    return;
                }

                // Set _data
                _data = tex;

                // Variables
                name = tex.name;
                _width = tex.width;
                _height = tex.height;
                Depth = depth;
                _rowWidth = _width * _bpp;

                // We're compiled
                _isCompiled = true;
            }

            public CBAttributeMapSODemand()
                : base()
            {
                // register here or on creation by parser?
                // for now we'll do it on creation (i.e. elsewhere)
            }

            // GetAtt
            public override MapAttribute GetAtt(double lat, double lon)
            {
                if (!IsLoaded)
                {
                    if (OnDemandStorage.onDemandLogOnMissing) Debug.Log("[OD] ERROR: getting attribute with unloaded CBmap " + name + " of path " + Path + ", autoload = " + AutoLoad);
                    if (AutoLoad) Load();
                    else return Attributes[0];
                }
                return base.GetAtt(lat, lon);
            }

            // GetPixelColor - Float
            public override Color GetPixelColor(float x, float y)
            {
                if (!IsLoaded)
                {
                    if (OnDemandStorage.onDemandLogOnMissing) Debug.Log("[OD] ERROR: getting pixelColF with unloaded map " + name + " of path " + Path + ", autoload = " + AutoLoad);
                    if (AutoLoad) Load();
                    else return Color.black;
                }

                BilinearCoords coords = ConstructBilinearCoords(x, y);
                return Color.Lerp(
                    Color.Lerp(
                        GetPixelColor(coords.xFloor, coords.yFloor),
                        GetPixelColor(coords.xCeiling, coords.yFloor),
                        coords.u),
                    Color.Lerp(
                        GetPixelColor(coords.xFloor, coords.yCeiling),
                        GetPixelColor(coords.xCeiling, coords.yCeiling),
                        coords.u),
                    coords.v);
            }

            // GetPixelColor - Int
            public override Color GetPixelColor(int x, int y)
            {
                if (!IsLoaded)
                {
                    if (OnDemandStorage.onDemandLogOnMissing) Debug.Log("[OD] ERROR: getting pixelColI with unloaded map " + name + " of path " + Path + ", autoload = " + AutoLoad);
                    if (AutoLoad) Load();
                    else return Color.black;
                }
                return _data.GetPixel(x, y);
            }

            // CompileToTexture
            public override Texture2D CompileToTexture()
            {
                if (!IsLoaded)
                {
                    if (OnDemandStorage.onDemandLogOnMissing) Debug.Log("OD: ERROR: compiling with unloaded CBmap " + name + " of path " + Path + ", autoload = " + AutoLoad);
                    if (AutoLoad) Load();
                    else return new Texture2D(_width, _height);
                }
                return _data;
            }

            // ConstructBilinearCoords from double
            protected new BilinearCoords ConstructBilinearCoords(double x, double y)
            {
                // Create the struct
                BilinearCoords coords = new BilinearCoords();

                // Floor
                x = Math.Abs(x - Math.Floor(x));
                y = Math.Abs(y - Math.Floor(y));

                // X to U
                coords.x = x * _width;
                coords.xFloor = (int)Math.Floor(coords.x);
                coords.xCeiling = (int)Math.Ceiling(coords.x);
                coords.u = (float)(coords.x - coords.xFloor);
                if (coords.xCeiling == _width) coords.xCeiling = 0;

                // Y to V
                coords.y = y * _height;
                coords.yFloor = (int)Math.Floor(coords.y);
                coords.yCeiling = (int)Math.Ceiling(coords.y);
                coords.v = (float)(coords.y - coords.yFloor);
                if (coords.yCeiling == this._height) coords.yCeiling = 0;

                // We're done
                return coords;
            }

            // ConstructBilinearCoords from float
            protected new BilinearCoords ConstructBilinearCoords(float x, float y)
            {
                return ConstructBilinearCoords((double)x, (double)y);
            }

            // BilinearCoords
            public struct BilinearCoords
            {
                public double x, y;
                public int xCeiling, xFloor, yCeiling, yFloor;
                public float u, v;
            }
        }

    }
}
