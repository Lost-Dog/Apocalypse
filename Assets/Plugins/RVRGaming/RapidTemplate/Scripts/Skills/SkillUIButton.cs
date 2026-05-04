using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.Inventory;
using GameCreator.Runtime.Characters;
using GameCreator.Runtime.VisualScripting;

public class SkillUIButton : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    public Item item;
    public bool Active;
    [Space]
    [SerializeField] public RunConditionsList onCheck = new RunConditionsList();
    [Space]
    [SerializeField] public InstructionList onUnlock = new InstructionList();
    [Space]
    public Image icon;
    public Image bgPassive;
    public Image cdPassive;
    public Image bgActive;
    public Image cdActive;
    [Space]
    public GameObject popupPrefab;
    public enum PopupPosition { Left, Right }
    public PopupPosition popupPosition;
    public Sprite descriptionImage;
    public int unlockCost;

    [SerializeField] private Color lockColor = new Color32(255, 255, 255, 255);
    [SerializeField] private Color unlockColor = new Color32(250, 203, 69, 255);

    private GameObject instantiatedPopup;
    private Args currentArgs;
    [Space]
    [SerializeField] private PropertyGetGameObject m_BagGetter = GetGameObjectPlayer.Create();
    [SerializeField] private EquipmentIndex m_EquipmentIndex = new EquipmentIndex();

    private bool isUnlocked = false;

    void Start()
    {
        currentArgs = new Args(this.gameObject);
        UpdateUI();
    }

    public void SetArgs(Args args)
    {
        currentArgs = args;
        UpdateUI();
    }

    void UpdateUI()
    {
        if (item != null && currentArgs != null && item.Info != null)
        {
            icon.sprite = item.Info.Sprite(currentArgs);
        }

        bgPassive.gameObject.SetActive(!Active);
        cdPassive.gameObject.SetActive(!Active);
        bgActive.gameObject.SetActive(Active);
        cdActive.gameObject.SetActive(Active);

        icon.color = unlockColor;
        bgPassive.color = unlockColor;
        bgActive.color = unlockColor;
        SetColor(lockColor);
    }

    public void SetActive(bool isActive)
    {
        Active = isActive;
        UpdateUI();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (CheckPreExecutionConditions(gameObject))
        {
            StartCoroutine(EquipItem());
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        ShowPopup(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        ShowPopup(false);
    }

    private bool CheckPreExecutionConditions(GameObject gameObject)
    {
        return this.onCheck.Check(new Args(gameObject));
    }

    private IEnumerator EquipItem()
    {
        Bag bag = m_BagGetter.Get<Bag>(this.gameObject);
        if (isUnlocked)
        {
            Debug.Log("Item is already unlocked and equipped.");
            yield break;
        }

        if (bag == null || bag.Equipment == null)
        {
            Debug.LogError("EquipItem: Bag or Equipment component not found or not initialized");
            yield break;
        }

        RuntimeItem runtimeItem = new RuntimeItem(item);
        bag.Content.Add(runtimeItem, true);
        if (!bag.Equipment.CanEquipToIndex(runtimeItem, m_EquipmentIndex.Index))
        {
            Debug.LogError($"Cannot equip the item to the specified index {m_EquipmentIndex.Index}. Check conditions.");
            Debug.Log($"Available Slots: {bag.Equipment.Count}, Slot Type Required: {bag.Equipment.GetSlotBaseName(m_EquipmentIndex.Index)}");
            yield break;
        }

        var equipTask = bag.Equipment.EquipToIndex(runtimeItem, m_EquipmentIndex.Index);
        yield return new WaitUntil(() => equipTask.IsCompleted);

        if (equipTask.Result)
        {
            SetColor(unlockColor);
            _ = this.onUnlock.Run(new Args(gameObject));
            isUnlocked = true;
        }
        else
        {
            Debug.LogError("Failed to equip the item.");
        }
    }

    private void SetColor(Color color)
    {
        icon.color = color;
        bgPassive.color = color;
        bgActive.color = color;
    }

    private void ShowPopup(bool show)
    {
        if (show)
        {
            if (instantiatedPopup == null) 
            {
                Vector3 positionOffset = new Vector3(popupPosition == PopupPosition.Right ? 275 : -275, 0, 0);
                instantiatedPopup = Instantiate(popupPrefab, transform.position + positionOffset, Quaternion.identity, transform);
                SkillUIPopup popup = instantiatedPopup.GetComponent<SkillUIPopup>();
                if (popup != null)
                {
                    popup.UpdateUI(item, Active, descriptionImage, unlockCost);
                }
            }
            instantiatedPopup.SetActive(true);
        }
        else
        {
            if (instantiatedPopup != null)
            {
                Destroy(instantiatedPopup);
                instantiatedPopup = null;
            }
        }
    }

    public void ResetUI()
    {
        Bag bag = m_BagGetter.Get<Bag>(this.gameObject);
        if (bag != null && bag.Equipment != null)
        {
            IdString runtimeItemID = bag.Equipment.GetSlotRootRuntimeItemID(m_EquipmentIndex.Index);
            if (!string.IsNullOrEmpty(runtimeItemID.String))
            {
                RuntimeItem runtimeItem = bag.Content.GetRuntimeItem(runtimeItemID);
                if (runtimeItem != null && runtimeItem.Item.ID == item.ID)
                {
                    bag.Equipment.Unequip(runtimeItem);
                    bag.Content.Remove(runtimeItem);
                }
            }
        }

        isUnlocked = false; 
        SetColor(lockColor);
        UpdateUI();
    }
}