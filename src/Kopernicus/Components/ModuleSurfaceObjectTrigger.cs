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
using Kopernicus.Components.Serialization;
using Kopernicus.ConfigParser;
using Kopernicus.ConfigParser.Attributes;
using Kopernicus.ConfigParser.BuiltinTypeParsers;
using Kopernicus.ConfigParser.Enumerations;
using Kopernicus.UI;
using UnityEngine;

namespace Kopernicus.Components
{
    [Serializable]
    public class ModuleSurfaceObjectTrigger : SerializablePartModule
    {
        /*
         * MODULE
         * {
         *     name = ModuleSurfaceObjectTrigger
         *     module = ModuleScienceExperiment
         *     distance = 20
         *     Properties
         *     {
         *         experimentID = scatterSample
         *     }
         *     Objects
         *     {
         *         name = Tree00
         *         name = boulder
         *     }
         *     Toggles
         *     {
         *         name = DeployExperiment
         *     }
         * }
         */

        [ParserTarget("module")]
        public String module;

        [ParserTargetCollection("Properties", NameSignificance = NameSignificance.None)]
        public Dictionary<String, String> properties;

        [ParserTargetCollection("Toggles", NameSignificance = NameSignificance.Key, Key = "name")]
        public List<String> toggles;

        [ParserTargetCollection("Objects", NameSignificance = NameSignificance.Key, Key = "name")]
        public List<String> objects;

        [ParserTarget("distance")]
        public NumericParser<Single> distance;

        private PartModule _targetModule;
        private Boolean _isNearObject = true;
        private readonly Collider[] _colliders = new Collider[128];

        public override void OnLoad(ConfigNode node)
        {
            Parser.LoadObjectFromConfigurationNode(this, node);
        }

        public override void OnSave(ConfigNode node)
        {
            PlanetConfigExporter.WriteToConfig(this, ref node);
        }

        public override void OnStart(StartState state)
        {
            Type type = Parser.ModTypes.Find(t => t.Name == module && typeof(PartModule).IsAssignableFrom(t));
            for (Int32 i = 0; i < part.Modules.Count; i++)
            {
                PartModule partModule = part.Modules[i];
                if (!type.IsInstanceOfType(partModule))
                {
                    continue;
                }

                Boolean isModule = true;
                foreach (KeyValuePair<String, String> keyValuePair in properties)
                {
                    BaseField field = partModule.Fields[keyValuePair.Key];
                    String fieldValue = field.GetValue(partModule).ToString();
                    if (fieldValue != keyValuePair.Value)
                    {
                        isModule = false;
                    }
                }

                if (!isModule)
                {
                    continue;
                }

                _targetModule = partModule;
                break;
            }
        }

        public override void OnUpdate()
        {
            // Check if we are near a surface feature
            Boolean isNearObject = false;
            Physics.OverlapSphereNonAlloc(FlightGlobals.ship_position, distance, _colliders);
            for (Int32 i = 0; i < _colliders.Length; i++)
            {
                if (_colliders[i] == null)
                {
                    continue;
                }

                if (!_colliders[i].gameObject.activeSelf)
                {
                    continue;
                }

                KopernicusSurfaceObject surfaceObject = _colliders[i].GetComponent<KopernicusSurfaceObject>();
                if (!surfaceObject || !objects.Contains(surfaceObject.objectName))
                {
                    continue;
                }

                isNearObject = true;
                break;
            }

            // Has the value changed?
            if (isNearObject != _isNearObject)
            {
                _targetModule.SendMessage(isNearObject ? "OnObjectInRange" : "OnObjectOutOfRange");
            }

            _isNearObject = isNearObject;

            if (toggles == null)
            {
                return;
            }

            for (Int32 i = 0; i < toggles.Count; i++)
            {
                if (_targetModule.Events[toggles[i]] != null)
                {
                    _targetModule.Events[toggles[i]].guiActive = _isNearObject;
                }
                if (_targetModule.Actions[toggles[i]] != null)
                {
                    _targetModule.Actions[toggles[i]].active = _isNearObject;
                }
                if (_targetModule.Fields[toggles[i]] != null)
                {
                    _targetModule.Fields[toggles[i]].guiActive = _isNearObject;
                }
            }
        }
    }
}
