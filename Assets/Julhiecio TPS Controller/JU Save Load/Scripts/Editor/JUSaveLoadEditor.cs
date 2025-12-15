using JU.SaveLoad;
using UnityEditor;

namespace JUTPS.SaveLoad.Editor
{
    /// <summary>
    /// Used only by the editor that have some utility options to manage saves.
    /// </summary>
    partial class JUSaveLoadEditor
    {
        [MenuItem("Window/JU TPS/Save Load/Delete All Saves")]
        public static void DeleteAllSaves()
        {
            if (EditorUtility.DisplayDialog("Delete all saves?", "Delete ALL game saves for this project?\nThis can't be undone.", "Delete", "Cancel"))
                JUSaveLoad.DeleteAllSaves();
        }
    }
}