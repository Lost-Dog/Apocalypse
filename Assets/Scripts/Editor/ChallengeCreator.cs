using UnityEngine;
using UnityEditor;
using System.IO;

public class ChallengeCreator : EditorWindow
{
    [MenuItem("Division Game/Create All Challenges")]
    public static void CreateAllChallenges()
    {
        CreateFolderStructure();
        CreateWorldEventChallenges();
        CreateDailyChallenges();
        CreateWeeklyChallenges();
        
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
        Debug.Log("✓ All challenges created successfully!");
        EditorUtility.DisplayDialog("Success", "All challenge assets have been created in:\n\n/Assets/Resources/Challenges/", "OK");
    }

    private static void CreateFolderStructure()
    {
        if (!AssetDatabase.IsValidFolder("Assets/Resources"))
            AssetDatabase.CreateFolder("Assets", "Resources");
        
        if (!AssetDatabase.IsValidFolder("Assets/Resources/Challenges"))
            AssetDatabase.CreateFolder("Assets/Resources", "Challenges");
        
        if (!AssetDatabase.IsValidFolder("Assets/Resources/Challenges/WorldEvents"))
            AssetDatabase.CreateFolder("Assets/Resources/Challenges", "WorldEvents");
        
        if (!AssetDatabase.IsValidFolder("Assets/Resources/Challenges/Daily"))
            AssetDatabase.CreateFolder("Assets/Resources/Challenges", "Daily");
        
        if (!AssetDatabase.IsValidFolder("Assets/Resources/Challenges/Weekly"))
            AssetDatabase.CreateFolder("Assets/Resources/Challenges", "Weekly");
    }

