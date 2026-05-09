using System;
using GameCreator.Runtime.Characters;
using GameCreator.Runtime.Common;
using UnityEngine;

/// <summary>
/// A no-op GC2 driver unit. Used when a Character component is added passively to a GameObject
/// that manages its own movement (e.g. Tobias TPS with ABC_MovementController). The kernel
/// requires a non-null driver to satisfy Animim's dependency on Character.Driver, but this
/// implementation never moves the character.
/// </summary>
[Title("Passive (No Movement)")]
[Category("Passive/Passive")]
[Description("Inert driver for characters that manage their own movement externally.")]
[Serializable]
public class UnitDriverPassive : TUnitDriver
{
    public override Vector3 WorldMoveDirection => Vector3.zero;
    public override Vector3 LocalMoveDirection => Vector3.zero;
    public override float SkinWidth => 0f;
    public override bool IsGrounded => true;
    public override Vector3 FloorNormal => Vector3.up;
    public override bool Collision { get => false; set { } }
    public override Axonometry Axonometry { get => null; set { } }

    public override void SetPosition(Vector3 position, bool teleport = false) { }
    public override void SetRotation(Quaternion rotation) { }
    public override void SetScale(Vector3 scale) { }
    public override void AddPosition(Vector3 amount) { }
    public override void AddRotation(Quaternion amount) { }
    public override void AddScale(Vector3 scale) { }
    public override void ResetVerticalVelocity() { }
}
