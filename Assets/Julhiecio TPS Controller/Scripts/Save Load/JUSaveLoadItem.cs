using JUTPS.ItemSystem;

namespace JU.SaveLoad
{
    /// <summary>
    /// Save and load data from a <typeparamref name="T"/> component.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class JUSaveLoadItem<T> : JUSaveLoadComponent where T : JUItem
    {
        private T _item;

        private const string UNLOCKED_KEY = "Unlocked";
        private const string QUANTITY_KEY = "Count";
        private const string MAX_QUANTITY_KEY = "Max Count";

        /// <summary>
        /// The <see cref="JUItem"/> component to save and load.
        /// </summary>
        public T Item
        {
            get
            {
                FindComponents();
                return _item;
            }
        }

        /// <inheritdoc/>
        protected JUSaveLoadItem()
        {
        }

        /// <inheritdoc/>
        protected override void Awake()
        {
            FindComponents();
            base.Awake();
        }

        /// <inheritdoc/>
        private void FindComponents()
        {
            if (!_item)
                _item = GetComponent<T>();
        }

        /// <inheritdoc/>
        public override void Load()
        {
            base.Load();

            Item.Unlocked = GetValue(UNLOCKED_KEY, Item.Unlocked);
            Item.ItemQuantity = GetValue(QUANTITY_KEY, Item.ItemQuantity);
            Item.MaxItemQuantity = GetValue(MAX_QUANTITY_KEY, Item.MaxItemQuantity);
        }

        /// <inheritdoc/>
        public override void Save()
        {
            base.Save();

            SetValue(UNLOCKED_KEY, Item.Unlocked);
            SetValue(QUANTITY_KEY, Item.ItemQuantity);
            SetValue(MAX_QUANTITY_KEY, Item.MaxItemQuantity);
        }

        /// <inheritdoc/>
        protected override void OnExitPlayMode()
        {
            base.OnExitPlayMode();
            _item = null;
        }
    }
}