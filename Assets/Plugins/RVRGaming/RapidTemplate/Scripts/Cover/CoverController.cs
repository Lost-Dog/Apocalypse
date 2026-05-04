using UnityEngine;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.Characters;
using GameCreator.Runtime.VisualScripting;
using System.Threading.Tasks;
using RT;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

[AddComponentMenu("Game Creator/Mechanics/Character Cover Controller")]
public class CoverController : MonoBehaviour
{
    public enum InputMode
    {
        Legacy,
        New,
        Both
    }

    [Header("Player Settings")]
    public bool Player = true;

    [Header("Legacy Input")]
    public KeyCode coverInput = KeyCode.C;
    public KeyCode vaultInput = KeyCode.V;

    [Header("Select Input System + New Input")]
    public InputMode inputMode = InputMode.Both;
    #if ENABLE_INPUT_SYSTEM
    [Tooltip("Drag in or create an Inline Vector2 action for movement (e.g. Move)")]
    public InputActionProperty moveAction;
    [Tooltip("Drag in or create an Inline Vector/Button action for Cover")]
    public InputActionProperty coverAction;
    [Tooltip("Drag in or create an Inline Vector/Button action for Vault")]
    public InputActionProperty vaultAction;
    #endif

    [Header("Switch Cover Settings")]
    public float switchRange = 10f;
    public GameObject switchMarker;
    public GameObject switchMarkerUIPrefab;
    [SerializeField] private PropertySetGameObject m_Set = SetGameObjectNone.Create;

    [Header("Switch Instructions")]
    [SerializeField] public InstructionList onSwitchCover = new InstructionList();

    [Header("Early Release Instructions")]
    [SerializeField] public InstructionList onEarlyRelease = new InstructionList();
    [HideInInspector] [SerializeField] private PropertyGetGameObject m_ActionsToStop = GetGameObjectActions.Create();

    private Character character;
    private Cover currentCover;
    private bool isInCover = false;
    private Vector3 coverDirection;
    private bool disableFacingCover = false;
    private bool isHoldingSwitchButton = false;
    private bool switchInProgress = false;
    private float buttonHoldTime = 0f;
    private float requiredHoldTime = 0.2f;
    private GameObject switchMarkerUIInstance;
    private bool isAtEdge;
    private bool isAtLeftEdge;

    private Camera mainCamera;
    private GameObject activeSwitchMarker;

    private void Start()
    {
        character = GetComponent<Character>();
        mainCamera = Camera.main;
    }

    public bool IsInCover()
    {
        return isInCover;
    }

    public Cover GetCurrentCover()
    {
        return currentCover;
    }

    public void SetAtEdge(bool atEdge, bool isLeftEdge)
    {
        isAtEdge = atEdge;
        this.isAtLeftEdge = isLeftEdge;
    }

    public bool IsAtEdge()
    {
        return isAtEdge;
    }

    public bool IsAtLeftEdge()
    {
        return isAtEdge && isAtLeftEdge;
    }

    public bool IsAtRightEdge()
    {
        return isAtEdge && !isAtLeftEdge;
    }

    private bool isButtonHeld;

    private void OnEnable()
    {
    #if ENABLE_INPUT_SYSTEM
        if (inputMode != InputMode.Legacy)
        {
            if (coverAction.action != null) coverAction.action.Enable();
            if (vaultAction.action != null) vaultAction.action.Enable();
        }
    #endif
    }

    private void OnDisable()
    {
    #if ENABLE_INPUT_SYSTEM
        if (coverAction.action != null) coverAction.action.Disable();
        if (vaultAction.action != null) vaultAction.action.Disable();
    #endif
    }

    private void Update()
    {
        // ─── LEGACY INPUT ───────────────────────────────
    #if ENABLE_LEGACY_INPUT_MANAGER
        if (inputMode != InputMode.New)
        {
            // press down
            if (Input.GetKeyDown(coverInput))
            {
                BeginCoverHold();
            }

            // holding
            if (isButtonHeld)
            {
                buttonHoldTime += Time.deltaTime;
                if (buttonHoldTime >= requiredHoldTime && !switchInProgress)
                {
                    isHoldingSwitchButton = true;
                    switchInProgress = true;
                    _ = TrySwitchCover();
                }
            }

            // released
            if (Input.GetKeyUp(coverInput))
            {
                EndCoverHold();
            }

            // vault
            if (Input.GetKeyDown(vaultInput))
                _ = TryVault();
        }
    #endif

        // ─── NEW INPUT ──────────────────────────────────
    #if ENABLE_INPUT_SYSTEM
        if (inputMode != InputMode.Legacy && coverAction.action != null)
        {
            bool pressed = coverAction.action.ReadValue<float>() > 0f;

            if (pressed && !isButtonHeld)
            {
                BeginCoverHold();
            }

            if (isButtonHeld)
            {
                buttonHoldTime += Time.deltaTime;
                if (buttonHoldTime >= requiredHoldTime && !switchInProgress)
                {
                    isHoldingSwitchButton = true;
                    switchInProgress = true;
                    _ = TrySwitchCover();
                }
            }

            if (!pressed && isButtonHeld)
            {
                EndCoverHold();
            }
        }

        if (inputMode != InputMode.Legacy && vaultAction.action?.triggered == true)
        {
            _ = TryVault();
        }
#endif

        if (isInCover)
        {
            if (!disableFacingCover)
            {
                StickToCover();
            }

            if (currentCover != null)
            {
                StickToCover();
                ConstrainMovementAndRotation();
            }

            PerformCoverSwitchRaycast();
        }
    }

