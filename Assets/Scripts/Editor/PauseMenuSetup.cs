using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public static class PauseMenuSetup
{
    private const float ButtonWidth   = 260f;
    private const float ButtonHeight  = 58f;
    private const float ButtonSpacing = 14f;

    [MenuItem("Tools/Scene Flow/Create Pause Menu Canvas")]
    public static void CreatePauseMenuCanvas()
    {
        // ── Root canvas ─────────────────────────────────────────────────────
        var canvasGo = new GameObject("PauseMenu");
        Undo.RegisterCreatedObjectUndo(canvasGo, "Create Pause Menu Canvas");

        var canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode   = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 50;

        var scaler = canvasGo.AddComponent<CanvasScaler>();
        scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight  = 0.5f;

        canvasGo.AddComponent<GraphicRaycaster>();
        var canvasGroup = canvasGo.AddComponent<CanvasGroup>();

        // ── Full-screen dim overlay ──────────────────────────────────────────
        var overlay = CreateImage(canvasGo, "Overlay", new Color(0f, 0f, 0f, 0.6f));
        Stretch(overlay.rectTransform);

        // ── Centre panel ─────────────────────────────────────────────────────
        var panel = new GameObject("Panel");
        panel.transform.SetParent(canvasGo.transform, false);
        var panelRect = panel.AddComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.35f, 0.2f);
        panelRect.anchorMax = new Vector2(0.65f, 0.8f);
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;

        var panelImg = panel.AddComponent<Image>();
        panelImg.color = new Color(0.08f, 0.08f, 0.1f, 0.97f);

        // ── Header ───────────────────────────────────────────────────────────
        var header = CreateTMP(panel, "Header", "PAUSED", 48f, TextAlignmentOptions.Top);
        header.color     = Color.white;
        header.fontStyle = FontStyles.Bold;
        var headerRect   = header.GetComponent<RectTransform>();
        headerRect.anchorMin = new Vector2(0f, 0.78f);
        headerRect.anchorMax = new Vector2(1f, 1f);
        headerRect.offsetMin = new Vector2(0f,   0f);
        headerRect.offsetMax = new Vector2(0f, -12f);

        // ── Button column ────────────────────────────────────────────────────
        var buttonPanel = new GameObject("Buttons");
        buttonPanel.transform.SetParent(panel.transform, false);
        var bpRect = buttonPanel.AddComponent<RectTransform>();
        bpRect.anchorMin = new Vector2(0.5f, 0.1f);
        bpRect.anchorMax = new Vector2(0.5f, 0.75f);
        bpRect.sizeDelta = new Vector2(ButtonWidth, 0f);

        var layout = buttonPanel.AddComponent<VerticalLayoutGroup>();
        layout.childAlignment        = TextAnchor.MiddleCenter;
        layout.spacing               = ButtonSpacing;
        layout.childControlHeight    = false;
        layout.childControlWidth     = true;
        layout.childForceExpandWidth = true;

        var csf = buttonPanel.AddComponent<ContentSizeFitter>();
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        Button resumeBtn     = CreateButton(buttonPanel, "ResumeButton",      "RESUME",       new Color(0.15f, 0.55f, 0.95f));
        Button settingsBtn   = CreateButton(buttonPanel, "SettingsButton",     "SETTINGS",     new Color(0.25f, 0.25f, 0.28f));
        Button quitMenuBtn   = CreateButton(buttonPanel, "QuitToMenuButton",   "QUIT TO MENU", new Color(0.55f, 0.35f, 0.1f));
        Button quitGameBtn   = CreateButton(buttonPanel, "QuitGameButton",     "QUIT GAME",    new Color(0.55f, 0.12f, 0.12f));

        // ── Settings panel (placeholder) ─────────────────────────────────────
        var settingsPanel = new GameObject("SettingsPanel");
        settingsPanel.transform.SetParent(canvasGo.transform, false);
        var spRect = settingsPanel.AddComponent<RectTransform>();
        spRect.anchorMin = new Vector2(0.15f, 0.15f);
        spRect.anchorMax = new Vector2(0.85f, 0.85f);
        spRect.offsetMin = Vector2.zero;
        spRect.offsetMax = Vector2.zero;

        var spImg = settingsPanel.AddComponent<Image>();
        spImg.color = new Color(0.1f, 0.1f, 0.12f, 0.97f);

        var spTitle = CreateTMP(settingsPanel, "SettingsTitle", "SETTINGS", 36f, TextAlignmentOptions.Top);
        spTitle.color = Color.white;
        var sptRect   = spTitle.GetComponent<RectTransform>();
        sptRect.anchorMin = new Vector2(0f, 0.85f);
        sptRect.anchorMax = new Vector2(1f, 1f);
        sptRect.offsetMin = Vector2.zero;
        sptRect.offsetMax = Vector2.zero;

        settingsPanel.SetActive(false);

        // ── Wire controller ──────────────────────────────────────────────────
        var controller = canvasGo.AddComponent<PauseMenuController>();

        var so = new SerializedObject(controller);
        so.FindProperty("canvasGroup").objectReferenceValue    = canvasGroup;
        so.FindProperty("resumeButton").objectReferenceValue   = resumeBtn;
        so.FindProperty("settingsButton").objectReferenceValue = settingsBtn;
        so.FindProperty("quitToMenuButton").objectReferenceValue = quitMenuBtn;
        so.FindProperty("quitGameButton").objectReferenceValue = quitGameBtn;
        so.FindProperty("headerLabel").objectReferenceValue    = header;
        so.FindProperty("settingsPanel").objectReferenceValue  = settingsPanel;
        so.ApplyModifiedProperties();

        Selection.activeGameObject = canvasGo;
        Debug.Log("[PauseMenuSetup] Pause Menu Canvas created and wired. Place this in your gameplay scene.");
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

        var btn    = go.AddComponent<Button>();
        var colors = btn.colors;
        colors.normalColor      = color;
        colors.highlightedColor = color * 1.25f;
        colors.pressedColor     = color * 0.65f;
        colors.selectedColor    = color;
        btn.colors              = colors;

        var tmp = CreateTMP(go, "Label", label, 22f, TextAlignmentOptions.Center);
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
