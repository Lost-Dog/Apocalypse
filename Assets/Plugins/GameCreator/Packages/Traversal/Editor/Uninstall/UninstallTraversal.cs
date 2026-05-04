using UnityEditor;

namespace GameCreator.Editor.Installs
{
    public static class UninstallTraversal
    {
        [MenuItem(
            itemName: "Game Creator/Uninstall/Traversal",
            isValidateFunction: false,
            priority: UninstallManager.PRIORITY
        )]
        
        private static void Uninstall()
        {
            UninstallManager.Uninstall("Traversal");
        }
    }
}