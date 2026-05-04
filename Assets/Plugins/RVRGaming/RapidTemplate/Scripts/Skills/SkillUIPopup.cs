using UnityEngine;
using UnityEngine.UI;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.Inventory;

public class SkillUIPopup : MonoBehaviour
{
    public Text titleName;
    public Text subtitleText;
    public Text descriptionText;
    public Image descriptionImage;
    public Text pointCostText;
    public GameObject unlockImage;

    public void UpdateUI(Item item, bool isActive, Sprite imageSprite, int pointCost)
    {
        if (item != null && item.Info != null)
        {
            titleName.text = item.Info.Name(new Args(this.gameObject));
            descriptionText.text = item.Info.Description(new Args(this.gameObject));
            subtitleText.text = isActive ? "Active Skill" : "Passive Skill";
            descriptionImage.sprite = imageSprite;
            pointCostText.text = pointCost.ToString();
        }
        else
        {
            Debug.LogError("Item or Item.Info is null");
        }
    }
}
