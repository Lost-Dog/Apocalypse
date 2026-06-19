using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using System.Collections;
using GameCreator.Runtime.Common;
using System;
using KingEdward;

namespace KingEdward.SkillTree
{
    public enum LineCurveType
{
    Straight,
    SingleCurve,
    SCurve
}

/// <summary>
/// UI Manager for the entire skill tree display.
/// Works with manually placed SkillItemUI components in the scene.
/// </summary>
[Icon(SkillTreePaths.SKILL_TREE_UI)]
[AddComponentMenu("KingEdward/Skill Tree/Skill Tree UI")]
public class SkillTreeUI : MonoBehaviour
{
    [Header("Core References")]
    [SerializeField] private PropertyGetGameObject m_SkillTreeComponent = GetGameObjectSelf.Create();
    
    public SkillTreeComponent skillTreeComponent => m_SkillTreeComponent.Get<SkillTreeComponent>(Args.EMPTY);
    
    [Header("Skill Item UIs")]
    public List<SkillItemUI> skillItems = new List<SkillItemUI>();
    
    [Header("Shared Tooltip")]
    [Tooltip("Assign the SkillTooltip that is already inside this prefab (e.g. a child). If null, one will be searched in children.")]
    [SerializeField] public SkillTooltip sharedTooltip;
    
    [HideInInspector][Header("Gamepad / Navigation")]
    [Tooltip("Selection = navigate with d-pad/stick between skills. Cursor = move pointer with stick (tooltip + click follow cursor).")]
    [SerializeField] private GamepadControlMode gamepadControlMode = GamepadControlMode.Selection;
    [Tooltip("When using Selection mode: show tooltip when navigating to a skill (focus).")]
    [SerializeField] private bool showTooltipOnSelection = true;
    [Tooltip("Cursor mode: stick movement speed (pixels per second).")]
    [SerializeField] private float cursorSpeed = 600f;
    [Tooltip("Cursor mode: optional cursor graphic (RectTransform). If null, a simple dot is created at runtime.")]
    [SerializeField] private RectTransform cursorGraphic;
    [Tooltip("Hide cursor when using gamepad; show again when mouse is used.")]
    [SerializeField] private bool hideCursorOnGamepad = false;
    [Tooltip("Selection mode: South on node enters inner buttons (Unlock/Level Up/Refund), East exits. When off, South/East use only SkillItemUI Submit Button or select for hotbar.")]
    [SerializeField] private bool selectionSouthEntersInnerButtons = true;
    
    public enum GamepadControlMode { Selection, Cursor }
    
    public static Vector2? GamepadCursorScreenPosition { get; private set; }
    
    private bool pendingLineUpdate = false;
    private GameObject lastSelectedForTooltip;
    private SkillItemUI lastHoveredSkillItemForCursor;
    private bool tooltipShownByHover;
    private Vector2 cursorScreenPosition;
    private RectTransform cursorRect;
    private bool cursorCreated;
    private bool cursorVisibleByGamepad;
    private bool cursorHiddenByThisUI;
    private bool lastInputWasGamepad;
    private const float CursorMouseDeltaThresholdSq = 0.002f;
    private const float CursorGamepadStickThreshold = 0.2f;
    private bool skipSelectFirstUntilInput;
    private bool pendingClearSelection;
    private GameObject navigationBlocker;
    
    #if UNITY_EDITOR
    private void OnValidate()
    {
        if (!Application.isPlaying && !pendingLineUpdate)
        {
            pendingLineUpdate = true;
            
            UnityEditor.EditorApplication.delayCall += () =>
            {
                if (this != null && gameObject != null)
                {
                    pendingLineUpdate = false;
                    if (drawConnectionLines)
                    {
                        if (linesContainer == null)
                        {
                            CreateLinesContainer();
                        }
                        CreateConnectionLines();
                    }
                    else
                    {
                        ClearConnectionLines();
                    }
                }
                else
                {
                    pendingLineUpdate = false;
                }
            };
        }
    }
    #endif
    
    [Header("Line Renderer Options")]
    public bool drawConnectionLines = true;
    public Color lineColor = Color.white;
    public float lineWidth = 3f;
    
    [Header("Line Positioning")]
    [Tooltip("How much to offset the line from the center of skill items (0 = center, 1 = edge, >1 = beyond edge)")]
    [Range(0f, 2f)]
    public float lineRecess = 0.8f;
    
