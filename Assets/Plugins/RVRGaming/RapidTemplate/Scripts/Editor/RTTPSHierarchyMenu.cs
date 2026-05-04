using UnityEditor;
using UnityEngine;

public class RTTPSHierarchyMenu : MonoBehaviour
{
    private static readonly string playerPrefabPath = "Assets/Plugins/RVRGaming/RapidTemplate/Prefabs/TPS/Player.prefab";
    private static readonly string AssaultPrefabPath = "Assets/Plugins/RVRGaming/RapidTemplate/Prefabs/TPS/Assault.prefab";
    private static readonly string RusherPrefabPath = "Assets/Plugins/RVRGaming/RapidTemplate/Prefabs/TPS/Rusher.prefab";
    private static readonly string TankPrefabPath = "Assets/Plugins/RVRGaming/RapidTemplate/Prefabs/TPS/Tank.prefab";
    private static readonly string factionsPrefabPath = "Assets/Plugins/RVRGaming/RapidTemplate/Prefabs/TPS/Factions.prefab";
    private static readonly string ControllerPrefabPath = "Assets/Plugins/RVRGaming/RapidTemplate/Prefabs/TPS/Controller.prefab";
    private static readonly string shotPrefabPath = "Assets/Plugins/RVRGaming/RapidTemplate/Prefabs/TPS/Camera Shot TPS.prefab";
    private static readonly string shot2PrefabPath = "Assets/Plugins/RVRGaming/RapidTemplate/Prefabs/TPS/Main Camera.prefab";
    private static readonly string invCharPrefabPath = "Assets/Plugins/RVRGaming/RapidTemplate/Prefabs/TPS/InventoryCharacter.prefab";
    private static readonly string checkPointPath = "Assets/Plugins/RVRGaming/RapidTemplate/Prefabs/Save/Checkpoint.prefab";
    private static readonly string savePointPath = "Assets/Plugins/RVRGaming/RapidTemplate/Prefabs/Save/Save Point.prefab";
    private static readonly string bedPath = "Assets/Plugins/RVRGaming/RapidTemplate/Prefabs/Basic/Bed.prefab";
    private static readonly string carryPath = "Assets/Plugins/RVRGaming/RapidTemplate/Prefabs/Basic/Carry.prefab";
    private static readonly string chairPath = "Assets/Plugins/RVRGaming/RapidTemplate/Prefabs/Basic/Chair.prefab";
    private static readonly string chestPath = "Assets/Plugins/RVRGaming/RapidTemplate/Prefabs/Basic/Chest.prefab";
    private static readonly string digPath = "Assets/Plugins/RVRGaming/RapidTemplate/Prefabs/Basic/Dig.prefab";
    private static readonly string doorPath = "Assets/Plugins/RVRGaming/RapidTemplate/Prefabs/Basic/Door.prefab";
    private static readonly string fishPath = "Assets/Plugins/RVRGaming/RapidTemplate/Prefabs/Basic/Fish.prefab";
    private static readonly string fountainPath = "Assets/Plugins/RVRGaming/RapidTemplate/Prefabs/Basic/Fountain.prefab";
    private static readonly string gatherPath = "Assets/Plugins/RVRGaming/RapidTemplate/Prefabs/Basic/Gather.prefab";
    private static readonly string pickupPath = "Assets/Plugins/RVRGaming/RapidTemplate/Prefabs/Basic/Pickup.prefab";
    private static readonly string emotePath = "Assets/Plugins/RVRGaming/RapidTemplate/Prefabs/Basic/Canvas-Emote.prefab";
    private static readonly string break01Path = "Assets/Plugins/RVRGaming/RapidTemplate/Prefabs/TPS/Bottle.prefab";
    private static readonly string break02Path = "Assets/Plugins/RVRGaming/RapidTemplate/Prefabs/TPS/Car.prefab";
    private static readonly string hudPath = "Assets/Plugins/RVRGaming/RapidTemplate/Prefabs/TPS/Player-HUD.prefab";
    private static readonly string menuPath = "Assets/Plugins/RVRGaming/RapidTemplate/Prefabs/TPS/Main Menu.prefab";
    private static readonly string indicatorPath = "Assets/Plugins/RVRGaming/RapidTemplate/Prefabs/TPS/Canvas-IndicatorAwareness.prefab";
    private static readonly string lightPath = "Assets/Plugins/RVRGaming/RapidTemplate/Prefabs/General/Directional Light.prefab";
    private static readonly string volumePath = "Assets/Plugins/RVRGaming/RapidTemplate/Prefabs/General/Global Volume.prefab";
    private static readonly string fadePath = "Assets/Plugins/RVRGaming/RapidTemplate/Prefabs/TPS/Canvas-FadeIn.prefab";
    private static readonly string lowWall2xPath = "Assets/Plugins/RVRGaming/RapidTemplate/Prefabs/TPS/Low Wall 2x.prefab";
    private static readonly string lowWall4xPath = "Assets/Plugins/RVRGaming/RapidTemplate/Prefabs/TPS/Low Wall 4x.prefab";
    private static readonly string lowWall8xPath = "Assets/Plugins/RVRGaming/RapidTemplate/Prefabs/TPS/Low Wall 8x.prefab";
    private static readonly string highWall4xPath = "Assets/Plugins/RVRGaming/RapidTemplate/Prefabs/TPS/High Wall 4x.prefab";
    private static readonly string lowBlockPath = "Assets/Plugins/RVRGaming/RapidTemplate/Prefabs/TPS/Low Block.prefab";
    private static readonly string lowCornerPath = "Assets/Plugins/RVRGaming/RapidTemplate/Prefabs/TPS/Low Corner.prefab";
    private static readonly string ammoBoxPath = "Assets/Plugins/RVRGaming/RapidTemplate/Prefabs/TPS/Ammo Box.prefab";
    private static readonly string menuSettingsPath = "Assets/Plugins/RVRGaming/RapidTemplate/Prefabs/General/Camera-Preview.prefab";


