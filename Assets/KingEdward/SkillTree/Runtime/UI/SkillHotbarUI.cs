using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using GameCreator.Runtime.Common;
using UnityEngine.InputSystem;
using System.Collections;
using System;
namespace KingEdward.SkillTree
{
    public enum HotkeyType
    {
        None,
        Keyboard,
        Mouse,
        Gamepad,
        InputSystem
    }

    public enum IndicatorInputMode
    {
        [Tooltip("Hold key to aim, release to cast")]
        HoldAndRelease,
        [Tooltip("First press/release shows indicator, second press/release casts")]
        DualStage
    }
    
    [System.Serializable]
    public class HotkeyConfig
    {
        public HotkeyType type = HotkeyType.Keyboard;
        public KeyCode keyCode = KeyCode.Alpha1;
        public Key key = Key.Digit1;
        public int gamepadButton = 0; // 0=South, 1=East, 2=West, 3=North
        public int mouseButton = 0; // 0=Left, 1=Right, 2=Middle
        public UnityEngine.InputSystem.InputActionAsset inputActionAsset; // For InputSystem type
        public string inputActionName = ""; // Name of the action within the asset
    }

    /// <summary>
    /// Manages hotbar slots for quick access to active skills.
    /// Handles drag and drop operations from the skill tree to hotbar slots.
    /// </summary>
    [Icon(SkillTreePaths.SKILL_HOTBAR)]
    [AddComponentMenu("KingEdward/Skill Tree/Skill Hotbar UI")]
    public class SkillHotbarUI : MonoBehaviour
{
    // REFERENCES
    [Header("Core References")]
    [SerializeField] private PropertyGetGameObject m_SkillTreeComponent = GetGameObjectSelf.Create();
    
    // Public property to access SkillTreeComponent
    public SkillTreeComponent skillTreeComponent => m_SkillTreeComponent.Get<SkillTreeComponent>(Args.EMPTY);
    
    [Header("Slot Configuration")]
    public List<HotbarSlot> slots = new List<HotbarSlot>();
    public Color slotHighlightColor = new Color(0.5f, 0.9f, 0.5f, 0.5f);
    
    [Header("Selection")]
    public int selectedSlotIndex = -1;
    public Color selectedSlotColor = new Color(0.9f, 0.5f, 0.3f, 0.5f);
    public Image selectionIndicator;
    
    // Track currently selected skill from skill tree
    private Skill currentlySelectedSkill;
    
    [Header("Hotkey Configuration")]
    [SerializeField] private InputPropertyButton[] slotHotkeys = new InputPropertyButton[8];
    [Tooltip("HoldAndRelease: hold to aim, release to cast. DualStage: first click shows indicator, second click casts. For hold/release and channelled skills, slot hotkey must send both press and release: use Input System > Input Action Perform (Button) (or While Holding) and bind keys/buttons in the asset.")]
    [SerializeField] private IndicatorInputMode m_IndicatorInputMode = IndicatorInputMode.HoldAndRelease;

    // PRIVATE: hold state from GC2 InputPropertyButton (RegisterStart / RegisterCancel)
    private bool[] m_SlotHeld;
    private bool[] m_WasPressedThisFrame;
    private bool[] m_WasReleasedThisFrame;
    private Action[] m_HotkeyStartCallbacks;
    private Action[] m_HotkeyCancelCallbacks;

    private Skill currentlyDraggedSkill;
    private int m_SlotInAimMode = -1;
    /// <summary> DualStage: set when we get Cancel (release) while in aim </summary>
    private bool m_DualStageReleasedAfterAim;
    /// <summary> DualStage: frame when we entered aim; used so second press (no Cancel) still casts after a short delay. </summary>
    private int m_DualStageAimStartFrame = -1;
    private Color[] originalSlotColors;

    // EVENTS
    public event Action<Skill> OnSkillActivated;

