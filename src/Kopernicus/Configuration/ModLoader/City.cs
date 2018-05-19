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
            public class City : ModLoader<PQSCity>
            {
                // LODRange loader
                [RequireConfigType(ConfigType.Node)]
                public class LODRangeLoader : IPatchable, ITypeParser<PQSCity.LODRange>
                {
                    // LOD object
                    public PQSCity.LODRange Value { get; set; }
                    
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
                            Value.renderers = value.Value.GetComponentsInChildren<Renderer>().Select(r => r.gameObject)
                                .ToArray();
                        }
                    }
                    
                    // scale
                    [ParserTarget("scale")]
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

                    [KittopiaConstructor(KittopiaConstructor.Parameter.Empty, purpose = KittopiaConstructor.Purpose.Create)]
                    public LODRangeLoader ()
                    {
                        // Initialize the LOD range
                        Value = new PQSCity.LODRange();
                        Value.objects = new GameObject[0];
                        Value.renderers = new GameObject[0];
                    }

                    [KittopiaConstructor(KittopiaConstructor.Parameter.Element, purpose = KittopiaConstructor.Purpose.Edit)]
                    public LODRangeLoader(PQSCity.LODRange c)
                    {
                        Value = c;
                        if (Value.objects == null)
                        {
                            Value.objects = new GameObject[0];
                        }

                        if (Value.renderers == null)
                        {
                            Value.renderers = new GameObject[0];
                        }
                    }

                    /// <summary>
                    /// Convert Parser to Value
                    /// </summary>
                    public static implicit operator PQSCity.LODRange(LODRangeLoader parser)
                    {
                        return parser.Value;
                    }
        
                    /// <summary>
                    /// Convert Value to Parser
                    /// </summary>
                    public static implicit operator LODRangeLoader(PQSCity.LODRange value)
                    {
                        return new LODRangeLoader(value);
                    }
                }
                
                // debugOrientated
                [ParserTarget("debugOrientated")]
                public NumericParser<Boolean> debugOrientated
                {
                    get { return mod.debugOrientated; }
                    set { mod.debugOrientated = value; }
                }

                // frameDelta
                [ParserTarget("frameDelta")]
                public NumericParser<Single> frameDelta
                {
                    get { return mod.frameDelta; }
                    set { mod.frameDelta = value; }
                }

                // randomizeOnSphere
                [ParserTarget("randomizeOnSphere")]
                public NumericParser<Boolean> randomizeOnSphere
                {
                    get { return mod.randomizeOnSphere; }
                    set { mod.randomizeOnSphere = value; }
                }

                // reorientToSphere
                [ParserTarget("reorientToSphere")]
                public NumericParser<Boolean> reorientToSphere
                {
                    get { return mod.reorientToSphere; }
                    set { mod.reorientToSphere = value; }
                }

                // reorientFinalAngle
                [ParserTarget("reorientFinalAngle")]
                public NumericParser<Single> reorientFinalAngle
                {
                    get { return mod.reorientFinalAngle; }
                    set { mod.reorientFinalAngle = value; }
                }

                // reorientInitialUp
                [ParserTarget("reorientInitialUp")]
                public Vector3Parser reorientInitialUp
                {
                    get { return mod.reorientInitialUp; }
                    set { mod.reorientInitialUp = value; }
                }

                // repositionRadial
                [ParserTarget("repositionRadial")]
                public Vector3Parser repositionRadial
                {
                    get { return mod.repositionRadial; }
                    set { mod.repositionRadial = value; }
                }

                // repositionRadial - Position
                [ParserTarget("RepositionRadial")]
                private PositionParser repositionRadialPosition
                {
                    set { mod.repositionRadial = value; }
                }

                // repositionRadiusOffset
                [ParserTarget("repositionRadiusOffset")]
                public NumericParser<Double> repositionRadiusOffset
                {
                    get { return mod.repositionRadiusOffset; }
                    set { mod.repositionRadiusOffset = value; }
                }

                // repositionToSphere
                [ParserTarget("repositionToSphere")]
                public NumericParser<Boolean> repositionToSphere
                {
                    get { return mod.repositionToSphere; }
                    set { mod.repositionToSphere = value; }
                }

                // repositionToSphereSurface
                [ParserTarget("repositionToSphereSurface")]
                public NumericParser<Boolean> repositionToSphereSurface
                {
                    get { return mod.repositionToSphereSurface; }
                    set { mod.repositionToSphereSurface = value; }
                }

                // repositionToSphereSurfaceAddHeight
                [ParserTarget("repositionToSphereSurfaceAddHeight")]
                public NumericParser<Boolean> repositionToSphereSurfaceAddHeight
                {
                    get { return mod.repositionToSphereSurfaceAddHeight; }
                    set { mod.repositionToSphereSurfaceAddHeight = value; }
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
                        mod.lod = lodRanges.Where(lodRange => !lodRange.delete)
                            .Select(lodRange => lodRange.Value).ToArray();
                        foreach (GameObject obj in e.Value.objects)
                        {
                            obj.transform.parent = mod.transform;
                            obj.transform.localPosition = Vector3.zero;
                            obj.SetLayerRecursive(GameLayers.LocalSpace);
                        }
                    });
                    mod.lod = new PQSCity.LODRange[0];
                }

                // Grabs a PQSMod of type T from a parameter with a given PQS
                public override void Create(PQSCity _mod, PQS pqsVersion)
                {
                    base.Create(_mod, pqsVersion);
                    
                    // Create the callback list
                    lodRanges = new CallbackList<LODRangeLoader> (e =>
                    {
                        mod.lod = lodRanges.Where(lodRange => !lodRange.delete)
                            .Select(lodRange => lodRange.Value).ToArray();
                        foreach (GameObject obj in e.Value.objects)
                        {
                            obj.transform.parent = mod.transform;
                            obj.transform.localPosition = Vector3.zero;
                            obj.SetLayerRecursive(GameLayers.LocalSpace);
                        }
                    });
                    
                    // Load LandClasses
                    if (mod.lod != null)
                    {
                        for (Int32 i = 0; i < mod.lod.Length; i++)
                        {
                            // Only activate the callback if we are adding the last loader
                            lodRanges.Add(new LODRangeLoader(mod.lod[i]), i == mod.lod.Length - 1);
                        }
                    }
                    else
                    {
                        mod.lod = new PQSCity.LODRange[0];
                    }
                }
            }
        }
    }
}

