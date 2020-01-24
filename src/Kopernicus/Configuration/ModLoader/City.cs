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
using System.Linq;
using CommNet;
using Kopernicus.Components;
using Kopernicus.ConfigParser.Attributes;
using Kopernicus.ConfigParser.BuiltinTypeParsers;
using Kopernicus.ConfigParser.Enumerations;
using Kopernicus.ConfigParser.Interfaces;
using Kopernicus.Configuration.Parsing;
using Kopernicus.Constants;
using Kopernicus.UI;
using UnityEngine;

namespace Kopernicus.Configuration.ModLoader
{
    [RequireConfigType(ConfigType.Node)]
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public class City : ModLoader<PQSCity>
    {
        // LODRange loader
        [RequireConfigType(ConfigType.Node)]
        public class LodRangeLoader : IPatchable, ITypeParser<PQSCity.LODRange>
        {
            // LOD object
            public PQSCity.LODRange Value { get; set; }

            // Fake property to allow patching by index
            public String name
            {
                get { return null; }
                set { }
            }

            // Delete the lod range
            [ParserTarget("delete")]
            [SuppressMessage("ReSharper", "FieldCanBeMadeReadOnly.Global")]
            public NumericParser<Boolean> Delete = false;

            // visibleRange
            [ParserTarget("visibleRange")]
            public NumericParser<Single> VisibleRange
            {
                get { return Value.visibleRange; }
                set { Value.visibleRange = value; }
            }

            // The mesh for the mod
            [ParserTarget("model")]
            public MuParser Model
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
            public Vector3Parser Scale
            {
                get
                {
                    Single x = 0;
                    Single y = 0;
                    Single z = 0;
                    Single count = Value.objects.Length > 0 ? Value.objects.Length : 1;
                    foreach (GameObject obj in Value.objects)
                    {
                        Vector3 localScale = obj.transform.localScale;
                        x += localScale.x;
                        y += localScale.y;
                        z += localScale.z;
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

            [KittopiaConstructor(KittopiaConstructor.ParameterType.Empty)]
            public LodRangeLoader()
            {
                // Initialize the LOD range
                Value = new PQSCity.LODRange
                {
                    objects = new GameObject[0],
                    renderers = new GameObject[0]
                };
            }

            public LodRangeLoader(PQSCity.LODRange c)
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
            public static implicit operator PQSCity.LODRange(LodRangeLoader parser)
            {
                return parser.Value;
            }

            /// <summary>
            /// Convert Value to Parser
            /// </summary>
            public static implicit operator LodRangeLoader(PQSCity.LODRange value)
            {
                return new LodRangeLoader(value);
            }
        }

        // debugOrientated
        [ParserTarget("debugOrientated")]
        public NumericParser<Boolean> DebugOrientated
        {
            get { return Mod.debugOrientated; }
            set { Mod.debugOrientated = value; }
        }

        // frameDelta
        [ParserTarget("frameDelta")]
        public NumericParser<Single> FrameDelta
        {
            get { return Mod.frameDelta; }
            set { Mod.frameDelta = value; }
        }

        // randomizeOnSphere
        [ParserTarget("randomizeOnSphere")]
        public NumericParser<Boolean> RandomizeOnSphere
        {
            get { return Mod.randomizeOnSphere; }
            set { Mod.randomizeOnSphere = value; }
        }

        // reorientToSphere
        [ParserTarget("reorientToSphere")]
        public NumericParser<Boolean> ReorientToSphere
        {
            get { return Mod.reorientToSphere; }
            set { Mod.reorientToSphere = value; }
        }

        // reorientFinalAngle
        [ParserTarget("reorientFinalAngle")]
        public NumericParser<Single> ReorientFinalAngle
        {
            get { return Mod.reorientFinalAngle; }
            set { Mod.reorientFinalAngle = value; }
        }

        // reorientInitialUp
        [ParserTarget("reorientInitialUp")]
        public Vector3Parser ReorientInitialUp
        {
            get { return Mod.reorientInitialUp; }
            set { Mod.reorientInitialUp = value; }
        }

        // repositionRadial
        [ParserTarget("repositionRadial")]
        public Vector3Parser RepositionRadial
        {
            get { return Mod.repositionRadial; }
            set { Mod.repositionRadial = value; }
        }

        // repositionRadial - Position
        [ParserTarget("RepositionRadial")]
        [SuppressMessage("ReSharper", "UnusedMember.Local")]
        private PositionParser RepositionRadialPosition
        {
            set { Mod.repositionRadial = value; }
        }

        // repositionRadiusOffset
        [ParserTarget("repositionRadiusOffset")]
        public NumericParser<Double> RepositionRadiusOffset
        {
            get { return Mod.repositionRadiusOffset; }
            set { Mod.repositionRadiusOffset = value; }
        }

        // repositionToSphere
        [ParserTarget("repositionToSphere")]
        public NumericParser<Boolean> RepositionToSphere
        {
            get { return Mod.repositionToSphere; }
            set { Mod.repositionToSphere = value; }
        }

        // repositionToSphereSurface
        [ParserTarget("repositionToSphereSurface")]
        public NumericParser<Boolean> RepositionToSphereSurface
        {
            get { return Mod.repositionToSphereSurface; }
            set { Mod.repositionToSphereSurface = value; }
        }

        // repositionToSphereSurfaceAddHeight
        [ParserTarget("repositionToSphereSurfaceAddHeight")]
        public NumericParser<Boolean> RepositionToSphereSurfaceAddHeight
        {
            get { return Mod.repositionToSphereSurfaceAddHeight; }
            set { Mod.repositionToSphereSurfaceAddHeight = value; }
        }

        // Commnet Station
        [ParserTarget("commnetStation")]
        public NumericParser<Boolean> CommnetStation
        {
            get { return Mod.gameObject.GetComponentInChildren<CommNetHome>() != null; }
            set
            {
                if (!value)
                {
                    return;
                }
                CommNetHome station = Mod.gameObject.AddComponent<CommNetHome>();
                station.isKSC = false;
            }
        }

        // Commnet Station
        [ParserTarget("isKSC")]
        public NumericParser<Boolean> IsKsc
        {
            get
            {
                CommNetHome home = Mod.gameObject.GetComponentInChildren<CommNetHome>();
                return home != null && home.isKSC;
            }
            set
            {
                CommNetHome home = Mod.gameObject.GetComponentInChildren<CommNetHome>();
                if (home != null)
                {
                    home.isKSC = value;
                }
            }
        }

        // The land classes
        [ParserTargetCollection("LOD", AllowMerge = true)]
        public CallbackList<LodRangeLoader> LodRanges { get; set; }

        // Creates the a PQSMod of type T with given PQS
        public override void Create(PQS pqsVersion)
        {
            base.Create(pqsVersion);

            // Create the callback list
            LodRanges = new CallbackList<LodRangeLoader>(e =>
            {
                Mod.lod = LodRanges.Where(lodRange => !lodRange.Delete)
                    .Select(lodRange => lodRange.Value).ToArray();
                foreach (GameObject obj in e.Value.objects)
                {
                    obj.transform.parent = Mod.transform;
                    obj.transform.localPosition = Vector3.zero;
                    obj.SetLayerRecursive(GameLayers.LOCAL_SPACE);
                    obj.gameObject.AddOrGetComponent<KopernicusSurfaceObject>().objectName = Mod.name;
                }
            });
            Mod.lod = new PQSCity.LODRange[0];
        }

        // Grabs a PQSMod of type T from a parameter with a given PQS
        public override void Create(PQSCity mod, PQS pqsVersion)
        {
            base.Create(mod, pqsVersion);

            // Create the callback list
            LodRanges = new CallbackList<LodRangeLoader>(e =>
            {
                Mod.lod = LodRanges.Where(lodRange => !lodRange.Delete)
                    .Select(lodRange => lodRange.Value).ToArray();
                foreach (GameObject obj in e.Value.objects)
                {
                    obj.transform.parent = Mod.transform;
                    obj.transform.localPosition = Vector3.zero;
                    obj.SetLayerRecursive(GameLayers.LOCAL_SPACE);
                    obj.gameObject.AddOrGetComponent<KopernicusSurfaceObject>().objectName = Mod.name;
                }
            });

            // Load LandClasses
            if (Mod.lod != null)
            {
                for (Int32 i = 0; i < Mod.lod.Length; i++)
                {
                    // Only activate the callback if we are adding the last loader
                    LodRanges.Add(new LodRangeLoader(Mod.lod[i]), i == Mod.lod.Length - 1);
                }
            }
            else
            {
                Mod.lod = new PQSCity.LODRange[0];
            }

        }
    }
}

