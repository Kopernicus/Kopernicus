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
using Kopernicus.Configuration.Asteroids;
using Kopernicus.Constants;
using KSPAchievements;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace Kopernicus.RuntimeUtility
{
    // Class to manage Asteroids
    [KSPScenario(ScenarioCreationOptions.AddToAllGames, GameScenes.FLIGHT, GameScenes.TRACKSTATION, GameScenes.SPACECENTER)]
    public class DiscoverableObjects : ScenarioModule
    {
        //The ScenarioDiscoverableObjects system
        public ScenarioDiscoverableObjects scenario = null;
        public KopernicusScenarioDiscoverableObjects kopernicusScenario = null;

        // All asteroid configurations we know
        public static List<Asteroid> Asteroids { get; }

        // Spawn interval
        public Single spawnInterval = 0.1f;

        // Construct
        static DiscoverableObjects()
        {
            Asteroids = new List<Asteroid>();
        }

        public override void OnAwake()
        {
            if (!CompatibilityChecker.IsCompatible())
            {
                Destroy(this);
                return;
            }
            base.OnAwake();
        }

        // Startup
        private void Start()
        {
            //Replace ScenarioDiscoverableObjects with our Override class
            if (HighLogic.CurrentGame.RemoveProtoScenarioModule(typeof(ScenarioDiscoverableObjects)))
            {
                // RemoveProtoScenarioModule doesn't remove the actual Scenario; workaround!
                foreach (Object o in
                    Resources.FindObjectsOfTypeAll(typeof(ScenarioDiscoverableObjects)))
                {
                    scenario = (ScenarioDiscoverableObjects)o;
                    scenario.StopAllCoroutines();
                    Destroy(scenario);
                    scenario = new KopernicusScenarioDiscoverableObjects();
                    kopernicusScenario = (KopernicusScenarioDiscoverableObjects)scenario;
                    kopernicusScenario.OnAwake();
                }
                Debug.Log("[Kopernicus] ScenarioDiscoverableObjects successfully replaced.");
            }

            foreach (Asteroid asteroid in Asteroids)
            {
                StartCoroutine(AsteroidDaemon(asteroid));
            }
        }

        // Update the Asteroids
        public void UpdateAsteroid(Asteroid asteroid, Double time)
        {
            List<Vessel> spaceObjects = FlightGlobals.Vessels.Where(v => !v.DiscoveryInfo.HaveKnowledgeAbout(DiscoveryLevels.StateVectors) && Math.Abs(v.DiscoveryInfo.GetSignalLife(Planetarium.GetUniversalTime())) < 0.01).ToList();
            Int32 limit = Random.Range(asteroid.SpawnGroupMinLimit, asteroid.SpawnGroupMaxLimit);
            if (spaceObjects.Any())
            {
                Vessel vessel = spaceObjects.First();
                Debug.Log("[Kopernicus] " + vessel.vesselName + " has been untracked for too long and is now lost.");
                vessel.Die();
            }
            else if (GameVariables.Instance.UnlockedSpaceObjectDiscovery(ScenarioUpgradeableFacilities.GetFacilityLevel(SpaceCenterFacility.TrackingStation)) || HighLogic.CurrentGame.Mode == Game.Modes.SANDBOX)
            {
                Int32 untrackedCount = FlightGlobals.Vessels.Count(v => !v.DiscoveryInfo.HaveKnowledgeAbout(DiscoveryLevels.Owned)) - spaceObjects.Count;
                Int32 max = Mathf.Max(untrackedCount, limit);
                if (max <= untrackedCount)
                {
                    return;
                }
                if (Random.Range(0, 100) < asteroid.Probability)
                {
                    UInt32 seed = (UInt32)Random.Range(0, Int32.MaxValue);
                    Random.InitState((Int32)seed);
                    SpawnAsteroid(asteroid, seed);
                }
            }
            else
            {
                Debug.Log("[Kopernicus] No new objects this time. (Probability is " + asteroid.Probability.Value + "%)");
            }
        }

        // Spawn the actual asteroid
        [SuppressMessage("ReSharper", "ConvertIfStatementToSwitchStatement")]
        public void SpawnAsteroid(Asteroid asteroid, UInt32 seed)
        {
            // Create Default Orbit
            Orbit orbit = null;
            CelestialBody body = null;

            // Select Orbit Type
            Int32 type = Random.Range(0, 3);
            if (type == 0 && asteroid.Location.Around.Count != 0)
            {
                // Around
                Location.AroundLoader[] around = GetProbabilityList(asteroid.Location.Around,
                    asteroid.Location.Around.Select(a => a.Probability.Value).ToList()).ToArray();
                Location.AroundLoader loader = around[Random.Range(0, around.Length)];
                body = UBI.GetBody(loader.Body);
                if (!body)
                {
                    return;
                }

                if (loader.Reached && !ReachedBody(body))
                {
                    return;
                }

                orbit = new Orbit
                {
                    referenceBody = body,
                    eccentricity = loader.Eccentricity,
                    semiMajorAxis = loader.SemiMajorAxis,
                    inclination = loader.Inclination,
                    LAN = loader.LongitudeOfAscendingNode,
                    argumentOfPeriapsis = loader.ArgumentOfPeriapsis,
                    meanAnomalyAtEpoch = loader.MeanAnomalyAtEpoch,
                    epoch = loader.Epoch
                };
                orbit.Init();
            }
            else if (type == 1 && asteroid.Location.Nearby.Count != 0)
            {
                // Nearby
                Location.NearbyLoader[] nearby = GetProbabilityList(asteroid.Location.Nearby,
                    asteroid.Location.Nearby.Select(a => a.Probability.Value).ToList()).ToArray();
                Location.NearbyLoader loader = nearby[Random.Range(0, nearby.Length)];
                body = UBI.GetBody(loader.Body);
                if (!body)
                {
                    return;
                }

                if (loader.Reached && !ReachedBody(body))
                {
                    return;
                }

                orbit = new Orbit
                {
                    eccentricity = body.orbit.eccentricity + loader.Eccentricity,
                    semiMajorAxis = body.orbit.semiMajorAxis * loader.SemiMajorAxis,
                    inclination = body.orbit.inclination + loader.Inclination,
                    LAN = body.orbit.LAN * loader.LongitudeOfAscendingNode,
                    argumentOfPeriapsis = body.orbit.argumentOfPeriapsis * loader.ArgumentOfPeriapsis,
                    meanAnomalyAtEpoch = body.orbit.meanAnomalyAtEpoch * loader.MeanAnomalyAtEpoch,
                    epoch = body.orbit.epoch,
                    referenceBody = body.orbit.referenceBody
                };
                orbit.Init();
            }
            else if (type == 2 && asteroid.Location.Flyby.Count != 0)
            {
                // Flyby
                Location.FlybyLoader[] flyby = GetProbabilityList(asteroid.Location.Flyby,
                    asteroid.Location.Flyby.Select(a => a.Probability.Value).ToList()).ToArray();
                Location.FlybyLoader loader = flyby[Random.Range(0, flyby.Length)];
                body = UBI.GetBody(loader.Body);
                if (!body)
                {
                    return;
                }

                if (loader.Reached && !ReachedBody(body))
                {
                    return;
                }

                orbit = Orbit.CreateRandomOrbitFlyBy(body, Random.Range(loader.MinDuration, loader.MaxDuration));
            }

            // Check 
            if (orbit == null)
            {
                Debug.Log("[Kopernicus] No new objects this time. (Probability is " + asteroid.Probability.Value + "%)");
                return;
            }

            // Name
            String asteroidName = DiscoverableObjectsUtil.GenerateAsteroidName();

            // Lifetime
            Double lifetime = Random.Range(asteroid.MinUntrackedLifetime, asteroid.MaxUntrackedLifetime) * 24d * 60d * 60d;
            Double maxLifetime = asteroid.MaxUntrackedLifetime * 24d * 60d * 60d;

            // Size
            UntrackedObjectClass size = (UntrackedObjectClass)(Int32)(asteroid.Size.Evaluate(Random.Range(0f, 1f)) * Enum.GetNames(typeof(UntrackedObjectClass)).Length);

            ProtoVessel protoVessel = kopernicusScenario.SpawnNewAsteroid(seed, asteroidName, orbit, size, lifetime, maxLifetime);
            if (asteroid.UniqueName && FlightGlobals.Vessels.Count(v => v.vesselName == protoVessel.vesselName) != 0)
            {
                return;
            }

            Kopernicus.Events.OnRuntimeUtilitySpawnAsteroid.Fire(asteroid, protoVessel);
            Debug.Log("[Kopernicus] New object found near " + body.name + ": " + protoVessel.vesselName + "!");
#if KSP_VERSION_1_10_1
            if ((RuntimeUtility.KopernicusConfig.enableComets == true) && (Random.Range(0, 100) < RuntimeUtility.KopernicusConfig.CometPercentage))
            {
                kopernicusScenario.SpawnComet();
                Debug.Log("[Kopernicus] A wild comet appears!");
            }
#endif
        }

        // Asteroid Spawner
        [SuppressMessage("ReSharper", "IteratorNeverReturns")]
        private IEnumerator<WaitForSeconds> AsteroidDaemon(Asteroid asteroid)
        {
            while (true)
            {
                // Update Asteroids
                UpdateAsteroid(asteroid, Planetarium.GetUniversalTime());

                // Wait
                yield return new WaitForSeconds(Mathf.Max(asteroid.Interval / TimeWarp.CurrentRate, spawnInterval));
            }
        }

        // Gets a list to reflect probabilities
        private static IEnumerable<T> GetProbabilityList<T>(IList<T> enumerable, IList<Single> amount)
        {
            for (Int32 i = 0; i < enumerable.Count; i++)
            {
                for (Int32 j = 0; j < amount[i]; j++)
                {
                    yield return enumerable[i];
                }
            }
        }

        // Overrides a ConfigNode recursively
        private static void OverrideNode(ref ConfigNode original, ConfigNode custom, Boolean rec = false)
        {
            // null checks
            if (original == null || custom == null)
            {
                return;
            }

            // Go through the values
            foreach (ConfigNode.Value value in custom.values)
            {
                original.SetValue(value.name, value.value, true);
            }

            // Get nodes that should get removed
            if (original.HasValue("removeNodes"))
            {
                String[] names = original.GetValue("removeNodes").Split(new[] { ',', ' ', ';' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (String nodeName in names)
                {
                    original.RemoveNodes(nodeName);
                }
            }

            // Go through the nodes
            foreach (ConfigNode node in custom.nodes)
            {
                if (!original.HasNode(node.name))
                {
                    original.AddNode(node).AddValue("__PATCHED__", "__YES__");
                    continue;
                }
                ConfigNode[] nodes = original.GetNodes(node.name);
                if (nodes.Any(n => !n.HasValue("__PATCHED__")))
                {
                    ConfigNode foundNode = nodes.First(n => !n.HasValue("__PATCHED__"));
                    OverrideNode(ref foundNode, node, true);
                    foundNode.AddValue("__PATCHED__", "__YES__");
                }
                else
                {
                    original.AddNode(node).AddValue("__PATCHED__", "__YES__");
                }
            }

            // Remove patches
            if (!rec)
            {
                Utility.DoRecursive(original, o => o.GetNodes(), node => node.RemoveValues("__PATCHED__"));
            }
        }

        // Determines whether a body was already visited
        private static Boolean ReachedBody(CelestialBody body)
        {
            CelestialBodySubtree bodyTree = ProgressTracking.Instance.GetBodyTree(body.name);
            return bodyTree != null && bodyTree.IsReached;
        }
    }
}
