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
using System.Linq;
using Kopernicus.ConfigParser;
using Kopernicus.ConfigParser.BuiltinTypeParsers;
using UnityEngine;

namespace Kopernicus.Components
{
    public class ModuleSurfaceObjectTrigger : PartModule
    {
        /*
         * MODULE
         * {
         *     name = ModuleScatterTrigger
         *     module = ModuleScienceExperiment
         * 
         *     key = experimentID
         *     value = temperatureScan
         *
         *
         *     toggle = DeployExperiment
         * }
         */
        
        [KSPField(isPersistant =  false, guiActive = false)]
        public String module;

        [KSPField(isPersistant =  false, guiActive = false)]
        public String key;

        [KSPField(isPersistant =  false, guiActive = false)]
        public String value;

        [KSPField(isPersistant =  false, guiActive = false)]
        public String toggle;

        [KSPField(isPersistant =  false, guiActive = false)]
        public Single distance;

        [KSPField(isPersistant =  false, guiActive = false)]
        public String objectName;
        
        private PartModule _targetModule;
        private Boolean _isNearObject = true;
        private List<String> _toggle;
        private readonly Collider[] _colliders = new Collider[128];

        public override void OnStart(StartState state)
        {
            StringCollectionParser parser = new StringCollectionParser();
            parser.SetFromString(toggle);
            _toggle = parser;
            
            Type type = Parser.ModTypes.Find(t => t.Name == module && typeof(PartModule).IsAssignableFrom(t));
            for (Int32 i = 0; i < part.Modules.Count; i++)
            {
                PartModule partModule = part.Modules[i];
                if (!type.IsInstanceOfType(partModule))
                {
                    continue;
                }

                BaseField field = partModule.Fields[key];
                String fieldValue = field.GetValue(partModule).ToString();
                if (fieldValue != value)
                {
                    continue;
                }

                _targetModule = partModule;
                break;
            }
        }

        public override void OnUpdate()
        {
            // Check if we are near one
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
                if (!surfaceObject || surfaceObject.objectName != objectName)
                {
                    continue;
                }

                isNearObject = true;
                break;
            }

            // Has the value changed?
            if (isNearObject != _isNearObject)
            {
                if (isNearObject)
                {
                    _targetModule.SendMessage("OnObjectInRange", objectName);
                }
                else
                {
                    _targetModule.SendMessage("OnObjectOutOfRange", objectName);
                }
            }

            _isNearObject = isNearObject;
            
            if (_toggle == null)
            {
                return;
            }
            
            for (Int32 i = 0; i < _toggle.Count; i++)
            {
                if (_targetModule.Events[_toggle[i]] != null)
                {
                    _targetModule.Events[_toggle[i]].guiActive = _isNearObject;
                }
                if (_targetModule.Actions[_toggle[i]] != null)
                {
                    _targetModule.Actions[_toggle[i]].active = _isNearObject;
                }
                if (_targetModule.Fields[_toggle[i]] != null)
                {
                    _targetModule.Fields[_toggle[i]].guiActive = _isNearObject;
                }
            }
        }
    }
}