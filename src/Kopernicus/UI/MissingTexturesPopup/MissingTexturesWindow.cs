using KSP.UI;
using Kopernicus.UI.Components;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kopernicus.UI.MissingTexturesPopup;

internal class MissingTexturesWindow : MonoBehaviour
{
    private const string WindowTitle = "Kopernicus - Missing Textures";

    internal static void Show()
    {
        Build(MainCanvasUtil.MainCanvas.transform);
    }

    internal static void ShowIfNeeded()
    {
        if (!MissingTextureLog.HasEntries)
            return;

        Show();
    }

    static GameObject Prefab(string name) => UISkinManager.GetPrefab(name);

    static T AddOrGetComponent<T>(GameObject go) where T : Component
    {
        return go.GetComponent<T>() ?? go.AddComponent<T>();
    }

    private static GameObject Build(Transform parent)
    {
        var skin = UISkinManager.defaultSkin;

        // Root window
        var windowGo = Object.Instantiate(Prefab("UIBoxPrefab"), parent);
        windowGo.name = "KopernicusMissingTexturesWindow";

        var windowRect = windowGo.GetComponent<RectTransform>();
        windowRect.anchorMin = new Vector2(0.5f, 0.5f);
        windowRect.anchorMax = new Vector2(0.5f, 0.5f);
        windowRect.pivot = new Vector2(0.5f, 0.5f);
        windowRect.anchoredPosition = Vector2.zero;
        windowRect.sizeDelta = new Vector2(500, 400);

        var windowImage = windowGo.GetComponent<Image>();
        if (skin.window.normal?.background != null)
            windowImage.sprite = skin.window.normal.background;
        windowImage.type = Image.Type.Sliced;

        windowGo.AddComponent<CanvasGroup>();
        windowGo.AddComponent<DragPanel>();

        var inputLock = windowGo.AddComponent<DialogMouseEnterControlLock>();
        inputLock.lockName = "Kopernicus_MissingTexturesWindow";

        var windowLayout = windowGo.AddComponent<VerticalLayoutGroup>();
        windowLayout.padding = new RectOffset(8, 8, 8, 8);
        windowLayout.spacing = 4;
        windowLayout.childForceExpandWidth = true;
        windowLayout.childForceExpandHeight = false;
        windowLayout.childControlWidth = true;
        windowLayout.childControlHeight = true;

        // Title bar
        BuildTitleBar(windowGo, skin);

        // Description text
        var descGo = CreateText(
            windowGo.transform,
            "The following textures failed to load during planet configuration. " +
            "This usually means a texture file is missing or the path is incorrect."
        );
        var descTmp = descGo.GetComponent<TextMeshProUGUI>();
        descTmp.fontSize = 12;
        descTmp.fontStyle = FontStyles.Italic;
        descTmp.enableWordWrapping = true;
        var descLE = AddOrGetComponent<LayoutElement>(descGo);
        descLE.flexibleWidth = 1;

        // Scroll view with texture list
        BuildTextureList(windowGo.transform);

        // Bottom button row
        BuildBottomRow(windowGo);

        // Attach component
        windowGo.AddComponent<MissingTexturesWindow>();

        return windowGo;
    }

    private static void BuildTitleBar(GameObject windowGo, UISkinDef skin)
    {
        var titleBarGo = Object.Instantiate(Prefab("UIHorizontalLayoutPrefab"), windowGo.transform);
        titleBarGo.SetActive(true);
        titleBarGo.name = "TitleBar";

        var layout = titleBarGo.GetComponent<HorizontalLayoutGroup>();
        layout.padding = new RectOffset(4, 4, 2, 2);
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = true;
        layout.childControlWidth = true;
        layout.childControlHeight = true;

        var le = titleBarGo.AddComponent<LayoutElement>();
        le.preferredHeight = 28;
        le.flexibleHeight = 0;
        le.flexibleWidth = 1;

        // Title text
        var titleGo = CreateText(titleBarGo.transform, WindowTitle);
        var titleTmp = titleGo.GetComponent<TextMeshProUGUI>();
        titleTmp.fontStyle = FontStyles.Bold;
        titleTmp.color = skin.window.normal.textColor;
        titleGo.GetComponent<LayoutElement>().flexibleWidth = 1;

        var align = titleGo.AddComponent<Components.TextAlignment>();
        align.text = titleTmp;
        align.alignment = TextAlignmentOptions.Center;

        // Close button
        var closeGo = CreateButton(titleBarGo.transform, "X", 12);
        var closeLE = AddOrGetComponent<LayoutElement>(closeGo);
        closeLE.preferredWidth = 24;
        closeLE.preferredHeight = 24;
        closeLE.flexibleWidth = 0;

        var closeBtn = closeGo.AddComponent<CloseButton>();
        closeBtn.button = closeGo.GetComponent<Button>();
        closeBtn.target = windowGo;
    }

