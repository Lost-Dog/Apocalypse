#if ABC_GC_2_Integration

using System.Collections;
using System.Collections.Generic;
using GameCreator.Runtime.Characters;
using UnityEngine;
using System.Linq;
using GameCreator.Runtime.Characters.IK;

#if ABC_GC_2_Stats_Integration
using GameCreator.Runtime.Stats;
#endif

namespace ABCToolkit {
public class ABC_GameCreator2Utilities
{


    //************************ Public Properties ****************************************


#region  Public Properties


#endregion


    //************************ Private Properties ****************************************


#region Private Properties

    /// <summary>
    /// ABC entity attached to this utility
    /// </summary>
    private ABC_IEntity abcEntity;

    /// <summary>
    /// Game Creator 2 character component
    /// </summary>
    private GameCreator.Runtime.Characters.Character gc2Character;

#if ABC_GC_2_Stats_Integration
    /// Game Creator 2 traits component
    private GameCreator.Runtime.Stats.Traits gc2Traits;
#endif

#endregion


    //************************ Private Methods ****************************************


#region Private Methods


#endregion


    //************************ Public Methods ****************************************


#region Public Methods

    public ABC_GameCreator2Utilities(ABC_IEntity Entity) {
        this.abcEntity = Entity;
        this.gc2Character = ABC_Utilities.TraverseObjectForComponent(Entity.gameObject, typeof(GameCreator.Runtime.Characters.Character)) as GameCreator.Runtime.Characters.Character;

#if ABC_GC_2_Stats_Integration
        this.gc2Traits = ABC_Utilities.TraverseObjectForComponent(Entity.gameObject, typeof(GameCreator.Runtime.Stats.Traits)) as GameCreator.Runtime.Stats.Traits;
#endif

    }

    /// <summary>
    /// Will run a GC2 Action
    /// </summary>
    /// <param name="GC2Action">GC2 Action to run</param>
    /// <param name="Args">Arguments to pass to the action</param>
    public void RunGC2Action(GameCreator.Runtime.VisualScripting.Actions GC2Action, GameCreator.Runtime.Common.Args Args) {

        //Run the G2 Action
        GC2Action?.Run(Args);

    }

    /// <summary>
    /// will return if the entity has a GC2 character component attached, else false
    /// </summary>
    /// <returns>True  if the entity has a GC2 character component attached, else false</returns>
    public bool HasGC2CharacterComponent() {

        if (this.gc2Character != null)
            return true;
        else
            return false; 

    }


    /// <summary>
    /// Will set the GC locomotion state to the character component
    /// </summary>
    /// <param name="State">State to set</param>
    /// <param name="Delay">Delay before state is set</param>
    /// <param name="Speed">The speed of the transition</param>
    /// <param name="Weight">The weight of the state</param>
    /// <param name="Transition">The transition time when setting the state</param>    
    public void SetGC2State(GameCreator.Runtime.Characters.State State, float Delay, float Speed, float Weight, float Transition, int Layer) {


        if (this.gc2Character == null)
            return;

        //If null then stop motion
        if (State == null) {
            this.gc2Character.States.Stop(Layer, Delay, Transition);
            return; 
        }

        GameCreator.Runtime.Characters.ConfigState configuration = new GameCreator.Runtime.Characters.ConfigState(
          Delay, Speed, Weight,
          Transition, Transition
      );

        _= this.gc2Character.States.SetState(
            State, Layer,
            GameCreator.Runtime.Characters.BlendMode.Blend, configuration
        );

    }

    /// <summary>
    /// Will modify the terminal velocity of the GC 2 motion
    /// </summary>
    /// <param name="Velocity">Velocity value to set</param>
    public void SetGC2TerminalVelocity(float Velocity) {

        if (this.gc2Character != null)
            this.gc2Character.Motion.TerminalVelocity = Velocity;
    }

    /// <summary>
    /// Will return the rotation/angular speed of the Game Creator
    /// </summary>
    /// <returns>Float representing the angulat speed of the Game Creator character</returns>
    public float GetGC2MotionAngularSpeed() {

        if (this.gc2Character != null)
            return this.gc2Character.Motion.AngularSpeed;
        else
            return 0;

    }

    /// <summary>
    /// Will move the GC 2 character by the vector 3 provided
    /// </summary>
    /// <param name="Velocity">Vector3 Velocity to move GC 2 character by</param>
    /// <param name="Space">Space Type</param>
    /// <param name="Priority">Priority for the move</param>
    public void MoveGC2ToDirection(Vector3 Velocity, Space Space, int Priority = 0) {
        
        if (this.gc2Character != null)
            this.gc2Character.Motion.MoveToDirection(Velocity, Space, Priority);
    }

