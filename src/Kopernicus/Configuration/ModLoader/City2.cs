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
 * which is copyright 2011-2017 Squad. Your usage of Kerbal Space Program
 * itself is governed by the terms of its EULA, not the license above.
 * 
 * https://kerbalspaceprogram.com
 */

using CommNet;
using System;
using System.Linq;
using Kopernicus.Constants;
using Kopernicus.UI;
using UnityEngine;

namespace Kopernicus
{
    namespace Configuration
    {
        namespace ModLoader
        {
            [RequireConfigType(ConfigType.Node)]
            public class City2 : ModLoader<PQSCity2>
            {
                // LODRange loader
                [RequireConfigType(ConfigType.Node)]
                public class LODRangeLoader : IPatchable, ITypeParser<PQSCity2.LodObject>
                {
                    // LOD object
                    public PQSCity2.LodObject Value { get; set; }
                    
                    // Fake property to allow patching by index
                    public String name
                    {
                        get { return null; }
                        set {}
                    }

                    // Delete the lod range
                    [ParserTarget("delete")]
                    public NumericParser<Boolean> delete = false;

                    // visibleRange
                    [ParserTarget("visibleRange")]
                    public NumericParser<Single> visibleRange 
                    {
                        get { return Value.visibleRange; }
                        set { Value.visibleRange = value; }
                    }

                    // keepActive
                    #if !KSP131
                    [ParserTarget("keepActive")]
                    public NumericParser<Boolean> keepActive 
                    {
                        get { return Value.KeepActive; }
                        set { Value.KeepActive = value; }
                    }
                    #endif

                    // The mesh for the mod
                    [ParserTarget("model")]
                    public MuParser model
                    {
                        get
                        {
                            GameObject obj = null;
                            if (Value.objects.Length > 1)
                            {
                                obj = new GameObject();
                                obj.transform.parent = Utility.Deactivator;
                                foreach (GameObject subobj in Value.objects)
                                {
                                    UnityEngine.Object.Instantiate(subobj).transform.parent = obj.transform;
                                }
                            }
                            else if (Value.objects.Length == 1)
                            {
                                obj = Value.objects[0];
                            }
                            return new MuParser(obj);
                        }
                        set
                        {
                            Value.objects = new[] {value.Value};
                        }
                    }
                    
                    // scale
                    [ParserTarget("visibleRange")]
                    public Vector3Parser scale
                    {
                        get
                        {
                            Single x = 0;
                            Single y = 0;
                            Single z = 0;
                            Single count = Value.objects.Length > 0 ? Value.objects.Length : 1;
                            foreach (GameObject obj in Value.objects)
                            {
                                x += obj.transform.localScale.x;
                                y += obj.transform.localScale.y;
                                z += obj.transform.localScale.z;
                            }

                            return new Vector3(x / count, y / count, z / count);
                        }
                        set
                        {
                            foreach (GameObject obj in Value.objects)
                            {
                                obj.transform.localScale = value;
                            }
                        }
                    } 

                    [KittopiaConstructor(KittopiaConstructor.Parameter.Empty)]
                    public LODRangeLoader ()
                    {
                        // Initialize the LOD range
                        Value = new PQSCity2.LodObject();
                        Value.objects = new GameObject[0];
                    }

                    public LODRangeLoader(PQSCity2.LodObject c)
                    {
                        Value = c;
                        if (Value.objects == null)
                        {
                            Value.objects = new GameObject[0];
                        }
                    }

                    /// <summary>
                    /// Convert Parser to Value
                    /// </summary>
                    public static implicit operator PQSCity2.LodObject(LODRangeLoader parser)
                    {
                        return parser.Value;
                    }
        
                    /// <summary>
                    /// Convert Value to Parser
                    /// </summary>
                    public static implicit operator LODRangeLoader(PQSCity2.LodObject value)
                    {
                        return new LODRangeLoader(value);
                    }
                }
                // snapToSurface
                [ParserTarget("snapToSurface")]
                public NumericParser<Boolean> snapToSurface
                {
                    get { return mod.snapToSurface; }
                    set { mod.snapToSurface = value; }
                }

