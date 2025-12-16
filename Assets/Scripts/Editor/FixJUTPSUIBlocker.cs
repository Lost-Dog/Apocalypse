using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using System.Collections.Generic;

public class FixJUTPSUIBlocker : EditorWindow
{
    [MenuItem("Tools/Fix JUTPS UI Blocker")]
    public static void ShowWindow()
    {
        GetWindow<FixJUTPSUIBlocker>("Fix JUTPS UI");
    }

    private void OnGUI()
    {
        EditorGUILayout.HelpBox(
            "This tool finds and fixes UI elements in the JUTPS UI that are blocking interactions.\n\n" +
            "It will:\n" +
            "• Find invisible or non-interactive UI Images with raycastTarget enabled\n" +
            "• Disable raycastTarget on blocking elements\n" +
            "• Report all changes made",
            MessageType.Info);

        EditorGUILayout.Space(10);

        if (GUILayout.Button("Scan and Fix JUTPS UI", GUILayout.Height(40)))
        {
            FixBlockingUIElements();
        }
    }

    private void FixBlockingUIElements()
    {
        // Find the JUTPS UI
        GameObject jutpsUI = GameObject.Find("JUTPS Default User Interface");
        if (jutpsUI == null)
        {
            EditorUtility.DisplayDialog("Not Found", "Could not find 'JUTPS Default User Interface' in the scene.", "OK");
            return;
        }

        int fixedCount = 0;
        List<string> fixedObjects = new List<string>();

        // Get all Image components in the JUTPS UI hierarchy
        Image[] allImages = jutpsUI.GetComponentsInChildren<Image>(true);

        foreach (Image img in allImages)
        {
            // Check if this image is blocking but shouldn't be
            if (img.raycastTarget)
            {
                // Check if it's invisible or has no sprite
                bool isInvisible = img.color.a < 0.01f;
                bool hasNoSprite = img.sprite == null;
                bool isFullScreen = IsFullScreenImage(img);

                // If it's a full-screen invisible image OR has no sprite but blocks raycasts
                if ((isFullScreen && isInvisible) || hasNoSprite)
                {
                    Undo.RecordObject(img, "Fix JUTPS UI Blocker");
                    img.raycastTarget = false;
                    EditorUtility.SetDirty(img);
                    
                    fixedCount++;
                    string reason = isFullScreen && isInvisible ? "invisible fullscreen" : "no sprite";
                    fixedObjects.Add($"{GetGameObjectPath(img.gameObject)} ({reason})");
                }
            }
        }

        // Also check for any CanvasGroup with blocksRaycasts = true on invisible elements
        CanvasGroup[] allCanvasGroups = jutpsUI.GetComponentsInChildren<CanvasGroup>(true);
        foreach (CanvasGroup cg in allCanvasGroups)
        {
            if (cg.blocksRaycasts && cg.alpha < 0.01f && !cg.gameObject.activeSelf)
            {
                Undo.RecordObject(cg, "Fix JUTPS CanvasGroup Blocker");
                cg.blocksRaycasts = false;
                EditorUtility.SetDirty(cg);
                
                fixedCount++;
                fixedObjects.Add($"{GetGameObjectPath(cg.gameObject)} (invisible CanvasGroup)");
            }
        }

        if (fixedCount > 0)
        {
            string message = $"Fixed {fixedCount} blocking UI element(s):\n\n";
            foreach (string obj in fixedObjects)
            {
                message += $"• {obj}\n";
            }
            
            Debug.Log($"<color=green>✓ Fixed {fixedCount} JUTPS UI blocking elements</color>");
            EditorUtility.DisplayDialog("Success", message, "OK");
        }
        else
        {
            EditorUtility.DisplayDialog("Complete", "No blocking UI elements found!", "OK");
        }
    }

    private bool IsFullScreenImage(Image img)
    {
        RectTransform rt = img.GetComponent<RectTransform>();
        if (rt == null) return false;

        // Check if anchors cover the full screen
        return rt.anchorMin == Vector2.zero && rt.anchorMax == Vector2.one;
    }

    private string GetGameObjectPath(GameObject obj)
    {
        string path = obj.name;
        Transform current = obj.transform.parent;
        
        while (current != null)
        {
            path = current.name + "/" + path;
            current = current.parent;
        }
        
        return path;
    }
}
