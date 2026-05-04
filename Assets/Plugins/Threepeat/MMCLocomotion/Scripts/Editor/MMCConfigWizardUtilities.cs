using MxM;
using System.Collections;
using System.Collections.Generic;
using Threepeat;
using UnityEditor;
using UnityEngine;

namespace ThreepeatEditor
{
    public static class MMCConfigWizardUtilities
    {
        public static void SetupCharacterController(ref CharacterController charController)
        {
            charController.slopeLimit = 50;
            charController.stepOffset = 0.3f;
            charController.skinWidth = 0.008f; //0.008f;
            charController.minMoveDistance = 0.0001f;
            charController.center = new Vector3(0f, 0.925f, 0f);
            charController.radius = 0.3f;
            charController.height = 1.85f;
        }

        public static void SetupCharacterBaseStuff<T>(
                ref T ngpc,
                ref MxMAnimator mxma,
                ref MxMTrajectoryGenerator mxmt,
//                ref LocomotionSpeedRamp lsr,
                ref MMCConfigWizardGUIDManager guidMgr) where T : NGCharacter
        {
            ngpc.mxmAnimator = mxma;
            ngpc.mxmTrajectoryGenerator = mxmt;

            ngpc.inputProfileLocomotion = mxmt.InputProfile;
            ngpc.inputProfileSprint = AssetDatabase.LoadAssetAtPath<MxMInputProfile>(AssetDatabase.GUIDToAssetPath(MMCConfigWizardUtilities.GetGuidFromManagerKey(ref guidMgr, "InputProfileSprint"))); //"780c76106035a7e4bbaacd7defdeb978"));
            ngpc.inputProfileStrafe = AssetDatabase.LoadAssetAtPath<MxMInputProfile>(AssetDatabase.GUIDToAssetPath(MMCConfigWizardUtilities.GetGuidFromManagerKey(ref guidMgr, "InputProfileStrafe")));
                

            //TODO: add different config selection to the wizard
            ngpc.config = ScriptableObject.CreateInstance<NGCharacterBaseConfig>();
            ngpc.movement = new NGCharacter_MovementHelper(ngpc);
            ngpc.anim = new NGCharacter_AnimationHelper(ngpc);
            ngpc.AI = new NGCharacter_AIHelper(ngpc);
            ngpc.movement.canJump = true;
            ngpc.movement.canRun = true;
            
            //ngpc.canParkour = false;
            ngpc.config.configJump = AssetDatabase.LoadAssetAtPath<NGJumpSettings>(AssetDatabase.GUIDToAssetPath(MMCConfigWizardUtilities.GetGuidFromManagerKey(ref guidMgr, "NGJumpConfig"))); //"b9888a40a727cfd4d995c6da13f48692"))); // ;

        }

        public static string GetGuidFromManagerKey(ref MMCConfigWizardGUIDManager guidMgr, string key)
        {
            string ret = null;

            if (guidMgr == null)
            {
                string[] tmpGuids = AssetDatabase.FindAssets("t:MMCConfigWizardGUIDManager");
                if (tmpGuids.Length > 0)
                {
                    guidMgr = AssetDatabase.LoadAssetAtPath<MMCConfigWizardGUIDManager>(
                                        AssetDatabase.GUIDToAssetPath(tmpGuids[0]));
                }
            }

            if (guidMgr == null)
            {
                return ret;
            }

            return guidMgr.GetGuidByKey(key);
        }

    }

}