    [Header("Line Styling")]
    [Tooltip("Enable tapered line ends (pointed tips)")]
    public bool taperedEnds = true;
    [Tooltip("Enable rounded line ends (circular caps)")]
    public bool roundedEnds = false;
    [Tooltip("Line curve type")]
    public LineCurveType curveType = LineCurveType.Straight;
    [Tooltip("Curve intensity (-1 = curved left, 0 = straight, 1 = curved right)")]
    [Range(-1f, 1f)]
    public float curveIntensity = 0.3f;
    [Tooltip("Line resolution (higher = smoother curves, but more GameObjects)")]
    [Range(3, 20)]
    public int lineResolution = 5;
    
    
    private UILineRendererOptimized optimizedLineRenderer;
    private Transform linesContainer;
    private Canvas parentCanvas;
    private bool initialized = false;

    [Header("Debug Settings")]
    [SerializeField] private bool enableDebugLogs = false;
    
    private void DebugLog(string message)
    {
        #if UNITY_EDITOR
        if (enableDebugLogs)
        {
            Debug.Log($"[SkillTreeUI] {message}");
        }
        #endif
    }
    
    private void DebugLogWarning(string message)
    {
        #if UNITY_EDITOR
        if (enableDebugLogs)
        {
            Debug.LogWarning($"[SkillTreeUI] {message}");
        }
        #endif
    }

    [ContextMenu("Find All Skill Items")]
    public void FindAllSkillItems()
    {
        SkillItemUI[] items = GetComponentsInChildren<SkillItemUI>(true);
        skillItems.Clear();
        
        foreach (var item in items)
        {
            if (item != null)
            {
                skillItems.Add(item);
            }
        }
        
        DebugLog($"Found {skillItems.Count} skill items");
    }
    
    [ContextMenu("Force Refresh Connections")]
    public void ForceRefreshConnections()
    {
        DebugLog("Force refresh connections");
        
        if (drawConnectionLines)
        {
            CreateConnectionLines();
        }
    }
    
    [ContextMenu("Debug Connection Info")]
    public void DebugConnectionInfo()
    {
        DebugLog($"Skill Items Count: {skillItems.Count}");
        DebugLog($"Draw Connection Lines: {drawConnectionLines}");
        
        int skillsWithPrerequisites = 0;
        foreach (var item in skillItems)
        {
            if (item != null && item.skill != null && item.skill.prerequisites != null && item.skill.prerequisites.Count > 0)
            {
                skillsWithPrerequisites++;
            }
        }
        DebugLog($"Skills with prerequisites: {skillsWithPrerequisites}");
    }

    private void Awake()
    {
        parentCanvas = GetComponentInParent<Canvas>();
        if (parentCanvas == null)
            Debug.LogError("SkillTreeUI must be child of a Canvas!");
        if (skillTreeComponent == null)
        {
            Debug.LogError("SkillTreeUI: Missing SkillTreeComponent reference!");
            return;
        }
        if (skillItems.Count == 0)
        {
            FindAllSkillItems();
        }
    }

    private void Start()
    {
        if (Application.isPlaying)
        {
            RefreshAllSkillItems();
            StartCoroutine(SetupGamepadNavigationDelayed());
        }
    }
    
    public void EnsureTooltipReference()
    {
        if (sharedTooltip != null) return;
        sharedTooltip = GetComponentInChildren<SkillTooltip>(true);
    }
    
    private System.Collections.IEnumerator SetupGamepadNavigationDelayed()
    {
        yield return null;
        SelectFirstSkillItemIfNone();
    }
    
