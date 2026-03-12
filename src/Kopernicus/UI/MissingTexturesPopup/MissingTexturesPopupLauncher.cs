using UnityEngine;

namespace Kopernicus.UI.MissingTexturesPopup;

[KSPAddon(KSPAddon.Startup.MainMenu, true)]
internal class MissingTexturesPopupLauncher : MonoBehaviour
{
    void Start()
    {
        MissingTexturesWindow.ShowIfNeeded();
    }
}
