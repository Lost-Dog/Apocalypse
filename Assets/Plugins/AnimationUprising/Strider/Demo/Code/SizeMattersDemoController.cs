using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AnimationUprising.Strider;
using Unity.Cinemachine;

namespace StriderDemo
{

    public class SizeMattersDemoController : MonoBehaviour
    {
        [SerializeField]
        private GameObject m_characterPrefab = null;

        [SerializeField]
        private CinemachineCamera m_freeLookCM = null;

        [SerializeField]
        private float m_minScale = 0.4f;

        [SerializeField]
        private float m_maxScale = 2.3f;

        private List<StriderBiped> m_strideWarpers = null;

        public void Start()
        {
            m_strideWarpers = new List<StriderBiped>(7);
        }

        public void ReSpawn()
        {
            Clear();

            Vector3 position = new Vector3(-2f, 0f, -2f);

            for(int rows = 0; rows < 3; ++rows)
            {
                for(int cols = 0; cols < 3; ++cols)
                {
                    float scale = Random.Range(m_minScale, m_maxScale);

                    GameObject newChar = GameObject.Instantiate(m_characterPrefab);
                    StriderBiped newSWB = newChar.GetComponent<StriderBiped>();

                    newChar.transform.localScale = new Vector3(scale, scale, scale);
                    newSWB.SizeCompensation = scale;
                    newChar.transform.position = position + new Vector3(rows * 2f, 0f, cols * 2f);

                    m_strideWarpers.Add(newSWB);
                }
            }

            Transform lookTransform = m_strideWarpers[4].transform;

            m_freeLookCM.LookAt = lookTransform;
            m_freeLookCM.Follow = lookTransform;
        }

        public void Clear()
        {
            foreach(StriderBiped swb in m_strideWarpers)
            {
                Destroy(swb.gameObject);
            }

            m_strideWarpers.Clear();

            m_freeLookCM.LookAt = null;
            m_freeLookCM.Follow = null;
        }

        public void SetCompensationMode(int a_mode)
        {
            switch(a_mode)
            {
                case 0: //Strider
                    {
                        foreach(StriderBiped swb in m_strideWarpers)
                        {
                            swb.enabled = true;
                            swb.GetComponent<Animator>().speed = 1f;
                        }
                    }
                    break;
                case 1: //Playback Speed
                    {
                        foreach (StriderBiped swb in m_strideWarpers)
                        {
                            swb.enabled = false;
                            swb.GetComponent<Animator>().speed = 1f / swb.SizeCompensation;
                            swb.GetComponent<SizeMattersRootMotion>().enabled = true;
                        }
                    }
                    break;
                case 2: //None
                    {
                        foreach (StriderBiped swb in m_strideWarpers)
                        {
                            swb.enabled = false;
                            swb.GetComponent<Animator>().speed = 1f;
                            swb.GetComponent<SizeMattersRootMotion>().enabled = true;
                        }
                    }
                    break;
            }
        }
    }
}