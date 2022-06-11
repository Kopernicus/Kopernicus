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
        internal static IDictionary<Vector2, string> biomeCoordCacheDictionary = new Dictionary<Vector2, string>();
        public override void OnVertexBuildHeight(PQS.VertexBuildData data)
        {
            base.OnVertexBuildHeight(data);
            try
            {
                float latitude = (float)Math.Round(UtilMath.ClampDegrees180(((UtilMath.ClampRadians(data.latitude) / 0.01745329238474369))),2);
                float longitude = (float)Math.Round(UtilMath.ClampDegrees180((((UtilMath.ClampRadians(data.longitude) / 0.01745329238474369) - 90) * -1)),2);
                Vector2 coordVector = new Vector2(latitude,longitude);
                if (biomeCoordCacheDictionary.ContainsKey(coordVector))
                {
                    return;
                }
                else
                {
                    biomeCoordCacheDictionary.Add(coordVector, ResourceUtilities.GetBiome(coordVector.x * 0.01745329238474369, coordVector.y * 0.01745329238474369, FlightGlobals.GetBodyByName(sphere.name)).name);
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
            lat = UtilMath.ClampDegrees180(lat);
            lon = UtilMath.ClampDegrees180(lon);
            Vector2 coordVector = new Vector2((float)Math.Round(lat,2),(float)Math.Round(lon,2));
            if (biomeCoordCacheDictionary.ContainsKey(coordVector))
            {
                return biomeCoordCacheDictionary[coordVector];
            }
            else
            {
                result = ResourceUtilities.GetBiome(UtilMath.ClampRadians(coordVector.x * 0.01745329238474369), UtilMath.ClampRadians(coordVector.y * 0.01745329238474369), cb).name;
                biomeCoordCacheDictionary.Add(coordVector, result);
                return result;
            }
        }
        public static string GetPreciseBiome(double lat, double lon, CelestialBody cb)
        {
            return ResourceUtilities.GetBiome(lat * 0.01745329238474369, lon * 0.01745329238474369, cb).name;
        }
    }
}
