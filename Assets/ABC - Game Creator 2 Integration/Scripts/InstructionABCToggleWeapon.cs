
#if ABC_GC_2_Integration
namespace GameCreator.Core {
    using System;
    using System.Threading.Tasks;
    using UnityEngine;
    using ABCToolkit;
    using GameCreator.Runtime.Characters;
    using GameCreator.Runtime.Common;
    using GameCreator.Runtime.VisualScripting;


#if UNITY_EDITOR
    using UnityEditor;
#endif

    public enum ABCToggleWeaponState {
        Enable,
        Disable,
        Toggle, 
        Equip, 
        ImportGlobal
    }

    [Version(0, 1, 1)]

    [Title("Toggle ABC Weapon")]
    [Category("ABC/Toggle Weapon")]

    [Image(typeof(IconCircleSolid), ColorTheme.Type.Blue, typeof(IconDice))]
    [Description("Enables/Disables/Equips or Imports an ABC Weapon")]

    [Parameter("Target", "The targeted game object to toggle weapon on")]
    [Parameter("Toggle State", "The state to set the weapon to: enable, disable, toggle from the current value to the other, equip or import a global weapon")]

    [Keywords("ABC", "Ability", "Integration", "Combat", "Magic", "Attack", "Weapon", "Enable", "Disable")]


    [Serializable]
    public class InstructionABCToggleWeapon: Instruction {


        [SerializeField] private PropertyGetGameObject m_Target = GetGameObjectPlayer.Create();

        [SerializeField] private ABCToggleWeaponState m_ToggleState = ABCToggleWeaponState.Enable;

        [SerializeField] private string m_WeaponReference;



        public override string Title => string.Format(
            "{2} ABC Weapon - {0} on {1}",
            this.m_WeaponReference != null
                ? this.m_WeaponReference.ToString()
                : "(none)",
            this.m_Target,
            m_ToggleState.ToString()
        );



        protected override Task Run(Args args) {
            GameObject target = this.m_Target.Get(args);

            ABC_IEntity entity = ABC_Utilities.GetStaticABCEntity(target);

            if (entity == null) {
                Debug.Log("Entity not found");
            }

            if (entity == null) return DefaultResult;

            //If StateManager or ABC controller object previously recorded is disabled then refresh to grab the latest references (new object may now have the components changed during play)
            if (entity != null && (entity.HasABCController() == false || entity.HasABCStateManager() == false))
                entity.ReSetupEntity();



            switch (m_ToggleState) {
                case ABCToggleWeaponState.Enable:

                    if (int.TryParse(this.m_WeaponReference, out int a))
                        entity.EnableWeapon(int.Parse(this.m_WeaponReference));
                    else
                        entity.EnableWeapon(this.m_WeaponReference);

                    break;
                case ABCToggleWeaponState.Disable:

                    if (int.TryParse(this.m_WeaponReference, out int b))
                        entity.DisableWeapon(int.Parse(this.m_WeaponReference));
                    else
                        entity.DisableWeapon(this.m_WeaponReference);

                    break;
                case ABCToggleWeaponState.Toggle:

                    if (int.TryParse(this.m_WeaponReference, out int c))
                        entity.ToggleWeaponEnableState(int.Parse(this.m_WeaponReference));
                    else
                        entity.ToggleWeaponEnableState(this.m_WeaponReference);

                    break;
                case ABCToggleWeaponState.Equip:

                    if (int.TryParse(this.m_WeaponReference, out int d))
                        entity.EquipWeapon(int.Parse(this.m_WeaponReference));
                    else
                        entity.EquipWeapon(this.m_WeaponReference);

                    break;
                case ABCToggleWeaponState.ImportGlobal:

                    if (int.TryParse(this.m_WeaponReference, out int e) == false)
                        ABC_Utilities.mbSurrogate.StartCoroutine(entity.AddGlobalElementAtRunTime(this.m_WeaponReference));
                    else
                        Debug.LogWarning("ABC & GC - Integration: Unable to add global weapons using weapon ID, please enter weapon name in dropdown");
           

                    break;
            }

            return DefaultResult;
        }


    

    }
}
#endif