    [MenuItem("GameObject/Rapid Template/TPS/Characters/Player", false, 10)]
    private static void AddPlayerPrefab()
    {
        AddPrefabToScene(playerPrefabPath, "Player");
    }

    [MenuItem("GameObject/Rapid Template/TPS/Characters/Assault", false, 11)]
    private static void AddAssaultPrefab()
    {
        AddPrefabToScene(AssaultPrefabPath, "Assault");
    }

    [MenuItem("GameObject/Rapid Template/TPS/Characters/Rusher", false, 11)]
    private static void AddRusherPrefab()
    {
        AddPrefabToScene(RusherPrefabPath, "Rusher");
    }

    [MenuItem("GameObject/Rapid Template/TPS/Characters/Tank", false, 11)]
    private static void AddTankPrefab()
    {
        AddPrefabToScene(TankPrefabPath, "Tank");
    }

    [MenuItem("GameObject/Rapid Template/TPS/Characters/Controller", false, 11)]
    private static void AddControllerPrefab()
    {
        AddPrefabToScene(ControllerPrefabPath, "Controller");
    }

    [MenuItem("GameObject/Rapid Template/TPS/Characters/Factions", false, 11)]
    private static void AddFactionsPrefab()
    {
        AddPrefabToScene(factionsPrefabPath, "Factions");
    }

    [MenuItem("GameObject/Rapid Template/TPS/Camera/Shot TPS", false, 14)]
    private static void AddShotPrefab()
    {
        AddPrefabToScene(shotPrefabPath, "Shot TPS");
    }

    [MenuItem("GameObject/Rapid Template/TPS/Camera/Main Camera", false, 14)]
    private static void AddShot2Prefab()
    {
        AddPrefabToScene(shot2PrefabPath, "Main Camera");
    }

    [MenuItem("GameObject/Rapid Template/TPS/Inventory/Inventory Character", false, 14)]
    private static void AddInvCharPrefab()
    {
        AddPrefabToScene(invCharPrefabPath, "Inventory Character");
    }

    [MenuItem("GameObject/Rapid Template/Save/Checkpoint", false, 14)]
    private static void AddcheckPointPrefab()
    {
        AddPrefabToScene(checkPointPath, "Checkpoint");
    }

    [MenuItem("GameObject/Rapid Template/Save/Save Point", false, 14)]
    private static void AddSavePointPrefab()
    {
        AddPrefabToScene(savePointPath, "Save Point");
    }

    [MenuItem("GameObject/Rapid Template/Basic/Bed", false, 15)]
    private static void AddBedPrefab()
    {
        AddPrefabToScene(bedPath, "Bed");
    }

    [MenuItem("GameObject/Rapid Template/Basic/Carry", false, 16)]
    private static void AddCarryPrefab()
    {
        AddPrefabToScene(carryPath, "Carry");
    }

