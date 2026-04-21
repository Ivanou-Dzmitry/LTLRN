using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ADV_InventorySlotUI : MonoBehaviour
{
    [SerializeField] private Image icon;
    [SerializeField] private TMP_Text quantityLabel;
    [SerializeField] private TMP_Text descriptionLabel;
    private string descriptionText;

    [Header("Frame")]
    [SerializeField] private Image frameImage;
    [SerializeField] private Button slotButton;
    [SerializeField] private Image frame;

    private ADV_ItemDefinition itemDef;

    public UIColorPalette palette;

    // broadcast which slot was selected
    public static event System.Action<ADV_InventorySlotUI> OnSlotSelected;

    public void Setup(ADV_ItemDefinition def, int quantity)
    {
        itemDef = def;

        icon.sprite = def.icon;
        quantityLabel.text = quantity > 1 ? quantity.ToString() : "";
  
        //load propper language description
        descriptionLabel.text = def.itemName.GetLocalizedString();

        slotButton.onClick.AddListener(OnSlotClicked);
        OnSlotSelected += HandleSelectionChanged;

        //hide frame by default
        frame.gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        OnSlotSelected -= HandleSelectionChanged;
        slotButton.onClick.RemoveListener(OnSlotClicked);
    }

    private void OnSlotClicked()
    {
        descriptionText = itemDef.itemDescription.GetLocalizedString();

        OnSlotSelected?.Invoke(this);
    }

    private void HandleSelectionChanged(ADV_InventorySlotUI selected)
    {
        //frameImage.color = selected == this ? palette.Success : Color.black;
        frame.gameObject.SetActive(selected == this);

        //load propper language description
        descriptionText = itemDef.itemDescription.GetLocalizedString();
    }

    public string GetDescription()
    {
        return descriptionText;
    }
}