    /// <summary>
    /// (ABC Integration) Returns the max value for the GC 2 attribute
    /// </summary>
    /// <param name="AttributeID">ID of the attribute to get the max value for</param>
    /// <returns>Float value representing the max value of the attribute</returns>
    public float GetGC2MaxAttributeValue(string AttributeID) {

#if ABC_GC_2_Stats_Integration
        if (this.gc2Traits == null) {
            Debug.Log("Game Creator 2 Stats - Trait component not found");
            return 1;
        }

        return (float)this.gc2Traits.RuntimeAttributes.Get(AttributeID).MaxValue;
#endif

        Debug.Log("Game Creator 2 Stats Integration is not correctly setup.");

        return 1;
    }


    /// <summary>
    /// (ABC Integration) Returns the current value for the GC 2 attribute/stat
    /// </summary>
    /// <param name="StatID">ID of the stat/attribute to get the current value for</param>
    /// <param name="GCStatType">The GC 2 stat type: Stat or Attribute</param>
    /// <returns>Float value representing the current value of the stat/attribute</returns>
    public float GetGC2StatValue(string StatID, GCStatType GCStatType = GCStatType.Stat) {

#if ABC_GC_2_Stats_Integration

        if (this.gc2Traits == null) {
            Debug.Log("Game Creator 2 Stats - Trait component not found");
            return 1;
        }

        switch (GCStatType) {
            case GCStatType.Attribute:
                return (float)this.gc2Traits.RuntimeAttributes.Get(StatID).Value;
            case GCStatType.Stat:
                return (float)this.gc2Traits.RuntimeStats.Get(StatID).Value;
        }

#endif

        Debug.Log("Game Creator 2 Stats Integration is not correctly setup.");
        return 1;
    }


    /// <summary>
    /// (ABC Integration) Will set a Game Creator 2 stat/attribute value by the amount provided
    /// </summary>
    /// <param name="StatID">ID of Stat which will have its value modified</param>
    /// <param name="Amount">Amount to increase or decrease the stat value by</param>
    /// <param name="GCStatType">The GC stat type: Stat or Attribute</param>
    public void SetGC2StatValue(string StatID, float Value, GCStatType GCStatType = GCStatType.Stat) {

#if ABC_GC_2_Stats_Integration

        if (this.gc2Traits == null)
            Debug.Log("Game Creator 2 Stats - Trait component not found");


        switch (GCStatType) {
            case GCStatType.Attribute:
                this.gc2Traits.RuntimeAttributes.Get(StatID).Value = Value;
                break;
            case GCStatType.Stat:
                this.gc2Traits.RuntimeStats.Get(StatID).Base = Value;
                break;
        }

        return;

#endif

        Debug.Log("Game Creator Stats Integration is not correctly setup.");
    }


    /// <summary>
    /// (ABC Integration) Will adjust a Game Creator 2 stats value by the amount provided
    /// </summary>
    /// <param name="StatID">ID of Stat which will have its value modified</param>
    /// <param name="Amount">Amount to increase or decrease the stat value by</param>
    /// <param name="GCStatType">The GC 2 stat type: Stat or Attribute</param>
    /// <param name="Modifier">(Stat only) If true then GC2 stat will have a modifier added rather then the base value changed </param>
    public void AdjustGC2StatValue(string StatID, float Value, GCStatType GCStatType = GCStatType.Stat, bool Modifier = true) {

#if ABC_GC_2_Stats_Integration

        if (this.gc2Traits == null)
            Debug.Log("Game Creator 2 Stats - Trait component not found");

        switch (GCStatType) {
            case GCStatType.Attribute:
                this.gc2Traits.RuntimeAttributes.Get(StatID).Value += Value;
                break;
            case GCStatType.Stat:

                if (Modifier == false)
                    this.gc2Traits.RuntimeStats.Get(StatID).Base += Value;
                else
                    this.gc2Traits.RuntimeStats.Get(StatID).AddModifier(ModifierType.Constant, Value);
                break;
        }

        return;

#endif

        Debug.Log("Game Creator 2 Integration is not correctly setup.");
    }

    /// <summary>
    /// Will enable/disable the rotation on the game creator 2 character component
    /// </summary>
    /// <param name="Enabled">True to allow rotation, else false to disable</param>
    public void AllowGC2Rotation(bool Enabled) {

        //AllowRotation Flag no longer exists in GC2
        //if (this.gc2Character != null)
        //    this.gc2Character.Facing.AllowRotation = Enabled;



    }


    /// <summary>
    /// Will update if the player is controllerable
    /// </summary>
    /// <param name="IsControllerable">True if player is controllerable, else false</param>
    public void SetGC2CharacterIsControllerable(bool IsControllerable) {

        if (this.gc2Character != null) {

            //Disable control
            this.gc2Character.Player.IsControllable = IsControllerable;

            //Disable IK
            if (this.gc2Character.IK.HasRig<RigFeetPlant>())
                this.gc2Character.IK.GetRig<RigFeetPlant>().IsActive = IsControllerable;
            
        }

    }

#endregion



}
}
#endif