    private static void CreateWorldEventChallenges()
    {
        CreateChallenge("Assets/Resources/Challenges/WorldEvents/WE_SupplyDrop_Easy.asset",
            "Small Supply Cache",
            "A small enemy squad is guarding a supply cache. Eliminate them and claim the loot.",
            ChallengeData.ChallengeType.SupplyDrop,
            ChallengeData.ChallengeFrequency.WorldEvent,
            ChallengeData.ChallengeDifficulty.Easy,
            300f, 40f, 1, 1, 5, false, "", 150, 75, LootManager.Rarity.Common, 1);

        CreateChallenge("Assets/Resources/Challenges/WorldEvents/WE_SupplyDrop_Medium.asset",
            "Armed Convoy",
            "An enemy convoy is moving supplies through the area. Ambush and secure the cargo.",
            ChallengeData.ChallengeType.SupplyDrop,
            ChallengeData.ChallengeFrequency.WorldEvent,
            ChallengeData.ChallengeDifficulty.Medium,
            600f, 50f, 3, 2, 10, false, "", 300, 150, LootManager.Rarity.Uncommon, 2);

        CreateChallenge("Assets/Resources/Challenges/WorldEvents/WE_SupplyDrop_Hard.asset",
            "Heavily Guarded Shipment",
            "A high-value shipment is being transported by elite forces. High risk, high reward.",
            ChallengeData.ChallengeType.SupplyDrop,
            ChallengeData.ChallengeFrequency.WorldEvent,
            ChallengeData.ChallengeDifficulty.Hard,
            900f, 60f, 7, 5, 15, true, "Supply Commander", 600, 300, LootManager.Rarity.Rare, 3);

        CreateChallenge("Assets/Resources/Challenges/WorldEvents/WE_CivilianRescue_Easy.asset",
            "Citizens in Distress",
            "A few civilians are being threatened by rogues. Get there fast!",
            ChallengeData.ChallengeType.CivilianRescue,
            ChallengeData.ChallengeFrequency.WorldEvent,
            ChallengeData.ChallengeDifficulty.Easy,
            240f, 35f, 1, 1, 4, false, "", 120, 60, LootManager.Rarity.Common, 1, 3);

        CreateChallenge("Assets/Resources/Challenges/WorldEvents/WE_CivilianRescue_Medium.asset",
            "Civilians Under Siege",
            "Multiple civilians are trapped and under heavy fire. Save them all!",
            ChallengeData.ChallengeType.CivilianRescue,
            ChallengeData.ChallengeFrequency.WorldEvent,
            ChallengeData.ChallengeDifficulty.Medium,
            420f, 45f, 4, 2, 8, false, "", 250, 125, LootManager.Rarity.Uncommon, 1, 6);

        CreateChallenge("Assets/Resources/Challenges/WorldEvents/WE_ControlPoint_Medium.asset",
            "Reclaim the Checkpoint",
            "Enemies have taken over a strategic checkpoint. Clear all hostiles and secure the area.",
            ChallengeData.ChallengeType.ControlPoint,
            ChallengeData.ChallengeFrequency.WorldEvent,
            ChallengeData.ChallengeDifficulty.Medium,
            600f, 50f, 3, 2, 12, false, "", 350, 175, LootManager.Rarity.Uncommon, 2);

        CreateChallenge("Assets/Resources/Challenges/WorldEvents/WE_ControlPoint_Hard.asset",
            "Fortified Position Assault",
            "A heavily fortified control point must be taken. Expect fierce resistance.",
            ChallengeData.ChallengeType.ControlPoint,
            ChallengeData.ChallengeFrequency.WorldEvent,
            ChallengeData.ChallengeDifficulty.Hard,
            900f, 65f, 6, 4, 20, true, "Point Commander", 650, 325, LootManager.Rarity.Rare, 2);

        CreateChallenge("Assets/Resources/Challenges/WorldEvents/WE_BossEncounter_Medium.asset",
            "Rogue Lieutenant",
            "A rogue officer has been spotted with a small squad. Neutralize the threat.",
            ChallengeData.ChallengeType.BossEncounter,
            ChallengeData.ChallengeFrequency.WorldEvent,
            ChallengeData.ChallengeDifficulty.Medium,
            600f, 55f, 4, 3, 8, true, "Rogue Lieutenant", 400, 200, LootManager.Rarity.Rare, 1);

        CreateChallenge("Assets/Resources/Challenges/WorldEvents/WE_BossEncounter_Hard.asset",
            "Elite Commander",
            "An Elite Commander leads a heavily armed squad. Extreme danger.",
            ChallengeData.ChallengeType.BossEncounter,
            ChallengeData.ChallengeFrequency.WorldEvent,
            ChallengeData.ChallengeDifficulty.Hard,
            900f, 60f, 7, 5, 12, true, "Elite Commander", 750, 375, LootManager.Rarity.Epic, 1);

        CreateChallenge("Assets/Resources/Challenges/WorldEvents/WE_BossEncounter_Extreme.asset",
            "Named Enemy - Warlord",
            "A legendary warlord has appeared. Only the strongest agents survive.",
            ChallengeData.ChallengeType.BossEncounter,
            ChallengeData.ChallengeFrequency.WorldEvent,
            ChallengeData.ChallengeDifficulty.Extreme,
            1200f, 70f, 10, 8, 15, true, "The Warlord", 1500, 750, LootManager.Rarity.Legendary, 1);

        CreateChallenge("Assets/Resources/Challenges/WorldEvents/WE_ExtractionDefense_Medium.asset",
            "Secure the LZ",
            "Hold the extraction zone until the helicopter arrives. Defend against enemy waves.",
            ChallengeData.ChallengeType.ExtractionDefense,
            ChallengeData.ChallengeFrequency.WorldEvent,
            ChallengeData.ChallengeDifficulty.Medium,
            480f, 45f, 5, 3, 25, false, "", 450, 225, LootManager.Rarity.Rare, 2);

        CreateChallenge("Assets/Resources/Challenges/WorldEvents/WE_ExtractionDefense_Hard.asset",
            "Hot Extraction",
            "Heavy enemy presence detected. Survive the onslaught and extract safely.",
            ChallengeData.ChallengeType.ExtractionDefense,
            ChallengeData.ChallengeFrequency.WorldEvent,
            ChallengeData.ChallengeDifficulty.Hard,
            600f, 50f, 8, 6, 40, true, "Assault Leader", 800, 400, LootManager.Rarity.Epic, 2);

        CreateChallenge("Assets/Resources/Challenges/WorldEvents/WE_HostageRescue_Hard.asset",
            "High Value Target Rescue",
            "VIP hostages must be rescued alive. Stealth and precision required.",
            ChallengeData.ChallengeType.HostageRescue,
            ChallengeData.ChallengeFrequency.WorldEvent,
            ChallengeData.ChallengeDifficulty.Hard,
            600f, 50f, 6, 4, 12, false, "", 700, 350, LootManager.Rarity.Epic, 1, 4);

        CreateChallenge("Assets/Resources/Challenges/WorldEvents/WE_RivalAgent_Extreme.asset",
            "Rogue Division Agent",
            "A Division agent has gone rogue. This is a 1v1 duel to the death.",
            ChallengeData.ChallengeType.RivalAgent,
            ChallengeData.ChallengeFrequency.WorldEvent,
            ChallengeData.ChallengeDifficulty.Extreme,
            600f, 40f, 10, 8, 1, true, "Rogue Agent", 1000, 500, LootManager.Rarity.Legendary, 1);
    }

