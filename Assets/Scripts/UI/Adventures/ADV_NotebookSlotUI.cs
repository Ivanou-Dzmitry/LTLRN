using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ADV_NotebookSlotUI : MonoBehaviour
{
    [SerializeField] private Image icon;
    [SerializeField] private TMP_Text quantityLabel;

    [Header("Text")]
    [SerializeField] private TMP_Text wordLabel;
    
    [Header("Button")]
    [SerializeField] private Button dropWordButton;

    private ADV_ItemDefinition itemDef;
    
    private void Awake()
    {
        dropWordButton.onClick.AddListener(OnDropWord);
    }

    public void Setup(ADV_ItemDefinition def, int quantity)
    {
        itemDef = def;

        if (icon != null)
            icon.sprite = def.icon;

        // The word must stay in the learned language regardless of UI language — resolve
        // it live from the DB rather than a LocalizedString (which tracks UI language).
        wordLabel.text = DBUtils.Instance.ResolveReference(def.wordReference);

        if (quantityLabel != null)
            quantityLabel.text = quantity > 1 ? quantity.ToString() : "";
    }

    private void OnDropWord()
    {
        ADV_InteractionManager.Instance.DropWord(itemDef.itemId);

        // Remove this slot immediately rather than waiting for the panel to reload.
        Destroy(gameObject);
    }

}
