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
using System.Linq;
using Kopernicus.ConfigParser.Attributes;
using Kopernicus.ConfigParser.BuiltinTypeParsers;
using Kopernicus.ConfigParser.Enumerations;
using Kopernicus.Configuration.Parsing;

namespace Kopernicus.Configuration.ModLoader
{
    [RequireConfigType(ConfigType.Node)]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class OceanFX : ModLoader<PQSMod_OceanFX>
    {
        // angle
        [ParserTarget("angle")]
        public NumericParser<Single> Angle
        {
            get { return Mod.angle; }
            set { Mod.angle = value; }
        }

        // The deformity of the map for the Quad Remover (?)
        [ParserTarget("blendA")]
        public NumericParser<Single> BlendA
        {
            get { return Mod.blendA; }
            set { Mod.blendA = value; }
        }

        // blendB
        [ParserTarget("blendB")]
        public NumericParser<Single> BlendB
        {
            get { return Mod.blendB; }
            set { Mod.blendB = value; }
        }

        // bump
        [ParserTarget("bump")]
        public Texture2DParser Bump
        {
            get { return Mod.bump; }
            set { Mod.bump = value; }
        }

        // framesPerSecond
        [ParserTarget("framesPerSecond")]
        public NumericParser<Single> FramesPerSecond
        {
            get { return Mod.framesPerSecond; }
            set { Mod.framesPerSecond = value; }
        }

        // fresnel (???)
        [ParserTarget("fresnel")]
        public Texture2DParser Fresnel
        {
            get { return Mod.fresnel; }
            set { Mod.fresnel = value; }
        }

        // oceanOpacity
        [ParserTarget("oceanOpacity")]
        public NumericParser<Single> OceanOpacity
        {
            get { return Mod.oceanOpacity; }
            set { Mod.oceanOpacity = value; }
        }

        // refraction
        [ParserTarget("refraction")]
        public Texture2DParser Refraction
        {
            get { return Mod.refraction; }
            set { Mod.refraction = value; }
        }

        // spaceAltitude
        [ParserTarget("spaceAltitude")]
        public NumericParser<Double> SpaceAltitude
        {
            get { return Mod.spaceAltitude; }
            set { Mod.spaceAltitude = value; }
        }

        // spaceSurfaceBlend
        [ParserTarget("spaceSurfaceBlend")]
        public NumericParser<Single> SpaceSurfaceBlend
        {
            get { return Mod.spaceSurfaceBlend; }
            set { Mod.spaceSurfaceBlend = value; }
        }

        // specColor
        [ParserTarget("specColor")]
        public ColorParser SpecColor
        {
            get { return Mod.specColor; }
            set { Mod.specColor = value; }
        }

        // texBlend
        [ParserTarget("texBlend")]
        public NumericParser<Single> TexBlend
        {
            get { return Mod.texBlend; }
            set { Mod.texBlend = value; }
        }

        // txIndex
        [ParserTarget("txIndex")]
        public NumericParser<Int32> TxIndex
        {
            get { return Mod.txIndex; }
            set { Mod.txIndex = value; }
        }

        // Watermain
        [ParserTargetCollection("Watermain")]
        public List<Texture2DParser> Watermain
        {
            get { return Mod.watermain.Select(t => new Texture2DParser(t)).ToList(); }
            set
            {
                Mod.watermain = value.Select(t => t.Value).ToArray();
                Mod.waterMainLength = value.Count;
            }
        }

        // Create the mod
        public override void Create(PQS pqsVersion)
        {
            // Call base
            base.Create(pqsVersion);

            // Create the base mod (I need to instance this one, because some parameters aren't loadable. :( )
            PSystemBody body = Utility.FindBody(Injector.StockSystemPrefab.rootBody, "Laythe");
            foreach (PQS ocean in body.pqsVersion.GetComponentsInChildren<PQS>(true))
            {
                if (ocean.name == "LaytheOcean")
                {
                    Utility.CopyObjectFields(ocean.GetComponentsInChildren<PQSMod_OceanFX>(true)[0], Mod);
                }
            }
        }

        // Create the mod
        public override void Create(PQSMod_OceanFX mod, PQS pqsVersion)
        {
            // Call base
            base.Create(mod, pqsVersion);

            // Create the base mod if needed
            if (Mod.reflection != null)
            {
                return;
            }
            PSystemBody body = Utility.FindBody(Injector.StockSystemPrefab.rootBody, "Laythe");
            foreach (PQS ocean in body.pqsVersion.GetComponentsInChildren<PQS>(true))
            {
                if (ocean.name == "LaytheOcean")
                {
                    Utility.CopyObjectFields(ocean.GetComponentsInChildren<PQSMod_OceanFX>(true)[0], Mod);
                }
            }
        }
    }
}

