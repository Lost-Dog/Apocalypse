using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Threepeat {
    public class Example_SetNavMeshTargetOnLKey : MonoBehaviour
    {
        public Transform target;

        private NGCharacter character;

        // Update is called once per frame
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.L))
            {
                if (character == null)
                {
                    character = GetComponent<NGCharacter>();
                }
                
                NGInputScheme_NavMesh inp = (NGInputScheme_NavMesh)character.InputScheme;
                inp.currentTarget = target;
            }
        }
    }
}