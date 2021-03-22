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
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using KSP;
using KSP.IO;
using UnityEngine;

namespace Kopernicus.Configuration
{
	public class ConfigReader
	{
		[Persistent]
		public bool EnforceShaders = false;
		[Persistent]
		public bool WarnShaders = false;
		[Persistent]
		public int EnforcedShaderLevel = 2;
		[Persistent]
		public int ScatterCullDistance = 5000;
		[Persistent]
		public string UseKopernicusAsteroidSystem = "True";
		[Persistent]
		public int SolarRefreshRate = 1;
		public UrlDir.UrlConfig[] baseConfigs;
		public void loadMainSettings()
		{
			baseConfigs = GameDatabase.Instance.GetConfigs("Kopernicus_config");
			if (baseConfigs.Length == 0)
			{
				Debug.LogWarning("No Kopernicus_Config file found, using defaults");
				return;
			}

			if (baseConfigs.Length > 1)
			{
				Debug.LogWarning("Multiple Kopernicus_Config files detected, check your install");
			}
			try
			{
				ConfigNode.LoadObjectFromConfig(this, baseConfigs[0].config);
			}
			catch
            {
				Debug.LogWarning("Error loading config, using defaults");
			}
		}
	}
}
