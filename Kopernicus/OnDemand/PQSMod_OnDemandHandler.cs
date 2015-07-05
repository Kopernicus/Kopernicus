/**
 * Kopernicus Planetary System Modifier
 * ====================================
 * Created by: - Bryce C Schroeder (bryce.schroeder@gmail.com)
 * 			   - Nathaniel R. Lewis (linux.robotdude@gmail.com)
 * 
 * Maintained by: - Thomas P.
 * 				  - NathanKell
 * 
* Additional Content by: Gravitasi, aftokino, KCreator, Padishar, Kragrathea, OvenProofMars, zengei, MrHappyFace
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
 * which is copyright 2011-2014 Squad. Your usage of Kerbal Space Program
 * itself is governed by the terms of its EULA, not the license above.
 * 
 * https://kerbalspaceprogram.com
 */

using UnityEngine;

namespace Kopernicus
{
    namespace OnDemand
    {
        public class PQSMod_OnDemandHandler : PQSMod
        {
            private bool isLoaded = false;
            private bool isAdded = false;
            private bool isHomeworld = false;

            private void DisableMapSO(GameScenes scene)
            {
                if (!isHomeworld && isLoaded)
                {
                    if (OnDemandStorage.DisableBody(base.sphere.name))
                    {
                        Debug.Log("[OD]: Disabled Body " + base.sphere.name);
                        isLoaded = false;
                    }
                }
            }

            private void EnableMapSO()
            {
                if (OnDemandStorage.EnableBody(base.sphere.name))
                {
                    Debug.Log("[OD]: Enabled Body " + base.sphere.name);
                    isLoaded = true;
                }
            }

            public override void OnSphereInactive()
            {
                Disable();
            }
            public override void OnQuadPreBuild(PQ quad)
            {
                Enable();
            }
            /*public override void OnVertexBuildHeight(PQS.VertexBuildData data)
            {
                Enable();
            }
            public override void OnVertexBuild(PQS.VertexBuildData data)
            {
                Enable();
            }
            public override void OnMeshBuild()
            {
                Enable();
            }
            public override void OnQuadUpdate(PQ quad)
            {
                Enable();
            }*/
            public override void OnSetup()
            {
                base.OnSetup();
                CelestialBody cb = Part.GetComponentUpwards<CelestialBody>(this.gameObject);
                if (cb != null && cb.isHomeWorld)
                    isHomeworld = true;

                if (isHomeworld && !isLoaded)
                    EnableMapSO();

                /*if(!isAdded)
                {
                    GameEvents.onGameSceneLoadRequested.Add(new EventData<GameScenes>.OnEvent(DisableMapSO));
                    isAdded = true;
                }*/
            }

            public void Enable()
            {   
                if (!isLoaded && !Injector.dontUpdate)
                {
                    EnableMapSO();
                }
            }

            public void Disable()
            {
                if (isLoaded && !Injector.dontUpdate && !base.sphere.isActive)
                {
                    DisableMapSO(HighLogic.LoadedScene);
                }
            }

            /*public void OnDestroy()
            {
                GameEvents.onGameSceneLoadRequested.Remove(new EventData<GameScenes>.OnEvent(DisableMapSO));
            }*/
        }
    }
}