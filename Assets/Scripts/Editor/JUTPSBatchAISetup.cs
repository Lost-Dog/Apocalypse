using UnityEngine;
using UnityEditor;
using JUTPS;
using JUTPS.ActionScripts;
using JUTPS.AI;
using JUTPS.FX;
using JUTPS.InteractionSystem;
using JUTPS.InventorySystem;
using JUTPS.JUInputSystem;
using JUTPS.PhysicsScripts;
using JUTPS.WeaponSystem;
using JU;
using JU.CharacterSystem.AI;
using System.Collections.Generic;
using System.Linq;

namespace ApocalypseEditor
{
    public class JUTPSBatchAISetup : EditorWindow
    {
        private enum AIType
        {
            Patrol,
            Zombie
        }

        private enum SetupMode
        {
            AdvancedTPS,
            SimpleTPS
        }

        private List<GameObject> selectedHumanoids = new List<GameObject>();
        private Vector2 scrollPosition;
        private AIType aiType = AIType.Patrol;
        private SetupMode setupMode = SetupMode.AdvancedTPS;
        
        private bool useRootMotion = true;
        private bool addFootPlacement = true;
        private bool addFootstep = true;
        private bool addBodyLean = true;
        private bool addRagdoll = true;
        
        private float moveSpeed = 3f;
        private float rotationSpeed = 3f;
        private float stoppingSpeed = 4f;

        private string characterTag = "Enemy";
        private int characterLayer = 8;

        private WaypointPath patrolPath = null;
        private JUBoxArea patrolArea = null;

        [MenuItem("Tools/JUTPS/Batch AI Setup")]
        public static void ShowWindow()
        {
            var window = GetWindow<JUTPSBatchAISetup>("Batch AI Setup");
            window.minSize = new Vector2(450, 600);
            window.Show();
        }

        private void OnEnable()
        {
            RefreshSelectedHumanoids();
        }

        private void OnSelectionChange()
        {
            RefreshSelectedHumanoids();
            Repaint();
        }

        private void RefreshSelectedHumanoids()
        {
            selectedHumanoids.Clear();

            if (Selection.gameObjects == null || Selection.gameObjects.Length == 0)
                return;

            foreach (var obj in Selection.gameObjects)
            {
                if (obj == null)
                    continue;

                if (IsValidHumanoid(obj))
                {
                    selectedHumanoids.Add(obj);
                }
            }
        }

        private bool IsValidHumanoid(GameObject obj)
        {
            var animator = obj.GetComponent<Animator>();
            if (animator == null)
                return false;

            if (!animator.isHuman)
                return false;

            return true;
        }

