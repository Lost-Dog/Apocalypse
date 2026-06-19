using MxM;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;

namespace Threepeat
{
    public class Example_ChangeModelOnKey : MonoBehaviour
    {
        private NGCharacter character;

        public int currIndex = 0;
        public GameObject[] models;

        public bool[] isFirstTimeActivating;

        public KeyCode activationKey = KeyCode.M;

        // Start is called before the first frame update
        void Start()
        {
            character = GetComponent<NGCharacter>();
            
            MxMAnimator[] anims = character.GetComponentsInChildren<MxMAnimator>(true);
            List<GameObject> mods = new List<GameObject>();
            List<bool> firstTime = new List<bool>();
            foreach (MxMAnimator anim in anims)
            {
                if ((models == null) || (models.Length == 0))
                {
                    mods.Add(anim.gameObject);
                }
                firstTime.Add(!anim.isActiveAndEnabled);
            }
            if ((models == null) || (models.Length == 0))
            {
                models = mods.ToArray();
            }
            isFirstTimeActivating = firstTime.ToArray();
        }

        // Update is called once per frame
        void Update()
        {
            if (Input.GetKeyDown(activationKey))
            {
                models[currIndex].SetActive(false);
                currIndex++;
                if (currIndex >= models.Length)
                {
                    currIndex = 0;
                }
                models[currIndex].SetActive(true);
                if (!isFirstTimeActivating[currIndex])
                {
                    character.ChangeModel(models[currIndex]);
                }
                else
                {
                    isFirstTimeActivating[currIndex] = false;
                }
            }
        }
    }
}