using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.Characters;
using GameCreator.Runtime.VisualScripting;
using KingEdward;

namespace KingEdward.SkillTree
{
    [Icon(SkillTreePaths.SKILL)]
    [CreateAssetMenu(menuName = "KingEdward/Skill Tree/Skill")]
    public class Skill : ScriptableObject
{
    [SerializeField] private string m_UniqueID = "";
    
    #if UNITY_EDITOR
    [SerializeField, HideInInspector] private string m_AssetGUID = "";
    
    private void OnValidate()
    {
        if (string.IsNullOrEmpty(m_UniqueID))
        {
            m_UniqueID = System.Guid.NewGuid().ToString();
            m_AssetGUID = UnityEditor.AssetDatabase.AssetPathToGUID(UnityEditor.AssetDatabase.GetAssetPath(this));
            UnityEditor.EditorUtility.SetDirty(this);
        }
        else
        {
            string currentGUID = UnityEditor.AssetDatabase.AssetPathToGUID(UnityEditor.AssetDatabase.GetAssetPath(this));
            if (!string.IsNullOrEmpty(currentGUID) && currentGUID != m_AssetGUID)
            {
                m_UniqueID = System.Guid.NewGuid().ToString();
                m_AssetGUID = currentGUID;
                UnityEditor.EditorUtility.SetDirty(this);
            }
        }
    }
    #endif
    
    [SerializeField] private PropertyGetString m_SkillName = new PropertyGetString("New Skill");
    [SerializeField] private PropertyGetString m_Description = new PropertyGetString("Skill Description");
    [SerializeField] private PropertyGetSprite m_Icon = new PropertyGetSprite();
    [SerializeField] private PropertyGetInteger m_Cost = new PropertyGetInteger(0);

    [Tooltip("Skills that must be unlocked and leveled before this skill can be unlocked")]
    [SerializeField] private List<SkillPrerequisite> m_Prerequisites = new List<SkillPrerequisite>();
    
    [SerializeField] private RunInstructionsList m_OnUnlock;
    [SerializeField] private RunInstructionsList m_OnUse;
    [SerializeField] private RunInstructionsList m_OnLevelUp;
    
    [Header("Save/Load Behavior")]
    [Tooltip("When loading a saved game, execute OnUnlock instructions again. Enable for passive skills that modify stats/attributes.")]
    [SerializeField] private bool m_ReapplyOnUnlockOnLoad = true;
    
    [Tooltip("If true, executes level up instructions BEFORE changing the level. If false, executes AFTER changing the level.")]
    [SerializeField] private bool m_ExecuteBeforeChange = false;
    
    [Header("Skill Type")]
    [SerializeField] private bool m_IsActiveSkill = true;
    
    public List<SkillPrerequisite> prerequisites => m_Prerequisites;
    public RunInstructionsList onUnlock => m_OnUnlock;
    public RunInstructionsList onUse => m_OnUse;
    public RunInstructionsList onLevelUp => m_OnLevelUp;
    public bool executeBeforeChange => m_ExecuteBeforeChange;
    public bool isActiveSkill => m_IsActiveSkill;
    public bool reapplyOnUnlockOnLoad => m_ReapplyOnUnlockOnLoad;
    

    [SerializeField] private bool m_UseSequencer = false;
    [SerializeField] private AnimationClip m_AnimationClip;
    [SerializeField] private AvatarMask m_AvatarMask;
    [SerializeField] private bool m_UseRootMotion = false;
    [SerializeField] private SkillTreeSequence m_Sequencer = new SkillTreeSequence();
    
    [Tooltip("If true, plays a charge State while the ground indicator is being aimed.")]
    [SerializeField] private bool m_UseChargeStateWithIndicator = false;
    [SerializeField] private StateData m_ChargeState = new StateData(StateData.StateType.State);
    [SerializeField] private int m_ChargeStateLayer = 0;
    
