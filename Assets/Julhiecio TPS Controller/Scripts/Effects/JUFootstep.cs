using UnityEngine;
using UnityEditor;
using UnityEngine.Events;

namespace JUTPS.FX
{
    /// <summary>
    /// Spawn footstep sound or effects while character walks.
    /// </summary>
    [RequireComponent(typeof(Animator))]
    [AddComponentMenu("JU TPS/FX/Footstep")]
    public class JUFootstep : MonoBehaviour
    {
        private const float CHECK_FOOTSTEP_DISTANCE_INTERVAL = 2f;

        // Use static variable to store camera to share the same data for all footsteps 
        // of all characters avoiding unnecessary calls to find the camera.
        private static Camera _mainCamera;

        private float _checkFootstepActiveTimer;

        private bool _leftFootGrounded;
        private bool _rightFootGrounded;

        private float _checkLeftFootTimer;
        private float _checkRightFootTimer;

        /// <summary>
        /// The audio source that will play the footstep sound.
        /// </summary>
        [Header("FX Settings")]
        public AudioSource AudioSource;

        /// <summary>
        /// All footsteps FXs.
        /// </summary>
        public SurfaceAudiosWithFX[] FootstepAudioClips;

        /// <summary>
        /// Invert the footstep decal X scale?
        /// </summary>
        public bool InvertX;

        /// <summary>
        /// Used to doesn't allow play multiple footstep audios on the same time.
        /// </summary>
        [Range(0, 1)]
        public float MinTimeToPlayAudio;

        /// <summary>
        /// The ground collider layer.
        /// </summary>
        [Header("Ground Check")]
        public LayerMask GroundLayers;

        /// <summary>
        /// The max distance to check if the foot is grounded.
        /// </summary>
        [Range(0, 1)]
        public float CheckDistance;

        /// <summary>
        /// The ground check position 'Y' relative to foot position.
        /// </summary>
        [Header("Ground Check Position Offset")]
        [Range(-0.2f, 0.2f)]
        public float UpOffset;

        /// <summary>
        /// The ground check position 'Z' relative to foot position.
        /// </summary>
        [Range(-0.2f, 0.2f)]
        public float ForwardOffset;

        /// <summary>
        /// The left foot transform.
        /// </summary>
        [Space]
        public Transform LeftFoot;

        /// <summary>
        /// The right foot transform.
        /// </summary>
        public Transform RightFoot;

        /// <summary>
        /// The max distance that the footstep can play based on <see cref="Camera.main"/> position.
        /// </summary>
        public float MaxFootstepDistance;

        /// <summary>
        /// Called on left foot hit ground.
        /// </summary>
        public UnityEvent<RaycastHit> OnLeftFootHit;

        /// <summary>
        /// Called on right foot hit ground.
        /// </summary>
        public UnityEvent<RaycastHit> OnRightFootHit;

        /// <summary>
        /// Return true if the character is closest of the <see cref="Camera.main"/> based on <see cref="MaxFootstepDistance"/>.
        /// If false, the footstep will not play (used to optimize distant characters).
        /// </summary>
        public bool IsFootsepActing { get; private set; }

        /// <summary>
        /// The animator used by the footstep system.
        /// </summary>
        public Animator Animator { get; private set; }

        /// <summary>
        /// The footstep checker position of the left foot.
        /// </summary>
        public Vector3 LeftFootCheckerPosition
        {
            get => LeftFoot ? GetFootCheckerPosition(LeftFoot) : Vector3.zero;
        }

        /// <summary>
        /// The footstep checker position of the right foot.
        /// </summary>
        public Vector3 RightFootCheckerPosition
        {
            get => RightFoot ? GetFootCheckerPosition(RightFoot) : Vector3.zero;
        }

        /// <summary>
        /// Create a component instance.
        /// </summary>
        public JUFootstep()
        {
            MinTimeToPlayAudio = 0.3f;
            CheckDistance = 0.2f;

            ForwardOffset = 0.07f;
            UpOffset = -0.07f;

            MaxFootstepDistance = 20;
        }

