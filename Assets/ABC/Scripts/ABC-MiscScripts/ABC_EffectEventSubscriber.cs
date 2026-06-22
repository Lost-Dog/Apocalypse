using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace ABCToolkit {
    /// <summary>
    /// Will link into the effect system to run custom functions
    /// </summary>
    [RequireComponent(typeof(ABC_StateManager))]
    [System.Serializable]
    public class ABC_EffectEventSubscriber : MonoBehaviour {

        [System.Serializable]
        public struct CustomEffectEvent {

            public string effectName;
            public UnityEvent unityEvent;

        }




        // ************ Settings *****************************

        #region Settings

        /// <summary>
        /// List of custom effects which contains the effect name and a unity event
        /// </summary>
        public List<CustomEffectEvent> customEffectEvents = new List<CustomEffectEvent>();


        /// <summary>
        /// List of custom effect removal which contains the effect name and a unity event
        /// </summary>
        public List<CustomEffectEvent> customEffectRemovalEvents = new List<CustomEffectEvent>();

        #endregion

        // ********************* Variables ******************

        #region Variables

        /// <summary>
        /// StateManager component we will subcribe too
        /// </summary>
        private ABC_StateManager playerStateManager = null;

        /// <summary>
        /// Transform of the entity this component is attached too
        /// </summary>
        private Transform meTransform;

        #endregion



        // ********************* Public Methods ********************

        #region Public Methods






        #endregion


        // ********************* Private Methods ********************

        #region Private Methods

        /// <summary>
        /// Method subscribed to the statemanager component will listen for effect activations
        /// </summary>
        /// <param name="effect">effect that activated</param>
        private void EffectListener(Effect effect, ABC_IEntity Target, ABC_IEntity Originator) {

            foreach (CustomEffectEvent CEE in this.customEffectEvents.Where(e => e.effectName == effect.effectName)) {
                CEE.unityEvent.Invoke();
            }


            switch (effect.effectName) {
                case "CustomEffect":
                    //add custom effect here
                    break;
            }
        }


        /// <summary>
        /// Method subscribed to the statemanager component will listen for effects being removed
        /// </summary>
        /// <param name="effect">effect that has been removed</param>
        private void EffectRemovalListener(Effect effect, ABC_IEntity Target, ABC_IEntity Originator) {

            foreach (CustomEffectEvent CEE in this.customEffectRemovalEvents.Where(e => e.effectName == effect.effectName)) {
                CEE.unityEvent.Invoke();
            }


            switch (effect.effectName) {
                case "CustomEffectRemoval":
                    //add what happens when effect is removed here
                    break;
            }
        }


        #endregion


        // ********************* Game ********************

        #region Game


        private void OnEnable() {

            //Record transform
            meTransform = transform;

            //Get Statemanger script
            if (playerStateManager == null)
                playerStateManager = meTransform.GetComponent<ABC_StateManager>();

            //Subscribe to event
            playerStateManager.onEffectActivation += EffectListener;
            playerStateManager.onEffectDeActivation += EffectRemovalListener;

        }

        private void OnDisable() {
            //unsubscribe from event
            playerStateManager.onEffectActivation -= EffectListener;
            playerStateManager.onEffectDeActivation -= EffectRemovalListener;

        }


        #endregion
    }
}