using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Animations;
using MxM;
using AnimationUprising.Strider;
using Unity.Mathematics;

public class StriderBipedMxM : StriderBiped, ILongitudinalWarper
{
    [Header("MxM")]
    [SerializeField] private float m_minStrideScale = 0.5f;
    [SerializeField] private float m_maxStrideScale = 1.0f;

    MxMAnimator m_mxmAnimator;

    protected override void Start()
    {
        m_mxmAnimator = GetComponentInChildren<MxMAnimator>();
        base.Start(); 
    }

    protected override void InitializePlayableGraph()
    {
        p_playableGraph = m_mxmAnimator.MxMPlayableGraph;

        if (!p_playableGraph.IsValid())
            p_playableGraph = PlayableGraph.Create(gameObject.name + "StriderGraph_");

        p_playableGraph.SetTimeUpdateMode(DirectorUpdateMode.GameTime);
        p_playableOutput = AnimationPlayableOutput.Create(p_playableGraph, "StrideWarpOutput", p_animator);
    }

    public void ApplySpeedScale(float a_speedScale)
    {
        SpeedScale = Mathf.Clamp(a_speedScale, m_minStrideScale, m_maxStrideScale);
    }

    protected override void UpdateParameters1()
    {
        UpdateSpeedScale(Time.deltaTime);
        float playbackSpeed = math.clamp((p_speedScale - 1f) * p_playbackSpeedWeight + 1f, p_minMaxPlaybackSpeed.x, p_minMaxPlaybackSpeed.y);
        float totalStrideScale = p_speedScale / (playbackSpeed * p_sizeCompensation);
        StrideScale = 1f + ((totalStrideScale - 1f) * p_weight);

        m_mxmAnimator.PlaybackSpeed = (1f + ((playbackSpeed - 1f) * p_weight)) * p_independentPlaybackSpeed;
    }

    public float RootMotionScale()
    {
        return StrideScale;
    }
}
