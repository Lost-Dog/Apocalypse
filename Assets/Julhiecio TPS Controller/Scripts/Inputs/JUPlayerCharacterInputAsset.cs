using UnityEngine;
using UnityEngine.InputSystem;

namespace JUTPS.JUInputSystem
{
    /// <summary>
    /// Contains all input values to control <see cref="JUPlayerCharacterInputAsset"/> players.
    /// </summary>
    [CreateAssetMenu(fileName = "Player Character Inputs", menuName = "JU TPS/Inputs/Player Character Inputs")]
    public class JUPlayerCharacterInputAsset : ScriptableObject
    {
        /// <summary>
        /// Character movement actions.
        /// </summary>
        [System.Serializable]
        public class MovementActions
        {
            /// <summary>
            /// Usefull for mobile sidescroller games.
            /// </summary>
            public bool MobileShotWhenAiming;

            /// <summary>
            /// The action that contains the inputs to move the player on mobile.
            /// </summary>
            [Header("Mobile Move Action")]
            public InputAction MobileMoveAction;

            /// <summary>
            /// The action that contains the axis to look to a direction on mobile.
            /// </summary>
            [Header("Mobile Look Action")]
            public InputAction MobileLookAction;

            /// <summary>
            /// The action that contains the buttons to player shot/attack/punch on mobile.
            /// </summary>
            [Header("Mobile Attack Action")]
            public InputAction MobileAttackAction;

            /// <summary>
            /// The action that contains the keys/buttons to move the player.
            /// </summary>
            [Header("Move Action")]
            public InputAction MoveAction;

            /// <summary>
            /// The action that contains the axis to look to a direction.
            /// </summary>
            [Header("Look Action")]
            public InputAction LookAction;

            /// <summary>
            /// The action that contains the keys/buttons to player jump.
            /// </summary>
            [Header("Jump Action")]
            public InputAction JumpAction;

            /// <summary>
            /// The action that contains the keys/buttons to player run.
            /// </summary>
            [Header("Run Action")]
            public InputAction RunAction;

            /// <summary>
            /// The action that contains the keys/buttons to player punch.
            /// </summary>
            [Header("Punch Action")]
            public InputAction PunchAction;

            /// <summary>
            /// The action that contains the keys/buttons to player attack using melee weapons.
            /// </summary>
            [Header("Melee Weapon Attack Action")]
            public InputAction MeleeWeaponAttackAction;

            /// <summary>
            /// The action that contains the keys/buttons to player shot.
            /// </summary>
            [Header("Shot Action")]
            public InputAction ShotAction;

            /// <summary>
            /// The action that contains the keys/buttons to player aim on a target.
            /// </summary>
            [Header("Aim Action")]
            public InputAction AimAction;

            /// <summary>
            /// The action that contains the keys/buttons to reload weapon.
            /// </summary>
            [Header("Reload Action")]
            public InputAction ReloadAction;

            /// <summary>
            /// The action that contains the keys/buttons to crouch the player.
            /// </summary>
            [Header("Crouch Action")]
            public InputAction CrouchAction;

            /// <summary>
            /// The action that contains the keys/buttons to prone the player.
            /// </summary>
            [Header("Prone Action")]
            public InputAction ProneAction;

            /// <summary>
            /// The action that contains the keys/buttons to player roll.
            /// </summary>
            [Header("Roll Action")]
            public InputAction RollAction;

