using System.Collections.Generic;
using UnityEngine;

namespace CompassNavigatorPro {

    public partial class ScanEffect : MonoBehaviour {

        private const string POOL_CONTAINER_NAME = "Scan_Pool";
        
        static List<ScanEffect> pool = new List<ScanEffect>();
        static GameObject prefab;
        static GameObject rootContainer;

        private static GameObject GetRootContainer() {
            if (rootContainer == null) {
                rootContainer = GameObject.Find(POOL_CONTAINER_NAME);
                if (rootContainer == null) {
                    rootContainer = new GameObject(POOL_CONTAINER_NAME);
                    DontDestroyOnLoad(rootContainer);
                }
            }
            return rootContainer;
        }

        public static ScanEffect GetInstanceFromPool() {
            ScanEffect sonar;
            int count = pool.Count;
            for (int k = 0; k < count; k++) {
                sonar = pool[k];
                if (sonar != null && !sonar.isActiveAndEnabled) {
                    sonar.Reset();
                    return sonar;
                }
            }
            if (prefab == null) {
                prefab = Resources.Load<GameObject>("CNPro/Prefabs/ScanSphere");
                if (prefab == null) {
                    Debug.LogError("ScanSphere prefab not found!");
                    return null;
                }
            }
            GameObject instance = Instantiate(prefab);
            instance.name = "Sonar FX";
            instance.transform.SetParent(GetRootContainer().transform);
            sonar = instance.GetComponent<ScanEffect>();
            pool.Add(sonar);
            return sonar;
        }

        public static void RemoveInstanceFromPool(ScanEffect sonar) {
            if (pool.Contains(sonar)) pool.Remove(sonar);
        }
      
    }

}