    // North = click active button. South = enter inner buttons or submit. East = exit to node.
    private void HandleSelectionModeSouthEast(EventSystem es)
    {
        GameObject selected = es.currentSelectedGameObject;
        if (selected == null || skillItems == null) return;
        
        bool northPressed = Gamepad.current != null && Gamepad.current.buttonNorth.wasPressedThisFrame;
        bool southPressed = (Gamepad.current != null && Gamepad.current.buttonSouth.wasPressedThisFrame) ||
            (Keyboard.current != null && (Keyboard.current.enterKey.wasPressedThisFrame || Keyboard.current.spaceKey.wasPressedThisFrame));
        bool eastPressed = (Gamepad.current != null && Gamepad.current.buttonEast.wasPressedThisFrame) ||
            (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame);
        
        if (northPressed)
        {
            SkillItemUI itemUnderSelection = selected.GetComponent<SkillItemUI>() ?? selected.GetComponentInParent<SkillItemUI>();
            if (itemUnderSelection != null && skillItems.Contains(itemUnderSelection))
            {
                itemUnderSelection.TryClickActiveButton();
                return;
            }
        }
        
        if (eastPressed)
        {
            SkillItemUI parentItem = selected.GetComponentInParent<SkillItemUI>();
            if (parentItem != null && skillItems.Contains(parentItem))
            {
                Selectable nodeSel = parentItem.GetNavigationSelectable();
                if (nodeSel != null)
                {
                    es.SetSelectedGameObject(nodeSel.gameObject);
                    return;
                }
            }
        }
        
        if (southPressed && selectionSouthEntersInnerButtons)
        {
            foreach (var item in skillItems)
            {
                if (item == null) continue;
                Selectable nodeSel = item.GetNavigationSelectable();
                if (nodeSel == null || nodeSel.gameObject != selected) continue;
                Selectable inner = item.GetFirstInteractableInnerSelectable();
                if (inner != null)
                    es.SetSelectedGameObject(inner.gameObject);
                return;
            }
        }
    }
    
    // Select first skill node if nothing in tree is selected (gamepad navigation).
    private void SelectFirstSkillItemIfNone()
    {
        if (skillItems == null || skillItems.Count == 0) return;
        EventSystem es = EventSystem.current;
        if (es == null) return;
        if (es.currentSelectedGameObject != null && es.currentSelectedGameObject.transform.IsChildOf(transform))
            return;
        Selectable first = null;
        foreach (var item in skillItems)
        {
            if (item == null) continue;
            first = item.GetNavigationSelectable();
            if (first != null && first.interactable) break;
        }
        if (first != null)
        {
            es.firstSelectedGameObject = first.gameObject;
            es.SetSelectedGameObject(first.gameObject);
        }
    }
    
    private void OnEnable()
    {
        #if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            if (!initialized && skillTreeComponent != null)
                Initialize();
        }
        #endif
        
        if (Application.isPlaying && skillTreeComponent != null)
        {
            skillTreeComponent.OnSkillUnlocked += OnSkillUnlocked;
            skillTreeComponent.ForceRefreshAllSkills += RefreshAllSkillItems;
            StartCoroutine(SelectFirstWhenEnabled());
        }
    }
    
    private System.Collections.IEnumerator SelectFirstWhenEnabled()
    {
        yield return null;
        yield return null;
        if (gameObject.activeInHierarchy && enabled)
            SelectFirstSkillItemIfNone();
    }
    
