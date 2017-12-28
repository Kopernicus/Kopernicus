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

using System;
using Kopernicus.UI;

namespace Kopernicus
{
    namespace Configuration
    {
        [RequireConfigType(ConfigType.Node)]
        public class BiomeLoader : IParserEventSubscriber, ITypeParser<CBAttributeMapSO.MapAttribute>
        {
            // The map attribute object we are creating
            public CBAttributeMapSO.MapAttribute Value { get; set; }

            // The name of this biome
            [ParserTarget("name")]
            [KittopiaDescription("The name of this biome.")]
            public String name
            {
                get { return Value.name; }
                set
                {
                    Value.name = value;
                    Value.localizationTag = value; // This is not displayName because of reasons
                }
            }

            // The displayName of this biome
            [ParserTarget("displayName")]
            [KittopiaDescription("The displayed name of the biome. Can be a localization tag.")]
            public String displayName
            {
                get { return Value.localizationTag; } // This is not displayName because of reasons
                set { Value.localizationTag = value; }
            }

            // The science multiplier for this biome
            [ParserTarget("value")]
            [KittopiaDescription("A value that gets multiplied with every amount of science that is returned in the biome.")]
            public NumericParser<Single> value
            {
                get { return Value.value; }
                set { Value.value = value; }
            }

            // The color in the map for this attribute
            [ParserTarget("color")]
            [KittopiaDescription("The color of the biome on the biome map.")]
            public ColorParser color
            {
                get { return Value.mapColor; }
                set { Value.mapColor = value; }
            }

            // Parser apply event
            void IParserEventSubscriber.Apply(ConfigNode node)
            {
                Events.OnBiomeLoaderApply.Fire(this, node);
            }

            // Parser post apply event
            void IParserEventSubscriber.PostApply(ConfigNode node)
            {
                Events.OnBiomeLoaderPostApply.Fire(this, node);
            }

            // Allocate the biome descriptor
            [KittopiaConstructor(KittopiaConstructor.Parameter.Empty, purpose = KittopiaConstructor.Purpose.Create)]
            public BiomeLoader()
            {
                Value = new CBAttributeMapSO.MapAttribute();
            }

            // Get reference to existing biome descriptor
            [KittopiaConstructor(KittopiaConstructor.Parameter.Element, purpose = KittopiaConstructor.Purpose.Edit)]
            public BiomeLoader(CBAttributeMapSO.MapAttribute attribute)
            {
                Value = attribute;
            }
        }
    }
}