    private void Awake()
    {
        InitializeHotkeys();
        
        CacheSlotColors();
    }
    

    
    /// <summary>
    /// Initialize hotkeys using GameCreator's InputPropertyButton pattern.
    /// Start/Cancel are used for hold state (indicator aim and channelled skills)
    /// </summary>
    private void InitializeHotkeys()
    {
        int n = slotHotkeys.Length;
        m_SlotHeld = new bool[n];
        m_WasPressedThisFrame = new bool[n];
        m_WasReleasedThisFrame = new bool[n];
        m_HotkeyStartCallbacks = new Action[n];
        m_HotkeyCancelCallbacks = new Action[n];

        for (int i = 0; i < n; i++)
        {
            if (slotHotkeys[i] == null)
            {
                slotHotkeys[i] = InputButtonKeyboardPress.Create((Key)((int)Key.Digit1 + i));
            }

            slotHotkeys[i].OnStartup();

            int slotIndex = i;
            slotHotkeys[i].RegisterPerform(() => OnHotkeyPressed(slotIndex));

            m_HotkeyStartCallbacks[i] = () => OnSlotKeyStart(slotIndex);
            m_HotkeyCancelCallbacks[i] = () => OnSlotKeyCancel(slotIndex);
            slotHotkeys[i].RegisterStart(m_HotkeyStartCallbacks[i]);
            slotHotkeys[i].RegisterCancel(m_HotkeyCancelCallbacks[i]);
        }
    }

    private void OnSlotKeyStart(int slotIndex)
    {
        if (slotIndex >= 0 && slotIndex < m_SlotHeld.Length)
        {
            m_SlotHeld[slotIndex] = true;
            m_WasPressedThisFrame[slotIndex] = true;
        }
    }

    private void OnSlotKeyCancel(int slotIndex)
    {
        if (slotIndex >= 0 && slotIndex < m_SlotHeld.Length)
        {
            m_SlotHeld[slotIndex] = false;
            m_WasReleasedThisFrame[slotIndex] = true;
            // DualStage: only allow cast after a second press (release received while in aim)
            if (m_IndicatorInputMode == IndicatorInputMode.DualStage && m_SlotInAimMode == slotIndex)
                m_DualStageReleasedAfterAim = true;
        }
    }

    private void OnEnable()
    {
        // Auto-register with SkillTreeComponent
        if (skillTreeComponent != null)
        {
            skillTreeComponent.RegisterHotbar(this);
        }
        
        // Subscribe to SkillItemUI drag events
        SkillItemUI.EventBeginDrag += OnSkillBeginDrag;
        SkillItemUI.EventEndDrag += OnSkillEndDrag;
        
        // Subscribe to SkillTreeComponent events
        if (skillTreeComponent != null)
        {
            skillTreeComponent.OnSkillCooldownChanged += OnSkillCooldownChanged;
        }
    }

    private void OnDisable()
    {
        // Unregister from SkillTreeComponent
        if (skillTreeComponent != null)
        {
            skillTreeComponent.UnregisterHotbar(this);
        }
        
        // Stop coroutines
        if (cooldownUpdateCoroutine != null)
        {
            StopCoroutine(cooldownUpdateCoroutine);
            cooldownUpdateCoroutine = null;
        }
        
        // Unsubscribe from drag events
        SkillItemUI.EventBeginDrag -= OnSkillBeginDrag;
        SkillItemUI.EventEndDrag -= OnSkillEndDrag;
        
        // Unsubscribe from SkillTreeComponent events
        if (skillTreeComponent != null)
        {
            skillTreeComponent.OnSkillCooldownChanged -= OnSkillCooldownChanged;
        }
        
        // Cleanup hotkeys
        CleanupHotkeys();
    }
    

    private void CleanupHotkeys()
    {
        for (int i = 0; i < slotHotkeys.Length; i++)
        {
            if (slotHotkeys[i] != null)
            {
                int slotIndex = i;
                slotHotkeys[i].ForgetPerform(() => OnHotkeyPressed(slotIndex));
                if (m_HotkeyStartCallbacks != null && i < m_HotkeyStartCallbacks.Length)
                    slotHotkeys[i].ForgetStart(m_HotkeyStartCallbacks[i]);
                if (m_HotkeyCancelCallbacks != null && i < m_HotkeyCancelCallbacks.Length)
                    slotHotkeys[i].ForgetCancel(m_HotkeyCancelCallbacks[i]);
                slotHotkeys[i].OnDispose();
            }
        }
    }
    
