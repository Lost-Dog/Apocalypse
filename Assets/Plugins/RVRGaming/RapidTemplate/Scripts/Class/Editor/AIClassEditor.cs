using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(AIClass))]
public class AIClassEditor : Editor
{
    public override void OnInspectorGUI()
    {
        AIClass aiClass = (AIClass)target;

        aiClass.selectedArchetype = (AIClass.Archetype)EditorGUILayout.EnumPopup("Archetype", aiClass.selectedArchetype);
        aiClass.selectedFaction = (AIClass.Faction)EditorGUILayout.EnumPopup("Faction", aiClass.selectedFaction);

        aiClass.behaviorLibrary = (AIBehaviorLibrary)EditorGUILayout.ObjectField(
            "Behavior Library",
            aiClass.behaviorLibrary,
            typeof(AIBehaviorLibrary),
            false
        );

        if (GUI.changed)
        {
            EditorUtility.SetDirty(target);
        }
    }
}
