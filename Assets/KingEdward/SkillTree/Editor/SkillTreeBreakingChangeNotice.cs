using UnityEditor;
using UnityEngine;

namespace KingEdward.SkillTree.Editor
{

    [InitializeOnLoad]
    public static class SkillTreeBreakingChangeNotice
    {

        private const string EditorPrefsKey = "KingEdward.SkillTree.BreakingChange.Notice.1.1";

        static SkillTreeBreakingChangeNotice()
        {

            if (EditorPrefs.GetBool(EditorPrefsKey, false))
            {
                return;
            }

            EditorPrefs.SetBool(EditorPrefsKey, true);

            EditorApplication.delayCall += ShowIfNeeded;
        }

        private static void ShowIfNeeded()
        {
            const string title = "KingEdward Skill Tree Update (1.1)";

            const string message =
                "Breaking change notice:\n\n" +
                "This update introduced an asmdef for the Skill Tree.\n" +
                "Older Skill assets or visual scripting may show empty in the editor.\n\n" +
                "Manual fix:\n" +
                "1) Close Unity.\n" +
                "2) Open the affected asset files with Notepad (or any text editor).\n" +
                "3) Replace: \"asm: Assembly-CSharp\" -> \"asm: KingEdward.SkillTree\" for Skill Tree types.\n" +
                "4) Re-open Unity.\n\n" ;
                

            const string ok = "OK";
            const string openRoadmap = "Open ROADMAP";

            int _ = EditorUtility.DisplayDialogComplex(title, message, ok, openRoadmap, "Close");


            if (_ == 1)
            {
                const string roadmapPath = "Assets/KingEdward/SkillTree/ROADMAP.md";
                Object roadmap = AssetDatabase.LoadAssetAtPath<Object>(roadmapPath);
                if (roadmap != null)
                {
                    Selection.activeObject = roadmap;
                    EditorGUIUtility.PingObject(roadmap);
                }
            }
        }
    }
}

