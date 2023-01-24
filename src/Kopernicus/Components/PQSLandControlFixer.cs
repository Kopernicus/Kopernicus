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
using System.Reflection;
using Kopernicus.Components.MaterialWrapper;
using Kopernicus.Configuration;
using UnityEngine;

namespace Kopernicus.Components
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class PQSLandControlFixer : PQSMod
    {
        // Cache density values for scatters
        private bool createScatter = true;

        // I have no idea what Squad did to LandControl but it worked just fine before
        public override void OnSetup()
        {
            PQSLandControl pqsLC = null;
            try
            {
                pqsLC = ((PQSLandControl)typeof(PQS).GetFields(BindingFlags.NonPublic | BindingFlags.Instance).First(f => f.FieldType == typeof(PQSLandControl)).GetValue(sphere));
                createScatter = pqsLC.createScatter;
            }
            catch
            {
                createScatter = true;
            }
            try
            {
                if (pqsLC)
                {
                    pqsLC.createColors = true;
                    pqsLC.createScatter = createScatter;
                }
            }
            catch
            {
                //woo, no LandControl at all.
            }
        }
    }
}
