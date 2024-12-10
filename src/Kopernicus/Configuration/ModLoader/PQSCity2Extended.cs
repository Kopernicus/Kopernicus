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
using Kopernicus.Configuration.ModularScatterLoader;
using Kopernicus.Configuration.Parsing;
using Kopernicus.Constants;
using Kopernicus.UI;
using UnityEngine;

namespace Kopernicus.Configuration.ModLoader
{
    public class PQSCity2Extended : PQSCity2
    {
        /// <summary>
        /// Kerbal-exclusive kill Radius. Zero means none.
        /// </summary>
        public int lethalRadius = 0;

        /// <summary>
        /// Kerbal-exclusive kill Radius message on death.  Empty string means off.
        /// </summary>
        public string lethalRadiusMsg = "";

        /// <summary>
        /// Kerbal-exclusive kill Radius 200% zone warning message.  Empty string means off.
        /// </summary>
        public string lethalRadiusWarnMsg = "";

        /// <summary>
        /// lethalRadius squared for fast distance comparison
        /// </summary>
        private float lethalSquareRadius;

        /// <summary>
        /// lethalRadius warning range squared for fast distance comparison
        /// </summary>
        private float lethalWarnSquareRadius;

        /// <summary>
        /// whether or not we have msg'd this kerbal already, to avoid spamming. Sets false again when out of danger.
        /// antiSpamCounterMult is a user definable parameter to say how many seconds the delay should do to avoid "spamming"
        /// antiSpamCounterMult is tied to the physics framerate so extremely low fps may affect it.
        /// </summary>
        public int antiSpamCounterMult = 30; //default 30 seconds
        private static bool lethalMsgSent = false;
        private static bool lethalWarnMsgSent = false;
        private static int antiSpamCounter = 7875; //default 30 seconds

