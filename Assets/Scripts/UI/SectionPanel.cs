using UnityEngine;
using TMPro;
using UnityEngine.UI;

//panel for section

public class SectionPanel : MonoBehaviour
{
    [Header("UI")]
    
    [SerializeField] public Image sectionImage;
    [SerializeField] public TMP_Text sectionHeaderText;
    [SerializeField] public GameObject headerPanel;
    [SerializeField] public TMP_Text sectionDescriptionText;

    public RectTransform questionsRectTransform;

    public void Initialize(Section section)
    {
        if (section.sectionIcon != null)
            sectionImage.sprite = section.sectionIcon;

        //set panel view
        sectionHeaderText.text = section.sectionTitle;
        sectionDescriptionText.text = section.sectionDescription;

        Image headerImage = headerPanel.GetComponent<Image>();
        headerImage.color = section.sectionHeaderColor;
    }
}
