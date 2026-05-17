using Invector.vItemManager;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Bridges the new Input System to Invector's vInventory open/close logic,
/// bypassing GenericInput which requires the legacy input manager.
/// </summary>
[RequireComponent(typeof(vInventory))]
public class InventoryInputBridge : MonoBehaviour
{
    private const string OpenInventoryKeyName = "I";

    [Tooltip("Key used to toggle the inventory open/closed.")]
    [SerializeField] private Key openInventoryKey = Key.I;

    private vInventory inventory;

    private void Awake()
    {
        inventory = GetComponent<vInventory>();
    }

    private void Update()
    {
        if (!Keyboard.current[openInventoryKey].wasPressedThisFrame) return;
        if (inventory.IsLocked() || !inventory.canEquip) return;

        if (inventory.isOpen)
            inventory.CloseInventory();
        else
            inventory.OpenInventory();
    }
}
