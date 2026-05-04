using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ABCToolkit {
    public class DemoMods : MonoBehaviour {


        // ************ Settings *****************************

        #region Settings


        /// <summary>
        /// Debug setting to enable weapons
        /// </summary>
        public bool EnableWeapons = false;

        /// <summary>
        /// Debug setting to enable invincibility
        /// </summary>
        public bool EnableInvincibility = false;

        /// <summary>
        /// Debug setting to disable invincibility
        /// </summary>
        public bool DisableInvincibility = false;


        #endregion


        // ********************* Public Methods ********************

        #region Public Methods

        /// <summary>
        /// Will enable all weapons for all objects tagged as 'Player' 
        /// </summary>
        public void EnableAllPlayerWeapons() {

            foreach (GameObject Obj in GameObject.FindGameObjectsWithTag("Player")) {

                ABC_IEntity entity = ABC_Utilities.GetStaticABCEntity(Obj);

                if (entity.HasABCController() == false)
                    continue;


                foreach (int ID in entity.AllWeapons.Select(w => w.weaponID).ToList())
                    entity.EnableWeapon(ID, false);
            }

        }


        /// <summary>
        /// Will toggle invincibility for all objects tagged as 'Player' 
        /// </summary>
        /// <param name="Enabled">True to enable invincibility, else false to disable</param>
        public void TogglePlayerInvincibility(bool Enabled = true) {

            foreach (GameObject Obj in GameObject.FindGameObjectsWithTag("Player")) {

                ABC_IEntity entity = ABC_Utilities.GetStaticABCEntity(Obj);

                if (entity.HasABCStateManager() == false)
                    continue;

                if (Enabled == true)
                    entity.AdjustHealthRegen(5000);
                else
                    entity.AdjustHealthRegen(-5000);
            }


        }



        #endregion


        // ********************* Game ********************

        #region Game


        // Update is called once per frame
        void Update() {


            //Debug tick box to enable weapons
            if (this.EnableWeapons == true) {
                this.EnableWeapons = false;

                this.EnableAllPlayerWeapons();
            }

            //Debug tick box to enable invincibility
            if (this.EnableInvincibility == true) {
                this.EnableInvincibility = false;

                this.TogglePlayerInvincibility(true);
            }

            //Debug tick box to disable invincibility
            if (this.DisableInvincibility == true) {
                this.DisableInvincibility = false;

                this.TogglePlayerInvincibility(false);
            }

        }

        #endregion

    }
}