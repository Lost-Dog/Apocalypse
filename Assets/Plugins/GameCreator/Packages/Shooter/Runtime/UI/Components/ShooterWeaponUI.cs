using System;
using GameCreator.Runtime.Characters;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.Common.UnityUI;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace GameCreator.Runtime.Shooter
{
    [Icon(EditorPaths.PACKAGES + "Shooter/Editor/Gizmos/GizmoShooterWeaponUI.png")]
    [DefaultExecutionOrder(ApplicationManager.EXECUTION_ORDER_LAST_LATER)]
    
    [AddComponentMenu("Game Creator/UI/Shooter/Shooter Weapon UI")]
    [Serializable]
    public class ShooterWeaponUI : MonoBehaviour
    {
        // EXPOSED MEMBERS: -----------------------------------------------------------------------
        
        [SerializeField] private PropertyGetGameObject m_Character = GetGameObjectPlayer.Create();
        
        [SerializeField] private TextReference m_WeaponName = new TextReference();
        [SerializeField] private TextReference m_WeaponDescription = new TextReference();
        [SerializeField] private Image m_WeaponIcon;
        [SerializeField] private Image m_WeaponColor;
        
        [SerializeField] private GameObject m_ActiveHasWeapon;
        [SerializeField] private GameObject m_ActiveHasNoWeapon;
        
        // MEMBERS: -------------------------------------------------------------------------------

        [NonSerialized] private Args m_Args;
        
        // INITIALIZERS: --------------------------------------------------------------------------

        private void OnEnable()
        {
             this.m_Args = new Args(this.gameObject);
        }
        
        // UPDATE METHODS: ------------------------------------------------------------------------
        
        private void Update()
        {
            ShooterWeapon weapon = null;
            
            Character character = this.m_Character.Get<Character>(this.gameObject);
            if (character != null)
            {
                weapon = character.Combat.GetActiveWeapon<ShooterWeapon>();
                GameObject prop = character.Combat.RequestStance<ShooterStance>().Get(weapon).Prop;
                
                if (this.m_Args.Self != character.gameObject)
                {
                    this.m_Args.ChangeSelf(character.gameObject);
                }

                if (this.m_Args.Target != prop)
                {
                    this.m_Args.ChangeTarget(prop);   
                }
            }
            
            bool hasWeapon = weapon != null;
            
            if (hasWeapon)
            {
                this.m_WeaponName.Text = weapon.GetName(this.m_Args);
                this.m_WeaponDescription.Text = weapon.GetDescription(this.m_Args);
                if (this.m_WeaponIcon != null) this.m_WeaponIcon.overrideSprite = weapon.GetSprite(this.m_Args);
                if (this.m_WeaponColor != null) this.m_WeaponColor.color = weapon.GetColor(this.m_Args);
            }
            else
            {
                this.m_WeaponName.Text = string.Empty;
                this.m_WeaponDescription.Text = string.Empty;
                if (this.m_WeaponIcon != null) this.m_WeaponIcon.overrideSprite = null;
                if (this.m_WeaponColor != null) this.m_WeaponColor.color = Color.white;
            }

            if (this.m_ActiveHasWeapon != null)
            {
                this.m_ActiveHasWeapon.SetActive(hasWeapon);
            }
            
            if (this.m_ActiveHasNoWeapon != null)
            {
                this.m_ActiveHasNoWeapon.SetActive(!hasWeapon);
            }
        }
    }
}
