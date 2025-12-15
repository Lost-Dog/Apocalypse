using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using JUTPS;

public class WorldSpaceHealthBarSetup : EditorWindow
{
    private GameObject characterPrefab;
    private GameObject healthBarPrefab;
    private string characterName = "Enemy";
    private int characterLevel = 1;
    private Vector3 offset = new Vector3(0f, 2.5f, 0f);
    
    [MenuItem("Tools/Character Health Bar Setup")]
    public static void ShowWindow()
    {
        GetWindow<WorldSpaceHealthBarSetup>("Health Bar Setup");
    }
    
    private void OnGUI()
    {
        EditorGUILayout.LabelField("World Space Health Bar Setup", EditorStyles.boldLabel);
        EditorGUILayout.Space(10);
        
        EditorGUILayout.HelpBox("This tool helps you add world-space health bars to characters with JUHealth component.", MessageType.Info);
        EditorGUILayout.Space(10);
        
        characterPrefab = EditorGUILayout.ObjectField("Character Prefab/GameObject", characterPrefab, typeof(GameObject), true) as GameObject;
        
        EditorGUILayout.Space(5);
        
        healthBarPrefab = EditorGUILayout.ObjectField("Health Bar Prefab (Optional)", healthBarPrefab, typeof(GameObject), false) as GameObject;
        
        EditorGUILayout.Space(5);
        
        characterName = EditorGUILayout.TextField("Character Name", characterName);
        characterLevel = EditorGUILayout.IntField("Character Level", characterLevel);
        
        EditorGUILayout.Space(5);
        
        offset = EditorGUILayout.Vector3Field("Height Offset", offset);
        
        EditorGUILayout.Space(10);
        
        if (characterPrefab == null)
        {
            EditorGUILayout.HelpBox("Please assign a character GameObject or prefab.", MessageType.Warning);
        }
        else
        {
            JUHealth health = characterPrefab.GetComponent<JUHealth>();
            if (health == null)
            {
                EditorGUILayout.HelpBox("⚠️ Character doesn't have JUHealth component!", MessageType.Error);
            }
            else
            {
                EditorGUILayout.HelpBox($"✓ JUHealth found: {health.Health}/{health.MaxHealth} HP", MessageType.Info);
            }
        }
        
        EditorGUILayout.Space(10);
        
        GUI.enabled = characterPrefab != null;
        
        if (GUILayout.Button("Add Health Bar to Character", GUILayout.Height(40)))
        {
            AddHealthBarToCharacter();
        }
        
        GUI.enabled = true;
        
        EditorGUILayout.Space(10);
        
        EditorGUILayout.LabelField("Quick Actions", EditorStyles.boldLabel);
        
        if (GUILayout.Button("Create ApocalypseHUD Health Bar Prefab"))
        {
            CreateApocalypseHealthBarPrefab();
        }
        
        if (GUILayout.Button("Create Simple Health Bar Prefab"))
        {
            CreateSimpleHealthBarPrefab();
        }
        
        EditorGUILayout.Space(10);
        
        EditorGUILayout.HelpBox("ApocalypseHUD prefabs are located in:\nAssets/Synty/InterfaceApocalypseHUD/Prefabs/NPC_HealthBars_EnemyData/", MessageType.Info);
    }
    
    private void AddHealthBarToCharacter()
    {
        if (characterPrefab == null) return;
        
        JUHealth health = characterPrefab.GetComponent<JUHealth>();
        if (health == null)
        {
            EditorUtility.DisplayDialog("Error", "Character must have JUHealth component!", "OK");
            return;
        }
        
        GameObject healthBarInstance;
        
        if (healthBarPrefab != null)
        {
            healthBarInstance = PrefabUtility.InstantiatePrefab(healthBarPrefab) as GameObject;
        }
        else
        {
            healthBarInstance = CreateSimpleHealthBarInstance();
        }
        
        if (healthBarInstance == null)
        {
            Debug.LogError("Failed to create health bar instance");
            return;
        }
        
        healthBarInstance.transform.SetParent(characterPrefab.transform);
        healthBarInstance.transform.localPosition = offset;
        healthBarInstance.transform.localRotation = Quaternion.identity;
        healthBarInstance.transform.localScale = Vector3.one;
        
        WorldSpaceHealthBar healthBarScript = healthBarInstance.GetComponent<WorldSpaceHealthBar>();
        if (healthBarScript == null)
        {
            healthBarScript = healthBarInstance.AddComponent<WorldSpaceHealthBar>();
        }
        
        SerializedObject so = new SerializedObject(healthBarScript);
        so.FindProperty("targetHealth").objectReferenceValue = health;
        so.FindProperty("targetTransform").objectReferenceValue = characterPrefab.transform;
        so.FindProperty("worldOffset").vector3Value = offset;
        so.ApplyModifiedProperties();
        
        healthBarScript.SetName(characterName);
        healthBarScript.SetLevel(characterLevel);
        
        EditorUtility.SetDirty(characterPrefab);
        
        if (PrefabUtility.IsPartOfPrefabInstance(characterPrefab))
        {
            PrefabUtility.RecordPrefabInstancePropertyModifications(characterPrefab);
        }
        
        Debug.Log($"✓ Added health bar to {characterPrefab.name}");
        EditorUtility.DisplayDialog("Success", $"Health bar added to {characterPrefab.name}!", "OK");
    }
    
