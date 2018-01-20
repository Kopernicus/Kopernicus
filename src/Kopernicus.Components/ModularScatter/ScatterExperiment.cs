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
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Kopernicus
{
    namespace Components
    {
        /// <summary>
        /// A Scatter Component that can add science results to a scatter object
        /// </summary>
        public class ScatterExperiment : IComponent<ModularScatter>
        {
            /// <summary>
            /// Contains the patched vessels (=> Kerbals)
            /// </summary>
            public static List<Guid> vesselID = new List<Guid>();

            /// <summary>
            /// Contains the blocked List of Base-Events
            /// </summary>
            public List<BaseEvent> blocked = new List<BaseEvent>();

            /// <summary>
            /// Which experiment should we run?
            /// </summary>
            public ConfigNode experimentNode;

            /// <summary>
            /// The experiment
            /// </summary>
            public ModuleScienceExperiment experiment;

            /// <summary>
            /// Gets executed every frame and checks if a Kerbal is within the range of the scatter object
            /// </summary>
            void IComponent<ModularScatter>.Update(ModularScatter system)
            {
                // Node null
                if (experimentNode == null)
                    return;

                // Process science
                if (!FlightGlobals.ActiveVessel)
                    return;

                // We need eva's
                if (!FlightGlobals.ActiveVessel.isEVA)
                    return;

                // We need an Experiment
                if (FlightGlobals.ActiveVessel.evaController.part.FindModulesImplementing<ModuleScienceExperiment>().All(e => e.experimentID != experimentNode.GetValue("experimentID")) &&
                    !vesselID.Contains(FlightGlobals.ActiveVessel.id) &&
                    !FlightGlobals.ActiveVessel.packed)
                    AddScienceExperiment();

                if (experiment == null)
                    return;

                // Toggle the experiment
                ToggleExperiment(Physics.OverlapSphere(FlightGlobals.ship_position, 10f).Any(c => c.gameObject.transform.parent.name == system.name));
            }

            /// <summary>
            /// Loads the experiment from the config node and attaches it to a vessel
            /// </summary>
            void AddScienceExperiment()
            {
                // If the Node is null, abort
                if (experimentNode == null)
                    return;

                // Create the ScienceExperiment
                Part kerbal = FlightGlobals.ActiveVessel.evaController.part;
                experiment = kerbal.AddModule(typeof(ModuleScienceExperiment).Name) as ModuleScienceExperiment;
                if (experiment == null)
                    return;

                // Load the experiment
                ConfigNode.LoadObjectFromConfig(experiment, experimentNode);

                // Deactivate some things
                experiment.resettable = false;

                // Start the experiment
                experiment.OnStart(PartModule.StartState.None);
                vesselID.Add(kerbal.vessel.id);
            }

            /// <summary>
            /// Toggles the visibility of the experiment
            /// </summary>
            void ToggleExperiment(Boolean state)
            {
                // Activate
                if (state && blocked.Contains(experiment.Events["DeployExperiment"]))
                {
                    experiment.Events["DeployExperiment"].guiActive = true;
                    blocked.Remove(experiment.Events["DeployExperiment"]);
                }

                // Deactivate
                else if (!state && !blocked.Contains(experiment.Events["DeployExperiment"]))
                {
                    experiment.Events["DeployExperiment"].guiActive = false;
                    blocked.Add(experiment.Events["DeployExperiment"]);
                }
            }
            
            void IComponent<ModularScatter>.Apply(ModularScatter system)
            {
                // We don't use this
            }

            void IComponent<ModularScatter>.PostApply(ModularScatter system)
            {
                // We don't use this
            }
        }
    }
}