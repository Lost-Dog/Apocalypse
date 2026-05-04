using UnityEngine;
using System.Collections;
using GameCreator.Runtime.Inventory.UnityUI;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class RotateGameObjects : MonoBehaviour
{
    public enum InputMode
    {
        Legacy,
        New,
        Both
    }

    [Header("Rotation Settings")]
    public GameObject[] gameObjects;
    public int middleIndex = 1;
    public float timeout = 2f;

    [Header("Legacy Input")]
    public KeyCode scrollUpKey = KeyCode.Mouse3;
    public KeyCode scrollDownKey = KeyCode.Mouse4;

    [Header("Select Input System + New Input")]
    public InputMode inputMode = InputMode.Both;

#if ENABLE_INPUT_SYSTEM
    [Tooltip("Drag in your ScrollWheel Vector2 action (Asset or Inline)")]
    public InputActionProperty scrollAction;
#endif

    private CanvasGroup canvasGroup;
    private Coroutine fadeCoroutine;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            Debug.LogWarning("CanvasGroup missing on " + name);
    }

    private void OnEnable()
    {
        ApplyVisuals();

#if ENABLE_INPUT_SYSTEM
        if ((inputMode == InputMode.New || inputMode == InputMode.Both)
            && scrollAction != null && scrollAction.action != null)
        {
            scrollAction.action.Enable();
            scrollAction.action.performed += OnScrollPerformed;
        }
#endif
    }

    private void OnDisable()
    {
#if ENABLE_INPUT_SYSTEM
        if (scrollAction != null && scrollAction.action != null)
        {
            scrollAction.action.performed -= OnScrollPerformed;
            scrollAction.action.Disable();
        }
#endif
    }

    private void Update()
    {
        // ─── Legacy input ─────────────────────────────────────────
#if ENABLE_LEGACY_INPUT_MANAGER
        if (inputMode != InputMode.New)
        {
            if (Input.GetKeyDown(scrollUpKey) || Input.mouseScrollDelta.y > 0f)
                RotateObjects(true);
            else if (Input.GetKeyDown(scrollDownKey) || Input.mouseScrollDelta.y < 0f)
                RotateObjects(false);
        }
#endif

        // ─── New input is handled in OnScrollPerformed() ─────────
    }

#if ENABLE_INPUT_SYSTEM
    private void OnScrollPerformed(InputAction.CallbackContext ctx)
    {
        if (inputMode != InputMode.Legacy)
        {
            Vector2 delta = ctx.ReadValue<Vector2>();
            if (delta.y > 0f) RotateObjects(true);
            else if (delta.y < 0f) RotateObjects(false);
        }
    }
#endif

    private void RotateObjects(bool up)
    {
        middleIndex = up
            ? (middleIndex - 1 + gameObjects.Length) % gameObjects.Length
            : (middleIndex + 1) % gameObjects.Length;

        UpdateObjectProperties();

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
            if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
            fadeCoroutine = StartCoroutine(FadeOutCanvasGroup());
        }
    }

    private void UpdateObjectProperties()
    {
        for (int i = 0; i < gameObjects.Length; i++)
        {
            var cg = gameObjects[i].GetComponent<CanvasGroup>();
            var rt = gameObjects[i].GetComponent<RectTransform>();
            var ui = gameObjects[i].GetComponent<BagEquipUI>();

            if (cg != null) { cg.alpha = 0.6f; cg.interactable = false; cg.blocksRaycasts = false; }
            if (rt != null) rt.sizeDelta = new Vector2(100, 100);
            if (ui != null) ui.enabled = false;
        }

        int prev = (middleIndex - 1 + gameObjects.Length) % gameObjects.Length;
        int next = (middleIndex + 1) % gameObjects.Length;

        var mCg = gameObjects[middleIndex].GetComponent<CanvasGroup>();
        var mRt = gameObjects[middleIndex].GetComponent<RectTransform>();
        var mUi = gameObjects[middleIndex].GetComponent<BagEquipUI>();

        if (mCg != null) { mCg.alpha = 1f; mCg.interactable = true; mCg.blocksRaycasts = true; }
        if (mRt != null) mRt.sizeDelta = new Vector2(150, 150);
        if (mUi != null) mUi.enabled = true;

        gameObjects[prev].transform.SetSiblingIndex(0);
        gameObjects[middleIndex].transform.SetSiblingIndex(1);
        gameObjects[next].transform.SetSiblingIndex(2);
    }

    private IEnumerator FadeOutCanvasGroup()
    {
        yield return new WaitForSeconds(timeout);

        float fadeDuration = 1f;
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
            yield return null;
        }

        canvasGroup.alpha = 0f;
    }

    // just to keep OnEnable nice and tidy
    private void ApplyVisuals() => UpdateObjectProperties();
}
