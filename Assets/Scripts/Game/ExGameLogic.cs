using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static Question;


public class ExGameLogic : MonoBehaviour
{
    private GameData gameData;
    private DBUtils dbUtils;

    [Header("Game Info")]
    public Slider progressBar;

    [Header("Data")]
    public Themes themes;
    public SectionManager sectionManager;
    public Section currentSection;
    public Question currentQuestion;

    private GameObject questionInstance;

    [Header("Question Stuff")]
    public Transform questionArea;
    public GameObject questionPrefab;

    [Header("Buttons")]
    public Button checkButton;
    public Button nextButton;
    public Button exitGameButton;

    [Header("Answer")]
    public int currentAnswerIndex = -1;

    private ButtonImage checkBtn;
    private ButtonImage nextBtn;
    private ButtonImage exitBtn;

    [Header("Score")]
    public int tempScore = 0;

    [Header("Log")]
    public TMP_Text log;

    private int qCounter = 0;

    //timers
    private float sessionStartTime;
    public float sessionDuration;

    private void Awake()
    {
        PanelManager.Open("exgamemain");

        //buttons
        checkBtn = checkButton.GetComponent<ButtonImage>();
        nextBtn = nextButton.GetComponent<ButtonImage>();
        exitBtn = exitGameButton.GetComponent<ButtonImage>();

        //button listeners
        checkButton.onClick.AddListener(checkBtnClicked);
        nextButton.onClick.AddListener(nextBtnClicked);
        exitGameButton.onClick.AddListener(exitBtnClicked);
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
        }

        dbUtils = GameObject.FindWithTag("DBUtils").GetComponent<DBUtils>();

        if(log!=null)
            log.text = dbUtils.CheckConnection();

        if(currentQuestion != null)
            QuestionLoader(currentQuestion);

        //set progress bar
        progressBar.maxValue = currentSection.questions.Length;
        progressBar.value = 1;

        nextButton.interactable = false;
        nextBtn.RefreshState();

        sessionStartTime = Time.time;
    }


    private void QuestionLoader(Question question)
    {
        foreach (Transform child in questionArea)
        {
            Destroy(child.gameObject);
        }

        //question type 1 loader
        if (question.questionType == QuestionType.Type1)
        {
            questionInstance = Instantiate(questionPrefab, questionArea);
            questionInstance.name = "Q" + question.name;

            RectTransform rt = questionInstance.GetComponent<RectTransform>();
            rt.localScale = Vector3.one;
            rt.anchoredPosition = Vector2.zero;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            //load data to prefab
            ExQManager01 qData = questionInstance.GetComponent<ExQManager01>();

            if (qData != null)
            {
                qData.qestionText.text = question.questionText;

                qData.qestionText.text = question.questionText;
                qData.SetAnswers(question.answerVariantsText);
            }
        }     
    }

    private void checkBtnClicked()
    {
/*        if (qCounter < currentSection.questions.Length - 1)
        {
            checkButton.interactable = false;
            checkBtn.RefreshState();

            nextButton.interactable = true;
            nextBtn.RefreshState();
        }else
        {
            checkButton.interactable = false;
            checkBtn.RefreshState();

            PanelManager.CloseAll();
            PanelManager.Open("exwin");
        }

        Check();*/
    }

    public void Check()
    {
        //load data to prefab
        ExQManager01 qData = questionInstance.GetComponent<ExQManager01>();

        int selectedIndex = qData.GetSelectedAnswerIndex();
        int correctIndex = (int)currentQuestion.correctAnswerNumber;

        Question question = currentQuestion;

        if (currentSection.sectionType == Section.SectionType.Type1)
        {
            if (selectedIndex == correctIndex)
            {
                qData.CheckAnswer(selectedIndex, correctIndex);

                // Add score, show success animation, etc.
                tempScore = tempScore + currentQuestion.rewardAmount;

                Debug.Log("Correct answer! Score: " + tempScore);
            }
            else
            {
                Debug.Log("Wrong answer!");
                qData.CheckAnswer(selectedIndex, correctIndex);
                // Deduct lives, show error animation, etc.
            }

            if (qCounter < currentSection.questions.Length - 1)
            {
                nextButton.interactable = true;
                nextBtn.RefreshState();
            }
            else
            {
                ResultProcessing();
            }
        }
    }

    private void ResultProcessing()
    {
        sessionDuration = Time.time - sessionStartTime;

        Debug.Log($"Session duration: {sessionDuration:F2} seconds");

        PanelManager.CloseAll();
        PanelManager.Open("exwin");
    }

    private void nextBtnClicked()
    {
        qCounter++;
       

        //load next question
        if (qCounter < currentSection.questions.Length)
        {
            progressBar.value = qCounter + 1;

            currentQuestion = currentSection.questions[qCounter];
            QuestionLoader(currentQuestion);

            nextButton.interactable = false;
            nextBtn.RefreshState();

/*            checkButton.interactable = true;
            checkBtn.RefreshState();*/
        }

        //Debug.Log(qCounter);
    }

    private void exitBtnClicked()
    {
        Debug.Log("Exit button clicked");
    }

}