            /// <summary>
            /// Reset inputs.
            /// </summary>
            internal void Reset()
            {
                MobileMoveAction = new InputAction("Mobile Move");
                MobileMoveAction.AddBinding("<Gamepad>/leftStick");

                MobileLookAction = new InputAction("Mobile Look", type: InputActionType.Value, expectedControlType: "Vector2");
                MobileLookAction.AddBinding("<Mouse>/delta")
                    .WithProcessor("scaleVector2(x=1,y=0.6)");

                MobileAttackAction = new InputAction("Mobile Shot");
                MobileAttackAction.AddBinding("<Gamepad>/rightTrigger");

                MobileAttackAction = new InputAction("Mobile Shot");
                MobileAttackAction.AddBinding("<Gamepad>/rightTrigger");

                MoveAction = new InputAction("Move");
                MoveAction.AddCompositeBinding("2DVector")
                    .With("Up", "<Keyboard>/w")
                    .With("Down", "<Keyboard>/s")
                    .With("Left", "<Keyboard>/a")
                    .With("Right", "<Keyboard>/d")
                    .With("Up", "<Gamepad>/leftStick/up")
                    .With("Down", "<Gamepad>/leftStick/down")
                    .With("Left", "<Gamepad>/leftStick/left")
                    .With("Right", "<Gamepad>/leftStick/right");

                LookAction = new InputAction("Look", type: InputActionType.Value, expectedControlType: "Vector2");
                LookAction.AddBinding("<Gamepad>/rightStick")
                    .WithProcessor("scaleVector2(x=1,y=0.6)");

                LookAction.AddBinding("<Mouse>/delta")
                    .WithProcessor("scaleVector2(x=0.2,y=0.1)");

                JumpAction = new InputAction("Jump");
                JumpAction.AddBinding("<Keyboard>/space");
                JumpAction.AddBinding("<Gamepad>/buttonSouth");

                RunAction = new InputAction("Run");
                RunAction.AddBinding("<Keyboard>/shift");
                RunAction.AddBinding("<Gamepad>/leftStickPress");

                ShotAction = new InputAction("Shot");
                ShotAction.AddBinding("<Mouse>/leftButton");
                ShotAction.AddBinding("<Gamepad>/rightTrigger");

                PunchAction = new InputAction("Punch");
                PunchAction.AddBinding("<Mouse>/leftButton");
                PunchAction.AddBinding("<Gamepad>/buttonNorth");

                MeleeWeaponAttackAction = new InputAction("Melee Weapon Attack");
                MeleeWeaponAttackAction.AddBinding("<Mouse>/leftButton");
                MeleeWeaponAttackAction.AddBinding("<Gamepad>/buttonNorth");

                AimAction = new InputAction("Aim", type: InputActionType.Button);
                AimAction.AddBinding("<Gamepad>/leftTrigger");
                AimAction.AddBinding("<Mouse>/rightButton");

                ReloadAction = new InputAction("Reload");
                ReloadAction.AddBinding("<Keyboard>/r");
                ReloadAction.AddBinding("<Gamepad>/buttonWest");

                CrouchAction = new InputAction("Crouch");
                CrouchAction.AddBinding("<Keyboard>/c");
                CrouchAction.AddBinding("<Gamepad>/dpad/down");

                ProneAction = new InputAction("Prone");
                ProneAction.AddBinding("<Keyboard>/z")
                    .WithInteraction("press");

                RollAction = new InputAction("Roll", type: InputActionType.Button);
                RollAction.AddBinding("<Keyboard>/ctrl");
                RollAction.AddBinding("<Gamepad>/buttonEast");

                ProneAction.AddBinding("<Gamepad>/dpad/down")
                    .WithInteraction("hold");
            }
        }

        /// <summary>
        /// Item switching actions.
        /// </summary>
        [System.Serializable]
        public class ItemSwitchingActions
        {
            /// <summary>
            /// Allow switch for the next or previous item using actions.
            /// </summary>
            [Space]
            public bool ItemSwitchingEnabled;

            /// <summary>
            /// Allow switch the inventory item using the inventory slots.
            /// </summary>
            public bool SlotInputsEnabled;

            /// <summary>
            /// The action that contains the keys/buttons to equipe next inventory item.
            /// </summary>
            [Header("Equipe Next Item")]
            public InputAction EquipeNextItemAction;

            /// <summary>
            /// The action that contains the keys/buttons to equipe previous inventory item.
            /// </summary>
            [Header("Equipe Previous Item")]
            public InputAction EquipePreviousItemAction;

