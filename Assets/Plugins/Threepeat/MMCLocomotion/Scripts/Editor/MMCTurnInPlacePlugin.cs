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
                MxMTIPExtension tip = modelObject.AddComponent<MxMTIPExtension>();
                FieldInfo fi = typeof(MxMTIPExtension).GetField("m_turnInPlaceProfiles", BindingFlags.NonPublic | BindingFlags.Instance);
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
                    //= (TurnInPlaceProfile[])fi.GetValue(tip);
                fi.SetValue(tip, tiparray);
                //FieldInfo fi2 = typeof(CharacterAnimation).GetField("graph", BindingFlags.NonPublic | BindingFlags.Instance);
                //PlayableGraph graph = (PlayableGraph)fi2.GetValue(cca);

            }
            return true;
        }

    }
}