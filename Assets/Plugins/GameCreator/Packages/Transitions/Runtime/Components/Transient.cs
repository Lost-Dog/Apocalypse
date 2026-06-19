using System;
using System.Collections;
using System.Threading.Tasks;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.Common.Audio;
using GameCreator.Runtime.Common.UnityUI;
using UnityEngine;
using UnityEngine.UI;

namespace GameCreator.Runtime.Transitions
{
    [AddComponentMenu("Game Creator/Transitions/Transient")]
    [Icon(RuntimePaths.PACKAGES + "Transitions/Editor/Gizmos/GizmoTransient.png")]
    
    public class Transient : MonoBehaviour
    {
        private static readonly int ANIM_OPEN = Animator.StringToHash("Open");
        private static readonly int ANIM_READY = Animator.StringToHash("Ready");
        private static readonly int ANIM_CLOSE = Animator.StringToHash("Close");

        #if UNITY_EDITOR

        [UnityEditor.InitializeOnEnterPlayMode]
        private static void OnEnterPlayMode()
        {
            Current = null;
            EventOpen = null;
            EventReady = null;
            EventClose = null;
        }
        
        #endif

        private enum Decimals
        {
            None,
            One,
            Two
        }

        // EXPOSED MEMBERS: -----------------------------------------------------------------------

        [SerializeField] private TextReference m_Value = new TextReference();
        [SerializeField] private Decimals m_Decimals = Decimals.None;
        
        [SerializeField] private Image m_Progress;

        [SerializeField] private GameObject m_ActiveIfOpen;
        [SerializeField] private GameObject m_ActiveIfReady;
        
        // MEMBERS: -------------------------------------------------------------------------------
        
        [NonSerialized] private Transition m_Transition;
        [NonSerialized] private Animator m_Animator;

        // PROPERTIES: ----------------------------------------------------------------------------

        [field: NonSerialized] public bool IsReady { get; private set; }
        
        [field: NonSerialized] public static Transient Current { get; private set; }

        // EVENTS: --------------------------------------------------------------------------------
        
        public static event Action EventOpen;
        public static event Action EventReady;
        public static event Action EventClose;
        
        // UPDATE METHODS: ------------------------------------------------------------------------

        private void Update()
        {
            if (this.m_Transition == null) return;
            if (this.m_Transition.AsyncLoad == null) return;
            
            float value = Mathf.InverseLerp(0f, 0.9f, this.m_Transition.AsyncLoad.progress);
            this.m_Value.Text = this.FormatPercent(value);
            if (this.m_Progress != null) this.m_Progress.fillAmount = value;
        }

        // INTERNAL METHODS: ----------------------------------------------------------------------

        internal async Task Open(Transition transition)
        {
            Current = this;
            this.m_Transition = transition;
            this.m_Animator = this.Get<Animator>();
            
            DontDestroyOnLoad(this.gameObject);
            this.IsReady = false;
            
            this.m_Value.Text = this.FormatPercent(0);
            if (this.m_Progress != null) this.m_Progress.fillAmount = 0f;
            
            if (this.m_ActiveIfOpen != null) this.m_ActiveIfOpen.SetActive(false);
            if (this.m_ActiveIfReady != null) this.m_ActiveIfReady.SetActive(false);

            if (this.m_Animator != null)
            {
                this.m_Animator.runtimeAnimatorController = transition.Controller;
                this.m_Animator.SetTrigger(ANIM_OPEN);
            }
            
            EventOpen?.Invoke();
            
            float startTime = Time.unscaledTime;
            float duration = this.m_Transition.DurationIn;

            while (Time.unscaledTime < startTime + duration)
            {
                if (ApplicationManager.IsExiting) return;
                await Task.Yield();
            }
            
            if (this.m_ActiveIfOpen != null) this.m_ActiveIfOpen.SetActive(true);
            if (this.m_ActiveIfReady != null) this.m_ActiveIfReady.SetActive(false);
        }

        internal void Ready()
        {
            if (this.m_Animator != null) this.m_Animator.SetTrigger(ANIM_READY);

            this.IsReady = true;
            if (this.m_ActiveIfOpen != null) this.m_ActiveIfOpen.SetActive(false);
            if (this.m_ActiveIfReady != null) this.m_ActiveIfReady.SetActive(true);
            
            EventReady?.Invoke();
        }

        internal async Task Close()
        {
            if (!this.IsReady) return;

            EventClose?.Invoke();
            await Task.Yield();

            if (this.m_ActiveIfOpen != null) this.m_ActiveIfOpen.SetActive(false);
            if (this.m_ActiveIfReady != null) this.m_ActiveIfReady.SetActive(false);
            
            if (this.m_Animator != null) this.m_Animator.SetTrigger(ANIM_CLOSE);

            float startTime = Time.unscaledTime;
            float duration = this.m_Transition.DurationOut;

            while (Time.unscaledTime < startTime + duration)
            {
                if (ApplicationManager.IsExiting) return;
                await Task.Yield();
            }

            Current = null;
            Destroy(this.gameObject);
        }
        
        // PRIVATE METHODS: -----------------------------------------------------------------------

        private string FormatPercent(float value)
        {
            return this.m_Decimals switch
            {
                Decimals.None => value.ToString("0%"),
                Decimals.One => value.ToString("0.0%"),
                Decimals.Two => value.ToString("0.00%"),
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }
}