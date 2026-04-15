using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ADV_InventorySlotUI : MonoBehaviour
{
    private LanguageSwitcher locManager;
    private Languages currentLang;

    [SerializeField] private Image icon;
    [SerializeField] private TMP_Text quantityLabel;
    [SerializeField] private TMP_Text descriptionLabel;
    private string descriptionText;

    [Header("Frame")]
    [SerializeField] private Image frameImage;
    [SerializeField] private Button slotButton;

    private ADV_ItemDefinition itemDef;

    public UIColorPalette palette;

    // broadcast which slot was selected
    public static event System.Action<ADV_InventorySlotUI> OnSlotSelected;

    public void Setup(ADV_ItemDefinition def, int quantity)
    {
        locManager = GameObject.FindWithTag("LangSwitcher").GetComponent<LanguageSwitcher>();

        itemDef = def;

        icon.sprite = def.icon;
        quantityLabel.text = quantity > 1 ? quantity.ToString() : "";

        //get current language
        currentLang = LanguageSwitcher.GetLanguageFromLocale(locManager.GetLocale());

        //load propper language description
        if (currentLang == Languages.RU)            
            descriptionLabel.text = def.displayNameRU;
        else
            descriptionLabel.text = def.displayNameEN;

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
        if (currentLang == Languages.RU)
            descriptionText = itemDef.descriptionLang01;
        else
            descriptionText = itemDef.descriptionLang02;

        OnSlotSelected?.Invoke(this);
    }

    private void HandleSelectionChanged(ADV_InventorySlotUI selected)
    {
        frameImage.color = selected == this ? palette.Success : Color.black;

        //load propper language description
        if (currentLang == Languages.RU)
            descriptionText = itemDef.descriptionLang01;
        else
            descriptionText = itemDef.descriptionLang02;
    }

    public string GetDescription()
    {
        return descriptionText;
    }
}