    [Tooltip("If true, this skill is channelled: holds state and runs On Channel Tick each frame while input/conditions are true.")]
    [SerializeField] private bool m_IsChannelSkill = false;
    [SerializeField] private StateData m_ChannelState = new StateData(StateData.StateType.State);
    [SerializeField] private int m_ChannelStateLayer = 0;
    [Tooltip("Runs every frame while channeling (e.g. spawn projectile, drain mana).")]
    [Header("On Update")]
    [SerializeField] private RunInstructionsList m_OnChannelTick = new RunInstructionsList();
    [Header("Can Channel")]
    [SerializeField] private RunConditionsList m_ChannelConditions = new RunConditionsList();

    public enum ChannelStartMode
    {

        // Start channel state only after the sequencer finishes. 
        AfterSequencer,

        // Start channel state at the end of Cast phase (sequencer normalized time).
        AtCastEnd,

        // Start channel state at the end of Release phase (sequencer normalized time).
        AtReleaseEnd,

        // Start channel state at a custom sequencer normalized time (0-1).
        CustomNormalizedTime
    }

    [Tooltip("When both Sequencer and Channel are enabled, controls when the ChannelState starts. AfterSequencer keeps the current behaviour.")]
    [SerializeField] private ChannelStartMode m_ChannelStartMode = ChannelStartMode.AfterSequencer;

    [Tooltip("Custom normalized time (0-1) on the sequencer timeline for ChannelStartMode = CustomNormalizedTime.")]
    [SerializeField] [Range(0f, 1f)] private float m_ChannelStartCustomNormalizedTime = 0.66f;

    [Tooltip("If true, stops Character gestures when ChannelState starts (useful to avoid animation continuing after channel begins).")]
    [SerializeField] private bool m_StopGestureOnChannelStart = true;

    [Tooltip("Delay before stopping gestures when ChannelState starts.")]
    [SerializeField] [Range(0f, 1f)] private float m_StopGestureDelay = 0f;

    [Tooltip("Blend time when stopping gestures when ChannelState starts.")]
    [SerializeField] [Range(0f, 1f)] private float m_StopGestureTransition = 0.1f;


    [Tooltip("Can this skill interrupt other skills that are currently executing?")]
    [SerializeField] private bool m_CanInterruptOthers = false;
    [Tooltip("Can this skill be interrupted by other skills?")]
    [SerializeField] private bool m_CanBeInterrupted = true;

    [Tooltip("Optional: shows an AOE/area indicator on the ground when aiming this skill.")]
    [SerializeField] private SkillIndicatorConfig m_IndicatorConfig = new SkillIndicatorConfig();

    public bool UseSequencer => m_UseSequencer;
    public AvatarMask AvatarMask => m_AvatarMask;
    public bool UseRootMotion => m_UseRootMotion;
    public AnimationClip AnimationClip => m_AnimationClip;
    public SkillTreeSequence Sequencer => m_Sequencer;
    
    public bool UseChargeStateWithIndicator => m_UseChargeStateWithIndicator;
    public StateData ChargeState => m_ChargeState;
    public int ChargeStateLayer => m_ChargeStateLayer;
    
    public bool IsChannelSkill => m_IsChannelSkill;
    public StateData ChannelState => m_ChannelState;
    public int ChannelStateLayer => m_ChannelStateLayer;
    public RunInstructionsList OnChannelTick => m_OnChannelTick;
    public RunConditionsList ChannelConditions => m_ChannelConditions;
    public bool CanInterruptOthers => m_CanInterruptOthers;
    public bool CanBeInterrupted => m_CanBeInterrupted;
    public SkillIndicatorConfig IndicatorConfig => m_IndicatorConfig;
    

