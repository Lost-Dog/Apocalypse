using System;
using GameCreator.Runtime.Characters;
using GameCreator.Runtime.Common;
using UnityEngine;

/// <summary>
/// Animim unit for characters whose movement and animations are managed entirely by an external
/// system (e.g. ABC Toolkit / Tobias TPS).
///
/// Extends TUnitAnimim directly — NOT UnitAnimimKinematic — to avoid the base.OnUpdate()
/// call chain that adds AnimimAnimatorProxy and calls OnUpdateModelLocation:
///
///   UnitAnimimKinematic.OnUpdate()
///     └─ base.OnUpdate()  [TUnitAnimim.OnUpdate]
///           ├─ RequireAnimatorProxy()  → adds AnimimAnimatorProxy MonoBehaviour
///           │     └─ OnAnimatorMove callback: forces applyRootMotion = true EVERY frame
///           └─ OnUpdateModelLocation() → ApplyMannequinPosition()
///                 └─ sets Mannequin.localPosition = Vector3.up * (Height * -0.5)
///                       When Mannequin == root, this pushes CharacterController underground.
///
/// Fixes:
///   1. Root-motion snap-back: by never calling RequireAnimatorProxy, no proxy is added,
///      so OnAnimatorMove is never called and applyRootMotion stays false.
///   2. Running-in-place / mannequin offset: by never calling OnUpdateModelLocation, the
///      root transform localPosition is never overwritten by GC2.
/// </summary>
[Title("Passive (External Movement)")]
[Category("Passive/Passive")]
[Description("Animim unit that lets ABC own the transform entirely. Writes Animator parameters so GC2 modules (Melee, Stats, Quests) work correctly.")]
[Serializable]
public class UnitAnimimPassive : TUnitAnimim
{
    private const string PROXY_TYPE_NAME = "GameCreator.Runtime.Characters.AnimimAnimatorProxy";

    // GC2 standard Animator parameter hashes.
    private static readonly int K_SPEED_X  = Animator.StringToHash("Speed-X");
    private static readonly int K_SPEED_Y  = Animator.StringToHash("Speed-Y");
    private static readonly int K_SPEED_Z  = Animator.StringToHash("Speed-Z");
    private static readonly int K_SPEED_XZ = Animator.StringToHash("Speed-XZ");
    private static readonly int K_SPEED_YZ = Animator.StringToHash("Speed-YZ");
    private static readonly int K_SPEED_XY = Animator.StringToHash("Speed-XY");
    private static readonly int K_INTENT_X = Animator.StringToHash("Intent-X");
    private static readonly int K_INTENT_Y = Animator.StringToHash("Intent-Y");
    private static readonly int K_INTENT_Z = Animator.StringToHash("Intent-Z");
    private static readonly int K_SPEED    = Animator.StringToHash("Speed");
    private static readonly int K_PIVOT    = Animator.StringToHash("Pivot");
    private static readonly int K_GROUNDED = Animator.StringToHash("Grounded");
    private static readonly int K_STAND    = Animator.StringToHash("Stand");

    private const float DECAY_PIVOT    = 5f;
    private const float DECAY_GROUNDED = 10f;
    private const float DECAY_STAND    = 5f;

    // LIFECYCLE: ---------------------------------------------------------------------------------

    public override void OnStartup(Character character)
    {
        base.OnStartup(character);
        ForceNoRootMotion();
    }

    public override void AfterStartup(Character character)
    {
        base.AfterStartup(character);
        PurgeAnimatorProxy();
        ForceNoRootMotion();
    }

    public override void OnEnable()
    {
        base.OnEnable();
        PurgeAnimatorProxy();
        ForceNoRootMotion();
    }

    // UPDATE: ------------------------------------------------------------------------------------

