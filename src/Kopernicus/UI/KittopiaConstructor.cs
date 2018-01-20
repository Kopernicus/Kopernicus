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

namespace Kopernicus
{
    namespace UI
    {
        /// <summary>
        /// Defines the constructor that should be used when creating the object
        /// </summary>
        [AttributeUsage(AttributeTargets.Constructor | AttributeTargets.Method)]
        public class KittopiaConstructor : Attribute
        {
            /// <summary>
            /// Describes the parameters that should be passed to the constructor
            /// </summary>
            public enum Parameter
            {
                CelestialBody,
                Element,
                Empty
            }

            /// <summary>
            /// Describes how the constructor is used
            /// </summary>
            public enum Purpose
            {
                Create = 1,
                Edit = 2,
                Both = Create | Edit
            }
            
            /// <summary>
            /// The parameters for the constructor
            /// </summary>
            public Parameter[] parameters;

            /// <summary>
            /// Describes how the constructor is used
            /// </summary>
            public Purpose purpose = Purpose.Both;

            public KittopiaConstructor(params Parameter[] parameters)
            {
                this.parameters = parameters;
            }
        }
    }
}