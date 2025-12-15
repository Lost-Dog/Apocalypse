using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Reflection;

public class DebugCharacterSpawnerWindow : EditorWindow
{
    [MenuItem("Division Game/Debug/Character Spawner Inspector")]
    public static void ShowWindow()
    {
        GetWindow<DebugCharacterSpawnerWindow>("Spawner Debug");
    }
    
    private void OnGUI()
    {
        GUILayout.Label("Character Spawner Debug", EditorStyles.boldLabel);
        
        CharacterSpawner spawner = FindFirstObjectByType<CharacterSpawner>();
        
        if (spawner == null)
        {
            EditorGUILayout.HelpBox("No CharacterSpawner found in scene!", MessageType.Warning);
            return;
        }
        
        SerializedObject so = new SerializedObject(spawner);
        SerializedProperty civilianPrefabsProp = so.FindProperty("civilianPrefabs");
        
        EditorGUILayout.LabelField("Civilian Prefabs Count:", civilianPrefabsProp.arraySize.ToString());
        
        EditorGUILayout.Space(10);
        
        if (civilianPrefabsProp.arraySize > 0)
        {
            EditorGUILayout.LabelField("Civilian Prefabs List:", EditorStyles.boldLabel);
            
            for (int i = 0; i < civilianPrefabsProp.arraySize; i++)
            {
                SerializedProperty element = civilianPrefabsProp.GetArrayElementAtIndex(i);
                GameObject prefab = element.objectReferenceValue as GameObject;
                
                if (prefab != null)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField($"{i}:", GUILayout.Width(30));
                    EditorGUILayout.ObjectField(prefab, typeof(GameObject), false);
                    EditorGUILayout.EndHorizontal();
                }
                else
                {
                    EditorGUILayout.LabelField($"{i}: NULL");
                }
            }
        }
        
        EditorGUILayout.Space(10);
        
        EditorGUILayout.LabelField("Settings:", EditorStyles.boldLabel);
        EditorGUILayout.LabelField($"Max Active: {so.FindProperty("maxActiveCharacters").intValue}");
        EditorGUILayout.LabelField($"Initial Pool Size: {so.FindProperty("initialPoolSize").intValue}");
        EditorGUILayout.LabelField($"Auto Spawn: {so.FindProperty("enableAutoSpawn").boolValue}");
        
        EditorGUILayout.Space(10);
        
        if (Application.isPlaying)
        {
            EditorGUILayout.LabelField("Runtime Info:", EditorStyles.boldLabel);
            EditorGUILayout.LabelField($"Active Characters: {spawner.GetActiveCharacterCount()}");
            EditorGUILayout.LabelField($"Pooled Characters: {spawner.GetPooledCharacterCount()}");
        }
    }
}
