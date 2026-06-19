using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using GameCreator.Runtime.Common;
using System.Text;

namespace KingEdward.SkillTree
{
    /// <summary>
    /// Configurable tooltip that displays skill information
    /// </summary>
    [Icon(SkillTreePaths.SKILL_TOOLTIP)]
    [AddComponentMenu("KingEdward/Skill Tree/Skill Tooltip")]
    public class SkillTooltip : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Text nameText;
        [SerializeField] private Text descriptionText;
        [SerializeField] private Text costText;
        [SerializeField] private Text levelText;
        [SerializeField] private Text cooldownText;
        [SerializeField] private Text prerequisitesText;
        [SerializeField] private Text statusText;
        [SerializeField] private Image iconImage;
        
        [Header("Optional Elements")]
        [SerializeField] private GameObject lockedIndicator;
        [SerializeField] private GameObject unlockedIndicator;
        [SerializeField] private GameObject canUnlockIndicator;
        
        [Header("Colors")]
        [SerializeField] private Color canUnlockColor = Color.green;
        [SerializeField] private Color cannotUnlockColor = Color.red;
        [SerializeField] private Color readyToUseColor = Color.cyan;
        
        [Header("Settings")]
        [SerializeField] private bool hideWhenNoSkill = true;
        [SerializeField] private Vector2 offset = new Vector2(15, 15);
        
        [Header("Prerequisites Display")]
        [SerializeField] private string prerequisitesLabel = "Prerequisites:";
        [SerializeField] private PrerequisiteStyle prerequisiteStyle = PrerequisiteStyle.Checkbox;
        [SerializeField] private string checkboxMet = "☑";
        [SerializeField] private string checkboxNotMet = "☐";
        [SerializeField] private string bulletMet = "✓";
        [SerializeField] private string bulletNotMet = "✗";
        [SerializeField] private string arrowMet = "→";
        [SerializeField] private string arrowNotMet = "→";
        
        public enum PrerequisiteStyle
        {
            Checkbox,   // ☑ / ☐
            Bullet,     // ✓ / ✗
            Arrow,      // → (same for both)
            Dash,       // - (same for both)
            None        // No icon
        }
        
        private Skill currentSkill;
        private SkillTreeComponent skillTreeComponent;
        private Canvas parentCanvas;
        private RectTransform rectTransform;
        private CanvasGroup canvasGroup;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            parentCanvas = GetComponentInParent<Canvas>();
            
            // Ensure tooltip doesn't block raycasts (prevents flickering)
            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;
            
            // Disable raycast on all child images
            Image[] images = GetComponentsInChildren<Image>(true);
            foreach (var img in images)
            {
                img.raycastTarget = false;
            }
            
            // Disable raycast on all child texts
            Text[] texts = GetComponentsInChildren<Text>(true);
            foreach (var txt in texts)
            {
                txt.raycastTarget = false;
            }
            
