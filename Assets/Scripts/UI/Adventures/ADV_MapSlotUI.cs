using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.UI;

public class ADV_MapSlotUI : MonoBehaviour
{
    [SerializeField] private Image icon;
    [SerializeField] private Button slotButton;
    [SerializeField] private Image frame;
    [SerializeField] private Image playerMarkerPosition;

    [Header("Localization")]
    public LocalizedString description;
    private string descriptionText;

    public static event System.Action<ADV_MapSlotUI> OnMapSlotSelected;

    public Sprite sprite
    {
        get => icon.sprite;
        set => icon.sprite = value;
    }

    public void SetupMapItem(string desc, string imgName, string mapName, Vector3 markerOffset)
    {       
        description.TableReference = "KELIAS_INVENTORY";
        description.TableEntryReference = desc;

        descriptionText = description.GetLocalizedString();
        
        if(slotButton!=null)
            slotButton.onClick.AddListener(OnMapSlotClicked);

        OnMapSlotSelected += HandleSelectionChanged;

        //frame hide by default
        if (frame != null)
            frame.gameObject.SetActive(false);

        //marker hide by default
        if (playerMarkerPosition != null)
        {
            if (mapName == imgName)
            {
                playerMarkerPosition.gameObject.SetActive(true);
                RectTransform markerRect = playerMarkerPosition.GetComponent<RectTransform>();
                markerRect.anchoredPosition += (Vector2)markerOffset;
            }                
            else
                playerMarkerPosition.gameObject.SetActive(false);
        }            
    }

    private void OnMapSlotClicked()
    {                
        descriptionText = description.GetLocalizedString();

        OnMapSlotSelected?.Invoke(this);
    }

    private void HandleSelectionChanged(ADV_MapSlotUI selected)
    {
        frame.gameObject.SetActive(selected == this);
        descriptionText = description.GetLocalizedString();
    }

    public string GetDescription() => descriptionText;

    private void OnDestroy()
    {
        OnMapSlotSelected -= HandleSelectionChanged;
        slotButton.onClick.RemoveListener(OnMapSlotClicked);
    }

}
