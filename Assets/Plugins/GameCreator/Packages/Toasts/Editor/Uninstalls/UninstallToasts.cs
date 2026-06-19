using GameCreator.Editor.Installs;
using UnityEditor;

namespace GameCreator.Editor.Toasts
{
    public static class UninstallToasts
    {
        [MenuItem(
            itemName: "Game Creator/Uninstall/Toasts",
            isValidateFunction: false,
            priority: UninstallManager.PRIORITY
        )]
        
        private static void Uninstall()
        {
            UninstallManager.Uninstall("Toasts");
        }
    }
}