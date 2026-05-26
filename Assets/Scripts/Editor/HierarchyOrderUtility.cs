using UnityEditor;
using UnityEngine;

/// <summary>
/// Adds right-click context menu items to the Hierarchy for reordering GameObjects
/// when Unity's built-in drag-and-drop is broken (Unity 6 EntityId bug).
/// </summary>
public static class HierarchyOrderUtility
{
    private const string MenuBase = "GameObject/Reorder/";

    [MenuItem(MenuBase + "Move to First (Top)", false, 0)]
    private static void MoveToFirst()
    {
        foreach (GameObject go in Selection.gameObjects)
            go.transform.SetAsFirstSibling();
    }

    [MenuItem(MenuBase + "Move to Last (Bottom)", false, 1)]
    private static void MoveToLast()
    {
        foreach (GameObject go in Selection.gameObjects)
            go.transform.SetAsLastSibling();
    }

    [MenuItem(MenuBase + "Move Up One", false, 2)]
    private static void MoveUp()
    {
        foreach (GameObject go in Selection.gameObjects)
        {
            int index = go.transform.GetSiblingIndex();
            if (index > 0)
                go.transform.SetSiblingIndex(index - 1);
        }
    }

    [MenuItem(MenuBase + "Move Down One", false, 3)]
    private static void MoveDown()
    {
        foreach (GameObject go in Selection.gameObjects)
        {
            int index = go.transform.GetSiblingIndex();
            go.transform.SetSiblingIndex(index + 1);
        }
    }

    // ── Validation — disable items when nothing is selected ──────────────────

    [MenuItem(MenuBase + "Move to First (Top)", true)]
    [MenuItem(MenuBase + "Move to Last (Bottom)", true)]
    [MenuItem(MenuBase + "Move Up One", true)]
    [MenuItem(MenuBase + "Move Down One", true)]
    private static bool ValidateSelection() => Selection.gameObjects.Length > 0;
}
