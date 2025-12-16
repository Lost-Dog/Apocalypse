using UnityEngine;
using UnityEditor;

#if UNITY_EDITOR
/// <summary>
/// Custom editor for ChallengeData to better display modifiers and rewards
/// </summary>
[CustomEditor(typeof(ChallengeData))]
public class ChallengeDataModifierEditor : Editor
{
    private bool showModifierPresets = false;
    private bool showRewardPreview = true;
    
    public override void OnInspectorGUI()
    {
        ChallengeData data = (ChallengeData)target;
        
        // Draw default inspector
        DrawDefaultInspector();
        
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Modifier & Reward Tools", EditorStyles.boldLabel);
        
        // Modifier Presets
        showModifierPresets = EditorGUILayout.Foldout(showModifierPresets, "Modifier Presets");
        if (showModifierPresets)
        {
            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Button("Speed Run"))
            {
                ApplySpeedRunPreset(data);
            }
            
            if (GUILayout.Button("Iron Man"))
            {
                ApplyIronManPreset(data);
            }
            
            if (GUILayout.Button("Elite Gauntlet"))
            {
                ApplyEliteGauntletPreset(data);
            }
            
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Button("Weekend Event"))
            {
                ApplyWeekendEventPreset(data);
            }
            
            if (GUILayout.Button("Clear Modifiers"))
            {
                Undo.RecordObject(data, "Clear Modifiers");
                data.modifiers.Clear();
                EditorUtility.SetDirty(data);
            }
            
            EditorGUILayout.EndHorizontal();
        }
        
        EditorGUILayout.Space(10);
        
        // Reward Preview
        showRewardPreview = EditorGUILayout.Foldout(showRewardPreview, "Reward Preview");
        if (showRewardPreview)
        {
            EditorGUILayout.HelpBox("Preview rewards for player levels 1, 5, and 10", MessageType.Info);
            
            DrawRewardPreview(data, 1);
            DrawRewardPreview(data, 5);
            DrawRewardPreview(data, 10);
        }
        
        EditorGUILayout.Space(10);
        
        // Active Modifiers Description
        if (data.modifiers.Count > 0)
        {
            EditorGUILayout.LabelField("Active Modifiers Description:", EditorStyles.boldLabel);
            string description = data.GetModifiersDescription();
            if (!string.IsNullOrEmpty(description))
            {
                EditorGUILayout.HelpBox(description, MessageType.None);
            }
        }
    }
    
    private void DrawRewardPreview(ChallengeData data, int playerLevel)
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField($"Player Level {playerLevel}", EditorStyles.boldLabel);
        
        var actualDifficulty = data.CalculateScaledDifficulty(playerLevel);
        
        // Base rewards
        int baseXP = data.GetScaledXPReward(playerLevel, actualDifficulty);
        int baseCurrency = data.GetScaledCurrencyReward(playerLevel, actualDifficulty);
        
        // With bonuses (perfect + speed)
        int maxXP = data.GetTotalXPReward(playerLevel, actualDifficulty, true, true, false);
        int maxCurrency = data.GetTotalCurrencyReward(playerLevel, actualDifficulty);
        
        var lootRarity = data.GetTotalLootRarity(actualDifficulty);
        int lootCount = data.GetTotalLootCount();
        
        EditorGUILayout.LabelField($"Difficulty: {actualDifficulty}");
        EditorGUILayout.LabelField($"XP: {baseXP} (max: {maxXP} with bonuses)");
        EditorGUILayout.LabelField($"Currency: {baseCurrency} (max: {maxCurrency})");
        EditorGUILayout.LabelField($"Loot: {lootCount}× {lootRarity}");
        
        EditorGUILayout.EndVertical();
        EditorGUILayout.Space(5);
    }
    
    private void ApplySpeedRunPreset(ChallengeData data)
    {
        Undo.RecordObject(data, "Apply Speed Run Preset");
        
        data.modifiers.Clear();
        data.modifiers.Add(new ChallengeData.ChallengeModifier
        {
            type = ChallengeData.ChallengeModifier.ModifierType.TimeTrial,
            value = 0.5f,
            isActive = true
        });
        data.modifiers.Add(new ChallengeData.ChallengeModifier
        {
            type = ChallengeData.ChallengeModifier.ModifierType.DoubleXP,
            value = 2.0f,
            isActive = true
        });
        
        data.speedCompletionBonus = true;
        data.speedThresholdPercentage = 0.8f;
        data.speedCompletionXPMultiplier = 1.5f;
        
        EditorUtility.SetDirty(data);
    }
    
    private void ApplyIronManPreset(ChallengeData data)
    {
        Undo.RecordObject(data, "Apply Iron Man Preset");
        
        data.modifiers.Clear();
        data.modifiers.Add(new ChallengeData.ChallengeModifier
        {
            type = ChallengeData.ChallengeModifier.ModifierType.IronMan,
            value = 1.0f,
            isActive = true
        });
        data.modifiers.Add(new ChallengeData.ChallengeModifier
        {
            type = ChallengeData.ChallengeModifier.ModifierType.NoHealthRegen,
            value = 1.0f,
            isActive = true
        });
        data.modifiers.Add(new ChallengeData.ChallengeModifier
        {
            type = ChallengeData.ChallengeModifier.ModifierType.IncreasedEnemyDamage,
            value = 1.3f,
            isActive = true
        });
        
        data.perfectCompletionBonus = true;
        data.perfectCompletionXPMultiplier = 2.0f;
        
        EditorUtility.SetDirty(data);
    }
    
    private void ApplyEliteGauntletPreset(ChallengeData data)
    {
        Undo.RecordObject(data, "Apply Elite Gauntlet Preset");
        
        data.modifiers.Clear();
        data.modifiers.Add(new ChallengeData.ChallengeModifier
        {
            type = ChallengeData.ChallengeModifier.ModifierType.EliteEnemiesOnly,
            value = 1.0f,
            isActive = true
        });
        data.modifiers.Add(new ChallengeData.ChallengeModifier
        {
            type = ChallengeData.ChallengeModifier.ModifierType.IncreasedEnemyHealth,
            value = 1.5f,
            isActive = true
        });
        data.modifiers.Add(new ChallengeData.ChallengeModifier
        {
            type = ChallengeData.ChallengeModifier.ModifierType.GuaranteedRareLoot,
            value = 1.0f,
            isActive = true
        });
        
        if ((int)data.guaranteedLootRarity < (int)LootManager.Rarity.Rare)
        {
            data.guaranteedLootRarity = LootManager.Rarity.Rare;
        }
        
        EditorUtility.SetDirty(data);
    }
    
    private void ApplyWeekendEventPreset(ChallengeData data)
    {
        Undo.RecordObject(data, "Apply Weekend Event Preset");
        
        data.modifiers.Clear();
        data.modifiers.Add(new ChallengeData.ChallengeModifier
        {
            type = ChallengeData.ChallengeModifier.ModifierType.DoubleXP,
            value = 2.0f,
            isActive = true
        });
        data.modifiers.Add(new ChallengeData.ChallengeModifier
        {
            type = ChallengeData.ChallengeModifier.ModifierType.DoubleCurrency,
            value = 2.0f,
            isActive = true
        });
        data.modifiers.Add(new ChallengeData.ChallengeModifier
        {
            type = ChallengeData.ChallengeModifier.ModifierType.BonusLootDrop,
            value = 0.5f,
            isActive = true
        });
        
        EditorUtility.SetDirty(data);
    }
}
#endif
