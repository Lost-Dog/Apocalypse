using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class KeyMapIdenticator : MonoBehaviour
{

    /// <summary>
    /// Gamepad Image
    /// </summary>
    public GameObject GamePadMap;

    /// <summary>
    /// Keyboard Image
    /// </summary>
    public GameObject KeyboardMap;

    // Start is called before the first frame update
    private void OnEnable() {

        //If using gamepad then find virtual mouse
        if (InputSystem.devices.Where(d => d is Gamepad).Count() > 0) {

            this.KeyboardMap.SetActive(false);
            this.GamePadMap.SetActive(true);

        } else {
            this.KeyboardMap.SetActive(true);
            this.GamePadMap.SetActive(false);
        }


    }


}