    [Header("Cooldown & Stacks")]
    [SerializeField] private PropertyGetDecimal m_CooldownDuration = new PropertyGetDecimal(3.0);
    [Tooltip("If true, this skill has stacks: can be used X times (and/or within time window) before cooldown starts.")]
    [SerializeField] private bool m_HasStacks = false;
    [Tooltip("Number of uses allowed before cooldown.")]
    [SerializeField] private PropertyGetDecimal m_StackUsesBeforeCooldown = new PropertyGetDecimal(1.0);
    [Tooltip("If true, cooldown also starts when this many seconds have passed since the first use in the window.")]
    [SerializeField] private bool m_UseStackTime = false;
    [SerializeField] private PropertyGetDecimal m_StackWindowDuration = new PropertyGetDecimal(0.0);
    [SerializeField] private int m_MaxLevel = 5;
    [SerializeField] private bool m_CanLevelUp = true;
    [SerializeField] private RunConditionsList m_CanUnlock = new RunConditionsList();
    [SerializeField] private RunConditionsList m_CanUse = new RunConditionsList();
    [SerializeField] private RunConditionsList m_CanLevelUpConditions = new RunConditionsList();
    
    [Tooltip("Define specific conditions and instructions for each level.")]
    [SerializeField] private List<SpecificConditionData> m_SpecificLevelUp = new List<SpecificConditionData>();
    
    public int maxLevel => m_MaxLevel;
    public bool canLevelUp => m_CanLevelUp;
    public RunConditionsList CanLevelUpConditions => m_CanLevelUpConditions;
    
    
    /// <summary>
    /// Unique identifier for this skill (used for save/load).
    /// </summary>
    public string UniqueID
    {
        get
        {
            #if UNITY_EDITOR
            // Auto-generate ID in editor if missing
            if (string.IsNullOrEmpty(m_UniqueID))
            {
                m_UniqueID = System.Guid.NewGuid().ToString();
                UnityEditor.EditorUtility.SetDirty(this);
            }
            #endif
            return m_UniqueID;
        }
    }
    
    public string SkillName
    {
        get => m_SkillName.Get(Args.EMPTY);
    }
    
    public string Description
    {
        get => m_Description.Get(Args.EMPTY);
    }
    
    public Sprite Icon
    {
        get => m_Icon.Get(Args.EMPTY);
    }
    
    public int Cost
    {
        get => (int)m_Cost.Get(Args.EMPTY);
    }
    

    public int GetCost(Args args)
    {
        return (int)m_Cost.Get(args);
    }
    
    public float CooldownDuration => (float)m_CooldownDuration.Get(Args.EMPTY);
    public bool HasStacks => m_HasStacks;
    public int StackUsesBeforeCooldown => (int)m_StackUsesBeforeCooldown.Get(Args.EMPTY);
    public bool UseStackTime => m_UseStackTime;
    public float StackWindowDuration => (float)m_StackWindowDuration.Get(Args.EMPTY);
    


    public bool CheckLevelUpConditions(int currentLevel, Args args)
    {
        int nextLevel = currentLevel + 1;
        
        // Check if there are specific conditions for the next level
        var specificCondition = GetSpecificConditionForLevel(nextLevel);
        if (specificCondition != null)
        {
            // Check specific conditions
            bool specificConditionsMet = specificCondition.CheckConditions(args);
            
            // If should use general conditions too
            if (specificCondition.useGeneralConditionsToo)
            {
                bool generalConditionsMet = (m_CanLevelUpConditions == null) || m_CanLevelUpConditions.Check(args);
                return specificConditionsMet && generalConditionsMet;
            }
            else
            {
                // Use only specific conditions
                return specificConditionsMet;
            }
        }
        
        // Otherwise, use only general conditions
        if (m_CanLevelUpConditions == null) return true;
        return m_CanLevelUpConditions.Check(args);
    }
    

    public RunConditionsList GetConditionsForLevel(int targetLevel)
    {
        var specificCondition = GetSpecificConditionForLevel(targetLevel);
        if (specificCondition != null)
        {
            return specificCondition.conditions;
        }
        return m_CanLevelUpConditions;
    }
    
    
    public SpecificConditionData GetSpecificConditionForLevel(int level)
    {
        if (m_SpecificLevelUp == null) return null;
        
        foreach (var condition in m_SpecificLevelUp)
        {
            if (condition.AppliesToLevel(level))
            {
                return condition;
            }
        }
        
        return null;
    }
    
    
    public List<SpecificConditionData> GetSpecificLevelUp()
    {
        return m_SpecificLevelUp ?? new List<SpecificConditionData>();
    }
    
    
    public bool HasSpecificConditionForLevel(int level)
    {
        return GetSpecificConditionForLevel(level) != null;
    }
    
    
    public bool CanUse(Args args)
    {
        return m_CanUse != null && m_CanUse.Check(args);
    }
    
