using UnityEngine;
namespace Kopernicus.RuntimeUtility
{
    [KSPAddon(KSPAddon.Startup.Flight, false)]
    public class SinkingBugFix : MonoBehaviour
    {
        private SphereCollider collider;

        public void Start()
        {
            gameObject.layer = 10;
            collider = gameObject.AddComponent<SphereCollider>();
        }

        private void Update()
        {
            CelestialBody mainBody = FlightGlobals.currentMainBody;
            if (mainBody == null)
            {
                collider.enabled = false;
                return;
            }
            else if (!collider.enabled)
            {
                collider.enabled = true;
            }
            if (FlightGlobals.currentMainBody.name.Equals(PSystemManager.Instance.systemPrefab.rootBody.celestialBody.name))
            {
                collider.enabled = false;
                return;
            }
            Vector3 dir = mainBody.scaledBody.transform.position - FlightGlobals.Bodies[0].scaledBody.transform.position;
            transform.position = mainBody.scaledBody.transform.position + dir * 0.01f;

        }
    }
}
