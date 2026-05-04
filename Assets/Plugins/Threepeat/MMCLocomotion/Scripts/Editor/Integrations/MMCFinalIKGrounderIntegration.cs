#if FINAL_IK
using RootMotion;
using RootMotion.FinalIK;
using System;
using System.Collections;
using System.Collections.Generic;
using Threepeat;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace ThreepeatEditor
{
    [System.Serializable]
    public class MMCFinalIKGrounderIntegration : MMCLegacyIntegrationBase
    {
        public bool doSetupFinalIK = false;
        public bool doSetupGrounder = false;

        public LayerMask grounderGroundLayers = new LayerMask();

        private Texture2D layerCollisionTex = null;

        bool showHelp_GrounderLayers = false;
        bool showHelp_PhysicsCollisionLayers = false;

        public override bool IsPlaceholder()
        {
            return false;
        }

        public override void MakeGUI()
        {
            if (doSetupGrounder)
            {
                doSetupFinalIK = true;
            }
            doSetupFinalIK = EditorGUILayout.BeginToggleGroup("Setup Final IK FBBIK On Character (Main Biped IK Component)", doSetupFinalIK);
            EditorGUILayout.EndToggleGroup();
            if (!doSetupFinalIK)
            {
                doSetupGrounder = false;
            }
            EditorGUILayout.Space();
            doSetupGrounder = EditorGUILayout.BeginToggleGroup(new GUIContent("Setup Final IK Grounder on Character", "Grounder is Final IK's foot placement solution.  If you're using a different solution (e.g. iStep), keep this disabled."), doSetupGrounder);

            EditorGUILayout.Space();

            showHelp_GrounderLayers = ThreepeatEditorGUIUtilities.LabelWithHelp.LabelWithHelpField(
                    "1. Setup Grounder-only and Grounder-Ignore Layers",
                    "1. Create a GrounderIgnore layer to be used for the actual smooth slope objects for the character collider to use.\n" +
                    "2. Create a GrounderOnly layer to be used only by Grounder in order to place the characters' feet on the visible ground surface (e.g. stairs)",
                    showHelp_GrounderLayers);

            showHelp_PhysicsCollisionLayers = ThreepeatEditorGUIUtilities.LabelWithHelp.LabelWithHelpField(
                    "2. Setup Physics Collision Layer Matrix",
                    "",
                    showHelp_PhysicsCollisionLayers,
                    DrawHelpBox_PhysicsCollisionLayers);

            EditorGUILayout.Space();

            LayerMask tempMask = EditorGUILayout.MaskField(new GUIContent("Grounder Ground Layers", "Which layers should be treated as ground by Grounder."), InternalEditorUtility.LayerMaskToConcatenatedLayersMask(grounderGroundLayers), InternalEditorUtility.layers);
            grounderGroundLayers = InternalEditorUtility.ConcatenatedLayersMaskToLayerMask(tempMask);

            EditorGUILayout.EndToggleGroup();
        }

        private void DrawHelpBox_PhysicsCollisionLayers()
        {
            if (layerCollisionTex == null)
            {
                layerCollisionTex = (Texture2D)AssetDatabase.LoadAssetAtPath("Assets/Plugins/Threepeat/MMCLocomotion/Scripts/Integrations/Editor/physicsLayerCollisions.png", typeof(Texture2D));
            }

            if (layerCollisionTex != null)
            {
                EditorGUILayout.Space();
                EditorGUILayout.BeginHorizontal("box");
                GUILayout.FlexibleSpace();
                GUILayout.Label(layerCollisionTex);
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.Space();
            }
        }

        public override bool SetupCharacter(GameObject coreObject, GameObject modelObject)
        {
            if (!doSetupGrounder && !doSetupFinalIK)
            {
                return true;
            }

            FullBodyBipedIK fbbik = modelObject.GetComponent<FullBodyBipedIK>();
            if (fbbik == null)
            {
                fbbik = modelObject.AddComponent<FullBodyBipedIK>();
            }

            if (doSetupGrounder)
            {
                GrounderFBBIK gik = modelObject.GetComponent<GrounderFBBIK>();
                if (gik == null)
                {
                    gik = modelObject.AddComponent<GrounderFBBIK>();
                }
                fbbik.fixTransforms = true;
                gik.solver.layers = grounderGroundLayers;
                gik.solver.maxStep = 0.5f;
                gik.solver.heightOffset = 0.01f;
                gik.solver.footSpeed = 2.5f;
                gik.solver.footRadius = 0.15f;
                gik.solver.prediction = 0.05f;
                gik.solver.footRotationWeight = 1;
                gik.solver.footRotationSpeed = 7;
                gik.solver.maxFootRotationAngle = 20;
                gik.solver.rotateSolver = false;
                gik.solver.pelvisSpeed = 5;
                gik.solver.pelvisDamper = 0;
                gik.solver.lowerPelvisWeight = 1;
                gik.solver.liftPelvisWeight = 0;
                gik.solver.rootSphereCastRadius = 0.1f;
                gik.solver.overstepFallsDown = true;
                gik.solver.quality = Grounding.Quality.Best;
                gik.spineBend = 2;
                gik.spineSpeed = 3;
                GrounderFBBIK.SpineEffector[] effectors =
                {
                    new GrounderFBBIK.SpineEffector(FullBodyBipedEffector.LeftShoulder, 1.95f, 0f),
                    new GrounderFBBIK.SpineEffector(FullBodyBipedEffector.RightShoulder, 1.95f, 0f),
                    new GrounderFBBIK.SpineEffector(FullBodyBipedEffector.LeftHand, 1.95f, 0f),
                    new GrounderFBBIK.SpineEffector(FullBodyBipedEffector.RightHand, 1.95f, 0f)
                };
                gik.spine = effectors;
                gik.ik = fbbik;

                MMCFootPlacementWrapper_FinalIK fpw = modelObject.GetComponent<MMCFootPlacementWrapper_FinalIK>();
                if (fpw == null)
                {
                    fpw = modelObject.AddComponent<MMCFootPlacementWrapper_FinalIK>();
                    fpw.gik = gik;
                }
            }

            BipedReferences.AutoDetectReferences(ref fbbik.references, modelObject.transform, BipedReferences.AutoDetectParams.Default);
            fbbik.SetReferences(fbbik.references, null);
            return true;
        }
    }
}

#else

using UnityEngine;

namespace ThreepeatEditor
{
    [System.Serializable]
    public class MMCFinalIKGrounderIntegration : MMCLegacyIntegrationBase
    {
        public override bool IsPlaceholder() => true;

        public override void MakeGUI()
        {
            // FinalIK not installed — no GUI to show.
        }

        public override bool SetupCharacter(GameObject coreObject, GameObject modelObject) => true;
    }
}
#endif
