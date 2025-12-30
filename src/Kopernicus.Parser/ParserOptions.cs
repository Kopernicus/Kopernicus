/**
 * Kopernicus ConfigNode Parser
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
using UnityEngine;

namespace Kopernicus.ConfigParser
{
    /// <summary>
    /// A handler to store options for the parsing library
    /// </summary>
    public static class ParserOptions
    {
        public class Data
        {
            /// <summary>
            /// The function that should be used for log output
            /// </summary>
            public Action<System.Object> LogCallback = Debug.Log;

            /// <summary>
            /// The function that should be used for error output
            /// </summary>
            public Action<Exception> ErrorCallback = Debug.LogException;
        }

        /// <summary>
        /// The data for each mod
        /// </summary>
        internal static readonly Dictionary<String, Data> Options = new Dictionary<String, Data>
        {
            {"Default", new Data()},
            {"Internal", new Data {LogCallback = s => { }, ErrorCallback = s => { }}}
        };

        /// <summary>
        /// Registers the settings for a mod
        /// </summary>
        public static void Register(String modName, Data data)
        {
            if (Options.ContainsKey(modName))
            {
                Options[modName] = data;
            }
            else
            {
                Options.Add(modName, data);
            }
        }
    }
}