        private void Start()
        {
            if (!AudioSource)
                AudioSource = GetComponent<AudioSource>();

            Animator = GetComponent<Animator>();
            if (Animator)
            {
                if (!LeftFoot) LeftFoot = Animator.GetBoneTransform(HumanBodyBones.LeftFoot);
                if (!RightFoot) RightFoot = Animator.GetBoneTransform(HumanBodyBones.RightFoot);
            }

            if (GroundLayers.value == 0)
                GroundLayers = LayerMask.GetMask("Default");
        }

        private void Update()
        {
            if (!LeftFoot || !RightFoot)
                return;

            UpdateFootstepActiveByDistance();

            if (!IsFootsepActing)
                return;

            if (_checkLeftFootTimer < MinTimeToPlayAudio)
                _checkLeftFootTimer += Time.deltaTime;

            else
            {
                bool hasLeftGroundHit = GetFootHitInfo(LeftFoot, out RaycastHit leftFootHit);

                if (hasLeftGroundHit && !_leftFootGrounded)
                {
                    DoFootstep(LeftFoot, leftFootHit);
                    _checkLeftFootTimer = 0;
                    _leftFootGrounded = true;

                    OnLeftFootHit.Invoke(leftFootHit);
                }

                if (!hasLeftGroundHit)
                    _leftFootGrounded = false;
            }

            if (_checkRightFootTimer < MinTimeToPlayAudio)
                _checkRightFootTimer += Time.deltaTime;

            else
            {
                bool hasRightGroundHit = GetFootHitInfo(RightFoot, out RaycastHit rightFootHit);

                if (hasRightGroundHit && !_rightFootGrounded)
                {
                    DoFootstep(RightFoot, rightFootHit);
                    _checkRightFootTimer = 0;
                    _rightFootGrounded = true;

                    OnRightFootHit.Invoke(rightFootHit);
                }

                if (!hasRightGroundHit)
                    _rightFootGrounded = false;
            }
        }

        private bool GetFootHitInfo(Transform foot, out RaycastHit hit)
        {
            Vector3 footPosition = GetFootCheckerPosition(foot);
            return Physics.Raycast(footPosition, -transform.up, out hit, CheckDistance, GroundLayers);
        }

        private Vector3 GetFootCheckerPosition(Transform foot)
        {
            return foot.position + transform.forward * ForwardOffset + transform.up * UpOffset;
        }

        private void DoFootstep(Transform foot, RaycastHit groundHit)
        {
            // Play random footstep audio, instantiate decal and return the decal gameobject
            GameObject footstepDecal = SurfaceAudiosWithFX.Play(AudioSource, FootstepAudioClips, groundHit.point, Quaternion.identity, null, groundHit.collider.tag);

            if (!footstepDecal)
                return;

            Transform decalTransform = footstepDecal.transform;

            // Align decal with ground.
            decalTransform.rotation = Quaternion.LookRotation(groundHit.normal) * Quaternion.Euler(90, 0, 0);

            // Look to the character move direction.
            var forward = Vector3.ProjectOnPlane(transform.forward, Vector3.up);
            forward /= forward.magnitude;
            decalTransform.rotation *= Quaternion.LookRotation(forward);

            // Fix Footstep Decal sides fix
            if (foot == RightFoot)
            {
                Vector3 decalScale = decalTransform.localScale;
                decalScale.x *= -1;

                if (InvertX)
                    decalScale.x *= -1;

                decalTransform.localScale = decalScale;
            }
            else
            {
                Vector3 decalScale = decalTransform.localScale;
                if (InvertX)
                    decalScale.x *= -1;

                decalTransform.localScale = decalScale;
            }

            // Draw a line in the upward direction of the Footstep Decal
            Debug.DrawRay(footstepDecal.transform.position, footstepDecal.transform.up * 2, Color.red, 1);
        }

        private void UpdateFootstepActiveByDistance()
        {
            if (!AudioSource)
            {
                return;
            }

            _checkFootstepActiveTimer += Time.deltaTime;
            if (_checkFootstepActiveTimer < CHECK_FOOTSTEP_DISTANCE_INTERVAL)
            {
                return;
            }

            if (_mainCamera && !_mainCamera.isActiveAndEnabled)
            {
                _mainCamera = null;
            }

            if (!_mainCamera)
            {
                _mainCamera = Camera.main;
            }

            if (!_mainCamera)
            {
                return;
            }

            _checkFootstepActiveTimer = 0;
            IsFootsepActing = Vector3.Distance(transform.position, _mainCamera.transform.position) < MaxFootstepDistance;
            AudioSource.enabled = IsFootsepActing;
        }

