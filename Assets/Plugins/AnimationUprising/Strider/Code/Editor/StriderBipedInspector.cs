using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using AnimationUprising.Strider;

namespace AnimationUprising.StriderEditor
{
    //============================================================================================
    /**
    *  @brief Custom inspector editor for StriderBiped component
    *         
    *********************************************************************************************/
    [CustomEditor(typeof(StriderBiped))]
    public class StriderBipedInspector : Editor
    {
        private StriderBiped m_striderBiped;
        private VisualElement m_rootElement;
        private VisualTreeAsset m_visualTree;

        private int m_desiredExecutionOrder = -50;
        
        private ObjectField m_animatorField;
        private ObjectField m_hipTransformField;
        private ObjectField m_leftThighTransformField;
        private ObjectField m_rightThighTransformField;
        private ObjectField m_leftLowerLegTransformField;
        private ObjectField m_rightLowerLegTransformField;
        private ObjectField m_leftFootTransformField;
        private ObjectField m_rightFootTransformField;
        private ObjectField m_leftFootIKTransformField;
        private ObjectField m_rightFootIKTransformField;

        private SerializedProperty m_spRootMotion;
        private SerializedProperty m_spWarpDirectionMode;
        private SerializedProperty m_spIKMethod;
        private SerializedProperty m_spAnimator;
        private SerializedProperty m_spHips;
        private SerializedProperty m_spLeftThigh;
        private SerializedProperty m_spRightThigh;
        private SerializedProperty m_spLeftLowerLeg;
        private SerializedProperty m_spRightLowerLeg;
        private SerializedProperty m_spLeftFoot;
        private SerializedProperty m_spRightFoot;
        private SerializedProperty m_spLeftFootIK;
        private SerializedProperty m_spRightFootIK;
        private SerializedProperty m_spOnSetupComplete;

        public void OnEnable()
        {
            if(!Application.isPlaying)
                SetExecutionOrder();

            m_striderBiped = (StriderBiped)target;

            FetchSerializedProperties();

            m_rootElement = new VisualElement();
            m_visualTree = Resources.Load<VisualTreeAsset>("StriderBipedInspectorLayout");

            StyleSheet styleSheet = Resources.Load<StyleSheet>("StriderBipedInspectorStyle");
            m_rootElement.styleSheets.Add(styleSheet);

            Animator animator = m_spAnimator.objectReferenceValue as Animator;

            if(animator == null)
            {
                animator = (target as StriderBiped).GetComponent<Animator>();
                m_spAnimator.objectReferenceValue = animator;
            }
        }

