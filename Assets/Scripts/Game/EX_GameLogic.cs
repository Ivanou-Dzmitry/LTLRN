
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
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
    private ScoreManager scoreManager;

    [Header("Game State")]
    public GameState gameState;

    [Header("Game Info")]
    public Slider progressBar;
    private Coroutine progressRoutine;

    [Header("Data")]
    public Themes themes;
    public SectionManager sectionManager;
    public Section currentSection;
    private Section nextSection;
    public QuestionT01 currentQuestion;

    //shuffle questions
    private List<QuestionT01> tempQuestions;

    public string sectionInfo;

    private GameObject questionInstance;

    [Header("Question Stuff")]
    public Transform questionArea;
    public GameObject[] questionPrefabs;

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

    private const float CHECK_DELAY = 3f;

    private ExQManager01 qData;

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
        scoreManager = GameObject.FindWithTag("ScoreManager").GetComponent<ScoreManager>();

        //load theme
        if (gameData != null)
        {
            //load selected theme
            sectionManager = themes.theme[gameData.saveData.selectedThemeIndex];

            //load selected section
            if (sectionManager != null)
                currentSection = sectionManager.sections[gameData.saveData.selectedSectionIndex];
            
            // Create runtime copy of questions
            tempQuestions = new List<QuestionT01>(currentSection.questions);

            ShuffleQuestions(tempQuestions);

            if (tempQuestions != null && tempQuestions.Count > 0)
                currentQuestion = tempQuestions[0];

            //load first question
            /*            if (currentSection != null)
                            currentQuestion = currentSection.questions[0];*/

            //losd info           
        }
        else
        {
            Debug.LogError("GameData not found in scene!");            
        }

        if(soundManager != null)
        {
            Debug.Log("SoundManager OK!");

            soundManager.LoadSoundData();
        }
        else
        {
            Debug.LogError("SoundManager not found in scene!");
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
            nextBtn.PlayAnimation(false, "Idle");
            nextBtn.RefreshState();
        }

        //start time
        sessionStartTime = Time.time;

        //start game
        gameState = GameState.Play;

        tempScore = 0;
    }


    void ShuffleQuestions(List<QuestionT01> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);

            QuestionT01 temp = list[i];
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
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

    //load section
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

    //load question
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
            //routine to load UI    
            QuestionUILoad(question, 0);

            //load data to prefab
            qData = questionInstance.GetComponent<ExQManager01>();

            if (qData != null)
            {
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
                    qData.qAudioClip = soundManager.LoadAudioClipByName(data.questionCategory, data.qSoundClipName[0]);
                else
                    qData.soundBtn.GetComponent<ButtonImage>().SetDisabled(true);
            }
        }

        //IMAGE question
        if (question.questionType == QuestionType.Type2)
        {
            //routine to load UI    
            QuestionUILoad(question, 1);

            //load data to prefab
            qData = questionInstance.GetComponent<ExQManager01>();

            if (qData != null)
            {
                qData.soundBtn.GetComponent<ButtonImage>().SetDisabled(false);

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
                        qData.qAudioClip = soundManager.LoadAudioClipByName(data.questionCategory, data.qSoundClipName[0]);
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
            //type3
            QuestionUILoad(question, 2);

            //load data to prefab
            qData = questionInstance.GetComponent<ExQManager01>();

            if (qData != null)
            {
                qData.soundBtn.GetComponent<ButtonImage>().SetDisabled(false);

                if (data.questionCategory != null)
                {
                    //load sound
                    if (data.qSoundClipName != null && data.qSoundClipName.Length > 0)
                        qData.qAudioClip = soundManager.LoadAudioClipByName(data.questionCategory, data.qSoundClipName[0]);
                    else
                        qData.soundBtn.GetComponent<ButtonImage>().SetDisabled(true);
                }

                //load question text
                if (question.isQuestionTextOnly)
                {
                    //text from question object
                    //
                    //qData.qestionText.text = question.questionText;

                    BuildQuestionWithInputs(question.questionText, qData.questionContainer);
                }
                else
                {
                    //text from database
                    qData.qestionText.text = data.questionText;
                }
            }
        }
    }

    void BuildQuestionWithInputs(string source, RectTransform container)
    {
        // Clear previous content
        foreach (Transform child in container)
            Destroy(child.gameObject);        

        qData.AddDataToQuestionContainer(source);
    }


    private void QuestionUILoad(QuestionT01 question, int index)
    {
        questionInstance = Instantiate(questionPrefabs[index], questionArea);
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
                nextBtn.PlayAnimation(true, "Scale");
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

        Debug.Log(correctAnswerText);

        //get question
        QuestionT01 question = currentQuestion;

        //check selected Type 1
        if (currentSection.sectionType == Section.SectionType.Type1)
        {
            if (input == correctAnswerText)
            {
                // Add score, show success animation, etc.
                tempScore = tempScore + currentQuestion.rewardAmount;

                qData.CheckInputAnswer(input, correctAnswerText, true);
            }
            else
            {
                qData.CheckInputAnswer(input, correctAnswerText, false);                
            }

            if (qCounter < currentSection.questions.Length - 1)
            {
                nextButton.interactable = true;
                nextBtn.PlayAnimation(true, "Scale");
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
        PanelManager.Open("exwin"); //run win panel
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
        if (qCounter < currentSection.questions.Length && qCounter < tempQuestions.Count)
        {
            //set progress
            int targetValue = qCounter + 1;
            AnimateProgress(progressBar.value, targetValue, 1f);

            //get question
            currentQuestion = tempQuestions[qCounter];

            //currentQuestion = currentSection.questions[qCounter];            

            //question load
            QLoad(currentQuestion);

            //set button state
            nextButton.interactable = false;
            nextBtn.PlayAnimation(false, "Idle");
            nextBtn.RefreshState();
        }
    }

    void AnimateProgress(float from, float to, float duration)
    {
        if (progressRoutine != null)
            StopCoroutine(progressRoutine);

        progressRoutine = StartCoroutine(AnimateProgressRoutine(from, to, duration));
    }

    IEnumerator AnimateProgressRoutine(float from, float to, float duration)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);

            progressBar.value = Mathf.Lerp(from, to, t);
            yield return null;
        }

        progressBar.value = to;
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

        //save only if increased
        if (tempScore > savedResult)
        {
            dbUtils.SetSectionResult(currentSection.name, tempScore);
            scoreManager.SaveCrystals(tempScore);
        }
            
        //questions done
        int done = dbUtils.GetSectionProgress(currentSection.name);

        bool complete = dbUtils.GetSectionComplete(currentSection.name);

        //set complete and stars
        if (tempScore == done && complete == false)
        {
            dbUtils.SetSectionComplete(currentSection.name, true);
            scoreManager.AddStar(1);
        }            
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


    public bool GetNextSection()
    {
        int nextIndex = gameData.saveData.selectedSectionIndex + 1;

        try
        {
            nextSection = sectionManager.sections[nextIndex];
            Debug.Log("Go to next section");
            return true;
        }
        catch
        {
            Debug.Log("No more themes available.");
            return false;
        }
    }


    public void NextSection()
    {
        bool hasNext = GetNextSection();

        int nextIndex = gameData.saveData.selectedSectionIndex + 1;

        if (hasNext)
        {
            currentSection = sectionManager.sections[nextIndex];
            //set selected section index
            gameData.saveData.selectedSectionIndex = nextIndex;
            //save data
            gameData.SaveToFile();
            //load next section
            PanelManager.CloseAll();
            PanelManager.OpenScene("ExGame");
        }
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
