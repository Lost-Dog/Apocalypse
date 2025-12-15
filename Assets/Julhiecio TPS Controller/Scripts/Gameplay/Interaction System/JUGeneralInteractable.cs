using JUTPS.ActionScripts;
using JUTPS.VehicleSystem;
using UnityEngine;
using UnityEngine.Events;
namespace JUTPS.InteractionSystem.Interactables
{
    [AddComponentMenu("JU TPS/Interaction System/Interactables/JU General Interactable")]
    public class JUGeneralInteractable : JUInteractable
    {
        public string InteractMessage = "Hold To Interact";
        
        public UnityEvent OnInteract;

        public ToggleSystem Toggle;
        
        [System.Serializable]
        public struct ToggleSystem
        {
            [Header("Toggle")]
            public bool IsToggle;
            public string ToggleOn_InteractMessage;
            public string ToggleOff_InteractMessage;
            public UnityEvent ToggleOnEvent;
            public UnityEvent ToggleOffEvent;
            public bool IsToggleOn;
        }
        protected override void Start()
        {
            if (Toggle.IsToggle)
            {
                InteractMessage = Toggle.IsToggleOn ? Toggle.ToggleOff_InteractMessage : Toggle.ToggleOn_InteractMessage;
            }
            base.Start();
        }
        public override bool CanInteract(JUInteractionSystem interactionSystem)
        {
            return InteractionEnabled;
        }
        public override void Interact()
        {
            Debug.Log("Interacted with " + gameObject.name);
            OnInteract.Invoke();
            if (Toggle.IsToggle)
            {
                Toggle.IsToggleOn = !Toggle.IsToggleOn;
                if (Toggle.IsToggleOn)
                {
                    InteractMessage = Toggle.ToggleOff_InteractMessage;
                    Toggle.ToggleOnEvent.Invoke();
                }
                else
                {
                    InteractMessage = Toggle.ToggleOn_InteractMessage;
                    Toggle.ToggleOffEvent.Invoke();
                }
            }
            base.Interact();
        }
    }
}