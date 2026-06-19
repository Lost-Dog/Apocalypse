using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Threepeat;

namespace ThreepeatEditor
{
    [CustomEditor(typeof(NGWarpableClip))]
    public class NGWarpableClipEditor : Editor
    {
        NGWarpableClip wc;

        Editor clipEditor;

        float currentAnimationTime = 0f;
        public GameObject sceneObject;

        Vector2 clipPlayRegionNormalized = new Vector2(0.2f, 0.4f);
        Vector2 clipWarpToPlatformNormalized = new Vector2(0.2f, 0.4f);

        Vector2 thingRange = new Vector2(0f, 1f);

        bool inAnimMode = false;

        private void OnEnable()
        {
            wc = (NGWarpableClip)target;
        }

        public float SliderWithChange(string label, ref float minVal, ref float maxVal)
        {
            Vector2 oldThingVal = new Vector2(minVal, maxVal);
            EditorGUILayout.MinMaxSlider(label, ref minVal, ref maxVal, 0f, 1f);
            if (Mathf.Abs(oldThingVal.x - minVal) > 0.001f)
            {
                return minVal;
            }
            else if (Mathf.Abs(oldThingVal.y - maxVal) > 0.001f)
            {
                return maxVal;
            }

            return -1f;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            wc.clip = (AnimationClip)EditorGUILayout.ObjectField("Animation Clip", wc.clip, typeof(AnimationClip), false);
            sceneObject = (GameObject)EditorGUILayout.ObjectField("Scene Object to Animate", sceneObject, typeof(GameObject), true);
            /*
            if ((clipEditor == null) && (wc.clip != null))
            {
                clipEditor = UnityEditor.Editor.CreateEditor(wc.clip);
                Debug.LogFormat("Creating clip editor {0}!", clipEditor.GetType());

                clipEditor.HasPreviewGUI();
            }
            else if (wc.clip == null)
            {
                clipEditor = null;
            }

            if (clipEditor != null)
            {
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                clipEditor.OnPreviewSettings();
                GUILayout.EndHorizontal();
                clipEditor.OnInteractivePreviewGUI(GUILayoutUtility.GetRect(256, 256), EditorStyles.whiteLabel);
            }*/

            float lastAnimTime = currentAnimationTime;

            Vector2 oldThingVal = new Vector2(clipPlayRegionNormalized.x, clipPlayRegionNormalized.y);
            float retval;
            EditorGUILayout.Space();
            retval = SliderWithChange("Region to play", ref clipPlayRegionNormalized.x, ref clipPlayRegionNormalized.y);
            if (retval >= 0f)
            {
                currentAnimationTime = retval * wc.clip.length;
            }
            EditorGUILayout.Space();
            /*
            EditorGUILayout.MinMaxSlider("Region to play", ref clipPlayRegionNormalized.x, ref clipPlayRegionNormalized.y, thingRange.x, thingRange.y);
            if (Mathf.Abs(oldThingVal.x - clipPlayRegionNormalized.x) > 0.001f)
            {
                currentAnimationTime = clipPlayRegionNormalized.x * wc.clip.length;
            }
            else if (Mathf.Abs(oldThingVal.y - clipPlayRegionNormalized.y) > 0.001f)
            {
                currentAnimationTime = clipPlayRegionNormalized.y * wc.clip.length;
            }*/

            EditorGUILayout.Space();
            retval = SliderWithChange("Warp to Platform", ref clipWarpToPlatformNormalized.x, ref clipWarpToPlatformNormalized.y);
            if (retval >= 0f)
            {
                currentAnimationTime = retval * wc.clip.length;
            }
            EditorGUILayout.Space();


            //EditorGUI.BeginChangeCheck();
            currentAnimationTime = EditorGUILayout.Slider(currentAnimationTime, 0f, (wc != null) ? wc.clip.length : 1.0f);

            /*if (lastAnimTime != currentAnimationTime) 
            {
                Debug.Log("Time change!");
            }*/

            /*if (EditorGUI.EndChangeCheck())
            {
                AnimationMode.StartAnimationMode();
            }*/


            /*if (GUILayout.Button("Get current time"))
            {
                if (clipEditor != null)
                {
                    //AnimationClipEditor ace = (AnimationClipEditor)clipEditor;
                    //FieldInfo fi = typeof(AnimationClipEditor).GetField("m_avatarPreview", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    

                }
            }*/

            bool justStopped = false;

            if (GUILayout.Button(inAnimMode ? "Stop Animation Mode" : "Start Animation Mode"))
            {
                if (inAnimMode)
                {
                    justStopped = true;
                    currentAnimationTime = 0f;
                }
                else
                {
                    AnimationMode.StartAnimationMode();
                }
                inAnimMode = !inAnimMode;
            }

            if (inAnimMode)
            {
                if (justStopped || (inAnimMode && !EditorApplication.isPlaying && (sceneObject != null)))
                {
                    AnimationMode.BeginSampling();

                    AnimationMode.SampleAnimationClip(
                        sceneObject,
                        wc.clip,
                        currentAnimationTime
                    );

                    AnimationMode.EndSampling();
                }
            }

            if (justStopped)
            {
                AnimationMode.StopAnimationMode();
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}