    private static void BuildTextureList(Transform parent)
    {
        var scrollGo = Object.Instantiate(Prefab("UIScrollViewPrefab"), parent);
        scrollGo.SetActive(true);
        scrollGo.name = "TextureListScroll";

        var scrollLE = AddOrGetComponent<LayoutElement>(scrollGo);
        scrollLE.flexibleHeight = 1;
        scrollLE.flexibleWidth = 1;
        scrollLE.preferredHeight = 250;

        // Find the content transform inside the scroll view
        var scrollRect = scrollGo.GetComponent<ScrollRect>();
        var contentGo = scrollRect.content.gameObject;

        var contentLayout = AddOrGetComponent<VerticalLayoutGroup>(contentGo);
        contentLayout.padding = new RectOffset(4, 4, 4, 4);
        contentLayout.spacing = 2;
        contentLayout.childForceExpandWidth = true;
        contentLayout.childForceExpandHeight = false;
        contentLayout.childControlWidth = true;
        contentLayout.childControlHeight = true;

        var contentFitter = AddOrGetComponent<ContentSizeFitter>(contentGo);
        contentFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        // Populate with current entries
        PopulateEntries(contentGo.transform);
    }

    private static void BuildBottomRow(GameObject windowGo)
    {
        var rowGo = Object.Instantiate(Prefab("UIHorizontalLayoutPrefab"), windowGo.transform);
        rowGo.SetActive(true);
        rowGo.name = "BottomRow";

        var layout = rowGo.GetComponent<HorizontalLayoutGroup>();
        layout.padding = new RectOffset(0, 0, 4, 0);
        layout.spacing = 8;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childAlignment = TextAnchor.MiddleRight;

        var le = rowGo.AddComponent<LayoutElement>();
        le.preferredHeight = 32;
        le.flexibleHeight = 0;
        le.flexibleWidth = 1;

        // Spacer to push button to the right
        var spacer = new GameObject("Spacer", typeof(RectTransform));
        spacer.transform.SetParent(rowGo.transform, false);
        var spacerLE = spacer.AddComponent<LayoutElement>();
        spacerLE.flexibleWidth = 1;

        // Close button
        var closeGo = CreateButton(rowGo.transform, "Close");
        var closeLE = AddOrGetComponent<LayoutElement>(closeGo);
        closeLE.preferredWidth = 80;

        var closeBtn = closeGo.AddComponent<CloseButton>();
        closeBtn.button = closeGo.GetComponent<Button>();
        closeBtn.target = windowGo;
    }

    private static GameObject CreateText(Transform parent, string text, float fontSize = 14)
    {
        var go = Object.Instantiate(Prefab("UITextPrefab"), parent);
        go.SetActive(true);
        go.name = text.Length > 30 ? text.Substring(0, 30) : text;

        var tmp = go.GetComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;

        return go;
    }

    private static GameObject CreateButton(Transform parent, string text, float fontSize = 14)
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

        var le = AddOrGetComponent<LayoutElement>(go);
        le.preferredHeight = fontSize + 16;

        return go;
    }

    private static void PopulateEntries(Transform parent)
    {
        var entries = MissingTextureLog.Entries;

        if (entries.Count == 0)
        {
            var noEntriesGo = CreateText(parent, "No missing textures detected.", 13);
            var tmp = noEntriesGo.GetComponent<TextMeshProUGUI>();
            tmp.fontStyle = FontStyles.Italic;
            tmp.color = new Color(0.7f, 0.7f, 0.7f);
            return;
        }

        for (int i = 0; i < entries.Count; i++)
        {
            CreateEntryRow(parent, entries[i], i);
        }
    }

    private static void CreateEntryRow(Transform parent, MissingTextureLog.Entry entry, int index)
    {
        var rowGo = Object.Instantiate(Prefab("UIBoxPrefab"), parent);
        rowGo.SetActive(true);
        rowGo.name = $"Entry_{index}";

        // Subtle background
        var image = rowGo.GetComponent<Image>();
        image.color = index % 2 == 0
            ? new Color(0.15f, 0.15f, 0.15f, 0.5f)
            : new Color(0.1f, 0.1f, 0.1f, 0.5f);
        image.type = Image.Type.Sliced;

        var layout = rowGo.AddComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(6, 6, 4, 4);
        layout.spacing = 1;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;
        layout.childControlWidth = true;
        layout.childControlHeight = true;

        // Body name
        var bodyGo = CreateText(rowGo.transform, entry.BodyName, 13);
        var bodyTmp = bodyGo.GetComponent<TextMeshProUGUI>();
        bodyTmp.fontStyle = FontStyles.Bold;
        bodyTmp.color = new Color(1f, 0.8f, 0.3f);

        // Texture path
        var pathGo = CreateText(rowGo.transform, entry.TexturePath, 12);
        var pathTmp = pathGo.GetComponent<TextMeshProUGUI>();
        pathTmp.color = new Color(0.9f, 0.5f, 0.5f);

        // Error message (if present)
        if (!string.IsNullOrEmpty(entry.ErrorMessage))
        {
            var errorGo = CreateText(rowGo.transform, entry.ErrorMessage, 11);
            var errorTmp = errorGo.GetComponent<TextMeshProUGUI>();
            errorTmp.fontStyle = FontStyles.Italic;
            errorTmp.color = new Color(0.6f, 0.6f, 0.6f);
        }
    }
}
