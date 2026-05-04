using System;
using System.Threading.Tasks;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using UnityEngine;

namespace GameCreator.Runtime.VisualScripting
{
    [Title("Change Primary Color")]
    [Description("Changes the Primary Color of a given Material")]

    [Image(typeof(IconColor), ColorTheme.Type.Yellow)]

    [Version(0, 0, 1)]

    [Category("Game Objects/Change Primary Color")]

    [Parameter("Material", "Material to change primary color")]
    [Parameter("Color", "Color target that the Material turns into")]

    [Keywords("Synty", "Material", "Color", "Primary")]
    [Serializable]
    public class InstructionMaterialChangePrimary : TInstructionRenderer
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [SerializeField]
        private Material mat;

        private Color defaultColor;

        [SerializeField]
        private PropertyGetColor m_Color;

        // PROPERTIES: ----------------------------------------------------------------------------

        public override string Title => $"Change Primary Color of {this.mat} to {this.m_Color}";

        // RUN METHOD: ----------------------------------------------------------------------------

        protected override async Task Run(Args args)
        {
            if (mat == null) return;

            Color valueTarget = this.m_Color.Get(args);
            int propertyID = Shader.PropertyToID("_Color");

            mat.SetColor(propertyID, valueTarget);

            await Task.Delay(10);
        }
    }
}
