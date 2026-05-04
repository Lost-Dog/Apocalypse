using System.Collections;
using UnityEngine;
using AnimationUprising.Strider;
using UnityEngine.UI;

public class MasterSpeedChangeTest : MonoBehaviour
{
    [SerializeField]
    [Range(0f, 3f)]
    private float m_totalSpeed = 1f;

    [SerializeField]
    private StriderBiped m_strideWarperBiped = null;

    [SerializeField]
    private PlaybackSpeedTest m_playbackSpeedTest = null;

    [SerializeField] private Slider m_speedSlider = null;
    [SerializeField] private InputField m_speedInput = null;
    [SerializeField] private Slider m_minPlaybackSpeedSlider = null;
    [SerializeField] private InputField m_minPlaybackSpeedInput = null;
    [SerializeField] private Slider m_maxPlaybackSpeedSlider = null;
    [SerializeField] private InputField m_maxPlaybackSpeedInput = null;
    [SerializeField] private Slider m_speedStrideBlendSlider = null;
    [SerializeField] private InputField m_speedStrideBlendInput = null;
    [SerializeField] private Slider m_hipAdjustCutoffSlider = null;
    [SerializeField] private InputField m_hipAdjustCutoffInput = null;
    [SerializeField] private Slider m_hipDampingSlider = null;
    [SerializeField] private InputField m_hipDampingInput = null;
    [SerializeField] private Slider m_strideOffsetSlider = null;
    [SerializeField] private InputField m_strideOffsetInput = null;
    [SerializeField] private Slider m_dynamicOffsetSlider = null;
    [SerializeField] private InputField m_dynamicOffsetInput = null;

    private Animator animatorA;
    private Animator animatorB;

    private Vector2 m_desiredDirection = new Vector2(0f, 1f);
    private Vector2 m_direction = new Vector2(0f, 1f);

    private Coroutine m_changeDirectionCoroutine;

    private void Start()
    {
        animatorA = m_strideWarperBiped.GetComponent<Animator>();
        animatorB = m_playbackSpeedTest.GetComponent<Animator>();

        m_speedSlider.value = m_strideWarperBiped.SpeedScale;

        m_speedStrideBlendSlider.value = m_strideWarperBiped.PlaybackSpeedWeight;
        m_speedStrideBlendInput.text = m_strideWarperBiped.PlaybackSpeedWeight.ToString("F2");

        m_maxPlaybackSpeedSlider.value = m_strideWarperBiped.MaxPlaybackSpeed;
        m_maxPlaybackSpeedInput.text = m_strideWarperBiped.MaxPlaybackSpeed.ToString("F2");

        m_minPlaybackSpeedSlider.value = m_strideWarperBiped.MinPlaybackSpeed;
        m_minPlaybackSpeedInput.text = m_strideWarperBiped.MinPlaybackSpeed.ToString("F2");

        m_hipAdjustCutoffSlider.value = m_strideWarperBiped.HipAdjustCutoff;
        m_hipAdjustCutoffInput.text = m_strideWarperBiped.HipAdjustCutoff.ToString("F2");

        m_hipDampingSlider.value = m_strideWarperBiped.HipDamping;
        m_hipDampingInput.text = m_strideWarperBiped.HipDamping.ToString("F2");

        m_dynamicOffsetSlider.value = m_strideWarperBiped.DynamicOffset;
        m_dynamicOffsetInput.text = m_strideWarperBiped.DynamicOffset.ToString("F2");

        m_strideOffsetSlider.value = m_strideWarperBiped.BaseOffset;
        m_strideOffsetInput.text = m_strideWarperBiped.BaseOffset.ToString("F2");
    }

    // Update is called once per frame
    public void Update()
    {
        m_totalSpeed = m_speedSlider.value;
        m_speedInput.text = m_totalSpeed.ToString();

        m_strideWarperBiped.SpeedScale = m_totalSpeed;
        m_playbackSpeedTest.PlaybackSpeed = m_totalSpeed;
    }

    public void UpdateSpeedDirect()
    {
        m_totalSpeed = float.Parse(m_speedInput.text);
        m_speedSlider.value = m_totalSpeed;

        m_strideWarperBiped.SpeedScale = m_totalSpeed;
        m_playbackSpeedTest.PlaybackSpeed = m_totalSpeed;
    }

    public void SetRun(bool a_run)
    {
        animatorA.SetBool("Run", a_run);
        animatorB.SetBool("Run", a_run);
    }

    public void SetStrideOffset()
    {
        m_strideWarperBiped.BaseOffset = m_strideOffsetSlider.value;
        m_strideOffsetInput.text = m_strideOffsetSlider.value.ToString("F2");
    }

    public void SetStrideOffsetDirect()
    {
        float strideOffset = float.Parse(m_strideOffsetInput.text);

        m_strideWarperBiped.BaseOffset = strideOffset;
        m_strideOffsetSlider.value = strideOffset;
    }
    
    public void SetDynamicOffset()
    {
        m_strideWarperBiped.DynamicOffset = m_dynamicOffsetSlider.value;
        m_dynamicOffsetInput.text = m_dynamicOffsetSlider.value.ToString("F2");
    }

    public void SetDynamicOffsetDirect()
    {
        float dynamicOffset = float.Parse(m_dynamicOffsetInput.text);

        m_strideWarperBiped.DynamicOffset = dynamicOffset;
        m_dynamicOffsetSlider.value = dynamicOffset;
    }

