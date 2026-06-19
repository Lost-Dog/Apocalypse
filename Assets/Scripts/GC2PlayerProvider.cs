using System;
using GameCreator.Runtime.Characters;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.Stats;
using UnityEngine;

/// <summary>
/// Bridges Game Creator 2 Character + Traits data into the project-agnostic IPlayerProvider API.
/// This allows SurvivalManager and UI systems to work without framework-specific code.
/// </summary>
[DisallowMultipleComponent]
public class GC2PlayerProvider : MonoBehaviour, IPlayerProvider, ISurvivalStatsProvider
{
    [Header("GC2 References")]
    [SerializeField] private Character gcCharacter;
    [SerializeField] private Traits gcTraits;
    [SerializeField] private bool autoResolveOnStart = true;
    [SerializeField] private bool autoWireToSurvivalManager = true;

    [Header("GC2 Trait IDs")]
    [SerializeField] private string healthAttributeId = "hp";
    [SerializeField] private string armourAttributeId = "armour";
    [SerializeField] private string maxArmourStatId = "max_armour";
    [SerializeField] private string staminaAttributeId = "stamina";
    [SerializeField] private string maxStaminaStatId = "max_stamina";
    [SerializeField] private string hungerAttributeId = "hunger";
    [SerializeField] private string maxHungerStatId = "max_hunger";
    [SerializeField] private string thirstAttributeId = "thirst";
    [SerializeField] private string maxThirstStatId = "max_thirst";
    [SerializeField] private string temperatureAttributeId = "temperature";
    [SerializeField] private string maxTemperatureStatId = "max_temperature";
    [SerializeField] private string infectionAttributeId = "infection";
    [SerializeField] private string maxInfectionStatId = "max_infection";

    [Header("Event Polling")]
    [Tooltip("How often to poll GC2 traits for external changes and emit provider events.")]
    [SerializeField] private float changePollInterval = 0.2f;

    [Header("Trait Direction")]
    [Tooltip("If true, hunger trait is interpreted as inverse (0 = full, max = starving).")]
    [SerializeField] private bool invertHungerTrait = true;
    [Tooltip("If true, thirst trait is interpreted as inverse (0 = hydrated, max = dehydrated).")]
    [SerializeField] private bool invertThirstTrait = true;
    [Tooltip("If true, temperature trait is interpreted as inverse (0 = optimal, max = freezing).")]
    [SerializeField] private bool invertTemperatureTrait = false;

    private static readonly string[] HealthAttributeAliases =
    {
        "hp", "health", "current_health"
    };

    private static readonly string[] MaxArmourStatAliases =
    {
        "max_armour", "max_armor", "armour_max", "armor_max", "armour", "armor"
    };

    private static readonly string[] ArmourAttributeAliases =
    {
        "armour", "armor", "current_armour", "current_armor"
    };

    private static readonly string[] StaminaAttributeAliases =
    {
        "stamina", "current_stamina"
    };

    private static readonly string[] MaxStaminaStatAliases =
    {
        "max_stamina", "stamina_max", "stamina"
    };

    private static readonly string[] HungerAttributeAliases =
    {
        "hunger", "current_hunger"
    };

    private static readonly string[] MaxHungerStatAliases =
    {
        "max_hunger", "hunger_max", "hunger"
    };

    private static readonly string[] ThirstAttributeAliases =
    {
        "thirst", "current_thirst"
    };

    private static readonly string[] MaxThirstStatAliases =
    {
        "max_thirst", "thirst_max", "thirst"
    };

    private static readonly string[] TemperatureAttributeAliases =
    {
        "temperature", "current_temperature"
    };

    private static readonly string[] MaxTemperatureStatAliases =
    {
        "max_temperature", "temperature_max", "temperature"
    };

    private static readonly string[] InfectionAttributeAliases =
    {
        "infection", "immunity", "current_infection", "current_immunity"
    };

    private static readonly string[] MaxInfectionStatAliases =
    {
        "max_infection", "infection_max", "max_immunity", "immunity_max"
    };

    private float currentArmour;
    private float cachedHealth;
    private float cachedMaxHealth;
    private float cachedArmour;
    private float cachedMaxArmour;
    private float changePollTimer;

    public GameObject PlayerObject => this.gameObject;

    public bool IsAlive => this.gcCharacter == null || !this.gcCharacter.IsDead;

    public float Health => GetHealthValue();

    public float MaxHealth => GetMaxHealthValue();

    public float Armour => GetArmourValue();

