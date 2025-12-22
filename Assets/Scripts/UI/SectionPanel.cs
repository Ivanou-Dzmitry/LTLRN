using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class SectionPanel : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] public Image sectionImage;
    [SerializeField] public TMP_Text sectionHeaderText;
    [SerializeField] public TMP_Text sectionDescriptionText;
    public RectTransform questionsRectTransform;

    public void Initialize(Section section)
    {
        if (section.sectionIcon != null)
            sectionImage.sprite = section.sectionIcon;

        sectionHeaderText.text = section.sectionTitle;
        sectionDescriptionText.text = section.sectionDescription;
    }
}
