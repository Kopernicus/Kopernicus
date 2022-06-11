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
            try
            {
                Vector2 coordVector = new Vector2((float)Math.Round(data.latitude,3),(float)Math.Round(data.longitude,3));
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
            Vector2 coordVector = new Vector2((float)Math.Round(lat,3),(float)Math.Round(lon,3));
            if (biomeCoordCacheDictionary.ContainsKey(coordVector))
            {
                return biomeCoordCacheDictionary[coordVector];
            }
            else
            {
                result = ResourceUtilities.GetBiome(coordVector.x * 0.01745329238474369, coordVector.y * 0.01745329238474369, cb).name;
                biomeCoordCacheDictionary.Add(coordVector, result);
                return result;
            }
        }
        public static string GetPreciseBiome(double lat, double lon, CelestialBody cb)
        {
            return ResourceUtilities.GetBiome(lat, lon, cb).name;
        }
    }
}
