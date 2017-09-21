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

namespace Kopernicus
{
    namespace Configuration
    {
        /// <summary>
        /// Provides basic static fields or methods for IParserEventSubscribers
        /// </summary>
        public class BaseLoader
        {
            /// <summary>
            /// Provides an override for the currently edited body
            /// </summary>
            private PSystemBody currentBody = null;

            /// <summary>
            /// Singleton of the currently edited body
            /// </summary>
            protected PSystemBody generatedBody
            {
                get { return currentBody ?? Loader.currentBody?.generatedBody; }
                set { currentBody = value; }
            }
        }
    }
}