    public float MaxArmour => Mathf.Max(0f, GetMaxArmourValue());

    public float Shield => Armour;

    public float MaxShield => MaxArmour;

    public float MoveSpeed => GetMoveSpeedValue();

    public float Temperature => GetTemperatureValue();

    public float MaxTemperature => GetMaxStatValue(maxTemperatureStatId, MaxTemperatureStatAliases, 100f, 1f);

    public float Stamina => GetCurrentValue(staminaAttributeId, StaminaAttributeAliases, MaxStamina, 0f);

    public float MaxStamina => GetMaxStatValue(maxStaminaStatId, MaxStaminaStatAliases, 100f, 1f);

    // Immunity semantics: if the trait is missing/unresolved, assume full immunity instead of critical low.
    public float Infection => GetCurrentValue(infectionAttributeId, InfectionAttributeAliases, MaxInfection, MaxInfection);

    public float MaxInfection => GetMaxStatValue(maxInfectionStatId, MaxInfectionStatAliases, 100f, 1f);

    public float Hunger => GetHungerValue();

    public float MaxHunger => GetMaxStatValue(maxHungerStatId, MaxHungerStatAliases, 100f, 1f);

    public float Thirst => GetThirstValue();

    public float MaxThirst => GetMaxStatValue(maxThirstStatId, MaxThirstStatAliases, 100f, 1f);

    public event Action OnDeath;
    public event Action<float, float> OnHealthChanged;
    public event Action<float, float> OnArmourChanged;

    private void Awake()
    {
        ResolveDependencies();
        InitializeArmourFromTraits();
        CacheSnapshot();
        TryAutoWireSurvivalManager();
    }

    private void OnEnable()
    {
        ResolveDependencies();
        TryAutoWireSurvivalManager();
        changePollTimer = 0f;

        if (gcCharacter != null)
            gcCharacter.EventDie += HandleCharacterDied;
    }

    private void OnDisable()
    {
        if (gcCharacter != null)
            gcCharacter.EventDie -= HandleCharacterDied;
    }

    private void Update()
    {
        if (autoResolveOnStart && (gcCharacter == null || gcTraits == null))
            ResolveDependencies();

        if (autoWireToSurvivalManager)
            TryAutoWireSurvivalManager();

        if (OnHealthChanged == null && OnArmourChanged == null)
            return;

        changePollTimer -= Time.deltaTime;
        if (changePollTimer > 0f)
            return;

        changePollTimer = Mathf.Max(0.05f, changePollInterval);
        EmitChangeEventsIfNeeded();
    }

    public void SetHealth(float value)
    {
        RuntimeAttributeData health = ResolveHealthAttribute();
        if (health == null) return;

        health.Value = value;

        if (health.Value <= float.Epsilon && gcCharacter != null && !gcCharacter.IsDead)
            gcCharacter.IsDead = true;

        EmitChangeEventsIfNeeded(forceHealthEvent: true);
    }

    public void ApplyDamage(float amount)
    {
        if (Mathf.Approximately(amount, 0f)) return;

        if (amount < 0f)
        {
            SetHealth(Health - amount);
            return;
        }

        float remainingDamage = amount;
        float maxArmour = MaxArmour;

        if (maxArmour > 0f)
        {
            float armourThreshold = maxArmour * 0.25f;
            float availableArmour = Mathf.Max(0f, Armour - armourThreshold);

            if (availableArmour > 0f)
            {
                float absorbed = Mathf.Min(remainingDamage, availableArmour);
                SetArmour(Armour - absorbed);
                remainingDamage -= absorbed;
            }
        }

        if (remainingDamage > 0f)
            SetHealth(Health - remainingDamage);
    }

    public void SetArmour(float value)
    {
        float previous = Armour;
        SetArmourValue(value);

        float current = Armour;
        if (!Mathf.Approximately(previous, current))
            OnArmourChanged?.Invoke(current, MaxArmour);
    }

    public void SetShield(float value)
    {
        SetArmour(value);
    }

    public void SetTemperature(float value)
    {
        float clamped = Mathf.Clamp(value, 0f, MaxTemperature);

        if (invertTemperatureTrait)
        {
            clamped = Mathf.Clamp(MaxTemperature - clamped, 0f, MaxTemperature);
        }

        SetCurrentValue(temperatureAttributeId, TemperatureAttributeAliases, clamped, MaxTemperature);
    }

    public void SetStamina(float value)
    {
        SetCurrentValue(staminaAttributeId, StaminaAttributeAliases, value, MaxStamina);
    }

