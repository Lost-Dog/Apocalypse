using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using GameCreator.Runtime.Common;

namespace GameCreator.Runtime.VisualScripting
{
    [Version(0, 1, 1)]

    [Title("Set Depth of Field")]
    [Description("Set the Depth of Field settings in the Universal Render Pipeline")]

    [Category("Rendering/Set Depth of Field")]

    [Keywords("Depth of Field", "Universal Render Pipeline", "Focus", "Blur")]
    [Image(typeof(IconCamera), ColorTheme.Type.Green)]

    [Serializable]
    public class InstructionSetDepthOfField : Instruction
    {
        // PROPERTIES: ----------------------------------------------------------------------------

        [SerializeField] private Volume volume;
        [SerializeField] private bool active = true;
        [SerializeField] private float start = 0.1f;
        [SerializeField] private float end = 100f;
        [SerializeField] private float maxRadius = 1f;

        public override string Title => $"Set URP Depth of Field to {start}, {end}, {maxRadius}";

        // RUN METHOD: ----------------------------------------------------------------------------

        protected override Task Run(Args args)
        {
            if (volume.profile.TryGet<DepthOfField>(out var depthOfField))
            {
                depthOfField.active = active;
                depthOfField.gaussianStart.value = start;
                depthOfField.gaussianEnd.value = end;
                depthOfField.gaussianMaxRadius.value = maxRadius;
            }
            else
            {
                Debug.LogError("Depth of Field settings not found in the Volume Profile");
            }

            return DefaultResult;
        }
    }
}
