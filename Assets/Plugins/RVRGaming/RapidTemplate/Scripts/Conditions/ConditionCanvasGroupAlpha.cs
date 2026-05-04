using System;
using GameCreator.Runtime.Common;
using UnityEngine;

namespace GameCreator.Runtime.VisualScripting
{
    [Title("Is Canvas Group Alpha")]
    [Description("Returns true if the alpha value of the Canvas Group matches the specified value")]

    [Category("UI/Is Canvas Group Alpha Match")]

    [Parameter("Game Object", "The game object with Canvas Group used in the condition")]
    [Parameter("Alpha Value", "The alpha value to compare against")]

    [Keywords("UI", "Canvas", "Transparency", "Visibility")]

    [Image(typeof(IconCubeSolid), ColorTheme.Type.Blue)]
    [Serializable]
    public class ConditionCanvasGroupAlpha : Condition
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [SerializeField] private PropertyGetGameObject m_GameObject = new PropertyGetGameObject();
        [SerializeField] private float alphaValue = 1.0f; // Default alpha value to check

        // PROPERTIES: ----------------------------------------------------------------------------

        protected override string Summary => $"Canvas Group Alpha is {alphaValue} in {this.m_GameObject}";

        // RUN METHOD: ----------------------------------------------------------------------------

        protected override bool Run(Args args)
        {
            GameObject gameObject = this.m_GameObject.Get(args);
            if (gameObject == null) return false;

            CanvasGroup canvasGroup = gameObject.GetComponent<CanvasGroup>();
            if (canvasGroup == null) return false;

            return Mathf.Approximately(canvasGroup.alpha, alphaValue);
        }
    }
}
