using JUTPS.InteractionSystem.Interactables;
using JUTPS.JUInputSystem;
using System;
using UnityEngine;
using UnityEngine.Events;

namespace JUTPS.InteractionSystem
{
    /// <summary>
    /// Interaction system, find and interact with <see cref="JUInteractable"/> objects.
    /// </summary>
    [AddComponentMenu("JU TPS/Interaction System/JU Interaction System")]
    public class JUInteractionSystem : MonoBehaviour
    {
        /// <summary>
        /// Stores the properties that allow to find and interact with closest <see cref="JUInteractable"/> that have a collider as trigger.
        /// </summary>
        [System.Serializable]
        public class FindInteractablesSettings
        {
            /// <summary>
            /// The layer that contains colliders with <see cref="InteractablesTag"/> and obstacles like walls or buildings. 
            /// </summary>
            public LayerMask Layer;

            /// <summary>
            /// Find interactables only if there are not obstacles between the interactable and the character.
            /// </summary>
            public bool AvoidObstacles;

            /// <summary>
            /// The max distance to find interactables.
            /// </summary>
            public float CheckRange;

            /// <summary>
            /// If true, <see cref="FindNearInteractables"/> will be called each <see cref="AutoCheckInterval"/> to find all closest interactales.
            /// </summary>
            [Header("Auto Check")]
            public bool AutoCheck;

            /// <summary>
            /// Used to auto find near interactables several times per second if <see cref="AutoCheck"/> is true. <para />
            /// Don't set it to less than 0.1 to avoid performance issues.
            /// </summary>
            [Min(0.1f)] public float AutoCheckInterval;

            /// <summary>
            /// Create an instance of properties to use with <see cref="FindNearInteractables"/>.
            /// </summary>
            public FindInteractablesSettings()
            {
                Layer = 1;
                CheckRange = 2;
                AutoCheck = true;
                AutoCheckInterval = 0.5f;
                AvoidObstacles = true;
            }
        }

        private float _findInteractablesTimer;
        private Collider _selfCollider;

        /// <summary>
        /// Use default player controls to interact with objects.
        /// </summary>
        public bool UseDefaultInputs;

        /// <summary>
        /// The inputs with interaction keys/buttons.
        /// </summary>
        public JUPlayerCharacterInputAsset Inputs;

        /// <summary>
        /// The settings to find closest interactables.
        /// </summary>
        public FindInteractablesSettings FindInteractables;

        /// <summary>
        /// If true, the interaction with objects is enabled. Also, check if <see cref="BlockInteractions"/> is false to allow interact
        /// With other objects.
        /// </summary>
        public bool InteractionEnabled;

        /// <summary>
        /// Allow blocking any interaction, useful for complex interactions that are not 
        /// instantaneous with multiple steps (like play enter/exit vehicle animations).
        /// </summary>
        public bool BlockInteractions { get; set; }

        /// <summary>
        /// Invoked on interact with someting.
        /// </summary>
        public UnityEvent<JUInteractable> OnInteract;

        /// <summary>
        /// Returns a <see cref="JUInteractable"/> near of the character if the character pass closest to a collider with <see cref="FindInteractablesSettings.InteractablesTag"/> tag.
        /// </summary>
        public JUInteractable NearestInteractable { get; private set; }

        /// <summary>
        /// This object center, based on the collider center, if have a collider component.
        /// </summary>
        public Vector3 SelfCenter
        {
            get
            {
                if (!_selfCollider)
                    return transform.position;

                return _selfCollider.bounds.center;
            }
        }

        /// <summary>
        /// Create instance.
        /// </summary>
        public JUInteractionSystem()
        {
            UseDefaultInputs = true;
            InteractionEnabled = true;
            BlockInteractions = false;
            FindInteractables = new FindInteractablesSettings();
        }

