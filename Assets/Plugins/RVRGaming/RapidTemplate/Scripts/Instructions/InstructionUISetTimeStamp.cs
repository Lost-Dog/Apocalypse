using System;
using System.Threading.Tasks;
using GameCreator.Runtime.Common;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GameCreator.Runtime.VisualScripting
{
    [Version(0, 1, 1)]

    [Title("Change Text to TimeStamp")]
    [Category("UI/Change Text to TimeStamp")]

    [Image(typeof(IconUIText), ColorTheme.Type.TextLight)]
    [Description("Changes the value of a Text or Text Mesh Pro component to stored DateTime")]

    [Parameter("Text", "The Text or Text Mesh Pro component that changes its value")]
    [Parameter("Slot", "The slot number to get stored DateTime")]

    [Serializable]
    public class InstructionUISetTimeStamp : Instruction
    {
        [SerializeField] private PropertyGetGameObject m_Text = GetGameObjectInstance.Create();
        public int slot = 0; // Slot number to select the PlayerPrefs slot.

        public override string Title => $"Time Stamp {this.m_Text}";

        protected override Task Run(Args args)
        {
            // Check if the slot is within range (0-2).
            if (slot < 0 || slot > 2)
            {
                Debug.LogError("Invalid slot number. It should be between 0 and 2.");
                return DefaultResult;
            }

            GameObject gameObject = this.m_Text.Get(args);
            if (gameObject == null) return DefaultResult;

            // Get stored DateTime from PlayerPrefs.
            string storedDateTime = PlayerPrefs.GetString($"DateTimeSlot{slot}");

            Text text = gameObject.Get<Text>();
            if (text != null)
            {
                text.text = storedDateTime;
                return DefaultResult;
            }

            TMP_Text textTMP = gameObject.Get<TMP_Text>();
            if (textTMP != null) textTMP.text = storedDateTime;

            return DefaultResult;
        }
    }
}
