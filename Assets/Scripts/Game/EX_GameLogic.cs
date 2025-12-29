using TMPro;
using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using static QuestionT01;

public class ExGameLogic : MonoBehaviour
{
    [System.Serializable]
    public class QuestionData
    {
        public string questionText;
        public string[] answerVariantsText;
        public string[] qSoundClipName;
        public string qSpriteFile;
        public int correctAnswerNumber;
    }

    private GameData gameData;
    private DBUtils dbUtils;
    private SoundManager soundManager;

    [Header("Game Info")]
    public Slider progressBar;

    [Header("Data")]
    public Themes themes;
    public SectionManager sectionManager;
    public Section currentSection;
    public QuestionT01 currentQuestion;

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
        StartCoroutine(WaitAndLoadData());
    }

    private IEnumerator WaitAndLoadData()
    {
        // Find DBUtils - Updated
        dbUtils = FindFirstObjectByType<DBUtils>();

        if (dbUtils == null)
        {
            Debug.LogError("DBUtils not found in scene!");
            yield break;
        }

        // Wait for database to be ready
        while (!dbUtils.IsReady)
        {
            yield return null;
        }

        //Debug.Log("Database is ready!");
        LoadGameData();
    }

    private void LoadGameData()
    {
        gameData = GameObject.FindWithTag("GameData").GetComponent<GameData>();
        soundManager = GameObject.FindWithTag("SoundManager").GetComponent<SoundManager>();

        //load theme
        if (gameData != null)
        {
            //load selected theme
            sectionManager = themes.theme[gameData.saveData.selectedThemeIndex];

            //load selected section
            if (sectionManager != null)
                currentSection = sectionManager.sections[gameData.saveData.selectedSectionIndex];

            //load first question
            if (currentSection != null)
                currentQuestion = currentSection.questions[0];            
        }

        //load question IMPORTANT
        if (currentQuestion != null)
            QLoad(currentQuestion);

        //set progress bar
        progressBar.maxValue = currentSection.questions.Length;
        progressBar.value = 1;

        nextButton.interactable = false;
        nextBtn.RefreshState();

        sessionStartTime = Time.time;
    }

    public static QuestionData LoadQuestionData(QuestionT01 question)
    {       
        QuestionData data = new QuestionData();

        data.questionText = DBUtils.Instance.ResolveReference(question.questionReference);

        // Load answer texts
        int answersCount = question.answerReferences.Length;

        if (answersCount > 0)
        {
            data.answerVariantsText = new string[answersCount];

            for (int i = 0; i < question.answerReferences.Length; i++)
            {
                data.answerVariantsText[i] = DBUtils.Instance.ResolveReference(question.answerReferences[i]);
            }
        }

        // Load sound clips
        int sondCount = question.soundReferences.Length;

        if (sondCount > 0)
        {
            data.qSoundClipName = new string[sondCount];

            for (int i = 0; i < question.soundReferences.Length; i++)
            {
                data.qSoundClipName[i] = DBUtils.Instance.ResolveReference(question.soundReferences[i]);
            }
        }

        // Copy other data
        //data.questionText = question.questionText;
        data.qSpriteFile = question.qSpriteFile;
        data.correctAnswerNumber = (int)question.correctAnswerNumber;

        return data;
    }

    private void QLoad(QuestionT01 question)
    {
        //step 1 - load DB
        QuestionData data = LoadQuestionData(question);

        //clear previous question
        foreach (Transform child in questionArea)
        {
            Destroy(child.gameObject);
        }

        //step 2 - question type 1 loader
        if (question.questionType == QuestionType.Type1)
        {
            questionInstance = Instantiate(questionPrefab, questionArea);
            questionInstance.name = question.name;

            RectTransform rt = questionInstance.GetComponent<RectTransform>();
            rt.localScale = Vector3.one;
            rt.anchoredPosition = Vector2.zero;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            //load data to prefab
            ExQManager01 qData = questionInstance.GetComponent<ExQManager01>();

            if (qData != null)
            {
                //load question text
                if (question.isQuestionTextOnly)
                {
                    //text from question object
                    qData.qestionText.text = question.questionText;                         
                }else
                {
                    //text from database
                    qData.qestionText.text = data.questionText;
                }

                //set answers
                if(question.isAnswerTextOnly)
                {
                    //text from question object
                    qData.SetAnswers(question.answerVariantsText);
                }
                else
                {
                    //text from database
                    qData.SetAnswers(data.answerVariantsText);
                }                

                //load sound
                if(data.qSoundClipName != null && data.qSoundClipName.Length > 0)
                    qData.qAudioClip = soundManager.LoadAudioClipByName(data.qSoundClipName[0]);
            }
        }

    }


/*    private void QuestionLoader(Question question)
    {
        foreach (Transform child in questionArea)
        {
            Destroy(child.gameObject);
        }

        //question type 1 loader
        if (question.questionType == QuestionType.Type1)
        {
            questionInstance = Instantiate(questionPrefab, questionArea);
            questionInstance.name = question.name;

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
    }*/

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

        QuestionT01 question = currentQuestion;

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
                //Debug.Log("Wrong answer!");
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
            QLoad(currentQuestion);

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
