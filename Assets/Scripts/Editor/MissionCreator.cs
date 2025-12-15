using UnityEngine;
using UnityEditor;
using System.IO;

public class MissionCreator : EditorWindow
{
    [MenuItem("Division Game/Create All 30 Missions")]
    public static void CreateAllMissions()
    {
        string missionsPath = "Assets/Missions";
        
        if (!Directory.Exists(missionsPath))
        {
            Directory.CreateDirectory(missionsPath);
        }
        
        CreateMission1();
        CreateMission2();
        CreateMission3();
        CreateMission4();
        CreateMission5();
        CreateMission6();
        CreateMission7();
        CreateMission8();
        CreateMission9();
        CreateMission10();
        CreateMission11();
        CreateMission12();
        CreateMission13();
        CreateMission14();
        CreateMission15();
        CreateMission16();
        CreateMission17();
        CreateMission18();
        CreateMission19();
        CreateMission20();
        CreateMission21();
        CreateMission22();
        CreateMission23();
        CreateMission24();
        CreateMission25();
        CreateMission26();
        CreateMission27();
        CreateMission28();
        CreateMission29();
        CreateMission30();
        
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
        Debug.Log("Successfully created all 30 missions in Assets/Missions/");
        EditorUtility.DisplayDialog("Success", "Created all 30 missions!", "OK");
    }
    
    private static void CreateMission1()
    {
        var mission = ScriptableObject.CreateInstance<MissionData>();
        mission.missionName = "First Blood";
        mission.description = "Eliminate a small group of hostile rogues in the residential district. This is your introduction to combat operations.";
        mission.levelRequirement = 1;
        mission.isMainStory = true;
        mission.isBossMission = false;
        mission.objectives.Add(new MissionObjective { type = MissionObjective.ObjectiveType.KillEnemies, description = "Eliminate 5 hostile enemies", targetCount = 5 });
        mission.xpReward = 100;
        mission.currencyReward = 50;
        AssetDatabase.CreateAsset(mission, "Assets/Missions/Mission_01_FirstBlood.asset");
    }
    
    private static void CreateMission2()
    {
        var mission = ScriptableObject.CreateInstance<MissionData>();
        mission.missionName = "Safe Haven";
        mission.description = "Secure the safe house and establish it as a forward operating base. Clear all hostiles from the perimeter.";
        mission.levelRequirement = 1;
        mission.isMainStory = true;
        mission.isBossMission = false;
        mission.objectives.Add(new MissionObjective { type = MissionObjective.ObjectiveType.ReachLocation, description = "Reach the safe house location", targetCount = 1 });
        mission.objectives.Add(new MissionObjective { type = MissionObjective.ObjectiveType.KillEnemies, description = "Clear hostiles around safe house", targetCount = 8 });
        mission.xpReward = 150;
        mission.currencyReward = 75;
        AssetDatabase.CreateAsset(mission, "Assets/Missions/Mission_02_SafeHaven.asset");
    }
    
    private static void CreateMission3()
    {
        var mission = ScriptableObject.CreateInstance<MissionData>();
        mission.missionName = "Supply Run";
        mission.description = "Collect medical supplies from abandoned clinics. The settlement desperately needs these resources.";
        mission.levelRequirement = 1;
        mission.isMainStory = false;
        mission.isBossMission = false;
        mission.objectives.Add(new MissionObjective { type = MissionObjective.ObjectiveType.CollectItems, description = "Collect medical supply crates", targetCount = 3 });
        mission.xpReward = 120;
        mission.currencyReward = 60;
        AssetDatabase.CreateAsset(mission, "Assets/Missions/Mission_03_SupplyRun.asset");
    }
    
    private static void CreateMission4()
    {
        var mission = ScriptableObject.CreateInstance<MissionData>();
        mission.missionName = "Reconnaissance";
        mission.description = "Scout enemy positions in the commercial district. Gather intel on rogue activity without being detected.";
        mission.levelRequirement = 2;
        mission.isMainStory = true;
        mission.isBossMission = false;
        mission.objectives.Add(new MissionObjective { type = MissionObjective.ObjectiveType.ReachLocation, description = "Reach observation point Alpha", targetCount = 1 });
        mission.objectives.Add(new MissionObjective { type = MissionObjective.ObjectiveType.ReachLocation, description = "Reach observation point Bravo", targetCount = 1 });
        mission.objectives.Add(new MissionObjective { type = MissionObjective.ObjectiveType.ReachLocation, description = "Reach observation point Charlie", targetCount = 1 });
        mission.xpReward = 180;
        mission.currencyReward = 90;
        AssetDatabase.CreateAsset(mission, "Assets/Missions/Mission_04_Recon.asset");
    }
    