    private static void CreateDailyChallenges()
    {
        CreateChallenge("Assets/Resources/Challenges/Daily/Daily_Hunter.asset",
            "Hunter",
            "Eliminate 50 enemy combatants throughout the city.",
            ChallengeData.ChallengeType.SupplyDrop,
            ChallengeData.ChallengeFrequency.Daily,
            ChallengeData.ChallengeDifficulty.Easy,
            86400f, 0f, 1, 1, 50, false, "", 400, 200, LootManager.Rarity.Uncommon, 1);

        CreateChallenge("Assets/Resources/Challenges/Daily/Daily_Marksman.asset",
            "Marksman",
            "Score 25 headshot kills. Precision matters.",
            ChallengeData.ChallengeType.SupplyDrop,
            ChallengeData.ChallengeFrequency.Daily,
            ChallengeData.ChallengeDifficulty.Medium,
            86400f, 0f, 1, 1, 25, false, "", 500, 250, LootManager.Rarity.Rare, 1);

        CreateChallenge("Assets/Resources/Challenges/Daily/Daily_GuardianAngel.asset",
            "Guardian Angel",
            "Rescue 10 civilians from danger.",
            ChallengeData.ChallengeType.CivilianRescue,
            ChallengeData.ChallengeFrequency.Daily,
            ChallengeData.ChallengeDifficulty.Easy,
            86400f, 0f, 1, 1, 0, false, "", 350, 175, LootManager.Rarity.Uncommon, 1, 10);

        CreateChallenge("Assets/Resources/Challenges/Daily/Daily_ZoneControl.asset",
            "Zone Control",
            "Capture 3 enemy control points.",
            ChallengeData.ChallengeType.ControlPoint,
            ChallengeData.ChallengeFrequency.Daily,
            ChallengeData.ChallengeDifficulty.Medium,
            86400f, 0f, 1, 1, 30, false, "", 600, 300, LootManager.Rarity.Rare, 1);

        CreateChallenge("Assets/Resources/Challenges/Daily/Daily_FlawlessOperator.asset",
            "Flawless Operator",
            "Complete 3 missions without dying.",
            ChallengeData.ChallengeType.SupplyDrop,
            ChallengeData.ChallengeFrequency.Daily,
            ChallengeData.ChallengeDifficulty.Hard,
            86400f, 0f, 1, 1, 3, false, "", 750, 375, LootManager.Rarity.Epic, 1);

        CreateChallenge("Assets/Resources/Challenges/Daily/Daily_BossHunter.asset",
            "Boss Hunter",
            "Defeat 2 elite or named enemies.",
            ChallengeData.ChallengeType.BossEncounter,
            ChallengeData.ChallengeFrequency.Daily,
            ChallengeData.ChallengeDifficulty.Hard,
            86400f, 0f, 1, 1, 2, true, "", 650, 325, LootManager.Rarity.Epic, 1);

        CreateChallenge("Assets/Resources/Challenges/Daily/Daily_Defender.asset",
            "Defender",
            "Successfully defend 2 extraction zones.",
            ChallengeData.ChallengeType.ExtractionDefense,
            ChallengeData.ChallengeFrequency.Daily,
            ChallengeData.ChallengeDifficulty.Medium,
            86400f, 0f, 1, 1, 2, false, "", 550, 275, LootManager.Rarity.Rare, 1);

        CreateChallenge("Assets/Resources/Challenges/Daily/Daily_ResourceCollector.asset",
            "Resource Collector",
            "Secure 5 supply drops.",
            ChallengeData.ChallengeType.SupplyDrop,
            ChallengeData.ChallengeFrequency.Daily,
            ChallengeData.ChallengeDifficulty.Easy,
            86400f, 0f, 1, 1, 5, false, "", 300, 150, LootManager.Rarity.Uncommon, 2);
    }