    public void SetInfection(float value)
    {
        SetCurrentValue(infectionAttributeId, InfectionAttributeAliases, value, MaxInfection);
    }

    public void SetHunger(float value)
    {
        float clamped = Mathf.Clamp(value, 0f, MaxHunger);

        if (invertHungerTrait)
        {
            clamped = Mathf.Clamp(MaxHunger - clamped, 0f, MaxHunger);
        }

        SetCurrentValue(hungerAttributeId, HungerAttributeAliases, clamped, MaxHunger);
    }

    public void SetThirst(float value)
    {
        float clamped = Mathf.Clamp(value, 0f, MaxThirst);

        if (invertThirstTrait)
        {
            clamped = Mathf.Clamp(MaxThirst - clamped, 0f, MaxThirst);
        }

        SetCurrentValue(thirstAttributeId, ThirstAttributeAliases, clamped, MaxThirst);
    }

    private void ResolveDependencies()
    {
        if (!autoResolveOnStart) return;

        if (gcCharacter == null)
        {
            gcCharacter = GetComponent<Character>();

            if (gcCharacter == null)
            {
                Character[] characters = FindObjectsByType<Character>(FindObjectsSortMode.None);
                for (int i = 0; i < characters.Length; i++)
                {
                    if (characters[i] != null && characters[i].IsPlayer)
                    {
                        gcCharacter = characters[i];
                        break;
                    }
                }

                if (gcCharacter == null)
                    gcCharacter = FindFirstObjectByType<Character>();
            }
        }

        if (gcTraits == null)
        {
            gcTraits = GetComponent<Traits>();

            if (gcTraits == null && gcCharacter != null)
                gcTraits = gcCharacter.GetComponent<Traits>();

            if (gcTraits == null)
                gcTraits = FindFirstObjectByType<Traits>();
        }
    }

    private void InitializeArmourFromTraits()
    {
        currentArmour = Mathf.Max(currentArmour, MaxArmour);
        currentArmour = Mathf.Clamp(currentArmour, 0f, MaxArmour);
    }

    private void CacheSnapshot()
    {
        cachedHealth = Health;
        cachedMaxHealth = MaxHealth;
        cachedArmour = Armour;
        cachedMaxArmour = MaxArmour;
    }

    private void EmitChangeEventsIfNeeded(bool forceHealthEvent = false)
    {
        float health = Health;
        float maxHealth = MaxHealth;

        if (forceHealthEvent || !Mathf.Approximately(health, cachedHealth) || !Mathf.Approximately(maxHealth, cachedMaxHealth))
        {
            cachedHealth = health;
            cachedMaxHealth = maxHealth;
            OnHealthChanged?.Invoke(health, maxHealth);
        }

        float maxArmour = MaxArmour;
        float armour = Armour;
        if (!Mathf.Approximately(armour, cachedArmour) || !Mathf.Approximately(maxArmour, cachedMaxArmour))
        {
            cachedArmour = armour;
            cachedMaxArmour = maxArmour;
            OnArmourChanged?.Invoke(armour, maxArmour);
        }
    }

    private void HandleCharacterDied()
    {
        OnDeath?.Invoke();
    }

    private void TryAutoWireSurvivalManager()
    {
        SurvivalManager survivalManager = SurvivalManager.Instance ?? FindFirstObjectByType<SurvivalManager>();
        if (survivalManager == null) return;

        if (survivalManager.playerProviderObject != this)
            survivalManager.playerProviderObject = this;

        survivalManager.EnsurePlayerProviderBinding();
    }

    private RuntimeAttributeData ResolveHealthAttribute()
    {
        return ResolveAttribute(healthAttributeId, HealthAttributeAliases);
    }

    private RuntimeAttributeData ResolveArmourAttribute()
    {
        return ResolveAttribute(armourAttributeId, ArmourAttributeAliases);
    }

    private RuntimeStatData ResolveMaxArmourStat()
    {
        return ResolveStat(maxArmourStatId, MaxArmourStatAliases);
    }

    private RuntimeAttributeData ResolveAttribute(string configuredId, string[] aliases)
    {
        if (gcTraits == null) return null;

        if (!string.IsNullOrWhiteSpace(configuredId))
        {
            RuntimeAttributeData configured = TryGetAttribute(configuredId);
            if (configured != null) return configured;
        }

        for (int i = 0; i < aliases.Length; i++)
        {
            RuntimeAttributeData alias = TryGetAttribute(aliases[i]);
            if (alias != null) return alias;
        }

        return null;
    }

