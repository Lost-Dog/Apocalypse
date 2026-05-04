using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using AnimationUprising;

namespace StriderDemo
{
    public class GameplayDemoUI : MonoBehaviour
    {
        public GameplayDemoController GameplayController;

        public Slider m_walkThresholdSlider;
        public Text m_walkThresholdText;
        public Slider m_runThresholdSlider;
        public Text m_runThresholdText;

        // Start is called before the first frame update
        void Start()
        {
            m_runThresholdSlider.value = GameplayController.RunThreshold;
            m_runThresholdText.text = m_runThresholdSlider.value.ToString("F2");

            m_walkThresholdSlider.value = GameplayController.WalkThreshold;
            m_walkThresholdText.text = m_walkThresholdSlider.value.ToString("F2");
        }

        public void SetRunThreshold()
        {
            m_runThresholdText.text = m_runThresholdSlider.value.ToString("F2");
            GameplayController.RunThreshold = m_runThresholdSlider.value;
        }

        public void SetWalkThreshold()
        {
            m_walkThresholdText.text = m_walkThresholdSlider.value.ToString("F2");
            GameplayController.WalkThreshold = m_walkThresholdSlider.value;
        }

        public void SetDefault()
        {
            m_runThresholdText.text = "0.7";
            m_runThresholdSlider.value = 0.7f;
            GameplayController.RunThreshold = 0.7f;

            m_walkThresholdText.text = "0.5";
            m_walkThresholdSlider.value = 0.5f;
            GameplayController.WalkThreshold = 0.5f;
        }

        public void SetWalkOnly()
        {
            m_runThresholdText.text = "0.6";
            m_runThresholdSlider.value = 0.6f;
            GameplayController.RunThreshold = 0.6f;

            m_walkThresholdText.text = "1.2";
            m_walkThresholdSlider.value = 1.2f;
            GameplayController.WalkThreshold = 1.2f;
        }

        public void SetRunOnly()
        {
            m_runThresholdText.text = "0.5";
            m_runThresholdSlider.value = 0.5f;
            GameplayController.RunThreshold = 0.5f;

            m_walkThresholdText.text = "0";
            m_walkThresholdSlider.value = 0f;
            GameplayController.WalkThreshold = 0f;
        }
    }
}
