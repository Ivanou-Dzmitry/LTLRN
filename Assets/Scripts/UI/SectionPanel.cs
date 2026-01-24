using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;

//panel for section
public class SectionPanel : MonoBehaviour
{
    private ExDataLoader dataLoader;
    private GameData gameData;
    private DBUtils dbUtils;

    private EX_BundleMenu bundlePanel;

    [Header("UI")]    
    public Image sectionImage;
    public TMP_Text sectionHeaderText;
    public GameObject headerPanel;
    public TMP_Text sectionDescriptionText;

    [Header("DB Data")]
    public TMP_Text sectionTimeText;
    public TMP_Text sectionResultText;
    public TMP_Text diffText;

    [Header("Sliders")]
    public Slider progressSlider;
    //public Slider difSlider;

    [Header("Buttons")]
    public Button likeButton;
    private Image likeButtonImage;
    [SerializeField] public bool isLiked = false;
    public Button playSectionButton;

    [Header("Section")]
    public Section currentSection; //important
    public int sectionIndex;

    public RectTransform questionsRectTransform;

    [Header("Palette")]
    [SerializeField] private UIColorPalette palette;

    [Header("Bundle")]
    public bool isBundleSection = false;
    public Section[] bundleSections; 


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

        if (playSectionButton != null)
            playSectionButton.onClick.AddListener(OnClicked);

        gameData = GameObject.FindWithTag("GameData").GetComponent<GameData>();
    }

    private void OnClicked()
    {
        if (isBundleSection)
        {
            //Debug.Log("Loading bundle section: ");

            PanelManager.Open("bundlemenu");

            //get bundle panel
            bundlePanel = GameObject.FindWithTag("BundlePanel").GetComponent<EX_BundleMenu>();

            //load sections into bundle panel
            if (bundlePanel != null)
            {
                bundlePanel.SectionLoader(bundleSections, sectionHeaderText.text);
            }
        }
        else
        {
            //get data loader and game data
            dataLoader = GameObject.FindWithTag("ExDataLoader").GetComponent<ExDataLoader>();

            //set current section
            if (dataLoader != null)
            {
                dataLoader.sectionClass = currentSection;
            }

            //save section
            if (gameData != null)
            {
                gameData.saveData.selectedSectionIndex = sectionIndex;
                gameData.saveData.sectionToLoad = currentSection;
                gameData.SaveToFile();
            }

            //load game
            PanelManager.OpenScene("ExGame");
        }
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
        sectionHeaderText.text = GetTitle(section);
        sectionDescriptionText.text = GetDescription(section);

        //set difficulty
        diffText.text = section.difficultyType.ToString();
        //difSlider.value = section.GetSectionDifValue(section.difficultyType);

        //color header
        Image headerImage = headerPanel.GetComponent<Image>();
        headerImage.color = section.sectionHeaderColor;
    }

/*    public void PlayButtonToggle(int questions)
    {
        //toggle play button interactable state based on questions count
       // ButtonImage buttonImage = playSectionButton.GetComponent<ButtonImage>();

        if (questions > 0)
        {
            playSectionButton.interactable = true;
            //buttonImage.SetDisabled(false);
        }
        else
        {
            playSectionButton.interactable = false;
            //buttonImage.SetDisabled(true);
        }
    }*/

    private void OnDestroy()
    {
        //remove listeners
        likeButton.onClick.RemoveListener(OnLike);
        playSectionButton.onClick.RemoveListener(OnClicked);
    }

    public string GetDescription(Section section)
    {        
        if (section == null || section.sectionDescription == null)
            return string.Empty;
        
        Locale locale = GetLocale();

        if (locale == null)
            return section.sectionDescription.en;

        switch (locale.Identifier.Code)
        {
            case "ru":
                return section.sectionDescription.ru;

            case "en":
            default:
                return section.sectionDescription.en;
        }
    }

    public string GetTitle(Section section)
    {        
        if (section == null || section.sectionTitle == null)
            return string.Empty;

        Locale locale = GetLocale();

        if (locale == null)
            return section.sectionTitle.en;

        switch (locale.Identifier.Code)
        {
            case "ru":
                return section.sectionTitle.ru;

            case "en":
            default:
                return section.sectionTitle.en;
        }
    }

    private Locale GetLocale()
    {
        
        string savedLang = gameData.saveData.lang.ToLower();

        Locale locale = LocalizationSettings.AvailableLocales.Locales
            .FirstOrDefault(l => l.Identifier.Code == savedLang);

        if (locale != null)
            LocalizationSettings.SelectedLocale = locale;

        return locale;
    }

}
