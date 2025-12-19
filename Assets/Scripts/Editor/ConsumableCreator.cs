using UnityEngine;
using UnityEditor;
using System.IO;

public class ConsumableCreator : EditorWindow
{
    private string itemName = "New Consumable";
    private float hungerRestore = 0f;
    private float thirstRestore = 0f;
    private float healthRestore = 0f;
    private float staminaRestore = 0f;
    private float infectionReduction = 0f;
    
    private Vector2 scrollPosition;
    
    [MenuItem("Division Game/Survival/Create Consumable Items")]
    public static void ShowWindow()
    {
        GetWindow<ConsumableCreator>("Consumable Creator");
    }
    
    private void OnGUI()
    {
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        
        EditorGUILayout.LabelField("Consumable Item Creator", EditorStyles.boldLabel);
        EditorGUILayout.Space();
        
        EditorGUILayout.HelpBox("Create consumable items for your survival game. Quick create buttons provide pre-configured items.", MessageType.Info);
        EditorGUILayout.Space();
        
        EditorGUILayout.LabelField("Custom Consumable", EditorStyles.boldLabel);
        itemName = EditorGUILayout.TextField("Item Name", itemName);
        hungerRestore = EditorGUILayout.Slider("Hunger Restore", hungerRestore, 0f, 100f);
        thirstRestore = EditorGUILayout.Slider("Thirst Restore", thirstRestore, 0f, 100f);
        healthRestore = EditorGUILayout.Slider("Health Restore", healthRestore, 0f, 100f);
        staminaRestore = EditorGUILayout.Slider("Stamina Restore", staminaRestore, 0f, 100f);
        infectionReduction = EditorGUILayout.Slider("Infection Reduction", infectionReduction, 0f, 100f);
        
        if (GUILayout.Button("Create Custom Item", GUILayout.Height(30)))
        {
            CreateConsumable(itemName, hungerRestore, thirstRestore, healthRestore, staminaRestore, infectionReduction);
        }
        
        EditorGUILayout.Space(20);
        EditorGUILayout.LabelField("Quick Create - Food Items", EditorStyles.boldLabel);
        
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("🥫 Canned Food\n+40 Hunger, +10 HP", GUILayout.Height(50)))
        {
            CreateConsumable("Canned Food", 40f, 0f, 10f, 0f, 0f);
        }
        if (GUILayout.Button("🍞 Bread\n+25 Hunger", GUILayout.Height(50)))
        {
            CreateConsumable("Bread", 25f, 0f, 0f, 0f, 0f);
        }
        if (GUILayout.Button("🍎 Fresh Fruit\n+15 Hunger, +5 HP", GUILayout.Height(50)))
        {
            CreateConsumable("Fresh Fruit", 15f, 0f, 5f, 0f, 0f);
        }
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("🥩 Cooked Meat\n+50 Hunger, +20 HP", GUILayout.Height(50)))
        {
            CreateConsumable("Cooked Meat", 50f, 0f, 20f, 0f, 0f);
        }
        if (GUILayout.Button("🍫 Chocolate Bar\n+20 Hunger, +15 Stamina", GUILayout.Height(50)))
        {
            CreateConsumable("Chocolate Bar", 20f, 0f, 0f, 15f, 0f);
        }
        if (GUILayout.Button("🌾 Protein Bar\n+35 Hunger, +10 Stamina", GUILayout.Height(50)))
        {
            CreateConsumable("Protein Bar", 35f, 0f, 0f, 10f, 0f);
        }
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Quick Create - Water Items", EditorStyles.boldLabel);
        
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("💧 Water Bottle\n+50 Thirst", GUILayout.Height(50)))
        {
            CreateConsumable("Water Bottle", 0f, 50f, 0f, 0f, 0f);
        }
        if (GUILayout.Button("🥤 Energy Drink\n+30 Thirst, +25 Stamina", GUILayout.Height(50)))
        {
            CreateConsumable("Energy Drink", 0f, 30f, 0f, 25f, 0f);
        }
        if (GUILayout.Button("☕ Coffee\n+15 Thirst, +20 Stamina", GUILayout.Height(50)))
        {
            CreateConsumable("Coffee", 0f, 15f, 0f, 20f, 0f);
        }
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("🧃 Juice Box\n+20 Thirst, +5 Hunger", GUILayout.Height(50)))
        {
            CreateConsumable("Juice Box", 5f, 20f, 0f, 0f, 0f);
        }
        if (GUILayout.Button("🥛 Sports Drink\n+40 Thirst, +15 Stamina", GUILayout.Height(50)))
        {
            CreateConsumable("Sports Drink", 0f, 40f, 0f, 15f, 0f);
        }
        if (GUILayout.Button("💦 Purified Water\n+60 Thirst", GUILayout.Height(50)))
        {
            CreateConsumable("Purified Water", 0f, 60f, 0f, 0f, 0f);
        }
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Quick Create - Medicine", EditorStyles.boldLabel);
        
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("💊 Med Kit\n+50 HP", GUILayout.Height(50)))
        {
            CreateConsumable("Med Kit", 0f, 0f, 50f, 0f, 0f);
        }
        if (GUILayout.Button("💉 Antibiotics\n+25 HP, -50 Infection", GUILayout.Height(50)))
        {
            CreateConsumable("Antibiotics", 0f, 0f, 25f, 0f, 50f);
        }
        if (GUILayout.Button("🩹 Bandage\n+15 HP", GUILayout.Height(50)))
        {
            CreateConsumable("Bandage", 0f, 0f, 15f, 0f, 0f);
        }
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("💊 Painkiller\n+10 HP, +10 Stamina", GUILayout.Height(50)))
        {
            CreateConsumable("Painkiller", 0f, 0f, 10f, 10f, 0f);
        }
        if (GUILayout.Button("🧪 Antiviral\n-75 Infection", GUILayout.Height(50)))
        {
            CreateConsumable("Antiviral", 0f, 0f, 0f, 0f, 75f);
        }
        if (GUILayout.Button("⚕️ Advanced Med Kit\n+75 HP, -25 Infection", GUILayout.Height(50)))
        {
            CreateConsumable("Advanced Med Kit", 0f, 0f, 75f, 0f, 25f);
        }
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Quick Create - Survival Packs", EditorStyles.boldLabel);
        
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("📦 MRE (Meal Ready-to-Eat)\n+60 Hunger, +30 Thirst", GUILayout.Height(50)))
        {
            CreateConsumable("MRE", 60f, 30f, 0f, 0f, 0f);
        }
        if (GUILayout.Button("🎒 Survival Ration\n+45 Hunger, +25 Thirst, +15 HP", GUILayout.Height(50)))
        {
            CreateConsumable("Survival Ration", 45f, 25f, 15f, 0f, 0f);
        }
        if (GUILayout.Button("🧰 Emergency Kit\n+30 HP, +20 Stamina, -30 Infection", GUILayout.Height(50)))
        {
            CreateConsumable("Emergency Kit", 0f, 0f, 30f, 20f, 30f);
        }
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space(20);
        
        if (GUILayout.Button("📁 Open Consumables Folder", GUILayout.Height(30)))
        {
            string path = "Assets/ScriptableObjects/Consumables";
            if (Directory.Exists(path))
            {
                EditorUtility.RevealInFinder(path);
            }
            else
            {
                EditorUtility.DisplayDialog("Folder Not Found", $"The folder {path} does not exist yet. Create your first consumable to generate it!", "OK");
            }
        }
        
        EditorGUILayout.EndScrollView();
    }
    
    private void CreateConsumable(string name, float hunger, float thirst, float health, float stamina, float infection)
    {
        string folderPath = "Assets/ScriptableObjects/Consumables";
        
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
            AssetDatabase.Refresh();
        }
        
        ConsumableItem item = ScriptableObject.CreateInstance<ConsumableItem>();
        item.itemName = name;
        item.hungerRestore = hunger;
        item.thirstRestore = thirst;
        item.healthRestore = health;
        item.staminaRestore = stamina;
        item.infectionChange = -infection;
        
        string assetPath = $"{folderPath}/{name}.asset";
        assetPath = AssetDatabase.GenerateUniqueAssetPath(assetPath);
        
        AssetDatabase.CreateAsset(item, assetPath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
        EditorGUIUtility.PingObject(item);
        Selection.activeObject = item;
        
        Debug.Log($"<color=green>✓ Created consumable: {name} at {assetPath}</color>");
    }
}
