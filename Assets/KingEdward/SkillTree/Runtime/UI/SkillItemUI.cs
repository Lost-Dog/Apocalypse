using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using System.Collections.Generic;
using GameCreator.Runtime.Common;
using KingEdward;

namespace KingEdward.SkillTree
{
    [Icon(SkillTreePaths.SKILL_ITEM_UI)]
    [AddComponentMenu("KingEdward/Skill Tree/Skill Item UI")]
    public class SkillItemUI : MonoBehaviour, 
    IBeginDragHandler, 
    IDragHandler, 
    IEndDragHandler,
    IPointerClickHandler,
    IPointerEnterHandler
{
    public static event Action<SkillItemUI, PointerEventData> EventBeginDrag;
    public static event Action<SkillItemUI, PointerEventData> EventEndDrag;
    public static event Action<SkillItemUI> EventSkillClicked;

    public Skill skill;

    public SkillTreeComponent GetSkillTreeComponent()
    {
        SkillTreeUI skillTreeUI = GetComponentInParent<SkillTreeUI>();
        return skillTreeUI?.skillTreeComponent;
    }

    public SkillHotbarUI GetSkillHotbarUI()
    {
        SkillTreeComponent skillTreeComponent = GetSkillTreeComponent();
        return skillTreeComponent?.GetSkillHotbarUI();
    }
    
    public Button unlockButton;
    public Image icon;
    public GameObject lockedOverlay;
    public GameObject unlockedOverlay;
    public GameObject dragVisual; 
    
    [Header("Cooldown Visualization")]
    public Image cooldownOverlay;
    public Text cooldownText;
    public bool showCooldownNumbers = true;
    public string cooldownFormat = "0.0";
    
    [Header("Tooltip")]
    [SerializeField] private bool showTooltipOnHover = true;
    [SerializeField] private Vector2 tooltipOffset = new Vector2(15, 15);
    public Vector2 TooltipOffset => tooltipOffset;
    
    [Header("Level System")]
    public Text levelText;
    public Text levelUpButtonText;
    public Button levelUpButton;
    public GameObject levelUpIndicator;
    
    [Header("Refund System")]
    public Button refundButton;
    public Text refundButtonText;
    
    [Header("Gamepad / Selection")]
    [Tooltip("When on: North (Y/Triangle) on this node clicks whichever inner button is active (Unlock → Level Up → Refund). South = select for hotbar.")]
    [SerializeField] private bool northClicksActiveButton = true;
    

    public bool debugDragDrop = false;

    private CanvasGroup canvasGroup;
    private bool isDragging = false;
    private Transform canvasTransform;
    private GameObject dragInstance;
    

    [SerializeField] private float dragIconScale = 0.5f; 
    

    [Header("Slot Visualization During Drag")]
    public bool highlightSlotsOnDrag = true;
    public Color slotHighlightColor = new Color(0.5f, 0.9f, 0.5f, 0.5f);
    private List<Image> slotBackgroundImages = new List<Image>();
    private List<Color> originalSlotColors = new List<Color>();

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();
        

        Image[] images = GetComponentsInChildren<Image>(true);
        foreach (var img in images)
        {
            img.raycastTarget = true;
        }

        if (dragVisual != null) dragVisual.SetActive(false);

        Canvas rootCanvas = GetComponentInParent<Canvas>();
        if (rootCanvas != null && rootCanvas.isRootCanvas)
        {
            canvasTransform = rootCanvas.transform;
        }
        else
        {
            Transform current = transform;
            while (current.parent != null)
            {
                Canvas canvas = current.parent.GetComponent<Canvas>();
                if (canvas != null && canvas.isRootCanvas)
                {
                    canvasTransform = canvas.transform;
                    break;
                }
                current = current.parent;
            }
        }
        
        gameObject.SetActive(true);
        if (canvasTransform == null)
        {
            Debug.LogWarning("SkillItemUI " + name + ": Root Canvas not found!");
        }
    }

    private SkillTreeComponent cachedSkillTreeComponent;
    
