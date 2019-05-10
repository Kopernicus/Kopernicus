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
using Kopernicus.Components.ModularComponentSystem;
using UnityEngine;

namespace Kopernicus.Components.ModularScatter
{
    /// <summary>
    /// A Scatter Component that can add science results to a scatter object
    /// </summary>
    [SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
    public class ScatterExperimentComponent : IComponent<ModularScatter>
    {
        /// <summary>
        /// Contains the patched vessels (=> Kerbals)
        /// </summary>
        private static readonly List<Guid> VesselId = new List<Guid>();

        /// <summary>
        /// Contains the blocked List of Base-Events
        /// </summary>
        private readonly List<BaseEvent> _blocked = new List<BaseEvent>();

        /// <summary>
        /// Which experiment should we run?
        /// </summary>
        public ConfigNode ExperimentNode;

        /// <summary>
        /// The experiment
        /// </summary>
        private ModuleScienceExperiment _experiment;

        /// <summary>
        /// Gets executed every frame and checks if a Kerbal is within the range of the scatter object
        /// </summary>
        void IComponent<ModularScatter>.Update(ModularScatter system)
        {
            // Node null
            if (ExperimentNode == null)
            {
                return;
            }

            // Process science
            if (!FlightGlobals.ActiveVessel)
            {
                return;
            }

            // We need eva's
            if (!FlightGlobals.ActiveVessel.isEVA)
            {
                return;
            }

            // We need an Experiment
            if (FlightGlobals.ActiveVessel.evaController.part.FindModulesImplementing<ModuleScienceExperiment>()
                    .All(e => e.experimentID != ExperimentNode.GetValue("experimentID")) &&
                !VesselId.Contains(FlightGlobals.ActiveVessel.id) &&
                !FlightGlobals.ActiveVessel.packed)
            {
                AddScienceExperiment();
            }

            if (_experiment == null)
            {
                return;
            }

            // Toggle the experiment
            ToggleExperiment(Physics.OverlapSphere(FlightGlobals.ship_position, 10f)
                .Any(c => c.gameObject.transform.parent.name == system.name));
        }

        /// <summary>
        /// Loads the experiment from the config node and attaches it to a vessel
        /// </summary>
        private void AddScienceExperiment()
        {
            // If the Node is null, abort
            if (ExperimentNode == null)
            {
                return;
            }

            // Create the ScienceExperiment
            Part kerbal = FlightGlobals.ActiveVessel.evaController.part;
            _experiment = kerbal.AddModule(typeof(ModuleScienceExperiment).Name) as ModuleScienceExperiment;
            if (_experiment == null)
            {
                return;
            }

            // Load the experiment
            ConfigNode.LoadObjectFromConfig(_experiment, ExperimentNode);

            // Deactivate some things
            _experiment.resettable = false;

            // Start the experiment
            _experiment.OnStart(PartModule.StartState.None);
            VesselId.Add(kerbal.vessel.id);
        }

        /// <summary>
        /// Toggles the visibility of the experiment
        /// </summary>
        private void ToggleExperiment(Boolean state)
        {
            // Activate
            if (state && _blocked.Contains(_experiment.Events["DeployExperiment"]))
            {
                _experiment.Events["DeployExperiment"].guiActive = true;
                _blocked.Remove(_experiment.Events["DeployExperiment"]);
            }

            // Deactivate
            else if (!state && !_blocked.Contains(_experiment.Events["DeployExperiment"]))
            {
                _experiment.Events["DeployExperiment"].guiActive = false;
                _blocked.Add(_experiment.Events["DeployExperiment"]);
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