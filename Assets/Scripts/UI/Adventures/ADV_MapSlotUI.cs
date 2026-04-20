using System.Net.Sockets;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.UI;

public class ADV_MapSlotUI : MonoBehaviour
{
    [SerializeField] private Image icon;
    [SerializeField] private Button slotButton;

    [Header("Localization")]
    public LocalizedString description;
    private string descriptionText;

    public static event System.Action<ADV_MapSlotUI> OnMapSlotSelected;

    public Sprite sprite
    {
        get => icon.sprite;
        set => icon.sprite = value;
    }

    public void SetupMapItem(string desc)
    {       
        description.TableReference = "LTLRN";
        description.TableEntryReference = desc;

        descriptionText = description.GetLocalizedString();
        slotButton.onClick.AddListener(OnMapSlotClicked);
    }

    private void OnMapSlotClicked()
    {
        Debug.Log($"Map slot clicked: {descriptionText}");

        OnMapSlotSelected?.Invoke(this);
    }

    public string GetDescription() => descriptionText;

    private void OnDestroy()
    {
        OnMapSlotSelected = null;
        slotButton.onClick.RemoveListener(OnMapSlotClicked);
    }
}
