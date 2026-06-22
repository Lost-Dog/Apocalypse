using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AI;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

#if ABC_GC_Stats_Integration
using GameCreator.Stats;
#endif


#if ABC_GC_2_Stats_Integration
using GameCreator.Runtime.Stats;
#endif

namespace ABCToolkit {

    /// <summary>
    /// A lightweight interface (not strictly typed as an interface) which holds information regarding entity objects using ABC. IEntity holds properties which are access by Abilities and Effects when performing actions like activating abilities
    /// </summary>
    /// <remarks>
    /// All ABC components access this interface when dealing with non ABC Generated Objects (Entities) any property in this class can be changed as long as it returns the same type of value it expects. 
    /// An example of this is if a user doesn't want to use StateManager for Health Management so they can plug in their own health component property here.
    /// Then when ABC calls this interface to reduce health it will simply effect the new component. 
    /// This also allows for different target systems to be used if desired. As long as these methods returns the same type of property then any of this class can be changed and it will still easily fit in with the rest of ABC.
    /// 
    /// Originator is a IEntity property which is used in many functions and generally means the entity which activates the ability. 
    /// 
    /// If you are making amendments it is recommend to override the methods in another ABC_IEntity partial class (which are inherited from the base class) so any future updates don't overwrite your changes: https://docs.microsoft.com/en-us/visualstudio/modeling/overriding-and-extending-the-generated-classes?view=vs-2019
    /// </remarks>
    [System.Serializable]
    public partial class ABC_IEntity : ABC_IEntityBase {

        /// <summary>
        /// Constructor to make the entity object. Add other scripts here if required. 
        /// </summary>
        /// <param name="Obj">object to be created into an entity object</param>
        /// <param name="StaticTracking">If true then the entity object will be added to a global static dictionary which can be retrieved later, stopping the need to make another IEntity for this gameobject</param>
        public ABC_IEntity(GameObject Obj, bool StaticTracking = true) : base(Obj, StaticTracking) {

            //Setup the Entity finding and storing all components to be referenced and used later on. 
            this.SetupEntity(Obj, this);

            //If Object has any ABC scripts then Track the object in global Utilities code to be retrieved later for performance 
            if (StaticTracking && (this._entitySM != null || this._entityABC != null))
                ABC_Utilities.AddStaticABCEntity(Obj, this);

        }


    }
}