    public bool CanUnlock(Args args)
    {
        return m_CanUnlock == null || m_CanUnlock.Check(args);
    }
    

    public string GetUnlockConditionsText(Args args)
    {
        if (m_CanUnlock == null)
        {
            return "";
        }
        
        System.Text.StringBuilder result = new System.Text.StringBuilder();
        
        bool passes = m_CanUnlock.Check(args);
        
        if (!passes)
        {
            result.AppendLine("  ✗ Unlock conditions not met");
        }
        else
        {
            result.AppendLine("  ✓ All conditions met");
        }
        
        return result.ToString();
    }

    // Gets the normalized time (0..1) on the SkillTreeSequence timeline when ChannelState should start.
    private float GetChannelStartTargetNormalizedTime()
    {
        float fallbackCastEnd = 0.33f;
        float fallbackReleaseEnd = 0.66f;

        if (m_Sequencer != null && m_Sequencer.PhasesTrack != null)
        {
            var clips = m_Sequencer.PhasesTrack.Clips;
            if (clips != null && clips.Length > 0 && clips[0] is ClipSkillTreePhases phases)
            {
                switch (m_ChannelStartMode)
                {
                    case ChannelStartMode.AtCastEnd:
                        return phases.CastEnd;
                    case ChannelStartMode.AtReleaseEnd:
                        return phases.ReleaseEnd;
                    case ChannelStartMode.CustomNormalizedTime:
                        return Mathf.Clamp01(m_ChannelStartCustomNormalizedTime);
                    case ChannelStartMode.AfterSequencer:
                    default:
                        return 1f;
                }
            }
        }

        switch (m_ChannelStartMode)
        {
            case ChannelStartMode.AtCastEnd:
                return fallbackCastEnd;
            case ChannelStartMode.AtReleaseEnd:
                return fallbackReleaseEnd;
            case ChannelStartMode.CustomNormalizedTime:
                return Mathf.Clamp01(m_ChannelStartCustomNormalizedTime);
            case ChannelStartMode.AfterSequencer:
            default:
                return 1f;
        }
    }
    
