using System.Collections;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;

public class ExDataLoader : MonoBehaviour
{
    private enum QuestionDataType
    {
        None,
        Type1,
        Type2
    }

    private GameData gameData;
    private DBUtils dbUtils;
    private LanguageSwitcher locManager;

    public Themes themes;

    public Button themeButton;
    [SerializeField] private TMP_Text themeNameTargetLangTxt;

    //public SectionManager[] themes;

    [Header("Data")]
    public Section sectionClass;
    public SectionManager sectionManager;

    public SectionManager tempSectionManager;
    public int tempThemeIndex;

    [Header("Stat")]
    public int totalQuestions;
    public int totalSections;

    [Header("UI Prefabs")]
    public GameObject sectionPanelPrefab;
    //public GameObject questionBtnPrefab;

    [Header("Bundel UI Prefab")]
    public GameObject bundlePanelPrefab;

    [Header("Content Area")]
    public RectTransform sectionsRectTransform;

    private void Start()
    {
        StartCoroutine(WaitAndLoadData());
    }
    private IEnumerator WaitAndLoadData()
    {
        // Find DBUtils (with safety check)
        GameObject dbUtilsObj = GameObject.FindWithTag("DBUtils");

        if (dbUtilsObj == null)
        {
            Debug.LogError("DBUtils GameObject with tag 'DBUtils' not found!");
            yield break;
        }

        dbUtils = dbUtilsObj.GetComponent<DBUtils>();

        if (dbUtils == null)
        {
            Debug.LogError("DBUtils component not found on GameObject!");
            yield break;
        }

        // Wait for database to be ready
        while (!dbUtils.IsReady)
        {
            yield return null;
        }

        //Debug.Log("Database is ready! Loading data...");

        // Now load your data IMPORTANT
        LoadData();
    }

    //IMPORTANT
    public void LoadData()
    {
        gameData = GameObject.FindWithTag("GameData").GetComponent<GameData>();
        locManager = GameObject.FindWithTag("LangSwitcher").GetComponent<LanguageSwitcher>();

        //load theme
        if (gameData != null)
            sectionManager = themes.theme[gameData.saveData.selectedThemeIndex];

        if (sectionManager != null)
        {
            //get total levels
            totalQuestions = sectionManager.GetTotalQuestionCount();
            totalSections = sectionManager.sections.Length;
            
            //get data
            ButtonImage themeBtn = themeButton.GetComponent<ButtonImage>();
            Locale locale = null;

            //get locale
            if (locManager != null)
                locale = locManager.GetLocale();

            //set theme button name
            if (locale != null && themeBtn != null)
            {
                themeBtn.buttonTextStr = sectionManager.GetThemeName(sectionManager, locale);
                themeBtn.RefreshState();
            }

            //set name on target lang
            themeNameTargetLangTxt.text = sectionManager.themeNameTargetLang;

            //Open theme button menu icon
            //themeBtn.buttonIcon.sprite = sectionManager.themeIcon;

            CreateSectionPanels();
        }        
    }

    //IMPORTANT
    private void CreateSectionPanels()
    {
        // Clear old panels (important when reloading)
        foreach (Transform child in sectionsRectTransform)
        {
            Destroy(child.gameObject);
        }

        for (int i = 0; i < totalSections; i++)
        {
            // OPTIONAL: initialize panel data
            Section section = sectionManager.sections[i];

            if (section.questions.Length > 0 && section.questions != null)
            {
                LoadSections(section, i); //section type1
            }
            
            //load bundle sections            
            if (section.isBundle && section.bundleSections.Length > 0 && section.bundleSections != null)
            {
                LoadBundle(section, i);
            }
        }
    }

    public string FormatTime(float seconds)
    {
        int minutes = Mathf.FloorToInt(seconds / 60f);
        int secs = Mathf.FloorToInt(seconds % 60f);
        return $"{minutes:00}:{secs:00}";
    }