            if (hideWhenNoSkill)
            {
                gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// Show tooltip for a specific skill
        /// </summary>
        public void Show(Skill skill, SkillTreeComponent skillTree, Vector2 position, Vector2? customOffset = null)
        {
            if (skill == null)
            {
                Hide();
                return;
            }

            currentSkill = skill;
            skillTreeComponent = skillTree;
            
            // Use custom offset if provided, otherwise use default
            Vector2 effectiveOffset = customOffset ?? offset;
            
            // Ensure tooltip is active before updating
            if (!gameObject.activeSelf)
            {
                gameObject.SetActive(true);
            }
            
            UpdateContent();
            UpdatePosition(position, effectiveOffset);
        }

        /// <summary>
        /// Hide the tooltip
        /// </summary>
        public void Hide()
        {
            currentSkill = null;
            positionLocked = false;
            gameObject.SetActive(false);
        }

        /// <summary>
        /// Update tooltip content
        /// </summary>
        private void UpdateContent()
        {
            if (currentSkill == null) return;

            Args args = skillTreeComponent != null ? new Args(skillTreeComponent.gameObject) : Args.EMPTY;
            
            bool isUnlocked = skillTreeComponent != null && skillTreeComponent.IsUnlocked(currentSkill);
            bool canUnlock = skillTreeComponent != null && skillTreeComponent.CanUnlock(currentSkill);
            bool canUse = isUnlocked && currentSkill.CanUse(args);
            
            SkillInstance skillInstance = skillTreeComponent?.GetSkill(currentSkill);
            int currentLevel = skillInstance?.currentLevel ?? 1;
            bool isMaxLevel = skillInstance?.IsMaxLevel ?? false;

            // Name
            if (nameText != null)
            {
                nameText.text = currentSkill.SkillName;
            }

            // Description
            if (descriptionText != null)
            {
                descriptionText.text = currentSkill.Description;
            }

            // Icon
            if (iconImage != null)
            {
                iconImage.sprite = currentSkill.Icon;
                iconImage.enabled = currentSkill.Icon != null;
            }

            // Cost
            if (costText != null)
            {
                if (!isUnlocked && currentSkill.Cost > 0)
                {
                    int availablePoints = skillTreeComponent?.CurrentSkillPoints ?? 0;
                    bool hasEnough = availablePoints >= currentSkill.Cost;
                    
                    costText.text = $"Cost: {currentSkill.Cost} SP";
                    costText.color = hasEnough ? Color.white : Color.red;
                    costText.gameObject.SetActive(true);
                }
                else
                {
                    costText.gameObject.SetActive(false);
                }
            }

            // Level
            if (levelText != null)
            {
                if (isUnlocked)
                {
                    string levelInfo = $"Level: {currentLevel}/{currentSkill.maxLevel}";
                    if (isMaxLevel)
                    {
                        levelInfo += " (MAX)";
                    }
                    levelText.text = levelInfo;
                    levelText.gameObject.SetActive(true);
                }
                else
                {
                    levelText.gameObject.SetActive(false);
                }
            }

            // Cooldown
            if (cooldownText != null)
            {
                if (currentSkill.isActiveSkill && currentSkill.CooldownDuration > 0)
                {
                    cooldownText.text = $"Cooldown: {currentSkill.CooldownDuration:0.0}s";
                    cooldownText.gameObject.SetActive(true);
                }
                else
                {
                    cooldownText.gameObject.SetActive(false);
                }
            }

            // Prerequisites
            if (prerequisitesText != null)
            {
                if (!isUnlocked)
                {
                    // Show only prerequisites (unlock conditions are checked but not displayed)
                    if (currentSkill.prerequisites != null && currentSkill.prerequisites.Count > 0)
                    {
                        StringBuilder prereqBuilder = new StringBuilder();
                        
                        // Add label if configured
                        if (!string.IsNullOrEmpty(prerequisitesLabel))
                        {
                            prereqBuilder.AppendLine(prerequisitesLabel);
                        }
                        
                        foreach (var prereq in currentSkill.prerequisites)
                        {
                            if (prereq != null && skillTreeComponent != null)
                            {
                                bool isMet = prereq.IsMet(skillTreeComponent);
                                string icon = GetPrerequisiteIcon(isMet);
                                string statusText = prereq.GetStatusText(skillTreeComponent);
                                
                                // Remove default icons (✅ ❌) from status text
                                if (statusText.StartsWith("✅ "))
                                {
                                    statusText = statusText.Substring(2);
                                }
                                else if (statusText.StartsWith("❌ "))
                                {
                                    statusText = statusText.Substring(2);
                                }
                                
                                if (prerequisiteStyle == PrerequisiteStyle.None)
                                {
                                    prereqBuilder.AppendLine(statusText);
                                }
                                else
                                {
                                    prereqBuilder.AppendLine($"{icon} {statusText}");
                                }
                            }
                        }
                        
                        prerequisitesText.text = prereqBuilder.ToString().TrimEnd();
                        prerequisitesText.gameObject.SetActive(true);
                    }
                    else
                    {
                        prerequisitesText.gameObject.SetActive(false);
                    }
                }
                else
                {
                    prerequisitesText.gameObject.SetActive(false);
                }
            }

            // Status
            if (statusText != null)
            {
                string status = GetStatusText(isUnlocked, canUnlock, canUse, skillInstance);
                statusText.text = status;
                
                // Color based on status
                if (!isUnlocked)
                {
                    statusText.color = canUnlock ? canUnlockColor : cannotUnlockColor;
                }
                else if (skillInstance != null && skillInstance.isOnCooldown)
                {
                    statusText.color = cannotUnlockColor;
                }
                else
                {
                    statusText.color = readyToUseColor;
                }
            }

            // Indicators
            if (lockedIndicator != null)
            {
                lockedIndicator.SetActive(!isUnlocked && !canUnlock);
            }

            if (canUnlockIndicator != null)
            {
                canUnlockIndicator.SetActive(!isUnlocked && canUnlock);
            }

            if (unlockedIndicator != null)
            {
                unlockedIndicator.SetActive(isUnlocked);
            }
        }

        private string GetStatusText(bool isUnlocked, bool canUnlock, bool canUse, SkillInstance skillInstance)
        {
            if (!isUnlocked)
            {
                return canUnlock ? "Available to Unlock" : "Locked";
            }

            if (skillInstance != null && skillInstance.isOnCooldown)
            {
                return $"On Cooldown ({skillInstance.cooldownRemaining:0.0}s)";
            }

            if (!canUse)
            {
                return "Conditions Not Met";
            }

            return "Ready to Use";
        }

        private Vector2 currentOffset;
        private bool positionLocked;
        
        /// <summary>
        /// When true, UpdatePosition() does not move the tooltip (e.g. when shown by gamepad selection).
        /// </summary>
        public void SetPositionLocked(bool locked) { positionLocked = locked; }
        
        /// <summary>
        /// Position tooltip at the configured offset only (fixed position, no cursor). E.g. for gamepad cursor mode.
        /// </summary>
        public void SetPositionToOffsetOnly()
        {
            if (rectTransform == null) return;
            rectTransform.anchoredPosition = offset;
        }
        
        /// <summary>
        /// Update tooltip position to follow cursor or target
        /// </summary>
        private void UpdatePosition(Vector2 screenPosition, Vector2 effectiveOffset)
        {
            if (rectTransform == null || parentCanvas == null) return;

            currentOffset = effectiveOffset;

            // Convert screen position to canvas position
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                parentCanvas.transform as RectTransform,
                screenPosition,
                parentCanvas.worldCamera,
                out Vector2 localPoint
            );

            // Apply offset
            localPoint += effectiveOffset;

            // Clamp to screen bounds
            RectTransform canvasRect = parentCanvas.transform as RectTransform;
            Vector2 tooltipSize = rectTransform.sizeDelta;
            
            float minX = -canvasRect.rect.width / 2 + tooltipSize.x / 2;
            float maxX = canvasRect.rect.width / 2 - tooltipSize.x / 2;
            float minY = -canvasRect.rect.height / 2 + tooltipSize.y / 2;
            float maxY = canvasRect.rect.height / 2 - tooltipSize.y / 2;

            localPoint.x = Mathf.Clamp(localPoint.x, minX, maxX);
            localPoint.y = Mathf.Clamp(localPoint.y, minY, maxY);

            rectTransform.anchoredPosition = localPoint;
        }
        
