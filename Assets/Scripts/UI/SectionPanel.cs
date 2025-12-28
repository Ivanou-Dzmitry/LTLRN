using TMPro;
using UnityEngine;
using UnityEngine.UI;

//panel for section

public class SectionPanel : MonoBehaviour
{
    private ExDataLoader dataLoader;
    private GameData gameData;
    private DBUtils dbUtils;

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
    [SerializeField] public bool isLiked = false;
    public Button playSectionButton;

    [Header("Section")]
    public Section currentSection;
    public int sectionIndex;

    public RectTransform questionsRectTransform;

    [Header("Palette")]
    [SerializeField] private UIColorPalette palette;    
    

    private void Awake()
    {
        //get button if not assigned
        if (playSectionButton == null)
            playSectionButton = GetComponent<Button>();

        //initialize like button color
        likeButtonImage = likeButton.GetComponent<Image>();
        likeButtonImage.color = palette.DisabledButton;

        //add listeners
        likeButton.onClick.AddListener(OnLike);
        playSectionButton.onClick.AddListener(OnClicked);
    }

    private void OnClicked()
    {
        //get data loader and game data
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
        //get db utils
        dbUtils = GameObject.FindWithTag("DBUtils").GetComponent<DBUtils>();

        //toggle like state
        isLiked = !isLiked;

        //update like button color
        likeButtonImage.color = isLiked
            ? palette.Secondary
            : palette.DisabledButton;

        //update database
        dbUtils.UpdateSectionLiked(currentSection.name, isLiked);
    }

    public void SetLikedState(bool liked)
    {
        //set like state
        isLiked = liked;

        likeButtonImage.color = isLiked
            ? palette.Secondary
            : palette.DisabledButton;
    }

    public void Initialize(Section section)
    {
        //set current section
        if (section.sectionIcon != null)
            sectionImage.sprite = section.sectionIcon;

        //set panel view
        sectionHeaderText.text = section.sectionTitle;
        sectionDescriptionText.text = section.sectionDescription;

        //color header
        Image headerImage = headerPanel.GetComponent<Image>();
        headerImage.color = section.sectionHeaderColor;
    }

    public void PlayButtonToggle(int questions)
    {
        //toggle play button interactable state based on questions count
        ButtonImage buttonImage = playSectionButton.GetComponent<ButtonImage>();

        if (questions > 0)
        {
            playSectionButton.interactable = true;
            buttonImage.SetDisabled(false);
        }
        else
        {
            playSectionButton.interactable = false;
            buttonImage.SetDisabled(true);
        }
    }

    private void OnDestroy()
    {
        //remove listeners
        likeButton.onClick.RemoveListener(OnLike);
        playSectionButton.onClick.RemoveListener(OnClicked);
    }
}
