using UnityEngine;
using UnityEditor;
using JU.CharacterSystem.AI;
namespace JUTPSEditor
{
    [CustomEditor(typeof(JU_AI_Zombie))]
    public class JU_AI_ZombieCharacterEditor : Editor
    {
        private JU_AI_Zombie _zombieAI;

        private SerializedProperty _moveEnabled;
        private SerializedProperty _navigationSettings;
        private SerializedProperty _head;
        private SerializedProperty _general;
        private SerializedProperty _aimSpeed;

        private SerializedProperty _fieldOfView;
        private SerializedProperty _damageDetector;
        private SerializedProperty _hear;

        private SerializedProperty _patrolPath;
        private SerializedProperty _patrolArea;

        private SerializedProperty _patrolRandomlyIfNotHavePath;
        private SerializedProperty _moveRandom;
        private SerializedProperty _followPatrolPath;
        private SerializedProperty _patrolInsideArea;
        private SerializedProperty _moveToLastTargetPosition;
        private SerializedProperty _searchLastTarget;
        private SerializedProperty _attack;

        private bool _showPatrolAI;
        private bool _showSensors;
        private bool _showPatrolAreas;
        private bool _showStates;

        void OnEnable()
        {
            _zombieAI = (JU_AI_Zombie)target;

            // Load states
            _showPatrolAI = EditorPrefs.GetBool($"{nameof(JU_AI_ZombieCharacterEditor)}.{nameof(_showPatrolAI)}");
            _showSensors = EditorPrefs.GetBool($"{nameof(JU_AI_ZombieCharacterEditor)}.{nameof(_showSensors)}");
            _showPatrolAreas = EditorPrefs.GetBool($"{nameof(JU_AI_ZombieCharacterEditor)}.{nameof(_showPatrolAreas)}");
            _showStates = EditorPrefs.GetBool($"{nameof(JU_AI_ZombieCharacterEditor)}.{nameof(_showStates)}");

            _moveEnabled = serializedObject.FindProperty(nameof(_zombieAI.MoveEnabled));
            _head = serializedObject.FindProperty(nameof(_zombieAI.Head));
            _navigationSettings = serializedObject.FindProperty(nameof(_zombieAI.NavigationSettings));
            _general = serializedObject.FindProperty(nameof(_zombieAI.General));
            _aimSpeed = serializedObject.FindProperty(nameof(_zombieAI.AimSpeed));

            _fieldOfView = serializedObject.FindProperty(nameof(_zombieAI.FieldOfView));
            _damageDetector = serializedObject.FindProperty(nameof(_zombieAI.DamageDetector));
            _hear = serializedObject.FindProperty(nameof(_zombieAI.Hear));

            _patrolPath = serializedObject.FindProperty(nameof(_zombieAI.PatrolPath));
            _patrolArea = serializedObject.FindProperty(nameof(_zombieAI.PatrolArea));

            _patrolRandomlyIfNotHavePath = serializedObject.FindProperty(nameof(_zombieAI.PatrolRandomlyIfNotHavePath));
            _moveRandom = serializedObject.FindProperty(nameof(_zombieAI.MoveRandom));
            _followPatrolPath = serializedObject.FindProperty(nameof(_zombieAI.FollowPatrolPath));
            _patrolInsideArea = serializedObject.FindProperty(nameof(_zombieAI.PatrolInsideArea));
            _moveToLastTargetPosition = serializedObject.FindProperty(nameof(_zombieAI.MoveToLastTargetPosition));
            _searchLastTarget = serializedObject.FindProperty(nameof(_zombieAI.SearchLastTarget));
            _attack = serializedObject.FindProperty(nameof(_zombieAI.Attack));
        }

        private void OnDisable()
        {
            // Save states
            EditorPrefs.SetBool($"{nameof(JU_AI_ZombieCharacterEditor)}.{nameof(_showPatrolAI)}", _showPatrolAI);
            EditorPrefs.SetBool($"{nameof(JU_AI_ZombieCharacterEditor)}.{nameof(_showSensors)}", _showSensors);
            EditorPrefs.SetBool($"{nameof(JU_AI_ZombieCharacterEditor)}.{nameof(_showPatrolAreas)}", _showPatrolAreas);
            EditorPrefs.SetBool($"{nameof(JU_AI_ZombieCharacterEditor)}.{nameof(_showStates)}", _showStates);
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            GUIStyle toolbarStyle = JUTPSEditor.CustomEditorStyles.Toolbar();

            JUTPSEditor.CustomEditorUtilities.JUTPSTitle("Zombie AI Behaviour");

            // Patrol AI
            _showPatrolAI = GUILayout.Toggle(_showPatrolAI, "Zombie AI", toolbarStyle);
            if (_showPatrolAI)
            {
                EditorGUILayout.PropertyField(_moveEnabled);
                EditorGUILayout.PropertyField(_navigationSettings, true);
                EditorGUILayout.PropertyField(_head);
                EditorGUILayout.PropertyField(_general, true);
                EditorGUILayout.PropertyField(_aimSpeed);
                EditorGUILayout.Space();
            }

            // Sensors
            _showSensors = GUILayout.Toggle(_showSensors, "Sensors", toolbarStyle);
            if (_showSensors)
            {
                EditorGUILayout.PropertyField(_fieldOfView, true);
                EditorGUILayout.PropertyField(_damageDetector, true);
                EditorGUILayout.PropertyField(_hear, true);
                EditorGUILayout.Space(20);
            }

            // Patrol Areas
            _showPatrolAreas = GUILayout.Toggle(_showPatrolAreas, "Patrol Areas", toolbarStyle);
            if (_showPatrolAreas)
            {
                EditorGUILayout.PropertyField(_patrolPath);
                EditorGUILayout.PropertyField(_patrolArea);
                EditorGUILayout.Space(20);
            }

            // States
            _showStates = GUILayout.Toggle(_showStates, "States", toolbarStyle);
            if (_showStates)
            {
                EditorGUILayout.PropertyField(_patrolRandomlyIfNotHavePath);
                EditorGUILayout.PropertyField(_moveRandom, true);
                EditorGUILayout.PropertyField(_followPatrolPath, true);
                EditorGUILayout.PropertyField(_patrolInsideArea, true);
                EditorGUILayout.PropertyField(_moveToLastTargetPosition, true);
                EditorGUILayout.PropertyField(_searchLastTarget, true);
                EditorGUILayout.PropertyField(_attack, true);
                EditorGUILayout.Space();
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}