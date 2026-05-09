using System;
using GameCreator.Runtime.Characters;
using GameCreator.Runtime.Common;
using UnityEngine;

/// <summary>
/// A no-op GC2 motion unit. Used alongside UnitDriverPassive when a Character component is
/// added passively to a GameObject that manages its own movement (e.g. Tobias TPS with ABC).
/// Returns safe default values for every property so UnitAnimimKinematic.OnUpdate (which reads
/// Character.Motion.Height and Character.Motion.JumpForce) does not null-reference.
/// </summary>
[Title("Passive (No Motion)")]
[Category("Passive/Passive")]
[Description("Inert motion unit for characters that manage their own movement externally.")]
[Serializable]
public class UnitMotionPassive : TUnitMotion
{
    private const float DEFAULT_HEIGHT = 1.8f;
    private const float DEFAULT_RADIUS = 0.4f;
    private const float DEFAULT_MASS   = 70f;

    public override float LinearSpeed  { get => 0f; set { } }
    public override float AngularSpeed { get => 0f; set { } }

    public override float GravityUpwards   { get => 0f; set { } }
    public override float GravityDownwards { get => 0f; set { } }
    public override float TerminalVelocity { get => 0f; set { } }

    public override float JumpForce    { get => 0f; set { } }
    public override float JumpCooldown { get => 0f; set { } }

    public override int   DashInSuccession { get => 0;     set { } }
    public override bool  DashInAir        { get => false; set { } }
    public override float DashCooldown     { get => 0f;    set { } }

    public override float Mass   { get => DEFAULT_MASS;   set { } }
    public override float Height { get => DEFAULT_HEIGHT; set { } }
    public override float Radius { get => DEFAULT_RADIUS; set { } }

    public override bool  UseAcceleration { get => false; set { } }
    public override float Acceleration    { get => 0f;   set { } }
    public override float Deceleration    { get => 0f;   set { } }

    public override bool CanJump  { get => false; set { } }
    public override int  AirJumps { get => 0;     set { } }

    /// <summary>Suppress the motion update entirely — ABC drives movement externally.</summary>
    public override void OnUpdate() { }

    public override void OnEnable()  { }
    public override void OnDisable() { }
}