            /// <summary>
            /// The action that contains the keys/buttons to equipe the previous or next inventory item using a scroll.
            /// </summary>
            [Header("Equipe Previous or Next Item Using Scroll")]
            public InputAction EquipItemScrollAction;

            /// <summary>
            /// The action used to equip the item that is on the slot 1 of the character inventory if have.
            /// </summary>
            [Header("Slot Switching")]
            [Header("Equipe Slot 1")]
            public InputAction EquipSlot1Action;

            /// <summary>
            /// The action used to equip the item that is on the slot 2 of the character inventory if have.
            /// </summary>
            [Space]
            [Header("Equipe Slot 2")]
            public InputAction EquipSlot2Action;

            /// <summary>
            /// The action used to equip the item that is on the slot 3 of the character inventory if have.
            /// </summary>
            [Header("Equipe Slot 3")]
            public InputAction EquipSlot3Action;

            /// <summary>
            /// The action used to equip the item that is on the slot 4 of the character inventory if have.
            /// </summary>
            [Header("Equipe Slot 4")]
            public InputAction EquipSlot4Action;

            /// <summary>
            /// The action used to equip the item that is on the slot 5 of the character inventory if have.
            /// </summary>
            [Header("Equipe Slot 5")]
            public InputAction EquipSlot5Action;

            /// <summary>
            /// The action used to equip the item that is on the slot 6 of the character inventory if have.
            /// </summary>
            [Header("Equipe Slot 6")]
            public InputAction EquipSlot6Action;

            /// <summary>
            /// The action used to equip the item that is on the slot 7 of the character inventory if have.
            /// </summary>
            [Header("Equipe Slot 7")]
            public InputAction EquipSlot7Action;

            /// <summary>
            /// The action used to equip the item that is on the slot 8 of the character inventory if have.
            /// </summary>
            [Header("Equipe Slot 8")]
            public InputAction EquipSlot8Action;

            /// <summary>
            /// The action used to equip the item that is on the slot 9 of the character inventory if have.
            /// </summary>
            [Header("Equipe Slot 9")]
            public InputAction EquipSlot9Action;

            /// <summary>
            /// The action used to equip the item that is on the slot 10 of the character inventory if have.
            /// </summary>
            [Header("Equipe Slot 10")]
            public InputAction EquipSlot10Action;

            /// <summary>
            /// Reset inputs.
            /// </summary>
            internal void Reset()
            {
                ItemSwitchingEnabled = true;
                SlotInputsEnabled = true;

                EquipeNextItemAction = new InputAction("Equipe Next Item", InputActionType.Button);
                EquipeNextItemAction.AddBinding("<Keyboard>/e");
                EquipeNextItemAction.AddBinding("<Gamepad>/dpad/right");

                EquipePreviousItemAction = new InputAction("Equipe Previous Item", InputActionType.Button);
                EquipePreviousItemAction.AddBinding("<Keyboard>/q");
                EquipePreviousItemAction.AddBinding("<Gamepad>/dpad/left");

                EquipItemScrollAction = new InputAction("Equip Item Scroll", InputActionType.Value);
                EquipItemScrollAction.AddBinding("<Mouse>/scroll/y")
                    .WithProcessor("axisDeadzone(min=20, max=90)");

                EquipSlot1Action = new InputAction("Equipe Slot 1", InputActionType.Button);
                EquipSlot1Action.AddBinding("<Keyboard>/1");

                EquipSlot2Action = new InputAction("Equipe Slot 2", InputActionType.Button);
                EquipSlot2Action.AddBinding("<Keyboard>/2");

                EquipSlot3Action = new InputAction("Equipe Slot 3", InputActionType.Button);
                EquipSlot3Action.AddBinding("<Keyboard>/3");

                EquipSlot4Action = new InputAction("Equipe Slot 4", InputActionType.Button);
                EquipSlot4Action.AddBinding("<Keyboard>/4");

                EquipSlot5Action = new InputAction("Equipe Slot 5", InputActionType.Button);
                EquipSlot5Action.AddBinding("<Keyboard>/5");

                EquipSlot6Action = new InputAction("Equipe Slot 6", InputActionType.Button);
                EquipSlot6Action.AddBinding("<Keyboard>/6");

                EquipSlot7Action = new InputAction("Equipe Slot 7", InputActionType.Button);
                EquipSlot7Action.AddBinding("<Keyboard>/7");

                EquipSlot8Action = new InputAction("Equipe Slot 8", InputActionType.Button);
                EquipSlot8Action.AddBinding("<Keyboard>/8");

                EquipSlot9Action = new InputAction("Equipe Slot 9", InputActionType.Button);
                EquipSlot9Action.AddBinding("<Keyboard>/9");

                EquipSlot10Action = new InputAction("Equipe Slot 10", InputActionType.Button);
                EquipSlot10Action.AddBinding("<Keyboard>/0");
            }
        }

