using System;
using System.Threading.Tasks;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.Characters;
using GameCreator.Runtime.VisualScripting;
using UnityEngine;

[Title("Set Faction")]
[Description("Sets the Faction on the AIClass component of a target GameObject")]

[Category("AI/Set Faction")]

[Parameter("Target", "The GameObject with the AIClass component")]
[Parameter("Faction", "The Faction to set")]

[Keywords("AI", "Faction", "Set")]

[Image(typeof(IconCubeSolid), ColorTheme.Type.Blue)]
[Serializable]
public class InstructionSetFactionOnAIClass : Instruction
{
    [SerializeField]
    private PropertyGetGameObject m_Target = GetGameObjectPlayer.Create();

    [SerializeField]
    private AIClass.Faction m_Faction = AIClass.Faction.EnemyGroup1;

    public override string Title => $"Set Faction {m_Faction} on {m_Target}";

    protected override Task Run(Args args)
    {
        GameObject target = this.m_Target.Get(args);
        if (target == null)
        {
            Debug.LogWarning("Target GameObject is null");
            return DefaultResult;
        }

        AIClass aiClass = target.GetComponent<AIClass>();
        if (aiClass == null)
        {
            Debug.LogWarning($"AIClass component not found on {target.name}");
            return DefaultResult;
        }

        aiClass.SetFaction(m_Faction);
        Debug.Log($"Faction set to {m_Faction} on {target.name}");

        return DefaultResult;
    }
}
