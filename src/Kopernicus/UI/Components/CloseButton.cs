using UnityEngine;
using UnityEngine.UI;

namespace Kopernicus.UI.Components;

internal class CloseButton : MonoBehaviour
{
    public Button button;
    public GameObject target;

    void Start()
    {
        button.onClick.AddListener(OnClick);
    }

    void OnClick()
    {
        Destroy(target);
    }
}