        public override VisualElement CreateInspectorGUI()
        {
            //Clear the root visual element
            m_rootElement.Clear();

            //Clone the base visual tree to the root element
            m_visualTree.CloneTree(m_rootElement);

            //Foldouts
            Foldout settingsFoldout = m_rootElement.Q(name: "settings", className: "foldout") as Foldout;
            settingsFoldout.text = "Settings";

            Foldout strideFoldout = m_rootElement.Q(name: "strideStyle", className: "foldout") as Foldout;
            strideFoldout.text = "Stride Style";

            Foldout playbackFoldout = m_rootElement.Q(name: "playbackSpeed", className: "foldout") as Foldout;
            playbackFoldout.text = "Playback Speed Control";

            Foldout referenceFoldout = m_rootElement.Q(name: "references", className: "foldout") as Foldout;
            referenceFoldout.text = "References";

            Foldout callbackFoldout = m_rootElement.Q(name: "callbacks", className: "foldout") as Foldout;
            callbackFoldout.text = "Callbacks";


            //Settings
            SerializedProperty spSpeedScale = serializedObject.FindProperty("p_speedScale");
            Slider speedScaleSlider = m_rootElement.Q(name: "speedScale", className: "slider") as Slider;
            speedScaleSlider.BindProperty(spSpeedScale);
            speedScaleSlider.RegisterCallback<ChangeEvent<float>>(e =>
            {
                (target as StriderBiped).SetSpeedScaleInstant(e.newValue);
            });
            FloatField speedScaleFloatField = m_rootElement.Q(name: "speedScale", className: "slider-input-field") as FloatField;
            speedScaleFloatField.RegisterCallback<ChangeEvent<float>>(e =>
            {
                (target as StriderBiped).SetSpeedScaleInstant(e.newValue);
            });
            speedScaleFloatField.BindProperty(spSpeedScale);

            Slider weightSlider = m_rootElement.Q(name: "weight", className: "slider") as Slider;
            FloatField weightFloatField = m_rootElement.Q(name: "weight", className: "slider-input-field") as FloatField;
            SerializedProperty spWeight = serializedObject.FindProperty("p_weight");
            weightSlider.BindProperty(spWeight);
            weightFloatField.BindProperty(spWeight);

            FloatField smoothRateFloatField = m_rootElement.Q(name: "smoothRate", className: "input-field") as FloatField;
            smoothRateFloatField.BindProperty(serializedObject.FindProperty("p_smoothRate"));

            FloatField warpSmoothRateFloatField = m_rootElement.Q(name: "strideSmoothRate", className: "input-field") as FloatField;
            warpSmoothRateFloatField.BindProperty(serializedObject.FindProperty("p_warpDirSmoothRate"));


            FloatField sizeCompFloatField = m_rootElement.Q(name: "sizeCompensation", className: "input-field") as FloatField;
            sizeCompFloatField.BindProperty(serializedObject.FindProperty("p_sizeCompensation"));

            VisualElement rootMotionEnumField = m_rootElement.Q(name: "rootMotion", className: "enum-popup");

            IMGUIContainer enumPopupIMGUI = new IMGUIContainer(() =>
            {
                m_spRootMotion.intValue = (int)(ERootMotionMode)EditorGUILayout.EnumPopup((ERootMotionMode)m_spRootMotion.intValue);
                m_spWarpDirectionMode.intValue = (int)(EWarpDirectionMode)EditorGUILayout.EnumPopup((EWarpDirectionMode)m_spWarpDirectionMode.intValue);

                EditorGUI.BeginChangeCheck();
                m_spIKMethod.intValue = (int)(EWarpDirectionMode)EditorGUILayout.EnumPopup((EIKMethod)m_spIKMethod.intValue);
                if(EditorGUI.EndChangeCheck())
                {
                    IKMethodSet();
                    if(Application.isPlaying)
                    {
                        (target as StriderBiped).SetupIKMethod((EIKMethod)m_spIKMethod.intValue);
                        (target as StriderBiped).OnSetupComplete.Invoke();
                    }
                }

                if(serializedObject != null)
                    serializedObject.ApplyModifiedProperties();
            });

            rootMotionEnumField.focusable = false;
            rootMotionEnumField.hierarchy.Add(enumPopupIMGUI);

            Toggle manualRootMotionToggle = m_rootElement.Q(name: "manualRootMotion", className: "toggle-field") as Toggle;
            manualRootMotionToggle.BindProperty(serializedObject.FindProperty("p_manualRootMotionScaleFix"));

            //Options
            FloatField maxSpeedFloatField = m_rootElement.Q(name: "maxSpeed", className: "input-field") as FloatField;
            maxSpeedFloatField.BindProperty(serializedObject.FindProperty("p_maxSpeed"));

            FloatField baseOffsetFloatField = m_rootElement.Q(name: "baseOffset", className: "input-field") as FloatField;
            baseOffsetFloatField.BindProperty(serializedObject.FindProperty("p_baseOffset"));

            FloatField dynamicOffsetFloatField = m_rootElement.Q(name: "dynamicOffset", className: "input-field") as FloatField;
            dynamicOffsetFloatField.BindProperty(serializedObject.FindProperty("p_dynamicOffset"));

            CurveField dynamicOffsetCurveField = m_rootElement.Q(name: "dynamicOffsetCurve") as CurveField;
            dynamicOffsetCurveField.BindProperty(serializedObject.FindProperty("p_dynamicOffsetCurve"));

            Slider hipAdjustCutoffSlider = m_rootElement.Q(name: "hipAdjustCutoff", className: "slider") as Slider;
            FloatField hipAdjustCutoffFloatField = m_rootElement.Q(name: "hipAdjustCutoff", className: "slider-input-field") as FloatField;
            SerializedProperty spHipAdjustCutoff = serializedObject.FindProperty("p_hipAdjustCutoff");
            hipAdjustCutoffSlider.BindProperty(spHipAdjustCutoff);
            hipAdjustCutoffFloatField.BindProperty(spHipAdjustCutoff);

            Slider hipDampingSlider = m_rootElement.Q(name: "hipDamping", className: "slider") as Slider;
            FloatField hipDampingFloatField = m_rootElement.Q(name: "hipDamping", className: "slider-input-field") as FloatField;
            CurveField hipDampingCurve = m_rootElement.Q(name: "hipDampingCurve") as CurveField;
            SerializedProperty spHipDamping = serializedObject.FindProperty("p_hipDamping");
            hipDampingSlider.BindProperty(spHipDamping);
            hipDampingFloatField.BindProperty(spHipDamping);
            hipDampingCurve.BindProperty(serializedObject.FindProperty("p_curHipDampingCurve"));

            //Playback Speed
            FloatField independentPlaybackFloatField = m_rootElement.Q(name: "independentPlayback", className: "slider-input-field") as FloatField;
            Slider independentPlaybackSlider = m_rootElement.Q(name: "independentPlayback", className: "slider-range") as Slider;
            SerializedProperty spindependentPlaybackSpeed = serializedObject.FindProperty("p_independentPlaybackSpeed");
            independentPlaybackSlider.BindProperty(spindependentPlaybackSpeed);
            independentPlaybackFloatField.BindProperty(spindependentPlaybackSpeed);

            FloatField minPlaybackFloatField = m_rootElement.Q(name: "minPlayback", className: "slider-input-field") as FloatField;
            MinMaxSlider minMaxPlaybackSlider = m_rootElement.Q(name: "minMaxPlayback", className: "slider-range") as MinMaxSlider;
            FloatField maxPlaybackFloatField = m_rootElement.Q(name: "maxPlayback", className: "slider-input-field") as FloatField;
            SerializedProperty spMinMaxPlaybackSpeed = serializedObject.FindProperty("p_minMaxPlaybackSpeed");  
            minMaxPlaybackSlider.BindProperty(spMinMaxPlaybackSpeed);
            minPlaybackFloatField.BindProperty(spMinMaxPlaybackSpeed.FindPropertyRelative("x"));
            maxPlaybackFloatField.BindProperty(spMinMaxPlaybackSpeed.FindPropertyRelative("y"));
            

            Slider playbackWeightSlider = m_rootElement.Q(name: "playbackWeight", className: "slider") as Slider;
            FloatField playbackWeightFloatField = m_rootElement.Q(name: "playbackWeight", className: "slider-input-field") as FloatField;
            SerializedProperty spPlaybackSpeedWeight = serializedObject.FindProperty("p_playbackSpeedWeight");
            playbackWeightSlider.BindProperty(spPlaybackSpeedWeight);
            playbackWeightFloatField.BindProperty(spPlaybackSpeedWeight);

            //References
            m_animatorField = m_rootElement.Q(name: "Animator") as ObjectField;
            m_animatorField.RegisterCallback<ChangeEvent<Animator>>(e =>
            {
                FindAnimatorReferences(e.newValue);
            });
            m_animatorField.objectType = typeof(Animator);
            m_animatorField.BindProperty(serializedObject.FindProperty("p_animator"));

            m_hipTransformField = m_rootElement.Q(name: "HipTF") as ObjectField;
            m_hipTransformField.objectType = typeof(Transform);
            m_hipTransformField.BindProperty(serializedObject.FindProperty("p_hips"));

            m_leftThighTransformField = m_rootElement.Q(name: "LeftThighTF") as ObjectField;
            m_leftThighTransformField.objectType = typeof(Transform);
            m_leftThighTransformField.BindProperty(serializedObject.FindProperty("p_leftThigh"));

            m_rightThighTransformField = m_rootElement.Q(name: "RightThighTF") as ObjectField;
            m_rightThighTransformField.objectType = typeof(Transform);
            m_rightThighTransformField.BindProperty(serializedObject.FindProperty("p_rightThigh"));

            m_leftLowerLegTransformField = m_rootElement.Q(name: "LeftLowerLegTF") as ObjectField;
            m_leftLowerLegTransformField.objectType = typeof(Transform);
            m_leftLowerLegTransformField.BindProperty(serializedObject.FindProperty("p_leftLowerLeg"));

            m_rightLowerLegTransformField = m_rootElement.Q(name: "RightLowerLegTF") as ObjectField;
            m_rightLowerLegTransformField.objectType = typeof(Transform);
            m_rightLowerLegTransformField.BindProperty(serializedObject.FindProperty("p_rightLowerLeg"));

            m_leftFootTransformField = m_rootElement.Q(name: "LeftFootTF") as ObjectField;
            m_leftFootTransformField.objectType = typeof(Transform);
            m_leftFootTransformField.BindProperty(serializedObject.FindProperty("p_leftFoot"));

            m_rightFootTransformField = m_rootElement.Q(name: "RightFootTF") as ObjectField;
            m_rightFootTransformField.objectType = typeof(Transform);
            m_rightFootTransformField.BindProperty(serializedObject.FindProperty("p_rightFoot"));

            m_leftFootIKTransformField = m_rootElement.Q(name: "LeftFootIKTF") as ObjectField;
            m_leftFootIKTransformField.objectType = typeof(Transform);
            m_leftFootIKTransformField.BindProperty(serializedObject.FindProperty("p_leftFootIK"));

            m_rightFootIKTransformField = m_rootElement.Q(name: "RightFootIKTF") as ObjectField;
            m_rightFootIKTransformField.objectType = typeof(Transform);
            m_rightFootIKTransformField.BindProperty(serializedObject.FindProperty("p_rightFootIK"));


            //Callbacks
            UQueryBuilder<VisualElement> builder = m_rootElement.Query(name: "CallbackContainer");
            builder.ForEach(AddCallbacksIMGUI);

            FindAnimatorReferences(m_spAnimator.objectReferenceValue as Animator);
            IKMethodSet();

            return m_rootElement;
        }

