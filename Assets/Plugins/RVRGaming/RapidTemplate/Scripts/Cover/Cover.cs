using UnityEngine;
using GameCreator.Runtime.Characters;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RT
{
    [AddComponentMenu("Game Creator/Mechanics/Cover")]
    public class Cover : MonoBehaviour
    {
        [Header("Cover Settings")]
        [Tooltip("Maximum distance to cover for it to be usable.")]
        public float maxCoverDistance = 2f;

        [Tooltip("Maximum height of cover that allows vaulting.")]
        public float maxCoverHeight = 1.5f;

        [Tooltip("Vault distance to move the character forward while vaulting.")]
        public float vaultDistance = 1.3f;

        [Tooltip("Duration for easing into cover or vault position.")]
        [SerializeField] private float easingDuration = 0.1f;

        [Header("Enter Cover")]
        [SerializeField] public StateData coverState = new StateData(StateData.StateType.State);
        [SerializeField] public int coverStateLayer = 0;
        [SerializeField] public float stateTransitionIn = 0.1f;
        [SerializeField] public float stateTransitionOut = 0.25f;

        [Header("Enter High Cover")]
        [SerializeField] public StateData highCoverState = new StateData(StateData.StateType.State);
        [SerializeField] public int highCoverStateLayer = 0;
        [SerializeField] public float highStateTransitionIn = 0.1f;
        [SerializeField] public float highStateTransitionOut = 0.25f;

        [Header("Vault")]
        [SerializeField] private AnimationClip vaultAnimation;
        [SerializeField] private AvatarMask animationMask;
        [SerializeField] private float animationTransitionIn = 0.1f;
        [SerializeField] private float animationTransitionOut = 0.25f;
        [SerializeField] private bool useRootMotion = true;

        [Header("Vault Options")]
        [Tooltip("If disabled, vaulting is disabled even for low cover.")]
        [SerializeField] private bool allowVault = true;

        [Header("UI Elements")]
        [SerializeField] private GameObject coverCanvasPrefab;
        [SerializeField] private GameObject vaultCanvasPrefab;
        [SerializeField] private Vector3 uiOffset = new Vector3(0.5f, 0f, 0f);
        [SerializeField] private bool displayUIElements = true;

        [Header("Enter Cover Instructions")]
        [Space]
        [SerializeField] public InstructionList onEnterCover = new InstructionList();
        [Header("Exit Cover Instructions")]
        [Space]
        [SerializeField] public InstructionList onExitCover = new InstructionList();

        private GameObject coverCanvasInstance;
        private GameObject vaultCanvasInstance;
        private Character character;

        private BoxCollider _collider;
        private Vector3 _coverSize;

        private Dictionary<Character, Vector3> users = new Dictionary<Character, Vector3>();

        private void Awake()
        {
            _collider = GetComponent<BoxCollider>();
            if (_collider != null)
            {
                _coverSize = _collider.size;
                _collider.isTrigger = true;
            }

            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject != null)
            {
                character = playerObject.GetComponent<Character>();
            }
        }

        private void Update()
        {
            if (displayUIElements)
            {
                HandleUIDisplay();
            }
        }

        private void LateUpdate()
        {
            if (displayUIElements)
            {
                UpdateUIPositions();
            }
        }

        public bool IsLowCover()
        {
            return _coverSize.y <= maxCoverHeight;
        }

        public bool IsHighCover()
        {
            return _coverSize.y > maxCoverHeight;
        }

        private void OnTriggerEnter(Collider other)
        {
            Character detectedCharacter = other.GetComponent<Character>();
            if (detectedCharacter != null)
            {
                CoverController coverController = other.GetComponent<CoverController>();

                if (coverController != null && coverController.IsInCover())
                {
                    coverController.TransitionToCover(this);
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            Character detectedCharacter = other.GetComponent<Character>();
            if (detectedCharacter != null)
            {
                // 
            }
        }

        private void HandleUIDisplay()
        {
            if (coverCanvasPrefab == null || vaultCanvasPrefab == null) return;

            if (!character.IsPlayer)
            {
                return;
            }

            if (IsCoverInReach(character.transform.position))
            {
                if (coverCanvasInstance == null)
                {
                    coverCanvasInstance = Instantiate(coverCanvasPrefab, this.transform);
                    coverCanvasInstance.transform.localPosition = uiOffset;
                }

                if (CanVault(character.transform.position))
                {
                    if (vaultCanvasInstance == null)
                    {
                        vaultCanvasInstance = Instantiate(vaultCanvasPrefab, this.transform);
                        vaultCanvasInstance.transform.localPosition = uiOffset;
                    }
                }
                else if (vaultCanvasInstance != null)
                {
                    Destroy(vaultCanvasInstance);
                    vaultCanvasInstance = null;
                }
            }
            else
            {
                if (coverCanvasInstance != null)
                {
                    Destroy(coverCanvasInstance);
                    coverCanvasInstance = null;
                }

                if (vaultCanvasInstance != null)
                {
                    Destroy(vaultCanvasInstance);
                    vaultCanvasInstance = null;
                }
            }
        }

        public void UpdateUIPositions()
        {
            if (coverCanvasInstance != null)
            {
                Vector3 targetLocalPosition = transform.InverseTransformPoint(character.transform.position) + uiOffset;
                targetLocalPosition.y = 0f;
                targetLocalPosition.z = 0f;

                float distance = Vector3.Distance(coverCanvasInstance.transform.localPosition, targetLocalPosition);

                // If the difference is large, snap immediately to avoid lag
                if (distance > 0.2f)
                {
                    coverCanvasInstance.transform.localPosition = targetLocalPosition;
                }
                else
                {
                    float moveSpeed = 20f;
                    coverCanvasInstance.transform.localPosition = Vector3.MoveTowards(
                        coverCanvasInstance.transform.localPosition,
                        targetLocalPosition,
                        moveSpeed * Time.deltaTime
                    );
                }
            }

            if (vaultCanvasInstance != null)
            {
                Vector3 targetLocalPosition = transform.InverseTransformPoint(character.transform.position) + uiOffset;
                targetLocalPosition.y = 0f;
                targetLocalPosition.z = 0f;

                float distance = Vector3.Distance(vaultCanvasInstance.transform.localPosition, targetLocalPosition);
                if (distance > 0.2f)
                {
                    vaultCanvasInstance.transform.localPosition = targetLocalPosition;
                }
                else
                {
                    float moveSpeed = 20f;
                    vaultCanvasInstance.transform.localPosition = Vector3.MoveTowards(
                        vaultCanvasInstance.transform.localPosition,
                        targetLocalPosition,
                        moveSpeed * Time.deltaTime
                    );
                }
            }
        }


        public bool IsCoverInReach(Vector3 characterPosition)
        {
            Vector3 closestPoint = _collider.ClosestPoint(characterPosition);
            float distance = Vector3.Distance(characterPosition, closestPoint);

            Vector3 objectForward = -transform.forward;
            Vector3 directionToCharacter = (characterPosition - (transform.position - transform.forward * -0.1f)).normalized;
            float dotProduct = Vector3.Dot(objectForward, directionToCharacter);

            float extraRange = 0.1f;
            bool isWithinWidth = Mathf.Abs(Vector3.Dot(characterPosition - transform.position, transform.right)) <= (_collider.size.x / 2f + extraRange);

            return distance <= maxCoverDistance && dotProduct > 0 && isWithinWidth;
        }

        public bool CanVault(Vector3 characterPosition)
        {
            if (!allowVault)
            {
                return false;
            }

            bool isInRange = IsCoverInReach(characterPosition);
            bool isVaultable = _coverSize.y <= maxCoverHeight;
            return isInRange && isVaultable;
        }

        public void RegisterUser(Character character, Vector3 position)
        {
            if (!users.ContainsKey(character))
            {
                users.Add(character, position);
            }
        }

        public void UnregisterUser(Character character)
        {
            if (users.ContainsKey(character))
            {
                users.Remove(character);
            }
        }

        public Vector3 GetClosestPoint(Vector3 characterPosition)
        {
            return _collider.ClosestPoint(characterPosition);
        }

        public async Task EnterCover(Character character)
        {
            if (IsCoverInReach(character.transform.position))
            {
                await EaseToCoverPosition(character);

                Vector3 coverPosition = GetClosestPoint(character.transform.position);
                character.transform.position = coverPosition;

                if (_coverSize.y > maxCoverHeight)
                {
                    ConfigState highCoverConfig = new ConfigState(
                        0f, 1f, 1f,
                        highStateTransitionIn,
                        highStateTransitionOut
                    );

                    _ = character.States.SetState(
                        highCoverState, highCoverStateLayer,
                        BlendMode.Blend,
                        highCoverConfig
                    );
                }
                else
                {
                    ConfigState lowCoverConfig = new ConfigState(
                        0f, 1f, 1f,
                        stateTransitionIn,
                        stateTransitionOut
                    );

                    _ = character.States.SetState(
                        coverState, coverStateLayer,
                        BlendMode.Blend,
                        lowCoverConfig
                    );
                }

                RegisterUser(character, coverPosition);
                RunInstructions(onEnterCover);
                DestroyUIElements();
            }
        }

        public async Task ExitCover(Character character)
        {
            character.States.Stop(
                coverStateLayer, 0f,
                stateTransitionOut
            );
            character.States.Stop(
                highCoverStateLayer, 0f,
                highStateTransitionOut
            );

            UnregisterUser(character);
            RunInstructions(onExitCover);

            if (displayUIElements)
            {
                HandleUIDisplay();
            }
        }

        public async Task VaultCover(Character character)
        {
            if (CanVault(character.transform.position))
            {
                await EaseToCoverPosition(character);

                Collider characterCollider = character.GetComponent<Collider>();
                if (characterCollider != null)
                {
                    characterCollider.enabled = false;
                }

                if (vaultAnimation != null)
                {
                    Task moveTask = MoveCharacterDuringVault(character, vaultDistance, vaultAnimation.length);
                    Task vaultTask = PlayAnimation(character, vaultAnimation);

                    await ExitCover(character);

                    await moveTask;
                    await vaultTask;
                }
                else
                {
                    character.transform.position += character.transform.forward * vaultDistance;
                    await ExitCover(character);
                }

                if (characterCollider != null)
                {
                    characterCollider.enabled = true;
                }
            }
        }

        private async Task EaseToCoverPosition(Character character)
        {
            Vector3 startPosition = character.transform.position;
            Vector3 endPosition = GetClosestPoint(character.transform.position);

            Quaternion startRotation = character.transform.rotation;
            Quaternion endRotation = Quaternion.LookRotation(transform.forward, Vector3.up);

            float elapsedTime = 0f;

            while (elapsedTime < easingDuration)
            {
                float t = elapsedTime / easingDuration;
                character.transform.position = Vector3.Lerp(startPosition, endPosition, t);
                character.transform.rotation = Quaternion.Slerp(startRotation, endRotation, t);

                elapsedTime += Time.deltaTime;
                await Task.Yield();
            }

            character.transform.position = endPosition;
            character.transform.rotation = endRotation;
        }

        public bool IsCharacterWithinBounds(Vector3 characterPosition)
        {
            Bounds bounds = _collider.bounds;
            bounds.Expand(0.1f);
            return bounds.Contains(characterPosition);
        }

        private async Task MoveCharacterDuringVault(Character character, float distance, float duration)
        {
            Vector3 startPosition = character.transform.position;
            Vector3 endPosition = startPosition + character.transform.forward * distance;

            float elapsedTime = 0f;

            while (elapsedTime < duration)
            {
                float t = elapsedTime / duration;
                character.transform.position = Vector3.Lerp(startPosition, endPosition, t);

                elapsedTime += Time.deltaTime;
                await Task.Yield();
            }

            character.transform.position = endPosition;
        }

        private async Task PlayAnimation(Character character, AnimationClip animationClip)
        {
            float speed = 1f;
            ConfigGesture gestureConfig = new ConfigGesture(
                0f, animationClip.length, speed, useRootMotion,
                animationTransitionIn, animationTransitionOut
            );

            _ = character.Gestures.CrossFade(
                animationClip, animationMask, BlendMode.Blend,
                gestureConfig, true
            );

            await Task.Delay((int)(animationClip.length * 1000 / speed));
        }

        private void RunInstructions(InstructionList instructions)
        {
            _ = instructions.Run(new Args(this.gameObject));
        }

        public void ActivateUI()
        {
            if (coverCanvasInstance != null)
            {
                coverCanvasInstance.SetActive(true);
            }

            if (vaultCanvasInstance != null)
            {
                vaultCanvasInstance.SetActive(true);
            }
        }

        public void DeactivateUI()
        {
            if (coverCanvasInstance != null)
            {
                coverCanvasInstance.SetActive(false);
            }

            if (vaultCanvasInstance != null)
            {
                vaultCanvasInstance.SetActive(false);
            }
        }

        private void DestroyUIElements()
        {
            if (coverCanvasInstance != null)
            {
                Destroy(coverCanvasInstance);
                coverCanvasInstance = null;
            }

            if (vaultCanvasInstance != null)
            {
                Destroy(vaultCanvasInstance);
                vaultCanvasInstance = null;
            }
        }
    }
}
