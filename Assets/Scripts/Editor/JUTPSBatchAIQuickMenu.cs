using UnityEngine;
using UnityEditor;
using JUTPS;
using JUTPS.ActionScripts;
using JUTPS.FX;
using JUTPS.InteractionSystem;
using JUTPS.InventorySystem;
using JUTPS.JUInputSystem;
using JUTPS.PhysicsScripts;
using JUTPS.WeaponSystem;
using JU.CharacterSystem.AI;

namespace ApocalypseEditor
{
    public static class JUTPSBatchAIQuickMenu
    {
        private const string ANIMATOR_TPS_PATH = "Assets/Julhiecio TPS Controller/Animations/Animator/AnimatorTPS Controller.controller";
        private const string ANIMATOR_ZOMBIE_PATH = "Assets/Julhiecio TPS Controller/Animations/Animator/Animator Zombie Controller.controller";
        private const string INPUT_ASSET_PATH = "Assets/Julhiecio TPS Controller/Input Controls/Player Character Inputs.asset";

        [MenuItem("GameObject/JUTPS Batch/Setup Selected as Patrol AI", false, 0)]
        public static void BatchSetupPatrolAI()
        {
            if (!ValidateSelection())
                return;

            int successCount = 0;
            int errorCount = 0;

            foreach (var obj in Selection.gameObjects)
            {
                if (SetupAsPatrolAI(obj))
                    successCount++;
                else
                    errorCount++;
            }

            ShowResultDialog("Patrol AI Batch Setup", successCount, errorCount);
        }

        [MenuItem("GameObject/JUTPS Batch/Setup Selected as Zombie AI", false, 1)]
        public static void BatchSetupZombieAI()
        {
            if (!ValidateSelection())
                return;

            int successCount = 0;
            int errorCount = 0;

            foreach (var obj in Selection.gameObjects)
            {
                if (SetupAsZombieAI(obj))
                    successCount++;
                else
                    errorCount++;
            }

            ShowResultDialog("Zombie AI Batch Setup", successCount, errorCount);
        }

        [MenuItem("GameObject/JUTPS Batch/Add AI to Selected Characters", false, 2)]
        public static void BatchAddAIOnly()
        {
            if (!ValidateSelection())
                return;

            int successCount = 0;
            int errorCount = 0;

            foreach (var obj in Selection.gameObjects)
            {
                if (!obj.GetComponent<JUCharacterController>())
                {
                    Debug.LogWarning($"{obj.name} doesn't have JUCharacterController. Skipping.", obj);
                    errorCount++;
                    continue;
                }

                if (AddPatrolAIComponent(obj))
                    successCount++;
                else
                    errorCount++;
            }

            ShowResultDialog("Add AI Components", successCount, errorCount);
        }

        [MenuItem("GameObject/JUTPS Batch/Setup Selected as Patrol AI", true)]
        [MenuItem("GameObject/JUTPS Batch/Setup Selected as Zombie AI", true)]
        [MenuItem("GameObject/JUTPS Batch/Add AI to Selected Characters", true)]
        public static bool ValidateSelection()
        {
            if (Selection.gameObjects == null || Selection.gameObjects.Length == 0)
            {
                EditorUtility.DisplayDialog("No Selection", "Please select one or more humanoid GameObjects.", "OK");
                return false;
            }

            bool hasValidHumanoid = false;
            foreach (var obj in Selection.gameObjects)
            {
                var animator = obj.GetComponent<Animator>();
                if (animator != null && animator.isHuman)
                {
                    hasValidHumanoid = true;
                    break;
                }
            }

            if (!hasValidHumanoid)
            {
                EditorUtility.DisplayDialog("Invalid Selection", "No valid humanoid characters found in selection.", "OK");
                return false;
            }

            return true;
        }

