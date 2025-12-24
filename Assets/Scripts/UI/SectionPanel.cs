using UnityEngine;
using TMPro;
using UnityEngine.UI;

//panel for section

public class SectionPanel : MonoBehaviour
{
    private ExDataLoader dataLoader;
    private GameData gameData;

    [Header("UI")]    
    public Image sectionImage;
    public TMP_Text sectionHeaderText;
    public GameObject headerPanel;
    public TMP_Text sectionDescriptionText;
    public TMP_Text sectionTimeText;
    public Slider progressSlider;

    [Header("Buttons")]
    public Button likeButton;
    private Image likeButtonImage;
    private bool isLiked = false;
    public Button playSectionButton;

    [Header("Section")]
    public Section currentSection;
    public int sectionIndex;

    public RectTransform questionsRectTransform;

    [Header("Palette")]
    [SerializeField] private UIColorPalette palette;

    private void Awake()
    {
        if (playSectionButton == null)
            playSectionButton = GetComponent<Button>();

        likeButtonImage = likeButton.GetComponent<Image>();

        likeButtonImage.color = palette.DisabledButton;

        likeButton.onClick.AddListener(OnLike);
        playSectionButton.onClick.AddListener(OnClicked);
    }

    private void OnClicked()
    {
        dataLoader = GameObject.FindWithTag("ExDataLoader").GetComponent<ExDataLoader>();
        gameData = GameObject.FindWithTag("GameData").GetComponent<GameData>();

        //set current section
        if (dataLoader != null)
        {
            dataLoader.sectionClass = currentSection;
        }

        //save section
        if (gameData != null)
        {
            gameData.saveData.selectedSectionIndex = sectionIndex;
            gameData.SaveToFile();
        }

        //load game
        PanelManager.OpenScene("ExGame");
    }

    private void OnLike()
    {
        isLiked = !isLiked;

        likeButtonImage.color = isLiked
            ? palette.Secondary
            : palette.DisabledButton;
    }


    public void Initialize(Section section)
    {
        if (section.sectionIcon != null)
            sectionImage.sprite = section.sectionIcon;

        //set panel view
        sectionHeaderText.text = section.sectionTitle;
        sectionDescriptionText.text = section.sectionDescription;

        //color header
        Image headerImage = headerPanel.GetComponent<Image>();
        headerImage.color = section.sectionHeaderColor;
    }
}
