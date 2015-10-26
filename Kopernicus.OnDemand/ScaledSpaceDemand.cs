using UnityEngine;

namespace Kopernicus
{
    namespace OnDemand
    {
        // Class to load ScaledSpace Textures on Demand
        public class ScaledSpaceDemand : MonoBehaviour
        {
            // Path to the Texture
            public string texture;

            // Path to the normal map
            public string normals;

            // ScaledSpace MeshRenderer
            public MeshRenderer scaledRenderer;

            // Start(), get the scaled Mesh renderer
            void Start()
            {
                scaledRenderer = GetComponent<MeshRenderer>();
                OnBecameInvisible();
            }

            // OnBecameVisible(), load the texture
            void OnBecameVisible()
            {
                if (OnDemandStorage.TextureExists(texture)) scaledRenderer.material.SetTexture("_MainTex", OnDemandStorage.LoadTexture(texture, false, false, false));
                if (OnDemandStorage.TextureExists(normals)) scaledRenderer.material.SetTexture("_BumpMap", OnDemandStorage.LoadTexture(normals, false, false, false));
                Debug.Log("Visible");
            }

            // OnBecameInvisible(), kill the texture
            void OnBecameInvisible()
            {
                if (OnDemandStorage.TextureExists(texture)) DestroyImmediate(scaledRenderer.material.GetTexture("_MainTex"));
                if (OnDemandStorage.TextureExists(normals)) DestroyImmediate(scaledRenderer.material.GetTexture("_BumpMap"));
                Debug.Log("Invisible");
            }
        }
    }
}