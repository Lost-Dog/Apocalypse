using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace ThreepeatEditor { 

//[CreateAssetMenu(fileName = "NGConfigWizardGuidMgr", menuName = "Threepeat/Config Wizard Guid Manager")]
    public class MMCConfigWizardGUIDManager : ScriptableObject
    {
        public List<GuidMapping> guids;

        [ContextMenu("Check guids match expected filenames")]
        void CheckGuids()
        {
            Debug.Log("TODO");
        }

        [ContextMenu("Log expected filenames to console and set in object")]
        void LogFilenames()
        {
            foreach (GuidMapping gm in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(gm.guid);
                if (path.Length > 0)
                {
                    Debug.LogFormat("mgr[{0}] = {1}", gm.keyName, path);
                    gm.expectedFilename = Path.GetFileName(path);
                }
                else
                {
                    Debug.LogFormat("mgr[{0}] = {1}", gm.keyName, "Not Found");
                }
            }

        }


        public string GetGuidByKey(string key)
        {
            string ret = null;

            string keyLower = key.ToLower();

            foreach (GuidMapping gm in guids)
            {
                if (gm.keyName.ToLower().Equals(keyLower))
                {
                    return gm.guid;
                }
            }

            return ret;
        }

        [System.Serializable]
        public class GuidMapping
        {
            public string keyName;
            public string guid;
            public string expectedFilename;
        }
    }

}