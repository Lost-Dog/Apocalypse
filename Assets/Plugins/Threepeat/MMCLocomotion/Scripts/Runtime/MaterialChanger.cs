//using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Threepeat
{
    public class MaterialChanger : MonoBehaviour
    {
        [System.Serializable]
        public struct MaterialPair
        {
            public Material mat1;
            public Material mat2;
        }

        public struct AllTheStuff
        {
            public MaterialPair matPair;
            public GameObject obj;
        }

        public Transform[] targetLocations;

        public Vector3 targetOffset = new Vector3(0, 1.2f, 0);

        public List<MaterialPair> materialPairs = new List<MaterialPair>();

        public List<AllTheStuff> currentOccluders = new List<AllTheStuff>();

        public float lastCheck = 0f;

        public float CHECK_INTERVAL = 1f;

        /*
        [Button("Fire")]
        private void ButtonRunCheck()
        {
            if (targetLocation != null)
            {
                RunOcclusionCheck();
            }
        }*/

        // Start is called before the first frame update
        void Start()
        {
            if ((targetLocations == null) || (targetLocations.Length <= 0))
            {
                Debug.LogError("targetLocation is null in MaterialChanger script.");
            }

        }

        // Update is called once per frame
        void Update()
        {
            if ((targetLocations == null) || (targetLocations.Length <= 0))
            {
                return;
            }
            if (Input.GetKeyDown(KeyCode.P))
            {
                RunOcclusionCheck();
            }

            if ((Time.time - lastCheck) > CHECK_INTERVAL)
            {
                lastCheck = Time.time;
                RunOcclusionCheck();
            }
        }

        public void RunOcclusionCheck()
        {
            List<AllTheStuff> newOccluders = new List<AllTheStuff>();
            foreach (Transform targetLocation in targetLocations)
            {
                Vector3 vecToTarget = (targetLocation.position + targetOffset) - this.transform.position;
                //RaycastHit hit;

                RaycastHit[] hits;

                hits = Physics.RaycastAll(this.transform.position, vecToTarget.normalized, vecToTarget.magnitude);
                //if (Physics.Raycast(this.transform.position, vecToTarget.normalized, out hit, vecToTarget.magnitude))
                foreach (RaycastHit hit in hits)
                {
                    // got a hit
                    GameObject hitObj = hit.transform.gameObject;
                    MeshRenderer rend = hitObj.GetComponent<MeshRenderer>();
                    if (!hitObj.name.Equals(targetLocation.gameObject.name) && (rend != null))
                    {
                        //Debug.LogFormat("got a renderer, material is: {0}", rend.material.name);
                        for (int ii = 0; ii < materialPairs.Count; ii++)
                        {
                            if (rend.material.name.StartsWith(materialPairs[ii].mat1.name))
                            {
                                //Debug.Log("setting material");
                                rend.material = materialPairs[ii].mat2;
                                AllTheStuff ats;
                                ats.matPair = materialPairs[ii];
                                ats.obj = hitObj;
                                newOccluders.Add(ats);
                            }
                        }
                    }
                }
            }

            /*else
            {
                Debug.LogFormat("no hit");
            }*/

            foreach (AllTheStuff ats in currentOccluders)
            {
                if (newOccluders.Contains(ats))
                {
                    continue;
                }
                else
                {
                    MeshRenderer rend = ats.obj.GetComponent<MeshRenderer>();
                    rend.material = ats.matPair.mat1;
                }
            }

            currentOccluders = newOccluders;
        }
    }
}