    /// <summary>
    /// Execute skill use instructions (async version)
    /// </summary>
    public async Task ExecuteAsync(Args args)
    {
        try
        {
            Character character = args.Self.Get<Character>();
            SkillTreeComponent tree = args.Self.Get<SkillTreeComponent>();

            bool wantsChannel = m_IsActiveSkill && m_IsChannelSkill && character != null;

            Task sequencerTask = null;
            bool ranOnUseInSequencer = false;

            if (m_IsActiveSkill && m_UseSequencer && m_AnimationClip != null && character != null)
            {
                ConfigGesture configuration = new ConfigGesture(
                    0f, m_AnimationClip.length,
                    1f, m_UseRootMotion,
                    0.1f, 0.1f
                );

                _ = character.Gestures.CrossFade(
                    m_AnimationClip, m_AvatarMask, BlendMode.Blend,
                    configuration, false
                );

                if (m_OnUse != null)
                {
                    _ = m_OnUse.Run(args);
                    ranOnUseInSequencer = true;
                }

                if (m_Sequencer != null)
                {
                    sequencerTask = m_Sequencer.Run(args, m_AnimationClip, character);
                }
                else
                {
                    Debug.LogWarning($"[Skill] {name} has UseSequencer enabled but Sequencer is null!");
                }
            }

            if (wantsChannel)
            {
                bool sequencerAwaitedAlready = false;

                if (m_ChannelStartMode == ChannelStartMode.AfterSequencer)
                {
                    if (sequencerTask != null)
                    {
                        await sequencerTask;
                        sequencerAwaitedAlready = true;
                    }
                }
                else if (sequencerTask != null)
                {
                    float targetT = GetChannelStartTargetNormalizedTime();

                    while (!sequencerTask.IsCompleted)
                    {
                        if (m_Sequencer != null && m_Sequencer.T >= targetT)
                            break;

                        await Task.Yield();
                    }
                }

                if (!ranOnUseInSequencer && m_OnUse != null)
                {
                    _ = m_OnUse.Run(args);
                }

                bool channelInputActive = tree != null && tree.IsChannelInputActive();
                bool channelConditionsActive = m_ChannelConditions == null || m_ChannelConditions.Check(args);
                if (!channelInputActive || !channelConditionsActive)
                {
                    if (sequencerTask != null && !sequencerAwaitedAlready)
                        await sequencerTask;
                    return;
                }

                if (m_StopGestureOnChannelStart && m_UseSequencer && m_AnimationClip != null)
                {
                    character?.Gestures.Stop(m_StopGestureDelay, m_StopGestureTransition);
                }

                bool stateActive = false;
                if (m_ChannelState.IsValid(character))
                {
                    ConfigState config = new ConfigState(0.1f, 1f, 1f, 0.1f, 0.1f);
                    _ = character.States.SetState(m_ChannelState, m_ChannelStateLayer, BlendMode.Blend, config);
                    stateActive = true;
                }

                while (tree != null &&
                       tree.IsChannelInputActive() &&
                       (m_ChannelConditions == null || m_ChannelConditions.Check(args)))
                {
                    if (m_OnChannelTick != null)
                        await m_OnChannelTick.Run(args);

                    await Task.Yield();
                }

                if (stateActive)
                    character.States.Stop(m_ChannelStateLayer, 0.1f, 0.1f);

                if (sequencerTask != null && !sequencerAwaitedAlready)
                    await sequencerTask;

                return;
            }

            // No channel: if have a sequencer wait it to complete.
            if (sequencerTask != null)
            {
                await sequencerTask;
                return;
            }

            // No sequencer and not channel, just OnUse
            if (!(m_IsActiveSkill && m_UseSequencer && m_AnimationClip != null) && !(m_IsActiveSkill && m_IsChannelSkill))
            {
                if (m_OnUse != null)
                    await m_OnUse.Run(args);
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[Skill] Error executing skill {name}: {ex.Message}\n{ex.StackTrace}");
        }
    }
    
    /// <summary>
    /// Execute skill use instructions (synchronous wrapper)
    /// </summary>
    public void Execute(Args args)
    {
        _ = ExecuteAsync(args);
    }
    
    /// <summary>
    /// Execute skill unlock instructions
    /// </summary>
    public void ExecuteUnlock(Args args)
    {
        if (m_OnUnlock != null)
        {
            _ = m_OnUnlock.Run(args);
        }
    }
    
    // CONTEXT MENU: --------------------------------------------------------------------------
    
    #if UNITY_EDITOR
    [UnityEditor.MenuItem("CONTEXT/Skill/ID/Copy Unique ID")]
    private static void CopyUniqueID(UnityEditor.MenuCommand command)
    {
        Skill skill = command.context as Skill;
        if (skill != null)
        {
            GUIUtility.systemCopyBuffer = skill.UniqueID;
        }
    }
    
    [UnityEditor.MenuItem("CONTEXT/Skill/ID/Regenerate Unique ID")]
    private static void RegenerateUniqueID(UnityEditor.MenuCommand command)
    {
        Skill skill = command.context as Skill;
        if (skill != null)
        {
            if (UnityEditor.EditorUtility.DisplayDialog(
                "Regenerate Unique ID",
                "Are you sure you want to regenerate the Unique ID? This will break any saved data references to this skill.",
                "Regenerate", "Cancel"))
            {
                skill.m_UniqueID = System.Guid.NewGuid().ToString();
                UnityEditor.EditorUtility.SetDirty(skill);
            }
        }
    }
    #endif
    
}
} 