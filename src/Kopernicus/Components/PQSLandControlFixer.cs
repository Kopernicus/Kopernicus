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
        private PQSLandControl[] _landControls;
        private static FieldInfo lcScatterListField;
        private PQ _quad;
        private bool createColors = true;
        private bool createScatter = true;

        // I have no idea what Squad did to LandControl but it worked just fine before
        public override void OnSetup()
        {
            CelestialBody cb = null;
            PQSLandControl pqsLC = null;
            try
            {
                cb = FlightGlobals.GetBodyByName(sphere.name);
            }
            catch
            {
            }
            try
            {
                pqsLC = ((PQSLandControl)typeof(PQS).GetFields(BindingFlags.NonPublic | BindingFlags.Instance).First(f => f.FieldType == typeof(PQSLandControl)).GetValue(sphere));
                createColors = pqsLC.createColors;
                createScatter = pqsLC.createScatter;
            }
            catch
            {
                createColors = true;
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
            // Try to cache density values that are used to distribute scatters
            _landControls = sphere.GetComponentsInChildren<PQSLandControl>(true);
            if (lcScatterListField != null)
            {
                return;
            }

            lcScatterListField = typeof(PQSLandControl).GetFields(BindingFlags.NonPublic | BindingFlags.Instance)
                .First(f => f.FieldType == typeof(Double[]));
        }

        public override void OnQuadPreBuild(PQ quad)
        {
            _quad = quad;
        }

        public override void OnMeshBuild()
        {
            Dictionary<String, Double> densities = new Dictionary<String, Double>();
            for (Int32 i = 0; i < _landControls.Length; i++)
            {
                if (!(lcScatterListField.GetValue(_landControls[i]) is Double[] lcScatterList))
                {
                    continue;
                }

                for (Int32 j = 0; j < _landControls[i].scatters.Length; j++)
                {
                    if (lcScatterList[j] <= 0)
                    {
                        continue;
                    }

                    PQSLandControl.LandClassScatter scatter = _landControls[i].scatters[j];
                    if (densities.ContainsKey(scatter.scatterName))
                    {
                        densities[scatter.scatterName] = lcScatterList[j];
                    }
                    else
                    {
                        densities.Add(scatter.scatterName, lcScatterList[j]);
                    }
                }
            }
            _quad.gameObject.AddOrGetComponent<DensityContainer>().densities = densities;
        }
    }

    public class DensityContainer : MonoBehaviour
    {
        public Dictionary<String, Double> densities;
    }
}
