using System.Collections;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Diagnostics;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;
using static Section;


//button for select theme

public class EX_ThemeBtn : MonoBehaviour
{
    private ExDataLoader dataLoader;
    private GameData gameData;
    private DBUtils dbUtils;

    [Header("Data")]
    public SectionManager sectionManager;

    [Header("Level buttons")]
    public Button[] levelButtons;
    [SerializeField] private GameObject levelButtonsPanel;

    // maps button index to difficulty — order must match levelButtons array
    private readonly DifficultyType[] difficultyMap = new[]
    {
        DifficultyType.A0,
        DifficultyType.A1,
        DifficultyType.A2,
        DifficultyType.B1,
        DifficultyType.B2,
        DifficultyType.C1
    };

    [Header("Button")]
    public Button button;

    [Header("Theme text stuff")]
    public TMP_Text themeName;
    public TMP_Text themeNameLocal;
    public TMP_Text themeDescription;

    [Header("Icons")]
    public Image themeIcon;
    public Image topPnlImg;
    public int themeIndex; //used for save

    [Header("Info")]
    public TMP_Text sectionsCount;
    public TMP_Text tasksCount;
    public TMP_Text questionsCount;

    [Header("Sliders")]
    public Slider themeProgressSlider;

    [Header("Panels")]
    [SerializeField] private GameObject topPanel;
    [SerializeField] private GameObject bottomPanel;    
    [SerializeField] private GameObject infoPanel;

    [Header("Transform")]
    [SerializeField] private RectTransform panelTransform;

    [Header("Palette")]
    [SerializeField] private UIColorPalette palette;

    private bool isExpanded = true;

    [Header("Exapnd button")]
    [SerializeField] private Button buttonExpand;
    [SerializeField] private RectTransform imageExpandTransform;

    private void Awake()
    {
        if (button == null)
            button = GetComponent<Button>();

        //button.onClick.AddListener(TogglePanel);

        //epand button
        buttonExpand.onClick.AddListener(TogglePanel);

        //set colors
        topPnlImg = topPanel.GetComponent<Image>();
    }

    private void Start()
    {
        //get data
        dataLoader = GameObject.FindWithTag("ExDataLoader").GetComponent<ExDataLoader>();
        gameData = GameObject.FindWithTag("GameData").GetComponent<GameData>();
        dbUtils= GameObject.FindWithTag("DBUtils").GetComponent<DBUtils>();

        TogglePanel();

        // set button interact
        for (int i = 0; i < levelButtons.Length; i++)
        {
            bool hasLevel = sectionManager.sections.Any(s => s.difficultyType == difficultyMap[i]);           
            //levelButtons[i].interactable = hasLevel;

            var btnImage = levelButtons[i].GetComponent<ButtonImage>();

            if (btnImage != null)
            {
                btnImage.SetDisabled(!hasLevel); // or whatever method you have
            }
        }

        //add listeners to level buttons
        for (int i = 0; i < levelButtons.Length; i++)
        {
            int index = i; // capture for closure
            levelButtons[i].onClick.AddListener(() =>
            {
                //Load data
                if (dataLoader.tempSectionManager != null)
                {
                    //set section from theme to loader
                    dataLoader.sectionManager = sectionManager;

                    //save selected theme index
                    gameData.saveData.selectedThemeIndex = themeIndex;
                    gameData.SaveToFile();
                }

                Debug.Log($"[EX_ThemeBtn] sectionManager set: '{sectionManager.name}' | difficulty: {difficultyMap[index]}");

                //work with panels
                PanelManager.CloseAll();
                PanelManager.Open("exmain");

                //important! load data for selected difficulty
                dataLoader.LoadExerciseData(difficultyMap[index]);
            });
        }
       
        UpdateThemeBtnUI();
    }

