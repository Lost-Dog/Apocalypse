using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Threepeat
{

    public class SwapModel : MonoBehaviour
    {
        public KeyCode swapKey = KeyCode.Q;
        protected Animator animator;

        public ModelInfo[] modelInfos;

        protected int currModelIndex = 0;

        [System.Serializable]
        public class ModelInfo
        {
            public Avatar avatar;
            public GameObject[] objectsToEnable;
        }

        // Start is called before the first frame update
        void Start()
        {
            animator = GetComponent<Animator>();
        }

        // Update is called once per frame
        void Update()
        {
            if (Input.GetKeyDown(swapKey))
            {
                foreach (GameObject obj in modelInfos[currModelIndex].objectsToEnable)
                {
                    obj.SetActive(false);
                }
                currModelIndex++;
                if (currModelIndex >= modelInfos.Length)
                {
                    currModelIndex = 0;
                }

                animator.avatar = modelInfos[currModelIndex].avatar;
                foreach (GameObject obj in modelInfos[currModelIndex].objectsToEnable)
                {
                    obj.SetActive(true);
                }

            }
        }
    }
}