using JU;
using JU.CharacterSystem.AI;
using UnityEngine;

namespace JUTPS.AI.Vfx
{
    /// <summary>
    /// Show an alert when the AI find a new target or when hear something (if have <see cref="JU.CharacterSystem.AI.HearSystem.HearSensor"/>).
    /// The script can be added to the AI or as child of the AI.
    /// </summary>
    public class JU_AiAlert : MonoBehaviour
    {
        private static Camera _mainCamera;

        private float _alertTime;
        private float _alertScale;
        private Vector3 _defaultAlertScale;

        [SerializeField] private SpriteRenderer _alertSprite;

        private IIsDead _deadChecker;
        private IIsOnRagdoll _ragdollChecker;
        private IOnSetTarget _targetDetector;
        private IOnHear _listener;

        private GameObject _currentTarget;

        /// <summary>
        /// Show alert animation speed.
        /// </summary>
        [Space]
        public float AnimationSpeed;

        /// <summary>
        /// Max time showing the alert.
        /// </summary>
        public float MaxTimeShowingAlert;

        /// <summary>
        /// Default alert color.
        /// </summary>
        public Color AlertColor;

        /// <summary>
        /// Alert color on find a target.
        /// </summary>
        public Color TargetDetectedColor;

        /// <summary>
        /// Return true if is showing the alert.
        /// </summary>
        public bool IsShowingAlert { get; private set; }

        /// <summary>
        /// The alert sprite.
        /// </summary>
        public SpriteRenderer AlertSprite
        {
            get => _alertSprite;
        }

        /// <summary>
        /// Return true if can show the indicator.
        /// </summary>
        public bool CanShowAlert
        {
            get
            {
                if (IsShowingAlert)
                    return false;

                if (_deadChecker != null && _deadChecker.IsDead)
                    return false;

                // Can't show if is ragdolling.
                if (_ragdollChecker != null && _ragdollChecker.IsOnRagdoll)
                    return false;

                return true;
            }
        }

        /// <summary>
        /// Create a component instance.
        /// </summary>
        public JU_AiAlert()
        {
            AlertColor = new Color(0.5f, 0.5f, 1, 0.6f);
            TargetDetectedColor = new Color(1, 0.5f, 0.5f, 0.6f);

            AnimationSpeed = 5f;
            MaxTimeShowingAlert = 1f;
        }

        private void Start()
        {
            if (!AlertSprite)
                return;

            _deadChecker = GetComponent<IIsDead>();
            _ragdollChecker = GetComponent<IIsOnRagdoll>();
            _targetDetector = GetComponent<IOnSetTarget>();
            _listener = GetComponent<IOnHear>();

            Transform parent = transform.parent;

            if (_deadChecker == null && parent)
                _deadChecker = parent.GetComponent<IIsDead>();

            if (_ragdollChecker == null && parent)
                _ragdollChecker = parent.GetComponent<IIsOnRagdoll>();

            if (_targetDetector == null && parent)
                _targetDetector = parent.GetComponentInChildren<IOnSetTarget>();

            if (_listener == null && parent)
                _listener = parent.GetComponentInChildren<IOnHear>();

            if (_targetDetector != null)
                _targetDetector.OnSetTarget += OnSetTarget;

            if (_listener != null)
                _listener.OnHear += OnHearSomething;

            _defaultAlertScale = AlertSprite.transform.localScale;
            AlertSprite.transform.localScale = new Vector3(1, 0, 1);
        }

        private void OnDestroy()
        {
            if (_targetDetector != null)
                _targetDetector.OnSetTarget -= OnSetTarget;

            if (_listener != null)
                _listener.OnHear -= OnHearSomething;
        }

        private void Update()
        {
            if (!AlertSprite)
                return;

            if (!IsShowingAlert)
                return;

            _alertTime += Time.deltaTime;
            _alertScale += Time.deltaTime * AnimationSpeed;
            Vector3 scale = _defaultAlertScale * Mathf.Clamp01(_alertScale);

            if (_alertTime > MaxTimeShowingAlert) // End show alert.
            {
                IsShowingAlert = false;
                scale = new Vector3(1, 0, 1);
            }

            if (!_mainCamera)
                _mainCamera = Camera.main;

            Vector3 alertDirection = Vector3.ProjectOnPlane(_mainCamera.transform.position - AlertSprite.transform.position, Vector3.up);
            Quaternion alertRotation = Quaternion.LookRotation(alertDirection);

            AlertSprite.transform.rotation = alertRotation;
            AlertSprite.transform.localScale = scale;
        }

        private void OnHearSomething(Vector3 point, GameObject obj)
        {
            if (!CanShowAlert)
                return;

            DoAlert();
        }

        private void OnSetTarget(GameObject target)
        {
            if (!target || target == _currentTarget)
                return;

            _currentTarget = target;
            DoTargetDetectedAlert();
        }

        /// <summary>
        /// Show the alert.
        /// </summary>
        public void DoAlert()
        {
            if (!CanShowAlert)
                return;

            _alertTime = 0;

            if (IsShowingAlert)
                return;

            _alertScale = 0;
            IsShowingAlert = true;
            AlertSprite.color = AlertColor;
        }

        /// <summary>
        /// Show the alert for target detected, have a different color of the normal alert.
        /// </summary>
        public void DoTargetDetectedAlert()
        {
            DoAlert();
            AlertSprite.color = TargetDetectedColor;
        }
    }
}