    private static void CreateWeeklyChallenges()
    {
        CreateChallenge("Assets/Resources/Challenges/Weekly/Weekly_EliteHunter.asset",
            "Elite Hunter",
            "Eliminate 200 enemies throughout the week.",
            ChallengeData.ChallengeType.SupplyDrop,
            ChallengeData.ChallengeFrequency.Weekly,
            ChallengeData.ChallengeDifficulty.Medium,
            604800f, 0f, 1, 1, 200, false, "", 2000, 1000, LootManager.Rarity.Epic, 2);

        CreateChallenge("Assets/Resources/Challenges/Weekly/Weekly_Humanitarian.asset",
            "Humanitarian",
            "Rescue 50 civilians this week. Every life matters.",
            ChallengeData.ChallengeType.CivilianRescue,
            ChallengeData.ChallengeFrequency.Weekly,
            ChallengeData.ChallengeDifficulty.Medium,
            604800f, 0f, 1, 1, 0, false, "", 1800, 900, LootManager.Rarity.Rare, 3, 50);

        CreateChallenge("Assets/Resources/Challenges/Weekly/Weekly_BossSlayer.asset",
            "Boss Slayer",
            "Defeat 5 named bosses or elite enemies.",
            ChallengeData.ChallengeType.BossEncounter,
            ChallengeData.ChallengeFrequency.Weekly,
            ChallengeData.ChallengeDifficulty.Hard,
            604800f, 0f, 1, 1, 5, true, "", 2500, 1250, LootManager.Rarity.Legendary, 1);

        CreateChallenge("Assets/Resources/Challenges/Weekly/Weekly_MasterAgent.asset",
            "Master Division Agent",
            "Complete 20 world events - the ultimate test.",
            ChallengeData.ChallengeType.ControlPoint,
            ChallengeData.ChallengeFrequency.Weekly,
            ChallengeData.ChallengeDifficulty.Extreme,
            604800f, 0f, 1, 1, 20, false, "", 3000, 1500, LootManager.Rarity.Legendary, 2);

        CreateChallenge("Assets/Resources/Challenges/Weekly/Weekly_TerritoryController.asset",
            "Territory Controller",
            "Capture 10 control points.",
            ChallengeData.ChallengeType.ControlPoint,
            ChallengeData.ChallengeFrequency.Weekly,
            ChallengeData.ChallengeDifficulty.Medium,
            604800f, 0f, 1, 1, 100, false, "", 2200, 1100, LootManager.Rarity.Epic, 2);

        CreateChallenge("Assets/Resources/Challenges/Weekly/Weekly_Sharpshooter.asset",
            "Sharpshooter",
            "Score 150 headshot kills this week.",
            ChallengeData.ChallengeType.SupplyDrop,
            ChallengeData.ChallengeFrequency.Weekly,
            ChallengeData.ChallengeDifficulty.Hard,
            604800f, 0f, 1, 1, 150, false, "", 2400, 1200, LootManager.Rarity.Epic, 2);
    }

    private static void CreateChallenge(string path, string challengeName, string description,
        ChallengeData.ChallengeType type, ChallengeData.ChallengeFrequency frequency,
        ChallengeData.ChallengeDifficulty difficulty, float timeLimit, float detectionRadius,
        int recommendedLevel, int requiredLevel, int enemyCount, bool spawnBoss, string bossName,
        int xpReward, int currencyReward, LootManager.Rarity lootRarity, int lootCount, int civilianCount = 0)
    {
        ChallengeData challenge = ScriptableObject.CreateInstance<ChallengeData>();
        
        challenge.challengeName = challengeName;
        challenge.description = description;
        challenge.challengeType = type;
        challenge.frequency = frequency;
        challenge.difficulty = difficulty;
        
        challenge.timeLimit = timeLimit;
        challenge.detectionRadius = detectionRadius;
        challenge.recommendedLevel = recommendedLevel;
        challenge.requiredPlayerLevel = requiredLevel;
        
        if (enemyCount > 0)
        {
            challenge.spawnItems.Add(new ChallengeData.SpawnableItem
            {
                itemName = spawnBoss ? "Boss Enemy" : "Enemies",
                category = spawnBoss ? ChallengeData.SpawnableCategory.Boss : ChallengeData.SpawnableCategory.Enemy,
                minCount = enemyCount,
                maxCount = enemyCount,
                spawnLocation = ChallengeData.SpawnLocationType.AroundPerimeter,
                spawnRadius = 15f,
                requireNavMesh = true,
                priority = spawnBoss ? 10 : 5
            });
        }
        
        if (civilianCount > 0)
        {
            challenge.spawnItems.Add(new ChallengeData.SpawnableItem
            {
                itemName = "Civilians",
                category = ChallengeData.SpawnableCategory.Civilian,
                minCount = civilianCount,
                maxCount = civilianCount,
                spawnLocation = ChallengeData.SpawnLocationType.RandomInRadius,
                spawnRadius = 10f,
                requireNavMesh = true,
                priority = 8
            });
        }
        
        challenge.xpReward = xpReward;
        challenge.currencyReward = currencyReward;
        challenge.guaranteedLootRarity = lootRarity;
        challenge.guaranteedLootCount = lootCount;
        
        challenge.difficultyColor = challenge.GetDifficultyColor();
        
        if (type == ChallengeData.ChallengeType.HostageRescue)
        {
            challenge.requireStealth = true;
        }
        
        if (challengeName.Contains("Flawless"))
        {
            challenge.requireNoDeaths = true;
        }
        
        AssetDatabase.CreateAsset(challenge, path);
        Debug.Log($"Created: {challengeName}");
    }
}
