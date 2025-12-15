using JUTPS;
using JUTPS.ActionScripts;
using JUTPS.ArmorSystem;
using JUTPS.InventorySystem;
using JUTPS.ItemSystem;
using JUTPS.VehicleSystem;
using System.Collections;
using UnityEngine;

namespace JU.SaveLoad
{
    /// <summary>
    /// Save the character controller state.
    /// </summary>
    [AddComponentMenu("JU TPS/Save Load/JU Save Load Character")]
    [RequireComponent(typeof(JUCharacterController))]
    public class JUSaveLoadCharacter : JUSaveLoadComponent
    {
        private string _currentVehicleName;

        private JUCharacterController _character;
        private DriveVehicles _driveVehicles;

        private const string POSITION_KEY = "Position";
        private const string ROTATION_KEY = "Rotation";
        private const string CROUCH_KEY = "Crouch";
        private const string PRONE_KEY = "Prone";
        private const string VEHICLE_TO_DRIVE_KEY = "Vehicle";
        private const string ITEM_TO_EQUIPE_KEY = "Item To Equipe";
        private const string ARMOR_EQUIPED_KEY = "Armor Equiped";

        /// <inheritdoc/>
        protected override void Awake()
        {
            _character = GetComponent<JUCharacterController>();
            _driveVehicles = GetComponent<DriveVehicles>();

            if (_driveVehicles)
            {
                _driveVehicles.OnEnterVehicle.AddListener(OnEnterVehicle);
                _driveVehicles.OnExitVehicle.AddListener(OnExitVehicle);
            }

            base.Awake();
        }

        /// <inheritdoc/>
        public override void Save()
        {
            base.Save();

            SetValue(POSITION_KEY, _character.rb.position);
            SetValue(ROTATION_KEY, _character.rb.rotation);
            SetValue(CROUCH_KEY, _character.IsCrouched);
            SetValue(PRONE_KEY, _character.IsProne);

            SetValue(ITEM_TO_EQUIPE_KEY, _character.HoldableItemInUseRightHand ? _character.HoldableItemInUseRightHand.ItemName : null);
            SetValue(VEHICLE_TO_DRIVE_KEY, _currentVehicleName);

            Armor[] armors = _character.GetComponentsInChildren<Armor>(true);
            foreach (var armor in armors)
                SetValue(GetArmorActiveKey(armor), armor.Equiped);
        }

        /// <inheritdoc/>
        public override void Load()
        {
            IEnumerator LoadAfterGameSetup()
            {
                yield return new WaitForEndOfFrame();

                // Load the vehicle name of the last play (if the character was driving a vehicle) and start driving.
                string vehicleToDriveName = GetValue(VEHICLE_TO_DRIVE_KEY, string.Empty);
                if (_driveVehicles && !string.IsNullOrEmpty(vehicleToDriveName))
                {
                    GameObject vehicleObject = GameObject.Find(vehicleToDriveName);
                    if (vehicleObject)
                    {
                        JUVehicle vehicleComponent = vehicleObject.GetComponent<JUVehicle>();
                        JUVehicleCharacterIK vehicleCharacterIk = vehicleObject.GetComponent<JUVehicleCharacterIK>();
                        _driveVehicles.DriveVehicle(vehicleComponent, vehicleCharacterIk, true);
                    }
                }

                // Load the character state, like position, rotation, pose...
                if (_driveVehicles && !_driveVehicles.IsDriving)
                {
                    _character.rb.position = GetValue(POSITION_KEY, _character.rb.position);
                    _character.rb.rotation = GetValue(ROTATION_KEY, _character.rb.rotation);
                    _character.rb.linearVelocity = Vector3.zero;
                    _character.rb.angularVelocity = Vector3.zero;

                    if (GetValue(CROUCH_KEY, false))
                        _character._Crouch();

                    if (GetValue(PRONE_KEY, false))
                        _character._Prone();

                    // Equip the last used item.
                    string itemToEquipeName = GetValue(ITEM_TO_EQUIPE_KEY, string.Empty);
                    int itemToEquipeId = -1;

                    for (int i = 0; i < _character.Inventory.HoldableItensRightHand.Length; i++)
                    {
                        JUHoldableItem item = _character.Inventory.HoldableItensRightHand[i];
                        if (item && string.Equals(item.ItemName, itemToEquipeName))
                        {
                            itemToEquipeId = item.ItemSwitchID;
                            break;
                        }
                    }

                    if (itemToEquipeId != _character.ItemToEquipOnStart)
                    {
                        _character.ItemToEquipOnStart = itemToEquipeId;
                        _character.SwitchToItem(itemToEquipeId);
                    }
                }

                // Equipe armor.
                JUInventory inventory = _character.Inventory;
                Armor[] armors = _character.GetComponentsInChildren<Armor>(true);
                foreach (var armor in armors)
                {
                    bool isArmorActive = GetValue(GetArmorActiveKey(armor), armor.Equiped);
                    if (isArmorActive != armor.Equiped)
                    {
                        int armorId = JUInventory.GetGlobalItemSwitchID(armor, inventory);

                        if (isArmorActive)
                            inventory.EquipItem(armorId);

                        else
                            inventory.UnequipItem(armorId);
                    }
                }
            }

            base.Load();

            if (_character.IsDriving)
                return;

            StartCoroutine(LoadAfterGameSetup());
        }

        private void OnEnterVehicle()
        {
            _currentVehicleName = _driveVehicles.CurrentVehicle.name;
        }

        private void OnExitVehicle()
        {
            _currentVehicleName = null;
        }

        private string GetArmorActiveKey(Armor armor)
        {
            return $"{_character.name} {ARMOR_EQUIPED_KEY} {armor.name}";
        }

        /// <inheritdoc/>
        protected override void OnExitPlayMode()
        {
            base.OnExitPlayMode();

            _currentVehicleName = null;
            _character = null;
            _driveVehicles = null;
        }
    }
}