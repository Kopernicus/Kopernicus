/**
 * Kopernicus Planetary System Modifier
 * ====================================
 * Created by: - Bryce C Schroeder (bryce.schroeder@gmail.com)
 * 			   - Nathaniel R. Lewis (linux.robotdude@gmail.com)
 * 
 * Maintained by: - Thomas P.
 * 				  - NathanKell
 * 
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
 * which is copyright 2011-2014 Squad. Your usage of Kerbal Space Program
 * itself is governed by the terms of its EULA, not the license above.
 * 
 * https://kerbalspaceprogram.com
 */

using System;
using UnityEngine;

namespace Kopernicus
{
    namespace Configuration
    {
        namespace ModLoader
        {
            [RequireConfigType(ConfigType.Node)]
            public class MaterialFadeAltitudeDouble : ModLoader, IParserEventSubscriber
            {
                // Actual PQS mod we are loading
                private PQSMod_MaterialFadeAltitudeDouble _mod;

                // inFadeEnd
                [ParserTarget("inFadeEnd", optional = true)]
                private NumericParser<float> inFadeEnd
                {
                    set { _mod.inFadeEnd = value.value; }
                }

                // inFadeStart
                [ParserTarget("inFadeStart", optional = true)]
                private NumericParser<float> inFadeStart
                {
                    set { _mod.inFadeStart = value.value; }
                }

                // outFadeEnd
                [ParserTarget("outFadeEnd", optional = true)]
                private NumericParser<float> outFadeEnd
                {
                    set { _mod.outFadeEnd = value.value; }
                }

                // outFadeStart
                [ParserTarget("outFadeStart", optional = true)]
                private NumericParser<float> outFadeStart
                {
                    set { _mod.outFadeStart = value.value; }
                }

                // valueEnd
                [ParserTarget("valueEnd", optional = true)]
                private NumericParser<float> valueEnd
                {
                    set { _mod.valueEnd = value.value; }
                }

                // valueMid
                [ParserTarget("valueMid", optional = true)]
                private NumericParser<float> valueMid
                {
                    set { _mod.valueMid = value.value; }
                }

                // valueStart
                [ParserTarget("valueStart", optional = true)]
                private NumericParser<float> valueStart
                {
                    set { _mod.valueStart = value.value; }
                }

                void IParserEventSubscriber.Apply(ConfigNode node)
                {

                }

                void IParserEventSubscriber.PostApply(ConfigNode node)
                {

                }

                public MaterialFadeAltitudeDouble()
                {
                    // Create the base mod
                    GameObject modObject = new GameObject("MaterialFadeAltitudeDouble");
                    modObject.transform.parent = Utility.Deactivator;
                    _mod = modObject.AddComponent<PQSMod_MaterialFadeAltitudeDouble>();
                    base.mod = _mod;
                }

                public MaterialFadeAltitudeDouble(PQSMod template)
                {
                    _mod = template as PQSMod_MaterialFadeAltitudeDouble;
                    _mod.transform.parent = Utility.Deactivator;
                    base.mod = _mod;
                }
            }
        }
    }
}