    private void Update()
    {

        for (int i = 0; i < slotHotkeys.Length; i++)
        {
            if (slotHotkeys[i] != null)
                slotHotkeys[i].OnUpdate();
        }

        if (m_SlotInAimMode >= 0 && (skillTreeComponent == null || !skillTreeComponent.IsAiming))
        {
            m_SlotInAimMode = -1;
            m_DualStageReleasedAfterAim = false;
            m_DualStageAimStartFrame = -1;
        }

        if (m_IndicatorInputMode == IndicatorInputMode.HoldAndRelease && skillTreeComponent != null && m_WasPressedThisFrame != null)
        {
            // Cast on release before processing a new press
            if (m_SlotInAimMode >= 0 && m_SlotInAimMode < slotHotkeys.Length && m_WasReleasedThisFrame != null && m_WasReleasedThisFrame[m_SlotInAimMode])
            {
                Skill skillToCast = m_SlotInAimMode < slots.Count ? slots[m_SlotInAimMode].skill : null;
                if (skillToCast != null)
                    skillTreeComponent.SetChannelInputContext(this, m_SlotInAimMode);
                skillTreeComponent.EndAimAndCast();
                m_SlotInAimMode = -1;
                if (skillToCast != null) OnSkillActivated?.Invoke(skillToCast);
            }
            else
            {
                // BeginAim
                for (int i = 0; i < slots.Count && i < slotHotkeys.Length; i++)
                {
                    if (slots[i].skill == null) continue;
                    if (slots[i].skill.IndicatorConfig == null || !slots[i].skill.IndicatorConfig.HasIndicator) continue;
                    var inst = GetSkillInstance(slots[i].skill);
                    if (inst != null && inst.isOnCooldown) continue;

                    if (m_WasPressedThisFrame[i])
                    {
                        skillTreeComponent.BeginAim(slots[i].skill);
                        m_SlotInAimMode = i;
                        break;
                    }
                }
            }
        }

        // Clear one-frame flags after processing
        if (m_WasPressedThisFrame != null)
        {
            for (int i = 0; i < m_WasPressedThisFrame.Length; i++)
            {
                m_WasPressedThisFrame[i] = false;
                m_WasReleasedThisFrame[i] = false;
            }
        }
    }
    
    /// <summary>
    /// Get the SkillInstance for a given Skill
    /// </summary>
    public SkillInstance GetSkillInstance(Skill skill)
    {
        return skillTreeComponent?.GetSkill(skill);
    }
    
    /// <summary>
    /// Handle hotkey
    /// </summary>
    private void OnHotkeyPressed(int slotIndex)
    {
        if (currentlySelectedSkill != null)
        {
            AssignSkillToSlot(currentlySelectedSkill, slotIndex);
        }
        else if (slotIndex >= 0 && slotIndex < slots.Count && slots[slotIndex].skill != null && skillTreeComponent != null)
        {
            Skill skill = slots[slotIndex].skill;
            var instance = GetSkillInstance(skill);
            bool hasIndicator = skill.IndicatorConfig != null && skill.IndicatorConfig.HasIndicator;
            bool canUse = instance == null || !instance.isOnCooldown;

            if (m_IndicatorInputMode == IndicatorInputMode.HoldAndRelease && hasIndicator)
            {
                return;
            }
            if (m_IndicatorInputMode == IndicatorInputMode.DualStage && hasIndicator && canUse)
            {
                bool canCast = skillTreeComponent.IsAiming && m_SlotInAimMode == slotIndex &&
                    (m_DualStageReleasedAfterAim || (m_DualStageAimStartFrame >= 0 && Time.frameCount > m_DualStageAimStartFrame + 1));
                if (canCast)
                {
                    skillTreeComponent.EndAimAndCast();
                    m_SlotInAimMode = -1;
                    m_DualStageReleasedAfterAim = false;
                    m_DualStageAimStartFrame = -1;
                    OnSkillActivated?.Invoke(skill);
                }
                else
                {
                    skillTreeComponent.BeginAim(skill);
                    m_SlotInAimMode = slotIndex;
                    m_DualStageReleasedAfterAim = false;
                    m_DualStageAimStartFrame = Time.frameCount;
                }
            }
            else
            {
                ActivateSlot(slotIndex);
            }
        }
        else
        {
            ActivateSlot(slotIndex);
        }
    }