    private void Update()
    {
        if (!Application.isPlaying || !gameObject.activeInHierarchy || !enabled) return;
        EnsureTooltipReference();
        EventSystem es = EventSystem.current;
        if (es == null) return;
        
        UpdateCursorVisibility(es);
        
        if (gamepadControlMode == GamepadControlMode.Selection)
        {
            if (pendingClearSelection)
            {
                es.SetSelectedGameObject(null);
                es.firstSelectedGameObject = null;
                pendingClearSelection = false;
            }
            GamepadCursorScreenPosition = null;
            lastHoveredSkillItemForCursor = null;
            if (cursorRect != null) cursorRect.gameObject.SetActive(false);
            if (navigationBlocker != null && es.currentSelectedGameObject == navigationBlocker)
                es.SetSelectedGameObject(null);
            
            if (showTooltipOnSelection)
                UpdateTooltipForSelection(es);
            if (tooltipShownByHover && sharedTooltip != null && sharedTooltip.gameObject.activeSelf && Mouse.current != null)
            {
                var pointerData = new PointerEventData(es) { position = Mouse.current.position.ReadValue() };
                var results = new System.Collections.Generic.List<RaycastResult>();
                es.RaycastAll(pointerData, results);
                bool overSkillItem = false;
                for (int i = 0; i < results.Count; i++)
                {
                    var item = results[i].gameObject.GetComponent<SkillItemUI>() ?? results[i].gameObject.GetComponentInParent<SkillItemUI>();
                    if (item != null && item.skill != null) { overSkillItem = true; break; }
                }
                if (!overSkillItem)
                {
                    DestroyTooltip();
                    tooltipShownByHover = false;
                }
            }
            
            bool selectionInTree = es.currentSelectedGameObject != null && es.currentSelectedGameObject.transform.IsChildOf(transform);
            if (selectionInTree)
            {
                HandleSelectionModeSouthEast(es);
                return;
            }
            
            bool gamepadSubmit = Gamepad.current != null && (Gamepad.current.buttonSouth.wasPressedThisFrame || Gamepad.current.buttonEast.wasPressedThisFrame);
            bool gamepadMove = Gamepad.current != null && (Gamepad.current.leftStick.ReadValue().sqrMagnitude > 0.04f || Gamepad.current.dpad.ReadValue().sqrMagnitude > 0.01f);
            bool keyboardSubmit = Keyboard.current != null && (Keyboard.current.enterKey.wasPressedThisFrame || Keyboard.current.spaceKey.wasPressedThisFrame);
            bool keyboardMove = Keyboard.current != null && (Keyboard.current.wKey.wasPressedThisFrame || Keyboard.current.sKey.wasPressedThisFrame || Keyboard.current.aKey.wasPressedThisFrame || Keyboard.current.dKey.wasPressedThisFrame || Keyboard.current.upArrowKey.wasPressedThisFrame || Keyboard.current.downArrowKey.wasPressedThisFrame || Keyboard.current.leftArrowKey.wasPressedThisFrame || Keyboard.current.rightArrowKey.wasPressedThisFrame);
            
            if (gamepadMove || keyboardMove)
                skipSelectFirstUntilInput = false;
            if ((gamepadSubmit || gamepadMove || keyboardSubmit || keyboardMove) && skillItems != null && skillItems.Count > 0 && !skipSelectFirstUntilInput)
                SelectFirstSkillItemIfNone();
        }
        else if (gamepadControlMode == GamepadControlMode.Cursor)
        {
            if (parentCanvas == null)
                parentCanvas = GetComponentInParent<Canvas>();
            if (cursorRect == null && !cursorCreated)
                EnsureCursorCreated();
            
            bool mouseUsed = Mouse.current != null && (
                Mouse.current.delta.ReadValue().sqrMagnitude > 0.0001f ||
                Mouse.current.leftButton.wasPressedThisFrame ||
                Mouse.current.rightButton.wasPressedThisFrame);
            bool gamepadUsed = Gamepad.current != null && (
                Gamepad.current.leftStick.ReadValue().sqrMagnitude > 0.0001f ||
                Gamepad.current.dpad.ReadValue().sqrMagnitude > 0.0001f ||
                Gamepad.current.buttonSouth.wasPressedThisFrame ||
                Gamepad.current.buttonEast.wasPressedThisFrame);
            if (mouseUsed) cursorVisibleByGamepad = false;
            if (gamepadUsed) cursorVisibleByGamepad = true;
            
            if (cursorRect != null)
            {
                cursorRect.gameObject.SetActive(cursorVisibleByGamepad);
                if (cursorVisibleByGamepad)
                    cursorRect.SetAsLastSibling();
            }
            
            Gamepad current = Gamepad.current;
            if (current != null && cursorVisibleByGamepad)
            {
                Vector2 stick = current.leftStick.ReadValue();
                Vector2 dpad = current.dpad.ReadValue();
                Vector2 move = stick.sqrMagnitude > dpad.sqrMagnitude ? stick : dpad;
                float dt = Time.unscaledDeltaTime;
                cursorScreenPosition += move * cursorSpeed * dt;
                cursorScreenPosition.x = Mathf.Clamp(cursorScreenPosition.x, 0, Screen.width);
                cursorScreenPosition.y = Mathf.Clamp(cursorScreenPosition.y, 0, Screen.height);
                GamepadCursorScreenPosition = cursorScreenPosition;
                
                if (current.buttonSouth.wasPressedThisFrame || current.buttonEast.wasPressedThisFrame)
                    SubmitAtCursor(es);
            }
            else
            {
                GamepadCursorScreenPosition = null;
            }
            
            if (cursorRect != null && cursorVisibleByGamepad && parentCanvas != null)
            {
                RectTransformUtility.ScreenPointToLocalPointInRectangle(parentCanvas.transform as RectTransform, cursorScreenPosition, parentCanvas.worldCamera, out Vector2 local);
                cursorRect.anchoredPosition = local;
            }
            
            if (cursorVisibleByGamepad)
                UpdateTooltipHoverForCursor(es);
            else if (Mouse.current != null)
            {
                cursorScreenPosition = Mouse.current.position.ReadValue();
                UpdateTooltipHoverForCursor(es);
            }
            else
                lastHoveredSkillItemForCursor = null;
        }
    }
    