        private void AddCallbacksIMGUI(VisualElement imguiContainer)
        {
            IMGUIContainer callbacks = new IMGUIContainer(() =>
            {
                EditorGUILayout.PropertyField(m_spOnSetupComplete);

                if(serializedObject != null)
                    serializedObject.ApplyModifiedProperties();
            });

            imguiContainer.focusable = false;
            imguiContainer.hierarchy.Add(callbacks);
        }

        private void SetExecutionOrder()
        {
            MonoScript monoScript = MonoScript.FromMonoBehaviour(target as StriderBiped);
            int curExecutionOrder = MonoImporter.GetExecutionOrder(monoScript);

            if (curExecutionOrder != m_desiredExecutionOrder)
                MonoImporter.SetExecutionOrder(monoScript, m_desiredExecutionOrder);
        }

        private void FetchSerializedProperties()
        {
            m_spRootMotion = serializedObject.FindProperty("p_rootMotion");
            m_spWarpDirectionMode = serializedObject.FindProperty("p_warpDirectionMode");
            m_spIKMethod = serializedObject.FindProperty("p_IKMethod");
            m_spAnimator = serializedObject.FindProperty("p_animator");
            m_spHips = serializedObject.FindProperty("p_hips");
            m_spLeftThigh = serializedObject.FindProperty("p_leftThigh");
            m_spRightThigh = serializedObject.FindProperty("p_rightThigh");
            m_spLeftLowerLeg = serializedObject.FindProperty("p_leftLowerLeg");
            m_spRightLowerLeg = serializedObject.FindProperty("p_rightLowerLeg");
            m_spLeftFoot = serializedObject.FindProperty("p_leftFoot");
            m_spRightFoot = serializedObject.FindProperty("p_rightFoot");
            m_spLeftFootIK = serializedObject.FindProperty("p_leftFootIK");
            m_spRightFootIK = serializedObject.FindProperty("p_rightFootIK");

            m_spOnSetupComplete = serializedObject.FindProperty("OnSetupComplete");
        }

