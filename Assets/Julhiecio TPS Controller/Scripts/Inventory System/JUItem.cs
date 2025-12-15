using UnityEngine;

using JUTPSEditor.JUHeader;

namespace JUTPS.ItemSystem
{
    public class JUItem : MonoBehaviour
    {
        [JUHeader("Item Setting")]
        public string ItemFilterTag = "General";
        public Sprite ItemIcon;
        public bool Unlocked;
        public int ItemQuantity;
        public int MaxItemQuantity = 1;
        public string ItemName;
        public int ItemSwitchID;

        public bool CanUseItem = true;

        public virtual void UseItem()
        {
            if (CanUseItem == false)
                return;

            if (ItemQuantity > 0)
            {
                RemoveItem();
            }
            else
            {
                return;
            }
        }
        public virtual void RemoveItem()
        {
            ItemQuantity--;
            ItemQuantity = Mathf.Clamp(ItemQuantity, 0, MaxItemQuantity);
            //if (ItemQuantity == 0) Unlocked = false;
        }
        public virtual void AddItem()
        {
            ItemQuantity++;
            ItemQuantity = Mathf.Clamp(ItemQuantity, 0, MaxItemQuantity);

            if (ItemQuantity > 0) Unlocked = true;
        }
    }
}