using MxM;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Threepeat;
using UnityEditor;
using UnityEngine;

namespace ThreepeatEditor
{

#if ENABLE_THREEPEAT_DEV_MODE
    [CreateAssetMenu(fileName = "MMCTIPPlugin", menuName = "Threepeat/Dev/TIPPlugin")]
#endif
    public class MMCTurnInPlacePlugin : MMCIntegrationBase
    {

        public override string GetDescription()
        {
            return "Adds MxM Turn-in-Place (TIP) Extension";
        }

        public override string GetIntegrationName()
        {
            return "Add Turn-in-Place (TIP) Extension";
        }

        public override bool SetupIntegration(GameObject rootObject, GameObject modelObject, NGCharacter character, GameObject camObject = null)
        {
            if (selected)
            {
                MxMTIPExtension tip = modelObject.GetComponent<MxMTIPExtension>();
                if (tip == null)
                {
                    tip = modelObject.AddComponent<MxMTIPExtension>();
                }

                TurnInPlaceProfile tip90 = new TurnInPlaceProfile();
                TurnInPlaceProfile tip180 = new TurnInPlaceProfile();
                tip90.EventName = "TIP90";
                tip90.TriggerRange = new Vector2(45, 140);
                tip90.Warp = true;
                tip180.EventName = "TIP180";
                tip180.TriggerRange = new Vector2(139, 225);
                tip180.Warp = false;

                TurnInPlaceProfile[] tiparray = new TurnInPlaceProfile[]
                {
                    tip90,
                    tip180
                };

                SerializedObject serializedTip = new SerializedObject(tip);
                SerializedProperty profilesProperty = serializedTip.FindProperty("m_turnInPlaceProfiles");
                if (profilesProperty != null)
                {
                    profilesProperty.arraySize = tiparray.Length;
                    for (int i = 0; i < tiparray.Length; i++)
                    {
                        SerializedProperty element = profilesProperty.GetArrayElementAtIndex(i);
                        element.FindPropertyRelative("EventName").stringValue = tiparray[i].EventName;
                        element.FindPropertyRelative("TriggerRange").vector2Value = tiparray[i].TriggerRange;
                        element.FindPropertyRelative("Warp").boolValue = tiparray[i].Warp;
                    }

                    serializedTip.ApplyModifiedPropertiesWithoutUndo();
                }
                else
                {
                    Debug.LogWarning("Could not find m_turnInPlaceProfiles on MxMTIPExtension while setting up TIP integration.");
                }

            }
            return true;
        }

    }
}