    private static void CreateMission5()
    {
        var mission = ScriptableObject.CreateInstance<MissionData>();
        mission.missionName = "Ambush";
        mission.description = "Set up an ambush and eliminate a rogue patrol before they can report your presence.";
        mission.levelRequirement = 2;
        mission.isMainStory = true;
        mission.isBossMission = false;
        mission.objectives.Add(new MissionObjective { type = MissionObjective.ObjectiveType.ReachLocation, description = "Reach ambush position", targetCount = 1 });
        mission.objectives.Add(new MissionObjective { type = MissionObjective.ObjectiveType.KillEnemies, description = "Eliminate rogue patrol", targetCount = 10 });
        mission.xpReward = 200;
        mission.currencyReward = 100;
        AssetDatabase.CreateAsset(mission, "Assets/Missions/Mission_05_Ambush.asset");
    }
    
    private static void CreateMission6()
    {
        var mission = ScriptableObject.CreateInstance<MissionData>();
        mission.missionName = "Rescue Operation";
        mission.description = "Civilians are trapped in the apartment complex. Rescue them before the rogues execute them.";
        mission.levelRequirement = 2;
        mission.isMainStory = false;
        mission.isBossMission = false;
        mission.objectives.Add(new MissionObjective { type = MissionObjective.ObjectiveType.ReachLocation, description = "Reach the apartment complex", targetCount = 1 });
        mission.objectives.Add(new MissionObjective { type = MissionObjective.ObjectiveType.KillEnemies, description = "Eliminate captors", targetCount = 6 });
        mission.objectives.Add(new MissionObjective { type = MissionObjective.ObjectiveType.CollectItems, description = "Rescue civilians", targetCount = 3 });
        mission.xpReward = 220;
        mission.currencyReward = 110;
        AssetDatabase.CreateAsset(mission, "Assets/Missions/Mission_06_RescueOperation.asset");
    }
    
    private static void CreateMission7()
    {
        var mission = ScriptableObject.CreateInstance<MissionData>();
        mission.missionName = "Clear the Streets";
        mission.description = "Main Street is controlled by rogues. Clear them out to establish a safer route through the district.";
        mission.levelRequirement = 3;
        mission.isMainStory = true;
        mission.isBossMission = false;
        mission.objectives.Add(new MissionObjective { type = MissionObjective.ObjectiveType.KillEnemies, description = "Eliminate hostile forces on Main Street", targetCount = 15 });
        mission.xpReward = 250;
        mission.currencyReward = 125;
        AssetDatabase.CreateAsset(mission, "Assets/Missions/Mission_07_ClearTheStreets.asset");
    }
    
    private static void CreateMission8()
    {
        var mission = ScriptableObject.CreateInstance<MissionData>();
        mission.missionName = "Weapons Cache";
        mission.description = "Locate and secure an abandoned military weapons cache before the rogues find it.";
        mission.levelRequirement = 3;
        mission.isMainStory = false;
        mission.isBossMission = false;
        mission.objectives.Add(new MissionObjective { type = MissionObjective.ObjectiveType.ReachLocation, description = "Locate the weapons cache", targetCount = 1 });
        mission.objectives.Add(new MissionObjective { type = MissionObjective.ObjectiveType.KillEnemies, description = "Defend against incoming rogues", targetCount = 12 });
        mission.objectives.Add(new MissionObjective { type = MissionObjective.ObjectiveType.CollectItems, description = "Secure weapon crates", targetCount = 4 });
        mission.xpReward = 280;
        mission.currencyReward = 140;
        AssetDatabase.CreateAsset(mission, "Assets/Missions/Mission_08_WeaponsCache.asset");
    }
    
