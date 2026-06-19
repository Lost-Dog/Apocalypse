using System.IO;
using System.Reflection;
using GameCreator.Editor.Common;
using GameCreator.Editor.Core;
using GameCreator.Runtime.Transitions;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace GameCreator.Editor.Transitions
{
    [CustomEditor(typeof(Transition))]
    public class TransitionEditor : SkinEditor
    {
        private const BindingFlags MEMBER_FLAGS = BindingFlags.Instance | BindingFlags.NonPublic;
        
        private const string ASSETS = "Assets/";
        
        private const string CONTROLLER_PATH = 
            Runtime.Common.RuntimePaths.PACKAGES + 
            "Transitions/Runtime/Animators/Transition.overrideController";

        private const string PROP_CONTROLLER = "m_Controller";
         
        private const string PROP_OPEN = "m_Open";
        private const string PROP_READY = "m_Ready";
        private const string PROP_LOADING = "m_Loading";
        private const string PROP_CLOSE = "m_Close";

        // PAINT METHOD: --------------------------------------------------------------------------
        
        public override VisualElement CreateInspectorGUI()
        {
            VisualElement root = base.CreateInspectorGUI();

            SerializedProperty open = this.serializedObject.FindProperty(PROP_OPEN);
            SerializedProperty ready = this.serializedObject.FindProperty(PROP_READY);
            SerializedProperty loading = this.serializedObject.FindProperty(PROP_LOADING);
            SerializedProperty close = this.serializedObject.FindProperty(PROP_CLOSE);
            
            root.Add(new SpaceSmall());
            root.Add(new LabelTitle("Animations"));
            root.Add(new PropertyField(open));
            root.Add(new PropertyField(loading));
            root.Add(new PropertyField(ready));
            root.Add(new PropertyField(close));

            return root;
        }
        
        // CREATE ASSET METHODS: ------------------------------------------------------------------
        
        [MenuItem("Assets/Create/Game Creator/Developer/Transition", false, 0)]
        internal static void CreateFromMenuItem()
        {
            Transition transition = CreateInstance<Transition>();

            string selection = Selection.activeObject != null
                ? AssetDatabase.GetAssetPath(Selection.activeObject)
                : ASSETS;

            string directory = File.Exists(Runtime.Common.PathUtils.PathForOS(selection)) 
                ? Runtime.Common.PathUtils.PathToUnix(Path.GetDirectoryName(selection)) 
                : selection;

            string path = AssetDatabase.GenerateUniqueAssetPath(
                Runtime.Common.PathUtils.Combine(directory ?? ASSETS, "Transition.asset")
            );
            
            AssetDatabase.CreateAsset(transition, path);
            AssetDatabase.SaveAssets();
            
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = transition;

            AnimatorOverrideController controller = Instantiate(
                AssetDatabase.LoadAssetAtPath<AnimatorOverrideController>(CONTROLLER_PATH)
            );

            controller.name = Path.GetFileNameWithoutExtension(CONTROLLER_PATH); 
            controller.hideFlags = HideFlags.HideInHierarchy;
            
            AssetDatabase.AddObjectToAsset(controller, transition);
            typeof(Transition)
                .GetField(PROP_CONTROLLER, MEMBER_FLAGS)?
                .SetValue(transition, controller);

            AssetDatabase.SaveAssets();
            AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(transition));
            
            transition.name = "Transition";
        }
    }
}