    private void BeginCoverHold()
    {
        isButtonHeld = true;
        isHoldingSwitchButton = false;
        switchInProgress = false;
        buttonHoldTime = 0f;
    }

    private async void EndCoverHold()
    {
        isButtonHeld = false;

        if (buttonHoldTime < requiredHoldTime)
        {
            if (isInCover) await ExitCover();
            else await TryEnterCover();
        }
        else
        {
            isHoldingSwitchButton = false;
            RunEarlyReleaseInstructions();
        }

        switchInProgress = false;
        buttonHoldTime = 0f;
    }

    private void OnVaultPerformed(InputAction.CallbackContext ctx)
    {
        if (inputMode != InputMode.Legacy)
            _ = TryVault();
    }

    private void PerformCoverSwitchRaycast()
    {
        if (!Player)
        {
            DestroyActiveSwitchMarker();
            return;
        }

        Ray ray = mainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, switchRange))
        {
            Cover hitCover = hit.collider.GetComponent<Cover>();

            if (hitCover != null && hitCover != currentCover)
            {
                if (switchMarker != null)
                {
                    if (activeSwitchMarker == null)
                    {
                        activeSwitchMarker = Instantiate(switchMarker, hit.point, Quaternion.identity);
                        SetSwitchMarkerGameObject(activeSwitchMarker);

                        if (switchMarkerUIPrefab != null)
                        {
                            switchMarkerUIInstance = Instantiate(switchMarkerUIPrefab, activeSwitchMarker.transform);
                        }
                    }
                    else
                    {
                        activeSwitchMarker.transform.position = hit.point;
                        SetSwitchMarkerGameObject(activeSwitchMarker);
                    }
                }
            }
            else
            {
                DestroyActiveSwitchMarker();
            }
        }
        else
        {
            DestroyActiveSwitchMarker();
        }
    }

    private void DestroyActiveSwitchMarker()
    {
        if (switchInProgress) return;

        if (activeSwitchMarker != null)
        {
            Destroy(activeSwitchMarker);
            activeSwitchMarker = null;
        }

        if (switchMarkerUIInstance != null)
        {
            Destroy(switchMarkerUIInstance);
            switchMarkerUIInstance = null;
        }
    }

    private void SetSwitchMarkerGameObject(GameObject marker)
    {
        Args args = new Args(this.gameObject);
        this.m_Set.Set(marker, args);
    }

    private async Task TrySwitchCover()
    {
        if (!Player || !isHoldingSwitchButton) return;
        if (activeSwitchMarker == null || !isHoldingSwitchButton || buttonHoldTime < requiredHoldTime) return;

        Ray ray = mainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, switchRange))
        {
            Cover newCover = hit.collider.GetComponent<Cover>();

            if (newCover != null && newCover == currentCover) return;

            if (newCover != null)
            {
                Vector3 bodyRayStart = character.transform.position + Vector3.up * -0.25f;
                Vector3 directionToMarker = (activeSwitchMarker.transform.position - bodyRayStart).normalized;

                if (IsObstructedByAnyCover(bodyRayStart, directionToMarker, newCover))
                {
                    await TryVault();
                }

                await ExitCover();
                await RunSwitchCoverInstructions();
                currentCover = newCover;

                switchInProgress = false;
            }
        }
    }

    private bool IsObstructedByAnyCover(Vector3 startPosition, Vector3 directionToMarker, Cover newCover)
    {
        RaycastHit[] hits = Physics.RaycastAll(startPosition, directionToMarker, switchRange);

        System.Array.Sort(hits, (x, y) => x.distance.CompareTo(y.distance));

        foreach (var hit in hits)
        {
            if (hit.collider.CompareTag("Cover") && hit.collider.GetComponent<Cover>() != newCover)
            {
                return true;
            }

            if (hit.collider.GetComponent<Cover>() == newCover)
            {
                return false;
            }
        }

        return false;
    }

    private void OnDrawGizmos()
    {
        if (isInCover && activeSwitchMarker != null)
        {
            Vector3 rayStart = character.transform.position + Vector3.up * -0.25f;
            Vector3 bodyRayDirection = (activeSwitchMarker.transform.position - rayStart).normalized;

            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(rayStart, activeSwitchMarker.transform.position);

            RaycastHit[] hits = Physics.RaycastAll(rayStart, bodyRayDirection, switchRange);
            foreach (var hit in hits)
            {
                Gizmos.color = (hit.collider.CompareTag("Cover")) ? Color.red : Color.green;
                Gizmos.DrawLine(rayStart, hit.point);
            }
        }
    }

    private async Task RunSwitchCoverInstructions()
    {
        await this.onSwitchCover.Run(new Args(this.gameObject));
    }

    private void RunEarlyReleaseInstructions()
    {
        if (!switchInProgress) return;
        _ = this.onEarlyRelease.Run(new Args(this.gameObject));
        DestroyActiveSwitchMarker();
    }

    public async Task TryEnterCover()
    {
        Cover[] covers = FindObjectsOfType<Cover>();
        foreach (Cover cover in covers)
        {
            if (cover.IsCoverInReach(character.transform.position))
            {
                currentCover = cover;
                await cover.EnterCover(character);
                isInCover = true;
                coverDirection = (cover.transform.right).normalized;
                break;
            }
        }
    }

    public async Task TryVault()
    {
        if (currentCover != null && currentCover.CanVault(character.transform.position))
        {
            UnstickFromCover();
            await currentCover.VaultCover(character);
            isInCover = false;
            currentCover = null;
        }
        else
        {
            Cover[] covers = FindObjectsOfType<Cover>();
            foreach (Cover cover in covers)
            {
                if (cover.IsCoverInReach(character.transform.position) && cover.CanVault(character.transform.position))
                {
                    currentCover = cover;
                    UnstickFromCover();
                    await currentCover.VaultCover(character);
                    isInCover = false;
                    currentCover = null;
                    break;
                }
            }
        }
    }

    public async Task ExitCover()
    {
        UnstickFromCover();
        DestroyActiveSwitchMarker();
        await ExitCoverState();
        currentCover = null;
    }

    private void UnstickFromCover()
    {
        if (currentCover != null)
        {
            isInCover = false;
        }
    }

    private async Task ExitCoverState()
    {
        if (currentCover != null)
        {
            await currentCover.ExitCover(character);
        }
    }

    private void StickToCover()
    {
        if (currentCover != null)
        {
            Vector3 coverPosition = currentCover.GetClosestPoint(character.transform.position);
            character.transform.position = coverPosition;
        }
    }

    public void TransitionToCover(Cover newCover)
    {
        if (newCover != null && newCover != currentCover)
        {
            UnstickFromCover();

            if (newCover.IsHighCover())
            {
                SwitchToHighCoverState();
            }
            else
            {
                SwitchToLowCoverState();
            }

            currentCover = newCover;

            StickToCover();

            coverDirection = currentCover.transform.right.normalized;

            isInCover = true;
        }
    }

    private void SwitchToHighCoverState()
    {
        ConfigState highCoverConfig = new ConfigState(
            0f, 1f, 1f,
            currentCover.highStateTransitionIn,
            currentCover.highStateTransitionOut
        );

        character.States.SetState(
            currentCover.highCoverState,
            currentCover.highCoverStateLayer,
            BlendMode.Blend,
            highCoverConfig
        );
    }

    private void SwitchToLowCoverState()
    {
        ConfigState lowCoverConfig = new ConfigState(
            0f, 1f, 1f,
            currentCover.stateTransitionIn,
            currentCover.stateTransitionOut
        );

        character.States.SetState(
            currentCover.coverState,
            currentCover.coverStateLayer,
            BlendMode.Blend,
            lowCoverConfig
        );
    }

    private void ConstrainMovementAndRotation()
    {
        if (currentCover == null) return;

        float horizontalInput = 0f;
    #if ENABLE_INPUT_SYSTEM
        if (inputMode != InputMode.Legacy && moveAction.action != null)
        {
            horizontalInput = moveAction.action.ReadValue<Vector2>().x;
        }
    #endif

    #if ENABLE_LEGACY_INPUT_MANAGER
        if (inputMode != InputMode.New)
        {
            // If pure Legacy, or if Both & no new-system input detected:
            if (inputMode == InputMode.Legacy 
                || Mathf.Approximately(horizontalInput, 0f))
            {
                horizontalInput = Input.GetAxis("Horizontal");
            }
        }
    #endif

        Vector3 constrainedMovement = horizontalInput * coverDirection;
        character.transform.position +=
            constrainedMovement * Time.deltaTime * character.Motion.LinearSpeed;

        Vector3 forwardDirection = currentCover.transform.forward;
        character.transform.rotation =
            Quaternion.LookRotation(forwardDirection, Vector3.up);
    }
}