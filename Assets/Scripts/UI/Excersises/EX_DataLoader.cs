using UnityEngine;
using System.Collections;

public class ExDataLoader : MonoBehaviour
{
    private GameData gameData;
    private DBUtils dbUtils;

    public Themes themes;

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
            GameObject panel = Instantiate(sectionPanelPrefab, sectionsRectTransform);
           
            // Ensure correct UI transform values
            RectTransform rt = panel.GetComponent<RectTransform>();
            rt.localScale = Vector3.one;
            rt.localPosition = Vector3.zero;

            // OPTIONAL: initialize panel data
            Section section = sectionManager.sections[i];

            //set UI name

            string sectionName = section.name;
            panel.name = sectionName;

            //db utils
            dbUtils.EnsureSectionExists(panel.name);

            SectionPanel sectionPanel = panel.GetComponent<SectionPanel>();
            sectionPanel.Initialize(section);

            //fill section data
            if (section.sectionIcon != null)
                sectionPanel.sectionImage.sprite = section.sectionIcon;

            //set name and description
            sectionPanel.sectionHeaderText.text = section.sectionTitle;
            sectionPanel.sectionDescriptionText.text = section.sectionDescription;

            //set likes
            bool isLiked = dbUtils.GetSectionLikedStatus(sectionName);
            sectionPanel.SetLikedState(isLiked);

            sectionPanel.currentSection = section;
            sectionPanel.sectionIndex = i;

            //set progress slider max value
            //Sections count
            if (section.questions.Length > 0 && section.questions != null)
            {
                sectionPanel.progressSlider.maxValue = section.questions.Length;
            }

            //disable play button if no questions
            sectionPanel.PlayButtonToggle(section.questions.Length);

            if (section.questions.Length > 0 && section.questions != null)
            {
                for (int j = 0; j < section.questions.Length; j++)
                {
/*                    GameObject button = Instantiate(questionBtnPrefab, sectionPanel.questionsRectTransform);
                    button.name = "S" + i + "Button" + j.ToString();

                    QuestionButton btn = button.GetComponent<QuestionButton>();

                    int fixedNumber = j + 1;

                    if (btn != null)
                        btn.qBtnText.text = $"{fixedNumber}";*/
                }
            }

        }
    }

}
