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

using Kopernicus.Constants;
using UnityEngine;

namespace Kopernicus.RuntimeUtility
{
    [KSPAddon(KSPAddon.Startup.Instantly, false)]
    public class TerrainQualitySetter : MonoBehaviour
    {
        private void Start()
        {
            if (!CompatibilityChecker.IsCompatible())
            {
                Destroy(this);
                return;
            }

            DontDestroyOnLoad(gameObject);
        }

        private void Update()
        {
            if (GameSettings.TERRAIN_SHADER_QUALITY == 3)
            {
                return;
            }
            if ((RuntimeUtility.KopernicusConfig.WarnShaders) && (!HighLogic.LoadedSceneIsGame))
            {
                ScreenMessages.PostScreenMessage("Kopernicus only supports terrain quality ultra!\nSome terrain packs may not work!.", 5f, ScreenMessageStyle.UPPER_LEFT);
            }
            if (RuntimeUtility.KopernicusConfig.EnforceShaders)
            {
                GameSettings.TERRAIN_SHADER_QUALITY = 3;
                GameSettings.SaveSettings();
            }
        }
    }
}