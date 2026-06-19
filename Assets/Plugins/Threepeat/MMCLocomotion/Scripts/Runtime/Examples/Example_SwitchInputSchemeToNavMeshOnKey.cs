using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Threepeat
{
    public class Example_SwitchInputSchemeToNavMeshOnKey : MonoBehaviour
    {
        public KeyCode activationKey = KeyCode.O;

        [Tooltip("If left blank, the default NavMesh scheme will be used.")]
        public NGInputScheme_NavMesh targetScheme;

        [Tooltip("If left null, agent will have no destination and remain idle.  Otherwise, NavMeshAgent's destination will be set to this transform (and updated at the configured rate).")]
        public Transform agentTargetTransform;

        private NGCharacter character;

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            if (Input.GetKeyDown(activationKey))
            {
                if (character == null)
                {
                    character = GetComponent<NGCharacter>();
                }

                character.SetInputScheme(targetScheme != null ? targetScheme : ScriptableObject.Instantiate<NGInputScheme_NavMesh>(targetScheme), null);

                NGInputScheme_NavMesh charScheme = (NGInputScheme_NavMesh)character.InputScheme;

                if (agentTargetTransform != null)
                {
                    charScheme.SetTarget(agentTargetTransform);
                }
            }
        }
    }
}