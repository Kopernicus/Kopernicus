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
            Vector3 dir = new Vector3(0f,0f,0f);
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
            else if (!FlightGlobals.currentMainBody.name.Equals(PSystemManager.Instance.systemPrefab.rootBody.celestialBody.name))
            {
                dir = mainBody.scaledBody.transform.position - Kopernicus.Components.KopernicusStar.GetNearestBodyOverSystenRoot(FlightGlobals.currentMainBody).scaledBody.transform.position;
            }
            else if (FlightGlobals.currentMainBody.referenceBody.name.Equals(PSystemManager.Instance.systemPrefab.rootBody.celestialBody.name))
            {
                dir = mainBody.scaledBody.transform.position - Kopernicus.Components.KopernicusStar.GetNearestBodyOverSystenRoot(FlightGlobals.currentMainBody.referenceBody).scaledBody.transform.position;
            }
            if (dir.x > 0.00000001)
            {
                transform.position = mainBody.scaledBody.transform.position + dir * 0.001f;
            }
        }
    }
}
