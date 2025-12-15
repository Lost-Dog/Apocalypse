using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(FactionMember))]
public class FactionMemberEditor : Editor
{
    public override void OnInspectorGUI()
    {
        FactionMember factionMember = (FactionMember)target;
        
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Faction Configuration", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("Set the faction for this character. This determines AI behavior and combat relationships.", MessageType.Info);
        
        DrawDefaultInspector();
        
        EditorGUILayout.Space();
        
        if (factionMember.faction == FactionManager.Faction.Player)
        {
            EditorGUILayout.HelpBox("This is a PLAYER faction member. All friendly NPCs and the player should use this.", MessageType.Info);
        }
        else if (factionMember.faction == FactionManager.Faction.Rogue)
        {
            EditorGUILayout.HelpBox("This is a ROGUE faction member. These are hostile enemies.", MessageType.Warning);
        }
        else if (factionMember.faction == FactionManager.Faction.Civilian)
        {
            EditorGUILayout.HelpBox("This is a CIVILIAN faction member. These are neutral NPCs.", MessageType.None);
        }
        else if (factionMember.faction == FactionManager.Faction.Neutral)
        {
            EditorGUILayout.HelpBox("This is a NEUTRAL faction member. No specific allegiance.", MessageType.None);
        }
        
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Quick Faction Setup", EditorStyles.boldLabel);
        
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Set as Player"))
        {
            Undo.RecordObject(factionMember, "Set Faction to Player");
            factionMember.faction = FactionManager.Faction.Player;
            EditorUtility.SetDirty(factionMember);
        }
        if (GUILayout.Button("Set as Rogue"))
        {
            Undo.RecordObject(factionMember, "Set Faction to Rogue");
            factionMember.faction = FactionManager.Faction.Rogue;
            EditorUtility.SetDirty(factionMember);
        }
        if (GUILayout.Button("Set as Civilian"))
        {
            Undo.RecordObject(factionMember, "Set Faction to Civilian");
            factionMember.faction = FactionManager.Faction.Civilian;
            EditorUtility.SetDirty(factionMember);
        }
        EditorGUILayout.EndHorizontal();
    }
}
