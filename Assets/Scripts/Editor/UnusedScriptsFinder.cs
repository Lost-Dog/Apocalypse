using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using System.IO;

public class UnusedScriptsFinder : EditorWindow
{
    private Vector2 scrollPosition;
    private List<ScriptInfo> allScripts = new List<ScriptInfo>();
    private List<ScriptInfo> unusedScripts = new List<ScriptInfo>();
    private List<ScriptInfo> testScripts = new List<ScriptInfo>();
    private List<ScriptInfo> editorScripts = new List<ScriptInfo>();
    private bool isAnalyzing = false;
    private string searchStatus = "";
    
    private class ScriptInfo
    {
        public string name;
        public string path;
        public MonoScript script;
        public int usageCount;
        public List<string> usedIn = new List<string>();
        public bool isEditorScript;
        public bool isTestScript;
        public bool isMarkdownDoc;
    }
    
    [MenuItem("Division Game/Tools/Find Unused Scripts")]
    public static void ShowWindow()
    {
        GetWindow<UnusedScriptsFinder>("Unused Scripts Finder");
    }
    
    private void OnGUI()
    {
        EditorGUILayout.LabelField("Unused Scripts Finder", EditorStyles.boldLabel);
        EditorGUILayout.Space(5);
        
        EditorGUILayout.HelpBox(
            "Scans /Assets/Scripts to find:\n" +
            "• Unused runtime scripts\n" +
            "• Test/Debug scripts\n" +
            "• Editor-only scripts\n" +
            "• Temporary fix scripts",
            MessageType.Info);
        
        EditorGUILayout.Space(10);
        
        if (GUILayout.Button("Analyze All Scripts in /Assets/Scripts", GUILayout.Height(40)))
        {
            AnalyzeScripts();
        }
        
        EditorGUILayout.Space(10);
        
        if (!string.IsNullOrEmpty(searchStatus))
        {
            EditorGUILayout.HelpBox(searchStatus, MessageType.None);
        }
        
        if (unusedScripts.Count > 0 || testScripts.Count > 0)
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            
            EditorGUILayout.LabelField($"❌ Unused Runtime Scripts ({unusedScripts.Count})", EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            foreach (var script in unusedScripts)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField($"• {script.name}", GUILayout.Width(300));
                if (GUILayout.Button("Select", GUILayout.Width(60)))
                {
                    Selection.activeObject = script.script;
                    EditorGUIUtility.PingObject(script.script);
                }
                if (GUILayout.Button("Delete", GUILayout.Width(60)))
                {
                    if (EditorUtility.DisplayDialog("Delete Script?",
                        $"Delete {script.name}?\n\nPath: {script.path}\n\nThis cannot be undone!",
                        "Delete", "Cancel"))
                    {
                        AssetDatabase.DeleteAsset(script.path);
                        AnalyzeScripts();
                    }
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.Space(10);
            
            EditorGUILayout.LabelField($"🧪 Test/Debug Scripts ({testScripts.Count})", EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            foreach (var script in testScripts)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField($"• {script.name}", GUILayout.Width(300));
                EditorGUILayout.LabelField($"Used: {script.usageCount}x", GUILayout.Width(80));
                if (GUILayout.Button("Select", GUILayout.Width(60)))
                {
                    Selection.activeObject = script.script;
                    EditorGUIUtility.PingObject(script.script);
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.Space(10);
            
            EditorGUILayout.LabelField($"🔧 Editor Scripts ({editorScripts.Count})", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Editor scripts are OK to keep", EditorStyles.miniLabel);
            
            EditorGUILayout.EndScrollView();
        }
    }
    
    private void AnalyzeScripts()
    {
        isAnalyzing = true;
        searchStatus = "Analyzing...";
        allScripts.Clear();
        unusedScripts.Clear();
        testScripts.Clear();
        editorScripts.Clear();
        
        string[] scriptGuids = AssetDatabase.FindAssets("t:MonoScript", new[] { "Assets/Scripts" });
        
        foreach (string guid in scriptGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            
            if (path.EndsWith(".md") || path.Contains("README"))
                continue;
            
            MonoScript script = AssetDatabase.LoadAssetAtPath<MonoScript>(path);
            if (script == null) continue;
            
            ScriptInfo info = new ScriptInfo
            {
                name = script.name,
                path = path,
                script = script,
                isEditorScript = path.Contains("/Editor/"),
                isMarkdownDoc = path.EndsWith(".md")
            };
            
            info.isTestScript = IsTestOrDebugScript(info.name);
            
            if (!info.isEditorScript && !info.isMarkdownDoc)
            {
                info.usageCount = FindScriptUsage(script, info);
            }
            
            allScripts.Add(info);
        }
        
        unusedScripts = allScripts
            .Where(s => !s.isEditorScript && !s.isMarkdownDoc && !s.isTestScript && s.usageCount == 0)
            .OrderBy(s => s.name)
            .ToList();
        
        testScripts = allScripts
            .Where(s => s.isTestScript && !s.isEditorScript)
            .OrderBy(s => s.name)
            .ToList();
        
        editorScripts = allScripts
            .Where(s => s.isEditorScript)
            .OrderBy(s => s.name)
            .ToList();
        
        searchStatus = $"Analysis complete!\n" +
                      $"Total scripts: {allScripts.Count}\n" +
                      $"Unused: {unusedScripts.Count}\n" +
                      $"Test/Debug: {testScripts.Count}\n" +
                      $"Editor: {editorScripts.Count}";
        
        isAnalyzing = false;
        Repaint();
        
        Debug.Log($"<color=cyan>Script Analysis Complete:</color>\n" +
                 $"• Total: {allScripts.Count}\n" +
                 $"• <color=red>Unused: {unusedScripts.Count}</color>\n" +
                 $"• <color=yellow>Test/Debug: {testScripts.Count}</color>\n" +
                 $"• <color=green>Editor: {editorScripts.Count}</color>");
        
        if (unusedScripts.Count > 0)
        {
            Debug.Log("<color=red>Unused Scripts:</color>");
            foreach (var s in unusedScripts)
            {
                Debug.Log($"  • {s.name} ({s.path})");
            }
        }
    }
    
    private bool IsTestOrDebugScript(string scriptName)
    {
        string lower = scriptName.ToLower();
        return lower.Contains("test") ||
               lower.Contains("debug") ||
               lower.Contains("tester") ||
               lower.Contains("example") ||
               lower.StartsWith("fix");
    }
    
    private int FindScriptUsage(MonoScript script, ScriptInfo info)
    {
        int count = 0;
        System.Type scriptType = script.GetClass();
        
        if (scriptType == null)
            return 0;
        
        string[] sceneGuids = AssetDatabase.FindAssets("t:Scene", new[] { "Assets" });
        foreach (string guid in sceneGuids)
        {
            string scenePath = AssetDatabase.GUIDToAssetPath(guid);
            string sceneContents = File.ReadAllText(scenePath);
            
            if (sceneContents.Contains(script.name))
            {
                count++;
                info.usedIn.Add(scenePath);
            }
        }
        
        string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets" });
        foreach (string guid in prefabGuids)
        {
            string prefabPath = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            
            if (prefab != null)
            {
                Component comp = prefab.GetComponent(scriptType);
                if (comp == null)
                    comp = prefab.GetComponentInChildren(scriptType, true);
                
                if (comp != null)
                {
                    count++;
                    info.usedIn.Add(prefabPath);
                }
            }
        }
        
        return count;
    }
}
