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
using Expansions.Missions.Actions;
using Kopernicus.Configuration.DiscoverableObjects;
using Kopernicus.Constants;
using KSPAchievements;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace Kopernicus.RuntimeUtility
{
    // Class to manage Asteroids
    [KSPScenario(ScenarioCreationOptions.AddToAllGames, GameScenes.FLIGHT, GameScenes.TRACKSTATION, GameScenes.SPACECENTER)]
    public class DiscoverableObjects : ScenarioModule
    {
        // All asteroid configurations we know
        public static List<Asteroid> Asteroids { get; }

        // Spawn interval
        public Single spawnInterval = 3f;

        // Construct
        static DiscoverableObjects()
        {
            Asteroids = new List<Asteroid>();
        }

        // Kill the old spawner
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
            if (!RuntimeUtility.KopernicusConfig.UseKopernicusAsteroidSystem.ToLower().Equals("stock"))
            {
                // Kill old Scenario Discoverable Objects without editing the collection while iterating through the same collection
                // @Squad: I stab you with a try { } catch { } block.

                if (HighLogic.CurrentGame.RemoveProtoScenarioModule(typeof(ScenarioDiscoverableObjects)))
                {
                    // RemoveProtoScenarioModule doesn't remove the actual Scenario; workaround!
                    foreach (Object o in
                        Resources.FindObjectsOfTypeAll(typeof(ScenarioDiscoverableObjects)))
                    {
                        ScenarioDiscoverableObjects scenario = (ScenarioDiscoverableObjects)o;
                        scenario.StopAllCoroutines();
                        Destroy(scenario);
                    }
                }
                if (RuntimeUtility.KopernicusConfig.UseKopernicusAsteroidSystem.ToLower().Equals("true"))
                {
                    Debug.Log("[Kopernicus] Using Kopernicus Discoverable Object Spawner.");
                    AsteroidSetup();
                    foreach (var asteroidGroup in Asteroids)
                    {
                        StartCoroutine(AsteroidDaemon(asteroidGroup));
                    }
                }
                else if (RuntimeUtility.KopernicusConfig.UseKopernicusAsteroidSystem.ToLower().Equals("false"))
                {
                    Debug.Log("[Kopernicus] Discoverable Object Spawners disabled.  Unless external spawner mod is installed no discoverable objects will be spawned.");
                }
                else
                {
                    Injector.DisplayWarning();
                    throw new InvalidCastException("Invalid value for Enum UseKopernicusAsteroidSystem.  Valid values are true, false, and stock.");
                }

            }
            else if (RuntimeUtility.KopernicusConfig.UseKopernicusAsteroidSystem.ToLower().Equals("stock"))
            {
                Debug.Log("[Kopernicus] Using stock Squad Discoverable Object Spawner.");
            }
        }

        // Update the Asteroids
        public void UpdateAsteroid(Asteroid asteroidGroup)
        {
            string launchedFromMatch = LaunchedFromName(asteroidGroup);
            int asteroidCount = 0;
            for (int i = 0; i < FlightGlobals.Vessels.Count; i++)
            {
                Vessel vessel = FlightGlobals.Vessels[i];
                // If we know the vectors of the object, it's one of ours or tracked. Ignore.
                if (vessel.DiscoveryInfo.HaveKnowledgeAbout(DiscoveryLevels.StateVectors)) continue;
                // Kill any unknowns past their lifetime, and return doing nothing else if any are found. (Not sure why we return, but that's how the code was.)
                if (Math.Abs(vessel.DiscoveryInfo.GetSignalLife(Planetarium.GetUniversalTime())) < 0.01)
                {
                    vessel.Die();
                    return;
                }
                // Increment count if it's an asteroid from this spawn region.
                if (string.Equals(launchedFromMatch, vessel.launchedFrom)) asteroidCount++;
            }

            // If we don't have access to space object discovery, exit.
            if (!GameVariables.Instance.UnlockedSpaceObjectDiscovery(ScenarioUpgradeableFacilities.GetFacilityLevel(SpaceCenterFacility.TrackingStation)))
            {
                return;
            }
            
            // If we're over the limit for the asteroid group, don't spawn anything.
            if (asteroidCount > Random.Range(asteroidGroup.SpawnGroupMinLimit, asteroidGroup.SpawnGroupMaxLimit)) return;
                
            // An asteroid only gets spawned if it gets lucky.
            if (Random.Range(0f, 100f) < asteroidGroup.Probability) return;
                
            // There was both room for the asteroid and it was lucky; spawn the asteroid.
            int seed = Random.Range(0, int.MaxValue);
            Random.InitState(seed);
            SpawnAsteroid(asteroidGroup, (uint)seed);
        }
        
        private readonly Dictionary<string, CelestialBody> _bodyDictionary = new Dictionary<string, CelestialBody>();

        private CelestialBody GetCachedBody(string bodyName)
        {
            if (_bodyDictionary.TryGetValue(bodyName, out CelestialBody cachedBody))
            {
                return cachedBody;
            }

            CelestialBody fetchedBody = UBI.GetBody(bodyName);
            if (fetchedBody is null || fetchedBody == default) return null;
            _bodyDictionary[bodyName] = fetchedBody;
            return fetchedBody;
        }

        // Spawn the actual asteroid
        [SuppressMessage("ReSharper", "ConvertIfStatementToSwitchStatement")]
        public void SpawnAsteroid(Asteroid asteroid, UInt32 seed)
        {
            // Create Default Orbit
            Orbit orbit;
            CelestialBody body;
            if (asteroid.Size is null)
            {
                Debug.Log($"[Kopernicus] No Size FloatCurve exists for {asteroid.Name}!");
                return;
            }

            // Select Orbit Type
            Int32 type = Random.Range(0, 3);
            if (type == 0 && asteroid.Location.Around.Count != 0)
            {
                // Around
                Location.AroundLoader[] around = _aroundLoaders[asteroid.InternalOrderID];
                Location.AroundLoader loader = around[Random.Range(0, around.Length)];
                body = GetCachedBody(loader.Body);
                if (body is null || !body)
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
                Location.NearbyLoader[] nearby = _nearbyLoaders[asteroid.InternalOrderID];
                Location.NearbyLoader loader = nearby[Random.Range(0, nearby.Length)];
                body = GetCachedBody(loader.Body);
                if (body is null || !body)
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
                Location.FlybyLoader[] flyby = _flybyLoaders[asteroid.InternalOrderID];
                Location.FlybyLoader loader = flyby[Random.Range(0, flyby.Length)];
                body = GetCachedBody(loader.Body);
                if (body is null || !body)
                {
                    return;
                }

                if (loader.Reached && !ReachedBody(body))
                {
                    return;
                }

                orbit = Orbit.CreateRandomOrbitFlyBy(body, Random.Range(loader.MinDuration, loader.MaxDuration));
            }
            else
            {
                Debug.Log("[Kopernicus] No new objects this time. (Probability is " + asteroid.Probability.Value + "%)");
                return;
            }

            if (orbit is null)
            {
                Debug.Log("[Kopernicus] No new objects this time. (Probability is " + asteroid.Probability.Value + "%)");
                return;
            }

            // Name
            string asteroidName = DiscoverableObjectsUtil.GenerateAsteroidName();
            if (string.IsNullOrEmpty(asteroidName) || (asteroid.UniqueName && FlightGlobals.Vessels.Any(v => string.Equals(v.vesselName, asteroidName))))
            {
                return;
            }

            double lifetime = Random.Range(asteroid.MinUntrackedLifetime, asteroid.MaxUntrackedLifetime) * 24d * 60d * 60d;;
            double maxLifetime = asteroid.MaxUntrackedLifetime * 24d * 60d * 60d;
            UntrackedObjectClass size = (UntrackedObjectClass)(Int32)(asteroid.Size.Evaluate(Random.Range(0f, 1f)) * Enum.GetNames(typeof(UntrackedObjectClass)).Length);

            // Spawn
            ConfigNode vessel = ProtoVessel.CreateVesselNode(
                asteroidName,
                VesselType.SpaceObject,
                orbit,
                0,
                new[]
                {
                    ProtoVessel.CreatePartNode(
                        "PotatoRoid",
                        seed
                    )
                },
                new ConfigNode("ACTIONGROUPS"),
                ProtoVessel.CreateDiscoveryNode(
                    DiscoveryLevels.Presence,
                    size,
                    lifetime,
                    maxLifetime
                )
            );
            OverrideNode(ref vessel, asteroid.Vessel);
            ProtoVessel protoVessel = new ProtoVessel(vessel, HighLogic.CurrentGame) { launchedFrom = LaunchedFromName(asteroid) };
            Kopernicus.Events.OnRuntimeUtilitySpawnAsteroid.Fire(asteroid, protoVessel);
            protoVessel.Load(HighLogic.CurrentGame.flightState);
            GameEvents.onNewVesselCreated.Fire(protoVessel.vesselRef);
            GameEvents.onAsteroidSpawned.Fire(protoVessel.vesselRef);
            Debug.Log("[Kopernicus] New object found near " + body.name + ": " + protoVessel.vesselName + "!");
        }

        // Asteroid Spawner
        [SuppressMessage("ReSharper", "IteratorNeverReturns")]
        private IEnumerator<WaitForSecondsRealtime> AsteroidDaemon(Asteroid asteroidGroup)
        {
            while (true)
            {
                if (RuntimeUtility.KopernicusConfig.UseKopernicusAsteroidSystem.ToLower().Equals("true"))
                {
                    // Update Asteroids
                    UpdateAsteroid(asteroidGroup);
                    // Don't adjust waiting time if we're in physical timewarp, and don't reduce interval by more than /50 when in on-rails timewarp.
                    float waitSeconds = TimeWarp.WarpMode == TimeWarp.Modes.LOW
                        ? Mathf.Max(asteroidGroup.Interval, spawnInterval)
                        : Mathf.Max(asteroidGroup.Interval / Mathf.Min(TimeWarp.CurrentRate, 50), spawnInterval);

                    // Add some random jitter to the wait time.
                    float minMaxJitter = waitSeconds / 4;
                    waitSeconds = Random.Range(waitSeconds - minMaxJitter, waitSeconds + minMaxJitter);

                    // Wait
                    yield return new WaitForSecondsRealtime(waitSeconds);
                }
            }
        }

        public static string LaunchedFromName(Asteroid asteroid) => $"AST-{asteroid.Name}";

        private Location.AroundLoader[][] _aroundLoaders;
        private Location.NearbyLoader[][] _nearbyLoaders;
        private Location.FlybyLoader[][] _flybyLoaders;

        private void AsteroidSetup()
        {
            int asteroidCount = Asteroids.Count;
            _aroundLoaders = new Location.AroundLoader[asteroidCount][];
            _nearbyLoaders = new Location.NearbyLoader[asteroidCount][];
            _flybyLoaders = new Location.FlybyLoader[asteroidCount][];
            for (int i = 0; i < asteroidCount; i++)
            {
                Asteroid asteroid = Asteroids[i];
                asteroid.InternalOrderID = i;
                _aroundLoaders[i] = GetProbabilityList(asteroid.Location.Around,
                    asteroid.Location.Around.Select(a => a.Probability.Value).ToList()).ToArray();
                _nearbyLoaders[i] = GetProbabilityList(asteroid.Location.Nearby,
                    asteroid.Location.Nearby.Select(a => a.Probability.Value).ToList()).ToArray();
                _flybyLoaders[i] = GetProbabilityList(asteroid.Location.Flyby,
                    asteroid.Location.Flyby.Select(a => a.Probability.Value).ToList()).ToArray();
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
