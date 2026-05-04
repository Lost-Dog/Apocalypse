using System;
using GameCreator.Runtime.Common;
using UnityEngine;
using UnityEngine.UI;

namespace GameCreator.Runtime.VisualScripting
{
    [Title("Toggle State ")]
    [Description("Returns true if the specified UI Toggle matches the desired state (on or off)")]

    [Category("UI/Toggle State")]

    [Parameter("Game Object", "The game object with the UI Toggle component used in the condition")]
    [Parameter("Toggle State", "The desired state of the toggle (On or Off)")]

    [Keywords("UI", "Toggle", "Switch", "Button")]

    [Image(typeof(IconCubeSolid), ColorTheme.Type.Green)]
    [Serializable]
    public class ConditionToggleState : Condition
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [SerializeField] private PropertyGetGameObject m_GameObject = new PropertyGetGameObject();
        [SerializeField] private bool toggleState = true; // Default state to check (true = on, false = off)

        // PROPERTIES: ----------------------------------------------------------------------------

        protected override string Summary => $"Toggle is {(toggleState ? "On" : "Off")} in {this.m_GameObject}";

        // RUN METHOD: ----------------------------------------------------------------------------

        protected override bool Run(Args args)
        {
            GameObject gameObject = this.m_GameObject.Get(args);
            if (gameObject == null) return false;

            Toggle toggle = gameObject.GetComponent<Toggle>();
            if (toggle == null) return false;

            return toggle.isOn == toggleState;
        }
    }
}