    private static void CreateMission9()
    {
        var mission = ScriptableObject.CreateInstance<MissionData>();
        mission.missionName = "Territory Control";
        mission.description = "Establish control over the park area. Hold the position against multiple waves of rogues.";
        mission.levelRequirement = 3;
        mission.isMainStory = true;
        mission.isBossMission = false;
        mission.objectives.Add(new MissionObjective { type = MissionObjective.ObjectiveType.ReachLocation, description = "Reach the park control point", targetCount = 1 });
        mission.objectives.Add(new MissionObjective { type = MissionObjective.ObjectiveType.DefendArea, description = "Defend the control point", targetCount = 1 });
        mission.objectives.Add(new MissionObjective { type = MissionObjective.ObjectiveType.KillEnemies, description = "Eliminate attackers", targetCount = 20 });
        mission.xpReward = 300;
        mission.currencyReward = 150;
        AssetDatabase.CreateAsset(mission, "Assets/Missions/Mission_09_TerritoryControl.asset");
    }
    
    private static void CreateMission10()
    {
        var mission = ScriptableObject.CreateInstance<MissionData>();
        mission.missionName = "The Enforcer";
        mission.description = "A heavily armed rogue leader known as The Enforcer controls the residential district. Take him down to break their grip on the area.";
        mission.levelRequirement = 4;
        mission.isMainStory = true;
        mission.isBossMission = true;
        mission.objectives.Add(new MissionObjective { type = MissionObjective.ObjectiveType.ReachLocation, description = "Locate The Enforcer's stronghold", targetCount = 1 });
        mission.objectives.Add(new MissionObjective { type = MissionObjective.ObjectiveType.KillEnemies, description = "Eliminate The Enforcer's guards", targetCount = 15 });
        mission.objectives.Add(new MissionObjective { type = MissionObjective.ObjectiveType.BossKill, description = "Defeat The Enforcer", targetCount = 1 });
        mission.xpReward = 400;
        mission.currencyReward = 200;
        AssetDatabase.CreateAsset(mission, "Assets/Missions/Mission_10_TheEnforcer.asset");
    }
    
    private static void CreateMission11()
    {
        var mission = ScriptableObject.CreateInstance<MissionData>();
        mission.missionName = "Into the Commercial District";
        mission.description = "Push into the commercial district and establish a foothold. Expect heavy resistance.";
        mission.levelRequirement = 4;
        mission.isMainStory = true;
        mission.isBossMission = false;
        mission.objectives.Add(new MissionObjective { type = MissionObjective.ObjectiveType.KillEnemies, description = "Clear the district entrance", targetCount = 18 });
        mission.objectives.Add(new MissionObjective { type = MissionObjective.ObjectiveType.ReachLocation, description = "Establish forward base", targetCount = 1 });
        mission.xpReward = 350;
        mission.currencyReward = 175;
        AssetDatabase.CreateAsset(mission, "Assets/Missions/Mission_11_IntoTheCommercial.asset");
    }
    
    private static void CreateMission12()
    {
        var mission = ScriptableObject.CreateInstance<MissionData>();
        mission.missionName = "Mall Sweep";
        mission.description = "Clear the shopping mall of rogue forces. They're using it as a base of operations.";
        mission.levelRequirement = 5;
        mission.isMainStory = false;
        mission.isBossMission = false;
        mission.objectives.Add(new MissionObjective { type = MissionObjective.ObjectiveType.KillEnemies, description = "Clear all floors of the mall", targetCount = 25 });
        mission.objectives.Add(new MissionObjective { type = MissionObjective.ObjectiveType.CollectItems, description = "Recover stolen supplies", targetCount = 5 });
        mission.xpReward = 380;
        mission.currencyReward = 190;
        AssetDatabase.CreateAsset(mission, "Assets/Missions/Mission_12_MallSweep.asset");
    }
    
    private static void CreateMission13()
    {
        var mission = ScriptableObject.CreateInstance<MissionData>();
        mission.missionName = "Sniper Nest";
        mission.description = "Rogue snipers are picking off civilians from rooftops. Eliminate them and secure the high ground.";
        mission.levelRequirement = 5;
        mission.isMainStory = false;
        mission.isBossMission = false;
        mission.objectives.Add(new MissionObjective { type = MissionObjective.ObjectiveType.KillEnemies, description = "Eliminate rooftop snipers", targetCount = 8 });
        mission.objectives.Add(new MissionObjective { type = MissionObjective.ObjectiveType.ReachLocation, description = "Secure the tower", targetCount = 1 });
        mission.xpReward = 360;
        mission.currencyReward = 180;
        AssetDatabase.CreateAsset(mission, "Assets/Missions/Mission_13_SniperNest.asset");
    }
    
