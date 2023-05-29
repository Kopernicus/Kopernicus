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
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Kopernicus.Components.MaterialWrapper;
using UnityEngine;
namespace Kopernicus.Components
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class PQSMod_BiomeSampler : PQSMod
    {
        internal static Dictionary <CelestialBody, Dictionary<Vector2, string>> celestialBodyDictionary = new Dictionary<CelestialBody, Dictionary<Vector2, string>>();
        internal CelestialBody celestial = null;
        public override void OnSetup()
        {
            celestial = FlightGlobals.GetBodyByName(sphere.name);
            base.OnSetup();
        }
        public override void OnVertexBuildHeight(PQS.VertexBuildData data)
        {
            base.OnVertexBuildHeight(data);
            try
            {
                float latitude = (float)Math.Round(clampLat(((ClampRadians(data.latitude) / 0.01745329238474369))),RuntimeUtility.RuntimeUtility.KopernicusConfig.ScatterlatLongDecimalPrecision);
                float longitude = (float)Math.Round(clampLon((((ClampRadians(data.longitude) / 0.01745329238474369) - 90) * -1)),RuntimeUtility.RuntimeUtility.KopernicusConfig.ScatterlatLongDecimalPrecision);
                Vector2 coordVector = new Vector2(latitude,longitude);
                if (celestialBodyDictionary.ContainsKey(celestial))
                {
                    if (celestialBodyDictionary[celestial].ContainsKey(coordVector))
                    {
                        return;
                    }
                    else
                    {
                        celestialBodyDictionary[celestial].Add(coordVector, ResourceUtilities.GetBiome(coordVector.x * 0.01745329238474369, coordVector.y * 0.01745329238474369, celestial).name);
                    }
                }
                else
                {
                    Dictionary<Vector2, string> biomeCoordCacheDictionary = new Dictionary<Vector2, string>();
                    biomeCoordCacheDictionary.Add(coordVector, ResourceUtilities.GetBiome(coordVector.x * 0.01745329238474369, coordVector.y * 0.01745329238474369, celestial).name);
                    celestialBodyDictionary.Add(celestial, biomeCoordCacheDictionary);
                }
            }
            catch
            {
                //Just in case data is not available
            }
        }
        public static string GetCachedBiome(double lat, double lon, CelestialBody cb)
        {
            string result;
            lat = clampLat(lat);
            lon = clampLon(lon);
            Vector2 coordVector = new Vector2((float)Math.Round(lat,RuntimeUtility.RuntimeUtility.KopernicusConfig.ScatterlatLongDecimalPrecision),(float)Math.Round(lon,RuntimeUtility.RuntimeUtility.KopernicusConfig.ScatterlatLongDecimalPrecision));
            lat = ResourceUtilities.Deg2Rad(clampLat(coordVector.x));
            lon = ResourceUtilities.Deg2Rad(clampLon(coordVector.y));
            if (celestialBodyDictionary.ContainsKey(cb))
            {
                if (celestialBodyDictionary[cb].ContainsKey(coordVector))
                {
                    return celestialBodyDictionary[cb][coordVector];
                }
                else
                {
                    result = ResourceUtilities.GetBiome(ClampRadians(lat), ClampRadians(lon), cb).name;
                    celestialBodyDictionary[cb].Add(coordVector, result);
                    return result;
                }
            }
            else
            {
                Dictionary<Vector2, string> biomeCoordCacheDictionary = new Dictionary<Vector2, string>();
                result = ResourceUtilities.GetBiome(coordVector.x * 0.01745329238474369, coordVector.y * 0.01745329238474369, cb).name;
                biomeCoordCacheDictionary.Add(coordVector, result);
                celestialBodyDictionary.Add(cb, biomeCoordCacheDictionary);
                return result;
            }
        }
        public static string GetPreciseBiome(double lat, double lon, CelestialBody cb)
        {
            lat = clampLat(lat);
            lon = clampLon(lon);
            lat = ResourceUtilities.Deg2Rad(clampLat(lat));
            lon = ResourceUtilities.Deg2Rad(clampLon(lon));
            return ResourceUtilities.GetBiome(lat, lon, cb).name;
        }
        public static Vector2 RoundPosition(double lat, double lon)
        {
            lat = clampLat(lat);
            lon = clampLon(lon);
            return new Vector2((float)Math.Round(lat, RuntimeUtility.RuntimeUtility.KopernicusConfig.ScatterlatLongDecimalPrecision), (float)Math.Round(lon, RuntimeUtility.RuntimeUtility.KopernicusConfig.ScatterlatLongDecimalPrecision));
        }
        private static double ClampDegrees360(double angle)
        {
            angle %= 360.0;
            if (angle < 0.0)
            {
                return angle + 360.0;
            }
            return angle;
        }
        private static double ClampDegrees180(double angle)
        {
            angle = ClampDegrees360(angle);
            if (angle > 180.0)
            {
                angle -= 360.0;
            }
            return angle;
        }
        private static double ClampRadians(double angle)
        {
            while (angle > 6.283185307179586)
            {
                angle -= 6.283185307179586;
            }
            while (angle < 0.0)
            {
                angle += 6.283185307179586;
            }
            return angle;
        }
        private static double clampLat(double lat)
        {
            return (lat + 180.0 + 90.0) % 180.0 - 90.0;
        }

        private static double clampLon(double lon)
        {
            return (lon + 360.0 + 180.0) % 360.0 - 180.0;
        }
    }
}
