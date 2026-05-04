using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Factions))]
public class FactionsEditor : Editor
{
    private bool showFactionLists = false; // Toggle for showing/hiding the lists

    public override void OnInspectorGUI()
    {
        Factions factions = (Factions)target;

        // Draw everything except the "m_Script" and "factions" fields
        serializedObject.Update();
        SerializedProperty property = serializedObject.GetIterator();
        property.NextVisible(true); // Skip "m_Script"

        while (property.NextVisible(false))
        {
            if (property.name == "factions") continue; // Skip the "factions" field
            EditorGUILayout.PropertyField(property, true);
        }

        EditorGUILayout.Space();

        // Add a button to toggle faction list visibility
        if (GUILayout.Button(showFactionLists ? "Hide Faction Lists" : "Show Faction Lists"))
        {
            showFactionLists = !showFactionLists;
        }

        // If toggle is enabled, display the faction lists
        if (showFactionLists)
        {
            EditorGUILayout.LabelField("Faction Lists", EditorStyles.boldLabel);

            foreach (var factionGroup in factions.factions)
            {
                EditorGUILayout.LabelField($"Faction: {factionGroup.faction}", EditorStyles.boldLabel);
                EditorGUI.indentLevel++;
                foreach (var member in factionGroup.members)
                {
                    EditorGUILayout.ObjectField(member, typeof(GameObject), true);
                }
                EditorGUI.indentLevel--;
                EditorGUILayout.Space();
            }
        }

        serializedObject.ApplyModifiedProperties();
    }
}