        private static bool SetupAsPatrolAI(GameObject character)
        {
            try
            {
                var animator = character.GetComponent<Animator>();
                if (animator == null || !animator.isHuman)
                {
                    Debug.LogWarning($"{character.name} is not a humanoid character. Skipping.", character);
                    return false;
                }

                if (character.GetComponent<JUCharacterController>() == null)
                {
                    SetupCharacterController(character, ANIMATOR_TPS_PATH, "Enemy", 8);
                }

                if (character.GetComponent<JU_AI_PatrolCharacter>() == null)
                {
                    AddPatrolAIComponent(character);
                }

                Debug.Log($"Successfully setup {character.name} as Patrol AI", character);
                return true;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to setup {character.name}: {e.Message}", character);
                return false;
            }
        }

        private static bool SetupAsZombieAI(GameObject character)
        {
            try
            {
                var animator = character.GetComponent<Animator>();
                if (animator == null || !animator.isHuman)
                {
                    Debug.LogWarning($"{character.name} is not a humanoid character. Skipping.", character);
                    return false;
                }

                if (character.GetComponent<JUCharacterController>() == null)
                {
                    SetupCharacterController(character, ANIMATOR_ZOMBIE_PATH, "Enemy", 8);
                }

                if (character.GetComponent<JU_AI_Zombie>() == null)
                {
                    AddZombieAIComponent(character);
                }

                Debug.Log($"Successfully setup {character.name} as Zombie AI", character);
                return true;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to setup {character.name}: {e.Message}", character);
                return false;
            }
        }

        private static void SetupCharacterController(GameObject character, string animatorPath, string tag, int layer)
        {
            Undo.RegisterCompleteObjectUndo(character, "Setup Character Controller");

            var animator = character.GetComponent<Animator>();

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

            var animatorController = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(animatorPath);
            if (animatorController != null)
            {
                animator.runtimeAnimatorController = animatorController;
            }

            var inputAsset = AssetDatabase.LoadAssetAtPath<JUPlayerCharacterInputAsset>(INPUT_ASSET_PATH);
            if (inputAsset != null)
            {
                tps.Inputs = inputAsset;
            }

            character.tag = tag;
            character.layer = layer;

            CreateWeaponRotationCenter(character, tps);

            tps.Speed = 3f;
            tps.CurvedMovement = true;
            tps.RotationSpeed = 3f;
            tps.LerpRotation = true;
            tps.StoppingSpeed = 4f;
            tps.RootMotion = true;

            Undo.AddComponent<JUFootPlacement>(character);

            var footstep = Undo.AddComponent<JUFootstep>(character);
            footstep.LoadDefaultFootstepInInspector();

            Undo.AddComponent<BodyLeanInert>(character);
            Undo.AddComponent<AdvancedRagdollController>(character);

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
        }

        private static void CreateWeaponRotationCenter(GameObject character, JUCharacterController tps)
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

        private static bool AddPatrolAIComponent(GameObject character)
        {
            var animator = character.GetComponent<Animator>();

            var patrolAI = Undo.AddComponent<JU_AI_PatrolCharacter>(character);

            if (animator != null && animator.isHuman)
            {
                patrolAI.Head = animator.GetBoneTransform(HumanBodyBones.Head);
            }

            patrolAI.PatrolRandomlyIfNotHavePath = true;

            return true;
        }

        private static bool AddZombieAIComponent(GameObject character)
        {
            var animator = character.GetComponent<Animator>();

            var zombieAI = Undo.AddComponent<JU_AI_Zombie>(character);

            if (animator != null && animator.isHuman)
            {
                zombieAI.Head = animator.GetBoneTransform(HumanBodyBones.Head);
            }

            return true;
        }

        private static void ShowResultDialog(string title, int successCount, int errorCount)
        {
            string message = $"Setup Results:\n\n" +
                           $"Successfully processed: {successCount}\n" +
                           $"Errors/Skipped: {errorCount}";

            EditorUtility.DisplayDialog(title, message, "OK");
        }
    }
}
