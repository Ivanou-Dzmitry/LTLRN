using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;

public class ExDataLoader : MonoBehaviour
{
    private GameData gameData;
    private DBUtils dbUtils;

    public Themes themes;

    public Button themeButton;

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
    public GameObject questionBtnPrefab;

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

        // Now load your data
        LoadData();
    }

    //IMPORTANT
    public void LoadData()
    {
        gameData = GameObject.FindWithTag("GameData").GetComponent<GameData>();        

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
            Locale locale = GetLocale();

            //set theme button name
            if (locale != null && themeBtn != null)
            {
                themeBtn.buttonTextStr = sectionManager.GetThemeName(sectionManager, locale);
                themeBtn.RefreshState();
            }

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
                GameObject panel = Instantiate(sectionPanelPrefab, sectionsRectTransform);

                // Ensure correct UI transform values
                RectTransform rt = panel.GetComponent<RectTransform>();
                rt.localScale = Vector3.one;
                rt.localPosition = Vector3.zero;

                //set UI name
                string sectionName = section.name;
                panel.name = sectionName;

                //db utils
                dbUtils.EnsureSectionExists(panel.name);

                //get section
                SectionPanel sectionPanel = panel.GetComponent<SectionPanel>();
                sectionPanel.Initialize(section);

                //fill section data
                if (section.sectionIcon != null)
                    sectionPanel.sectionImage.sprite = section.sectionIcon;

                //set name ru en
                sectionPanel.sectionHeaderText.text = sectionPanel.GetTitle(section);

                //description loader ru en
                sectionPanel.sectionDescriptionText.text = sectionPanel.GetDescription(section);

                //set likes
                bool isLiked = dbUtils.GetSectionLikedStatus(sectionName);
                sectionPanel.SetLikedState(isLiked);

                //set progress slider max value
                if (section.questions.Length > 0 && section.questions != null)
                {
                    sectionPanel.progressSlider.maxValue = section.questions.Length;
                }
                //set progress slider max value
                Debug.Log("1: " + section.questions.Length);

                //get progress from BD
                int progress = dbUtils.GetSectionProgress(section.name);
                Debug.Log("2: "+progress);
                sectionPanel.progressSlider.value = progress;

                //get time from DB
                float time = dbUtils.GetSectionTime(section.name);
                sectionPanel.sectionTimeText.text = FormatTime(time);

                sectionPanel.currentSection = section;
                sectionPanel.sectionIndex = i;
              
                //disable play button if no questions
                sectionPanel.PlayButtonToggle(section.questions.Length);

                if (section.questions.Length > 0 && section.questions != null)
                {
                    for (int j = 0; j < section.questions.Length; j++)
                    {
                        //optional
                    }
                }
            }
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

    public string FormatTime(float seconds)
    {
        int minutes = Mathf.FloorToInt(seconds / 60f);
        int secs = Mathf.FloorToInt(seconds % 60f);
        return $"{minutes:00}:{secs:00}";
    }

}
