using System.Collections;
using System.Collections.Generic;
using Threepeat;
using UnityEngine;

namespace Threepeat { 
public class Example_MakeCharacterJumpWhenYouPressGKey : MonoBehaviour
{
    public NGCharacter character;

    // Start is called before the first frame update
    void Start()
    {
        character = GetComponent<NGCharacter>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.G))
        {
            character.DoJump();
        }
    }
}

}