    private void UpdateCursorVisibility(EventSystem es)
    {
        bool mouseUsed = Mouse.current != null && (
            Mouse.current.delta.ReadValue().sqrMagnitude > CursorMouseDeltaThresholdSq ||
            Mouse.current.leftButton.wasPressedThisFrame ||
            Mouse.current.rightButton.wasPressedThisFrame);
        Vector2 stick = Gamepad.current != null ? Gamepad.current.leftStick.ReadValue() : default;
        Vector2 dpad = Gamepad.current != null ? Gamepad.current.dpad.ReadValue() : default;
        bool gamepadButtons = Gamepad.current != null && (
            Gamepad.current.buttonSouth.wasPressedThisFrame ||
            Gamepad.current.buttonEast.wasPressedThisFrame ||
            Gamepad.current.buttonNorth.wasPressedThisFrame ||
            Gamepad.current.buttonWest.wasPressedThisFrame);
        bool gamepadStickOrDpad = Gamepad.current != null && (
            stick.sqrMagnitude > CursorGamepadStickThreshold * CursorGamepadStickThreshold ||
            dpad.sqrMagnitude > CursorGamepadStickThreshold * CursorGamepadStickThreshold);
        bool gamepadUsed = gamepadButtons || gamepadStickOrDpad;
        
        bool inputIsGamepad = gamepadUsed ? true : (mouseUsed ? false : lastInputWasGamepad);
        if ((mouseUsed || gamepadUsed) && inputIsGamepad != lastInputWasGamepad)
        {
            if (sharedTooltip != null)
                sharedTooltip.Hide();
            if (inputIsGamepad)
            {
                lastSelectedForTooltip = null;
                lastHoveredSkillItemForCursor = null;
            }
            else
            {
                lastHoveredSkillItemForCursor = null;
                lastSelectedForTooltip = es != null ? es.currentSelectedGameObject : null;
                if (es != null)
                {
                    if (es.currentSelectedGameObject != null && es.currentSelectedGameObject.transform.IsChildOf(transform))
                    {
                        es.firstSelectedGameObject = null;
                        es.SetSelectedGameObject(null);
                    }
                }
            }
        }
        lastInputWasGamepad = inputIsGamepad;
        
        if (!hideCursorOnGamepad)
        {
            if (cursorHiddenByThisUI)
            {
                Cursor.visible = true;
                cursorHiddenByThisUI = false;
            }
            return;
        }
        if (mouseUsed)
        {
            Cursor.visible = true;
            cursorHiddenByThisUI = false;
        }
        else if (gamepadUsed)
        {
            Cursor.visible = false;
            cursorHiddenByThisUI = true;
        }
    }
    
    private void LateUpdate()
    {
        if (!Application.isPlaying || !gameObject.activeInHierarchy || !enabled) return;
        if (gamepadControlMode != GamepadControlMode.Cursor) return;
        if (!cursorVisibleByGamepad) return;
        EventSystem es = EventSystem.current;
        if (es == null) return;
        EnsureNavigationBlocker();
        if (navigationBlocker != null)
            es.SetSelectedGameObject(navigationBlocker);
    }
    
    // In Cursor mode: invisible selectable so gamepad doesn't focus nodes while moving virtual cursor.
    private void EnsureNavigationBlocker()
    {
        if (navigationBlocker != null) return;
        Canvas can = parentCanvas != null ? parentCanvas : GetComponentInParent<Canvas>();
        if (can == null) return;
        navigationBlocker = new GameObject("SkillTree_NavigationBlocker");
        navigationBlocker.transform.SetParent(can.transform, false);
        var rt = navigationBlocker.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.zero;
        rt.sizeDelta = Vector2.zero;
        rt.anchoredPosition = Vector2.zero;
        var img = navigationBlocker.AddComponent<Image>();
        img.color = new Color(0, 0, 0, 0);
        img.raycastTarget = false;
        var btn = navigationBlocker.AddComponent<Button>();
        btn.interactable = false;
        btn.navigation = new Navigation { mode = Navigation.Mode.None };
    }
    
