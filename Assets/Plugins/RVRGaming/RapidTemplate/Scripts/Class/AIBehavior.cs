using System;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using UnityEngine;

[CreateAssetMenu(fileName = "AIBehavior", menuName = "Rapid Template/AI Behavior", order = 1)]
public class AIBehavior : ScriptableObject
{
    [SerializeField] public InstructionList onRun = new InstructionList();

    public void RunInstructions(GameObject gameObject)
    {
        if (gameObject == null) return;
        _ = this.onRun.Run(new Args(gameObject));
    }
}
