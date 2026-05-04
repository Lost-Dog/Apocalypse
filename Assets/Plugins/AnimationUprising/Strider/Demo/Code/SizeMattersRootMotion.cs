using UnityEngine;

namespace StriderDemo
{
    public class SizeMattersRootMotion : MonoBehaviour
    {
        private Animator m_animator;

        public void Awake()
        {
            m_animator = GetComponent<Animator>();
        }

        public void Update()
        {
            
        }

        private void OnAnimatorMove()
        {
            transform.SetPositionAndRotation(m_animator.rootPosition, m_animator.rootRotation);
        }


    }
}
