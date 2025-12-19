using UnityEngine;
using UnityEditor;

public class ConsumableSpawner : EditorWindow
{
    private ConsumableItem selectedItem;
    private GameObject visualPrefab;
    private bool autoConsume = true;
    private int spawnCount = 1;
    private float spawnRadius = 5f;
    
    [MenuItem("Division Game/Survival/Spawn Consumables in Scene")]
    public static void ShowWindow()
    {
        GetWindow<ConsumableSpawner>("Consumable Spawner");
    }
    
    private void OnGUI()
    {
        EditorGUILayout.LabelField("Consumable Scene Spawner", EditorStyles.boldLabel);
        EditorGUILayout.Space();
        
        EditorGUILayout.HelpBox("Spawn consumable pickups in your scene. Select a consumable item and click spawn.", MessageType.Info);
        EditorGUILayout.Space();
        
        selectedItem = (ConsumableItem)EditorGUILayout.ObjectField("Consumable Item", selectedItem, typeof(ConsumableItem), false);
        visualPrefab = (GameObject)EditorGUILayout.ObjectField("Visual Prefab (Optional)", visualPrefab, typeof(GameObject), false);
        
        autoConsume = EditorGUILayout.Toggle("Auto-Consume on Pickup", autoConsume);
        spawnCount = EditorGUILayout.IntSlider("Spawn Count", spawnCount, 1, 50);
        spawnRadius = EditorGUILayout.Slider("Spawn Radius", spawnRadius, 1f, 50f);
        
        EditorGUILayout.Space();
        
        if (selectedItem == null)
        {
            EditorGUILayout.HelpBox("Select a ConsumableItem to spawn.", MessageType.Warning);
        }
        else
        {
            EditorGUILayout.LabelField($"Selected: {selectedItem.itemName}", EditorStyles.helpBox);
            
            if (selectedItem.hungerRestore > 0f)
                EditorGUILayout.LabelField($"  🍖 Hunger: +{selectedItem.hungerRestore:F0}");
            if (selectedItem.thirstRestore > 0f)
                EditorGUILayout.LabelField($"  💧 Thirst: +{selectedItem.thirstRestore:F0}");
            if (selectedItem.healthRestore > 0f)
                EditorGUILayout.LabelField($"  ❤ Health: +{selectedItem.healthRestore:F0}");
            if (selectedItem.staminaRestore > 0f)
                EditorGUILayout.LabelField($"  ⚡ Stamina: +{selectedItem.staminaRestore:F0}");
            if (selectedItem.infectionChange < 0f)
                EditorGUILayout.LabelField($"  💊 Infection: {selectedItem.infectionChange:F0}");
        }
        
        EditorGUILayout.Space();
        
        GUI.enabled = selectedItem != null;
        
        if (GUILayout.Button("Spawn at Scene View Center", GUILayout.Height(40)))
        {
            Vector3 spawnPosition = GetSceneViewCenterPosition();
            SpawnConsumables(spawnPosition);
        }
        
        if (GUILayout.Button("Spawn at Player Position", GUILayout.Height(40)))
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                SpawnConsumables(player.transform.position);
            }
            else
            {
                EditorUtility.DisplayDialog("No Player", "Could not find player in scene!", "OK");
            }
        }
        
        if (Selection.activeTransform != null)
        {
            if (GUILayout.Button($"Spawn at Selected Object ({Selection.activeTransform.name})", GUILayout.Height(40)))
            {
                SpawnConsumables(Selection.activeTransform.position);
            }
        }
        
        GUI.enabled = true;
        
        EditorGUILayout.Space(10);
        
        if (GUILayout.Button("Clear All Consumables in Scene", GUILayout.Height(30)))
        {
            if (EditorUtility.DisplayDialog("Clear Consumables", 
                "Are you sure you want to delete all ConsumablePickup objects in the scene?", 
                "Yes", "No"))
            {
                ClearAllConsumables();
            }
        }
    }
    
    private Vector3 GetSceneViewCenterPosition()
    {
        SceneView sceneView = SceneView.lastActiveSceneView;
        if (sceneView != null)
        {
            return sceneView.pivot;
        }
        return Vector3.zero;
    }
    
    private void SpawnConsumables(Vector3 centerPosition)
    {
        if (selectedItem == null)
        {
            EditorUtility.DisplayDialog("No Item Selected", "Please select a consumable item first.", "OK");
            return;
        }
        
        GameObject parent = new GameObject($"Consumables_{selectedItem.itemName}");
        Undo.RegisterCreatedObjectUndo(parent, "Spawn Consumables");
        
        for (int i = 0; i < spawnCount; i++)
        {
            Vector3 randomOffset = Random.insideUnitSphere * spawnRadius;
            randomOffset.y = Mathf.Abs(randomOffset.y);
            Vector3 spawnPosition = centerPosition + randomOffset;
            
            GameObject pickup = new GameObject($"{selectedItem.itemName}_{i}");
            pickup.transform.position = spawnPosition;
            pickup.transform.SetParent(parent.transform);
            
            SphereCollider collider = pickup.AddComponent<SphereCollider>();
            collider.isTrigger = true;
            collider.radius = 1f;
            
            ConsumablePickup consumable = pickup.AddComponent<ConsumablePickup>();
            consumable.itemData = selectedItem;
            consumable.autoConsumeOnPickup = autoConsume;
            consumable.visualPrefab = visualPrefab;
            
            if (visualPrefab != null)
            {
                GameObject visual = (GameObject)PrefabUtility.InstantiatePrefab(visualPrefab, pickup.transform);
                visual.transform.localPosition = Vector3.zero;
            }
            else
            {
                GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                sphere.transform.SetParent(pickup.transform);
                sphere.transform.localPosition = Vector3.zero;
                sphere.transform.localScale = Vector3.one * 0.5f;
                
                if (sphere.GetComponent<Collider>() != null)
                    DestroyImmediate(sphere.GetComponent<Collider>());
                
                Renderer renderer = sphere.GetComponent<Renderer>();
                if (renderer != null)
                {
                    Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                    
                    if (selectedItem.hungerRestore > 0f)
                        mat.color = new Color(1f, 0.6f, 0.2f);
                    else if (selectedItem.thirstRestore > 0f)
                        mat.color = new Color(0.3f, 0.7f, 1f);
                    else if (selectedItem.healthRestore > 0f)
                        mat.color = new Color(1f, 0.3f, 0.3f);
                    else
                        mat.color = Color.white;
                    
                    renderer.material = mat;
                }
            }
            
            Undo.RegisterCreatedObjectUndo(pickup, "Spawn Consumable");
        }
        
        Selection.activeGameObject = parent;
        Debug.Log($"<color=green>✓ Spawned {spawnCount}x {selectedItem.itemName} at {centerPosition}</color>");
    }
    
    private void ClearAllConsumables()
    {
        ConsumablePickup[] allPickups = FindObjectsByType<ConsumablePickup>(FindObjectsSortMode.None);
        
        foreach (ConsumablePickup pickup in allPickups)
        {
            Undo.DestroyObjectImmediate(pickup.gameObject);
        }
        
        Debug.Log($"<color=yellow>Cleared {allPickups.Length} consumable pickups from scene</color>");
    }
}
