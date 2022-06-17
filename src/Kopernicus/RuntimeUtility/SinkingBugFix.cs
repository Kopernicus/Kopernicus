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
            if (mainBody.name.Equals(FlightGlobals.Bodies[0].name))
            {
                collider.enabled = false;
                return;
            }
            Vector3 dir = mainBody.scaledBody.transform.position - FlightGlobals.Bodies[0].scaledBody.transform.position;
            transform.position = mainBody.scaledBody.transform.position + dir * 0.01f;
            double distance = Vector3d.Distance(FlightGlobals.Bodies[0].scaledBody.transform.position,mainBody.scaledBody.transform.position) * 0.0001;
            collider.radius = (float)distance;

        }
    }
}
