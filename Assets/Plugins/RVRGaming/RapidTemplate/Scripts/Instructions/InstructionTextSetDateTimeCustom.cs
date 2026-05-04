using System;
using System.Threading.Tasks;
using GameCreator.Runtime.Common;
using UnityEngine;

namespace GameCreator.Runtime.VisualScripting
{
    [Title("Set DateTime Custom")]
    [Description("Changes the value of a string to current DateTime and saves it in PlayerPrefs")]
    [Image(typeof(IconString), ColorTheme.Type.Yellow)]

    [Category("UI/Set DateTime Custom")]
    [Parameter("DateTime", "The source of the DateTime")]

    [Serializable]
    public class InstructionTextSetDateTimeCustom : TInstructionText
    {
        // PROPERTIES: ----------------------------------------------------------------------------
        public int slot = 0;  // Slot variable to specify which slot to save the DateTime.

        public override string Title => $"Set Current DateTime";

        // RUN METHOD: ----------------------------------------------------------------------------
        protected override Task Run(Args args)
        {
            // Check if the slot is within range (0-2).
            if (slot < 0 || slot > 2)
            {
                Debug.LogError("Invalid slot number. It should be between 0 and 2.");
                return DefaultResult;
            }

            // Get current local DateTime
            string currentDateTime = DateTime.Now.ToString();

            this.m_Set.Set(currentDateTime, args);

            // Save the DateTime in PlayerPrefs
            PlayerPrefs.SetString($"DateTimeSlot{slot}", currentDateTime);
            PlayerPrefs.Save();

            return DefaultResult;
        }
    }
}
