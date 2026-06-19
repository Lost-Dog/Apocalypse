using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Windows;

namespace ThreepeatEditor
{
#if ENABLE_THREEPEAT_DEV_MODE
    [CreateAssetMenu(fileName = "PackageExporterConfig", menuName = "Threepeat/PackageExporterConfig")]
#endif

    public class MMCPackageExporter : ScriptableObject
    {
        public string outputFullPath = "Assets/Plugins/Threepeat/MMCLocomotion/Integrations/PackageName.unityPackage";

        public string[] assets;

        [ContextMenu("Add selection to asset list")]
        public void PopulateThisObjectWithSelection()
        {
            List<string> newAssets = new List<string>();
            if (assets != null)
            {
                newAssets = new List<string>(assets);
            }
            foreach (UnityEngine.Object obj in Selection.objects)
            {
                var assetPath = AssetDatabase.GetAssetPath(obj);

                newAssets.Add(assetPath);
            }
            assets = newAssets.ToArray();
            AssetDatabase.SaveAssets();
        }

        [ContextMenu("Export Package")]
        public void ExportPackage()
        {
            AssetDatabase.ExportPackage(assets, outputFullPath);
        }

        [ContextMenu("Delete package files")]
        public void DeletePackageFiles()
        {
            if (EditorUtility.DisplayDialog("Are you sure?", "This will delete all the files in the assets array.  Be sure!", "I fear nothing.", "You're right, that was a dumb idea"))
            {
                foreach (string fname in assets)
                {
                    Debug.LogFormat("deleting {0}", fname);
                    File.Delete(fname);
                }

                AssetDatabase.Refresh();
            }
        }

    }
}