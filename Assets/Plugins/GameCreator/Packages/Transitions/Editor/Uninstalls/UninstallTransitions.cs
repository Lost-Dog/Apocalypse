using GameCreator.Editor.Installs;
using UnityEditor;

namespace GameCreator.Editor.Transitions
{
    public static class UninstallTransitions
    {
        [MenuItem(
            itemName: "Game Creator/Uninstall/Transitions",
            isValidateFunction: false,
            priority: UninstallManager.PRIORITY
        )]
        
        private static void Uninstall()
        {
            UninstallManager.Uninstall("Transitions");
        }
    }
}