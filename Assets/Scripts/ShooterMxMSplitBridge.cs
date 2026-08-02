using GameCreator.Runtime.Characters;
using GameCreator.Runtime.Shooter;
using Threepeat;
using UnityEngine;

[DisallowMultipleComponent]
public class ShooterMxMSplitBridge : MonoBehaviour
{
    [SerializeField] private Character gcCharacter;
    [SerializeField] private MMCGameCreator2 mmcBridge;
    [SerializeField] private NGCharacter ngCharacter;
    [SerializeField] private float blendDuration = 0.15f;
    [SerializeField] private bool keepGCCharacterEnabledInMxMMode = true;
    [SerializeField] private bool keepGCPlayerControllableInMxMMode = true;
    [SerializeField] private bool enforceWhileShooterWeaponEquipped = true;
    [SerializeField] private float enforceInterval = 0.5f;

    private bool originalKeepGCEnabledInMxM;
    private bool originalKeepGCPlayerControllableInMxM;
    private bool hasCapturedDefaults;
    private bool shooterWeaponEquipped;
    private bool shooterAimActive;
    private bool isPlayerDead;
    private float nextEnforceTime;

    // Strafe state captured before the first weapon equip so it can be restored on unequip.
    private bool strafeStateBeforeWeapon;
    private bool hasCapturedStrafeState;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void AutoAttachToPlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;