    private static void CreateMission14()
    {
        var mission = ScriptableObject.CreateInstance<MissionData>();
        mission.missionName = "Convoy Escort";
        mission.description = "Protect a supply convoy as it moves through hostile territory. Don't let them destroy the trucks.";
        mission.levelRequirement = 5;
        mission.isMainStory = true;
        mission.isBossMission = false;
        mission.objectives.Add(new MissionObjective { type = MissionObjective.ObjectiveType.DefendArea, description = "Escort the convoy safely", targetCount = 1 });
        mission.objectives.Add(new MissionObjective { type = MissionObjective.ObjectiveType.KillEnemies, description = "Eliminate ambushers", targetCount = 20 });
        mission.xpReward = 420;
        mission.currencyReward = 210;
        AssetDatabase.CreateAsset(mission, "Assets/Missions/Mission_14_ConvoyEscort.asset");
    }
    
    private static void CreateMission15()
    {
        var mission = ScriptableObject.CreateInstance<MissionData>();
        mission.missionName = "Power Play";
        mission.description = "Restore power to the commercial district by securing the power station. Rogues will try to stop you.";
        mission.levelRequirement = 6;
        mission.isMainStory = true;
        mission.isBossMission = false;
        mission.objectives.Add(new MissionObjective { type = MissionObjective.ObjectiveType.ReachLocation, description = "Reach the power station", targetCount = 1 });
        mission.objectives.Add(new MissionObjective { type = MissionObjective.ObjectiveType.KillEnemies, description = "Clear the power station", targetCount = 22 });
        mission.objectives.Add(new MissionObjective { type = MissionObjective.ObjectiveType.CollectItems, description = "Activate power systems", targetCount = 3 });
        mission.xpReward = 450;
        mission.currencyReward = 225;
        AssetDatabase.CreateAsset(mission, "Assets/Missions/Mission_15_PowerPlay.asset");
    }
    
    private static void CreateMission16()
    {
        var mission = ScriptableObject.CreateInstance<MissionData>();
        mission.missionName = "The Warlord";
        mission.description = "The commercial district is controlled by a ruthless warlord. Eliminate him to free the district.";
        mission.levelRequirement = 6;
        mission.isMainStory = true;
        mission.isBossMission = true;
        mission.objectives.Add(new MissionObjective { type = MissionObjective.ObjectiveType.KillEnemies, description = "Fight through the warlord's compound", targetCount = 30 });
        mission.objectives.Add(new MissionObjective { type = MissionObjective.ObjectiveType.BossKill, description = "Defeat The Warlord", targetCount = 1 });
        mission.xpReward = 550;
        mission.currencyReward = 275;
        AssetDatabase.CreateAsset(mission, "Assets/Missions/Mission_16_TheWarLord.asset");
    }
    
    private static void CreateMission17()
    {
        var mission = ScriptableObject.CreateInstance<MissionData>();
        mission.missionName = "Industrial Push";
        mission.description = "Advance into the industrial zone. This area is heavily fortified by rogue forces.";
        mission.levelRequirement = 7;
        mission.isMainStory = true;
        mission.isBossMission = false;
        mission.objectives.Add(new MissionObjective { type = MissionObjective.ObjectiveType.KillEnemies, description = "Clear the industrial entrance", targetCount = 28 });
        mission.objectives.Add(new MissionObjective { type = MissionObjective.ObjectiveType.ReachLocation, description = "Secure the warehouse district", targetCount = 1 });
        mission.xpReward = 500;
        mission.currencyReward = 250;
        AssetDatabase.CreateAsset(mission, "Assets/Missions/Mission_17_IndustrialPush.asset");
    }
    
    private static void CreateMission18()
    {
        var mission = ScriptableObject.CreateInstance<MissionData>();
        mission.missionName = "Chemical Threat";
        mission.description = "Rogues are storing dangerous chemicals in the industrial zone. Secure the containers before they weaponize them.";
        mission.levelRequirement = 7;
        mission.isMainStory = false;
        mission.isBossMission = false;
        mission.objectives.Add(new MissionObjective { type = MissionObjective.ObjectiveType.ReachLocation, description = "Reach the chemical storage facility", targetCount = 1 });
        mission.objectives.Add(new MissionObjective { type = MissionObjective.ObjectiveType.KillEnemies, description = "Neutralize facility guards", targetCount = 24 });
        mission.objectives.Add(new MissionObjective { type = MissionObjective.ObjectiveType.CollectItems, description = "Secure chemical containers", targetCount = 6 });
        mission.xpReward = 520;
        mission.currencyReward = 260;
        AssetDatabase.CreateAsset(mission, "Assets/Missions/Mission_18_ChemicalThreat.asset");
    }
    