    /// <summary>
    /// Writes GC2 Animator parameters so GC2 modules (Melee, Stats, Quests) read correct
    /// values. Does NOT call base.OnUpdate() — that would invoke RequireAnimatorProxy()
    /// (re-adds the proxy MonoBehaviour) and OnUpdateModelLocation() (overwrites root
    /// localPosition every frame, sinking the CharacterController underground).
    /// </summary>
    public override void OnUpdate()
    {
        // Defensively purge any proxy that may have been added before this unit was installed.
        PurgeAnimatorProxy();
        ForceNoRootMotion();

        if (this.Animator == null) return;
        if (!this.Animator.gameObject.activeInHierarchy) return;

        this.Animator.updateMode = this.Character.Time.UpdateTime == TimeMode.UpdateMode.GameTime
            ? AnimatorUpdateMode.Normal
            : AnimatorUpdateMode.UnscaledTime;

        IUnitMotion motion = this.Character.Motion;
        IUnitDriver driver = this.Character.Driver;
        IUnitFacing facing = this.Character.Facing;

        Vector3 intent = motion.LinearSpeed > float.Epsilon
            ? Vector3.ClampMagnitude(
                this.Transform.InverseTransformDirection(motion.MoveDirection) / motion.LinearSpeed, 1f)
            : Vector3.zero;

        Vector3 speed = motion.LinearSpeed > float.Epsilon
            ? driver.LocalMoveDirection / motion.LinearSpeed
            : Vector3.zero;

        float pivot     = facing.PivotSpeed;
        float deltaTime = this.Character.Time.DeltaTime;
        float decay     = Mathf.Lerp(1f, 25f, this.m_SmoothTime);

        Animator ani = this.Animator;
        ani.SetFloat(K_SPEED_X,  MathUtils.ExponentialDecay(ani.GetFloat(K_SPEED_X),  speed.x,                     decay,          deltaTime));
        ani.SetFloat(K_SPEED_Y,  MathUtils.ExponentialDecay(ani.GetFloat(K_SPEED_Y),  speed.y,                     decay,          deltaTime));
        ani.SetFloat(K_SPEED_Z,  MathUtils.ExponentialDecay(ani.GetFloat(K_SPEED_Z),  speed.z,                     decay,          deltaTime));
        ani.SetFloat(K_SPEED,    MathUtils.ExponentialDecay(ani.GetFloat(K_SPEED),    speed.magnitude,             decay,          deltaTime));
        ani.SetFloat(K_SPEED_XZ, MathUtils.ExponentialDecay(ani.GetFloat(K_SPEED_XZ), speed.XZ().magnitude,        decay,          deltaTime));
        ani.SetFloat(K_SPEED_XY, MathUtils.ExponentialDecay(ani.GetFloat(K_SPEED_XY), speed.XY().magnitude,        decay,          deltaTime));
        ani.SetFloat(K_SPEED_YZ, MathUtils.ExponentialDecay(ani.GetFloat(K_SPEED_YZ), speed.YZ().magnitude,        decay,          deltaTime));
        ani.SetFloat(K_INTENT_X, MathUtils.ExponentialDecay(ani.GetFloat(K_INTENT_X), intent.x,                    decay,          deltaTime));
        ani.SetFloat(K_INTENT_Y, MathUtils.ExponentialDecay(ani.GetFloat(K_INTENT_Y), intent.y,                    decay,          deltaTime));
        ani.SetFloat(K_INTENT_Z, MathUtils.ExponentialDecay(ani.GetFloat(K_INTENT_Z), intent.z,                    decay,          deltaTime));
        ani.SetFloat(K_PIVOT,    MathUtils.ExponentialDecay(ani.GetFloat(K_PIVOT),    pivot,                       DECAY_PIVOT,    deltaTime));
        ani.SetFloat(K_GROUNDED, MathUtils.ExponentialDecay(ani.GetFloat(K_GROUNDED), driver.IsGrounded ? 1f : 0f, DECAY_GROUNDED, deltaTime));
        ani.SetFloat(K_STAND,    MathUtils.ExponentialDecay(ani.GetFloat(K_STAND),    motion.StandLevel.Current,   DECAY_STAND,    deltaTime));
    }

    // HELPERS: -----------------------------------------------------------------------------------

    /// <summary>
    /// Immediately destroys any AnimimAnimatorProxy on the Animator's GameObject.
    /// Uses DestroyImmediate so removal takes effect in the same frame.
    /// The proxy type is internal to GC2, matched by FullName.
    /// </summary>
    private void PurgeAnimatorProxy()
    {
        if (this.Animator == null) return;

        MonoBehaviour[] components = this.Animator.gameObject.GetComponents<MonoBehaviour>();
        foreach (MonoBehaviour mb in components)
        {
            if (mb != null && mb.GetType().FullName == PROXY_TYPE_NAME)
            {
                UnityEngine.Object.DestroyImmediate(mb);
            }
        }
    }

    private void ForceNoRootMotion()
    {
        if (this.Animator != null)
        {
            this.Animator.applyRootMotion = false;
        }
    }
}
