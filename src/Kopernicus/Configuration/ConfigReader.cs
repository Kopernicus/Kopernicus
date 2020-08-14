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
		public bool EnforceShaders = true;
		[Persistent]
		public bool WarnShaders = true;
		[Persistent]
		public bool UseStockAsteroidGenerator = true;

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