    [MenuItem("GameObject/Rapid Template/Basic/Chair", false, 17)]
    private static void AddChairPrefab()
    {
        AddPrefabToScene(chairPath, "Chair");
    }

    [MenuItem("GameObject/Rapid Template/Basic/Chest", false, 18)]
    private static void AddChestPrefab()
    {
        AddPrefabToScene(chestPath, "Chest");
    }

    [MenuItem("GameObject/Rapid Template/Basic/Dig", false, 19)]
    private static void AddDigPrefab()
    {
        AddPrefabToScene(digPath, "Dig");
    }

    [MenuItem("GameObject/Rapid Template/Basic/Door", false, 20)]
    private static void AddDoorPrefab()
    {
        AddPrefabToScene(doorPath, "Door");
    }

    [MenuItem("GameObject/Rapid Template/Basic/Fish", false, 21)]
    private static void AddFishPrefab()
    {
        AddPrefabToScene(fishPath, "Fish");
    }

    [MenuItem("GameObject/Rapid Template/Basic/Fountain", false, 22)]
    private static void AddFountainPrefab()
    {
        AddPrefabToScene(fountainPath, "Fountain");
    }

    [MenuItem("GameObject/Rapid Template/Basic/Gather", false, 23)]
    private static void AddGatherPrefab()
    {
        AddPrefabToScene(gatherPath, "Gather");
    }

    [MenuItem("GameObject/Rapid Template/Basic/Pickup", false, 24)]
    private static void AddPickupPrefab()
    {
        AddPrefabToScene(pickupPath, "Pickup");
    }

    [MenuItem("GameObject/Rapid Template/Basic/Emote", false, 25)]
    private static void AddEmotePrefab()
    {
        AddPrefabToScene(emotePath, "Canvas-Emote");
    }

    [MenuItem("GameObject/Rapid Template/TPS/Props/Bottle", false, 25)]
    private static void AddBreak01Prefab()
    {
        AddPrefabToScene(break01Path, "Bottle");
    }

    [MenuItem("GameObject/Rapid Template/TPS/Props/Car", false, 25)]
    private static void AddBreak02Prefab()
    {
        AddPrefabToScene(break02Path, "Car");
    }

    [MenuItem("GameObject/Rapid Template/TPS/UI/HUD", false, 25)]
    private static void AddHudPrefab()
    {
        AddPrefabToScene(hudPath, "HUD");
    }

    [MenuItem("GameObject/Rapid Template/TPS/UI/Main Menu", false, 25)]
    private static void AddMenuPrefab()
    {
        AddPrefabToScene(menuPath, "Main Menu");
    }

    [MenuItem("GameObject/Rapid Template/TPS/UI/Fade In", false, 25)]
    private static void AddFadePrefab()
    {
        AddPrefabToScene(fadePath, "Fade In");
    }

    [MenuItem("GameObject/Rapid Template/TPS/UI/Indicator", false, 25)]
    private static void AddIndicatorPrefab()
    {
        AddPrefabToScene(indicatorPath, "Indicator");
    }

    [MenuItem("GameObject/Rapid Template/General/Directional Light", false, 25)]
    private static void AddLightPrefab()
    {
        AddPrefabToScene(lightPath, "Directional Light");
    }

    [MenuItem("GameObject/Rapid Template/General/Global Volume", false, 25)]
    private static void AddVolumetPrefab()
    {
        AddPrefabToScene(volumePath, "Global Volume");
    }

    [MenuItem("GameObject/Rapid Template/TPS/Cover/Low Wall 2x", false, 26)]
    private static void AddLowWall2xPrefab()
    {
        AddPrefabToScene(lowWall2xPath, "Low Wall 2x");
    }

    [MenuItem("GameObject/Rapid Template/TPS/Cover/Low Wall 4x", false, 27)]
    private static void AddLowWall4xPrefab()
    {
        AddPrefabToScene(lowWall4xPath, "Low Wall 4x");
    }

    [MenuItem("GameObject/Rapid Template/TPS/Cover/Low Wall 8x", false, 28)]
    private static void AddLowWall8xPrefab()
    {
        AddPrefabToScene(lowWall8xPath, "Low Wall 8x");
    }

    [MenuItem("GameObject/Rapid Template/TPS/Cover/High Wall 4x", false, 29)]
    private static void AddHighWall4xPrefab()
    {
        AddPrefabToScene(highWall4xPath, "High Wall 4x");
    }

    [MenuItem("GameObject/Rapid Template/TPS/Cover/Low Block", false, 30)]
    private static void AddLowBlockPrefab()
    {
        AddPrefabToScene(lowBlockPath, "Low Block");
    }