        private void Reset()
        {
            LayerMask[] defaultFindInteractablesLayer = {
                LayerMask.NameToLayer("Default"),
                LayerMask.NameToLayer("Ignore Raycast"),
                LayerMask.NameToLayer("Wall"),
                LayerMask.NameToLayer("Walls"),
                LayerMask.NameToLayer("Obstacle"),
                LayerMask.NameToLayer("Obstacles"),
                LayerMask.NameToLayer("Terrain"),
            };

            // Setup default interactables and obstacle layers.
            FindInteractables.Layer = 0;
            for (int i = 0; i < defaultFindInteractablesLayer.Length; i++)
            {
                if (defaultFindInteractablesLayer[i] != -1)
                    FindInteractables.Layer |= 1 << defaultFindInteractablesLayer[i];
            }

#if UNITY_EDITOR
            Inputs = UnityEditor.AssetDatabase.LoadAssetAtPath<JUPlayerCharacterInputAsset>("Assets/Julhiecio TPS Controller/Input Controls/Player Character Inputs.asset");
#endif
        }

        private void OnEnable()
        {
            if (Inputs)
                Inputs.InteractAction.started += OnPressInteractButton;
        }

        private void OnDestroy()
        {
            if (Inputs)
                Inputs.InteractAction.started -= OnPressInteractButton;
        }

        private void Start()
        {
            _selfCollider = GetComponent<Collider>();

            if (Inputs) Inputs.SetActiveInputs(true);
        }

        private void Update()
        {
            if (FindInteractables.AutoCheck)
            {
                _findInteractablesTimer += Time.deltaTime;
                if (_findInteractablesTimer > FindInteractables.AutoCheckInterval)
                {
                    _findInteractablesTimer = 0;
                    FindNearInteractables();
                }
            }
        }

        /// <summary>
        /// Find interactables near of this gameObject using <see cref="FindNearInteractables"/>. <para />
        /// The <see cref="JUInteractable"/> must have a collider as trigger to be found.
        /// </summary>
        public void FindNearInteractables()
        {
            Vector3 characterCenter = SelfCenter;
            Collider[] colliders = Physics.OverlapSphere(characterCenter, FindInteractables.CheckRange, FindInteractables.Layer);

            NearestInteractable = null;

            if (colliders.Length == 0)
                return;

            // Find the nearest interactable.
            Array.Sort(colliders, (a, b) =>
            {
                float aDistance = (characterCenter - a.transform.position).magnitude;
                float bDistance = (characterCenter - b.transform.position).magnitude;
                return aDistance > bDistance ? 1 : -1;
            });

            Collider nearestCollider = colliders[0];

            if (FindInteractables.AvoidObstacles)
            {
                Physics.Linecast(characterCenter, nearestCollider.bounds.center, out RaycastHit hit, FindInteractables.Layer, QueryTriggerInteraction.Ignore);
                if (hit.collider && hit.collider != nearestCollider)
                {
                    NearestInteractable = null;
                    return;
                }
            }

            NearestInteractable = nearestCollider.GetComponentInParent<JUInteractable>();

            return;
        }

        private void OnPressInteractButton(UnityEngine.InputSystem.InputAction.CallbackContext context)
        {
            if (!UseDefaultInputs)
                return;

            Interact(NearestInteractable);
        }

        /// <summary>
        /// Interact with some <see cref="JUInteractable"/>
        /// </summary>
        /// <param name="interactable">The object to interact.</param>
        public void Interact(JUInteractable interactable)
        {
            if (!CanInteract(interactable))
                return;

            interactable.Interact();
            OnInteract?.Invoke(interactable);
        }

        /// <summary>
        /// Return true if the <see cref="JUInteractionSystem"/> can interact with the <see cref="JUInteractable"/>.
        /// </summary>
        /// <param name="interactable">The interactable to check.</param>
        /// <returns>Return true if can interact.</returns>
        public bool CanInteract(JUInteractable interactable)
        {
            return interactable && InteractionEnabled && !BlockInteractions && interactable.CanInteract(this);
        }
    }
}