        private void OnGUI()
        {
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("JUTPS Batch AI Setup Tool", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Setup multiple humanoid characters as JUTPS AI characters at once.", MessageType.Info);
            EditorGUILayout.Space(10);

            DrawSelectionSection();
            EditorGUILayout.Space(10);

            DrawSetupOptionsSection();
            EditorGUILayout.Space(10);

            DrawAIConfigurationSection();
            EditorGUILayout.Space(10);

            DrawActionButtons();
        }

        private void DrawSelectionSection()
        {
            EditorGUILayout.LabelField("Selected Humanoids", EditorStyles.boldLabel);
            
            if (selectedHumanoids.Count == 0)
            {
                EditorGUILayout.HelpBox("No valid humanoid characters selected. Select one or more humanoid GameObjects in the scene.", MessageType.Warning);
                return;
            }

            EditorGUILayout.LabelField($"Valid Humanoids: {selectedHumanoids.Count}", EditorStyles.miniLabel);
            
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.MaxHeight(150));
            foreach (var humanoid in selectedHumanoids)
            {
                if (humanoid == null)
                    continue;

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.ObjectField(humanoid, typeof(GameObject), true);
                
                bool hasController = humanoid.GetComponent<JUCharacterController>() != null;
                bool hasAI = humanoid.GetComponent<JU_AI_PatrolCharacter>() != null || 
                            humanoid.GetComponent<JU_AI_Zombie>() != null;

                if (hasController)
                    EditorGUILayout.LabelField("✓ Controller", GUILayout.Width(80));
                else
                    EditorGUILayout.LabelField("✗ Controller", GUILayout.Width(80));

                if (hasAI)
                    EditorGUILayout.LabelField("✓ AI", GUILayout.Width(50));
                else
                    EditorGUILayout.LabelField("✗ AI", GUILayout.Width(50));

                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndScrollView();
        }

        private void DrawSetupOptionsSection()
        {
            EditorGUILayout.LabelField("Character Setup Options", EditorStyles.boldLabel);
            
            setupMode = (SetupMode)EditorGUILayout.EnumPopup("Setup Mode", setupMode);
            
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            moveSpeed = EditorGUILayout.FloatField("Move Speed", moveSpeed);
            rotationSpeed = EditorGUILayout.FloatField("Rotation Speed", rotationSpeed);
            stoppingSpeed = EditorGUILayout.FloatField("Stopping Speed", stoppingSpeed);
            
            EditorGUILayout.Space(5);
            useRootMotion = EditorGUILayout.Toggle("Use Root Motion", useRootMotion);
            addFootPlacement = EditorGUILayout.Toggle("Add Foot Placement", addFootPlacement);
            addFootstep = EditorGUILayout.Toggle("Add Footstep", addFootstep);
            addBodyLean = EditorGUILayout.Toggle("Add Body Lean", addBodyLean);
            addRagdoll = EditorGUILayout.Toggle("Add Ragdoll", addRagdoll);
            
            EditorGUILayout.Space(5);
            characterTag = EditorGUILayout.TagField("Character Tag", characterTag);
            characterLayer = EditorGUILayout.LayerField("Character Layer", characterLayer);
            
            EditorGUILayout.EndVertical();
        }

        private void DrawAIConfigurationSection()
        {
            EditorGUILayout.LabelField("AI Configuration", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            aiType = (AIType)EditorGUILayout.EnumPopup("AI Type", aiType);
            
            if (aiType == AIType.Patrol)
            {
                EditorGUILayout.Space(5);
                EditorGUILayout.LabelField("Patrol Settings", EditorStyles.miniBoldLabel);
                patrolPath = (WaypointPath)EditorGUILayout.ObjectField("Patrol Path (Optional)", patrolPath, typeof(WaypointPath), true);
                patrolArea = (JUBoxArea)EditorGUILayout.ObjectField("Patrol Area (Optional)", patrolArea, typeof(JUBoxArea), true);
                
                if (patrolPath == null && patrolArea == null)
                {
                    EditorGUILayout.HelpBox("No patrol path or area assigned. AI will patrol randomly around spawn position.", MessageType.Info);
                }
            }
            
            EditorGUILayout.EndVertical();
        }

        private void DrawActionButtons()
        {
            if (selectedHumanoids.Count == 0)
            {
                GUI.enabled = false;
            }

            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Button("Setup Character Controllers Only", GUILayout.Height(30)))
            {
                SetupCharacterControllers();
            }
            
            if (GUILayout.Button("Setup AI Only", GUILayout.Height(30)))
            {
                SetupAIComponents();
            }
            
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space(5);
            
            if (GUILayout.Button("Setup Complete AI Characters (Controller + AI)", GUILayout.Height(40)))
            {
                SetupCompleteAICharacters();
            }

            GUI.enabled = true;

            EditorGUILayout.Space(10);
            
            if (GUILayout.Button("Refresh Selection", GUILayout.Height(25)))
            {
                RefreshSelectedHumanoids();
            }
        }

        private void SetupCharacterControllers()
        {
            int successCount = 0;
            int skippedCount = 0;

            foreach (var humanoid in selectedHumanoids)
            {
                if (humanoid == null)
                    continue;

                if (humanoid.GetComponent<JUCharacterController>() != null)
                {
                    Debug.LogWarning($"{humanoid.name} already has JUCharacterController. Skipping.", humanoid);
                    skippedCount++;
                    continue;
                }

                try
                {
                    SetupCharacterController(humanoid);
                    successCount++;
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"Failed to setup {humanoid.name}: {e.Message}", humanoid);
                }
            }

            EditorUtility.DisplayDialog("Setup Complete", 
                $"Character controllers setup:\nSuccess: {successCount}\nSkipped: {skippedCount}", 
                "OK");
        }

