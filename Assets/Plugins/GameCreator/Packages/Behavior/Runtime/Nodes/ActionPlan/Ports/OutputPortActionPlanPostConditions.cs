using System;

namespace GameCreator.Runtime.Behavior
{
    [Serializable]
    public class OutputPortActionPlanPostConditions : TOutputPort
    {
        public override PortAllowance Allowance => PortAllowance.Single;

        public override WireShape WireShape => WireShape.Bezier;

        public override PortPosition Position
        {
            get => PortPosition.Right;
            set => _ = value;
        }
    }
}