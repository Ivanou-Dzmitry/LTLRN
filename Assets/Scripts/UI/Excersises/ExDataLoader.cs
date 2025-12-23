using UnityEngine;

public class ExDataLoader : MonoBehaviour
{
    private GameData gameData;

    public SectionManager[] themes;

    public Section sectionClass;
    public SectionManager sectionManager;
    public Question question;

    public SectionManager tempSectionManager;
    public int tempThemeIndex;

    public int totalQuestions;
    public int totalSections;

    [Header("UI Prefabs")]
    public GameObject sectionPanelPrefab;
    public GameObject questionBtnPrefab;

    public RectTransform sectionsRectTransform;

    private void Start()
    {
        LoadData();
    }

    //IMPORTANT
    public void LoadData()
    {
        gameData = GameObject.FindWithTag("GameData").GetComponent<GameData>();

        //load theme
        if(gameData != null)
            sectionManager = themes[gameData.saveData.selectedThemeIndex];

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

            panel.name = "SectionPanel_" + i.ToString();

            // Ensure correct UI transform values
            RectTransform rt = panel.GetComponent<RectTransform>();
            rt.localScale = Vector3.one;
            rt.localPosition = Vector3.zero;

            // OPTIONAL: initialize panel data
            Section section = sectionManager.sections[i];

            SectionPanel sectionPanel = panel.GetComponent<SectionPanel>();
            sectionPanel.Initialize(section);

            //fill section data
            if (section.sectionIcon != null)
                sectionPanel.sectionImage.sprite = section.sectionIcon;

            sectionPanel.sectionHeaderText.text = section.sectionTitle;
            sectionPanel.sectionDescriptionText.text = section.sectionDescription;

            /*            if (section.questions == null || section.questions.Length == 0)
                            return;*/

            if (section.questions.Length > 0 && section.questions != null)
            {
                for (int j = 0; j < section.questions.Length; j++)
                {
                    GameObject button = Instantiate(questionBtnPrefab, sectionPanel.questionsRectTransform);
                    button.name = "S" + i + "Button" + j.ToString();

                    QuestionButton btn = button.GetComponent<QuestionButton>();

                    int fixedNumber = j + 1;

                    if (btn != null)
                        btn.qBtnText.text = $"{fixedNumber}";
                }
            }

        }
    }

}
