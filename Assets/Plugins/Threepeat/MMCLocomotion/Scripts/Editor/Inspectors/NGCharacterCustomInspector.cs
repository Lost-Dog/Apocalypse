using Threepeat;
using UnityEditor;
using UnityEngine;


namespace ThreepeatEditor
{
    [CustomEditor(typeof(NGCharacter))]
    public class NGCharacterCustomInspector : Editor
    {
        public bool showRuntimeHelpers = false;

        public override void OnInspectorGUI()
        {
            if (Application.isPlaying)
            {
                NGCharacter character = (NGCharacter)target;
                showRuntimeHelpers = EditorGUILayout.BeginFoldoutHeaderGroup(showRuntimeHelpers, "Runtime Helpers");
                if (showRuntimeHelpers)
                {
                    /*NGInputScheme_SplineCinematic iSpline = character.InputScheme as NGInputScheme_SplineCinematic;
                    if (iSpline != null)
                    {
                    }
                    else
                    {*/
                        NGInputScheme_NavMesh iNavMesh = character.InputScheme as NGInputScheme_NavMesh;
                        if (iNavMesh != null)
                        {
                            Transform tempTransform = null;
                            tempTransform = (Transform)EditorGUILayout.ObjectField("NavMesh wantedTarget", null, typeof(Transform), true);
                            if (tempTransform != null) {
                                Debug.Log("Setting MMLC NavMesh target transform");
                                iNavMesh.wantedTargetTransform = tempTransform;
                            }

                        }
                    //}
                }
                EditorGUILayout.EndFoldoutHeaderGroup();
            }

            //Called whenever the inspector is drawn for this object.
            DrawDefaultInspector();
            //This draws the default screen.  You don't need this if you want
            //to start from scratch, but I use this when I'm just adding a button or
            //some small addition and don't feel like recreating the whole inspector.
            if (GUILayout.Button("Activate Input Scheme"))
            {
                //Debug.LogFormat("Target name: {0}", target.GetType().Name);
                Debug.Log("Activating Input scheme");
                NGCharacter character = (NGCharacter)target;
                character.SetInputScheme(Instantiate(character.InputScheme));
                character.InputScheme.Initialize(character, character.mxmTrajectoryGenerator);


            }

            EditorGUILayout.LabelField("Drop MMCAnimationEventLayers here", EditorStyles.boldLabel);
            Rect dropArea = GUILayoutUtility.GetRect(0f, 50f, GUILayout.ExpandWidth(true));
            GUI.Box(dropArea, "Drag and drop MMCAnimationEventLayer SO's here");

            Event currentEvent = Event.current;
            if (currentEvent.type == EventType.DragUpdated)
            {
                if (dropArea.Contains(currentEvent.mousePosition))
                {
                    DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                    currentEvent.Use();
                }
            }
            else if (currentEvent.type == EventType.DragPerform)
            {
                if (dropArea.Contains(currentEvent.mousePosition))
                {
                    DragAndDrop.AcceptDrag();

                    foreach (Object draggedObject in DragAndDrop.objectReferences)
                    {
                        MMCAnimationEventLayer animLayer = draggedObject as MMCAnimationEventLayer;
                        if (animLayer != null)
                        {
                            // add it!
                            NGCharacter character = (NGCharacter)target;
                            Debug.LogFormat("Adding {0}", animLayer.BlendSpaceName);
                            animLayer.AddToCharacter(character);
                        }
                    }


                    currentEvent.Use();
                }
            }

            /*NGCharacter character2 = (NGCharacter)target;
            NGCharacterControllerWrapper wrapper = character2.GetComponent<NGCharacterControllerWrapper>();
            LayerMask mask = wrapper.GroundLayers;
            LayerMask mask2 = ~mask;
            EditorGUILayout.MaskField("Mask2", InternalEditorUtility.LayerMaskToConcatenatedLayersMask(~mask), InternalEditorUtility.layers);*/
        }
    }
}