        /// <summary>
        /// Get the icon for prerequisite based on style and met status
        /// </summary>
        private string GetPrerequisiteIcon(bool isMet)
        {
            switch (prerequisiteStyle)
            {
                case PrerequisiteStyle.Checkbox:
                    return isMet ? checkboxMet : checkboxNotMet;
                case PrerequisiteStyle.Bullet:
                    return isMet ? bulletMet : bulletNotMet;
                case PrerequisiteStyle.Arrow:
                    return isMet ? arrowMet : arrowNotMet;
                case PrerequisiteStyle.Dash:
                    return "-";
                case PrerequisiteStyle.None:
                default:
                    return "";
            }
        }

        /// <summary>
        /// Get pointer position (New Input System): gamepad cursor, mouse, touch (mobile).
        /// </summary>
        private static Vector2 GetPointerScreenPosition()
        {
            if (SkillTreeUI.GamepadCursorScreenPosition.HasValue)
                return SkillTreeUI.GamepadCursorScreenPosition.Value;
            if (Pointer.current != null)
                return Pointer.current.position.ReadValue();
            if (Mouse.current != null)
                return Mouse.current.position.ReadValue();
            if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed)
                return Touchscreen.current.primaryTouch.position.ReadValue();
            return Vector2.zero;
        }

        /// <summary>
        /// Update position to follow pointer (mouse, touch, or gamepad cursor).
        /// Does nothing if position is locked (e.g. tooltip shown by gamepad selection).
        /// </summary>
        public void UpdatePosition()
        {
            if (positionLocked || currentSkill == null) return;
            Vector2 screenPos = GetPointerScreenPosition();
            if (screenPos != Vector2.zero)
                UpdatePosition(screenPos, currentOffset);
        }
        
        /// <summary>
        /// Get conditions text from skill
        /// </summary>
        private string GetConditionsText(Skill skill, Args args)
        {
            if (skill == null) return "";
            return skill.GetUnlockConditionsText(args);
        }
    }
}
