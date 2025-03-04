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

using System.Diagnostics.CodeAnalysis;
using Kopernicus.ConfigParser;

namespace Kopernicus.Configuration.Parsing
{
    /// <summary>
    /// Provides basic static fields or methods for IParserEventSubscribers
    /// </summary>
    public class BaseLoader
    {
        /// <summary>
        /// Provides an override for the currently edited body
        /// </summary>
        private PSystemBody _currentBody;

        /// <summary>
        /// Singleton of the currently edited body
        /// </summary>
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        protected PSystemBody generatedBody
        {
            get
            {
                return _currentBody
                    ? _currentBody
                    : _currentBody = Parser.GetState<Body>("Kopernicus:currentBody")?.GeneratedBody;
            }
            set { _currentBody = value; }
        }
    }
}
