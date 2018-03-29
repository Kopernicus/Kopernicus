#if FALSE
using UnityEngine;

namespace Kopernicus
{
    namespace Components
    {
        /// <summary>
        /// Updates the parameters of an AFG with the values from their partner star
        /// </summary>
        public class KopernicusStarAFG : MonoBehaviour
        {
            /// <summary>
            /// The AFG that is being modified
            /// </summary>
            private AtmosphereFromGround _afg;

            /// <summary>
            /// The star that influences the AFG
            /// </summary>
            public KopernicusStar Star;

            /// <summary>
            /// Calculate the lightdot parameter of the AFG manually, based on the new star and the intensity
            /// </summary>
            void LateUpdate()
            {
                if (_afg == null)
                {
                    _afg = GetComponent<AtmosphereFromGround>();
                }
                _afg.sunLight = Star.gameObject;
                Vector3 planet2cam = _afg.planet.scaledBody.transform.position -
                                     _afg.mainCamera.transform.position;
                _afg.lightDot =
                    Mathf.Clamp01(Vector3.Dot(planet2cam,
                                      _afg.mainCamera.transform.position - Star.transform.position) *
                                  _afg.dawnFactor * Star.RelativeIntensity);
                _afg.GetComponent<Renderer>().material.SetFloat("_lightDot", _afg.lightDot);
            }
        }
    }
}
#endif