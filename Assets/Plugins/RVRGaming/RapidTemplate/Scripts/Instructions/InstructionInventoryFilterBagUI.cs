using System;
using System.Threading.Tasks;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using UnityEngine;

namespace GameCreator.Runtime.Inventory.UnityUI
{
    [Version(0, 0, 1)]
    [Title("Filter Bag UI")]
    [Description("Filters the Bag UI")]
    [Category("Inventory/UI/Filter Bag UI")]
    [Parameter("Bag UI", "Filter")]
    [Parameter("Bag", "The new Bag")]
    [Image(typeof(IconBagSolid), ColorTheme.Type.Green)]
    [Serializable]
    public class InstructionInventoryFilterBagUI : Instruction
    {
        // EXPOSED MEMBERS: -----------------------------------------------------------------------
        [SerializeField] private BagListUI m_BagListUI;
        [SerializeField] private Item m_FilterByParent;

        // PROPERTIES: ----------------------------------------------------------------------------
        public bool IsActive { get; private set; } = false;

        // INITIALIZERS: --------------------------------------------------------------------------
        private void OnEnable()
        {
            if (this.m_BagListUI == null) return;
            this.m_BagListUI.EventRefreshUI -= this.RefreshUI;
            this.m_BagListUI.EventRefreshUI += this.RefreshUI;

            this.RefreshUI();
        }

        private void OnDisable()
        {
            if (this.m_BagListUI == null) return;
            this.m_BagListUI.EventRefreshUI -= this.RefreshUI;
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------
        public void SetBagList()
        {
            if (this.m_BagListUI == null) return;
            this.m_BagListUI.FilterByParent = this.m_FilterByParent;
            this.RefreshUI();
        }

        private void RefreshUI()
        {
            if (this.m_BagListUI == null) return;

            Item currentFilter = this.m_BagListUI.FilterByParent;
            this.IsActive = this.m_FilterByParent == currentFilter;
        }

        // IMPLEMENTED ABSTRACT METHOD: -----------------------------------------------------------
        protected override Task Run(Args args)
        {
            SetBagList();
            return Task.CompletedTask;
        }
    }
}
