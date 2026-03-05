using KSP.UI;
using Kopernicus.UI.Components;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kopernicus.UI;

internal static class UIBuilder
{
    internal static GameObject Prefab(string name) => UISkinManager.GetPrefab(name);

    internal static GameObject CreateCloseButton(Transform parent, GameObject target, string text = "X", float fontSize = 12)
    {
        var go = CreateButton(parent, text, fontSize);

        var closeBtn = go.AddComponent<CloseButton>();
        closeBtn.button = go.GetComponent<Button>();
        closeBtn.target = target;

        return go;
    }

    internal static GameObject CreateText(Transform parent, string text, float fontSize = 14)
    {
        var go = Object.Instantiate(Prefab("UITextPrefab"), parent);
        go.SetActive(true);
        go.name = text.Length > 30 ? text.Substring(0, 30) : text;

        var tmp = go.GetComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;

        return go;
    }

    internal static GameObject CreateButton(Transform parent, string text, float fontSize = 14)
    {
        var go = Object.Instantiate(Prefab("UIButtonPrefab"), parent);
        go.SetActive(true);
        go.name = text;

        var tmp = go.GetComponentInChildren<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;

        var textRect = tmp.rectTransform;
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        var le = go.AddOrGetComponent<LayoutElement>();
        le.preferredHeight = fontSize + 16;

        return go;
    }
}