    private void LoadSections(Section section, int i)
    {
        GameObject panel = Instantiate(sectionPanelPrefab, sectionsRectTransform);
        RectTransform rt = panel.GetComponent<RectTransform>();
        rt.localScale = Vector3.one;
        rt.localPosition = Vector3.zero;

        string sectionName = section.name;
        panel.name = sectionName;

        //Debug.Log("Loading section: " + panel.name);

        //upd section data in db
        dbUtils.EnsureSectionExists(panel.name);

        SectionPanel sectionPanel = panel.GetComponent<SectionPanel>();
        sectionPanel.Initialize(section);

        if (section.sectionIcon != null)
            sectionPanel.sectionImage.sprite = section.sectionIcon;

        sectionPanel.sectionHeaderText.text = sectionPanel.GetTitle(section);
        sectionPanel.sectionDescriptionText.text = sectionPanel.GetDescription(section);

        bool isLiked = dbUtils.GetSectionLikedStatus(sectionName);
        sectionPanel.SetLikedState(isLiked);

        // Get question count regardless of type
        int questionsCount = GetQuestionCount(section);

        // Set progress slider
        if (questionsCount > 0)
        {
            sectionPanel.progressSlider.maxValue = questionsCount;
        }

        int progress = dbUtils.GetSectionProgress(section.name);
        sectionPanel.progressSlider.value = progress;

        float time = dbUtils.GetSectionTime(section.name);
        sectionPanel.sectionTimeText.text = FormatTime(time);

        int result = dbUtils.GetSectionResult(section.name);
        string fromTxt = LocalizationSettings.StringDatabase.GetLocalizedString("LTLRN", "FromSTxt");
        string resultText = $"{result} {fromTxt} {questionsCount}";
        sectionPanel.sectionResultText.text = resultText;

        sectionPanel.currentSection = section;
        sectionPanel.sectionIndex = i;

        //sectionPanel.PlayButtonToggle(questionsCount);
    }

    private void LoadBundle(Section section, int i)
    {
        GameObject panel = Instantiate(bundlePanelPrefab, sectionsRectTransform);
        RectTransform rt = panel.GetComponent<RectTransform>();
        rt.localScale = Vector3.one;
        rt.localPosition = Vector3.zero;

        //setname
        string sectionName = section.name;
        panel.name = sectionName;

        //get controller
        SectionPanel sectionPanel = panel.GetComponent<SectionPanel>();
        sectionPanel.Initialize(section);

        //set bundel flag
        sectionPanel.isBundleSection = true;

        //get icon
        if (section.sectionIcon != null)
            sectionPanel.sectionImage.sprite = section.sectionIcon;

        int bundleLenght = section.bundleSections.Length;

        //BACKLOGIC: get total questions in bundle
        for (int j = 0; j < bundleLenght; j++)
        {
            //get/set progress
            int progress = dbUtils.GetSectionProgress(section.bundleSections[j].name);
            sectionPanel.progressSlider.value = progress;

            //get-set time
            float time = dbUtils.GetSectionTime(section.bundleSections[j].name);
            sectionPanel.sectionTimeText.text = FormatTime(time);

            //transfer bundle sections
            sectionPanel.bundleSections = section.bundleSections;
        }

        sectionPanel.currentSection = section;

        /*        int result = dbUtils.GetSectionResult(section.name);
                string fromTxt = LocalizationSettings.StringDatabase.GetLocalizedString("LTLRN", "FromSTxt");
                string resultText = $"{result} {fromTxt} {questionsCount}";
                sectionPanel.sectionResultText.text = resultText;*/

        // Implement bundle loading logic here
        //Debug.Log("Loading bundled sections...");
    }


    // Helper method to get question count
    private int GetQuestionCount(Section section)
    {
        if (section.questions != null && section.questions.Length > 0)
            return section.questions.Length;

        return 0;
    }

}