        /// <summary>
        /// The action that contains the keys/buttons to pick-up items or interact with objects.
        /// </summary>
        [Header("Interact Action")]
        public InputAction InteractAction;

        /// <summary>
        /// The action that contains the keys/buttons to open inventory screen.
        /// </summary>
        [Header("Open Inventory Action")]
        public InputAction OpenInventoryAction;

        /// <summary>
        /// Character movement actions.
        /// </summary>
        public MovementActions Movement;

        /// <summary>
        /// Item Switching actions.
        /// </summary>
        public ItemSwitchingActions ItemSwitching;

        /// <summary>
        /// The result direction to move action.
        /// </summary>
        public Vector2 MoveAxis
        {
            get
            {
                if (JUGameManager.IsMobileControls)
                    return Movement.MobileMoveAction.ReadValue<Vector2>().normalized;
                return Movement.MoveAction.ReadValue<Vector2>().normalized;
            }
        }

        /// <summary>
        /// The axis with turn view direction, like mouse delta or joystick direction.
        /// </summary>
        public Vector2 LookAxis
        {
            get
            {
                if (JUGameManager.IsMobileControls)
                    return Movement.MobileLookAction.ReadValue<Vector2>();
                return Movement.LookAction.ReadValue<Vector2>();
            }
        }

        /// <summary>
        ///  Return true if the jump action was pressend by first time.
        /// </summary>
        public bool IsJumpTriggered
        {
            get => Movement.JumpAction.WasPressedThisFrame();
        }

        /// <summary>
        /// Return true if is pressing the run action.
        /// </summary>
        public bool IsRunPressed
        {
            get => Movement.RunAction.inProgress;
        }

        /// <summary>
        /// Return true if the run action was pressend by first time.
        /// </summary>
        public bool IsRunTriggered
        {
            get => Movement.RunAction.WasPressedThisFrame();
        }

        /// <summary>
        /// Return true if the run was dropped.
        /// </summary>
        public bool IsRunPerformed
        {
            get => Movement.RunAction.phase == InputActionPhase.Performed;
        }

        /// <summary>
        /// Return true if is pressing shot action.
        /// </summary>
        public bool IsShotPressed
        {
            get
            {
                if (JUGameManager.IsMobileControls)
                {
                    if (Movement.MobileShotWhenAiming && Movement.MobileLookAction.inProgress)
                        return true;

                    return Movement.MobileAttackAction.inProgress;
                }

                return Movement.ShotAction.inProgress;
            }
        }

        /// <summary>
        /// Return true if the shot action was pressend by first time.
        /// </summary>
        public bool IsShotTriggered
        {
            get
            {
                if (JUGameManager.IsMobileControls)
                {
                    if (Movement.MobileShotWhenAiming && Movement.MobileLookAction.WasPressedThisFrame())
                        return true;

                    return Movement.MobileAttackAction.WasPressedThisFrame();
                }

                return Movement.ShotAction.WasPressedThisFrame();
            }
        }

