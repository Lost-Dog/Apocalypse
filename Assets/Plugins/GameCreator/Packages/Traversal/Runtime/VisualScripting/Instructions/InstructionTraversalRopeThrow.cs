using System;
using System.Threading.Tasks;
using GameCreator.Runtime.Characters;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using UnityEngine;

namespace GameCreator.Runtime.Traversal
{
    [Version(0, 1, 1)]

    [Title("Throw Rope")]
    [Description("Starts the rope throw animation on a Character")]

    [Category("Traversal/Rope/Throw Rope")]

    [Keywords("Grapple", "Hook")]
    [Image(typeof(IconTraverseRope), ColorTheme.Type.Yellow, typeof(OverlayArrowRight))]
    
    [Serializable]
    public class InstructionTraversalRopeThrow : Instruction
    {
        [SerializeField]
        private PropertyGetGameObject m_Character = GetGameObjectPlayer.Create();

        [SerializeField]
        private PropertyGetGameObject m_Rope = GetGameObjectPlayer.Create();
        
        [SerializeField]
        private PropertyGetGameObject m_RopeSource = GetGameObjectCharactersBone.Create(
            GetGameObjectPlayer.Create(),
            new Bone(HumanBodyBones.LeftHand)
        );

        [SerializeField] 
        private PropertyGetGameObject m_RopeTarget = new PropertyGetGameObject();
        
        [SerializeField]
        private PropertyGetDecimal m_DurationAnticipation = new PropertyGetDecimal(0f);
        
        [SerializeField]
        private PropertyGetDecimal m_DurationThrow = new PropertyGetDecimal(0.25f);
        
        [SerializeField]
        private PropertyGetDecimal m_DurationTension = new PropertyGetDecimal(2f);
        
        [SerializeField]
        private PropertyGetDecimal m_DurationReel = new PropertyGetDecimal(1f);

        public override string Title => $"Throw Rope on {this.m_Rope}";
        
        protected override Task Run(Args args)
        {
            Character character = this.m_Character.Get<Character>(args);
            Rope rope = this.m_Rope.Get<Rope>(args);

            if (rope == null) return DefaultResult;
            
            float anticipationDuration = (float) this.m_DurationAnticipation.Get(args);
            float throwDuration = (float) this.m_DurationThrow.Get(args);
            float tensionDuration = (float) this.m_DurationTension.Get(args);
            float reelDuration = (float) this.m_DurationReel.Get(args);
            
            rope.Throw(
                character,
                this.m_RopeSource.Get<Transform>(args),
                this.m_RopeTarget.Get<Transform>(args),
                anticipationDuration,
                throwDuration,
                tensionDuration,
                reelDuration
            );
            
            return DefaultResult;
        }
    }
}
