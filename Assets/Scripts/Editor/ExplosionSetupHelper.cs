using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class ExplosionSetupHelper : EditorWindow
{
    private GameObject explosionPrefab;
    private string explosionObjectName = "Explosion";
    private bool setInactive = true;
    private bool addAutoDeactivate = true;
    
    [MenuItem("Tools/Explosion Setup Helper")]
    static void Init()
    {
        ExplosionSetupHelper window = (ExplosionSetupHelper)EditorWindow.GetWindow(typeof(ExplosionSetupHelper));
        window.titleContent = new GUIContent("Explosion Setup");
        window.Show();
    }
    
    void OnGUI()
    {
        GUILayout.Label("Explosion Setup Helper", EditorStyles.boldLabel);
        GUILayout.Space(10);
        
        explosionPrefab = (GameObject)EditorGUILayout.ObjectField("Explosion Prefab (Optional)", explosionPrefab, typeof(GameObject), false);
        explosionObjectName = EditorGUILayout.TextField("Explosion Name", explosionObjectName);
        setInactive = EditorGUILayout.Toggle("Set Inactive", setInactive);
        addAutoDeactivate = EditorGUILayout.Toggle("Add AutoDeactivate", addAutoDeactivate);
        
        GUILayout.Space(10);
        
        if (GUILayout.Button("Add Explosions to Selected GameObjects"))
        {
            AddExplosionsToSelected();
        }
        
        GUILayout.Space(10);
        
        if (GUILayout.Button("Create ExplosionManager"))
        {
            CreateExplosionManager();
        }
    }
    
    void AddExplosionsToSelected()
    {
        if (Selection.gameObjects.Length == 0)
        {
            EditorUtility.DisplayDialog("No Selection", "Please select GameObjects to add explosions to.", "OK");
            return;
        }
        
        int addedCount = 0;
        
        foreach (GameObject obj in Selection.gameObjects)
        {
            Transform existingExplosion = obj.transform.Find(explosionObjectName);
            
            if (existingExplosion != null)
            {
                Debug.LogWarning($"Explosion already exists on {obj.name}, skipping...");
                continue;
            }
            
            GameObject explosion;
            
            if (explosionPrefab != null)
            {
                explosion = (GameObject)PrefabUtility.InstantiatePrefab(explosionPrefab, obj.transform);
                explosion.name = explosionObjectName;
            }
            else
            {
                explosion = new GameObject(explosionObjectName);
                explosion.transform.SetParent(obj.transform);
            }
            
            explosion.transform.localPosition = Vector3.zero;
            explosion.transform.localRotation = Quaternion.identity;
            
            if (setInactive)
            {
                explosion.SetActive(false);
            }
            
            if (addAutoDeactivate && explosion.GetComponent<AutoDeactivateExplosion>() == null)
            {
                explosion.AddComponent<AutoDeactivateExplosion>();
            }
            
            addedCount++;
        }
        
        Debug.Log($"Added {addedCount} explosion(s) to selected GameObjects");
        EditorUtility.DisplayDialog("Complete", $"Added {addedCount} explosion(s) successfully!", "OK");
    }
    
    void CreateExplosionManager()
    {
        GameObject existingManager = GameObject.Find("ExplosionManager");
        
        if (existingManager != null)
        {
            bool overwrite = EditorUtility.DisplayDialog("ExplosionManager Exists", 
                "An ExplosionManager already exists. Replace it?", "Yes", "No");
            
            if (overwrite)
            {
                DestroyImmediate(existingManager);
            }
            else
            {
                return;
            }
        }
        
        GameObject manager = new GameObject("ExplosionManager");
        ExplosionManager explosionMgr = manager.AddComponent<ExplosionManager>();
        
        if (Selection.gameObjects.Length > 0)
        {
            bool addSelected = EditorUtility.DisplayDialog("Add Selected GameObjects?", 
                $"Add {Selection.gameObjects.Length} selected GameObjects as explosion targets?", "Yes", "No");
            
            if (addSelected)
            {
                List<GameObject> targets = new List<GameObject>(Selection.gameObjects);
                
                System.Reflection.FieldInfo field = typeof(ExplosionManager).GetField("explosionTargets");
                if (field != null)
                {
                    field.SetValue(explosionMgr, targets);
                }
            }
        }
        
        Selection.activeGameObject = manager;
        
        Debug.Log("ExplosionManager created successfully!");
        EditorUtility.DisplayDialog("Complete", "ExplosionManager created! Check Inspector to configure.", "OK");
    }
}
