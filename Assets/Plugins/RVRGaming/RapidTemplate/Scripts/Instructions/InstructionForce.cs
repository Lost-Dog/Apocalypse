using System;
using System.Threading.Tasks;
using GameCreator.Runtime.Characters;
using GameCreator.Runtime.Common;
using UnityEngine;

namespace GameCreator.Runtime.VisualScripting
{
    [Title("Physics Force")]
    [Description("Throws an object in a straight line and optionally returns it back to its origin")]

    [Category("Physics 3D/Physics Force")]

    [Parameter("Force Vector", "The direction and magnitude of the force")]
    [Parameter("Use Impulse", "Whether to apply the force as an impulse")]

    [Keywords("Apply", "Velocity", "Impulse", "Propel", "Push", "Pull")]
    [Keywords("Physics", "Rigidbody")]
    [Image(typeof(IconPhysics), ColorTheme.Type.Green, typeof(OverlayArrowRight))]

    [Serializable]
    public class InstructionForce : Instruction
    {
        [SerializeField] private PropertyGetGameObject m_GameObject = new PropertyGetGameObject();
        [SerializeField] private float forceAmount = 10.0f;
        [SerializeField] private bool useImpulse = true;
        [SerializeField] private Vector3 offset = Vector3.zero;
        [SerializeField] private bool useLocalSpace = true;

        public override string Title => $"Apply Force with Offset to Rigidbody";

        protected override async Task Run(Args args)
        {
            GameObject throwObjectInstance = this.m_GameObject.Get(args);
            Rigidbody rb = throwObjectInstance.GetComponent<Rigidbody>();

            if (rb != null)
            {
                Vector3 appliedOffset = useLocalSpace
                                        ? throwObjectInstance.transform.TransformPoint(offset) - throwObjectInstance.transform.position
                                        : offset;

                Vector3 forceDirection = appliedOffset.normalized;
                Vector3 force = forceDirection * forceAmount;
                ForceMode forceMode = useImpulse ? ForceMode.Impulse : ForceMode.Force;
                Vector3 position = throwObjectInstance.transform.position + appliedOffset;

                rb.AddForceAtPosition(force, position, forceMode);

                await Task.Delay(10);
            }
        }
    }
}