        private void SetupAIComponents()
        {
            int successCount = 0;
            int skippedCount = 0;

            foreach (var humanoid in selectedHumanoids)
            {
                if (humanoid == null)
                    continue;

                if (humanoid.GetComponent<JUCharacterController>() == null)
                {
                    Debug.LogWarning($"{humanoid.name} doesn't have JUCharacterController. Setup controller first.", humanoid);
                    skippedCount++;
                    continue;
                }

                if (humanoid.GetComponent<JU_AI_PatrolCharacter>() != null || 
                    humanoid.GetComponent<JU_AI_Zombie>() != null)
                {
                    Debug.LogWarning($"{humanoid.name} already has AI component. Skipping.", humanoid);
                    skippedCount++;
                    continue;
                }

                try
                {
                    SetupAIComponent(humanoid);
                    successCount++;
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"Failed to setup AI on {humanoid.name}: {e.Message}", humanoid);
                }
            }

            EditorUtility.DisplayDialog("Setup Complete", 
                $"AI components setup:\nSuccess: {successCount}\nSkipped: {skippedCount}", 
                "OK");
        }

        private void SetupCompleteAICharacters()
        {
            int successCount = 0;
            int skippedCount = 0;

            foreach (var humanoid in selectedHumanoids)
            {
                if (humanoid == null)
                    continue;

                try
                {
                    bool needsController = humanoid.GetComponent<JUCharacterController>() == null;
                    bool needsAI = humanoid.GetComponent<JU_AI_PatrolCharacter>() == null && 
                                   humanoid.GetComponent<JU_AI_Zombie>() == null;

                    if (!needsController && !needsAI)
                    {
                        Debug.LogWarning($"{humanoid.name} already has controller and AI. Skipping.", humanoid);
                        skippedCount++;
                        continue;
                    }

                    if (needsController)
                    {
                        SetupCharacterController(humanoid);
                    }

                    if (needsAI)
                    {
                        SetupAIComponent(humanoid);
                    }

                    successCount++;
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"Failed to setup {humanoid.name}: {e.Message}", humanoid);
                }
            }

