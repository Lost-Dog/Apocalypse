using System;
using GameCreator.Runtime.Characters;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.Common.UnityUI;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace GameCreator.Runtime.Shooter
{
    [Icon(EditorPaths.PACKAGES + "Shooter/Editor/Gizmos/GizmoAmmoUI.png")]
    [DefaultExecutionOrder(ApplicationManager.EXECUTION_ORDER_LAST_LATER)]
    
    [AddComponentMenu("Game Creator/UI/Shooter/Ammo UI")]
    [Serializable]
    public class AmmoUI : MonoBehaviour
    {
        private const string INFINITE = "INFINITE";
        
        // EXPOSED MEMBERS: -----------------------------------------------------------------------
        
        [SerializeField] private PropertyGetGameObject m_Character = GetGameObjectPlayer.Create();
        [SerializeField] private PropertyGetWeapon m_Weapon = GetWeaponShooterInstance.Create();

        [SerializeField] private TextReference m_InMagazine = new TextReference();
        [SerializeField] private TextReference m_InPouch = new TextReference();
        [SerializeField, FormerlySerializedAs("m_InMunition")]
        private TextReference m_InTotal = new TextReference();

        [SerializeField] private Image m_MagazineFill;
        
        [SerializeField] private GameObject m_PrefabInMagazine;
        [SerializeField] private RectTransform m_ContentInMagazine;

        [SerializeField] private GameObject m_ActiveIfEmpty;
        [SerializeField] private GameObject m_ActiveIfFull;
        
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
            Character character = this.m_Character.Get<Character>(this.gameObject);
            if (character == null) return;
            
            ShooterWeapon weapon = this.m_Weapon.Get(this.gameObject) as ShooterWeapon;
            if (weapon == null) return;

            TMunitionValue munition = character.Combat.RequestMunition(weapon);
            if (munition is not ShooterMunition shooterMunition) return;
            
            GameObject prop = character.Combat.RequestStance<ShooterStance>().Get(weapon)?.Prop;

            if (this.m_Args.Self != character.gameObject)
            {
                this.m_Args.ChangeSelf(character.gameObject);
            }

            if (this.m_Args.Target != prop)
            {
                this.m_Args.ChangeTarget(prop);   
            }
            
            int inMagazine = shooterMunition.InMagazine;
            int inTotal = weapon.Magazine.GetTotalAmmo(this.m_Args); 
            int inPouch = Mathf.Max(0, inTotal - inMagazine);
            
            this.m_InMagazine.Text = shooterMunition.InMagazine.ToString();
            this.m_InPouch.Text = inTotal >= int.MaxValue ? INFINITE : inPouch.ToString();
            this.m_InTotal.Text = inTotal >= int.MaxValue ? INFINITE : inTotal.ToString();

            int magazineSize = weapon.Magazine.GetHasMagazine(this.m_Args)
                ? weapon.Magazine.GetMagazineSize(this.m_Args)
                : 0;
            
            float ratio = magazineSize >= 1 
                ? inMagazine / (float) magazineSize 
                : 1f;

            if (this.m_MagazineFill != null)
            {
                this.m_MagazineFill.fillAmount = ratio;
            }

            if (this.m_ContentInMagazine != null)
            {
                RectTransformUtils.RebuildChildren(
                    this.m_ContentInMagazine,
                    this.m_PrefabInMagazine,
                    inMagazine
                );
            }

            if (this.m_ActiveIfEmpty != null)
            {
                this.m_ActiveIfEmpty.SetActive(ratio <= float.Epsilon);
            }
            
            if (this.m_ActiveIfFull != null)
            {
                this.m_ActiveIfFull.SetActive(ratio >= 1f);
            }
        }
    }
}