    /// <summary>
    /// Returns true while the input associated with this hotbar slot is being held.
    /// </summary>
    public bool IsSlotInputHeld(int slotIndex)
    {
        if (m_SlotHeld == null || slotIndex < 0 || slotIndex >= m_SlotHeld.Length)
            return false;
        return m_SlotHeld[slotIndex];
    }

    private void CacheSlotColors()
    {
        originalSlotColors = new Color[slots.Count];
        
        for (int i = 0; i < slots.Count; i++)
        {
            if (slots[i].slotRectTransform != null)
            {
                Image slotImage = slots[i].slotRectTransform.GetComponent<Image>();
                if (slotImage != null)
                {
                    originalSlotColors[i] = slotImage.color;
                }
            }
        }
    }


    // Coroutines para otimização de performance
    private Coroutine cooldownUpdateCoroutine;
    
    private void Start()
    {
        // Initial update of the UI
        ForceCompleteVisualRefresh();
        
        // Set up slot buttons
        for (int i = 0; i < slots.Count; i++)
        {
            if (slots[i].slotButton != null)
            {
                int slotIndex = i; // Capture for lambda
                slots[i].slotButton.onClick.AddListener(() => OnSlotClicked(slotIndex));
            }
        }
        
        // Reset cooldowns of all skills in slots
        ResetAllSkillCooldowns();
        
        // Start coroutines for optimized updates
        StartOptimizedUpdates();
    }
    

    private void StartOptimizedUpdates()
    {
        if (cooldownUpdateCoroutine != null)
            StopCoroutine(cooldownUpdateCoroutine);
            
        cooldownUpdateCoroutine = StartCoroutine(CooldownUpdateRoutine());
    }
    

    private IEnumerator CooldownUpdateRoutine()
    {
        while (true)
        {
            UpdateCooldownVisuals();
            yield return new WaitForSeconds(0.1f); // Update 10 times per second
        }
    }
    

    private void OnSkillBeginDrag(SkillItemUI skillItem, UnityEngine.EventSystems.PointerEventData eventData)
    {
        // Store the skill being dragged
        if (skillItem != null && skillItem.skill != null && skillItem.skill.isActiveSkill)
        {
            currentlyDraggedSkill = skillItem.skill;
            
            // Highlight all slots
            HighlightAllSlots(true);
        }
    }


    private void OnSkillEndDrag(SkillItemUI skillItem, UnityEngine.EventSystems.PointerEventData eventData)
    {
        // Skip if not dragging an active skill
        if (currentlyDraggedSkill == null)
            return;
            
        // Find the slot under the pointer
        for (int i = 0; i < slots.Count; i++)
        {
            if (slots[i].slotRectTransform != null && 
                RectTransformUtility.RectangleContainsScreenPoint(slots[i].slotRectTransform, eventData.position))
            {
                // Assign skill to this slot
                AssignSkillToSlot(currentlyDraggedSkill, i);
                break;
            }
        }
        
        // Reset dragging state and slot highlighting
        currentlyDraggedSkill = null;
        HighlightAllSlots(false);
    }

    /// <summary>
    /// Handle cooldown changes for skills in slots
    /// </summary>
    private void OnSkillCooldownChanged(Skill skill)
    {
        // Update visuals for any slots containing this skill
        foreach (var slot in slots)
        {
            if (slot.skill == skill)
            {
                UpdateSlotVisuals(slot);
            }
        }
    }

    /// <summary>
    /// Forcefully update all visual elements
    /// </summary>
    public void ForceCompleteVisualRefresh()
    {
        foreach (var slot in slots)
        {
            UpdateSlotVisuals(slot);
        }
        
        // Update selection indicator
        UpdateSelectionIndicator();
    }

    /// <summary>
    /// Update cooldown indicators for all slots
    /// </summary>
    private void UpdateCooldownVisuals()
    {
        foreach (var slot in slots)
        {
            if (slot != null && slot.skill != null)
            {
                try
                {
                    // Get skill instance and update its cooldown state
                    var skillInstance = GetSkillInstance(slot.skill);
                    if (skillInstance != null)
                    {
                        skillInstance.UpdateCooldown();
                    }
                    
                    // Then update the visual
                    UpdateSlotCooldown(slot);
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"Error updating cooldown for skill {slot.skill.name}: {e.Message}");
                }
            }
        }
    }

