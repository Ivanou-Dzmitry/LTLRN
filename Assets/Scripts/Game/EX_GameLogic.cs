using System;
using System.Collections;
using TMPro;
using Unity.Collections;
using UnityEngine;
using UnityEngine.UI;
using static QuestionT01;

public enum GameState
{
    Play,  
    Finish,    
    Pause
}

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

    [Header("Game State")]
    public GameState gameState;

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
    //public Button checkButton;
    public Button nextButton;
    public Button interruptGameButton;

    [Header("Answer")]
    public int currentAnswerIndex = -1;

    //private ButtonImage checkBtn;
    private ButtonImage nextBtn;

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
        //checkBtn = checkButton.GetComponent<ButtonImage>();
        nextBtn = nextButton.GetComponent<ButtonImage>();

        //button listeners
        //checkButton.onClick.AddListener(checkBtnClicked);
        nextButton.onClick.AddListener(nextBtnClicked);
        interruptGameButton.onClick.AddListener(OnGameInterrupt);
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

        // IMPORTANT
        LoadGameData();
    }

    private void LoadGameData()
    {
        //get classes
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

        //disable check button
        if(nextButton != null)
        {
            nextButton.interactable = false;
            nextBtn.RefreshState();
        }

        //start time
        sessionStartTime = Time.time;

        //start game
        gameState = GameState.Play;

        tempScore = 0;
    }

    //prepare data for question. Step 1
    public static QuestionData LoadQuestionData(QuestionT01 question)
    {       
        //create question data
        QuestionData data = new QuestionData();

        //load q text from db
        data.questionText = DBUtils.Instance.ResolveReference(question.questionReference);

        // get answer count
        int answersCount = question.answerReferences.Length;

        if (answersCount > 0)
        {
            //set answers
            data.answerVariantsText = new string[answersCount];

            //load answers
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
        //step 2 - load prepared data
        QuestionData data = LoadQuestionData(question);

        //clear previous question
        foreach (Transform child in questionArea)
        {
            Destroy(child.gameObject);
        }

        //step 3 - question type 1 loader
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

    public void Check()
    {
        gameState = GameState.Pause;

        //load data to prefab
        ExQManager01 qData = questionInstance.GetComponent<ExQManager01>();

        //get index
        int selectedIndex = qData.GetSelectedAnswerIndex();
        
        //get correct index
        int correctIndex = (int)currentQuestion.correctAnswerNumber;

        //get question
        QuestionT01 question = currentQuestion;

        //check selected Type 1
        if (currentSection.sectionType == Section.SectionType.Type1)
        {
            //compare indexes
            if (selectedIndex == correctIndex)
            {
                qData.CheckAnswer(selectedIndex, correctIndex);

                // Add score, show success animation, etc.
                tempScore = tempScore + currentQuestion.rewardAmount;

                //Debug.Log("Correct answer! Score: " + tempScore);
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
                gameState = GameState.Finish;
                ResultProcessing();
            }
        }
    }

    private void ResultProcessing()
    {              
        GameFinishRoutine();

        //Debug.Log($"Session duration: {sessionDuration:F2} seconds");

        PanelManager.CloseAll();
        PanelManager.Open("exwin");
    }

    private void GameFinishRoutine()
    {
        //get time
        sessionDuration = Time.time - sessionStartTime;        
    }

    private void nextBtnClicked()
    {
        //srt game state
        gameState = GameState.Play;

        //increase question
        qCounter++;
       
        //load next question
        if (qCounter < currentSection.questions.Length)
        {
            //set progress
            progressBar.value = qCounter + 1;

            //get question
            currentQuestion = currentSection.questions[qCounter];
            
            //question load
            QLoad(currentQuestion);

            //set button state
            nextButton.interactable = false;
            nextBtn.RefreshState();
        }
    }

    private void OnGameInterrupt()
    {
        InterruptGame();
    }

    public void InterruptGame()
    {
        PanelManager.CloseAll();

        //run panel with choise
        PanelManager.Open("exit");        
    }

    public void ExitGame()
    {
        //save progress and time
        SaveGameProgress();
        SaveResult();
        SaveTime();

        PanelManager.CloseAll();

        PanelManager.OpenScene("ExscersisesMenu");                
    }

    public void InterruptedExit()
    {
        //save progress only
        SaveGameProgress();

        PanelManager.CloseAll();

        PanelManager.OpenScene("ExscersisesMenu");
    }

    private void SaveGameProgress()
    {
        if (dbUtils == null) return;

        int currentProgress = (int)progressBar.value;

        int savedProgress = dbUtils.GetSectionProgress(currentSection.name);

        if(currentProgress > savedProgress)
            dbUtils.SetSectionProgress(currentSection.name, currentProgress);
    }

    private void SaveResult()
    {
        //saved result
        int savedResult = dbUtils.GetSectionResult(currentSection.name);

        if(tempScore > savedResult)
            dbUtils.SetSectionResult(currentSection.name, tempScore);

        //questions done
        int done = dbUtils.GetSectionProgress(currentSection.name);

        //set complete
        if (tempScore == done)
            dbUtils.SetSectionComplete(currentSection.name, true);
        else
            dbUtils.SetSectionComplete(currentSection.name, false);
    }

    private void SaveTime()
    {
        float currentTime = sessionDuration;

        float savedTime = dbUtils.GetSectionTime(currentSection.name);

        //avoid initial data = 0
        if(savedTime < 1)
            dbUtils.SetSectionTime(currentSection.name, currentTime);
        else if (currentTime < savedTime)
            dbUtils.SetSectionTime(currentSection.name, currentTime);
    }

    private void SaveProgress()
    {
        //int savedScore = dbUtils.GetSectionProgress(currentSection.name);
    }

    public void NextSection()
    {
        Debug.Log("Go to next section");
    }

    private void OnDestroy()
    {
        nextButton.onClick.RemoveListener(nextBtnClicked);
        interruptGameButton.onClick.RemoveListener(OnGameInterrupt);
    }

    public string FormatTime(float seconds)
    {
        int minutes = Mathf.FloorToInt(seconds / 60f);
        int secs = Mathf.FloorToInt(seconds % 60f);
        return $"{minutes:00}:{secs:00}";
    }

}