            EditorUtility.DisplayDialog("Setup Complete", 
                $"Complete AI setup:\nSuccess: {successCount}\nSkipped: {skippedCount}", 
                "OK");
        }

        private void SetupCharacterController(GameObject character)
        {
            Undo.RegisterCompleteObjectUndo(character, "Setup JUTPS Character");

            var animator = character.GetComponent<Animator>();
            if (animator == null || !animator.isHuman)
            {
                throw new System.Exception("Character must have a humanoid animator.");
            }

            if (!character.TryGetComponent<CapsuleCollider>(out var col))
            {
                col = Undo.AddComponent<CapsuleCollider>(character);
            }

            var noSlip = Resources.Load<PhysicsMaterial>("NoSlip");
            if (noSlip != null)
                col.material = noSlip;

            col.height = 1.7f;
            col.center = new Vector3(0, 0.85f, 0);
            col.radius = 0.4f;

            Undo.AddComponent<ResizableCapsuleCollider>(character);

            if (!character.TryGetComponent<Rigidbody>(out var rb))
            {
                rb = Undo.AddComponent<Rigidbody>(character);
            }

            rb.mass = 85;
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
            rb.constraints = RigidbodyConstraints.FreezeRotation;

            if (!character.TryGetComponent<JUInteractionSystem>(out var interaction))
            {
                Undo.AddComponent<JUInteractionSystem>(character);
            }

            var tps = Undo.AddComponent<JUCharacterController>(character);

            string animatorPath = aiType == AIType.Zombie 
                ? "Assets/Julhiecio TPS Controller/Animations/Animator/Animator Zombie Controller.controller"
                : "Assets/Julhiecio TPS Controller/Animations/Animator/AnimatorTPS Controller.controller";

            var animatorController = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(animatorPath);
            if (animatorController != null)
            {
                animator.runtimeAnimatorController = animatorController;
            }

            string inputAssetPath = "Assets/Julhiecio TPS Controller/Input Controls/Player Character Inputs.asset";
            var inputAsset = AssetDatabase.LoadAssetAtPath<JUPlayerCharacterInputAsset>(inputAssetPath);
            if (inputAsset != null)
            {
                tps.Inputs = inputAsset;
            }

            character.tag = characterTag;
            character.layer = characterLayer;

            CreateWeaponRotationCenter(character, tps);

            tps.Speed = moveSpeed;
            tps.CurvedMovement = true;
            tps.RotationSpeed = rotationSpeed;
            tps.LerpRotation = true;
            tps.StoppingSpeed = stoppingSpeed;
            tps.RootMotion = useRootMotion;

            if (addFootPlacement)
            {
                Undo.AddComponent<JUFootPlacement>(character);
            }

            if (addFootstep)
            {
                var footstep = Undo.AddComponent<JUFootstep>(character);
                footstep.LoadDefaultFootstepInInspector();
            }

            if (addBodyLean)
            {
                Undo.AddComponent<BodyLeanInert>(character);
            }

            if (addRagdoll)
            {
                Undo.AddComponent<AdvancedRagdollController>(character);
            }

            var inventory = Undo.AddComponent<JUInventory>(character);
            if (inputAsset != null)
            {
                inventory.PlayerInputs = inputAsset;
            }

            if (!character.TryGetComponent<AudioSource>(out var audioSource))
            {
                Undo.AddComponent<AudioSource>(character);
            }

            if (!character.TryGetComponent<JUHealth>(out var health))
            {
                Undo.AddComponent<JUHealth>(character);
            }

            Debug.Log($"Character controller setup complete for {character.name}", character);
        }

        private void CreateWeaponRotationCenter(GameObject character, JUCharacterController tps)
        {
            var existingCenter = character.GetComponentInChildren<WeaponAimRotationCenter>();
            if (existingCenter != null)
            {
                tps.PivotItemRotation = existingCenter.gameObject;
                return;
            }

            var animator = character.GetComponent<Animator>();
            if (animator == null || !animator.isHuman)
                return;

            var spine = animator.GetBoneTransform(HumanBodyBones.Spine);
            if (spine == null)
                return;

            GameObject rotationCenter = new GameObject("ItemRotationCenter");
            Undo.RegisterCreatedObjectUndo(rotationCenter, "Create Weapon Rotation Center");
            rotationCenter.transform.SetParent(spine);
            rotationCenter.transform.localPosition = Vector3.zero;
            rotationCenter.transform.localRotation = Quaternion.identity;

            var centerComponent = Undo.AddComponent<WeaponAimRotationCenter>(rotationCenter);
            tps.PivotItemRotation = rotationCenter;
        }

        private void SetupAIComponent(GameObject character)
        {
            Undo.RegisterCompleteObjectUndo(character, "Setup JUTPS AI");

            var animator = character.GetComponent<Animator>();
            
            if (aiType == AIType.Patrol)
            {
                var patrolAI = Undo.AddComponent<JU_AI_PatrolCharacter>(character);
                
                if (animator != null && animator.isHuman)
                {
                    patrolAI.Head = animator.GetBoneTransform(HumanBodyBones.Head);
                }

                if (patrolPath != null)
                {
                    patrolAI.PatrolPath = patrolPath;
                }

                if (patrolArea != null)
                {
                    patrolAI.PatrolArea = patrolArea;
                }

                patrolAI.PatrolRandomlyIfNotHavePath = (patrolPath == null && patrolArea == null);

                Debug.Log($"Patrol AI setup complete for {character.name}", character);
            }
            else if (aiType == AIType.Zombie)
            {
                var zombieAI = Undo.AddComponent<JU_AI_Zombie>(character);
                
                if (animator != null && animator.isHuman)
                {
                    zombieAI.Head = animator.GetBoneTransform(HumanBodyBones.Head);
                }

                Debug.Log($"Zombie AI setup complete for {character.name}", character);
            }
        }
    }
}