        /// <summary>
        /// Return true if is pressing melee weapon attack action.
        /// </summary>
        public bool IsMeleeWeaponAttackPressed
        {
            get
            {
                if (JUGameManager.IsMobileControls)
                {
                    if (Movement.MobileShotWhenAiming && Movement.MobileLookAction.inProgress)
                        return true;

                    return Movement.MobileAttackAction.inProgress;
                }

                return Movement.MeleeWeaponAttackAction.inProgress;
            }
        }

        /// <summary>
        /// Return true if the melee weapon attack action was pressed by first time.
        /// </summary>
        public bool IsMeleeWeaponAttackTriggered
        {
            get
            {
                if (JUGameManager.IsMobileControls)
                {
                    if (Movement.MobileShotWhenAiming && Movement.MobileLookAction.inProgress)
                        return true;

                    return Movement.MobileAttackAction.inProgress;
                }

                return Movement.MeleeWeaponAttackAction.WasPressedThisFrame();
            }
        }

        /// <summary>
        /// Return true if is pressing the punch action.
        /// </summary>
        public bool IsPunchPressed
        {
            get
            {
                if (JUGameManager.IsMobileControls)
                {
                    if (Movement.MobileShotWhenAiming && Movement.MobileLookAction.inProgress)
                        return true;

                    return Movement.MobileAttackAction.inProgress;
                }

                return Movement.PunchAction.inProgress;
            }
        }

        /// <summary>
        /// Return true if the punch action was pressend by first time.
        /// </summary>
        public bool IsPunchTriggered
        {
            get
            {
                if (JUGameManager.IsMobileControls)
                {
                    if (Movement.MobileShotWhenAiming && Movement.MobileLookAction.WasPressedThisFrame())
                        return true;

                    return Movement.MobileAttackAction.WasPressedThisFrame();
                }

                return Movement.PunchAction.WasPressedThisFrame();
            }
        }

        /// <summary>
        /// Return true if the aim action was pressend by first time.
        /// </summary>
        public bool IsAimTriggered
        {
            get => Movement.AimAction.WasPressedThisFrame();
        }

        /// <summary>
        /// Return true if is pressing the aim action.
        /// </summary>
        public bool IsAimPressed
        {
            get => Movement.AimAction.IsPressed();
        }

        /// <summary>
        /// Return true if the <see cref="InteractAction"/> was pressend by first time.
        /// </summary>
        public bool IsInteractTriggered
        {
            get => InteractAction.WasPressedThisFrame();
        }

        /// <summary>
        /// Return true if is pressing <see cref="InteractAction"/>.
        /// </summary>
        public bool IsInteractPressed
        {
            get => InteractAction.inProgress;
        }

        /// <summary>
        /// Return true if the <see cref="ReloadAction"/> was pressend by first time.
        /// </summary>
        public bool IsReloadTriggered
        {
            get => Movement.ReloadAction.WasPressedThisFrame();
        }

        /// <summary>
        /// Return true if the <see cref="CrouchAction"/> was pressend by first time.
        /// </summary>
        public bool IsCrouchTriggered
        {
            get => Movement.CrouchAction.triggered;
        }

        /// <summary>
        /// Return true if the <see cref="ProneAction"/> was pressend by first time.
        /// </summary>
        public bool IsProneTriggered
        {
            get => Movement.ProneAction.triggered;
        }

        /// <summary>
        /// Return true if the <see cref="RollAction"/> was pressend by first time.
        /// </summary>
        public bool IsRollTriggered
        {
            get => Movement.RollAction.WasPressedThisFrame();
        }

        /// <summary>
        /// Return true if the <see cref="OpenInventoryAction"/> was pressend by first time.
        /// </summary>
        public bool IsOpenInventoryTriggered
        {
            get => OpenInventoryAction.WasPressedThisFrame();
        }

        /// <summary>
        /// Return true if was pressed the action to change to the next item.
        /// </summary>
        public bool IsEquipeNextItemTriggered
        {
            get
            {
                if (!ItemSwitching.ItemSwitchingEnabled)
                    return false;

                return ItemSwitching.EquipeNextItemAction.WasPerformedThisFrame() || ItemSwitching.EquipItemScrollAction.ReadValue<float>() > 0;
            }
        }

