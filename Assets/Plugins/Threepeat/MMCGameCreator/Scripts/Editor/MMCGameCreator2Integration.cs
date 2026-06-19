using GameCreator.Runtime.Characters;
using GameCreator.Runtime.Characters.IK;
using MxM;
using MxMGameplay;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Threepeat;
using ThreepeatEditor;
using UnityEditor;
using UnityEngine;

namespace ThreepeatEditor
{
    [CreateAssetMenu(fileName = "MMCGameCreator2Integration", menuName = "Threepeat/GC2 Integration SO")]
    public class MMCGameCreator2Integration : MMCIntegrationBase
    {
        public bool startInMxMMode = false;

        public bool fireGCEvents_OnFootstep = false;
        public bool fireGCEvents_OnJump = false;
        public bool fireGCEvents_OnLand = false;

        public override string GetDescription()
        {
            return "Integrates MMLC with Game Creator 2";
        }

        public override bool IsMajorIntegration()
        {
            return true;
        }

        public override string GetIntegrationName()
        {
            return "Game Creator 2 Integration";
        }

        public override void MakeGUI()
        {
            EditorGUILayout.Space();
            EditorGUILayout.HelpBox("Game Creator Integration only works with 'Update Existing Character' selected, above in the Integration Type field:\n1. Create a Game Creator Character in your scene\n2. Drag the Character object (the object containing Game Creator Player or Character component) into the 'Scene Root Object' field.\n3. Drag the original model prefab for your character into the 'Model Prefab' field.", MessageType.Info);
            EditorGUILayout.Space();
            startInMxMMode = EditorGUILayout.BeginToggleGroup(
                    new GUIContent("Start in MMLC/MxM Motion-Matching mode", "Automatically switch to MxM on character initialization"), startInMxMMode);
            EditorGUILayout.EndToggleGroup();


            EditorGUILayout.Space();
        }

        // This is called whenever the integration enable checkbox is enabled (checked) by user.
        public override void OnIntegrationEnable()
        {
            //Debug.Log("MMCGC1Integration: OnEnable");
            MMCConfigWizardWindow wiz = MMCConfigWizardWindow.GetWindowInstance();

            wiz.SetIntegrationType(MMCConfigWizardWindow.CharacterIntegrationType.UpdateExistingCharacter);
            wiz.SetCharacterInputScheme(MMCConfigWizardWindow.CharacterInputScheme.InputManager);
            wiz.addAudioSourceToRootObject = false;
            wiz.addCinemachineToScene = false;
        }

        public override bool SetupIntegration(GameObject rootObject, GameObject modelObject, NGCharacter character, GameObject camObject = null)
        {
            if (!selected)
            {
                return true;
            }

            NGCharacterControllerWrapper controllerWrapper = character.controllerWrapper;
            if (controllerWrapper == null)
            {
                controllerWrapper = character.GetComponent<NGCharacterControllerWrapper>();
            }

            character.manuallyInitialize = true;

            controllerWrapper.gravityEnabled = true;
            //GC1 only controllerWrapper.jumpForceHandledExternally = true;
            MMCConfigWizardWindow wiz = MMCConfigWizardWindow.GetWindowInstance();

            controllerWrapper.ConfigureObject(controllerWrapper.ApplyGravityWhenGrounded, -1f, 0.25f, true, wiz.groundLayers);

            MxMAnimator mxma = modelObject.GetComponent<MxMAnimator>();
            FieldInfo fi = typeof(MxMAnimator).GetField("p_DIYPlayableGraph", BindingFlags.NonPublic | BindingFlags.Instance);
            fi.SetValue(mxma, true);
            //mxma
            controllerWrapper.jumpForceHandledExternally = false;

            MMCGameCreator2 mmcgc = rootObject.GetComponent<MMCGameCreator2>();
            if (mmcgc == null)
            {
                mmcgc = rootObject.AddComponent<MMCGameCreator2>();
            }

            mmcgc.StartInMxMMode = startInMxMMode;
            mmcgc.mxmAnimator = mxma;
            mmcgc.mmlcCharacter = character;
            Character gcChar = rootObject.GetComponent<Character>();
            mmcgc.gcCharacter = gcChar;
            
            if (gcChar.IK.HasRig<RigFeetPlant>())
            {
                RigFeetPlant feetPlant = gcChar.IK.GetRig<RigFeetPlant>();
                feetPlant.IsActive = false;
                Debug.Log("ConfigWizard: disabling GC2 Feet Plant IK Rig for MMLC integration");
            }
            //RigAlignGround
            //RigLean
            //

            return true;
        }

        public override string GetHelpLink()
        {
            return "https://threepeatgames.com/mmc_gc1";
        }


    }
}