    void Start()
    {
        if (unlockButton != null)
        {
            unlockButton.onClick.AddListener(UnlockSkill);
        }
        
        if (levelUpButton != null)
        {
            levelUpButton.onClick.AddListener(LevelUpSkill);
        }
        
        if (refundButton != null)
        {
            refundButton.onClick.AddListener(RefundSkill);
        }
        
        cachedSkillTreeComponent = GetSkillTreeComponent();
        if (cachedSkillTreeComponent == null)
        {
            if (!name.Contains("(Clone)"))
            {
                Debug.LogError($"SkillItemUI {name}: SkillTreeComponent not found! Make sure this SkillItemUI is inside a SkillTreeUI.");
            }
            return;
        }
        
        if (skill != null && cachedSkillTreeComponent != null)
        {
            cachedSkillTreeComponent.OnSkillCooldownChanged += OnSkillCooldownChanged;
        }
        
        Refresh();
        
        EnsureSelectableForGamepad();
        
        if (highlightSlotsOnDrag && !name.Contains("(Clone)"))
        {
            SkillHotbarUI hotbarUI = GetSkillHotbarUI();
            if (hotbarUI != null)
            {
                foreach (var slot in hotbarUI.slots)
                {
                    if (slot.slotRectTransform != null)
                    {
                        Image img = slot.slotRectTransform.GetComponent<Image>();
                        if (img != null)
                        {
                            slotBackgroundImages.Add(img);
                            originalSlotColors.Add(img.color);
                        }
                    }
                }
            }
        }
    }
    
    void OnDestroy()
    {
        if (cachedSkillTreeComponent != null && skill != null)
        {
            cachedSkillTreeComponent.OnSkillCooldownChanged -= OnSkillCooldownChanged;
        }
        
        cachedSkillTreeComponent = null;
    }
    
    private void OnSkillCooldownChanged(Skill changedSkill)
    {
        if (changedSkill == skill)
        {
            UpdateCooldownVisual();
        }
    }
    
    private SkillInstance GetSkillInstance()
    {
        var skillTreeComponent = GetSkillTreeComponent();
        if (skillTreeComponent == null || skill == null) return null;
        return skillTreeComponent.GetSkill(skill);
    }