        /// <summary>
        /// Return true if was pressed the action to change to the previous item.
        /// </summary>
        public bool IsEquipePreviousItemTriggered
        {
            get
            {
                if (!ItemSwitching.ItemSwitchingEnabled)
                    return false;

                return ItemSwitching.EquipePreviousItemAction.WasPerformedThisFrame() || ItemSwitching.EquipItemScrollAction.ReadValue<float>() < 0;
            }
        }

        /// <summary>
        /// Return true if was pressed the action to change to the slot 1 (character inventory slot).
        /// </summary>
        public bool IsEquipSlot1Triggered
        {
            get => ItemSwitching.SlotInputsEnabled && ItemSwitching.EquipSlot1Action.WasPerformedThisFrame();
        }

        /// <summary>
        /// Return true if was pressed the action to change to the slot 2 (character inventory slot).
        /// </summary>
        public bool IsEquipSlot2Triggered
        {
            get => ItemSwitching.SlotInputsEnabled && ItemSwitching.EquipSlot2Action.WasPerformedThisFrame();
        }

        /// <summary>
        /// Return true if was pressed the action to change to the slot 3 (character inventory slot).
        /// </summary>
        public bool IsEquipSlot3Triggered
        {
            get => ItemSwitching.SlotInputsEnabled && ItemSwitching.EquipSlot3Action.WasPerformedThisFrame();
        }

        /// <summary>
        /// Return true if was pressed the action to change to the slot 4 (character inventory slot).
        /// </summary>
        public bool IsEquipSlot4Triggered
        {
            get => ItemSwitching.SlotInputsEnabled && ItemSwitching.EquipSlot4Action.WasPerformedThisFrame();
        }

        /// <summary>
        /// Return true if was pressed the action to change to the slot 5 (character inventory slot).
        /// </summary>
        public bool IsEquipSlot5Triggered
        {
            get => ItemSwitching.SlotInputsEnabled && ItemSwitching.EquipSlot5Action.WasPerformedThisFrame();
        }

        /// <summary>
        /// Return true if was pressed the action to change to the slot 6 (character inventory slot).
        /// </summary>
        public bool IsEquipSlot6Triggered
        {
            get => ItemSwitching.SlotInputsEnabled && ItemSwitching.EquipSlot7Action.WasPerformedThisFrame();
        }

        /// <summary>
        /// Return true if was pressed the action to change to the slot 6 (character inventory slot).
        /// </summary>
        public bool IsEquipSlot7Triggered
        {
            get => ItemSwitching.SlotInputsEnabled && ItemSwitching.EquipSlot7Action.WasPerformedThisFrame();
        }

        /// <summary>
        /// Return true if was pressed the action to change to the slot 8 (character inventory slot).
        /// </summary>
        public bool IsEquipSlot8Triggered
        {
            get => ItemSwitching.SlotInputsEnabled && ItemSwitching.EquipSlot8Action.WasPerformedThisFrame();
        }

        /// <summary>
        /// Return true if was pressed the action to change to the slot 9 (character inventory slot).
        /// </summary>
        public bool IsEquipSlot9Triggered
        {
            get => ItemSwitching.SlotInputsEnabled && ItemSwitching.EquipSlot9Action.WasPerformedThisFrame();
        }

        /// <summary>
        /// Return true if was pressed the action to change to the slot 10 (character inventory slot).
        /// </summary>
        public bool IsEquipSlot10Triggered
        {
            get => ItemSwitching.SlotInputsEnabled && ItemSwitching.EquipSlot10Action.WasPerformedThisFrame();
        }

        private void Reset()
        {
            Movement.Reset();
            ItemSwitching.Reset();

            InteractAction = new InputAction("Interact");
            InteractAction.AddBinding("<Keyboard>/f");
            InteractAction.AddBinding("<Gamepad>/buttonWest");

            OpenInventoryAction = new InputAction("Open Inventory", type: InputActionType.Button);
            OpenInventoryAction.AddBinding("<Gamepad>/dpad/up");
            OpenInventoryAction.AddBinding("<Keyboard>/tab");
        }

