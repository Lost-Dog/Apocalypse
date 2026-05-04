using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AnimationUprising;

namespace StriderDemo
{
    public class SpeedPickup : MonoBehaviour
    {
        [SerializeField]
        private float m_speedMultiplier = 1.5f;

        [SerializeField]
        private float m_respawnTime = 5f;

        private float m_respawnTimer = -1f;
        private MeshRenderer m_mesh;
        private SphereCollider m_collider;
        private GameplayDemoController m_gameplayController;

        private void Start()
        {
            m_collider = GetComponent<SphereCollider>();
            m_mesh = GetComponentInChildren<MeshRenderer>();
        }


        public void Update()
        {
            if(m_respawnTimer > -1f)
            {
                m_respawnTimer += Time.deltaTime;

                if(m_respawnTimer >= m_respawnTime)
                {
                    m_mesh.gameObject.SetActive(true);
                    m_collider.enabled = true;

                    if (--m_gameplayController.BuffCount == 0)
                    {
                        m_gameplayController.SpeedBuff = 1f;
                    }

                    m_respawnTimer = -1f;
                }
            }
        }

        private void OnTriggerEnter(Collider a_other)
        {
            GameplayDemoController controller = a_other.GetComponent<GameplayDemoController>();

            if (controller != null)
            {
                m_gameplayController = controller;

                m_gameplayController.SpeedBuff = m_speedMultiplier;
                m_gameplayController.BuffCount++;

                m_collider.enabled = false;
                m_mesh.gameObject.SetActive(false);

                m_respawnTimer = 0f;
            }
        }
    }
}