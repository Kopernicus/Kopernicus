
using UnityEngine;
using System.Collections;
using System.IO;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

using KSP.IO;

namespace Kopernicus.ShadowMan
{

    public class TweakShadowCascades : MonoBehaviour
    {
        Vector3 tweakedSplit, defaultSplit;

        public TweakShadowCascades()
        {

        }

        public void Init(Vector3 inputSplit)
        {
            Utils.LogDebug("Adding TweakShadowCascades: " + inputSplit.ToString("F3") + " to Camera " + gameObject.GetComponent<Camera>().name);
            defaultSplit = QualitySettings.shadowCascade4Split;
            Utils.LogDebug("Default split: " + defaultSplit.ToString("F3"));
            tweakedSplit = inputSplit;

        }

        public void OnPreRender()
        {
            QualitySettings.shadowCascade4Split = tweakedSplit;
        }

        public void OnPostRender()
        {
            QualitySettings.shadowCascade4Split = defaultSplit;
        }

        public void OnDestroy()
        {
            QualitySettings.shadowCascade4Split = defaultSplit;
        }
    }
}

