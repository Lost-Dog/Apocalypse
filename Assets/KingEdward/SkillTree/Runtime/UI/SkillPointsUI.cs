using UnityEngine;
using UnityEngine.UI;
using GameCreator.Runtime.Common;

namespace KingEdward.SkillTree
{
    /// <summary>
    /// Lightweight UI component to display current skill points
    /// Automatically subscribes to skill points change events
    /// </summary>
    [Icon(SkillTreePaths.SKILL_POINTS_UI)]
    [AddComponentMenu("KingEdward/Skill Tree/Skill Points UI")]
    public class SkillPointsUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PropertyGetGameObject m_SkillTreeComponent = GetGameObjectSelf.Create();
        [SerializeField] private Text m_PointsText;
        
        [Header("Display Settings")]
        [SerializeField] private string m_Format = "{0}";
        [SerializeField] private string m_LabelFormat = "Skill Points: {0}";
        [SerializeField] private bool m_UseLabel = true;
        [SerializeField] private bool m_ShowMaxPoints = false;
        [SerializeField] private string m_MaxPointsFormat = "{0}/{1}";
        
        [Header("Visual Feedback")]
        [SerializeField] private bool m_AnimateOnChange = true;
        [SerializeField] private float m_AnimationDuration = 0.3f;
        [SerializeField] private Color m_IncreaseColor = Color.green;
        [SerializeField] private Color m_DecreaseColor = Color.red;
        [SerializeField] private Color m_NormalColor = Color.white;
        
        private SkillTreeComponent m_CachedSkillTree;
        private int m_LastPoints = -1;
        private Coroutine m_AnimationCoroutine;
        
        private void Start()
        {
            // Get SkillTreeComponent reference
            m_CachedSkillTree = m_SkillTreeComponent.Get<SkillTreeComponent>(Args.EMPTY);
            
            if (m_CachedSkillTree == null)
            {
                Debug.LogError("[SkillPointsUI] SkillTreeComponent not found! Assign it in the Inspector.");
                enabled = false;
                return;
            }
            
            // Subscribe to event
            m_CachedSkillTree.OnSkillPointsChanged += OnSkillPointsChanged;
            
            // Initial update
            UpdateDisplay(m_CachedSkillTree.CurrentSkillPoints, false);
        }
        
        private void OnDestroy()
        {
            // Unsubscribe from event
            if (m_CachedSkillTree != null)
            {
                m_CachedSkillTree.OnSkillPointsChanged -= OnSkillPointsChanged;
            }
        }
        
        private void OnSkillPointsChanged(int newPoints)
        {
            UpdateDisplay(newPoints, true);
        }
        
        private void UpdateDisplay(int points, bool animate)
        {
            if (m_PointsText == null) return;
            
            // Format text
            string displayText;
            
            if (m_ShowMaxPoints && m_CachedSkillTree != null)
            {
                displayText = string.Format(m_MaxPointsFormat, points, m_CachedSkillTree.MaxSkillPoints);
            }
            else
            {
                displayText = string.Format(m_Format, points);
            }
            
            if (m_UseLabel)
            {
                displayText = string.Format(m_LabelFormat, displayText);
            }
            
            m_PointsText.text = displayText;
            
            // Animate if enabled
            if (animate && m_AnimateOnChange && m_LastPoints >= 0)
            {
                bool increased = points > m_LastPoints;
                
                if (m_AnimationCoroutine != null)
                {
                    StopCoroutine(m_AnimationCoroutine);
                }
                
                m_AnimationCoroutine = StartCoroutine(AnimateColorChange(increased));
            }
            
            m_LastPoints = points;
        }
        
        private System.Collections.IEnumerator AnimateColorChange(bool increased)
        {
            Color targetColor = increased ? m_IncreaseColor : m_DecreaseColor;
            
            // Flash to target color
            m_PointsText.color = targetColor;
            
            // Wait
            yield return new WaitForSeconds(m_AnimationDuration);
            
            // Fade back to normal
            float elapsed = 0f;
            while (elapsed < m_AnimationDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / m_AnimationDuration;
                m_PointsText.color = Color.Lerp(targetColor, m_NormalColor, t);
                yield return null;
            }
            
            m_PointsText.color = m_NormalColor;
            m_AnimationCoroutine = null;
        }
        
        /// <summary>
        /// Manually refresh the display (useful if you change format at runtime)
        /// </summary>
        public void RefreshDisplay()
        {
            if (m_CachedSkillTree != null)
            {
                UpdateDisplay(m_CachedSkillTree.CurrentSkillPoints, false);
            }
        }
        
        /// <summary>
        /// Set the SkillTreeComponent reference at runtime
        /// </summary>
        public void SetSkillTreeComponent(SkillTreeComponent skillTree)
        {
            // Unsubscribe from old
            if (m_CachedSkillTree != null)
            {
                m_CachedSkillTree.OnSkillPointsChanged -= OnSkillPointsChanged;
            }
            
            // Set new
            m_CachedSkillTree = skillTree;
            
            if (m_CachedSkillTree != null)
            {
                m_CachedSkillTree.OnSkillPointsChanged += OnSkillPointsChanged;
                UpdateDisplay(m_CachedSkillTree.CurrentSkillPoints, false);
            }
        }
    }
}