    /// <summary>
    /// Update the cooldown visual for a specific slot
    /// </summary>
    private void UpdateSlotCooldown(HotbarSlot slot)
    {
        // Do nothing if there's no skill or cooldown overlay
        if (slot == null || slot.skill == null || slot.cooldownOverlay == null)
        {
            return;
        }
            
        // Get the skill instance to check cooldown state
        var skillInstance = GetSkillInstance(slot.skill);
        if (skillInstance == null)
        {
            slot.cooldownOverlay.gameObject.SetActive(false);
            return;
        }
        
        // Check if skill is actually on cooldown
        bool isOnCooldown = skillInstance.isOnCooldown;
        
        // Configure overlay based on cooldown state
        if (isOnCooldown)
        {
            slot.cooldownOverlay.gameObject.SetActive(true);
                
            if (slot.cooldownOverlay.type != Image.Type.Filled)
            {
                slot.cooldownOverlay.type = Image.Type.Filled;
                slot.cooldownOverlay.fillMethod = Image.FillMethod.Radial360;
                slot.cooldownOverlay.fillOrigin = (int)Image.Origin360.Top;
                slot.cooldownOverlay.fillClockwise = true;
            }
            
            // Set fill amount based on progress
            float progress = skillInstance.cooldownRemaining / skillInstance.CooldownDuration;
            slot.cooldownOverlay.fillAmount = progress;
            
            // Update cooldown text if present
            if (slot.cooldownText != null)
            {
                slot.cooldownText.gameObject.SetActive(true);
                slot.cooldownText.text = skillInstance.cooldownRemaining.ToString("0.0");
            }

            // While on cooldown, hide stack text
            if (slot.stackText != null)
                slot.stackText.gameObject.SetActive(false);
        }
        else
        {
            // Hide all cooldown visuals
            slot.cooldownOverlay.gameObject.SetActive(false);
                
            if (slot.cooldownText != null)
                slot.cooldownText.gameObject.SetActive(false);

            // Show remaining stack uses if configured
            if (slot.stackText != null)
            {
                if (skillInstance.MaxStackUses > 0)
                {
                    slot.stackText.gameObject.SetActive(true);
                    slot.stackText.text = skillInstance.RemainingStackUses.ToString();
                }
                else
                {
                    slot.stackText.gameObject.SetActive(false);
                }
            }
        }
    }

    /// <summary>
    /// Update all visual elements for a slot
    /// </summary>
    private void UpdateSlotVisuals(HotbarSlot slot)
    {
        // Skip slots with missing components
        if (slot == null)
            return;
        
        // Configure the icon image
        if (slot.iconImage != null)
        {
            if (slot.skill != null && slot.skill.Icon != null)
            {
                // Set and enable the icon for the skill
                slot.iconImage.sprite = slot.skill.Icon;
                slot.iconImage.enabled = true;
                slot.iconImage.color = Color.white; // Full opacity
        }
        else
        {
                // Empty slot
                slot.iconImage.sprite = null;
                slot.iconImage.enabled = false;
            }
        }
        
        if (slot.cooldownOverlay != null)
                {
                    slot.cooldownOverlay.gameObject.SetActive(false);
        }
        
        // Update cooldown state separately
        UpdateSlotCooldown(slot);
        
        // Update hotkey text
        if (slot.hotkeyText != null)
        {
            int index = slots.IndexOf(slot);
            if (index >= 0 && index < slotHotkeys.Length && slotHotkeys[index] != null)
            {
                slot.hotkeyText.text = (index + 1).ToString();
            }
        }
        
        // Update level text
        if (slot.levelText != null)
        {
            if (slot.skill != null)
            {
                var skillInstance = GetSkillInstance(slot.skill);
                if (skillInstance != null && skillInstance.currentLevel > 1)
                {
                    slot.levelText.text = skillInstance.currentLevel.ToString();
                    slot.levelText.gameObject.SetActive(true);
                }
                else
                {
                    slot.levelText.gameObject.SetActive(false);
                }
            }
            else
            {
                slot.levelText.gameObject.SetActive(false);
            }
        }
    }