    private GameObject CreateSimpleHealthBarInstance()
    {
        GameObject healthBarObj = new GameObject("HealthBar_WorldSpace");
        
        Canvas canvas = healthBarObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.worldCamera = Camera.main;
        
        RectTransform canvasRect = healthBarObj.GetComponent<RectTransform>();
        canvasRect.sizeDelta = new Vector2(2f, 0.3f);
        canvasRect.localScale = new Vector3(0.01f, 0.01f, 0.01f);
        
        healthBarObj.AddComponent<CanvasGroup>();
        
        GameObject bgObj = new GameObject("Background");
        bgObj.transform.SetParent(healthBarObj.transform);
        Image bgImage = bgObj.AddComponent<Image>();
        bgImage.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
        RectTransform bgRect = bgObj.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.sizeDelta = Vector2.zero;
        bgRect.anchoredPosition = Vector2.zero;
        
        GameObject sliderObj = new GameObject("HealthSlider");
        sliderObj.transform.SetParent(healthBarObj.transform);
        Slider slider = sliderObj.AddComponent<Slider>();
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.value = 1f;
        RectTransform sliderRect = sliderObj.GetComponent<RectTransform>();
        sliderRect.anchorMin = Vector2.zero;
        sliderRect.anchorMax = Vector2.one;
        sliderRect.sizeDelta = Vector2.zero;
        sliderRect.anchoredPosition = Vector2.zero;
        
        GameObject fillAreaObj = new GameObject("Fill Area");
        fillAreaObj.transform.SetParent(sliderObj.transform);
        RectTransform fillAreaRect = fillAreaObj.AddComponent<RectTransform>();
        fillAreaRect.anchorMin = Vector2.zero;
        fillAreaRect.anchorMax = Vector2.one;
        fillAreaRect.sizeDelta = new Vector2(-10f, -10f);
        fillAreaRect.anchoredPosition = Vector2.zero;
        
        GameObject fillObj = new GameObject("Fill");
        fillObj.transform.SetParent(fillAreaObj.transform);
        Image fillImage = fillObj.AddComponent<Image>();
        fillImage.color = Color.green;
        RectTransform fillRect = fillObj.GetComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.sizeDelta = Vector2.zero;
        fillRect.anchoredPosition = Vector2.zero;
        
        slider.fillRect = fillRect;
        slider.targetGraphic = fillImage;
        
        WorldSpaceHealthBar healthBarScript = healthBarObj.AddComponent<WorldSpaceHealthBar>();
        SerializedObject so = new SerializedObject(healthBarScript);
        so.FindProperty("worldSpaceCanvas").objectReferenceValue = canvas;
        so.FindProperty("healthSlider").objectReferenceValue = slider;
        so.FindProperty("fillImage").objectReferenceValue = fillImage;
        so.ApplyModifiedProperties();
        
        return healthBarObj;
    }
    
    private void CreateApocalypseHealthBarPrefab()
    {
        string prefabPath = "Assets/Synty/InterfaceApocalypseHUD/Prefabs/NPC_HealthBars_EnemyData/HUD_Apocalypse_WorldSpace_EnemyInfo_01.prefab";
        
        GameObject apocalypsePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        
        if (apocalypsePrefab == null)
        {
            EditorUtility.DisplayDialog("Not Found", "ApocalypseHUD prefab not found. Make sure the InterfaceApocalypseHUD package is installed.", "OK");
            return;
        }
        
        healthBarPrefab = apocalypsePrefab;
        Debug.Log($"✓ Loaded ApocalypseHUD health bar prefab: {prefabPath}");
        EditorUtility.DisplayDialog("Success", "ApocalypseHUD health bar prefab loaded!\n\nYou can now add it to characters using the 'Add Health Bar' button.", "OK");
    }
    
    private void CreateSimpleHealthBarPrefab()
    {
        GameObject healthBarObj = CreateSimpleHealthBarInstance();
        
        string savePath = EditorUtility.SaveFilePanelInProject(
            "Save Health Bar Prefab",
            "HealthBar_Simple",
            "prefab",
            "Save the health bar as a prefab",
            "Assets/Prefabs"
        );
        
        if (string.IsNullOrEmpty(savePath))
        {
            DestroyImmediate(healthBarObj);
            return;
        }
        
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(healthBarObj, savePath);
        DestroyImmediate(healthBarObj);
        
        healthBarPrefab = prefab;
        
        EditorUtility.DisplayDialog("Success", $"Simple health bar prefab created at:\n{savePath}", "OK");
        Debug.Log($"✓ Created simple health bar prefab: {savePath}");
    }
}
