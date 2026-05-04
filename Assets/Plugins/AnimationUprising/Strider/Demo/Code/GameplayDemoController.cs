using UnityEngine;
using AnimationUprising.Strider;

namespace StriderDemo
{

    public class GameplayDemoController : MonoBehaviour
    {
        [SerializeField] private Transform m_camera = null;
        [SerializeField] private float m_idleThreshold = 0.3f;
        [SerializeField] private float m_walkThreshold = 0.4f;
        [SerializeField] private float m_runThreshold = 0.8f;
        [SerializeField] private float m_blendRate = 10f;

        private StriderBiped m_strideWarper;
        private Animator m_animator;
        private CharacterController m_charController;

        private Vector2 m_currentSpeed = Vector2.zero;
        private bool m_run = false;
        private bool m_idle = true;
        private float m_idleTimer = -1f;
        private float m_walkTimer = -1f;
        private int m_speedControlMode = 0;

        private float m_verticalSpeed = 0f;

        public float WalkThreshold { get { return m_walkThreshold; } set { m_walkThreshold = value; } }
        public float RunThreshold { get { return m_runThreshold; } set { m_runThreshold = value; } }

        public float SpeedBuff { get; set; }
        public int BuffCount { get; set; }

        // Start is called before the first frame update
        void Start()
        {
            SpeedBuff = 1f;
            BuffCount = 0;

            m_animator = GetComponent<Animator>();
            m_strideWarper = GetComponent<StriderBiped>();
            m_charController = GetComponent<CharacterController>();

            m_animator.SetBool("Idle", true);
        }

        // Update is called once per frame
        void Update()
        {
            Vector2 inputVector = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
            float inputMagnitude = inputVector.magnitude;

            if (m_idle)
            {
                if(inputMagnitude > m_idleThreshold)
                {
                    m_idle = false;
                    m_animator.SetBool("Idle", m_idle);
                    m_strideWarper.EnableSmooth(5f);

                    if (inputMagnitude > m_walkThreshold)
                        m_run = true;
                    else
                        m_run = false;

                    m_animator.SetBool("Run", m_run);
                }
            }
            else
            {
                if (inputMagnitude < m_idleThreshold)
                {
                    if(m_idleTimer < Mathf.Epsilon)
                    {
                        m_idleTimer = Mathf.Epsilon;
                    }
                    else
                    {
                        m_idleTimer += Time.deltaTime;

                        if(m_idleTimer > 0.2f)
                        {
                            m_idle = true;
                            m_animator.SetBool("Idle", m_idle);
                            m_strideWarper.DisableSmooth(5f);
                            
                            m_run = false;
                            m_animator.SetBool("Run", m_run);
                            
                            m_strideWarper.SpeedScale = 1f * SpeedBuff;

                            if(m_speedControlMode == 1)
                                m_animator.speed = 1f * SpeedBuff;

                            m_idleTimer = -1f;
                        }
                    }
                }
                else //Moving (Not Idle)
                {
                    Vector3 faceDirection = Vector3.ProjectOnPlane(m_camera.forward, Vector3.up);
                    transform.rotation = Quaternion.LookRotation(faceDirection, Vector3.up);

                    Vector2 desiredSpeed = inputVector.normalized;

                    if (m_speedControlMode == 3)
                        desiredSpeed = inputVector;

                    m_currentSpeed = Vector2.MoveTowards(m_currentSpeed, desiredSpeed, m_blendRate * Time.deltaTime);
                    m_animator.SetFloat("SpeedX", m_currentSpeed.x);
                    m_animator.SetFloat("SpeedY", m_currentSpeed.y);

                    if (m_run) //running
                    {
                        if (inputMagnitude < m_walkThreshold)
                        {
                            if(m_walkTimer < Mathf.Epsilon)
                                m_walkTimer = Mathf.Epsilon;

                            m_walkTimer += Time.deltaTime;

                            if(m_walkTimer > 0.2f)
                            {
                                m_run = false;
                                m_animator.SetBool("Run", m_run);

                                m_walkTimer = -1f;
                            }
                        }

                        switch(m_speedControlMode)
                        {
                            case 1: { m_animator.speed = (inputMagnitude / m_runThreshold) * SpeedBuff; } break;
                            case 2:
                                {
                                    if (inputMagnitude > m_runThreshold)
                                    {
                                        m_animator.speed = (inputMagnitude / m_runThreshold) * SpeedBuff;
                                    }
                                    else
                                    {
                                        m_animator.speed = 1f * SpeedBuff; 
                                    }
                                }
                                break;
                            case 3:
                                {
                                    if (inputMagnitude > m_runThreshold)
                                    {
                                        m_animator.speed = (inputMagnitude / m_runThreshold) * SpeedBuff;
                                    }
                                    else
                                    {
                                        m_animator.speed = 1f * SpeedBuff;
                                    }
                                }
                                break;
                        }


                        m_strideWarper.SpeedScale = (inputVector.magnitude / m_runThreshold) * SpeedBuff;

                    }
                    else //walking
                    {
                        if(inputMagnitude > m_walkThreshold)
                        {
                            m_run = true;
                            m_animator.SetBool("Run", m_run);
                        }

                        switch (m_speedControlMode)
                        {
                            case 1: { m_animator.speed = (inputMagnitude / (m_runThreshold * m_walkThreshold)) * SpeedBuff; } break;
                            case 2:
                                {
                                    if (inputMagnitude > m_runThreshold)
                                    {
                                        m_animator.speed = (inputMagnitude / m_runThreshold) * SpeedBuff;
                                    }
                                    else
                                    {
                                        m_animator.speed = 1f * SpeedBuff;
                                    }
                                }
                                break;
                            case 3:
                                {
                                    m_animator.speed = 1f * SpeedBuff;
                                }
                                break;
                        }

                        m_strideWarper.SpeedScale = (inputVector.magnitude / (m_runThreshold * m_walkThreshold)) * SpeedBuff;
                    }
                }
            }
        }

        private void OnAnimatorMove()
        {
            if(m_charController.isGrounded)
                m_verticalSpeed = 0f;
            else
                m_verticalSpeed += 9.81f * 2f * Time.deltaTime;

            float gravityDelta = -m_verticalSpeed * Time.deltaTime;

            if (m_speedControlMode == 0)
            {
                m_charController.Move(m_animator.deltaPosition * m_strideWarper.StrideScale + new Vector3(0f, gravityDelta, 0f));
                transform.rotation = m_animator.rootRotation;
            }
            else
            {
                m_charController.Move(m_animator.deltaPosition  + new Vector3(0f, gravityDelta, 0f));
                transform.rotation = m_animator.rootRotation;
            }
        }

        public void SetSpeedControlMode(int a_mode)
        {
            m_speedControlMode = a_mode;

            switch(m_speedControlMode)
            {
                case 0: //Strider
                    {
                        m_strideWarper.enabled = true;

                    } break;
                case 1: //playback speed
                    {
                        m_strideWarper.enabled = false;
                    }
                    break;
                case 2: //Digital
                    {
                        m_animator.speed = 1f * SpeedBuff;
                        m_strideWarper.enabled = false;
                    }
                    break;
                case 3: //Blended
                    {
                        m_animator.speed = 1f * SpeedBuff;
                        m_strideWarper.enabled = false;
                    }
                    break;
            }
        }
    }
}