        private void OnEnable()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.playModeStateChanged += OnExitPlayMode;
#endif
        }

        private void OnDisable()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.playModeStateChanged -= OnExitPlayMode;
#endif
        }

        /// <summary>
        /// Set player inputs as enabled or disabled.
        /// </summary>
        /// <param name="active"></param>
        public void SetActiveInputs(bool active)
        {
            switch (active)
            {
                case true:
                    Movement.MobileMoveAction.Enable();
                    Movement.MobileLookAction.Enable();
                    Movement.MobileAttackAction.Enable();
                    Movement.MoveAction.Enable();
                    Movement.LookAction.Enable();
                    Movement.JumpAction.Enable();
                    Movement.RunAction.Enable();
                    Movement.ShotAction.Enable();
                    Movement.MeleeWeaponAttackAction.Enable();
                    Movement.PunchAction.Enable();
                    Movement.AimAction.Enable();
                    Movement.ReloadAction.Enable();
                    Movement.CrouchAction.Enable();
                    Movement.ProneAction.Enable();
                    Movement.RollAction.Enable();
                    InteractAction.Enable();
                    OpenInventoryAction.Enable();

                    ItemSwitching.EquipeNextItemAction.Enable();
                    ItemSwitching.EquipePreviousItemAction.Enable();
                    ItemSwitching.EquipItemScrollAction.Enable();
                    ItemSwitching.EquipSlot1Action.Enable();
                    ItemSwitching.EquipSlot2Action.Enable();
                    ItemSwitching.EquipSlot3Action.Enable();
                    ItemSwitching.EquipSlot4Action.Enable();
                    ItemSwitching.EquipSlot5Action.Enable();
                    ItemSwitching.EquipSlot6Action.Enable();
                    ItemSwitching.EquipSlot7Action.Enable();
                    ItemSwitching.EquipSlot8Action.Enable();
                    ItemSwitching.EquipSlot9Action.Enable();
                    ItemSwitching.EquipSlot10Action.Enable();
                    break;
                case false:
                    Movement.MobileMoveAction.Disable();
                    Movement.MobileLookAction.Disable();
                    Movement.MobileAttackAction.Disable();
                    Movement.MoveAction.Disable();
                    Movement.LookAction.Disable();
                    Movement.JumpAction.Disable();
                    Movement.RunAction.Disable();
                    Movement.ShotAction.Disable();
                    Movement.MeleeWeaponAttackAction.Disable();
                    Movement.PunchAction.Disable();
                    Movement.AimAction.Disable();
                    Movement.ReloadAction.Disable();
                    InteractAction.Disable();
                    Movement.CrouchAction.Disable();
                    Movement.ProneAction.Disable();
                    Movement.RollAction.Disable();
                    OpenInventoryAction.Disable();

                    ItemSwitching.EquipeNextItemAction.Disable();
                    ItemSwitching.EquipePreviousItemAction.Disable();
                    ItemSwitching.EquipItemScrollAction.Disable();
                    ItemSwitching.EquipSlot1Action.Disable();
                    ItemSwitching.EquipSlot2Action.Disable();
                    ItemSwitching.EquipSlot3Action.Disable();
                    ItemSwitching.EquipSlot4Action.Disable();
                    ItemSwitching.EquipSlot5Action.Disable();
                    ItemSwitching.EquipSlot6Action.Disable();
                    ItemSwitching.EquipSlot7Action.Disable();
                    ItemSwitching.EquipSlot8Action.Disable();
                    ItemSwitching.EquipSlot9Action.Disable();
                    ItemSwitching.EquipSlot10Action.Disable();
                    break;
            }
        }

#if UNITY_EDITOR
        private void OnExitPlayMode(UnityEditor.PlayModeStateChange mode)
        {
            if (mode != UnityEditor.PlayModeStateChange.ExitingPlayMode)
                return;

            SetActiveInputs(false);
        }
#endif
    }
}
