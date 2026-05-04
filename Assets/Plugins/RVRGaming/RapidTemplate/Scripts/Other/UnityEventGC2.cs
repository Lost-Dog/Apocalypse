using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[AddComponentMenu("")]
public class UnityEventGC2 : MonoBehaviour
{
    [Space]
    [SerializeField] public InstructionList onRun = new InstructionList();

    private void Start()
    {

    }

    private void Update()
    {

    }

    public void RunInstructions()
    {
        _ = this.onRun.Run(new Args(this.gameObject));
    }
}
