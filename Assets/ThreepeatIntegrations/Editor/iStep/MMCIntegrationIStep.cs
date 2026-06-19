#if HOAXGAMES_ISTEP
using System.Collections;
using System.Collections.Generic;
using Threepeat;
using UnityEngine;
using HoaxGames;
using System.Reflection;
using UnityEditorInternal;
using UnityEditor;

namespace ThreepeatEditor
{
    //[CreateAssetMenu(fileName = "MMCiStepIntegration", menuName = "Threepeat/iStep Integration SO")]
    public class MMCIntegrationIStep : MMCIntegrationBase
    {
        protected LayerMask groundLayers = new LayerMask();

        protected int ikLayerIndex = 0;

        public override string GetDescription()
        {
            return "Integrates iStep foot placement system with MMLC";
        }

        public override string GetIntegrationName()
        {
            return "iStep (foot placement system)";
        }

        public override void MakeGUI()
        {
            EditorGUILayout.Space();

            LayerMask tempMask = EditorGUILayout.MaskField(new GUIContent("iStep Ground Layers", "Which layers iStep should treat as ground."), InternalEditorUtility.LayerMaskToConcatenatedLayersMask(groundLayers), InternalEditorUtility.layers);
            groundLayers = InternalEditorUtility.ConcatenatedLayersMaskToLayerMask(tempMask);

            EditorGUILayout.Space();
            EditorGUILayout.HelpBox("iStep can only work on an AnimatorController layer with IK Pass enabled.\n1. Ensure IK Pass is enabled on whichever layer makes most sense in your AnimatorController.\n2. Put the layer index (First layer in layer list is index 0) for that layer below.", MessageType.Info);
            EditorGUILayout.Space();
            ikLayerIndex = EditorGUILayout.IntField(new GUIContent("iStep IK Pass layer"), ikLayerIndex);
            EditorGUILayout.Space();
        }
        public override bool IsMajorIntegration()
        {
            return true;
        }

        public override void OnIntegrationEnable()
        {
            //Debug.Log("MMCGC1Integration: OnEnable");
            MMCConfigWizardWindow wiz = MMCConfigWizardWindow.GetWindowInstance();
            groundLayers = wiz.groundLayers;
        }

        public override bool SetupIntegration(GameObject rootObject, GameObject modelObject, NGCharacter character, GameObject camObject)
        {
            if (!selected)
            {
                return true;
            }

            FootIK fik = modelObject.GetComponent<FootIK>();
            if (fik == null)
            {
                fik = modelObject.AddComponent<FootIK>();
            }
            
            FieldInfo fi = typeof(FootIK).GetField("m_collisionLayerMask", BindingFlags.NonPublic | BindingFlags.Instance);
            fi.SetValue(fik, groundLayers);

            FieldInfo fi_ik = typeof(FootIK).GetField("m_animatorIkPassLayerIndex", BindingFlags.NonPublic | BindingFlags.Instance);
            fi_ik.SetValue(fik, ikLayerIndex);

            FieldInfo fi2 = typeof(FootIK).GetField("m_ikFootHeight", BindingFlags.NonPublic | BindingFlags.Instance);
            fi2.SetValue(fik, 0.07f);

            fik.setIKUpwardsExtrapolation(3.63f);
            fik.ikUpwardsPullBackDownExtrapolation = 3.6f;
            fik.setIKDownwardsExtrapolation(3.78f);

            fik.setSlopeBendingUpwardsStrength(15.6f);
            fik.setSlopeBendingDownwardsStrength(16.2f);

            MMCFootPlacementWrapper_iStep fpw = modelObject.GetComponent<MMCFootPlacementWrapper_iStep>();

            if (fpw == null)
            {
                fpw = modelObject.AddComponent<MMCFootPlacementWrapper_iStep>();
                fpw.footIK = fik;
            }

            return true;
        }

    }
}
#else
using System.Collections;
using System.Collections.Generic;
using Threepeat;
using UnityEngine;
using UnityEditor;

namespace ThreepeatEditor
{
    public class MMCIntegrationIStep : MMCIntegrationBase
    {
        public override string GetDescription()
        {
            return "Integrates iStep foot placement system with MMLC";
        }

        public override string GetIntegrationName()
        {
            return "iStep (foot placement system)";
        }

        public override void MakeGUI()
        {
            EditorGUILayout.HelpBox("iStep is not installed. Install iStep and add HOAXGAMES_ISTEP define to enable this integration.", MessageType.Info);
            selected = false;
        }

        public override bool IsMajorIntegration()
        {
            return true;
        }

        public override bool SetupIntegration(GameObject rootObject, GameObject modelObject, NGCharacter character, GameObject camObject)
        {
            return true;
        }
    }
}
#endif