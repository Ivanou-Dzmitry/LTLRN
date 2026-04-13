using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ADV_InventorySlotUI : MonoBehaviour
{
    [SerializeField] private Image icon;
    [SerializeField] private TMP_Text quantityLabel;
    [SerializeField] private TMP_Text descriptionLabel;

    [Header("Frame")]
    [SerializeField] private Image frameImage;
    [SerializeField] private Button slotButton;

    public UIColorPalette palette;

    // broadcast which slot was selected
    public static event System.Action<ADV_InventorySlotUI> OnSlotSelected;

    public void Setup(ADV_ItemDefinition def, int quantity)
    {
        icon.sprite = def.icon;
        quantityLabel.text = quantity > 1 ? quantity.ToString() : "";
        descriptionLabel.text = def.description;

        slotButton.onClick.AddListener(OnSlotClicked);
        OnSlotSelected += HandleSelectionChanged;
    }

    private void OnDestroy()
    {
        OnSlotSelected -= HandleSelectionChanged;
        slotButton.onClick.RemoveListener(OnSlotClicked);
    }

    private void OnSlotClicked()
    {
        OnSlotSelected?.Invoke(this);
    }

    private void HandleSelectionChanged(ADV_InventorySlotUI selected)
    {
        frameImage.color = selected == this ? palette.Success : Color.black;
    }
}