    public void SetHipDamping()
    {
        m_strideWarperBiped.HipDamping = m_hipDampingSlider.value;
        m_hipDampingInput.text = m_hipDampingSlider.value.ToString("F2");
    }

    public void SetHipDampingDirect()
    {
        float hipDamping = float.Parse(m_hipDampingInput.text);

        m_strideWarperBiped.HipDamping = hipDamping;
        m_hipDampingSlider.value = hipDamping;
    }

    public void SetHipAdjustCutoff()
    {
        m_strideWarperBiped.HipAdjustCutoff = m_hipAdjustCutoffSlider.value;
        m_hipAdjustCutoffInput.text = m_hipAdjustCutoffSlider.value.ToString("F2");
    }

    public void SetHipAdjustCuttofDirect()
    {
        float hipAdjustCutoff = float.Parse(m_hipAdjustCutoffInput.text);

        m_strideWarperBiped.HipAdjustCutoff = hipAdjustCutoff;
        m_hipAdjustCutoffSlider.value = hipAdjustCutoff;
    }

    public void SetSpeedStrideBlendRatio()
    {
        m_speedStrideBlendInput.text = m_speedStrideBlendSlider.value.ToString("F2");
        m_strideWarperBiped.PlaybackSpeedWeight = m_speedStrideBlendSlider.value;
    }

    public void SetSpeedStrideBlendRatioDirect()
    {
        m_speedStrideBlendSlider.value = float.Parse(m_speedStrideBlendInput.text);
        m_strideWarperBiped.PlaybackSpeedWeight = m_speedStrideBlendSlider.value;
    }

    public void SetMinPlaybackSpeedSlider()
    {
        m_strideWarperBiped.MinPlaybackSpeed = m_minPlaybackSpeedSlider.value;
        m_minPlaybackSpeedInput.text = m_minPlaybackSpeedSlider.value.ToString("F2");
    }

    public void SetMaxPlaybackSpeedSlider()
    {
        m_strideWarperBiped.MaxPlaybackSpeed = m_maxPlaybackSpeedSlider.value;
        m_maxPlaybackSpeedInput.text = m_maxPlaybackSpeedSlider.value.ToString("F2");
    }

    public void SetMinPlaybackSpeedDirect()
    {
        float minSpeed = float.Parse(m_minPlaybackSpeedInput.text);

        m_strideWarperBiped.MinPlaybackSpeed = minSpeed;
        m_minPlaybackSpeedSlider.value = minSpeed;
    }

    public void SetMaxPlaybackSpeedDirect()
    {
        float maxSpeed = float.Parse(m_maxPlaybackSpeedInput.text);

        m_strideWarperBiped.MaxPlaybackSpeed = maxSpeed;
        m_maxPlaybackSpeedSlider.value = maxSpeed;
    }

    public void SetDirection(int a_direction)
    {
        switch(a_direction)
        {
            case 0: { SetDirection(new Vector2(0f, 1f)); } break;
            case 1: { SetDirection(new Vector2(1f, 1f)); } break;
            case 2: { SetDirection(new Vector2(1f, 0f)); } break;
            case 3: { SetDirection(new Vector2(1f, -1f)); } break;
            case 4: { SetDirection(new Vector2(0f, -1f)); } break;
            case 5: { SetDirection(new Vector2(-1f, -1f)); } break;
            case 6: { SetDirection(new Vector2(-1f, 0f)); } break;
            case 7: { SetDirection(new Vector2(-1f, 1f)); } break;
        }
    }

    public void SetDirection(Vector2 a_direction)
    {
        m_desiredDirection = a_direction;

        if (m_changeDirectionCoroutine != null)
            StopCoroutine(m_changeDirectionCoroutine);

        m_changeDirectionCoroutine = StartCoroutine(ChangeDirection());
    }

    public void ResetDefaults()
    {
        SetDirection(0);
        SetRun(true);
        m_speedStrideBlendSlider.value = 0.5f;
        m_speedStrideBlendInput.text = "0.5";
        m_speedSlider.value = 1.5f;
        m_speedInput.text = "1.5";
        m_minPlaybackSpeedSlider.value = 0.9f;
        m_minPlaybackSpeedInput.text = "0.9";
        m_maxPlaybackSpeedSlider.value = 1.1f;
        m_maxPlaybackSpeedInput.text = "1.1";

        m_hipDampingSlider.value = 1f;
        m_hipDampingInput.text = "1.0f";
        m_hipAdjustCutoffSlider.value = 0.25f;
        m_hipAdjustCutoffInput.text = "0.25";

        m_strideOffsetSlider.value = 0f;
        m_strideOffsetInput.text = "0";
        m_dynamicOffsetSlider.value = -0.2f;
        m_dynamicOffsetInput.text = "-0.2";
    }

    public void ToggleEnabled()
    {
        if(m_strideWarperBiped.enabled)
        {
            m_strideWarperBiped.DisableSmooth(1f);
        }
        else
        {
            m_strideWarperBiped.EnableSmooth(1f);
        }
    }

    private IEnumerator ChangeDirection()
    {
        while(true)
        {
            m_direction = Vector2.MoveTowards(m_direction, m_desiredDirection, 6 * Time.deltaTime);
            animatorA.SetFloat("SpeedX", m_direction.x);
            animatorA.SetFloat("SpeedY", m_direction.y);
            animatorB.SetFloat("SpeedX", m_direction.x);
            animatorB.SetFloat("SpeedY", m_direction.y);

            if(Vector2.Distance(m_direction, m_desiredDirection) < 0.0001f)
            {
                break;
            }

            yield return null;
        }
    }
}