    private void EnsureCursorCreated()
    {
        cursorCreated = true;
        if (cursorGraphic != null)
        {
            cursorRect = cursorGraphic;
            cursorRect.gameObject.SetActive(true);
            var img = cursorRect.GetComponent<UnityEngine.UI.Image>();
            if (img != null) img.raycastTarget = false;
        }
        else if (parentCanvas != null)
        {
            GameObject go = new GameObject("GamepadCursor");
            go.transform.SetParent(parentCanvas.transform, false);
            RectTransform rt = go.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(24, 24);
            UnityEngine.UI.Image img = go.AddComponent<UnityEngine.UI.Image>();
            img.color = new Color(1f, 1f, 1f, 0.9f);
            img.raycastTarget = false;
            cursorRect = rt;
        }
        cursorScreenPosition = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
    }
    
    private void UpdateTooltipHoverForCursor(EventSystem es)
    {
        if (sharedTooltip == null) return;
        var pointerData = new PointerEventData(es) { position = cursorScreenPosition };
        var results = new System.Collections.Generic.List<RaycastResult>();
        es.RaycastAll(pointerData, results);
        SkillItemUI underCursor = null;
        for (int i = 0; i < results.Count; i++)
        {
            GameObject go = results[i].gameObject;
            if (cursorRect != null && (go == cursorRect.gameObject || go.transform.IsChildOf(cursorRect)))
                continue;
            SkillItemUI item = go.GetComponent<SkillItemUI>() ?? go.GetComponentInParent<SkillItemUI>();
            if (item != null && item.skill != null)
            {
                underCursor = item;
                break;
            }
        }
        if (underCursor != lastHoveredSkillItemForCursor)
        {
            lastHoveredSkillItemForCursor = underCursor;
            if (underCursor != null && skillTreeComponent != null)
            {
                sharedTooltip.Show(underCursor.skill, skillTreeComponent, cursorScreenPosition, underCursor.TooltipOffset);
                sharedTooltip.SetPositionLocked(false);
            }
            else
            {
                if (sharedTooltip != null)
                {
                    sharedTooltip.Hide();
                    sharedTooltip.SetPositionLocked(false);
                }
            }
        }
        if (underCursor != null && sharedTooltip != null && sharedTooltip.gameObject.activeSelf)
            sharedTooltip.UpdatePosition();
    }
    
    private void SubmitAtCursor(EventSystem es)
    {
        var pointerData = new PointerEventData(es)
        {
            position = cursorScreenPosition,
            button = PointerEventData.InputButton.Left,
            clickCount = 1,
            pressPosition = cursorScreenPosition
        };
        var results = new System.Collections.Generic.List<RaycastResult>();
        es.RaycastAll(pointerData, results);
        GameObject target = null;
        for (int i = 0; i < results.Count; i++)
        {
            GameObject go = results[i].gameObject;
            if (cursorRect != null && (go == cursorRect.gameObject || go.transform.IsChildOf(cursorRect)))
                continue;
            Button btn = go.GetComponent<Button>() ?? go.GetComponentInParent<Button>();
            if (btn != null)
            {
                target = btn.gameObject;
                break;
            }
            SkillItemUI item = go.GetComponent<SkillItemUI>() ?? go.GetComponentInParent<SkillItemUI>();
            if (item != null && item.skill != null)
            {
                target = item.gameObject;
                break;
            }
        }
        if (target == null) return;
        pointerData.pointerPress = target;
        ExecuteEvents.Execute(target, pointerData, ExecuteEvents.pointerDownHandler);
        ExecuteEvents.Execute(target, pointerData, ExecuteEvents.pointerUpHandler);
        ExecuteEvents.Execute(target, pointerData, ExecuteEvents.pointerClickHandler);
    }
    
    private void UpdateTooltipForSelection(EventSystem es)
    {
        GameObject selected = es.currentSelectedGameObject;
        if (selected == lastSelectedForTooltip) return;
        lastSelectedForTooltip = selected;
        
        if (selected == null || !selected.transform.IsChildOf(transform))
        {
            tooltipShownByHover = false;
            if (sharedTooltip != null)
            {
                sharedTooltip.Hide();
                sharedTooltip.SetPositionLocked(false);
            }
            return;
        }
        
        SkillItemUI item = selected.GetComponent<SkillItemUI>() ?? selected.GetComponentInParent<SkillItemUI>();
        if (item == null || item.skill == null || skillTreeComponent == null)
        {
            tooltipShownByHover = false;
            if (sharedTooltip != null)
            {
                sharedTooltip.Hide();
                sharedTooltip.SetPositionLocked(false);
            }
            return;
        }
        
        if (sharedTooltip == null) return;
        
        RectTransform rect = selected.GetComponent<RectTransform>();
        if (rect == null) rect = selected.GetComponentInParent<RectTransform>();
        Vector2 screenPos = Vector2.zero;
        if (rect != null && parentCanvas != null)
            screenPos = RectTransformUtility.WorldToScreenPoint(parentCanvas.worldCamera, rect.TransformPoint(rect.rect.center));
        
        tooltipShownByHover = false;
        sharedTooltip.Show(item.skill, skillTreeComponent, screenPos, item.TooltipOffset);
        sharedTooltip.SetPositionLocked(true);
    }