    public void Refresh()
    {
        var skillTreeComponent = GetSkillTreeComponent();
        if (skill == null || skillTreeComponent == null) return;
        
        bool unlocked = skillTreeComponent.IsUnlocked(skill);
        bool canUnlock = skillTreeComponent.CanUnlock(skill);

        if (icon != null)
        {
            icon.sprite = skill.Icon;
            icon.enabled = true;
            
            icon.raycastTarget = true; 
        }
        
        if (unlockButton != null)
        {
            unlockButton.interactable = canUnlock && !unlocked;
            
            if (unlockButton.gameObject.activeSelf != !unlocked)
            {
                unlockButton.gameObject.SetActive(!unlocked);
            }
        }
        
        if (lockedOverlay != null)
        {
            bool shouldBeActive = !canUnlock && !unlocked;
            if (lockedOverlay.activeSelf != shouldBeActive)
            {
                lockedOverlay.SetActive(shouldBeActive);
            }
        }
        
        if (unlockedOverlay != null)
        {
            if (unlockedOverlay.activeSelf != unlocked)
            {
                unlockedOverlay.SetActive(unlocked);
            }
        }
        

        UpdateCooldownVisual();
        
        UpdateLevelVisual();
    }
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (showTooltipOnHover && skill != null)
        {
            var skillTreeComponent = GetSkillTreeComponent();
            SkillTreeUI skillTreeUI = GetComponentInParent<SkillTreeUI>();
            
            if (skillTreeUI != null)
            {
                skillTreeUI.EnsureTooltipReference();
                if (skillTreeUI.sharedTooltip != null)
                {
                    skillTreeUI.sharedTooltip.Show(skill, skillTreeComponent, eventData.position, tooltipOffset);
                    skillTreeUI.NotifyTooltipShownByHover();
                }
            }
        }
    }
    
    private void Update()
    {
        SkillTreeUI skillTreeUI = GetComponentInParent<SkillTreeUI>();
        if (skillTreeUI != null && skillTreeUI.sharedTooltip != null && skillTreeUI.sharedTooltip.gameObject.activeSelf)
        {
            skillTreeUI.sharedTooltip.UpdatePosition();
        }
    }
    
    private void UpdateCooldownVisual()
    {
        if (skill == null) return;
        
        SkillInstance skillInstance = GetSkillInstance();
        if (skillInstance == null) return;
        
        bool isOnCooldown = skillInstance.isOnCooldown;
        
        if (cooldownOverlay != null)
        {
            if (cooldownOverlay.gameObject.activeSelf != isOnCooldown)
            {
                cooldownOverlay.gameObject.SetActive(isOnCooldown);
            }
            
            if (isOnCooldown)
            {
                cooldownOverlay.fillAmount = 1f - skillInstance.CooldownProgress;
            }
        }
        
        if (cooldownText != null)
        {
            bool shouldShowText = isOnCooldown && showCooldownNumbers;
            
            if (cooldownText.gameObject.activeSelf != shouldShowText)
            {
                cooldownText.gameObject.SetActive(shouldShowText);
            }
            
            if (isOnCooldown && showCooldownNumbers)
            {
                cooldownText.text = skillInstance.cooldownRemaining.ToString(cooldownFormat);
            }
        }
    }
    
    private void UpdateLevelVisual()
    {
        if (skill == null) return;
        
        var skillTreeComponent = GetSkillTreeComponent();
        bool unlocked = skillTreeComponent != null && skillTreeComponent.IsUnlocked(skill);
        
        SkillInstance skillInstance = skillTreeComponent?.GetSkill(skill);
        int currentLevel = skillInstance?.currentLevel ?? 1;
        bool canLevelUp = skillInstance?.CanLevelUp ?? false;
        bool isMaxLevel = skillInstance?.IsMaxLevel ?? false;
        
        if (levelText != null)
        {
            levelText.text = $"Level {currentLevel}";
            levelText.gameObject.SetActive(unlocked);
        }
        
        if (levelUpButton != null)
        {
            levelUpButton.interactable = unlocked && canLevelUp;
            levelUpButton.gameObject.SetActive(unlocked && canLevelUp);
        }
        
        if (levelUpButtonText != null)
        {
            if (isMaxLevel)
            {
                levelUpButtonText.text = "MAX";
            }
            else
            {
                levelUpButtonText.text = "↑";
            }
        }
        
        if (levelUpIndicator != null)
        {
            bool showIndicator = unlocked && canLevelUp;
            levelUpIndicator.SetActive(showIndicator);
        }
        
        if (refundButton != null && skillTreeComponent != null)
        {
            bool hasDependent = skillTreeComponent.IsPrerequisiteForUnlockedSkill(skill);
            bool allowCascade = skillTreeComponent.AllowCascadeRefund;
            bool canRefund = unlocked && (!hasDependent || allowCascade);
            
            refundButton.interactable = canRefund;
            refundButton.gameObject.SetActive(unlocked);
        }
        
        if (refundButtonText != null && unlocked && skillInstance != null && skillTreeComponent != null)
        {
            int refundAmount = skill.Cost + (skillInstance.currentLevel - 1) * skill.Cost;
            bool hasDependent = skillTreeComponent.IsPrerequisiteForUnlockedSkill(skill);
            bool allowCascade = skillTreeComponent.AllowCascadeRefund;
            
            if (hasDependent && !allowCascade)
            {
                refundButtonText.text = "Locked";
            }
            else if (hasDependent && allowCascade)
            {
                refundButtonText.text = $"Refund All ({refundAmount})";
            }
            else
            {
                refundButtonText.text = $"Refund ({refundAmount})";
            }
        }
    }
    
    private void LevelUpSkill()
    {
        var skillTreeComponent = GetSkillTreeComponent();
        if (skill == null || skillTreeComponent == null) return;
        
        if (!skillTreeComponent.IsUnlocked(skill)) return;
        
        SkillInstance skillInstance = skillTreeComponent.GetSkill(skill);
        if (skillInstance == null || !skillInstance.CanLevelUp) return;
        
        SkillTreeConfirmationUI confirmationUI = FindFirstObjectByType<SkillTreeConfirmationUI>();
        
        if (confirmationUI != null && confirmationUI.enabled && confirmationUI.PreventDirectUnlock)
        {
            confirmationUI.SetSkillTreeComponent(skillTreeComponent);
            confirmationUI.AddPendingLevelUp(skill, skill.Cost);
            return;
        }
        
        skillTreeComponent.LevelUpSkill(skill);
        GetComponentInParent<SkillTreeUI>()?.ClearSelectionAndTooltip();
    }
    
    private void RefundSkill()
    {
        var skillTreeComponent = GetSkillTreeComponent();
        if (skill == null || skillTreeComponent == null) return;
        
        if (!skillTreeComponent.IsUnlocked(skill)) return;
        
        SkillInstance skillInstance = skillTreeComponent.GetSkill(skill);
        if (skillInstance == null) return;
        
        int refundAmount = skillInstance.totalPointsSpent;
        
        SkillTreeConfirmationUI confirmationUI = FindFirstObjectByType<SkillTreeConfirmationUI>();
        
        if (confirmationUI != null && confirmationUI.enabled && confirmationUI.PreventDirectUnlock)
        {
            confirmationUI.SetSkillTreeComponent(skillTreeComponent);
            confirmationUI.AddPendingRefund(skill, refundAmount);
            return;
        }
        
        skillTreeComponent.RefundSkill(skill, refundAllLevels: true);
        GetComponentInParent<SkillTreeUI>()?.ClearSelectionAndTooltip();
    }

    void UnlockSkill()
    {
        var skillTreeComponent = GetSkillTreeComponent();
        if (skillTreeComponent == null || skill == null) return;
        
        SkillTreeConfirmationUI confirmationUI = FindFirstObjectByType<SkillTreeConfirmationUI>();
        if (confirmationUI != null && confirmationUI.enabled && confirmationUI.PreventDirectUnlock)
        {
            EventSkillClicked?.Invoke(this);
            return;
        }
        
        skillTreeComponent.UnlockSkill(skill);
        Refresh();
        GetComponentInParent<SkillTreeUI>()?.ClearSelectionAndTooltip();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        var skillTreeComponent = GetSkillTreeComponent();
        if (skill == null || skillTreeComponent == null || 
            !skillTreeComponent.IsUnlocked(skill) || 
            !skill.isActiveSkill) 
        {
            return;
        }
        SkillInstance skillInstance = GetSkillInstance();
        if (skillInstance != null && skillInstance.isOnCooldown)
        {
            return;
        }

        isDragging = true;
        
        if (highlightSlotsOnDrag)
        {
            HighlightSlots(true);
        }
        
        Vector2 iconSize = Vector2.zero;
        if (icon != null && icon.rectTransform != null)
        {
            iconSize = icon.rectTransform.sizeDelta * dragIconScale; 
        }
        else
        {
            iconSize = new Vector2(30, 30); 
        }
        
        if (dragVisual != null)
        {
            dragInstance = Instantiate(dragVisual, canvasTransform);
            dragInstance.SetActive(true);
            
            Image dragImage = dragInstance.GetComponent<Image>();
            if (dragImage != null && icon != null)
            {
                dragImage.sprite = icon.sprite;
                dragImage.color = new Color(1f, 1f, 1f, 0.7f);
                RectTransform dragRT = dragImage.rectTransform;
                if (dragRT != null)
                {
                    dragRT.sizeDelta = iconSize;
                }

            }
            dragInstance.transform.position = eventData.position;
        }
        else
        {
            dragInstance = new GameObject("DragSkillVisual");
            if (canvasTransform != null)
            {
                dragInstance.transform.SetParent(canvasTransform);
            }
            else
            {
                dragInstance.transform.SetParent(transform.root);
                Debug.LogWarning("SkillItemUI: Canvas não encontrado, usando transform.root");
            }
            
            Image dragImage = dragInstance.AddComponent<Image>();
            dragImage.sprite = icon.sprite;
            dragImage.raycastTarget = false;
            dragImage.color = new Color(1f, 1f, 1f, 0.7f);
            RectTransform rt = dragInstance.GetComponent<RectTransform>();
            rt.sizeDelta = iconSize;
            rt.position = eventData.position;
        }
        canvasGroup.blocksRaycasts = false;
        
        EventBeginDrag?.Invoke(this, eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isDragging || dragInstance == null) return;
        dragInstance.transform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!isDragging) return;

        isDragging = false;
        if (highlightSlotsOnDrag)
        {
            HighlightSlots(false);
        }
        
        if (canvasGroup != null)
            canvasGroup.blocksRaycasts = true;
        if (dragInstance != null)
        {
            Destroy(dragInstance);
            dragInstance = null;
        }
        SkillHotbarUI hotbarUI = GetSkillHotbarUI();
        if (hotbarUI != null)
        {
            int slotIndex = FindClosestSlotToPosition(eventData.position);
            if (slotIndex >= 0)
            {
                hotbarUI.AssignSkillToSlot(skill, slotIndex);
            }
        }
        
        EventEndDrag?.Invoke(this, eventData);
    }
    
    private void HighlightSlots(bool highlight)
    {
        SkillHotbarUI hotbarUI = GetSkillHotbarUI();
        if (hotbarUI == null || slotBackgroundImages.Count == 0)
            return;
            
        for (int i = 0; i < slotBackgroundImages.Count; i++)
        {
            if (slotBackgroundImages[i] != null)
            {
                slotBackgroundImages[i].color = highlight ? slotHighlightColor : originalSlotColors[i];
            }
        }
    }
    
    private int FindClosestSlotToPosition(Vector2 position)
    {
        SkillHotbarUI hotbarUI = GetSkillHotbarUI();
        if (hotbarUI == null)
            return -1;
            
        int closestIndex = -1;
        float closestDistance = float.MaxValue;
        
        for (int i = 0; i < hotbarUI.slots.Count; i++)
        {
            var slot = hotbarUI.slots[i];
            if (slot.slotRectTransform == null) continue;
            if (RectTransformUtility.RectangleContainsScreenPoint(slot.slotRectTransform, position))
                return i;
            Vector3 slotCenter = slot.slotRectTransform.position;
            float distance = Vector2.Distance(position, slotCenter);
            
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestIndex = i;
            }
        }
        if (closestDistance < 100f)
            return closestIndex;
        return -1;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        PerformSelectSkill();
    }
    
    // When North is pressed on this node: if northClicksActiveButton, clicks the active inner button (Unlock/Level Up/Refund). Returns true if a button was clicked.
    public bool TryClickActiveButton()
    {
        if (!northClicksActiveButton) return false;
        Selectable activeInner = GetFirstInteractableInnerSelectable();
        if (activeInner != null && activeInner is Button activeBtn)
        {
            activeBtn.onClick.Invoke();
            return true;
        }
        return false;
    }
    
    private void PerformSelectSkill()
    {
        EventSkillClicked?.Invoke(this);
        
        var skillTreeComponent = GetSkillTreeComponent();
        if (skill == null || skillTreeComponent == null ||
            !skillTreeComponent.IsUnlocked(skill) ||
            !skill.isActiveSkill)
            return;
        
        SkillInstance clickSkillInstance = GetSkillInstance();
        if (clickSkillInstance != null && clickSkillInstance.isOnCooldown)
            return;
        
        SkillHotbarUI hotbarUI = GetSkillHotbarUI();
        if (hotbarUI != null)
        {
            hotbarUI.SelectSkill(skill);
            if (icon != null)
                StartCoroutine(PulseIconEffect());
        }
    }
    
    private bool ensureSelectableDone;
    
    // Adds Button/Selectable to this node so gamepad can focus and submit; onClick calls PerformSelectSkill.
    private void EnsureSelectableForGamepad()
    {
        if (name.Contains("(Clone)")) return;
        if (ensureSelectableDone) return;
        ensureSelectableDone = true;
        
        Button navButton = GetComponent<Button>();
        if (navButton == null)
        {
            Image img = GetComponent<Image>();
            if (img == null)
            {
                img = gameObject.AddComponent<Image>();
                img.color = new Color(1f, 1f, 1f, 0f);
                img.raycastTarget = true;
            }
            navButton = gameObject.AddComponent<Button>();
            navButton.transition = Selectable.Transition.None;
            navButton.targetGraphic = img;
            navButton.interactable = true;
        }
        navButton.onClick.AddListener(PerformSelectSkill);
    }
    
    // Selectable for this node; used by SkillTreeUI for gamepad navigation.
    public Selectable GetNavigationSelectable()
    {
        return GetComponent<Selectable>();
    }
    
    // First active inner button (Unlock, then Level Up, then Refund). Used when entering with South or when North clicks active button.
    public Selectable GetFirstInteractableInnerSelectable()
    {
        if (unlockButton != null && unlockButton.gameObject.activeInHierarchy && unlockButton.interactable) return unlockButton;
        if (levelUpButton != null && levelUpButton.gameObject.activeInHierarchy && levelUpButton.interactable) return levelUpButton;
        if (refundButton != null && refundButton.gameObject.activeInHierarchy && refundButton.interactable) return refundButton;
        return null;
    }
    
    private System.Collections.IEnumerator PulseIconEffect()
    {
        Vector3 originalScale = icon.transform.localScale;
        float pulseDuration = 0.3f;
        float pulseSize = 1.2f;
        float half = pulseDuration / 2f;
        float timer = 0f;
        while (timer < half)
        {
            timer += Time.deltaTime;
            icon.transform.localScale = Vector3.Lerp(originalScale, originalScale * pulseSize, timer / half);
            yield return null;
        }
        timer = 0f;
        while (timer < half)
        {
            timer += Time.deltaTime;
            icon.transform.localScale = Vector3.Lerp(originalScale * pulseSize, originalScale, timer / half);
            yield return null;
        }
        icon.transform.localScale = originalScale;
    }
}
}