                // alt
                [ParserTarget("alt")]
                public NumericParser<Double> alt
                {
                    get { return mod.alt; }
                    set { mod.alt = value; }
                }

                // lat
                [ParserTarget("lat")]
                public NumericParser<Double> lat
                {
                    get { return mod.lat; }
                    set { mod.lat = value; }
                }

                // lon
                [ParserTarget("lon")]
                public NumericParser<Double> lon
                {
                    get { return mod.lon; }
                    set { mod.lon = value; }
                }

                // objectName
                [ParserTarget("objectName")]
                public String objectName
                {
                    get { return mod.objectName; }
                    set { mod.objectName = value; }
                }

                // up
                [ParserTarget("up")]
                public Vector3Parser up
                {
                    get { return mod.up; }
                    set { mod.up = value; }
                }

                // rotation
                [ParserTarget("rotation")]
                public NumericParser<Double> rotation
                {
                    get { return mod.rotation; }
                    set { mod.rotation = value; }
                }

                // snapHeightOffset
                [ParserTarget("snapHeightOffset")]
                public NumericParser<Double> snapHeightOffset
                {
                    get { return mod.snapHeightOffset; }
                    set { mod.snapHeightOffset = value; }
                }

                // Commnet Station
                [ParserTarget("commnetStation")]
                public NumericParser<Boolean> commnetStation
                {
                    get { return mod.gameObject.GetComponentInChildren<CommNetHome>() != null; }
                    set
                    {
                        if (!value) return;
                        CommNetHome station = mod.gameObject.AddComponent<CommNetHome>();
                        station.isKSC = false;
                    }
                }

                // Commnet Station
                [ParserTarget("isKSC")]
                public NumericParser<Boolean> isKSC
                {
                    get 
                    { 
                        CommNetHome home = mod.gameObject.GetComponentInChildren<CommNetHome>();
                        return home != null && home.isKSC;
                    }
                    set
                    {
                        CommNetHome home = mod.gameObject.GetComponentInChildren<CommNetHome>();
                        if (home != null)
                        {
                            home.isKSC = value;
                        }
                    }
                }

                // The land classes
                [ParserTargetCollection("LOD", AllowMerge = true)]
                public CallbackList<LODRangeLoader> lodRanges { get; set; }

                // Creates the a PQSMod of type T with given PQS
                public override void Create(PQS pqsVersion)
                {
                    base.Create(pqsVersion);
                    
                    // Create the callback list
                    lodRanges = new CallbackList<LODRangeLoader> ((e) =>
                    {
                        mod.objects = lodRanges.Where(lodRange => !lodRange.delete)
                            .Select(lodRange => lodRange.Value).ToArray();
                        foreach (GameObject obj in e.Value.objects)
                        {
                            obj.transform.parent = mod.transform;
                            obj.transform.localPosition = Vector3.zero;
                            obj.SetLayerRecursive(GameLayers.LocalSpace);
                        }
                    });
                    mod.objects = new PQSCity2.LodObject[0];
                }

                // Grabs a PQSMod of type T from a parameter with a given PQS
                public override void Create(PQSCity2 _mod, PQS pqsVersion)
                {
                    base.Create(_mod, pqsVersion);
                    
                    // Create the callback list
                    lodRanges = new CallbackList<LODRangeLoader> (e =>
                    {
                        mod.objects = lodRanges.Where(lodRange => !lodRange.delete)
                            .Select(lodRange => lodRange.Value).ToArray();
                        foreach (GameObject obj in e.Value.objects)
                        {
                            obj.transform.parent = mod.transform;
                            obj.transform.localPosition = Vector3.zero;
                            obj.SetLayerRecursive(GameLayers.LocalSpace);
                        }
                    });
                    
                    // Load LandClasses
                    if (mod.objects != null)
                    {
                        for (Int32 i = 0; i < mod.objects.Length; i++)
                        {
                            // Only activate the callback if we are adding the last loader
                            lodRanges.Add(new LODRangeLoader(mod.objects[i]), i == mod.objects.Length - 1);
                        }
                    }
                    else
                    {
                        mod.objects = new PQSCity2.LodObject[0];
                    }
                }
            }
        }
    }
}

