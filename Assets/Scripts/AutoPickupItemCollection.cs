using Invector.vCharacterController.vActions;
using Invector.vItemManager;
using UnityEngine;

/// <summary>
/// Attach to the Player alongside vGenericAction.
/// Every frame, if the nearest trigger action is a vItemCollection, the input type is
/// temporarily treated as AutoAction so the item is collected without requiring a button press.
/// </summary>
[RequireComponent(typeof(vGenericAction))]
[AddComponentMenu("Apocalypse/Items/Auto Pickup Item Collection")]
public class AutoPickupItemCollection : MonoBehaviour
{
    private vGenericAction _genericAction;

    private void Awake()
    {
        _genericAction = GetComponent<vGenericAction>();
    }

    private void Update()
    {
        if (_genericAction == null) return;

        vTriggerGenericAction trigger = _genericAction.triggerAction;
        if (trigger == null) return;

        // Only override items that still require a button press.
        if (trigger.inputType != vTriggerGenericAction.InputType.GetButtonDown &&
            trigger.inputType != vTriggerGenericAction.InputType.GetDoubleButton)
            return;

        // Only auto-collect vItemCollection triggers — leave other interaction types alone.
        if (trigger is vItemCollection)
        {
            trigger.inputType = vTriggerGenericAction.InputType.AutoAction;
        }
    }
}
