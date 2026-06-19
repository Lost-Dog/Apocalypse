using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public static class MainMenuSetup
{
    private const float ButtonWidth  = 280f;
    private const float ButtonHeight = 60f;
    private const float ButtonSpacing = 16f;

    [MenuItem("Tools/Scene Flow/Create Main Menu Canvas")]
    public static void CreateMainMenuCanvas()
    {
        // ── Root canvas ─────────────────────────────────────────────────────
        var canvasGo = new GameObject("MainMenu");
        Undo.RegisterCreatedObjectUndo(canvasGo, "Create Main Menu Canvas");

        var canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode   = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 10;

        var scaler = canvasGo.AddComponent<CanvasScaler>();
        scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight  = 0.5f;

        canvasGo.AddComponent<GraphicRaycaster>();
        var canvasGroup = canvasGo.AddComponent<CanvasGroup>();

        // ── Background ──────────────────────────────────────────────────────
        var bg = CreateImage(canvasGo, "Background", new Color(0.06f, 0.06f, 0.08f, 1f));
        Stretch(bg.rectTransform);

        // ── Title ────────────────────────────────────────────────────────────
        var title = CreateTMP(canvasGo, "Title", "APOCALYPSE", 72f, TextAlignmentOptions.Center);
        title.fontStyle   = FontStyles.Bold;
        title.color       = Color.white;
        var titleRect     = title.GetComponent<RectTransform>();
        titleRect.anchorMin        = new Vector2(0f, 0.7f);
        titleRect.anchorMax        = new Vector2(1f, 0.9f);
        titleRect.offsetMin        = Vector2.zero;
        titleRect.offsetMax        = Vector2.zero;

        // ── Version label ────────────────────────────────────────────────────
        var version = CreateTMP(canvasGo, "VersionLabel", "v0.1", 18f, TextAlignmentOptions.BottomRight);
        version.color = new Color(0.5f, 0.5f, 0.5f);
        var versionRect = version.GetComponent<RectTransform>();
        versionRect.anchorMin = new Vector2(0.5f, 0f);
        versionRect.anchorMax = new Vector2(1f,   0f);
        versionRect.offsetMin = new Vector2(0f,  12f);
        versionRect.offsetMax = new Vector2(-20f, 48f);

        // ── Button column ────────────────────────────────────────────────────
        var buttonPanel = new GameObject("Buttons");
        buttonPanel.transform.SetParent(canvasGo.transform, false);
        var panelRect = buttonPanel.AddComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.3f);
        panelRect.anchorMax = new Vector2(0.5f, 0.65f);
        panelRect.sizeDelta = new Vector2(ButtonWidth, 0f);

        var layout = buttonPanel.AddComponent<VerticalLayoutGroup>();
        layout.childAlignment        = TextAnchor.MiddleCenter;
        layout.spacing               = ButtonSpacing;
        layout.childControlHeight    = false;
        layout.childControlWidth     = true;
        layout.childForceExpandWidth = true;

        var csf = buttonPanel.AddComponent<ContentSizeFitter>();
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        CreateButton(buttonPanel, "PlayButton",     "PLAY",     new Color(0.15f, 0.55f, 0.95f));
        Button settingsBtn = CreateButton(buttonPanel, "SettingsButton", "SETTINGS", new Color(0.25f, 0.25f, 0.28f));
        Button quitBtn     = CreateButton(buttonPanel, "QuitButton",     "QUIT",     new Color(0.55f, 0.12f, 0.12f));

        // ── Settings panel (placeholder) ─────────────────────────────────────
        var settingsPanel = new GameObject("SettingsPanel");
        settingsPanel.transform.SetParent(canvasGo.transform, false);
        var settingsPanelRect = settingsPanel.AddComponent<RectTransform>();
        settingsPanelRect.anchorMin = new Vector2(0.25f, 0.2f);
        settingsPanelRect.anchorMax = new Vector2(0.75f, 0.8f);
        settingsPanelRect.offsetMin = Vector2.zero;
        settingsPanelRect.offsetMax = Vector2.zero;

        var settingsBg = settingsPanel.AddComponent<Image>();
        settingsBg.color = new Color(0.1f, 0.1f, 0.12f, 0.97f);

        var settingsTitle = CreateTMP(settingsPanel, "SettingsTitle", "SETTINGS", 36f, TextAlignmentOptions.Top);
        settingsTitle.color = Color.white;
        var stRect = settingsTitle.GetComponent<RectTransform>();
        stRect.anchorMin = new Vector2(0f, 0.8f);
        stRect.anchorMax = new Vector2(1f, 1f);
        stRect.offsetMin = Vector2.zero;
        stRect.offsetMax = Vector2.zero;

        settingsPanel.SetActive(false);

        Selection.activeGameObject = canvasGo;
        Debug.Log("[MainMenuSetup] Main Menu Canvas created. Configure your own menu manager as needed.");
    }

    // ── Helpers ───────────────────────────────────────────────────────────────
    private static Button CreateButton(GameObject parent, string name, string label, Color color)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);

        var rt = go.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(ButtonWidth, ButtonHeight);

        var img = go.AddComponent<Image>();
        img.color = color;

        var btn = go.AddComponent<Button>();

        // Tint colours
        var colors        = btn.colors;
        colors.normalColor      = color;
        colors.highlightedColor = color * 1.2f;
        colors.pressedColor     = color * 0.7f;
        colors.selectedColor    = color;
        btn.colors              = colors;

        var tmp = CreateTMP(go, "Label", label, 24f, TextAlignmentOptions.Center);
        tmp.color     = Color.white;
        tmp.fontStyle = FontStyles.Bold;
        Stretch(tmp.GetComponent<RectTransform>());

        return btn;
    }

    private static Image CreateImage(GameObject parent, string name, Color color)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        go.AddComponent<RectTransform>();
        var img = go.AddComponent<Image>();
        img.color = color;
        return img;
    }

    private static TextMeshProUGUI CreateTMP(GameObject parent, string name,
                                              string text, float size,
                                              TextAlignmentOptions alignment)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        go.AddComponent<RectTransform>();
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text      = text;
        tmp.fontSize  = size;
        tmp.alignment = alignment;
        tmp.color     = Color.white;
        return tmp;
    }

    private static void Stretch(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }
}
