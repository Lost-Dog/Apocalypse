using UnityEngine;
using JUTPS.JUInputSystem;
using JUTPS.CrossPlataform;
using JUTPS.ActionScripts;
using JUTPS.InteractionSystem;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace JUTPS.UI
{
    public class MobileRig : MonoBehaviour
    {
        /// <summary>
        /// Contains mobile controls for character controllers.
        /// </summary>
        [System.Serializable]
        public struct CharacterControlsSettings
        {
            /// <summary>
            /// Show the <see cref="ShotButton"/> only if is holding an item?
            /// </summary>
            public bool ShowShotButtonIfHaveItem;

            /// <summary>
            /// The shot button.
            /// </summary>
            public JUButtonVirtual ShotButton;

            /// <summary>
            /// The punch button.
            /// </summary>
            public JUButtonVirtual PunchButton;

            /// <summary>
            /// The aim button.
            /// </summary>
            public JUButtonVirtual AimingButton;

            /// <summary>
            /// The reload button.
            /// </summary>
            public JUButtonVirtual ReloadButton;

            /// <summary>
            /// The interaction button.
            /// </summary>
            public JUButtonVirtual InteractButton;

            /// <summary>
            /// The enter/exit vehicle button.
            /// </summary>
            public JUButtonVirtual EnterVehicleButton;
        }

        /// <summary>
        /// The container that contains all mobile controls.
        /// </summary>
        [Header("Panels")]
        public GameObject MobileScreenPanel;

        /// <summary>
        /// The container that contains only mobile controls for characters.
        /// </summary>
        public GameObject CharacterControlsPanel;

        /// <summary>
        /// The container that contains only mobile controls for vehicles.
        /// </summary>
        public GameObject VehicleControlsPanel;

        /// <summary>
        /// Contains mobile controls for character controllers.
        /// </summary>
        public CharacterControlsSettings CharacterControls;

        public MobileRig()
        {
            CharacterControls = new CharacterControlsSettings();
            CharacterControls.ShowShotButtonIfHaveItem = true;
        }

        internal void FindButtonsAndTouches()
        {
            //Screen Panels

            MobileScreenPanel = FindGameObject(new string[]
            {
                "Mobile Screen",
                "MobileScreen",
                "Mobile",
                "Screen Mobile",
                "ScreenMobile",
                "mobile screen",
                "mobileScreen",
                "mobile",
                "screen mobile",
                "screenMobile",
            });

            CharacterControlsPanel = FindGameObject(new string[]
            {
                "Mobile Character Controls",
                "Mobile Character",
                "Normal Mobile Screen",
                "Normal Mobile Screen Panel",
                "Character Mobile Controls",
                "Character Controls Mobile",
                "mobile character controls",
                "mobile character",
                "normal mobile screen",
                "normal mobile screen panel",
                "character mobile controls",
                "character controls mobile",
            });

            VehicleControlsPanel = FindGameObject(new string[]
            {
                "Vehicle Mobile Screen",
                "Vehicle Mobile Controls",
                "Vehicle Controls Mobile",
                "Driving Mobile screen",
                "Driving Mobile Screen Panel",
                "vehicle mobile screen",
                "vehicle mobile controls",
                "vehicle controls mobile",
                "driving mobile screen",
                "driving mobile Screen panel"
            });

            //Controll Buttons

            CharacterControls.ShotButton = FindComponent<JUButtonVirtual>(new string[]
             {
                "Shot",
                "ShotButton",
                "Shot Button",
                "Button Shot",
                "Shooting",
                "Shooting Button",
                "ShootingButton",
                "Shoot Button",
                "ShotButton",
                "Button Shoot",
                "ShootButton",
                "shot",
                "shotButton",
                "shot button",
                "button shot",
                "shooting",
                "shooting button",
                "shootingbutton",
                "shoot button",
                "shootButton",
                "button shoot",
                "shootButton",
             });

            CharacterControls.PunchButton = FindComponent<JUButtonVirtual>(new string[]
              {
                "Punch",
                "PunchButton",
                "Punch Button",
                "Button Punch",
                "punch button",
                "punchButton",
                "button punch",
                "punch",
              });

            CharacterControls.AimingButton = FindComponent<JUButtonVirtual>(new string[]
            {
                "Aiming",
                "AimingButton",
                "Aiming Button",
                "Aim Button",
                "AimButton",
                "Button Aim",
                "ButtonAim",
                "aimingButton",
                "aiming button",
                "button aim",
                "buttonAim",
                "aim",
            });

            CharacterControls.ReloadButton = FindComponent<JUButtonVirtual>(new string[]
            {
                "Reload",
                "ReloadButton",
                "Reload Button",
                "Button Reload",
                "reload",
                "reloadButton",
                "reload button",
                "button reload"
            });

            //Interact Buttons

            CharacterControls.InteractButton = FindComponent<JUButtonVirtual>(new string[]
            {
                "Interact",
                "InteractButton",
                "Interact Button",
                "Button Interact",
                "interact",
                "interact button",
                "interactButton",
                "button interact"
            });

            CharacterControls.EnterVehicleButton = FindComponent<JUButtonVirtual>(new string[]
            {
                "VehicleButton",
                "Enter Vehicle",
                "Enter Vehicle Button",
                "Enter The Vehicle Button",
                "Vehicle Button",
                "enter vehicle",
                "enter vehicle button",
                "enter the vehicle button",
                "vehicle button",
            });
        }

        private GameObject FindGameObject(string[] names)
        {
            for (int i = 0; i < names.Length; i++)
            {
                GameObject obj = GameObject.Find(names[i]);
                if (obj)
                {
                    return obj;
                }
            }

            return null;
        }

        private T FindComponent<T>(string[] names) where T : MonoBehaviour
        {
            for (int i = 0; i < names.Length; i++)
            {
                GameObject obj = GameObject.Find(names[i]);
                if (obj)
                {
                    T component = obj.GetComponent<T>();
                    if (component)
                    {
                        return component;
                    }
                }
            }

            return null;
        }

        private void Update()
        {
            if (JUGameManager.IsMobileControls)
            {
                MobileScreenPanel.SetActive(true);
                UpdateMobileScreens();
                UpdateMobileButtons();
            }
            else
            {
                MobileScreenPanel.SetActive(false);
            }
        }

        private void UpdateMobileScreens()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            MobileScreenPanel.SetActive(JUGameManager.IsMobileControls);
            CharacterControlsPanel.SetActive(!JUGameManager.PlayerController.IsDriving);
            VehicleControlsPanel.SetActive(JUGameManager.PlayerController.IsDriving);
        }

        private void UpdateMobileButtons()
        {
            if (CharacterControls.InteractButton)
            {
                bool showButton = JUGameManager.PlayerController.Inventory ? JUGameManager.PlayerController.Inventory.ItemToPickUp : false;
                CharacterControls.InteractButton.gameObject.SetActive(showButton);
            }

            if (CharacterControls.ReloadButton)
            {
                bool showButton = JUGameManager.PlayerController.RightHandWeapon || JUGameManager.PlayerController.LeftHandWeapon;
                CharacterControls.ReloadButton.gameObject.SetActive(showButton);
            }

            if (CharacterControls.AimingButton)
            {
                bool showButton = JUGameManager.PlayerController.RightHandWeapon;
                CharacterControls.AimingButton.gameObject.SetActive(showButton);
            }

            if (CharacterControls.ShotButton)
            {
                bool showButton = CharacterControls.ShowShotButtonIfHaveItem ? JUGameManager.PlayerController.IsItemEquiped : true;
                CharacterControls.ShotButton.gameObject.SetActive(showButton);
            }
            if (CharacterControls.PunchButton)
            {
                bool showButton = true;

                if (CharacterControls.ShotButton && CharacterControls.ShotButton.gameObject.activeSelf)
                {
                    showButton = false;
                }

                CharacterControls.PunchButton.gameObject.SetActive(showButton);
            }

            // Enter/Exit vehicle button.
            if (CharacterControls.EnterVehicleButton && JUGameManager.PlayerController.TryGetComponent<DriveVehicles>(out var driver))
            {
                if (driver.IsDriving)
                {
                    CharacterControls.EnterVehicleButton.gameObject.SetActive(driver.CanExitVehicle);
                }
                else
                {
                    JUInteractionSystem interactionSystem = JUGameManager.PlayerController.GetComponent<JUInteractionSystem>();
                    bool showButton = interactionSystem && interactionSystem.CanInteract(interactionSystem.NearestInteractable);
                    CharacterControls.EnterVehicleButton.gameObject.SetActive(showButton);
                }
            }
            else if (CharacterControls.EnterVehicleButton)
            {
                CharacterControls.EnterVehicleButton.gameObject.SetActive(false);
            }
        }
    }
}

namespace JUTPS.CustomEditors
{
#if UNITY_EDITOR
    using MobileRig = JUTPS.UI.MobileRig;

    /// <summary>
    /// Custom editor for <see cref="MobileRig"/>.
    /// </summary>
    [CustomEditor(typeof(MobileRig))]
    public class MobileRigEditor : Editor
    {
        private static readonly string[] _dontIncludeMe = new string[] { "m_Script" };

        /// <inheritdoc/>
        public override void OnInspectorGUI()
        {
            MobileRig mobileRig = (MobileRig)target;

            serializedObject.Update();

            if (GUILayout.Button(" ► Auto Setup", GUILayout.Height(30)))
            {
                mobileRig.FindButtonsAndTouches();
                EditorUtility.SetDirty(mobileRig);
            }
            DrawPropertiesExcluding(serializedObject, _dontIncludeMe);
            serializedObject.ApplyModifiedProperties();
        }
    }
#endif
}