    /// <summary>
    /// Highlight all slots to indicate valid drop targets
    /// </summary>
    private void HighlightAllSlots(bool highlight)
    {
        if (originalSlotColors == null) return;
        for (int i = 0; i < slots.Count; i++)
        {
            if (slots[i].slotRectTransform != null)
            {
                Image slotImage = slots[i].slotRectTransform.GetComponent<Image>();
                if (slotImage != null)
                {
                    if (i == selectedSlotIndex)
                    {
                        slotImage.color = highlight ? Color.Lerp(selectedSlotColor, slotHighlightColor, 0.5f) : selectedSlotColor;
                    }
                    else
                    {
                        Color baseColor = (i < originalSlotColors.Length) ? originalSlotColors[i] : slotImage.color;
                        slotImage.color = highlight ? slotHighlightColor : baseColor;
                    }
                }
            }
        }
    }

    /// <summary>
    /// Select a skill from the skill tree
    /// </summary>
    public void SelectSkill(Skill skill)
    {
        if (skill == null || !skill.isActiveSkill)
            return;
            
        currentlySelectedSkill = skill;
    }

    /// <summary>
    /// Assign a skill to a specific slot
    /// </summary>
    public void AssignSkillToSlot(Skill skill, int slotIndex)
    {
        if (skill == null || slotIndex < 0 || slotIndex >= slots.Count)
        {
            Debug.LogWarning($"[SkillHotbar] Invalid slot assignment: skill={skill}, slot={slotIndex}");
            return;
        }
        
        // Verify skill is unlocked
        if (skillTreeComponent != null && !skillTreeComponent.IsUnlocked(skill))
        {
            Debug.LogWarning($"[SkillHotbar] Cannot assign locked skill {skill.name} to slot {slotIndex}");
            return;
        }
            
        // Remove from any existing slot
        foreach (var slot in slots)
        {
            if (slot.skill == skill)
            {
                slot.skill = null;
                slot.UpdateVisual();
            }
        }
        
        // Assign to new slot
        slots[slotIndex].skill = skill;
        slots[slotIndex].UpdateVisual();
        
        // Clear the selected skill since we've assigned it
        currentlySelectedSkill = null;
    }
    

    public void ClearAllSlots()
    {
        foreach (var slot in slots)
        {
            slot.skill = null;
            slot.UpdateVisual();
        }
        
        currentlySelectedSkill = null;
    }
    
    /// <summary>
    /// Remove a specific skill from all slots
    /// </summary>
    public void RemoveSkillFromAllSlots(Skill skill)
    {
        if (skill == null) return;
        
        foreach (var slot in slots)
        {
            if (slot.skill == skill)
            {
                slot.skill = null;
                slot.UpdateVisual();
            }
        }
        
    }

    /// <summary>
    /// Handle slot click event
    /// </summary>
    private void OnSlotClicked(int slotIndex)
    {
        // If already selected, activate it directly
        if (slotIndex == selectedSlotIndex)
        {
            ActivateSlot(slotIndex);
        }
        else
        {
            // Otherwise, select it
            SelectSlot(slotIndex);
        }
    }

    /// <summary>
    /// Select a slot as the current active slot
    /// </summary>
    private void SelectSlot(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= slots.Count)
            return;
            
        // Set selection
        selectedSlotIndex = slotIndex;
        
        // Update selection indicator
        UpdateSelectionIndicator();
        
