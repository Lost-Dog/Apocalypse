using System;
using System.Threading.Tasks;
using GameCreator.Runtime.Characters;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using GameCreator.Runtime.Variables;
using UnityEngine;
using UnityEngine.UI;

namespace GameCreator.Runtime.Inventory
{
    [Title("Add Item Plus")]
    [Description("Creates a new item and adds a popup notification")]

    [Category("Inventory/Bags/Add Item Plus")]

    [Parameter("Item", "The type of item created")]
    [Parameter("Bag", "The targeted Bag component")]
    [Parameter("Game Object", "Game Object reference that is instantiated")]

    [Keywords("Bag", "Inventory", "Container", "Stash", "Add", "Item")]
    [Keywords("Give", "Take", "Borrow", "Lend", "Buy", "Purchase", "Sell", "Steal", "Plus")]

    [Image(typeof(IconItem), ColorTheme.Type.Green, typeof(OverlayPlus))]

    [Serializable]
    public class InstructionAddItemPlus : Instruction
    {
        [SerializeField] private PropertyGetItem m_Item = new PropertyGetItem();
        [SerializeField] private PropertyGetGameObject m_Bag = GetGameObjectPlayer.Create();
        [SerializeField] private PropertyGetDecimal m_Amount = new PropertyGetDecimal();

        [SerializeField] private PropertyGetInstantiate m_GameObject = new PropertyGetInstantiate();

        [HideInInspector] [SerializeField] private PropertyGetPosition m_Position = GetPositionCharactersPlayer.Create;
        [HideInInspector] [SerializeField] private PropertyGetRotation m_Rotation = GetRotationCharactersPlayer.Create;
        [HideInInspector] [SerializeField] private PropertyGetGameObject m_Parent = GetGameObjectNone.Create();
        [HideInInspector] [SerializeField] private PropertySetGameObject m_Save = SetGameObjectNone.Create;

        public override string Title => $"Add {this.m_Item} to {this.m_Bag} and a popup";

        protected override Task Run(Args args)
        {
            Item item = this.m_Item.Get(args);
            Bag bag = this.m_Bag.Get<Bag>(args);
            if (item != null && bag != null)
            {
                double amount = this.m_Amount.Get(args);
                int intAmount = (int)amount; 

                for (int i = 0; i < intAmount; i++)
                {
                    bag.Content.AddType(item, true);
                }
            }

            Vector3 position = this.m_Position.Get(args);
            Quaternion rotation = this.m_Rotation.Get(args);
            GameObject instance = this.m_GameObject.Get(args, position, rotation);

            if (instance != null)
            {
                Transform parent = this.m_Parent.Get<Transform>(args);
                if (parent != null) instance.transform.SetParent(parent);

                if (item != null)
                {
                    string itemName = item.Info.Name(args);
                    Sprite itemSprite = item.Info.Sprite(args);

                    Transform iconTransform = instance.transform.Find("Icon");
                    if (iconTransform != null)
                    {
                        Image imageComponent = iconTransform.GetComponent<Image>();
                        if (imageComponent != null)
                        {
                            imageComponent.sprite = itemSprite;
                        }
                    }

                    Transform descriptionTransform = instance.transform.Find("Description");
                    if (descriptionTransform != null)
                    {
                        Text descriptionText = descriptionTransform.GetComponent<Text>();
                        if (descriptionText != null)
                        {
                            descriptionText.text = $"+{m_Amount} {itemName}";
                        }
                    }
                }

                this.m_Save.Set(instance, args);
            }

            return DefaultResult;
        }
    }
}
