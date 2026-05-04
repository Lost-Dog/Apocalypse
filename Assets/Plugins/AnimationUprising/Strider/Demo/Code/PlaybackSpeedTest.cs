using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaybackSpeedTest : MonoBehaviour
{
    [SerializeField]
    [Range(0f, 3f)]
    private float m_playbackSpeed = 1f;

    private Animator m_animator;

    public float PlaybackSpeed { get { return m_playbackSpeed; } set { m_playbackSpeed = value; } }

    public void Awake()
    {
        m_animator = GetComponent<Animator>();
    }

    public void Update()
    {
        m_animator.speed = m_playbackSpeed;
    }
}
