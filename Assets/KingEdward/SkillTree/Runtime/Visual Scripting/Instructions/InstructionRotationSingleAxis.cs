using System;
using System.Threading.Tasks;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using UnityEngine;

namespace KingEdward.SkillTree.Instructions
{
    [Title("Change Rotation Single Axis")]
    [Description("Rotates around one axis only to look at the target")]

    [Image(typeof(IconRotation), ColorTheme.Type.Yellow)]

    [Category("KingEdward/Transforms/Change Rotation Single Axis")]

    [Parameter("Target", "Position or GameObject to look at")]
    [Parameter("Axis", "Which axis to rotate around (Y = horizontal, X = pitch, Z = roll)")]
    [Parameter("Space", "World or Local")]
    [Parameter("Transition", "Duration and easing")]

    [Keywords("Look", "Face", "Target", "Axis", "Turret")]
    [Serializable]
    public class InstructionRotationSingleAxis : TInstructionTransform
    {
        private enum AxisMode
        {
            X,
            Y,
            Z
        }

        private enum SpaceMode
        {
            World,
            Local
        }

        [SerializeField] private PropertyGetPosition m_Target = GetPositionTarget.Create();
        [SerializeField] private AxisMode m_Axis = AxisMode.Y;
        [SerializeField] private SpaceMode m_Space = SpaceMode.World;
        [SerializeField] private Transition m_Transition = new Transition();

        public override string Title => $"{this.m_Transform} look at {this.m_Target} ({m_Axis}-axis)";

        protected override async Task Run(Args args)
        {
            GameObject go = this.m_Transform.Get(args);
            if (go == null) return;

            Vector3 targetPos = this.m_Target.Get(args);
            Transform t = go.transform;

            Quaternion currentRot = m_Space == SpaceMode.World ? t.rotation : t.localRotation;
            Vector3 eulerSource = currentRot.eulerAngles;
            Vector3 pos = t.position;

            Vector3 toTarget = targetPos - pos;
            float valueTarget = GetLookAtAngle(toTarget, t, m_Axis);

            float valueSource = m_Axis switch
            {
                AxisMode.X => eulerSource.x,
                AxisMode.Y => eulerSource.y,
                AxisMode.Z => eulerSource.z,
                _ => 0f
            };

            int axisIndex = m_Axis switch { AxisMode.X => 0, AxisMode.Y => 1, AxisMode.Z => 2, _ => 1 };

            ITweenInput tween = new TweenInput<float>(
                valueSource,
                valueTarget,
                this.m_Transition.Duration,
                (a, b, tParam) =>
                {
                    float lerped = Mathf.LerpAngle(a, b, tParam);
                    Vector3 newEuler = eulerSource;
                    newEuler[axisIndex] = lerped;
                    Quaternion newRot = Quaternion.Euler(newEuler);
                    if (m_Space == SpaceMode.World)
                        t.rotation = newRot;
                    else
                        t.localRotation = newRot;
                },
                Tween.GetHash(typeof(Transform), "look-at-axis-" + m_Axis),
                this.m_Transition.EasingType,
                this.m_Transition.Time
            );

            Tween.To(go, tween);
            if (this.m_Transition.WaitToComplete) await this.Until(() => tween.IsFinished);
        }

        private static float GetLookAtAngle(Vector3 toTarget, Transform t, AxisMode axis)
        {
            if (toTarget.sqrMagnitude < 0.0001f) return 0f;

            switch (axis)
            {
                case AxisMode.Y:
                {
                    Vector3 flat = toTarget;
                    flat.y = 0f;
                    if (flat.sqrMagnitude < 0.0001f) return t.eulerAngles.y;
                    return Mathf.Atan2(flat.x, flat.z) * Mathf.Rad2Deg;
                }
                case AxisMode.X:
                {
                    float horizontal = new Vector3(toTarget.x, 0f, toTarget.z).magnitude;
                    if (horizontal < 0.0001f) return t.eulerAngles.x;
                    return -Mathf.Atan2(toTarget.y, horizontal) * Mathf.Rad2Deg;
                }
                case AxisMode.Z:
                {
                    Vector3 right = new Vector3(toTarget.x, 0f, toTarget.z);
                    if (right.sqrMagnitude < 0.0001f) return t.eulerAngles.z;
                    return Mathf.Atan2(toTarget.y, right.magnitude) * Mathf.Rad2Deg;
                }
                default:
                    return 0f;
            }
        }
    }
}