    private void OnDisable()
    {
        if (cursorHiddenByThisUI)
        {
            Cursor.visible = true;
            cursorHiddenByThisUI = false;
        }
        GamepadCursorScreenPosition = null;
        lastSelectedForTooltip = null;
        lastHoveredSkillItemForCursor = null;
        if (sharedTooltip != null)
            sharedTooltip.SetPositionLocked(false);
        if (skillTreeComponent != null)
        {
            skillTreeComponent.OnSkillUnlocked -= OnSkillUnlocked;
            skillTreeComponent.ForceRefreshAllSkills -= RefreshAllSkillItems;
        }
        
        #if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            ClearConnectionLines();
        }
        #endif
    }
    
    // Assign skills to UI items, create lines container and connections (editor/play).
    private void Initialize()
    {
        if (initialized) return;
        CreateLinesContainer();
        InitializeSkillItems();
        #if UNITY_EDITOR
        if (!Application.isPlaying && drawConnectionLines)
            CreateConnectionLines();
        #endif
        RefreshAllSkillItems();
        initialized = true;
    }
    
    private void CreateLinesContainer()
    {
        linesContainer = transform.Find("ConnectionLines");
        if (linesContainer == null)
        {
            GameObject containerGO = new GameObject("ConnectionLines");
            linesContainer = containerGO.transform;
            linesContainer.SetParent(transform, false);
            RectTransform rectTransform = containerGO.AddComponent<RectTransform>();
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.sizeDelta = Vector2.zero;
            rectTransform.anchoredPosition = Vector2.zero;
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            linesContainer.SetAsFirstSibling();
            DebugLog("Created lines container");
        }
        else
        {
            RectTransform rectTransform = linesContainer.GetComponent<RectTransform>();
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.sizeDelta = Vector2.zero;
            rectTransform.anchoredPosition = Vector2.zero;
        }
    }
    
    private void InitializeSkillItems()
    {
        foreach (var item in skillItems)
        {
            if (item != null)
            {
                if (!item.gameObject.activeSelf)
                {
                    item.gameObject.SetActive(true);
                }
                
                if (item.GetSkillTreeComponent() == null)
                {
                    DebugLogWarning($"{item.name} needs SkillTreeComponent reference configured in Inspector");
                }
            }
        }
    }
    
    
    /// <summary>
    /// Create visual connection lines between skills and their prerequisites
    /// Lines are created in Editor Mode and become part of the scene
    /// </summary>
    private void CreateConnectionLines()
    {
        if (skillItems.Count == 0) return;
        
        if (linesContainer == null)
        {
            CreateLinesContainer();
        }
        
        // Clear existing lines first to avoid duplicates
        ClearConnectionLines();
        
        // Always use optimized renderer
        CreateConnectionLinesOptimized();
    }
    
    
    
    private void RefreshConnectionLines()
    {
        if (!drawConnectionLines)
            return;
            
        Dictionary<Skill, SkillItemUI> skillToUIMap = new Dictionary<Skill, SkillItemUI>();
        foreach (var item in skillItems)
        {
            if (item != null && item.skill != null)
            {
                skillToUIMap[item.skill] = item;
            }
        }
        
        
        foreach (var item in skillItems)
        {
            if (item == null || item.skill == null || item.skill.prerequisites == null)
                continue;
                
            bool skillIsUnlocked = skillTreeComponent.IsUnlocked(item.skill);
        }
    }
    
    private void OnSkillUnlocked(Skill skill)
    {
        // Refresh all skills to update their state
        RefreshAllSkillItems();
    }
    
    public void RefreshAllSkillItems()
    {
        // First check if any skill items are deactivated and activate them
        foreach (var item in skillItems)
        {
            if (item != null && !item.gameObject.activeSelf)
            {
                item.gameObject.SetActive(true);
            }
        }
        
        // Refresh each individual skill item
        foreach (var item in skillItems)
        {
            if (item != null)
            {
                // Make sure the gameObject is active
                if (!item.gameObject.activeSelf)
                {
                    item.gameObject.SetActive(true);
                }
                item.Refresh();
            }
        }
        
        // Refresh connection lines only in Editor Mode
        #if UNITY_EDITOR
        if (!Application.isPlaying && drawConnectionLines)
        {
            RefreshConnectionLines();
        }
        #endif
    }
    
    /// <summary>
    /// Create connection lines using optimized single-mesh renderer
    /// </summary>
    private void CreateConnectionLinesOptimized()
    {
        // Find existing renderer first
        if (linesContainer != null && linesContainer.childCount > 0)
        {
            optimizedLineRenderer = linesContainer.GetComponentInChildren<UILineRendererOptimized>();
            if (optimizedLineRenderer != null)
            {
                // Reuse existing renderer
                optimizedLineRenderer.ClearLines();
            }
        }
        
        // Only create new if doesn't exist
        if (optimizedLineRenderer == null)
        {
            // Clear all children from lines container
            if (linesContainer != null)
            {
                for (int i = linesContainer.childCount - 1; i >= 0; i--)
                {
                    Transform child = linesContainer.GetChild(i);
                    if (Application.isPlaying)
                    {
                        Destroy(child.gameObject);
                    }
                    else
                    {
                        DestroyImmediate(child.gameObject);
                    }
                }
            }
            
            // Create new optimized renderer
            GameObject rendererGO = new GameObject("OptimizedLineRenderer");
            rendererGO.transform.SetParent(linesContainer, false);
            
            RectTransform rectTransform = rendererGO.AddComponent<RectTransform>();
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.sizeDelta = Vector2.zero;
            rectTransform.anchoredPosition = Vector2.zero;
            
            optimizedLineRenderer = rendererGO.AddComponent<UILineRendererOptimized>();
            optimizedLineRenderer.raycastTarget = false;
        }
        
        // Create mapping
        Dictionary<Skill, SkillItemUI> skillToUIMap = new Dictionary<Skill, SkillItemUI>();
        foreach (var item in skillItems)
        {
            if (item != null && item.skill != null)
            {
                skillToUIMap[item.skill] = item;
            }
        }
        
        int linesCreated = 0;
        
        // Add lines to optimized renderer
        foreach (var item in skillItems)
        {
            if (item == null || item.skill == null || item.skill.prerequisites == null)
                continue;
            
            foreach (var prereq in item.skill.prerequisites)
            {
                if (prereq == null || prereq.skill == null)
                    continue;
                
                if (skillToUIMap.TryGetValue(prereq.skill, out SkillItemUI prereqUI))
                {
                    RectTransform startRect = prereqUI.GetComponent<RectTransform>();
                    RectTransform endRect = item.GetComponent<RectTransform>();
                    
                    optimizedLineRenderer.AddLine(startRect, endRect, lineColor, lineWidth, lineRecess, 
                                                 taperedEnds, roundedEnds, curveType, curveIntensity, lineResolution);
                    linesCreated++;
                }
            }
        }
    }
    
    private void ClearConnectionLines()
    {
        // Clear optimized renderer
        if (optimizedLineRenderer != null)
        {
            optimizedLineRenderer.ClearLines();
        }
    }
    
    public void DestroyTooltip()
    {
        if (sharedTooltip != null)
            sharedTooltip.Hide();
        tooltipShownByHover = false;
    }
    
    // Called by SkillItemUI when tooltip is shown on hover; used to hide when cursor leaves.
    public void NotifyTooltipShownByHover()
    {
        tooltipShownByHover = true;
    }
    
    // Clears selection and tooltip. Call after Unlock/Level Up/Refund so focus doesn't jump to first node; next stick/d-pad input will allow selecting again.
    public void ClearSelectionAndTooltip()
    {
        lastSelectedForTooltip = null;
        if (sharedTooltip != null)
            sharedTooltip.Hide();
        EventSystem es = EventSystem.current;
        if (es != null)
        {
            if (es.currentSelectedGameObject != null && es.currentSelectedGameObject.transform.IsChildOf(transform))
            {
                es.firstSelectedGameObject = null;
                es.SetSelectedGameObject(null);
            }
        }
        skipSelectFirstUntilInput = true;
        pendingClearSelection = true;
    }
}
}
