using UnityEngine;
using System.Collections;

namespace ABCToolkit {
    /// <summary>
    ///  surrogate MonoBehaviour which is used to call courotines
    /// </summary>
    /// <remarks>allows the (non MonoBehaviour) classes to use and cache a MonoBehaviour surrogate from the running instance to allow for coroutine calls.</remarks>
    public class ABC_MbSurrogate : MonoBehaviour {


        void Awake() {
            //Script is attached to pool so lets persist it through different scenes so the pool remains if character remains 
            DontDestroyOnLoad(this.gameObject);
        }
    }
}