    private static void CreateMission19()
    {
        var mission = ScriptableObject.CreateInstance<MissionData>();
        mission.missionName = "Factory Assault";
        mission.description = "Rogues have converted an old factory into a weapons manufacturing facility. Shut it down permanently.";
        mission.levelRequirement = 7;
        mission.isMainStory = true;
        mission.isBossMission = false;
        mission.objectives.Add(new MissionObjective { type = MissionObjective.ObjectiveType.KillEnemies, description = "Clear the factory exterior", targetCount = 20 });
        mission.objectives.Add(new MissionObjective { type = MissionObjective.ObjectiveType.KillEnemies, description = "Clear the factory interior", targetCount = 25 });
        mission.objectives.Add(new MissionObjective { type = MissionObjective.ObjectiveType.CollectItems, description = "Plant explosives on assembly lines", targetCount = 4 });
        mission.xpReward = 580;
        mission.currencyReward = 290;
        AssetDatabase.CreateAsset(mission, "Assets/Missions/Mission_19_FactoryAssault.asset");
    }
    
    private static void CreateMission20()
    {
        var mission = ScriptableObject.CreateInstance<MissionData>();
        mission.missionName = "Last Stand";
        mission.description = "Defend the industrial base against a massive rogue counterattack. Hold the line at all costs.";
        mission.levelRequirement = 8;
        mission.isMainStory = true;
        mission.isBossMission = false;
        mission.objectives.Add(new MissionObjective { type = MissionObjective.ObjectiveType.DefendArea, description = "Defend the base perimeter", targetCount = 1 });
        mission.objectives.Add(new MissionObjective { type = MissionObjective.ObjectiveType.SurviveTime, description = "Survive for 10 minutes", targetCount = 600 });
        mission.objectives.Add(new MissionObjective { type = MissionObjective.ObjectiveType.KillEnemies, description = "Repel enemy waves", targetCount = 50 });
        mission.xpReward = 650;
        mission.currencyReward = 325;
        AssetDatabase.CreateAsset(mission, "Assets/Missions/Mission_20_LastStand.asset");
    }
    
    private static void CreateMission21()
    {
        var mission = ScriptableObject.CreateInstance<MissionData>();
        mission.missionName = "The Commander";
        mission.description = "The industrial zone's rogue commander is coordinating all attacks. Take him down to cripple their operations.";
        mission.levelRequirement = 8;
        mission.isMainStory = true;
        mission.isBossMission = true;
        mission.objectives.Add(new MissionObjective { type = MissionObjective.ObjectiveType.ReachLocation, description = "Infiltrate the command center", targetCount = 1 });
        mission.objectives.Add(new MissionObjective { type = MissionObjective.ObjectiveType.KillEnemies, description = "Eliminate elite guards", targetCount = 35 });
        mission.objectives.Add(new MissionObjective { type = MissionObjective.ObjectiveType.BossKill, description = "Defeat The Commander", targetCount = 1 });
        mission.xpReward = 750;
        mission.currencyReward = 375;
        AssetDatabase.CreateAsset(mission, "Assets/Missions/Mission_21_TheCommander.asset");
    }
    
    private static void CreateMission22()
    {
        var mission = ScriptableObject.CreateInstance<MissionData>();
        mission.missionName = "Downtown Incursion";
        mission.description = "Begin the final push into downtown - the heart of rogue territory. This will be brutal.";
        mission.levelRequirement = 9;
        mission.isMainStory = true;
        mission.isBossMission = false;
        mission.objectives.Add(new MissionObjective { type = MissionObjective.ObjectiveType.KillEnemies, description = "Breach downtown defenses", targetCount = 40 });
        mission.objectives.Add(new MissionObjective { type = MissionObjective.ObjectiveType.ReachLocation, description = "Establish forward command post", targetCount = 1 });
        mission.xpReward = 700;
        mission.currencyReward = 350;
        AssetDatabase.CreateAsset(mission, "Assets/Missions/Mission_22_DowntownIncursion.asset");
    }
    
