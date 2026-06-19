using GameCreator.Runtime.Characters;
using GameCreator.Runtime.Shooter;
using Threepeat;
using UnityEngine;

[DisallowMultipleComponent]
public class ShooterMxMSplitBridge : MonoBehaviour
{
    [SerializeField] private Character gcCharacter;
    [SerializeField] private MMCGameCreator2 mmcBridge;
    [SerializeField] private float blendDuration = 0.15f;
    [SerializeField] private bool keepGCCharacterEnabledInMxMMode = true;
    [SerializeField] private bool keepGCPlayerControllableInMxMMode = true;
    [SerializeField] private bool enforceWhileShooterWeaponEquipped = true;
    [SerializeField] private float enforceInterval = 0.5f;

    private bool originalKeepGCEnabledInMxM;
    private bool originalKeepGCPlayerControllableInMxM;
    private bool hasCapturedDefaults;
    private bool shooterWeaponEquipped;
    private bool isPlayerDead;
    private float nextEnforceTime;

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
        if (shooterWeaponEquipped)
            ApplyShooterMxMSplit();

        isPlayerDead = gcCharacter != null && gcCharacter.IsDead;
        if (isPlayerDead)
            ApplyDeathControlLock();
    }

    private void OnDisable()
    {
        Unsubscribe();

        if (shooterWeaponEquipped)
            RestoreDefaults(immediate: true);

        shooterWeaponEquipped = false;
    }

    private void Update()
    {
        if (gcCharacter != null)
        {
            if (!isPlayerDead && gcCharacter.IsDead)
            {
                isPlayerDead = true;
                ApplyDeathControlLock();
            }
            else if (isPlayerDead && !gcCharacter.IsDead)
            {
                isPlayerDead = false;

                if (shooterWeaponEquipped)
                    ApplyShooterMxMSplit();
                else
                    RestoreDefaults();
            }
        }

        if (isPlayerDead)
            return;

        if (!enforceWhileShooterWeaponEquipped || !shooterWeaponEquipped)
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

        shooterWeaponEquipped = true;
        ApplyShooterMxMSplit();
    }

    private void OnUnequip(IWeapon weapon, GameObject instance)
    {
        if (weapon is not ShooterWeapon)
            return;

        shooterWeaponEquipped = gcCharacter != null && gcCharacter.Combat.GetActiveWeapon<ShooterWeapon>() != null;

        if (shooterWeaponEquipped)
        {
            ApplyShooterMxMSplit();
        }
        else
        {
            RestoreDefaults();
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
    }
}
