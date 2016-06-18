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

namespace Kopernicus
{
    namespace Configuration
    {
        namespace ModLoader
        {
            [RequireConfigType(ConfigType.Node)]
            public class MaterialFadeAltitudeDouble : ModLoader<PQSMod_MaterialFadeAltitudeDouble>
            {
                // inFadeEnd
                [ParserTarget("inFadeEnd")]
                public NumericParser<float> inFadeEnd
                {
                    get { return mod.inFadeEnd; }
                    set { mod.inFadeEnd = value; }
                }

                // inFadeStart
                [ParserTarget("inFadeStart")]
                public NumericParser<float> inFadeStart
                {
                    get { return mod.inFadeStart; }
                    set { mod.inFadeStart = value; }
                }

                // outFadeEnd
                [ParserTarget("outFadeEnd")]
                public NumericParser<float> outFadeEnd
                {
                    get { return mod.outFadeEnd; }
                    set { mod.outFadeEnd = value; }
                }

                // outFadeStart
                [ParserTarget("outFadeStart")]
                public NumericParser<float> outFadeStart
                {
                    get { return mod.outFadeStart; }
                    set { mod.outFadeStart = value; }
                }

                // valueEnd
                [ParserTarget("valueEnd")]
                public NumericParser<float> valueEnd
                {
                    get { return mod.valueEnd; }
                    set { mod.valueEnd = value; }
                }

                // valueMid
                [ParserTarget("valueMid")]
                public NumericParser<float> valueMid
                {
                    get { return mod.valueMid; }
                    set { mod.valueMid = value; }
                }

                // valueStart
                [ParserTarget("valueStart")]
                public NumericParser<float> valueStart
                {
                    get { return mod.valueStart; }
                    set { mod.valueStart = value; }
                }
            }
        }
    }
}