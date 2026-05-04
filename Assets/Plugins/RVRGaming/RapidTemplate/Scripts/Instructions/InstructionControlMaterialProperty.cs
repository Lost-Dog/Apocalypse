using System;
using System.Threading.Tasks;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using UnityEngine;

namespace GameCreator.Runtime.VisualScripting
{
    [Title("Control Material Property")]
    [Description("Controls the material's float and HDR color properties on a specified object")]

    [Image(typeof(IconColor), ColorTheme.Type.Yellow)]

    [Category("Renderer/Control Material Property")]

    [Parameter("Action", "Choose whether to Increase or Decrease the float property")]
    [Parameter("Float Property", "The float property name to change (for Increase/Decrease actions)")]
    [Parameter("HDR Color", "The target HDR color to change to")]
    [Parameter("Object", "The object that has the ChangeMaterialProperty script attached")]

    [Serializable]
    public class InstructionControlMaterialProperty : TInstructionGameObject
    {
        public enum ActionType
        {
            Increase,
            Decrease
        }

        // MEMBERS: -------------------------------------------------------------------------------

        [SerializeField] private ActionType m_Action = ActionType.Increase;

        [SerializeField] private PropertyGetString m_FloatProperty = new PropertyGetString("_MyFloatProperty");

        [SerializeField]
        private PropertyGetColor m_Color = new PropertyGetColor(Color.white);

        // PROPERTIES: ----------------------------------------------------------------------------

        public override string Title => $"Control {this.m_GameObject} Material Properties";

        // RUN METHOD: ----------------------------------------------------------------------------

        protected override async Task Run(Args args)
        {
            GameObject gameObject = this.m_GameObject.Get(args);
            if (gameObject == null) return;

            ChangeMaterialProperty materialProperty = gameObject.GetComponent<ChangeMaterialProperty>();
            if (materialProperty == null) return;

            materialProperty.propertyName = this.m_FloatProperty.Get(args);
            if (m_Action == ActionType.Increase)
            {
                materialProperty.Increase();
            }
            else if (m_Action == ActionType.Decrease)
            {
                materialProperty.Decrease();
            }

            materialProperty.ChangeHDRColor(this.m_Color.Get(args));

            await Task.Yield();
        }
    }
}
