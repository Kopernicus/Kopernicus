using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using KSP.UI;
using TMPro;
using KSP.Localization;
using UnityEngine;
using UnityEngine.UI;

namespace Kopernicus.UI.MissingTexturesPopup;

internal class MissingTexturesWindow : MonoBehaviour
{
    private static readonly string WindowTitle = Localizer.Format("#Kopernicus_UI_MissingTextures_Title");
    private static readonly string NoBodyLabel = Localizer.Format("#Kopernicus_UI_MissingTextures_NoBody");

    private static readonly Color GroupBackgroundEven = new Color(0.15f, 0.15f, 0.15f, 0.5f);
    private static readonly Color GroupBackgroundOdd = new Color(0.1f, 0.1f, 0.1f, 0.5f);
    private static readonly Color BodyNameColor = new Color(1f, 0.8f, 0.3f);
    private static readonly Color TexturePathColor = new Color(0.9f, 0.5f, 0.5f);

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

    private static GameObject Build(Transform parent)
    {
        var skin = UISkinManager.defaultSkin;

        // Root window
        var windowGo = Object.Instantiate(UIBuilder.Prefab("UIBoxPrefab"), parent);
        windowGo.name = "KopernicusMissingTexturesWindow";

        var windowRect = windowGo.GetComponent<RectTransform>();
        windowRect.anchorMin = new Vector2(0.5f, 0.5f);
        windowRect.anchorMax = new Vector2(0.5f, 0.5f);
        windowRect.pivot = new Vector2(0.5f, 0.5f);
        windowRect.anchoredPosition = Vector2.zero;
        windowRect.sizeDelta = new Vector2(500, 400);

        var windowImage = windowGo.GetComponent<Image>();
        var opaqueBackground = skin.customStyles?.FirstOrDefault(s => s?.name == "List Item")
            ?.normal?.background ?? skin.window.normal?.background;
        if (opaqueBackground != null)
            windowImage.sprite = opaqueBackground;
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
        var descGo = UIBuilder.CreateText(
            windowGo.transform,
            Localizer.Format("#Kopernicus_UI_MissingTextures_Description")
        );
        var descTmp = descGo.GetComponent<TextMeshProUGUI>();
        descTmp.fontSize = 12;
        descTmp.fontStyle = FontStyles.Italic;
        descTmp.enableWordWrapping = true;
        var descLE = descGo.AddOrGetComponent<LayoutElement>();
        descLE.flexibleWidth = 1;

        // Scroll view with texture list
        BuildTextureList(windowGo.transform);

        // Bottom button row
        BuildBottomRow(windowGo);

        // Attach component
        windowGo.AddComponent<MissingTexturesWindow>();

        windowGo.SetActive(true);
        return windowGo;
    }

    private static void BuildTitleBar(GameObject windowGo, UISkinDef skin)
    {
        UIBuilder.CreateTitleBar(windowGo.transform, WindowTitle, windowGo, skin);
    }

    private static void BuildTextureList(Transform parent)
    {
        var scrollGo = Object.Instantiate(UIBuilder.Prefab("UIScrollViewPrefab"), parent);
        scrollGo.SetActive(true);
        scrollGo.name = "TextureListScroll";

        var scrollLE = scrollGo.AddOrGetComponent<LayoutElement>();
        scrollLE.flexibleHeight = 1;
        scrollLE.flexibleWidth = 1;
        scrollLE.preferredHeight = 250;

        // Disable horizontal scrolling
        var scrollRect = scrollGo.GetComponentInChildren<ScrollRect>();
        scrollRect.horizontal = false;
        Object.Destroy(scrollRect.horizontalScrollbar?.gameObject);
        scrollRect.horizontalScrollbar = null;
        RectTransform contentRect;
        if (scrollRect.content != null)
        {
            contentRect = scrollRect.content;
        }
        else
        {
            var contentObj = new GameObject("Content", typeof(RectTransform));
            contentRect = contentObj.GetComponent<RectTransform>();
            contentRect.SetParent(scrollRect.viewport ?? scrollGo.transform, false);
            contentRect.anchorMin = Vector2.zero;
            contentRect.anchorMax = new Vector2(1, 1);
            contentRect.sizeDelta = Vector2.zero;
            scrollRect.content = contentRect;
        }
        var contentGo = contentRect.gameObject;

        var contentLayout = contentGo.AddOrGetComponent<VerticalLayoutGroup>();
        contentLayout.padding = new RectOffset(4, 12, 4, 4);
        contentLayout.spacing = 2;
        contentLayout.childForceExpandWidth = true;
        contentLayout.childForceExpandHeight = false;
        contentLayout.childControlWidth = true;
        contentLayout.childControlHeight = true;
        contentLayout.childAlignment = TextAnchor.UpperLeft;

        contentRect.anchorMin = new Vector2(0, 1);
        contentRect.anchorMax = new Vector2(1, 1);
        contentRect.pivot = new Vector2(0, 1);

        var contentFitter = contentGo.AddOrGetComponent<ContentSizeFitter>();
        contentFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        // Populate with current entries
        PopulateEntries(contentGo.transform);
    }

