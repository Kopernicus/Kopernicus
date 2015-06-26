/**
 * Kopernicus Planetary System Modifier
 * Copyright (C) 2014 Bryce C Schroeder (bryce.schroeder@gmail.com), Nathaniel R. Lewis (linux.robotdude@gmail.com)
 * 
 * http://www.ferazelhosting.net/~bryce/contact.html
 * 
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

            private void DisableMapSO(GameScenes scene)
            {
                if (OnDemandStorage.DisableBody(base.sphere.name))
                {
                    Debug.Log("[OD]: Disabled Body " + base.sphere.name);
                    isLoaded = false;
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

            public override void OnSetup()
            {
                if (!isAdded)
                {
                    GameEvents.onGameSceneLoadRequested.Add(new EventData<GameScenes>.OnEvent(DisableMapSO));
                    isAdded = true;
                }

                // tracking station enables all PQSs. For some reason. So we ignore it.
                if (!isLoaded && !Injector.dontUpdate && HighLogic.LoadedScene != GameScenes.TRACKSTATION)
                {
                    EnableMapSO();
                }
            }

            public override void OnSphereInactive()
            {
                if (isLoaded && !Injector.dontUpdate && !base.sphere.isAlive)
                {
                    DisableMapSO(HighLogic.LoadedScene);
                }
            }

            public void OnDestroy()
            {
                GameEvents.onGameSceneLoadRequested.Remove(new EventData<GameScenes>.OnEvent(DisableMapSO));
            }
        }
    }
}