    public void UpdateThemeBtnUI()
    {
        //localized theme name
        try
        {
            themeName.text = sectionManager.themeName.GetLocalizedString();
        }
        catch
        {
            themeName.text = "Loc not assigned yet";
        }

        //thame name in target lang (lern lang LT, LV etc)
        themeNameLocal.text = sectionManager.themeNameTargetLang;

        //set theme button icon
        themeIcon.sprite = sectionManager.themeIcon;

        int sCont = sectionManager.sections.Count();

        //get localized "Sections" text
        string locString01 = LocalizationSettings.StringDatabase.GetLocalizedString("KELIAS_UI", "SectionsTxt");

        //set sections count and questions count
        sectionsCount.text = $"{sCont} {locString01.ToLower()}";

        //set color Gray if no sections, set button interactable
        if (sCont == 0)
        {
            //get local description for WIP themes
            string wipTxt = LocalizationSettings.StringDatabase.GetLocalizedString("KELIAS_UI", "TopicWIPTxt");

            //set UI
            topPnlImg.color = palette.Panel02;
            button.interactable = false;
            themeDescription.text = wipTxt;

            levelButtonsPanel.SetActive(false);
        }
        else
        {
            topPnlImg.color = sectionManager.themeHeaderColor;
            button.interactable = true;
            //description
            //themeBtnComponent.themeDescription.text = currentTheme.GeThemetDescription(currentTheme, locale);
        }

        //hide info panel if no sections
        if (sectionsCount.text == "0")
        {
            infoPanel.gameObject.SetActive(false);
        }

        //for questions count
        int qCount = 0;
        
        // Step 1 - try to get questions count - basic
        qCount = sectionManager.GetTotalQuestionCount();

        // Step 2 - try to get questions count - try bundle
        if (qCount == 0)
            qCount = sectionManager.GetBundleTotalQuestionCount();

        //set questions count (any)
        questionsCount.text = qCount.ToString(); // bundle       
    }

    private void SaveLoadData()
    {
        //get data
        dataLoader = GameObject.FindWithTag("ExDataLoader").GetComponent<ExDataLoader>();
        gameData = GameObject.FindWithTag("GameData").GetComponent<GameData>();

        //check data
        if (gameData == null || dataLoader == null) return;

        //Load data
        if (dataLoader.tempSectionManager != null)
        {
            //set section from theme to loader
            dataLoader.sectionManager = sectionManager;

            //save selected theme index
            gameData.saveData.selectedThemeIndex = themeIndex;
            gameData.SaveToFile();
        }

        //work with panels
        PanelManager.CloseAll();
        PanelManager.Open("exmain");

        //load exercise data
        dataLoader.LoadExerciseData();        
    }

    private void LevelSelector()
    {
        // Implement level selection logic here
    }

    private void OnDestroy()
    {
        button.onClick.RemoveListener(TogglePanel);  
        buttonExpand.onClick.RemoveListener(TogglePanel);
    }

    public void TogglePanel()
    {
        isExpanded = !isExpanded;

        //height change
        float height = isExpanded ? 440f : 207f;

        //panel size change
        Vector2 size = panelTransform.sizeDelta;
        size.y = height;
        panelTransform.sizeDelta = size;

        //icon flip
        Vector3 scale = imageExpandTransform.localScale;
        scale.y = isExpanded ? -1f : 1f;
        imageExpandTransform.localScale = scale;

        //panels show/hide
        bottomPanel.gameObject.SetActive(isExpanded);

        //slider animation
        if (isExpanded)
            SliderAnimator();
    }

    private void SliderAnimator()
    {

        int sCont = sectionManager.sections.Count();
        //set valused for progress bar
        themeProgressSlider.maxValue = sCont;
        themeProgressSlider.value = 0;

        //initial values
        int completeSections = 0;
        int totalCompleteSections = 0;

        foreach (var section in sectionManager.sections)
        {
            if (!section.isBundle) continue;

            foreach (var bundleSection in section.bundleSections)
            {
                totalCompleteSections++;

                bool isComplete = dbUtils.GetSectionComplete(bundleSection.name);
                if (isComplete)
                {
                    completeSections++;
                }
            }
        }

        //tasks cont
        //themeBtnComponent.tasksCount.text = totalCompleteSections.ToString();

        //set slider max value
        themeProgressSlider.maxValue = totalCompleteSections;
        //themeBtnComponent.themeProgressSlider.value = comp;

        //slider animator
        themeProgressSlider.GetComponent<EX_SliderAnimator>().AnimateTo(completeSections, 0.5f);

        //fill slider with theme color
        Image fillImage = themeProgressSlider.fillRect.GetComponent<Image>();

        //set slider color
        Color tColor = sectionManager.themeHeaderColor;
        //tColor.a = 0.5f; // 50% opacity

        fillImage.color = tColor;

        UpdateThemeBtnUI();
    }

}
