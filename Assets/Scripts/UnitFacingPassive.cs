using System;
using GameCreator.Runtime.Characters;
using GameCreator.Runtime.Common;
using UnityEngine;

/// <summary>
/// A no-op GC2 facing unit. Used alongside UnitDriverPassive and UnitMotionPassive when a
/// Character component is added passively to a GameObject that manages its own rotation
/// externally (e.g. Tobias TPS with ABC Toolkit).
///
/// UnitAnimimKinematic.OnUpdate reads Character.Facing.PivotSpeed every frame.
/// This stub returns 0 for PivotSpeed and disables all GC2 rotation logic.
/// </summary>
[Title("Passive (No Facing)")]
[Category("Passive/Passive")]
[Description("Inert facing unit for characters that manage their own rotation externally.")]
[Serializable]
public class UnitFacingPassive : TUnitFacing
{
    public override Axonometry Axonometry { get => null; set { } }

    /// <summary>Keep the character facing whatever direction it already faces.</summary>
    protected override Vector3 GetDefaultDirection()
        => this.Character != null
            ? this.Character.transform.forward
            : Vector3.forward;

    /// <summary>Suppress all GC2 rotation logic — ABC owns rotation.</summary>
    public override void OnUpdate() { }

    public override void OnEnable()  { }
    public override void OnDisable() { }
}