        private void FindAnimatorReferences(Animator a_animator)
        {
            if (a_animator == null)
                return;

            if(a_animator.isHuman)
            {
                m_spHips.objectReferenceValue = a_animator.GetBoneTransform(HumanBodyBones.Hips);
                m_spLeftThigh.objectReferenceValue = a_animator.GetBoneTransform(HumanBodyBones.LeftUpperLeg);
                m_spLeftLowerLeg.objectReferenceValue = a_animator.GetBoneTransform(HumanBodyBones.LeftLowerLeg);
                m_spLeftFoot.objectReferenceValue = a_animator.GetBoneTransform(HumanBodyBones.LeftFoot);
                m_spRightThigh.objectReferenceValue = a_animator.GetBoneTransform(HumanBodyBones.RightUpperLeg);
                m_spRightLowerLeg.objectReferenceValue = a_animator.GetBoneTransform(HumanBodyBones.RightLowerLeg);
                m_spRightFoot.objectReferenceValue = a_animator.GetBoneTransform(HumanBodyBones.RightFoot);

                m_leftThighTransformField.SetEnabled(false);
                m_leftLowerLegTransformField.SetEnabled(false);
                m_leftFootTransformField.SetEnabled(false);
                m_rightThighTransformField.SetEnabled(false);
                m_rightLowerLegTransformField.SetEnabled(false);
                m_rightFootTransformField.SetEnabled(false);
                m_hipTransformField.SetEnabled(false);
            }
            else
            {
                m_leftThighTransformField.SetEnabled(true);
                m_leftLowerLegTransformField.SetEnabled(true);
                m_leftFootTransformField.SetEnabled(true);
                m_rightThighTransformField.SetEnabled(true);
                m_rightLowerLegTransformField.SetEnabled(true);
                m_rightFootTransformField.SetEnabled(true);
                m_hipTransformField.SetEnabled(true);
            }

            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }

        private void IKMethodSet()
        {
            if((EIKMethod)m_spIKMethod.intValue == EIKMethod.AnimationRigging)
            {
                m_leftFootIKTransformField.SetEnabled(true);
                m_rightFootIKTransformField.SetEnabled(true);
            }
            else
            {
                m_leftFootIKTransformField.SetEnabled(false);
                m_rightFootIKTransformField.SetEnabled(false);
            }
        }

    }//End of class: StriderBipedInspector
}//End of namespace: AnimationUprising.StriderEditor
