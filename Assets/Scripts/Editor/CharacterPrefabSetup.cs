using UnityEngine;
using UnityEngine.AI;
using UnityEditor;

public class CharacterPrefabSetup : EditorWindow
{
    [MenuItem("Tools/Setup Character Prefabs")]
    public static void ShowWindow()
    {
        GetWindow<CharacterPrefabSetup>("Character Setup");
    }

    private GameObject characterPrefab;
    private RuntimeAnimatorController animatorController;
    private bool isCivilian = true;
    private bool isEnemy = false;
    
    private void OnGUI()
    {
        GUILayout.Label("Character Prefab Setup", EditorStyles.boldLabel);
        GUILayout.Space(10);
        
        characterPrefab = (GameObject)EditorGUILayout.ObjectField("Character Prefab", characterPrefab, typeof(GameObject), false);
        animatorController = (RuntimeAnimatorController)EditorGUILayout.ObjectField("Animator Controller", animatorController, typeof(RuntimeAnimatorController), false);
        
        GUILayout.Space(10);
        GUILayout.Label("Character Type:", EditorStyles.boldLabel);
        isCivilian = EditorGUILayout.Toggle("Civilian (Friendly)", isCivilian);
        isEnemy = EditorGUILayout.Toggle("Enemy (Hostile)", isEnemy);
        
        GUILayout.Space(20);
        
        if (GUILayout.Button("Setup Character", GUILayout.Height(40)))
        {
            SetupCharacter();
        }
        
        GUILayout.Space(10);
        EditorGUILayout.HelpBox(
            "This will add:\n" +
            "• NavMeshAgent\n" +
            "• Animator\n" +
            "• CivilianBehavior or Enemy AI\n" +
            "• Capsule Collider\n" +
            "• Rigidbody (if needed)", 
            MessageType.Info
        );
    }
    
    private void SetupCharacter()
    {
        if (characterPrefab == null)
        {
            EditorUtility.DisplayDialog("Error", "Please assign a character prefab!", "OK");
            return;
        }
        
        if (animatorController == null)
        {
            EditorUtility.DisplayDialog("Error", "Please assign an animator controller!", "OK");
            return;
        }
        
        string prefabPath = AssetDatabase.GetAssetPath(characterPrefab);
        GameObject instance = PrefabUtility.LoadPrefabContents(prefabPath);
        
        NavMeshAgent agent = instance.GetComponent<NavMeshAgent>();
        if (agent == null)
        {
            agent = instance.AddComponent<NavMeshAgent>();
            agent.radius = 0.5f;
            agent.height = 1.8f;
            agent.speed = 1.5f;
            agent.acceleration = 8f;
            agent.angularSpeed = 120f;
            agent.stoppingDistance = 0.5f;
        }
        
        Animator animator = instance.GetComponentInChildren<Animator>();
        if (animator == null)
        {
            animator = instance.AddComponent<Animator>();
        }
        animator.runtimeAnimatorController = animatorController;
        animator.applyRootMotion = false;
        
        CapsuleCollider collider = instance.GetComponent<CapsuleCollider>();
        if (collider == null)
        {
            collider = instance.AddComponent<CapsuleCollider>();
            collider.center = new Vector3(0, 0.9f, 0);
            collider.radius = 0.3f;
            collider.height = 1.8f;
        }
        
        Rigidbody rb = instance.GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = instance.AddComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.useGravity = false;
        }
        
        if (isCivilian)
        {
            if (instance.GetComponent<CivilianBehavior>() == null)
            {
                instance.AddComponent<CivilianBehavior>();
            }
            instance.tag = "Civilian";
            instance.layer = LayerMask.NameToLayer("Civilian");
        }
        
        if (isEnemy)
        {
            instance.tag = "Enemy";
            instance.layer = LayerMask.NameToLayer("Enemy");
        }
        
        PrefabUtility.SaveAsPrefabAsset(instance, prefabPath);
        PrefabUtility.UnloadPrefabContents(instance);
        
        EditorUtility.DisplayDialog("Success", $"Character prefab '{characterPrefab.name}' has been configured!", "OK");
    }
}
