#if FALSE
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Kopernicus
{
    namespace Components
    {
        /// <summary>
        /// Modifies the TimeOfDay Animation to support multiple stars
        /// </summary>
        public class KopernicusStarTimeOfDay : MonoBehaviour
        {
            private TimeOfDayAnimation _animation;

            void Update()
            {
                if (_animation == null)
                {
                    _animation = GetComponent<TimeOfDayAnimation>();
                }

                _animation.target = null;

                List<Single> dots = new List<Single>();
                for (Int32 i = 0; i < KopernicusStar.Stars.Count; i++)
                {
                    KopernicusStar star = KopernicusStar.Stars[i];
                    dots.Add(Vector3.Dot(star.transform.forward, transform.up) * star.RelativeIntensity);
                }

                _animation.dot = 1f - (dots.Min() + 1f) * 0.5f;
            }
        }
    }
}
#endif