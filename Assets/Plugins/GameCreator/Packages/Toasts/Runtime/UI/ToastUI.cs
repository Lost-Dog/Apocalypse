using System;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.Common.UnityUI;
using UnityEngine;
using UnityEngine.UI;

namespace GameCreator.Runtime.Toasts
{
    [Icon(RuntimePaths.PACKAGES + "Toasts/Editor/Gizmos/GizmoToastUI.png")]
    [AddComponentMenu("Game Creator/UI/Toast UI")]
    
    [Serializable]
    public class ToastUI : MonoBehaviour
    {
        [SerializeField] private TextReference m_Text = new TextReference();
        [SerializeField] private Image m_Icon;
        [SerializeField] private Graphic m_Color;
        [SerializeField] private Image m_Duration;

        [SerializeField] private GameObject m_ActiveIfText;
        [SerializeField] private GameObject m_ActiveIfIcon;

        public void Refresh(ToastsPanelUI.Toast toast, float progress)
        {
            this.m_Text.Text = toast.text;
            
            if (this.m_Icon != null) this.m_Icon.sprite = toast.sprite;
            if (this.m_Color != null) this.m_Color.color = toast.color;
            if (this.m_Duration != null) this.m_Duration.fillAmount = progress;
            
            if (this.m_ActiveIfText != null) this.m_ActiveIfText.SetActive(!string.IsNullOrEmpty(toast.text));
            if (this.m_ActiveIfIcon != null) this.m_ActiveIfIcon.SetActive(toast.sprite != null);
        }
    }
}