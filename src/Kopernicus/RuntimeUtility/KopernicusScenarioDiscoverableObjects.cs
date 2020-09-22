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
using Kopernicus.Configuration.Asteroids;
using KSPAchievements;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace Kopernicus.RuntimeUtility
{
	[KSPScenario(ScenarioCreationOptions.AddToAllGames, GameScenes.FLIGHT, GameScenes.TRACKSTATION, GameScenes.SPACECENTER)]
	public class KopernicusScenarioDiscoverableObjects : ScenarioDiscoverableObjects
	{
		private bool discoveryUnlocked;

		private string tmpIDs;

		private string[] tmpIDList;

		private uint tmpId;

		new public static ScenarioDiscoverableObjects Instance
		{
			get;
			protected set;
		}

		public override void OnLoad(ConfigNode node)
		{
			untrackedObjectIDs = loadIDList(node, "untrackedIDs");
			discoveredObjectIDs = loadIDList(node, "discoveredIDs");
		}

		private List<uint> loadIDList(ConfigNode node, string listName)
		{
			List<uint> list = new List<uint>();
			tmpIDs = "";
			node.TryGetValue(listName, ref tmpIDs);
			tmpIDList = tmpIDs.Split(',');
			for (int i = 0; i < tmpIDList.Length; i++)
			{
				tmpId = 0u;
				if (uint.TryParse(tmpIDList[i], out tmpId))
				{
					list.Add(tmpId);
				}
			}
			return list;
		}

		public override void OnSave(ConfigNode node)
		{
			SaveIDList(node, "untrackedIDs", untrackedObjectIDs);
			SaveIDList(node, "discoveredIDs", discoveredObjectIDs);
		}

		private void SaveIDList(ConfigNode node, string name, List<uint> loadList)
		{
			tmpIDs = string.Join(",", loadList);
			node.AddValue(name, tmpIDs);
		}

		public override void OnAwake()
		{
			if (Instance != null && Instance != this)
			{
				Debug.LogError("[ScenarioDiscoverableObjects]: Instance already exists!", Instance.gameObject);
				UnityEngine.Object.Destroy(this);
				return;
			}
			Instance = this;
			if (sizeCurve == null)
			{
				sizeCurve = new FloatCurve();
				sizeCurve.Add(0f, 0f);
				sizeCurve.Add(0.3f, 0.45f);
				sizeCurve.Add(0.7f, 0.55f);
				sizeCurve.Add(1f, 1f);
			}
			discoveredObjectIDs = new List<uint>();
			untrackedObjectIDs = new List<uint>();
			GameEvents.onKnowledgeChanged.Add(OnKnowledgeChanged);
			GameEvents.onVesselDestroy.Add(OnVesselDestroy);
		}

		private void OnKnowledgeChanged(GameEvents.HostedFromToAction<IDiscoverable, DiscoveryLevels> kChg)
		{
			Vessel vessel = kChg.host as Vessel;
			if (!(vessel == null) && discoveredObjectIDs.Contains(vessel.persistentId))
			{
				if (vessel.DiscoveryInfo.HaveKnowledgeAbout(DiscoveryLevels.StateVectors))
				{
					untrackedObjectIDs.Remove(vessel.persistentId);
				}
				else
				{
					untrackedObjectIDs.AddUnique(vessel.persistentId);
				}
			}
		}

		private void OnVesselDestroy(Vessel v)
		{
			discoveredObjectIDs.Remove(v.persistentId);
			untrackedObjectIDs.Remove(v.persistentId);
		}

		new public void Start()
		{
			UnityEngine.Random.InitState(lastSeed);
			discoveryUnlocked = GameVariables.Instance.UnlockedSpaceObjectDiscovery(ScenarioUpgradeableFacilities.GetFacilityLevel(SpaceCenterFacility.TrackingStation));
			int count = untrackedObjectIDs.Count;
			while (count-- > 0)
			{
				if (!FlightGlobals.PersistentVesselIds.Contains(untrackedObjectIDs[count]))
				{
					untrackedObjectIDs.RemoveAt(count);
				}
			}
		}

		private void OnDestroy()
		{
			GameEvents.onKnowledgeChanged.Remove(OnKnowledgeChanged);
			GameEvents.onVesselDestroy.Remove(OnVesselDestroy);
			discoveredObjectIDs = null;
			untrackedObjectIDs = null;
		}
		//unused
		new public void SpawnAsteroid()
		{
			return;
		}
		//unused
		new public void SpawnLastAsteroid()
		{
			return;
		}
		//unusued
		new public void SpawnHomeAsteroid(int asteroidSeed)
		{
			return;
		}

		public ProtoVessel SpawnNewAsteroid(uint asteroidSeed, string name, Orbit orbit, UntrackedObjectClass asteroidClass, double lifeTime, double lifeTimeMax)
		{
			double randomDuration = GetRandomDuration();
			ProtoVessel protoVessel = DiscoverableObjectsUtil.SpawnAsteroid(name, orbit, asteroidSeed, asteroidClass, lifeTime, lifeTimeMax);
			discoveredObjectIDs.Add(protoVessel.persistentId);
			untrackedObjectIDs.Add(protoVessel.persistentId);
			return protoVessel;

		}

		//unused
		new public void SpawnDresAsteroid(int asteroidSeed)
		{
			return;
		}

		[ContextMenu("Spawn A Comet")]
		new public void SpawnComet()
		{
			CometOrbitType cometType = CometManager.GenerateWeightedCometType();
			SpawnComet(cometType);
		}

		internal void SpawnComet(string typeName)
		{
			CometOrbitType cometOrbitType = CometManager.GetCometOrbitType(typeName);
			if (cometOrbitType == null)
			{
				Debug.LogWarning("[ScenarioDiscoverableObject] Unable to find Spawn Comet Type: " + typeName);
			}
			else
			{
				SpawnComet(cometOrbitType);
			}
		}

		internal void SpawnComet(CometOrbitType cometType)
		{
			int seed = UnityEngine.Random.Range(0, int.MaxValue);
			UnityEngine.Random.InitState(seed);
			lastSeed = seed;
			double lifeTime = (double)UnityEngine.Random.Range(minUntrackedLifetime, maxUntrackedLifetime) * 24.0 * 60.0 * 60.0;
			double lifeTimeMax = (double)maxUntrackedLifetime * 24.0 * 60.0 * 60.0;
			UntrackedObjectClass randomObjClass = cometType.GetRandomObjClass();
			CometDefinition cometDef = CometManager.GenerateDefinition(cometType, randomObjClass, seed);
			Orbit o = cometType.CalculateHomeOrbit();
			DiscoverableObjectsUtil.SpawnComet(DiscoverableObjectsUtil.GenerateCometName(), o, cometDef, (uint)seed, randomObjClass, lifeTime, lifeTimeMax, optimizedCollider: false, 0f);
			Debug.Log("[CometSpawner]: New object found near " + FlightGlobals.GetHomeBodyName());
		}

		private double GetRandomDuration()
		{
			return UnityEngine.Random.Range(15f, 60f);
		}

		private bool ReachedBody(string bodyName)
		{
			if (ProgressTracking.Instance != null)
			{
				CelestialBodySubtree bodyTree = ProgressTracking.Instance.GetBodyTree(bodyName);
				if (bodyTree != null && bodyTree.IsReached)
				{
					return true;
				}
			}
			return false;
		}
	}
}