    private static void CreateMission23()
    {
        var mission = ScriptableObject.CreateInstance<MissionData>();
        mission.missionName = "Hospital Siege";
        mission.description = "Rogues have seized the main hospital and are holding medical staff hostage. Rescue them.";
        mission.levelRequirement = 9;
        mission.isMainStory = false;
        mission.isBossMission = false;
        mission.objectives.Add(new MissionObjective { type = MissionObjective.ObjectiveType.KillEnemies, description = "Clear hospital floors", targetCount = 32 });
        mission.objectives.Add(new MissionObjective { type = MissionObjective.ObjectiveType.CollectItems, description = "Rescue medical staff", targetCount = 8 });
        mission.xpReward = 720;
        mission.currencyReward = 360;
        AssetDatabase.CreateAsset(mission, "Assets/Missions/Mission_23_HospitalSiege.asset");
    }
    
    private static void CreateMission24()
    {
        var mission = ScriptableObject.CreateInstance<MissionData>();
        mission.missionName = "Bank Vault";
        mission.description = "Rogues are looting the central bank. Secure the vault and recover critical financial records.";
        mission.levelRequirement = 9;
        mission.isMainStory = false;
        mission.isBossMission = false;
        mission.objectives.Add(new MissionObjective { type = MissionObjective.ObjectiveType.ReachLocation, description = "Reach the central bank", targetCount = 1 });
        mission.objectives.Add(new MissionObjective { type = MissionObjective.ObjectiveType.KillEnemies, description = "Eliminate looters", targetCount = 28 });
        mission.objectives.Add(new MissionObjective { type = MissionObjective.ObjectiveType.CollectItems, description = "Secure the vault contents", targetCount = 1 });
        mission.xpReward = 680;
        mission.currencyReward = 340;
        AssetDatabase.CreateAsset(mission, "Assets/Missions/Mission_24_BankVault.asset");
    }
    
    private static void CreateMission25()
    {
        var mission = ScriptableObject.CreateInstance<MissionData>();
        mission.missionName = "Tower Defense";
        mission.description = "Secure the communications tower to coordinate the final assault. Defend it against continuous attacks.";
        mission.levelRequirement = 9;
        mission.isMainStory = true;
        mission.isBossMission = false;
        mission.objectives.Add(new MissionObjective { type = MissionObjective.ObjectiveType.KillEnemies, description = "Clear the tower", targetCount = 30 });
        mission.objectives.Add(new MissionObjective { type = MissionObjective.ObjectiveType.DefendArea, description = "Defend the tower", targetCount = 1 });
        mission.objectives.Add(new MissionObjective { type = MissionObjective.ObjectiveType.KillEnemies, description = "Repel attackers", targetCount = 45 });
        mission.xpReward = 800;
        mission.currencyReward = 400;
        AssetDatabase.CreateAsset(mission, "Assets/Missions/Mission_25_TowerDefense.asset");
    }
    
    private static void CreateMission26()
    {
        var mission = ScriptableObject.CreateInstance<MissionData>();
        mission.missionName = "Stadium Strike";
        mission.description = "The rogues are using the stadium as their main training facility. Destroy it to weaken their forces.";
        mission.levelRequirement = 10;
        mission.isMainStory = true;
        mission.isBossMission = false;
        mission.objectives.Add(new MissionObjective { type = MissionObjective.ObjectiveType.KillEnemies, description = "Fight through the stadium", targetCount = 50 });
        mission.objectives.Add(new MissionObjective { type = MissionObjective.ObjectiveType.CollectItems, description = "Plant demolition charges", targetCount = 5 });
        mission.objectives.Add(new MissionObjective { type = MissionObjective.ObjectiveType.ReachLocation, description = "Escape the stadium", targetCount = 1 });
        mission.xpReward = 850;
        mission.currencyReward = 425;
        AssetDatabase.CreateAsset(mission, "Assets/Missions/Mission_26_StadiumStrike.asset");
    }
    
