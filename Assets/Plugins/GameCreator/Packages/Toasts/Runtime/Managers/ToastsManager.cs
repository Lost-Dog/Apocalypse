using System;
using System.Collections.Generic;
using GameCreator.Runtime.Common;
using UnityEngine;

namespace GameCreator.Runtime.Toasts
{
    [AddComponentMenu("")]
    public class ToastsManager : Singleton<ToastsManager>
    {
        // MEMBERS: -------------------------------------------------------------------------------
        
        [NonSerialized]
        private readonly Dictionary<IdString, Dictionary<int, ToastsPanelUI>> m_Panels = 
            new Dictionary<IdString, Dictionary<int, ToastsPanelUI>>();

        // PUBLIC METHODS: ------------------------------------------------------------------------
        
        public void RegisterPanel(ToastsPanelUI toastsPanelUI)
        {
            if (toastsPanelUI == null) return;
            
            IdString panelId = toastsPanelUI.PanelId;
            if (!this.m_Panels.TryGetValue(panelId, out Dictionary<int, ToastsPanelUI> panels))
            {
                panels = new Dictionary<int, ToastsPanelUI>();
                this.m_Panels.Add(panelId, panels);
            }
            
            panels.Add(toastsPanelUI.GetEntityId().GetHashCode(), toastsPanelUI);
        }

        public void UnregisterPanel(ToastsPanelUI toastsPanelUI)
        {
            if (toastsPanelUI == null) return;
            
            IdString panelId = toastsPanelUI.PanelId;
            if (this.m_Panels.TryGetValue(panelId, out Dictionary<int, ToastsPanelUI> panels))
            {
                panels.Remove(toastsPanelUI.GetEntityId().GetHashCode());
                if (panels.Count == 0) this.m_Panels.Remove(panelId);   
            }
        }
        
        public void Push(IdString toastPanelId, string text, Sprite sprite, Color color)
        {
            if (this.m_Panels.TryGetValue(toastPanelId, out Dictionary<int, ToastsPanelUI> panels))
            {
                foreach (KeyValuePair<int, ToastsPanelUI> entry in panels)
                {
                    entry.Value.Push(text, sprite, color);
                }
            }
        }
    }
}