    private RuntimeStatData ResolveStat(string configuredId, string[] aliases)
    {
        if (gcTraits == null) return null;

        if (!string.IsNullOrWhiteSpace(configuredId))
        {
            RuntimeStatData configured = TryGetStat(configuredId);
            if (configured != null) return configured;
        }

        for (int i = 0; i < aliases.Length; i++)
        {
            RuntimeStatData alias = TryGetStat(aliases[i]);
            if (alias != null) return alias;
        }

        return null;
    }

    private RuntimeAttributeData TryGetAttribute(string id)
    {
        if (gcTraits == null || string.IsNullOrWhiteSpace(id)) return null;

        try
        {
            return gcTraits.RuntimeAttributes.Get(new IdString(id));
        }
        catch
        {
            return null;
        }
    }

    private RuntimeStatData TryGetStat(string id)
    {
        if (gcTraits == null || string.IsNullOrWhiteSpace(id)) return null;

        try
        {
            return gcTraits.RuntimeStats.Get(new IdString(id));
        }
        catch
        {
            return null;
        }
    }

    private float GetHealthValue()
    {
        RuntimeAttributeData health = ResolveHealthAttribute();
        return health != null ? (float)health.Value : 0f;
    }

    private float GetMaxHealthValue()
    {
        RuntimeAttributeData health = ResolveHealthAttribute();
        return health != null ? Mathf.Max(1f, (float)health.MaxValue) : 1f;
    }

    private float GetMaxArmourValue()
    {
        RuntimeStatData stat = ResolveMaxArmourStat();
        return stat != null ? Mathf.Max(0f, (float)stat.Value) : 0f;
    }

    private float GetCurrentValue(string configuredId, string[] aliases, float maxValue, float fallback)
    {
        RuntimeAttributeData attribute = ResolveAttribute(configuredId, aliases);
        if (attribute == null) return fallback;

        return Mathf.Clamp((float) attribute.Value, 0f, Mathf.Max(0f, maxValue));
    }

    private float GetHungerValue()
    {
        float maxHunger = MaxHunger;
        float rawValue = GetCurrentValue(hungerAttributeId, HungerAttributeAliases, maxHunger, 0f);

        if (!invertHungerTrait)
        {
            return rawValue;
        }

        return Mathf.Clamp(maxHunger - rawValue, 0f, maxHunger);
    }

    private float GetThirstValue()
    {
        float maxThirst = MaxThirst;
        float rawValue = GetCurrentValue(thirstAttributeId, ThirstAttributeAliases, maxThirst, 0f);

        if (!invertThirstTrait)
        {
            return rawValue;
        }

        return Mathf.Clamp(maxThirst - rawValue, 0f, maxThirst);
    }

    private float GetTemperatureValue()
    {
        float maxTemperature = MaxTemperature;
        float rawValue = GetCurrentValue(temperatureAttributeId, TemperatureAttributeAliases, maxTemperature, maxTemperature);

        if (!invertTemperatureTrait)
        {
            return rawValue;
        }

        return Mathf.Clamp(maxTemperature - rawValue, 0f, maxTemperature);
    }

    private float GetMaxStatValue(string configuredId, string[] aliases, float fallback, float minimum)
    {
        RuntimeStatData stat = ResolveStat(configuredId, aliases);
        if (stat == null) return Mathf.Max(minimum, fallback);

        return Mathf.Max(minimum, (float) stat.Value);
    }

    private void SetCurrentValue(string configuredId, string[] aliases, float value, float maxValue)
    {
        RuntimeAttributeData attribute = ResolveAttribute(configuredId, aliases);
        if (attribute == null) return;

        attribute.Value = Mathf.Clamp(value, 0f, Mathf.Max(0f, maxValue));
    }

    private float GetArmourValue()
    {
        RuntimeAttributeData armour = ResolveArmourAttribute();
        if (armour != null)
            return Mathf.Clamp((float) armour.Value, 0f, MaxArmour);

        return Mathf.Clamp(currentArmour, 0f, MaxArmour);
    }

    private void SetArmourValue(float value)
    {
        float clamped = Mathf.Clamp(value, 0f, MaxArmour);

        RuntimeAttributeData armour = ResolveArmourAttribute();
        if (armour != null)
        {
            armour.Value = clamped;
            return;
        }

        currentArmour = clamped;
    }

    private float GetMoveSpeedValue()
    {
        if (gcCharacter?.Driver != null)
            return gcCharacter.Driver.WorldMoveDirection.magnitude;

        return 0f;
    }
}