        private void OnDrawGizmos()
        {
            if (LeftFoot == null || RightFoot == null)
            {
                Animator = GetComponent<Animator>();
                LeftFoot = Animator.GetBoneTransform(HumanBodyBones.LeftFoot);
                RightFoot = Animator.GetBoneTransform(HumanBodyBones.RightFoot);
                return;
            }
            Color collisionColor = Color.green;
            collisionColor.a = 0.4f;

            Color noCollisionColor = Color.red;
            noCollisionColor.a = 0.2f;

            Gizmos.color = _leftFootGrounded ? collisionColor : noCollisionColor;
            Gizmos.DrawSphere(LeftFootCheckerPosition, CheckDistance / 2);
            Gizmos.DrawWireSphere(LeftFootCheckerPosition, CheckDistance / 2);

            Gizmos.color = _rightFootGrounded ? collisionColor : noCollisionColor;
            Gizmos.DrawSphere(RightFootCheckerPosition, CheckDistance / 2);
            Gizmos.DrawWireSphere(RightFootCheckerPosition, CheckDistance / 2);
        }

#if UNITY_EDITOR
        [ContextMenu("Load Default Footstep Audios", false, 100)]
        public void LoadDefaultFootstepInInspector()
        {
            LoadDefaultFootstepAudios(this);
        }

        private static void LoadDefaultFootstepAudios(JUFootstep footsteper, string path = "Assets/Julhiecio TPS Controller/Audio/Footstep/")
        {
            if (!System.IO.Directory.Exists(path))
            {
                Debug.LogError("Unable to load default footstep audios as the indicated path does not exist.");
                return;
            }

            // Create empty audio slots.
            footsteper.FootstepAudioClips = new SurfaceAudiosWithFX[4];
            for (int i = 0; i < 4; i++)
            {
                footsteper.FootstepAudioClips[i] = new SurfaceAudiosWithFX();
                for (int x = 0; x < 4; x++)
                    footsteper.FootstepAudioClips[i].AudioClips.Add(null);
            }

            //Load Footstep Audios.
            footsteper.FootstepAudioClips[0].SurfaceTag = "Untagged";
            for (int i = 0; i < 4; i++)
            {
                string audioClipPath = $"{path}Concrete/Footstep on Concrete 0{i + 1}.ogg";
                footsteper.FootstepAudioClips[0].AudioClips[i] = LoadAsset<AudioClip>(audioClipPath);
            }

            footsteper.FootstepAudioClips[1].SurfaceTag = "Stone";
            for (int i = 0; i < 4; i++)
            {
                string audioClipPath = $"{path}Stones/Footsteps-on-stone0{i + 1}.ogg";
                footsteper.FootstepAudioClips[1].AudioClips[i] = LoadAsset<AudioClip>(audioClipPath);
            }

            footsteper.FootstepAudioClips[2].SurfaceTag = "Grass";
            for (int i = 0; i < 4; i++)
            {
                string audioClipPath = $"{path}Grass/Footsteps-on-grass0{i + 1}.ogg";
                footsteper.FootstepAudioClips[2].AudioClips[i] = LoadAsset<AudioClip>(audioClipPath);
            }

            footsteper.FootstepAudioClips[3].SurfaceTag = "Tiles";
            for (int i = 0; i < 4; i++)
            {
                string audioClipPath = $"{path}Tiles/Footstep-on-tiles0{i + 1}.ogg";
                footsteper.FootstepAudioClips[3].AudioClips[i] = LoadAsset<AudioClip>(audioClipPath);
            }
        }

        private static T LoadAsset<T>(string path) where T : Object
        {
            if (!System.IO.File.Exists(path))
            {
                Debug.LogWarning($"Unable to load asset {typeof(T).Name}: {path}");
                return null;
            }

            return AssetDatabase.LoadAssetAtPath<T>(path);
        }
#endif
    }
}
