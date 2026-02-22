using System.Collections;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;
using System.Threading.Tasks;

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

    //public LeaderboardManager leaderboardManager;

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

        // Now load your data IMPORTANT
        LoadData();
    }

    //IMPORTANT
    public bool LoadData()
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

            return true;
        }

        return false;
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

            if (!section.isBundle && section.questions.Length > 0 && section.questions != null)
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
        if (sectionPanelPrefab == null)
            return;

        //prefab instance
        GameObject panel = Instantiate(sectionPanelPrefab, sectionsRectTransform);
        RectTransform rt = panel.GetComponent<RectTransform>();
        rt.localScale = Vector3.one;
        rt.localPosition = Vector3.zero;

        //naming
        string sectionName = section.name;
        panel.name = sectionName;
       
        //upd section data in db
        dbUtils.EnsureSectionExists(panel.name);

        //get controller
        SectionPanel sectionPanel = panel.GetComponent<SectionPanel>();
        sectionPanel.Initialize(section);

        //set icon
        if (section.sectionIcon != null)
            sectionPanel.sectionImage.sprite = section.sectionIcon;

        //set header and description
        sectionPanel.sectionHeaderText.text = sectionPanel.GetTitle(section);
        sectionPanel.sectionDescriptionText.text = sectionPanel.GetDescription(section);

        //set liked state
        bool isLiked = dbUtils.GetSectionLikedStatus(sectionName);
        sectionPanel.SetLikedState(isLiked);

        // Get question count regardless of type
        int questionsCount = GetQuestionCount(section);

        // Set progress slider
        if (questionsCount > 0)
        {
            sectionPanel.progressSlider.maxValue = questionsCount;
        }

        //get progress
        int progress = dbUtils.GetSectionProgress(section.name);
        sectionPanel.progressSlider.value = progress;

        //get time
        float time = dbUtils.GetSectionTime(section.name);
        sectionPanel.sectionTimeText.text = FormatTime(time);

        //get result
        int result = dbUtils.GetSectionResult(section.name);
        string fromTxt = LocalizationSettings.StringDatabase.GetLocalizedString("LTLRN", "FromSTxt");
        string resultText = $"{result} {fromTxt} {questionsCount}";
        sectionPanel.sectionResultText.text = resultText;

        //set data
        sectionPanel.currentSection = section;
        sectionPanel.sectionIndex = i;        
    }

    private void LoadBundle(Section section, int i)
    {
        if (bundlePanelPrefab == null)
            return;

        //instance object
        GameObject panel = Instantiate(bundlePanelPrefab, sectionsRectTransform);
        RectTransform rt = panel.GetComponent<RectTransform>();
        rt.localScale = Vector3.one;
        rt.localPosition = Vector3.zero;

        //set name
        string sectionName = section.name;
        panel.name = sectionName;

        //upd section data in db. Set bundle state to true
        dbUtils.EnsureSectionExists(sectionName, true);

        //get controller
        SectionPanel sectionPanel = panel.GetComponent<SectionPanel>();
        sectionPanel.Initialize(section);

        //set bundel flag
        sectionPanel.isBundleSection = true;

        //get icon
        if (section.sectionIcon != null)
            sectionPanel.sectionImage.sprite = section.sectionIcon;

        int bundleLenght = section.bundleSections.Length;
        //Debug.Log($"Bundle '{section.name}' has {bundleLenght} sections.");

        //set slider max value to bundle length
        sectionPanel.progressSlider.maxValue = bundleLenght;

        //set liked state
        bool isLiked = dbUtils.GetSectionLikedStatus(sectionName);
        sectionPanel.SetLikedState(isLiked);

        int bundleProgress = 0;
        float bundleTime = 0;
        int bundleResult = 0;
        int bundleQuestionCount = 0;

        //BACKLOGIC: get total questions in bundle
        for (int j = 0; j < bundleLenght; j++)
        {
            //upd section data in db. Set bundle state to true
            dbUtils.EnsureSectionExists(section.bundleSections[j].name);

            bool complete = dbUtils.GetSectionComplete(section.bundleSections[j].name);
            
            if(complete)
                bundleProgress++;

            //get result
            int result = dbUtils.GetSectionResult(section.bundleSections[j].name);
            bundleResult += result;

            // Get question count regardless of type
            int questionsCount = 0;
            if (section.bundleSections[j].sectionType != Section.SectionType.LearnType01)
                questionsCount = GetQuestionCount(section.bundleSections[j]);
            
            bundleQuestionCount += questionsCount;

            //get-set time
            float time = dbUtils.GetSectionTime(section.bundleSections[j].name);           
            bundleTime += time;

            //transfer bundle sections
            sectionPanel.bundleSections = section.bundleSections;
        }

        //set topic count
        float percentTopic = (float)bundleProgress / bundleLenght * 100f;
        sectionPanel.topicsCount.text = $"{percentTopic:0}%";

        //sectionPanel.topicsCount.text = $"{bundleProgress}/{bundleLenght}"; ;

        //set slider
        sectionPanel.progressSlider.value = bundleProgress;
        
        //set time
        sectionPanel.sectionTimeText.text = FormatTime(bundleTime);

        //set result
        float percentQ = (float)bundleResult / bundleQuestionCount * 100f;
        
        //string resultText = $"{bundleResult}/{bundleQuestionCount}";
        sectionPanel.sectionResultText.text = $"{percentQ:0}%";

        sectionPanel.currentSection = section;
    }


    // Helper method to get question count
    public int GetQuestionCount(Section section)
    {
        if (section.questions != null && section.questions.Length > 0)
            return section.questions.Length;

        return 0;
    }

}