    [MenuItem("GameObject/Rapid Template/TPS/Cover/Low Corner", false, 31)]
    private static void AddLowCornerPrefab()
    {
        AddPrefabToScene(lowCornerPath, "Low Corner");
    }

    [MenuItem("GameObject/Rapid Template/TPS/Props/Ammo Box", false, 31)]
    private static void AddAmmoBoxPrefab()
    {
        AddPrefabToScene(ammoBoxPath, "Ammo Box");
    }

    [MenuItem("GameObject/Rapid Template/General/Menu Settings", false, 25)]
    private static void AddMenuSettingsPrefab()
    {
        AddPrefabToScene(menuSettingsPath, "Menu Settings");
    }

    private static void AddPrefabToScene(string prefabPath, string prefabName)
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

        if (prefab == null)
        {
            Debug.LogError($"Prefab '{prefabName}' not found at path: {prefabPath}");
            return;
        }

        GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        Undo.RegisterCreatedObjectUndo(instance, $"Create {prefabName}");
        Selection.activeObject = instance;
    }

    [MenuItem("GameObject/Rapid Template/TPS/Characters/Player", true)]
    [MenuItem("GameObject/Rapid Template/TPS/Characters/Assault", true)]
    [MenuItem("GameObject/Rapid Template/TPS/Characters/Rusher", true)]
    [MenuItem("GameObject/Rapid Template/TPS/Characters/Tank", true)]
    [MenuItem("GameObject/Rapid Template/TPS/Characters/Controller", true)]
    [MenuItem("GameObject/Rapid Template/TPS/Characters/Factions", true)]
    [MenuItem("GameObject/Rapid Template/TPS/Camera/Shot TPS", true)]
    [MenuItem("GameObject/Rapid Template/TPS/Camera/Main Camera", true)]
    [MenuItem("GameObject/Rapid Template/TPS/Inventory/Inventory Character", true)]
    [MenuItem("GameObject/Rapid Template/Save/Checkpoint", true)]
    [MenuItem("GameObject/Rapid Template/Save/Save Point", true)]
    [MenuItem("GameObject/Rapid Template/Basic/Bed", true)]
    [MenuItem("GameObject/Rapid Template/Basic/Carry", true)]
    [MenuItem("GameObject/Rapid Template/Basic/Chair", true)]
    [MenuItem("GameObject/Rapid Template/Basic/Chest", true)]
    [MenuItem("GameObject/Rapid Template/Basic/Dig", true)]
    [MenuItem("GameObject/Rapid Template/Basic/Door", true)]
    [MenuItem("GameObject/Rapid Template/Basic/Emote", true)]
    [MenuItem("GameObject/Rapid Template/Basic/Fish", true)]
    [MenuItem("GameObject/Rapid Template/Basic/Fountain", true)]
    [MenuItem("GameObject/Rapid Template/Basic/Gather", true)]
    [MenuItem("GameObject/Rapid Template/Basic/Pickup", true)]
    [MenuItem("GameObject/Rapid Template/TPS/Props/Bottle", true)]
    [MenuItem("GameObject/Rapid Template/TPS/Props/Car", true)]
    [MenuItem("GameObject/Rapid Template/TPS/UI/HUD", true)]
    [MenuItem("GameObject/Rapid Template/TPS/UI/Main Menu", true)]
    [MenuItem("GameObject/Rapid Template/TPS/UI/Indicator", true)]
    [MenuItem("GameObject/Rapid Template/TPS/UI/Fade In", true)]
    [MenuItem("GameObject/Rapid Template/TPS/Cover/Low Wall 2x", true)]
    [MenuItem("GameObject/Rapid Template/TPS/Cover/Low Wall 4x", true)]
    [MenuItem("GameObject/Rapid Template/TPS/Cover/Low Wall 8x", true)]
    [MenuItem("GameObject/Rapid Template/TPS/Cover/High Wall 4x", true)]
    [MenuItem("GameObject/Rapid Template/TPS/Cover/Low Block", true)]
    [MenuItem("GameObject/Rapid Template/TPS/Cover/Low Corner", true)]
    [MenuItem("GameObject/Rapid Template/TPS/Props/Ammo Box", true)]
    [MenuItem("GameObject/Rapid Template/RPG/UI/Menu Settings", true)]


    private static bool ValidateAddPrefab()
    {
        return true;
    }
}
