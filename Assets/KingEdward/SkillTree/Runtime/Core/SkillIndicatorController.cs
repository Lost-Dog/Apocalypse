using System;
using UnityEngine;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.Cameras;
using GameCreator.Runtime.Characters;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace KingEdward.SkillTree
{
    /// <summary>
    /// Controls the skill indicator and exposes the last target for Visual Scripting.
    /// </summary>
    [AddComponentMenu("KingEdward/Skill Tree/Skill Indicator Controller")]
    public class SkillIndicatorController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private SkillIndicator m_Indicator;
        [SerializeField] private PropertyGetGameObject m_Character = GetGameObjectPlayer.Create();
        [SerializeField] private PropertyGetGameObject m_Camera = GetGameObjectCameraMain.Create;
        [SerializeField] private LayerMask m_GroundLayers = -1;
        
#if ENABLE_INPUT_SYSTEM
        [Header("Gamepad Aim")]
        [SerializeField] private InputActionFromAsset m_AimAction;
        [SerializeField] [Range(0.05f, 1f)] private float m_AimDeadzone = 0.25f;
#endif

        private Skill m_ActiveSkill;
        private Vector3 m_LastTargetPosition;
        private Vector3 m_LastDirection;
        private float m_LastRadius;
        private float m_HoldStartTime;
        private Character m_ChargeCharacter;
        private bool m_ChargeStateActive;

        public static Vector3 LastTargetPosition { get; private set; }
        public static float LastRadius { get; private set; }

        /// <summary>
        /// Set static aim result for Visual Scripting properties.
        /// </summary>
        public static void SetAimResult(Vector3 position, float radius)
        {
            LastTargetPosition = position;
            LastRadius = radius;
        }


        public Vector3 LastDirection => m_LastDirection;


        public Skill ActiveSkill => m_ActiveSkill;

        public bool IsShowing => m_Indicator != null && m_Indicator.IsVisible;

        public static SkillIndicatorController Instance { get; private set; }

        private void Awake()
        {
            if (m_Indicator == null)
            {
                m_Indicator = GetComponentInChildren<SkillIndicator>();
            }

        }

        private void OnEnable()
        {
            Instance = this;
        }

        private void OnDisable()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        private void Update()
        {
            if (m_Indicator == null || m_ActiveSkill == null) return;

            var config = m_ActiveSkill.IndicatorConfig;
            if (config == null || !config.HasIndicator) return;

            Vector3 position;
            Vector3 direction;

            GameObject characterObj = m_Character.Get(Args.EMPTY);
            bool useFixedPos = config.fixedAtCharacter && config.IsCircleType;
            if (useFixedPos && characterObj != null)
            {
                position = GetGroundPositionAt(characterObj.transform.position);
            }
            else
            {
                if (!TryGetGamepadAimPosition(config, characterObj, out position) &&
                    !TryGetCursorGroundPosition(out position))
                {
                    return;
                }
            }

            {
                m_LastTargetPosition = position;
                LastTargetPosition = position;
                if (characterObj != null)
                {
                    Vector3 charPos = characterObj.transform.position;
                    direction = (position - charPos).normalized;
                    direction.y = 0f;
                }
                else
                {
                    direction = Vector3.forward;
                }

                if (direction.sqrMagnitude < 0.01f)
                {
                    direction = Vector3.forward;
                }

                m_LastDirection = direction;

                float radiusToUse = config.Radius;
                if (config.IsExpanding)
                {
                    float t = Mathf.Clamp01((Time.time - m_HoldStartTime) / Mathf.Max(0.01f, config.ExpandDuration));
                    radiusToUse = Mathf.Lerp(config.MinRadius, config.MaxRadius, t);
                    m_LastRadius = radiusToUse;
                    LastRadius = radiusToUse;
                    Vector3 circlePos = useFixedPos && characterObj != null ? characterObj.transform.position : position;
                    circlePos.y = position.y;
                    m_Indicator.UpdatePosition(circlePos, direction, radiusToUse);
                }
                else if (config.IsExpandingLine)
                {
                    float t = Mathf.Clamp01((Time.time - m_HoldStartTime) / Mathf.Max(0.01f, config.ExpandDuration));
                    float rangeToUse = Mathf.Lerp(config.MinRadius, config.MaxRadius, t);
                    m_LastRadius = rangeToUse;
                    LastRadius = rangeToUse;
                    Vector3 origin = characterObj != null ? GetGroundPositionAt(characterObj.transform.position) : position - direction * rangeToUse;
                    Vector3 tipPos = origin + direction * rangeToUse;
                    tipPos.y = origin.y;
                    m_LastTargetPosition = tipPos;
                    LastTargetPosition = tipPos;
                    m_Indicator.UpdatePosition(tipPos, direction, rangeToUse);
                }
                else
                {
                    m_LastRadius = config.Radius;
                    LastRadius = config.Radius;
                    if (config.IsCircleType)
                    {
                        Vector3 circlePos = useFixedPos && characterObj != null ? characterObj.transform.position : position;
                        circlePos.y = position.y;
                        m_Indicator.UpdatePosition(circlePos, direction);
                    }
                    else if (config.IsLineType)
                    {
                        Vector3 origin = characterObj != null ? GetGroundPositionAt(characterObj.transform.position) : position - direction * config.Range;
                        Vector3 tipPos = origin + direction * config.Range;
                        tipPos.y = origin.y;
                        m_LastTargetPosition = tipPos;
                        LastTargetPosition = tipPos;
                        m_Indicator.UpdatePosition(tipPos, direction);
                    }
                    else
                    {
                        Vector3 origin = characterObj != null
                            ? GetGroundPositionAt(characterObj.transform.position)
                            : (position - direction * config.Range);
                        m_Indicator.UpdatePosition(origin, direction);
                    }
                }
            }
        }

        public void ShowForSkill(Skill skill)
        {
            if (skill == null)
            {
                Hide();
                return;
            }

            var config = skill.IndicatorConfig;
            if (config == null || !config.HasIndicator)
            {
                Hide();
                return;
            }

            m_ActiveSkill = skill;
            m_HoldStartTime = Time.time;
            m_LastRadius = (config.IsExpanding || config.IsExpandingLine) ? config.MinRadius : config.Radius;
            LastRadius = m_LastRadius;

            GameObject characterObj = m_Character.Get(Args.EMPTY);
            bool useFixedPos = config.fixedAtCharacter && config.IsCircleType;

            if (useFixedPos && characterObj != null)
            {
                m_LastTargetPosition = GetGroundPositionAt(characterObj.transform.position);
            }
            else if (!TryGetCursorGroundPosition(out m_LastTargetPosition))
            {
                if (characterObj != null)
                {
                    m_LastTargetPosition = characterObj.transform.position + characterObj.transform.forward * config.Range;
                }
                else if (config.IsCircleType)
                {
                    m_LastTargetPosition = Vector3.forward * config.Range;
                }
            }

            if (characterObj != null)
            {
                m_LastDirection = (m_LastTargetPosition - characterObj.transform.position).normalized;
                m_LastDirection.y = 0f;
                if (m_LastDirection.sqrMagnitude < 0.01f) m_LastDirection = characterObj.transform.forward;
            }
            else
            {
                m_LastDirection = Vector3.forward;
            }

            if (config.IsLineType && characterObj != null)
            {
                Vector3 origin = GetGroundPositionAt(characterObj.transform.position);
                float range = config.IsExpandingLine ? config.MinRadius : config.Range;
                Vector3 tip = origin + m_LastDirection * range;
                tip.y = origin.y;
                m_LastTargetPosition = tip;
                LastTargetPosition = tip;
            }

            LastTargetPosition = m_LastTargetPosition;
            Vector3 pos;
            if (config.IsCircleType)
                pos = useFixedPos && characterObj != null ? GetGroundPositionAt(characterObj.transform.position) : m_LastTargetPosition;
            else if (config.IsLineType)
                pos = m_LastTargetPosition;
            else
                pos = characterObj != null ? GetGroundPositionAt(characterObj.transform.position) : m_LastTargetPosition;

            m_Indicator.Show(config, pos, m_LastDirection);

            StartChargeStateIfNeeded();
        }


        public void Hide()
        {
            StopChargeStateIfNeeded();
            m_ActiveSkill = null;
            if (m_Indicator != null)
            {
                m_Indicator.Hide();
            }
        }

        public void SetPositionAndDirection(Vector3 position, Vector3 direction)
        {
            m_LastTargetPosition = position;
            LastTargetPosition = position;
            direction.y = 0f;
            m_LastDirection = direction.sqrMagnitude > 0.01f ? direction.normalized : Vector3.forward;

            if (m_Indicator != null && m_ActiveSkill != null)
            {
                var config = m_ActiveSkill.IndicatorConfig;
                if (config != null && config.HasIndicator)
                {
                    Vector3 pos;
                    if (config.IsCircleType) pos = position;
                    else if (config.IsLineType) pos = position;
                    else pos = GetGroundPositionAt(GetCharacterPosition());
                    m_Indicator.UpdatePosition(pos, m_LastDirection);
                }
            }
        }

        private Vector3 GetGroundPositionAt(Vector3 worldPoint)
        {
            Vector3 from = worldPoint + Vector3.up * 50f;
            Ray ray = new Ray(from, Vector3.down);
            if (m_GroundLayers != 0 && Physics.Raycast(ray, out RaycastHit hit, 100f, m_GroundLayers))
                return hit.point;
            worldPoint.y = 0f;
            return worldPoint;
        }

        private bool TryGetCursorGroundPosition(out Vector3 position)
        {
            position = m_LastTargetPosition;

#if ENABLE_INPUT_SYSTEM
            if (Mouse.current == null) return false;
            Vector2 screenPoint = Mouse.current.position.ReadValue();
#else
            Vector2 screenPoint = Input.mousePosition;
#endif

            Camera cam = GetCamera();
            if (cam == null) return false;

            Ray ray = cam.ScreenPointToRay(screenPoint);

            if (m_GroundLayers != 0 && Physics.Raycast(ray, out RaycastHit hit, 1000f, m_GroundLayers))
            {
                position = hit.point;
                return true;
            }

            Plane ground = new Plane(Vector3.up, Vector3.zero);
            if (ground.Raycast(ray, out float enter))
            {
                position = ray.GetPoint(enter);
                return true;
            }

            return false;
        }

        private Vector3 GetCharacterPosition()
        {
            GameObject characterObj = m_Character.Get(Args.EMPTY);
            if (characterObj != null)
            {
                return characterObj.transform.position;
            }
            return m_LastTargetPosition - m_LastDirection * (m_ActiveSkill?.IndicatorConfig?.Range ?? 5f);
        }

#if ENABLE_INPUT_SYSTEM
        private bool TryGetGamepadAimPosition(SkillIndicatorConfig config, GameObject characterObj, out Vector3 position)
        {
            position = m_LastTargetPosition;

            if (m_AimAction == null || characterObj == null)
            {
                return false;
            }

            var action = m_AimAction.InputAction;
            if (action == null) return false;

            // Only treat this as "gamepad aim" if the last actuated control comes from a Gamepad.
            if (action.activeControl == null || action.activeControl.device is not Gamepad)
            {
                return false;
            }

            Vector2 stick = action.ReadValue<Vector2>();
            if (stick.sqrMagnitude < m_AimDeadzone * m_AimDeadzone) return false;

            Camera cam = GetCamera();
            Vector3 forward, right;

            if (cam != null)
            {
                forward = cam.transform.forward;
                right = cam.transform.right;
            }
            else
            {
                forward = characterObj.transform.forward;
                right = characterObj.transform.right;
            }

            forward.y = 0f;
            right.y = 0f;
            forward.Normalize();
            right.Normalize();

            Vector3 dir = forward * stick.y + right * stick.x;
            if (dir.sqrMagnitude < 0.0001f)
            {
                dir = characterObj.transform.forward;
                dir.y = 0f;
            }
            dir.Normalize();

            float distance = config.IsCircleType ? config.Radius : config.Range;
            if (distance <= 0f) distance = 5f;

            Vector3 origin = GetGroundPositionAt(characterObj.transform.position);
            Vector3 target = origin + dir * distance;
            target.y = origin.y;

            position = target;
            return true;
        }
#else
        private bool TryGetGamepadAimPosition(SkillIndicatorConfig config, GameObject characterObj, out Vector3 position)
        {
            position = m_LastTargetPosition;
            return false;
        }
#endif

        private Camera GetCamera()
        {
            GameObject camObj = m_Camera != null ? m_Camera.Get(Args.EMPTY) : null;
            if (camObj != null)
            {
                var cam = camObj.GetComponent<Camera>();
                if (cam != null) return cam;
            }

            return Camera.main;
        }

        private void StartChargeStateIfNeeded()
        {
            if (m_ActiveSkill == null || !m_ActiveSkill.UseChargeStateWithIndicator) return;

            GameObject characterObj = m_Character.Get(Args.EMPTY);
            if (characterObj == null) return;

            Character character = characterObj.GetComponent<Character>();
            if (character == null) return;

            if (!m_ActiveSkill.ChargeState.IsValid(character)) return;

            m_ChargeCharacter = character;

            ConfigState config = new ConfigState(
                0.1f, 1f, 1f,
                0.1f,
                0.1f
            );

            _ = character.States.SetState(
                m_ActiveSkill.ChargeState,
                m_ActiveSkill.ChargeStateLayer,
                BlendMode.Blend,
                config
            );

            m_ChargeStateActive = true;
        }

        private void StopChargeStateIfNeeded()
        {
            if (!m_ChargeStateActive || m_ChargeCharacter == null) return;

            int layer = m_ActiveSkill != null ? m_ActiveSkill.ChargeStateLayer : 0;
            m_ChargeCharacter.States.Stop(layer, 0.1f, 0.1f);
            m_ChargeStateActive = false;
            m_ChargeCharacter = null;
        }
    }
}
