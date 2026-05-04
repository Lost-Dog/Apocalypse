using System;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using UnityEngine;

namespace KingEdward.SkillTree
{
    [Serializable]
    public class ClipSkillTreeInstructions : GameCreator.Runtime.VisualScripting.Clip
    {
        [SerializeField] private RunInstructionsList m_Instructions = new RunInstructionsList();
        
        public RunInstructionsList Instructions => this.m_Instructions;
        
        public ClipSkillTreeInstructions() : base(0f, 0f)
        { }
        
        protected override void OnStart(GameCreator.Runtime.VisualScripting.ITrack track, Args args)
        {
            base.OnStart(track, args);
            _ = this.m_Instructions.Run(args);
        }
    }
}
