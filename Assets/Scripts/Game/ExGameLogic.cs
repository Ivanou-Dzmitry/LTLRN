using TMPro;
using UnityEngine;
using UnityEngine.Diagnostics;

public class ExGameLogic : MonoBehaviour
{
    private GameData gameData;
    private DBUtils dbUtils;

    public Themes themes;
    public SectionManager sectionManager;
    public Section currentSection;
    public Question currentQuestion;

    [Header("Log")]
    public TMP_Text log;

    private void Awake()
    {
        PanelManager.Open("exgamemain");
    }

    private void Start()
    {
      LoadGameData();
    }

    private void LoadGameData()
    {
        gameData = GameObject.FindWithTag("GameData").GetComponent<GameData>();
     
        //load theme
        if (gameData != null)
        {
            sectionManager = themes.theme[gameData.saveData.selectedThemeIndex];
            
            if(sectionManager != null)
                currentSection = sectionManager.sections[gameData.saveData.selectedSectionIndex];   

            if(currentSection != null)
                currentQuestion = currentSection.questions[0];

            Debug.Log(currentQuestion.uID);
        }

        dbUtils = GameObject.FindWithTag("DBUtils").GetComponent<DBUtils>();

        log.text = dbUtils.CheckConnection();
    }

}
