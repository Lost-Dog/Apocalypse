using JUTPS.InventorySystem.UI;
using JUTPS.ItemSystem;
using UnityEngine;

namespace JU.SaveLoad
{
    /// <summary>
    /// Save and load <see cref="InventorySlotUI"/> state.
    /// </summary>
    [RequireComponent(typeof(InventorySlotUI))]
    [AddComponentMenu("JU TPS/Save Load/JU Save Load Inventory Slot UI")]
    public class JUSaveLoadInventorySlotUI : JUSaveLoadComponent
    {
        private JUSaveLoadModeComponent _saveMode;

        private InventorySlotUI _slotUi;
        private const string CURRENT_EQUIPED_ITEM_KEY = "EQUIPED ITEM";

        /// <inheritdoc/>
        protected override void Awake()
        {
            _slotUi = GetComponent<InventorySlotUI>();

            base.Awake();
        }

        /// <inheritdoc/>
        public override void Load()
        {
            base.Load();

            if (!_slotUi)
                return;

            // Already have an item equiped.
            if (_slotUi.CurrentSlotItem())
                return;

            string lastEquipedItem = GetValue(CURRENT_EQUIPED_ITEM_KEY, string.Empty);

            // There is no an item to equip.
            if (string.IsNullOrEmpty(lastEquipedItem))
                return;

            for (int i = 0; i < _slotUi.Inventory.AllItems.Length; i++)
            {
                JUItem inventoryItem = _slotUi.Inventory.AllItems[i];
                if (!inventoryItem)
                    continue;

                if (inventoryItem.ItemName.Equals(lastEquipedItem))
                {
                    _slotUi.SetItemOnSlot(inventoryItem);
                    break;
                }
            }
        }

        /// <inheritdoc/>
        public override void Save()
        {
            base.Save();

            if (!_slotUi)
                return;

            JUItem item = _slotUi.CurrentSlotItem();
            string itemName = item ? item.ItemName : string.Empty;
            SetValue(CURRENT_EQUIPED_ITEM_KEY, itemName);
        }
    }
}