    private static void CreateMission27()
    {
        var mission = ScriptableObject.CreateInstance<MissionData>();
        mission.missionName = "The General's Plan";
        mission.description = "Intel suggests the rogue general is planning a major counteroffensive. Infiltrate and steal the battle plans.";
        mission.levelRequirement = 10;
        mission.isMainStory = true;
        mission.isBossMission = false;
        mission.objectives.Add(new MissionObjective { type = MissionObjective.ObjectiveType.ReachLocation, description = "Infiltrate headquarters undetected", targetCount = 1 });
        mission.objectives.Add(new MissionObjective { type = MissionObjective.ObjectiveType.CollectItems, description = "Recover battle plans", targetCount = 1 });
        mission.objectives.Add(new MissionObjective { type = MissionObjective.ObjectiveType.KillEnemies, description = "Fight your way out", targetCount = 40 });
        mission.xpReward = 900;
        mission.currencyReward = 450;
        AssetDatabase.CreateAsset(mission, "Assets/Missions/Mission_27_TheGeneralsPlan.asset");
    }
    
    private static void CreateMission28()
    {
        var mission = ScriptableObject.CreateInstance<MissionData>();
        mission.missionName = "Point of No Return";
        mission.description = "Launch the final assault on the capitol building. This is it - the decisive battle for the city.";
        mission.levelRequirement = 10;
        mission.isMainStory = true;
        mission.isBossMission = false;
        mission.objectives.Add(new MissionObjective { type = MissionObjective.ObjectiveType.KillEnemies, description = "Breach capitol defenses", targetCount = 60 });
        mission.objectives.Add(new MissionObjective { type = MissionObjective.ObjectiveType.ReachLocation, description = "Secure the main entrance", targetCount = 1 });
        mission.objectives.Add(new MissionObjective { type = MissionObjective.ObjectiveType.KillEnemies, description = "Clear the lobby and first floor", targetCount = 35 });
        mission.xpReward = 950;
        mission.currencyReward = 475;
        AssetDatabase.CreateAsset(mission, "Assets/Missions/Mission_28_PointOfNoReturn.asset");
    }
    
    private static void CreateMission29()
    {
        var mission = ScriptableObject.CreateInstance<MissionData>();
        mission.missionName = "The General";
        mission.description = "Face the rogue general in his war room. He's heavily armored and commands elite forces. This is the ultimate test.";
        mission.levelRequirement = 10;
        mission.isMainStory = true;
        mission.isBossMission = true;
        mission.objectives.Add(new MissionObjective { type = MissionObjective.ObjectiveType.KillEnemies, description = "Fight through elite guards", targetCount = 25 });
        mission.objectives.Add(new MissionObjective { type = MissionObjective.ObjectiveType.BossKill, description = "Defeat The General (Phase 1)", targetCount = 1 });
        mission.objectives.Add(new MissionObjective { type = MissionObjective.ObjectiveType.KillEnemies, description = "Eliminate reinforcements", targetCount = 20 });
        mission.objectives.Add(new MissionObjective { type = MissionObjective.ObjectiveType.BossKill, description = "Defeat The General (Phase 2)", targetCount = 1 });
        mission.xpReward = 1200;
        mission.currencyReward = 600;
        AssetDatabase.CreateAsset(mission, "Assets/Missions/Mission_29_TheGeneral.asset");
    }
    
    private static void CreateMission30()
    {
        var mission = ScriptableObject.CreateInstance<MissionData>();
        mission.missionName = "Endgame";
        mission.description = "The final mission. Activate the emergency broadcast system and declare the city liberated. Hold the broadcast tower against the last rogue forces.";
        mission.levelRequirement = 10;
        mission.isMainStory = true;
        mission.isBossMission = false;
        mission.objectives.Add(new MissionObjective { type = MissionObjective.ObjectiveType.ReachLocation, description = "Reach the broadcast tower roof", targetCount = 1 });
        mission.objectives.Add(new MissionObjective { type = MissionObjective.ObjectiveType.CollectItems, description = "Activate the emergency broadcast", targetCount = 1 });
        mission.objectives.Add(new MissionObjective { type = MissionObjective.ObjectiveType.DefendArea, description = "Defend the tower during broadcast", targetCount = 1 });
        mission.objectives.Add(new MissionObjective { type = MissionObjective.ObjectiveType.SurviveTime, description = "Hold position for 5 minutes", targetCount = 300 });
        mission.objectives.Add(new MissionObjective { type = MissionObjective.ObjectiveType.KillEnemies, description = "Repel final assault", targetCount = 75 });
        mission.xpReward = 1500;
        mission.currencyReward = 750;
        AssetDatabase.CreateAsset(mission, "Assets/Missions/Mission_30_Endgame.asset");
    }
}
