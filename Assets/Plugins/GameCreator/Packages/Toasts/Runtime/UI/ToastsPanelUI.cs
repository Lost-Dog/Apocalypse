using System;
using System.Collections.Generic;
using GameCreator.Runtime.Common;
using UnityEngine;

namespace GameCreator.Runtime.Toasts
{
    [Icon(RuntimePaths.PACKAGES + "Toasts/Editor/Gizmos/GizmoToastsPanelUI.png")]
    [AddComponentMenu("Game Creator/UI/Toasts Panel UI")]
    
    [Serializable]
    public class ToastsPanelUI : MonoBehaviour
    {
        public struct Toast
        {
            [NonSerialized] public string text;
            [NonSerialized] public Sprite sprite;
            [NonSerialized] public Color color;
            [NonSerialized] public float startTime;
        }
        
        // EXPOSED MEMBERS: -----------------------------------------------------------------------

        [SerializeField] private TimeMode.UpdateMode m_TimeMode = TimeMode.UpdateMode.GameTime;
        [SerializeField] private IdString m_PanelId = IdString.EMPTY;

        [SerializeField] private RectTransform m_Content;
        [SerializeField] private GameObject m_ToastPrefab;
        
        [SerializeField] private float m_Duration = 5f;
        [SerializeField] private EnablerInt m_MaxToasts = new EnablerInt(true, 5);
        
        // MEMBERS: -------------------------------------------------------------------------------
        
        [NonSerialized] private readonly Queue<Toast> m_Toasts = new Queue<Toast>();

        [NonSerialized] private readonly List<GameObject> m_DestroyList = new List<GameObject>();
        
        // PROPERTIES: ----------------------------------------------------------------------------
        
        public IdString PanelId => this.m_PanelId;
        
        // INITIALIZATION: ------------------------------------------------------------------------
        
        private void OnEnable()
        {
            if (ApplicationManager.IsExiting) return;
            ToastsManager.Instance.RegisterPanel(this);
        }

        private void OnDisable()
        {
            if (ApplicationManager.IsExiting) return;
            ToastsManager.Instance.UnregisterPanel(this);
        }
        
        // UPDATE METHODS: ------------------------------------------------------------------------

        private void Update()
        {
            if (this.m_Content == null || this.m_ToastPrefab == null) return;
            
            this.m_DestroyList.Clear();
            
            float currentTime = this.m_TimeMode switch
            {
                TimeMode.UpdateMode.GameTime => Time.time,
                TimeMode.UpdateMode.UnscaledTime => Time.unscaledTime,
                _ => throw new ArgumentOutOfRangeException()
            };

            if (this.m_MaxToasts.IsEnabled)
            {
                while (this.m_Toasts.Count > this.m_MaxToasts.Value)
                {
                    this.m_Toasts.Dequeue();
                }
            }
            
            RectTransformUtils.RebuildChildren(
                this.m_Content,
                this.m_ToastPrefab,
                this.m_Toasts.Count
            );

            int index = 0;
            foreach (Toast toast in this.m_Toasts)
            {
                if (this.m_Content.childCount <= index) break;
                
                Transform toastTransform = this.m_Content.GetChild(index);
                ToastUI toastUI = toastTransform.Get<ToastUI>();

                if (toastUI != null)
                {
                    float progress = this.m_Duration > float.Epsilon
                        ? (currentTime - toast.startTime) / this.m_Duration
                        : 0f;
                    
                    toastUI.Refresh(toast, progress);
                }

                if (currentTime - toast.startTime > this.m_Duration)
                {
                    this.m_DestroyList.Add(toastTransform.gameObject);
                }
                
                index += 1;
            }

            foreach (GameObject destroyItem in this.m_DestroyList)
            {
                this.m_Toasts.Dequeue();
                Destroy(destroyItem);
            }
        }

        // PUBLIC METHODS: ------------------------------------------------------------------------
        
        public void Push(string text, Sprite sprite, Color color)
        {
            Toast toast = new Toast
            {
                text = text,
                sprite = sprite,
                color = color,
                startTime = this.m_TimeMode switch
                {
                    TimeMode.UpdateMode.GameTime => Time.time,
                    TimeMode.UpdateMode.UnscaledTime => Time.unscaledTime,
                    _ => throw new ArgumentOutOfRangeException()
                }
            };
            
            this.m_Toasts.Enqueue(toast);
        }
    }
}