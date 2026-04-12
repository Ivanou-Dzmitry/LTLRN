using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ADV_InventorySlotUI : MonoBehaviour
{
    [SerializeField] private Image icon;
    [SerializeField] private TMP_Text quantityLabel;
    [SerializeField] private TMP_Text descriptionLabel;

    public void Setup(ADV_ItemDefinition def, int quantity)
    {
        icon.sprite = def.icon;
        quantityLabel.text = quantity > 1 ? quantity.ToString() : "";
        descriptionLabel.text = def.description;
    }
}
