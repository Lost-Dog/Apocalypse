using System;
using System.Threading.Tasks;
using GameCreator.Runtime.Common;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GameCreator.Runtime.Transitions
{
    [Icon(RuntimePaths.PACKAGES + "Transitions/Editor/Gizmos/GizmoTransition.png")]
    
    public class Transition : TSkin<GameObject>, ISerializationCallbackReceiver
    {
        private const string MSG = "The transition style when switching between scenes";
        private const string ERR_NO_VALUE = "Prefab value cannot be empty";
        private const string ERR_TRANSIENT = "Prefab does not contain a 'Transient' component";

        #if UNITY_EDITOR

        [UnityEditor.InitializeOnEnterPlayMode]
        private static void OnEnterPlayMode()
        {
            Current = null;
        }
        
        #endif
        
        // EXPOSED MEMBERS: -----------------------------------------------------------------------

        [SerializeField] private AnimatorOverrideController m_Controller;
        
        [SerializeField] private AnimationClip m_Open;
        [SerializeField] private AnimationClip m_Loading;
        [SerializeField] private AnimationClip m_Ready;
        [SerializeField] private AnimationClip m_Close;
        
        // MEMBERS: -------------------------------------------------------------------------------

        [NonSerialized] private Transient m_Transient;
        
        // PROPERTIES: ----------------------------------------------------------------------------
        
        public override string Description => MSG;

        public override string HasError
        {
            get
            {
                if (this.Value == null) return ERR_NO_VALUE;
                return !this.Value.GetComponentInChildren<Transient>() 
                    ? ERR_TRANSIENT 
                    : string.Empty;
            }
        }

        [field: NonSerialized] internal static Transition Current { get; private set; }
        [field: NonSerialized] internal AsyncOperation AsyncLoad { get; private set; }
        
        internal AnimatorOverrideController Controller => this.m_Controller;

        internal float DurationIn => this.m_Open != null ? this.m_Open.length : 0f;
        internal float DurationOut => this.m_Close != null ? this.m_Close.length : 0f;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public async Task ToScene(int scene, UnityEngine.SceneManagement.LoadSceneMode mode, bool activate)
        {
            if (Current != null) return;
            
            await this.RequireTransient();
            if (ApplicationManager.IsExiting) return;
            
            this.AsyncLoad = SceneManager.LoadSceneAsync(scene, mode);
            this.AsyncLoad.allowSceneActivation = false;

            while (this.AsyncLoad.progress < 0.9f)
            {
                if (ApplicationManager.IsExiting) return;
                await Task.Yield();
            }

            Transient.EventClose -= this.ActivateScene;
            Transient.EventClose += this.ActivateScene;
            
            this.m_Transient.Ready();
            
            if (activate) await this.Out();
        }

        public async Task In()
        {
            await this.RequireTransient();
            this.m_Transient.Ready();
        }

        public async Task Out()
        {
            if (Current == null || !this.m_Transient.IsReady) return;
            
            await this.m_Transient.Close();
            
            Current = null;
            this.AsyncLoad = null;
        }
        
        // PRIVATE METHODS: -----------------------------------------------------------------------

        private async Task RequireTransient()
        {
            if (Current != null) return;
            Current = this;

            GameObject value = Instantiate(this.Value, Vector3.zero, Quaternion.identity);
            if (value == null) return;

            this.m_Transient = value.Get<Transient>();
            await this.m_Transient.Open(this);
        }

        // CALLBACK METHODS: ----------------------------------------------------------------------

        private void ActivateScene()
        {
            Transient.EventClose -= this.ActivateScene;
            
            if (this.AsyncLoad == null) return;
            this.AsyncLoad.allowSceneActivation = true;
        }
        
        // SERIALIZATION CALLBACKS: ---------------------------------------------------------------
        
        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
            #if UNITY_EDITOR
            
            if (this.m_Controller == null) return;
            this.m_Controller["Transition@Open"] = this.m_Open;
            this.m_Controller["Transition@Loading"] = this.m_Loading;
            this.m_Controller["Transition@Ready"] = this.m_Ready;
            this.m_Controller["Transition@Close"] = this.m_Close;

            #endif
        }

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        { }
    }
}