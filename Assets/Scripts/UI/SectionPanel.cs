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
    public TMP_Text topicsCount;

    [Header("Sliders")]
    public Slider progressSlider;
    //public Slider difSlider;

    [Header("Buttons")]
    public Button likeButton;
    [SerializeField] private Image likeButtonImage;
    [SerializeField] public bool isLiked = false;

    [Header("Play Section")]
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
        //likeButtonImage = likeButton.GetComponent<Image>();
        likeButtonImage.color = palette.Transparent50Panel;

        //add listeners
        likeButton.onClick.AddListener(OnLike);

        //play section
        if (playSectionButton != null)
            playSectionButton.onClick.AddListener(OnClicked);

        //get game data
        gameData = GameObject.FindWithTag("GameData").GetComponent<GameData>();
    }

    private void Start()
    {
        //get db utils        
        dbUtils = GameObject.FindWithTag("DBUtils").GetComponent<DBUtils>();
    }

    private void OnClicked()
    {
        //for bundle sections, open bundle menu and load sections into it, otherwise load game directly with selected section
        if (isBundleSection)
        {            
            LoadBundle(currentSection.name);
        }
        else
        {
            LoadBasic();
        }
    }

    private void OnLike()
    {
        //toggle like state
        isLiked = !isLiked;

        if (likeButtonImage == null)
            return;

        //update like button color
        likeButtonImage.color = isLiked
            ? palette.Gray3Dark
            : palette.Transparent50Panel;

        //update database
        dbUtils.UpdateSectionLiked(currentSection.name, isLiked);
    }

    public void SetLikedState(bool liked)
    {
        //set like state
        isLiked = liked;

        if(likeButtonImage==null)
            return;

        likeButtonImage.color = isLiked
            ? palette.Gray3Dark
            : palette.Transparent50Panel;
    }

    public void Initialize(Section section)
    {
        //set current section
        if (section.sectionIcon != null)
            sectionImage.sprite = section.sectionIcon;

        //set panel view
        //set header and description with localization
        try
        {
            sectionHeaderText.text = section.sectionTitle.GetLocalizedString(); //GetTitle(section);
        }
        catch
        {
            sectionHeaderText.text = "Loc not assigned yet";
        }

        try
        {
            sectionDescriptionText.text = section.sectionDescription.GetLocalizedString(); //GetDescription(section);
        }
        catch
        {
            sectionDescriptionText.text = "Loc not assigned yet";
        }

        //set difficulty
        diffText.text = section.difficultyType.ToString();
        //difSlider.value = section.GetSectionDifValue(section.difficultyType);

        //color header
        Image headerImage = headerPanel.GetComponent<Image>();
        headerImage.color = section.sectionHeaderColor;
    }

    private void OnDestroy()
    {
        //remove listeners
        likeButton.onClick.RemoveListener(OnLike);
        playSectionButton.onClick.RemoveListener(OnClicked);
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

    private void LoadBundle(string bundleSectionName)
    {
        //Debug.Log("Loading bundle section: ");
        PanelManager.Open("bundlemenu");

        //get bundle panel
        bundlePanel = GameObject.FindWithTag("BundlePanel").GetComponent<EX_BundleMenu>();

        //load sections into bundle panel
        if (bundlePanel != null)
        {
            //load sections into bundle panel
            bundlePanel.SectionLoader(bundleSections, sectionHeaderText.text, bundleSectionName);

            //save bundles
            if (gameData != null)
                gameData.saveData.bundleSections = bundleSections;

            gameData.SaveToFile();
        }
    }

    private void LoadBasic()
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
