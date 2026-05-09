using System;
using GameCreator.Runtime.Characters;
using GameCreator.Runtime.Common;

/// <summary>
/// A no-op GC2 player unit. Used when a Character component is added passively to a GameObject
/// whose input and movement is managed externally (e.g. Tobias TPS with ABC Toolkit).
///
/// ABC's GC2 integration reads Character.Player.IsControllable to enable/disable control during
/// ability casts. This stub exposes that property while suppressing all GC2 input processing.
/// </summary>
[Title("Passive (No Input)")]
[Category("Passive/Passive")]
[Description("Inert player unit for characters that manage their own input externally.")]
[Serializable]
public class UnitPlayerPassive : TUnitPlayer
{
    // TUnitPlayer has no abstract members. All virtual lifecycle methods (OnUpdate, OnEnable,
    // OnDisable, etc.) are already no-ops in the base class — nothing to override.
    // IsControllable is a concrete get/set property on TUnitPlayer, so ABC can freely read
    // and write it without any GC2 input machinery being active.
}