    private static void BuildBottomRow(GameObject windowGo)
    {
        var rowGo = Object.Instantiate(UIBuilder.Prefab("UIHorizontalLayoutPrefab"), windowGo.transform);
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

#if UI_DUMP_LAYOUT_BUTTON
        // Dump Layout button
        var dumpGo = UIBuilder.CreateButton(rowGo.transform, "Dump Layout");
        var dumpLE = dumpGo.AddOrGetComponent<LayoutElement>();
        dumpLE.preferredWidth = 100;
        dumpGo.GetComponent<Button>().onClick.AddListener(() => DumpLayout(windowGo));
#endif

        // Spacer to push close button to the right
        var spacer = new GameObject("Spacer", typeof(RectTransform));
        spacer.transform.SetParent(rowGo.transform, false);
        var spacerLE = spacer.AddComponent<LayoutElement>();
        spacerLE.flexibleWidth = 1;

        // Close button
        var closeGo = UIBuilder.CreateCloseButton(rowGo.transform, windowGo, Localizer.Format("#Kopernicus_UI_MissingTextures_Close"), 14);
        var closeLE = closeGo.AddOrGetComponent<LayoutElement>();
        closeLE.preferredWidth = 80;
    }

    private static void PopulateEntries(Transform parent)
    {
        var entries = MissingTextureLog.Entries;

        var grouped = entries.GroupBy(e => e.Body).OrderBy(g => g.Key?.name ?? NoBodyLabel);
        int groupIndex = 0;
        foreach (var group in grouped)
        {
            var bodyName = group.Key?.name ?? NoBodyLabel;
            var displayName = group.Key != null
                ? Localizer.Format(group.Key.celestialBody.displayName).Replace("^N", "")
                : NoBodyLabel;
            CreateBodyGroup(parent, bodyName, displayName, group.Select(e => e.TexturePath).Distinct(), groupIndex);
            groupIndex++;
        }
    }

    private static void CreateBodyGroup(Transform parent, string name, string displayName, IEnumerable<string> texturePaths, int index)
    {
        var groupGo = Object.Instantiate(UIBuilder.Prefab("UIBoxPrefab"), parent);
        groupGo.SetActive(true);
        groupGo.name = $"Group({name})";

        var image = groupGo.GetComponent<Image>();
        image.color = index % 2 == 0 ? GroupBackgroundEven : GroupBackgroundOdd;
        image.type = Image.Type.Sliced;

        var layout = groupGo.AddComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(6, 6, 4, 4);
        layout.spacing = 1;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;
        layout.childControlWidth = true;
        layout.childControlHeight = true;

        // Body name header
        var bodyGo = UIBuilder.CreateText(groupGo.transform, displayName, 13);
        var bodyTmp = bodyGo.GetComponent<TextMeshProUGUI>();
        bodyTmp.fontStyle = FontStyles.Bold;
        bodyTmp.color = BodyNameColor;

        // Texture paths
        foreach (var path in texturePaths)
        {
            var pathGo = UIBuilder.CreateText(groupGo.transform, path, 12);
            var pathTmp = pathGo.GetComponent<TextMeshProUGUI>();
            pathTmp.color = TexturePathColor;
        }
    }

    private static void DumpLayout(GameObject root)
    {
        var sb = new StringBuilder();
        DumpGameObject(sb, root, 0);

        var dir = Path.Combine(KSPUtil.ApplicationRootPath, "Logs/Kopernicus");
        Directory.CreateDirectory(dir);
        var path = Path.Combine(dir, "MissingTexturesWindow.txt");
        File.WriteAllText(path, sb.ToString());
        Debug.Log($"[Kopernicus] Window layout dumped to {path}");
    }

    private static void DumpGameObject(StringBuilder sb, GameObject go, int depth)
    {
        var indent = new string(' ', depth * 2);
        var rect = go.GetComponent<RectTransform>();
        var rectInfo = "";
        if (rect != null)
        {
            var sd = rect.sizeDelta;
            var amin = rect.anchorMin;
            var amax = rect.anchorMax;
            var piv = rect.pivot;
            var ap = rect.anchoredPosition;
            rectInfo = $" [size={sd.x}x{sd.y} anchor=({amin.x},{amin.y})-({amax.x},{amax.y}) pivot=({piv.x},{piv.y}) pos=({ap.x},{ap.y})]";
        }

        sb.AppendLine($"{indent}{go.name} (active={go.activeSelf}){rectInfo}");

        foreach (var comp in go.GetComponents<Component>())
        {
            if (comp == null || comp is Transform or RectTransform)
                continue;
            var compInfo = "";
            if (comp is TextMeshProUGUI tmp)
                compInfo = $" alignment={tmp.alignment} overflowMode={tmp.overflowMode} enableWordWrapping={tmp.enableWordWrapping}";
            sb.AppendLine($"{indent}  + {comp.GetType().Name}{compInfo}");
        }

        for (int i = 0; i < go.transform.childCount; i++)
            DumpGameObject(sb, go.transform.GetChild(i).gameObject, depth + 1);
    }
}
