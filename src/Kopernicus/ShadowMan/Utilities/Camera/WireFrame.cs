using UnityEngine;
using System.Collections;

namespace Kopernicus.ShadowMan
{

    public class Wireframe : MonoBehaviour
    {

        // Use this for initialization
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        void OnPreRender()
        {
            GL.wireframe = true;
        }

        void OnPostRender()
        {
            GL.wireframe = false;
        }
    }
}