using UnityEditor;
using UnityEngine;
#if COMPASS_NAVIGATOR_PRO
using CompassNavigatorPro;
#endif

/// <summary>
/// Temporary editor window to tune CompassPro global indicator scales and per-POI scales live in the scene.
/// Open via Tools > Compass Scale Tool.
/// Requires the COMPASS_NAVIGATOR_PRO scripting define to be active.
/// </summary>
#if COMPASS_NAVIGATOR_PRO
public class CompassScaleTool : EditorWindow
{
    private CompassPro _compass;
    private Vector2 _poiScroll;

    private SerializedObject _compassSO;
    private SerializedProperty _onScreenScaleProp;
    private SerializedProperty _offScreenScaleProp;

    [MenuItem("Tools/Compass Scale Tool")]
    private static void Open()
    {
        GetWindow<CompassScaleTool>("Compass Scale Tool");
    }

    private void OnEnable()
    {
        TryBindCompass();
    }

    private void OnFocus()
    {
        TryBindCompass();
    }

    private void TryBindCompass()
    {
        _compass = FindFirstObjectByType<CompassPro>();

        if (_compass == null)
        {
            _compassSO = null;
            _onScreenScaleProp = null;
            _offScreenScaleProp = null;
            return;
        }

        _compassSO = new SerializedObject(_compass);
        _onScreenScaleProp = _compassSO.FindProperty("_onScreenIndicatorScale");
        _offScreenScaleProp = _compassSO.FindProperty("_offScreenIndicatorScale");
    }

    private void OnGUI()
    {
        if (_compass == null || _compassSO == null)
        {
            EditorGUILayout.HelpBox("No CompassPro found in the loaded scene.", MessageType.Warning);
            if (GUILayout.Button("Retry")) TryBindCompass();
            return;
        }

        _compassSO.Update();

        EditorGUILayout.LabelField("Global Scales", EditorStyles.boldLabel);
        EditorGUI.BeginChangeCheck();

        EditorGUILayout.PropertyField(_onScreenScaleProp, new GUIContent("On-Screen Scale", "Multiplier for indicators rendered over in-view POIs."));
        EditorGUILayout.PropertyField(_offScreenScaleProp, new GUIContent("Off-Screen Scale", "Multiplier for edge indicators when POIs are outside the screen."));

        if (EditorGUI.EndChangeCheck())
        {
            _compassSO.ApplyModifiedProperties();
            EditorUtility.SetDirty(_compass);
        }

        EditorGUILayout.Space(12);
        EditorGUILayout.LabelField("Per-POI Scale Override", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("poi.onScreenIndicatorScale multiplies into both on-screen and off-screen final scale.", MessageType.None);

        CompassProPOI[] pois = FindObjectsByType<CompassProPOI>(FindObjectsSortMode.None);

        if (pois.Length == 0)
        {
            EditorGUILayout.LabelField("No CompassProPOI components found in scene.");
            return;
        }

        _poiScroll = EditorGUILayout.BeginScrollView(_poiScroll);

        foreach (CompassProPOI poi in pois)
        {
            if (poi == null) continue;

            SerializedObject poiSO = new SerializedObject(poi);
            SerializedProperty scaleProp = poiSO.FindProperty("onScreenIndicatorScale");

            if (scaleProp == null) continue;

            poiSO.Update();

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.ObjectField(poi, typeof(CompassProPOI), true);
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(scaleProp, new GUIContent("Scale Override"));
            EditorGUI.indentLevel--;

            if (EditorGUI.EndChangeCheck())
            {
                poiSO.ApplyModifiedProperties();
                EditorUtility.SetDirty(poi);
            }
        }

        EditorGUILayout.EndScrollView();

        EditorGUILayout.Space(8);
        if (GUILayout.Button("Mark Scene Dirty"))
        {
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        }
    }
}
#endif // COMPASS_NAVIGATOR_PRO
