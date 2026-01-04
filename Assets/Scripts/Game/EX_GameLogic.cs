
using System.Collections;
using TMPro;
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
        public string[] answerSecondWord;
        public string[] qSoundClipName;
        public string[] questionImageFile;
        public int correctAnswerNumber;
        public string questionCategory;
    }

    [System.Serializable]
    public class SectionData
    {
        public string infoText;
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

    public string sectionInfo;

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
    public ButtonImage nextBtn;

    [Header("Score")]
    public int tempScore = 0;

    [Header("Log")]
    public TMP_Text log;

    private int qCounter = 0;

    //timers
    private float sessionStartTime;
    public float sessionDuration;

    private const float CHECK_DELAY = 1f;

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

            //losd info           
        }

        //load section IMPORTANT
        if (currentSection != null)
            SLoad(currentSection);

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

    public static SectionData LoadSectionData(Section section)
    {
        SectionData sectionData = new SectionData();

        if (section == null)
        {
            sectionData.infoText = "NULL";
            return sectionData;
        }

        string resolvedText = null;

        if (section.sectionInfo != null && DBUtils.Instance != null)
        {
            resolvedText = DBUtils.Instance.ResolveReference(section.sectionInfo);
        }

        sectionData.infoText = string.IsNullOrEmpty(resolvedText)
            ? "NULL"
            : resolvedText;

        return sectionData;
    }


    //prepare data for question. Step 1
    public static QuestionData LoadQuestionData(QuestionT01 question)
    {       
        //create question data
        QuestionData data = new QuestionData();

        //load q text from db
        data.questionText = DBUtils.Instance.ResolveReference(question.questionReference);

        // get answer count
        int answersCount = 0;

        if(question.answerReferences != null)
            answersCount = question.answerReferences.Length;

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

        int secondWordCount = 0;

        //optional 2nd word answers
        if (question.answerSecondWord != null) 
            secondWordCount = question.answerSecondWord.Length;

        if (secondWordCount > 0)
        {
            //set answers
            data.answerSecondWord = new string[secondWordCount];
            //load answers
            for (int i = 0; i < question.answerSecondWord.Length; i++)
            {
                data.answerSecondWord[i] = DBUtils.Instance.ResolveReference(question.answerSecondWord[i]);
            }
        }

        // Load sound clips
        int soundCount = 0;

        if(question.soundReferences != null)
            soundCount = question.soundReferences.Length;

        if (soundCount > 0)
        {
            data.qSoundClipName = new string[soundCount];

            for (int i = 0; i < question.soundReferences.Length; i++)
            {
                data.qSoundClipName[i] = DBUtils.Instance.ResolveReference(question.soundReferences[i]);
            }
        }

        //get image count
        int imgCount = 0;

        if(question.questionImageFile != null)
            imgCount = question.questionImageFile.Length;

        if (imgCount > 0)
        {
            data.questionImageFile = new string[imgCount];

            for (int i = 0; i < question.questionImageFile.Length; i++)
            {
                data.questionImageFile[i] = DBUtils.Instance.ResolveReference(question.questionImageFile[i]);
            }
        }

        data.correctAnswerNumber = (int)question.correctAnswerNumber;

        data.questionCategory = DBUtils.Instance.ResolveReference(question.questionCategory);

        return data;
    }

    private void SLoad(Section section)
    {
        SectionData sectionData = LoadSectionData(section);

        if (sectionData == null) return;

        //load rule from DB
        if (sectionData.infoText.Length > 0)
            sectionInfo = sectionData.infoText;
        else
            sectionInfo = "No data";                 
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

        // Text only question
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
                qData.qImagePanel.gameObject.SetActive(false); //hide image in type 1
                qData.ActivateInputField(false);

                qData.soundBtn.GetComponent<ButtonImage>().SetDisabled(false);

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
                    //text from database 1 and 2 word
                    qData.SetAnswers(data.answerVariantsText, data.answerSecondWord);
                }
                else
                {
                    //text from database
                    qData.SetAnswers(data.answerVariantsText);
                }

                //load sound
                if (data.qSoundClipName != null && data.qSoundClipName.Length > 0)
                    qData.qAudioClip = soundManager.LoadAudioClipByName(data.qSoundClipName[0]);
                else
                    qData.soundBtn.GetComponent<ButtonImage>().SetDisabled(true);
            }
        }

        //IMAGE
        if (question.questionType == QuestionType.Type2)
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
                qData.soundBtn.GetComponent<ButtonImage>().SetDisabled(false);

                qData.qestionText.gameObject.SetActive(false); //hide text in type 2

                qData.ActivateInputField(false);

                //load image
                if (data.questionCategory != null)
                {
                    qData.qImagePanel.gameObject.SetActive(true);

                    //load images
                    for (int i = 0; i < data.questionImageFile.Length; i++)
                    {
                        GameObject newImg = Instantiate(qData.imagePrefab, qData.qImagePanel);
                        newImg.name = "QImage_"+i;
                        Image img = newImg.GetComponent<Image>();
                        img.sprite = DBUtils.Instance.LoadSpriteByName(data.questionCategory, data.questionImageFile[i]);
                        img.SetNativeSize();
                        img.color = question.questionImageColor;
                        float randomZ = Random.Range(-5f, 5f);
                        img.rectTransform.localEulerAngles = new Vector3(0f, 0f, randomZ);
                    }

                    //load sound
                    if (data.qSoundClipName != null && data.qSoundClipName.Length > 0)
                        qData.qAudioClip = soundManager.LoadAudioClipByName(data.qSoundClipName[0]);
                    else
                        qData.soundBtn.GetComponent<ButtonImage>().SetDisabled(true);
                }

                //set answers
                if (question.isAnswerTextOnly)
                {
                    //text from question object
                    qData.SetAnswers(question.answerVariantsText);
                }
                else
                {
                    //text from database 1 and 2 word
                    qData.SetAnswers(data.answerVariantsText, data.answerSecondWord);
                }
            }
        }

        //input
        if (question.questionType == QuestionType.Type3)
        {
            questionLoadStep01(question);

            //load data to prefab
            ExQManager01 qData = questionInstance.GetComponent<ExQManager01>();
            if (qData != null)
            {
                qData.soundBtn.GetComponent<ButtonImage>().SetDisabled(false);

                qData.ActivateInputField(true);
                qData.answerPanel.SetActive(false);

                qData.qImagePanel.gameObject.SetActive(false);

                //load question text
                if (question.isQuestionTextOnly)
                {
                    //text from question object
                    qData.qestionText.text = question.questionText;
                }
                else
                {
                    //text from database
                    qData.qestionText.text = data.questionText;
                }
            }
        }

    }

    private void questionLoadStep01(QuestionT01 question)
    {
        questionInstance = Instantiate(questionPrefab, questionArea);
        questionInstance.name = question.name;

        RectTransform rt = questionInstance.GetComponent<RectTransform>();
        rt.localScale = Vector3.one;
        rt.anchoredPosition = Vector2.zero;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
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
                StartCoroutine(ResultProcessingDelayed(CHECK_DELAY)); // wait 1.5 seconds
            }
        }
    }

    public void CheckInput(string input)
    {
        gameState = GameState.Pause;

        //load data to prefab
        ExQManager01 qData = questionInstance.GetComponent<ExQManager01>();

        //get correct index
        int correctIndex = (int)currentQuestion.correctAnswerNumber;

        string correctAnswerText = string.Empty;

        if (currentQuestion.answerReferences != null && currentQuestion.answerReferences[correctIndex] != null)
        {
            correctAnswerText = currentQuestion.answerReferences[correctIndex].value;
        }        

        //get question
        QuestionT01 question = currentQuestion;

        //check selected Type 1
        if (currentSection.sectionType == Section.SectionType.Type1)
        {
            if (input == correctAnswerText)
            {
                // Add score, show success animation, etc.
                tempScore = tempScore + currentQuestion.rewardAmount;

                qData.CheckInputAnswer(input, true);
            }
            else
            {
                qData.CheckInputAnswer(input, false);                
            }

            if (qCounter < currentSection.questions.Length - 1)
            {
                nextButton.interactable = true;
                nextBtn.RefreshState();
            }
            else
            {
                gameState = GameState.Finish;
                StartCoroutine(ResultProcessingDelayed(CHECK_DELAY)); // wait 1.5 seconds
            }
        }
    }

    private IEnumerator ResultProcessingDelayed(float delay)
    {
        yield return new WaitForSeconds(delay);
        ResultProcessing();
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