        // Highlight the selected slot
        for (int i = 0; i < slots.Count; i++)
        {
            Image slotImage = slots[i].slotRectTransform?.GetComponent<Image>();
            if (slotImage != null)
            {
                Color baseColor = (originalSlotColors != null && i < originalSlotColors.Length) ? originalSlotColors[i] : slotImage.color;
                slotImage.color = (i == selectedSlotIndex) ? selectedSlotColor : baseColor;
            }
        }
    }

    private void UpdateSelectionIndicator()
    {
        if (selectionIndicator == null)
            return;
            
        if (selectedSlotIndex >= 0 && selectedSlotIndex < slots.Count)
        {
            selectionIndicator.gameObject.SetActive(true);
            selectionIndicator.rectTransform.position = slots[selectedSlotIndex].slotRectTransform.position;
        }
        else
        {
            selectionIndicator.gameObject.SetActive(false);
        }
    }

    private void ClearSelection()
    {
        selectedSlotIndex = -1;
        UpdateSelectionIndicator();
        
        if (originalSlotColors == null) return;
        for (int i = 0; i < slots.Count; i++)
        {
            Image slotImage = slots[i].slotRectTransform?.GetComponent<Image>();
            if (slotImage != null && i < originalSlotColors.Length)
            {
                slotImage.color = originalSlotColors[i];
            }
        }
    }


    /// <summary>
    /// Activate the skill in the specified slot
    /// </summary>
    public void ActivateSlot(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= slots.Count)
        {
            return;
        }
            
        // Get the slot
        HotbarSlot slot = slots[slotIndex];
        
        // No skill in slot
        if (slot.skill == null)
        {
            return;
        }
        
        // Select the slot visually
        SelectSlot(slotIndex);
        
        // Skip if on cooldown
        var skillInstance = GetSkillInstance(slot.skill);
        if (skillInstance != null && skillInstance.isOnCooldown)
        {
            return;
        }
            
        if (skillTreeComponent != null)
        {
            // Provide channel context so channelled skills know which input/slot they came from
            skillTreeComponent.SetChannelInputContext(this, slotIndex);
            skillTreeComponent.UseSkill(slot.skill);
            
            OnSkillActivated?.Invoke(slot.skill);
            
            StartCoroutine(FlashSlotIcon(slotIndex));
            
            UpdateSlotVisuals(slot);
            
            ClearSelection();
        }
    }


    private IEnumerator FlashSlotIcon(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= slots.Count || slots[slotIndex].iconImage == null)
            yield break;
            
        // Cache original color
        Image icon = slots[slotIndex].iconImage;
        Color originalColor = icon.color;
        
        // Flash white
        icon.color = Color.white;
        
        // Wait briefly
        yield return new WaitForSeconds(0.1f);
        
        // Restore color
        icon.color = originalColor;
    }


    private void ResetAllSkillCooldowns()
    {
        if (skillTreeComponent != null)
        {
            skillTreeComponent.ResetAllCooldowns();
        }
        UpdateCooldownVisuals();
    }

    /// <summary>
    /// Data structure for a hotbar slot
    /// </summary>
    [Serializable]
    public class HotbarSlot
    {
        public Skill skill;
        public RectTransform slotRectTransform;
        public Button slotButton;
        public Image iconImage;
        public Image cooldownOverlay;
        public Text cooldownText;
        public Text hotkeyText;
        public Text levelText; 
        public Text stackText; 
    
        public void UpdateVisual()
        {
            if (iconImage != null)
            {
                if (skill != null && skill.Icon != null)
                {
                    iconImage.sprite = skill.Icon;
                    iconImage.enabled = true;
                    iconImage.color = Color.white;
                }
                else
                {
                    iconImage.sprite = null;
                    iconImage.enabled = false;
                }
            }
    
            // Update cooldown state
            if (cooldownOverlay != null)
            {
                cooldownOverlay.gameObject.SetActive(false);
            }
    
            // Update hotkey text
            if (hotkeyText != null)
            {
                SkillHotbarUI hotbarUI = slotRectTransform.GetComponentInParent<SkillHotbarUI>();
                if (hotbarUI != null)
                {
                    int index = hotbarUI.slots.IndexOf(this);
                    if (index >= 0 && index < hotbarUI.slots.Count)
                    {
                        hotkeyText.text = (index + 1).ToString();
                    }
                }
            }
        
            // Update level text
            if (levelText != null)
            {
                if (skill != null)
                {
                    // Get SkillHotbarUI to access SkillTreeComponent
                    SkillHotbarUI hotbarUI = slotRectTransform.GetComponentInParent<SkillHotbarUI>();
                    if (hotbarUI != null && hotbarUI.skillTreeComponent != null)
                    {
                        var skillInstance = hotbarUI.GetSkillInstance(skill);
                        if (skillInstance != null && skillInstance.currentLevel > 1)
                        {
                            levelText.text = skillInstance.currentLevel.ToString();
                            levelText.gameObject.SetActive(true);
                        }
                        else
                        {
                            levelText.gameObject.SetActive(false);
                        }
                    }
                    else
                    {
                        levelText.gameObject.SetActive(false);
                    }
                }
                else
                {
                    levelText.gameObject.SetActive(false);
                }
            }

            // Reset stack text 
            if (stackText != null)
            {
                stackText.gameObject.SetActive(false);
            }
        }
    }
}
}