        if (player.GetComponent<ShooterMxMSplitBridge>() == null)
            player.AddComponent<ShooterMxMSplitBridge>();
    }

    private void Awake()
    {
        ResolveReferences();
        CaptureDefaults();
    }

    private void OnEnable()
    {
        ResolveReferences();
        CaptureDefaults();
        Subscribe();

        shooterWeaponEquipped = gcCharacter != null && gcCharacter.Combat.GetActiveWeapon<ShooterWeapon>() != null;
        shooterAimActive = IsShooterAimActive();
        if (shooterAimActive)
            ApplyShooterMxMSplit();
        else if (shooterWeaponEquipped)
            RestoreDefaults();

        isPlayerDead = gcCharacter != null && gcCharacter.IsDead;
        if (isPlayerDead)
            ApplyDeathControlLock();
    }

    private void OnDisable()
    {
        Unsubscribe();

        if (shooterWeaponEquipped)
            RestoreDefaults(immediate: true);

        // Always restore the original strafe state when this bridge is disabled.
        RestoreStrafeState();
        hasCapturedStrafeState = false;

        shooterWeaponEquipped = false;
        shooterAimActive = false;
    }

    private void Update()
    {
        if (gcCharacter != null)
        {
            bool weaponEquippedNow = gcCharacter.Combat.GetActiveWeapon<ShooterWeapon>() != null;
            bool aimActiveNow = weaponEquippedNow && IsShooterAimActive();

            if (weaponEquippedNow != shooterWeaponEquipped || aimActiveNow != shooterAimActive)
            {
                shooterWeaponEquipped = weaponEquippedNow;
                shooterAimActive = aimActiveNow;

                if (shooterAimActive)
                    ApplyShooterMxMSplit();
                else
                    RestoreDefaults();
            }

            if (!isPlayerDead && gcCharacter.IsDead)
            {
                isPlayerDead = true;
                ApplyDeathControlLock();
            }
            else if (isPlayerDead && !gcCharacter.IsDead)
            {
                isPlayerDead = false;

                if (shooterAimActive)
                    ApplyShooterMxMSplit();
                else
                    RestoreDefaults();
            }
        }

        if (isPlayerDead)
            return;

        if (!enforceWhileShooterWeaponEquipped || !shooterAimActive)
            return;

        if (Time.time < nextEnforceTime)
            return;

        nextEnforceTime = Time.time + Mathf.Max(0.1f, enforceInterval);

        if (mmcBridge != null && !mmcBridge.IsMxMContributing())
            ApplyShooterMxMSplit(0f);
    }

    private void ResolveReferences()
    {
        if (gcCharacter == null)
            gcCharacter = GetComponent<Character>();

        if (mmcBridge == null)
            mmcBridge = GetComponent<MMCGameCreator2>();

        if (ngCharacter == null)
            ngCharacter = GetComponent<NGCharacter>();
    }

    private void CaptureDefaults()
    {
        if (hasCapturedDefaults || mmcBridge == null)
            return;

        originalKeepGCEnabledInMxM = mmcBridge.keepGCCharacterEnabledForPlayerInMxMMode;
        originalKeepGCPlayerControllableInMxM = mmcBridge.keepGCPlayerControllableInMxMMode;
        hasCapturedDefaults = true;
    }

    private void Subscribe()
    {
        if (gcCharacter == null) return;

        gcCharacter.Combat.EventEquip -= OnEquip;
        gcCharacter.Combat.EventUnequip -= OnUnequip;
        gcCharacter.EventDie -= OnCharacterDied;

        gcCharacter.Combat.EventEquip += OnEquip;
        gcCharacter.Combat.EventUnequip += OnUnequip;
        gcCharacter.EventDie += OnCharacterDied;
    }

    private void Unsubscribe()
    {
        if (gcCharacter == null) return;

        gcCharacter.Combat.EventEquip -= OnEquip;
        gcCharacter.Combat.EventUnequip -= OnUnequip;
        gcCharacter.EventDie -= OnCharacterDied;
    }

    private void OnCharacterDied()
    {
        isPlayerDead = true;
        ApplyDeathControlLock();
    }

    private void OnEquip(IWeapon weapon, GameObject instance)
    {
        if (weapon is not ShooterWeapon)
            return;

        // Capture the pre-weapon strafe state exactly once so we can restore it on unequip.
        if (!hasCapturedStrafeState && ngCharacter != null)
        {
            strafeStateBeforeWeapon = ngCharacter.Strafing;
            hasCapturedStrafeState = true;
        }

        shooterWeaponEquipped = true;
        shooterAimActive = IsShooterAimActive();

        if (shooterAimActive)
            ApplyShooterMxMSplit();
        else
            RestoreDefaults();
    }

    private void OnUnequip(IWeapon weapon, GameObject instance)
    {
        if (weapon is not ShooterWeapon)
            return;

        shooterWeaponEquipped = gcCharacter != null && gcCharacter.Combat.GetActiveWeapon<ShooterWeapon>() != null;
        shooterAimActive = shooterWeaponEquipped && IsShooterAimActive();

        if (shooterAimActive)
        {
            ApplyShooterMxMSplit();
        }
        else
        {
            RestoreDefaults();

            // Restore the strafe state that was active before any weapon was equipped,
            // but only once all shooter weapons are fully unequipped.
            if (!shooterWeaponEquipped)
            {
                RestoreStrafeState();
                hasCapturedStrafeState = false;
            }
        }
    }

    private void ApplyShooterMxMSplit()
    {
        ApplyShooterMxMSplit(blendDuration);
    }

    private void ApplyShooterMxMSplit(float duration)
    {
        if (mmcBridge == null)
            return;

        mmcBridge.keepGCCharacterEnabledForPlayerInMxMMode = keepGCCharacterEnabledInMxMMode;
        mmcBridge.keepGCPlayerControllableInMxMMode = keepGCPlayerControllableInMxMMode;

        mmcBridge.SetMxMAnimatorBlendWeight(1f, Mathf.Max(0f, duration), false);

        // Enable strafing only when the player is actively aiming.
        SetStrafe(true);
    }

    private void ApplyDeathControlLock()
    {
        if (mmcBridge == null)
            return;

        mmcBridge.keepGCPlayerControllableInMxMMode = false;
    }

    private void RestoreDefaults()
    {
        RestoreDefaults(immediate: false);
    }

    private void RestoreDefaults(bool immediate)
    {
        if (mmcBridge == null || !hasCapturedDefaults)
            return;

        mmcBridge.keepGCCharacterEnabledForPlayerInMxMMode = originalKeepGCEnabledInMxM;
        mmcBridge.keepGCPlayerControllableInMxMMode = originalKeepGCPlayerControllableInMxM;

        // MMC starts a coroutine only when duration > 0. During OnDisable we force immediate restore (duration 0).
        float duration = immediate ? 0f : Mathf.Max(0f, blendDuration);
        mmcBridge.SetMxMAnimatorBlendWeight(0f, duration, false);

        // When not aiming, disable strafe so normal forward locomotion resumes.
        SetStrafe(false);
    }

    /// <summary>Sets NGCharacter.Strafing if the reference is available.</summary>
    private void SetStrafe(bool strafe)
    {
        if (ngCharacter == null)
            return;

        ngCharacter.Strafing = strafe;
    }

    /// <summary>Restores the strafe state that was captured before the weapon was equipped.</summary>
    private void RestoreStrafeState()
    {
        if (ngCharacter == null || !hasCapturedStrafeState)
            return;

        ngCharacter.Strafing = strafeStateBeforeWeapon;
    }

    private bool IsShooterAimActive()
    {
        if (gcCharacter == null)
            return false;

        ShooterWeapon activeWeapon = gcCharacter.Combat.GetActiveWeapon<ShooterWeapon>();
        if (activeWeapon == null)
            return false;

        ShooterStance stance = gcCharacter.Combat.RequestStance<ShooterStance>();
        WeaponData weaponData = stance != null ? stance.Get(activeWeapon) : null;
        if (weaponData == null)
            return false;

        return weaponData.SightId != activeWeapon.Sights.DefaultId;
    }
}
