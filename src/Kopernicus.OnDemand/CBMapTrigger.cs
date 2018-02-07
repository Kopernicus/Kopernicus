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

using UnityEngine;

namespace Kopernicus
{
    namespace OnDemand
    {
        public class CBMapTrigger : MonoBehaviour
        {
            private CelestialBody _body;

            void Start()
            {
                _body = GetComponent<CelestialBody>();
                GameEvents.onVesselSOIChanged.Add(OnVesselSOIChanged);
                GameEvents.onVesselLoaded.Add(OnVesselLoad);
                GameEvents.OnMapEntered.Add(OnMapEntered);
                GameEvents.OnMapExited.Add(OnMapExited);
                GameEvents.onGameSceneLoadRequested.Add(OnGameSceneLoadRequested);
            }

            private void OnVesselSOIChanged(GameEvents.HostedFromToAction<Vessel, CelestialBody> action)
            {
                if (action.@from.transform.name == _body.transform.name)
                {
                    OnDemandStorage.DisableBodyCBMaps(_body.transform.name);
                }

                if (action.@to.transform.name == _body.transform.name)
                {
                    OnDemandStorage.EnableBodyCBMaps(_body.transform.name);
                }
            }

            private void OnMapEntered()
            {
                // OnDemandStorage.EnableBodyCBMaps(_body.transform.name);
            }

            private void OnMapExited()
            {
                if (_body != FlightGlobals.currentMainBody)
                {
                    OnDemandStorage.DisableBodyCBMaps(_body.transform.name);
                }
            }

            private void OnGameSceneLoadRequested(GameScenes scene)
            {
                OnDemandStorage.DisableBodyCBMaps(_body.transform.name);
            }

            private void OnVesselLoad(Vessel vessel)
            {
                if (vessel.mainBody == _body)
                {
                    OnDemandStorage.EnableBodyCBMaps(_body.transform.name);
                }
                else
                {
                    OnDemandStorage.DisableBodyCBMaps(_body.transform.name);
                }
            }

            private void OnDestroy()
            {
                GameEvents.onVesselSOIChanged.Remove(OnVesselSOIChanged);
                GameEvents.onVesselLoaded.Remove(OnVesselLoad);
                GameEvents.OnMapEntered.Remove(OnMapEntered);
                GameEvents.OnMapExited.Remove(OnMapExited);
                GameEvents.onLevelWasLoaded.Remove(OnGameSceneLoadRequested);
            }
        }
    }
}