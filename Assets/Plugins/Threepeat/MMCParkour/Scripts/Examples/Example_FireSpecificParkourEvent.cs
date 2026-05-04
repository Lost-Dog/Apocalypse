using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Threepeat
{
    public class Example_FireSpecificParkourEvent : MonoBehaviour
    {
        private bool functionEnabled = false;
        public KeyCode activationKey = KeyCode.Q;
        public KeyCode activationKey2 = KeyCode.Joystick1Button2;
        private NGCharacter character;
        private NGAbilityParkour parkour;

        public NGMxMEventDef eventDefinition;

        // Start is called before the first frame update
        void Start()
        {
            character = GetComponent<NGCharacter>();
            if ((character == null) && (transform.parent != null))
            {
                character = transform.parent.GetComponent<NGCharacter>();
            }
            parkour = character.GetComponent<NGAbilityParkour>();
        }

        // Update is called once per frame
        void Update()
        {
            if (Input.GetKeyDown(activationKey) || Input.GetKeyDown(activationKey2))
            {
                FireEvent();
            }
        }

        private void FireEvent()
        {
            Debug.Log("Firing specific event");
            character.forceHardLandingOneShot = true;
            eventDefinition.ClearContacts();
            
            parkour.ExecuteSpecificParkourEvent(eventDefinition);
        }
    }
}