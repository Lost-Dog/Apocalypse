using UnityEngine;
using UnityEditor;
using System.IO;

public class FixLootItemReferences : EditorWindow
{
    [MenuItem("Tools/Fix Loot Item Script References")]
    public static void FixAllLootItems()
    {
        string[] guids = AssetDatabase.FindAssets("t:ScriptableObject", new[] { "Assets/Game/Loot/Items" });
        
        int fixedCount = 0;
        int errorCount = 0;
        
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            
            try
            {
                string fileContent = File.ReadAllText(path);
                
                if (fileContent.Contains("m_Script: {fileID: 0}") || fileContent.Contains("m_Script:"))
                {
                    MonoScript scriptAsset = AssetDatabase.LoadAssetAtPath<MonoScript>("Assets/Scripts/LootItemData.cs");
                    
                    if (scriptAsset != null)
                    {
                        string scriptGuid;
                        long scriptFileID;
                        
                        if (AssetDatabase.TryGetGUIDAndLocalFileIdentifier(scriptAsset, out scriptGuid, out scriptFileID))
                        {
                            fileContent = System.Text.RegularExpressions.Regex.Replace(
                                fileContent,
                                @"m_Script: \{fileID: \d+.*?\}",
                                $"m_Script: {{fileID: 11500000, guid: {scriptGuid}, type: 3}}"
                            );
                            
                            File.WriteAllText(path, fileContent);
                            fixedCount++;
                            Debug.Log($"Fixed script reference in: {path}");
                        }
                    }
                    else
                    {
                        Debug.LogError("Could not find LootItemData.cs script!");
                    }
                }
            }
            catch (System.Exception e)
            {
                errorCount++;
                Debug.LogError($"Error fixing {path}: {e.Message}");
            }
        }
        
        AssetDatabase.Refresh();
        
        if (fixedCount > 0)
        {
            Debug.Log($"<color=green>Successfully fixed {fixedCount} loot item(s)!</color>");
        }
        
        if (errorCount > 0)
        {
            Debug.LogWarning($"Failed to fix {errorCount} item(s)");
        }
        
        if (fixedCount == 0 && errorCount == 0)
        {
            Debug.Log("No broken references found!");
        }
    }
}
