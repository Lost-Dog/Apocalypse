using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Threepeat
{
    public class GameObjectNotes : MonoBehaviour
    {
        [TextArea(2, 20)]
        public string Notes = "";
    }

}