        public override void OnSetup()
        {
            base.OnSetup();
            lethalSquareRadius = lethalRadius * lethalRadius;
            lethalWarnSquareRadius = (lethalRadius * 2) * (lethalRadius * 2);
            antiSpamCounter = 262 * antiSpamCounterMult; //Set timer
        }
        public override void OnUpdateFinished()
        {
            base.OnUpdateFinished();
            // if no active vessel, we aren't in the flight scene, no need to update either
            Vessel activeVessel = FlightGlobals.ActiveVessel;
            if (activeVessel.IsNullOrDestroyed())
                return;

            // perf optimization
            bool doLethalCheck = lethalRadius != 0 && activeVessel.isEVA;
            if (!doLethalCheck)
                return;

            Vector3 evaKerbalPos = activeVessel.transform.position;
            Vector3 cityPos = this.transform.position;
            if (doLethalCheck)
            {
                if ((cityPos - evaKerbalPos).sqrMagnitude < lethalSquareRadius)
                {
                    if ((lethalRadiusMsg.Length != 0) && (lethalMsgSent == false))
                    {
                        lethalMsgSent = true;
                        PopupDialog.SpawnPopupDialog(new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), "Poof!", "Poof!", lethalRadiusMsg, "Poof!", true, UISkinManager.defaultSkin);
                    }
                    activeVessel.rootPart.explode();
                    return;
                }
                else if ((cityPos - evaKerbalPos).sqrMagnitude < lethalWarnSquareRadius)
                {
                    if ((lethalRadiusWarnMsg.Length != 0) && (lethalWarnMsgSent == false))
                    {
                        lethalWarnMsgSent = true;
                        PopupDialog.SpawnPopupDialog(new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), "DANGER", "DANGER", lethalRadiusWarnMsg, "UhOh!", true, UISkinManager.defaultSkin);
                    }
                }
            }
            if (antiSpamCounter == 1)
            {
                antiSpamCounter = 262 * antiSpamCounterMult;
                lethalMsgSent = false;
                lethalWarnMsgSent = false;
            }
            else
            {
                antiSpamCounter--;
            }
        }
    }
    [RequireConfigType(ConfigType.Node)]
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public class City2Extended : ModLoader<PQSCity2Extended>
    {
        // lethalRadius, a per scatter lethal kill radius.  Zero means disable.
        [ParserTarget("lethalRadius")]
        public NumericParser<Int32> LethalRadius
        {
            get { return Mod.lethalRadius; }
            set { Mod.lethalRadius = value; }
        }

        // lethalRadiusMsg, a message for when a kerbal dies from lethalRadius.  Empty string means disabled.
        [ParserTarget("lethalRadiusMsg")]
        public String LethalRadiusMsg
        {
            get { return Mod.lethalRadiusMsg; }
            set { Mod.lethalRadiusMsg = value; }
        }

        // lethalRadiusWarnMsg, a message for when a kerbal comes close to a lethal radius (within 200% of the killzone). Empty string means disabled.
        [ParserTarget("lethalRadiusWarnMsg")]
        public String LethalRadiusWarnMsg
        {
            get { return Mod.lethalRadiusWarnMsg; }
            set { Mod.lethalRadiusWarnMsg = value; }
        }

        // lethalRadiusAntiSpamMult is a user definable parameter to say how many seconds the delay should do to avoid "spamming"
        // lethalRadiusAntiSpamMult is tied to the physics framerate so extremely low fps may affect it.  Zero means disable.
        [ParserTarget("lethalRadiusAntiSpamMult")]
        public NumericParser<Int32> LethalRadiusAntiSpamMult
        {
            get { return Mod.antiSpamCounterMult; }
            set { Mod.antiSpamCounterMult = value; }
        }
        // LODRange loader
        [RequireConfigType(ConfigType.Node)]
        public class LodRangeLoader : IPatchable, ITypeParser<PQSCity2Extended.LodObject>
        {
            // LOD object
            public PQSCity2Extended.LodObject Value { get; set; }

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

            // keepActive
            [ParserTarget("keepActive")]
            public NumericParser<Boolean> KeepActive
            {
                get { return Value.KeepActive; }
                set { Value.KeepActive = value; }
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
                set { Value.objects = new[] { value.Value }; }
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
                Value = new PQSCity2Extended.LodObject { objects = new GameObject[0] };
            }

            public LodRangeLoader(PQSCity2Extended.LodObject c)
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
            public static implicit operator PQSCity2Extended.LodObject(LodRangeLoader parser)
            {
                return parser.Value;
            }

            /// <summary>
            /// Convert Value to Parser
            /// </summary>
            public static implicit operator LodRangeLoader(PQSCity2Extended.LodObject value)
            {
                return new LodRangeLoader(value);
            }
        }

        // snapToSurface
        [ParserTarget("snapToSurface")]
        public NumericParser<Boolean> SnapToSurface
        {
            get { return Mod.snapToSurface; }
            set { Mod.snapToSurface = value; }
        }

        // alt
        [ParserTarget("alt")]
        public NumericParser<Double> Alt
        {
            get { return Mod.alt; }
            set { Mod.alt = value; }
        }

        // lat
        [ParserTarget("lat")]
        public NumericParser<Double> Lat
        {
            get { return Mod.lat; }
            set { Mod.lat = value; }
        }

        // lon
        [ParserTarget("lon")]
        public NumericParser<Double> Lon
        {
            get { return Mod.lon; }
            set { Mod.lon = value; }
        }

        // objectName
        [ParserTarget("objectName")]
        public String ObjectName
        {
            get { return Mod.objectName; }
            set { Mod.objectName = value; }
        }

        // up
        [ParserTarget("up")]
        public Vector3Parser Up
        {
            get { return Mod.up; }
            set { Mod.up = value; }
        }

        // rotation
        [ParserTarget("rotation")]
        public NumericParser<Double> Rotation
        {
            get { return Mod.rotation; }
            set { Mod.rotation = value; }
        }

        // snapHeightOffset
        [ParserTarget("snapHeightOffset")]
        public NumericParser<Double> SnapHeightOffset
        {
            get { return Mod.snapHeightOffset; }
            set { Mod.snapHeightOffset = value; }
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
                Mod.objects = LodRanges.Where(lodRange => !lodRange.Delete)
                    .Select(lodRange => lodRange.Value).ToArray();
                foreach (GameObject obj in e.Value.objects)
                {
                    obj.transform.parent = Mod.transform;
                    obj.transform.localPosition = Vector3.zero;
                    obj.SetLayerRecursive(GameLayers.LOCAL_SPACE);
                    obj.AddOrGetComponent<KopernicusSurfaceObject>().objectName = Mod.name;
                }
            });
            Mod.objects = new PQSCity2Extended.LodObject[0];
        }

        // Grabs a PQSMod of type T from a parameter with a given PQS
        public override void Create(PQSCity2Extended mod, PQS pqsVersion)
        {
            base.Create(mod, pqsVersion);

            // Create the callback list
            LodRanges = new CallbackList<LodRangeLoader>(e =>
            {
                Mod.objects = LodRanges.Where(lodRange => !lodRange.Delete)
                    .Select(lodRange => lodRange.Value).ToArray();
                foreach (GameObject obj in e.Value.objects)
                {
                    obj.transform.parent = Mod.transform;
                    obj.transform.localPosition = Vector3.zero;
                    obj.SetLayerRecursive(GameLayers.LOCAL_SPACE);
                    obj.AddOrGetComponent<KopernicusSurfaceObject>().objectName = Mod.name;
                }
            });

            // Load LandClasses
            if (Mod.objects != null)
            {
                for (Int32 i = 0; i < Mod.objects.Length; i++)
                {
                    // Only activate the callback if we are adding the last loader
                    LodRanges.Add(new LodRangeLoader(Mod.objects[i]), i == Mod.objects.Length - 1);
                }
            }
            else
            {
                Mod.objects = new PQSCity2Extended.LodObject[0];
            }
        }
    }
}
