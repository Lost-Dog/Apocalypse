using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Diagnostic tool to check dial configurations in the scene
/// </summary>
public class DialDiagnostic : EditorWindow
{
    [MenuItem("Tools/Dial Diagnostic")]
    public static void ShowWindow()
    {
        GetWindow<DialDiagnostic>("Dial Diagnostic");
    }

    private Vector2 scrollPosition;

    private void OnGUI()
    {
        GUILayout.Label("Dial Configuration Diagnostic", EditorStyles.boldLabel);
        GUILayout.Space(10);

        if (GUILayout.Button("Scan Scene for Dials", GUILayout.Height(30)))
        {
            ScanForDials();
        }

        GUILayout.Space(10);
        scrollPosition = GUILayout.BeginScrollView(scrollPosition);
        // Results will be displayed here via Debug.Log
        GUILayout.EndScrollView();
    }

    private void ScanForDials()
    {
        Debug.Log("=== DIAL DIAGNOSTIC SCAN ===");
        Debug.Log("");

        // Find all GameObjects with "Dial" in their name
        GameObject[] allObjects = GameObject.FindObjectsOfType<GameObject>();
        int dialCount = 0;

        foreach (GameObject obj in allObjects)
        {
            if (obj.name.Contains("Dial") || obj.name.Contains("dial"))
            {
                dialCount++;
                Debug.Log($"[{dialCount}] Found: {GetFullPath(obj.transform)}");
                
                // Check for TemperatureDial component
                TemperatureDial tempDial = obj.GetComponent<TemperatureDial>();
                if (tempDial != null)
                {
                    Debug.Log($"    ✓ Has TemperatureDial component");
                    Debug.Log($"      - SurvivalManager: {(tempDial.survivalManager != null ? "✓ Assigned" : "✗ MISSING")}");
                    Debug.Log($"      - Dial Fill Image: {(tempDial.dialFillImage != null ? "✓ Assigned" : "✗ MISSING")}");
                    if (tempDial.dialFillImage != null)
                    {
                        Debug.Log($"        Type: {tempDial.dialFillImage.type}");
                        if (tempDial.dialFillImage.type == Image.Type.Filled)
                        {
                            Debug.Log($"        Fill Method: {tempDial.dialFillImage.fillMethod}");
                            Debug.Log($"        Fill Amount: {tempDial.dialFillImage.fillAmount}");
                        }
                    }
                    Debug.Log($"      - Temperature Text: {(tempDial.temperatureText != null ? "✓ Assigned" : "○ Optional (not set)")}");
                    Debug.Log($"      - Auto Find: {tempDial.autoFindComponents}");
                }
                else
                {
                    Debug.Log($"    ○ No TemperatureDial component");
                }

                // Check for StaminaDial
                StaminaDial staminaDial = obj.GetComponent<StaminaDial>();
                if (staminaDial != null)
                {
                    Debug.Log($"    ✓ Has StaminaDial component");
                }

                // Check for InfectionDial
                InfectionDial infectionDial = obj.GetComponent<InfectionDial>();
                if (infectionDial != null)
                {
                    Debug.Log($"    ✓ Has InfectionDial component");
                }

                // Check for Image components
                Image[] images = obj.GetComponentsInChildren<Image>();
                if (images.Length > 0)
                {
                    Debug.Log($"    Images found: {images.Length}");
                    foreach (Image img in images)
                    {
                        Debug.Log($"      - {img.gameObject.name}: Type={img.type}");
                    }
                }

                Debug.Log("");
            }
        }

        // Check for SurvivalManager
        Debug.Log("=== SURVIVAL MANAGER CHECK ===");
        SurvivalManager survivalManager = FindObjectOfType<SurvivalManager>();
        if (survivalManager != null)
        {
            Debug.Log($"✓ SurvivalManager found on: {survivalManager.gameObject.name}");
            Debug.Log($"  Current Temperature: {survivalManager.currentTemperature}");
            Debug.Log($"  Max Temperature: {survivalManager.maxTemperature}");
        }
        else
        {
            Debug.LogError("✗ SurvivalManager NOT FOUND in scene!");
        }

        Debug.Log("");
        Debug.Log($"=== SCAN COMPLETE: Found {dialCount} dial objects ===");
    }

    private string GetFullPath(Transform transform)
    {
        string path = transform.name;
        while (transform.parent != null)
        {
            transform = transform.parent;
            path = transform.name + "/" + path;
        }
        return path;
    }
}
