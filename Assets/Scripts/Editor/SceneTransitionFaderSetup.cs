using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public static class SceneTransitionFaderSetup
{
    [MenuItem("Tools/Scene Flow/Create Scene Transition Fader")]
    public static void CreateSceneTransitionFader()
    {
        var root = new GameObject("SceneTransitionFader");
        Undo.RegisterCreatedObjectUndo(root, "Create Scene Transition Fader");

        var canvas = root.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 200;

        var scaler = root.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        root.AddComponent<GraphicRaycaster>();
        var canvasGroup = root.AddComponent<CanvasGroup>();
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        var imageGo = new GameObject("FadeOverlay");
        imageGo.transform.SetParent(root.transform, false);

        var imageRect = imageGo.AddComponent<RectTransform>();
        imageRect.anchorMin = Vector2.zero;
        imageRect.anchorMax = Vector2.one;
        imageRect.offsetMin = Vector2.zero;
        imageRect.offsetMax = Vector2.zero;

        var image = imageGo.AddComponent<Image>();
        image.color = Color.black;
        image.raycastTarget = true;

        var controller = root.AddComponent<SceneTransitionFader>();
        var so = new SerializedObject(controller);
        so.FindProperty("canvasGroup").objectReferenceValue = canvasGroup;
        so.FindProperty("fadeImage").objectReferenceValue = image;
        so.ApplyModifiedProperties();

        Selection.activeGameObject = root;
        Debug.Log("[SceneTransitionFaderSetup] Scene Transition Fader created and wired.");
    }
}
