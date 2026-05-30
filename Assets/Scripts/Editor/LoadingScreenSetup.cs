using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public static class LoadingScreenSetup
{
    [MenuItem("Tools/Scene Flow/Create Loading Screen Canvas")]
    public static void CreateLoadingScreenCanvas()
    {
        // ── Canvas ──────────────────────────────────────────────────────────
        var canvasGo = new GameObject("LoadingScreen");
        Undo.RegisterCreatedObjectUndo(canvasGo, "Create Loading Screen Canvas");

        var canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100; // above gameplay UI

        canvasGo.AddComponent<CanvasScaler>().uiScaleMode =
            CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasGo.GetComponent<CanvasScaler>().referenceResolution = new Vector2(1920, 1080);
        canvasGo.AddComponent<GraphicRaycaster>();
        var cueSource = canvasGo.AddComponent<AudioSource>();
        cueSource.playOnAwake = false;
        cueSource.loop = false;
        cueSource.spatialBlend = 0f;

        var canvasGroup = canvasGo.AddComponent<CanvasGroup>();

        // ── Background ──────────────────────────────────────────────────────
        var bg = CreateUIImage(canvasGo, "Background", new Color(0.05f, 0.05f, 0.05f, 1f));
        StretchFull(bg.GetComponent<RectTransform>());

        // ── Content holder (centred column) ────────────────────────────────
        var content = new GameObject("Content");
        content.transform.SetParent(canvasGo.transform, false);
        var contentRect = content.AddComponent<RectTransform>();
        contentRect.anchorMin        = new Vector2(0.1f, 0.3f);
        contentRect.anchorMax        = new Vector2(0.9f, 0.7f);
        contentRect.offsetMin        = Vector2.zero;
        contentRect.offsetMax        = Vector2.zero;

        var layout = content.AddComponent<VerticalLayoutGroup>();
        layout.childAlignment        = TextAnchor.MiddleCenter;
        layout.spacing               = 20f;
        layout.childControlHeight    = false;
        layout.childControlWidth     = true;
        layout.childForceExpandWidth = true;

        // ── Phase label ─────────────────────────────────────────────────────
        var phaseLabel = CreateTMPLabel(content, "PhaseLabel", "Loading...", 28, TextAlignmentOptions.Center);
        phaseLabel.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 50);
        phaseLabel.color = new Color(0.85f, 0.85f, 0.85f);

        // ── Optional phase artwork ──────────────────────────────────────────
        var art = CreateUIImage(content, "PhaseArtwork", new Color(1f, 1f, 1f, 0.15f));
        var artRect = art.GetComponent<RectTransform>();
        artRect.sizeDelta = new Vector2(0f, 140f);
        var artImage = art.GetComponent<Image>();
        artImage.preserveAspect = true;

        // ── Tip label ───────────────────────────────────────────────────────
        var tipLabel = CreateTMPLabel(content, "TipLabel", "Preparing world...", 20, TextAlignmentOptions.Center);
        tipLabel.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 56);
        tipLabel.color = new Color(0.72f, 0.72f, 0.72f);

        // ── Progress bar container ──────────────────────────────────────────
        var barContainer = new GameObject("ProgressBarContainer");
        barContainer.transform.SetParent(content.transform, false);
        var barContainerRect = barContainer.AddComponent<RectTransform>();
        barContainerRect.sizeDelta = new Vector2(0, 20);

        // Bar track
        var track = CreateUIImage(barContainer, "Track", new Color(0.2f, 0.2f, 0.2f));
        StretchFull(track.GetComponent<RectTransform>());
        track.GetComponent<Image>().type = Image.Type.Sliced;

        // Bar fill
        var fill = CreateUIImage(barContainer, "Fill", new Color(0.2f, 0.7f, 1f));
        var fillRect = fill.GetComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.offsetMin = Vector2.zero;
        fillRect.offsetMax = Vector2.zero;
        fillRect.sizeDelta = Vector2.zero;
        var fillImage = fill.GetComponent<Image>();
        fillImage.type      = Image.Type.Filled;
        fillImage.fillMethod = Image.FillMethod.Horizontal;
        fillImage.fillAmount = 0f;

        // ── Percentage label ────────────────────────────────────────────────
        var pctLabel = CreateTMPLabel(content, "PercentageLabel", "0%", 22, TextAlignmentOptions.Center);
        pctLabel.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 36);
        pctLabel.color = new Color(0.6f, 0.6f, 0.6f);

        // ── Attach controller ───────────────────────────────────────────────
        var controller = canvasGo.AddComponent<LoadingScreenController>();

        var so = new SerializedObject(controller);
        so.FindProperty("canvasGroup").objectReferenceValue     = canvasGroup;
        so.FindProperty("progressBarFill").objectReferenceValue = fillImage;
        so.FindProperty("phaseLabel").objectReferenceValue      = phaseLabel;
        so.FindProperty("percentageLabel").objectReferenceValue = pctLabel;
        so.FindProperty("tipLabel").objectReferenceValue        = tipLabel;
        so.FindProperty("phaseArtwork").objectReferenceValue    = artImage;
        so.FindProperty("cueAudioSource").objectReferenceValue  = cueSource;

        SerializedProperty phaseContent = so.FindProperty("phaseContent");
        if (phaseContent != null)
        {
            phaseContent.arraySize = 5;
            SetPhaseContentEntry(phaseContent, 0, "Loading Scene", "Initializing loading systems...");
            SetPhaseContentEntry(phaseContent, 1, "Environment Scene", "Streaming world geometry and terrain...");
            SetPhaseContentEntry(phaseContent, 2, "Environmental Effects Scene", "Applying atmosphere, lighting, and weather...");
            SetPhaseContentEntry(phaseContent, 3, "Gameplay Objects Scene", "Spawning gameplay systems and actors...");
            SetPhaseContentEntry(phaseContent, 4, "Complete", "Ready. Entering game...");
        }

        so.ApplyModifiedProperties();

        // ── Finish ──────────────────────────────────────────────────────────
        Selection.activeGameObject = canvasGo;

        Debug.Log("[LoadingScreenSetup] Loading Screen Canvas created and wired. " +
                  "Place this GameObject in your Loading scene.");
    }

    // ── Helpers ─────────────────────────────────────────────────────────────
    private static GameObject CreateUIImage(GameObject parent, string name, Color color)
    {
        var go   = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        go.AddComponent<RectTransform>();
        var img  = go.AddComponent<Image>();
        img.color = color;
        img.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
        return go;
    }

    private static TMP_Text CreateTMPLabel(GameObject parent, string name,
                                           string text, float fontSize,
                                           TextAlignmentOptions alignment)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        go.AddComponent<RectTransform>();
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text           = text;
        tmp.fontSize       = fontSize;
        tmp.alignment      = alignment;
        tmp.color          = Color.white;
        return tmp;
    }

    private static void StretchFull(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    private static void SetPhaseContentEntry(SerializedProperty arrayProp, int index, string phaseName, string tip)
    {
        SerializedProperty entry = arrayProp.GetArrayElementAtIndex(index);
        if (entry == null)
        {
            return;
        }

        SerializedProperty nameProp = entry.FindPropertyRelative("phaseName");
        SerializedProperty tipProp = entry.FindPropertyRelative("tipText");
        if (nameProp != null) nameProp.stringValue = phaseName;
        if (tipProp != null) tipProp.stringValue = tip;
    }
}
