
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.SocialPlatforms;
using UnityEngine.UI;
using static QuestionBase;


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
        public string[] answerFirstWord;
        public string[] answerSecondWord;
        public string[] qSoundClipName;        
        public string[] questionImageFile;
        public int imagesCount;
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
    private LanguageSwitcher locManager;

    [Header("App State")]
    public GameState gameState;

    [Header("Data Info")]
    public Slider progressBar;
    public TMP_Text progressText;
    public TMP_Text skillLevelText; //Language proficiency level, or test difficulty level
    private int questionsCount = 0;
    private Coroutine progressRoutine;

    [Header("Data")]
    public Themes themes;
    public SectionManager sectionManager;
    public Section currentSection;
    private Section nextSection;
    public QuestionBase currentQuestion;

    //shuffle questions
    private List<QuestionBase> tempQuestions;

    public string sectionInfo;

    private GameObject questionInstance;

    [Header("Question Stuff")]
    public RectTransform questionArea;
    public GameObject[] questionPrefabs;

    [Header("Buttons")]
    //public Button checkButton;
    public Button nextButton; //important button
    public Button interruptGameButton;
    public Button finishButton;

    [Header("Answer")]
    public int currentAnswerIndex = -1;

    //private ButtonImage checkBtn;
    public ButtonImage nextBtn;

    [Header("Score")]
    public int tempScore = 0;

    [Header("UI")]
    public GameObject panelUI;
    public GameObject panelScroll;

    [Header("Log")]
    public TMP_Text log;

    private int qCounter = 0;

    //timers
    private float sessionStartTime;
    public float sessionDuration;

    //finish stuff
    private const float CHECK_DELAY = 10f;
    private Coroutine resultRoutine;
    private const int QUESTIONS_COUNT = 4;
    private const int LEARN_PANEL_PADDING = 24;

    //question data handler
    private ExQManager01 qData;

    private bool isLearnSection = false;

    private void Awake()
    {
        PanelManager.Open("waiting");

        //buttons
        nextButton.gameObject.SetActive(true);
        finishButton.gameObject.SetActive(false);

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
        locManager = GameObject.FindWithTag("LangSwitcher").GetComponent<LanguageSwitcher>();

        //load theme
        if (gameData != null)
        {
            //load selected theme
            sectionManager = themes.theme[gameData.saveData.selectedThemeIndex];

            //load selected section
            if (sectionManager != null)
            {
                try
                {
                    //temporary section
                    Section tempSection = null;

                    //set temp from selected index
                    tempSection = sectionManager.sections[gameData.saveData.selectedSectionIndex];

                    //avoid bundle
                    if(tempSection.isBundle)
                        tempSection = gameData.saveData.sectionToLoad;

                    currentSection = tempSection;
                }
                catch
                {
                    Debug.LogError("Selected section index is out of range. Resetting to first section.");
                    currentSection = gameData.saveData.sectionToLoad;
                }
            }

            //get section type: question or learn
            if (currentSection.sectionType == Section.SectionType.LearnType01)
                isLearnSection = true;

            //Debug.Log($"{currentSection.name}");

            // Create runtime copy of questions
            if (currentSection.questions.Length > 0)                       
                tempQuestions = new List<QuestionBase>(currentSection.questions);
            else
                tempQuestions = new List<QuestionBase>(gameData.saveData.sectionToLoad.questions);

            //shuffle if not learn section
            if(!isLearnSection)
                ShuffleQuestions(tempQuestions);

            //load first
            if (tempQuestions != null && tempQuestions.Count > 0)
                currentQuestion = tempQuestions[0];       
        }
        else
        {
            Debug.LogError("GameData not found in scene!");            
        }

        //load sound
        if(soundManager != null)
        {
            soundManager.LoadSoundData();
        }
        else
        {
            Debug.LogError("SoundManager not found in scene!");
        }

        //load section IMPORTANT
        if (currentSection != null)
            SectionLoad(currentSection);

        //load question IMPORTANT
        if (currentQuestion != null)
            QuestionDataLoadUI(currentQuestion);

        //get questions count
        questionsCount = currentSection.questions.Length;

        //set progress bar
        progressBar.maxValue = questionsCount;
        progressBar.value = 1;

        //set text
        progressText.text = $"0/{questionsCount}";

        //Debug.Log($"Section type: {currentSection.sectionType}, isLearnSection: {isLearnSection}");

        //set UI depends on type - test or learn
        StartCoroutine(SetupUI());
                   
        //start time
        sessionStartTime = Time.time;

        //start game
        gameState = GameState.Play;

        tempScore = 0;

        //show panel
        PanelManager.CloseAll();
        PanelManager.Open("exgamemain");

        Ex_GamePanel gamePanel= panelUI.gameObject.GetComponent<Ex_GamePanel>();

        //get current language
        Languages currentLang = LanguageSwitcher.GetLanguageFromLocale(locManager.GetLocale());

        //Debug.Log(currentLang.ToString());

        //set UI lang
        gamePanel.SetUIData(currentLang.ToString());
    }


    void ShuffleQuestions(List<QuestionBase> list)
    {
        //Debug.Log("Shuffling questions..." + list.Count);
        for (int i = list.Count - 1; i > 0; i--)
        {
            int randomIndex = UnityEngine.Random.Range(0, i + 1);

            QuestionBase temp = list[i];
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
    public static QuestionData LoadQuestionData(QuestionBase question, string sysLang)
    {
        string questionLang = question.questionLang.ToString();

        //create question data
        QuestionData data = new QuestionData();

        //get auto flag
        bool isAuto = question.isAutomated;        

        //load q text from db
        if (questionLang == QuestionLang.LT.ToString() || questionLang == QuestionLang.IMG.ToString())
            data.questionText = DBUtils.Instance.ResolveReference(question.questionReference);
        else
            data.questionText = DBUtils.Instance.ResolveLangReference(question.questionReference, sysLang); ;

        //table name for automation
        string tableName = string.Empty;
        string columnName = string.Empty;
        int qID = -1;

        //get question params based on question reference
        var qp = DBUtils.Instance.GetQuestionParams(question.questionReference);

        tableName = qp.Value.TableName;
        columnName = qp.Value.ColumnName;
        qID = qp.Value.RecordID;

        //get answer word count
        int wordCount = 0;
        wordCount = question.GetAnswerColumns().Length;
        string[] answerColumns = question.GetAnswerColumns();

        string firstColumnName = string.Empty;
        string secondColumnName = string.Empty;

        //get names
        if (answerColumns.Length > 0)
            firstColumnName = answerColumns[0];

        if (answerColumns.Length > 1)
            secondColumnName = answerColumns[1];

        Debug.Log($"FC: {firstColumnName}, SC: {secondColumnName}, Count: {wordCount}, Lang: {sysLang}, Qlang:{questionLang}");

        /*  FIRST START */

        // get answer count
        int firstAnswerWordsCount = 0;

        //if no answers defined - set default
        if (question.answerReferences != null)
            firstAnswerWordsCount = question.answerReferences.Length;
        
        //set auto default
        if (firstAnswerWordsCount == 0)
            firstAnswerWordsCount = QUESTIONS_COUNT; //default 4

        //set answers
        data.answerFirstWord = new string[firstAnswerWordsCount];

        //load answers
        for (int i = 0; i < question.answerReferences.Length; i++)
        {
            data.answerFirstWord[i] = DBUtils.Instance.ResolveReference(question.answerReferences[i]);
        }

        //load auto answers if null
        if (data.answerFirstWord[0] == null)
        {
            string dynamicColumnName = string.Empty;

            //for columns not equal NomSing
            if (firstColumnName != qp.Value.ColumnName)
                dynamicColumnName = firstColumnName;
            else
                dynamicColumnName = qp.Value.ColumnName;

                data.answerFirstWord = DBUtils.Instance.AutoResolveReference(
                    qp.Value.TableName,
                    dynamicColumnName,
                    qp.Value.RecordID,
                    questionLang,                    
                    sysLang,
                    true
                );

            //set correct answer
            data.correctAnswerNumber = DBUtils.Instance.GetCorrectIndex();
        }
        else
        {
            //set correct answer
            data.correctAnswerNumber = (int)question.correctAnswerNumber;
        }

        /*  FIRST END */


        /*  SECOND START */

        //load second word
        int secondAnswerWordsCount = 0;        

        //optional 2nd word answers
        if (question.answerSecondWord != null) 
            secondAnswerWordsCount = question.answerSecondWord.Length;

        //set auto default second word
        if (secondAnswerWordsCount == 0)
            secondAnswerWordsCount = QUESTIONS_COUNT; //default 4

        data.answerSecondWord = new string[secondAnswerWordsCount];
                      
        //load answers
        for (int i = 0; i < question.answerSecondWord.Length; i++)
        {
            data.answerSecondWord[i] = DBUtils.Instance.ResolveReference(question.answerSecondWord[i]);
        }

        //load auto answers if null and 2 words
        if (wordCount == 2 &&
            (data.answerSecondWord == null ||
                data.answerSecondWord.Length == 0 ||
                    string.IsNullOrWhiteSpace(data.answerSecondWord[0])))
        {
            //get second based on first word
            data.answerSecondWord = DBUtils.Instance.GetSecondWord(tableName, data.answerFirstWord, firstColumnName, secondColumnName);
            //Debug.Log($"Auto loaded second word: {data.answerSecondWord[0]}, {data.answerSecondWord[1]}, {data.answerSecondWord[2]}, {data.answerSecondWord[3]}");
        }

        /*  SECOND END */

        // Load sound clips
        int soundCount = 0;

        if (isAuto)
        {
            string soundFileName = string.Empty;

            //try to get sound
            try
            {                
                soundFileName = DBUtils.Instance.GetSound(tableName, data.questionText, columnName);
            }
            catch(Exception e)
            {
                Debug.LogError("Error getting sound file name: " + e.Message);
            }

            //assign clip
            if (!string.IsNullOrEmpty(soundFileName))
            {
                data.qSoundClipName = new[] { soundFileName }; // Length = 1
            }
            else
            {
                data.qSoundClipName = Array.Empty<string>();   // Length = 0
            }
        }
        else
        {
            //manual data
            if (question.soundReferences != null)
                soundCount = question.soundReferences.Length;

            if (soundCount > 0)
            {
                //clip file name
                data.qSoundClipName = new string[soundCount];

                for (int i = 0; i < question.soundReferences.Length; i++)
                {
                    //get clip file name
                    data.qSoundClipName[i] = DBUtils.Instance.ResolveReference(question.soundReferences[i]);
                }
            }
        }


        //get image count
        int imgCount = 0;

        if (isAuto)
        {
            
            //auto data - get image based on selected question
            string imageFileName = DBUtils.Instance.GetImage(tableName, data.questionText, columnName);

            //Debug.Log($"imageFileName: {imageFileName}");
           
            if (!string.IsNullOrEmpty(imageFileName))
            {
                data.questionImageFile = new[] { imageFileName }; // Length = 1                
            }
            else
            {
                //try get image for complex questions with many pictures
                try
                {
                    data.questionImageFile = new[] { DBUtils.Instance.ResolveReference(question.questionImageFile[0]) };                    
                }
                catch (Exception ex)
                {
                    //Debug.LogException(ex);
                    data.questionImageFile = Array.Empty<string>();   // Length = 0
                }                
            }
        }
        else
        {
            if (question.questionImageFile != null)
                imgCount = question.questionImageFile.Length;

            if (imgCount > 0)
            {
                data.questionImageFile = new string[imgCount];

                for (int i = 0; i < question.questionImageFile.Length; i++)
                {
                    data.questionImageFile[i] = DBUtils.Instance.ResolveReference(question.questionImageFile[i]);
                }
            }
        }

        
        //get count of images - question with images Type2
        if (question.imagesCount > 0)
            data.imagesCount = question.imagesCount;
        else if(question.questionType == QuestionType.Image)
            data.imagesCount = DBUtils.Instance.GetImagesCount(tableName, data.questionText, columnName);

        //cat for sound and img
        data.questionCategory = DBUtils.Instance.ResolveReference(question.questionCategory);

        return data;
    }

    //load section
    private void SectionLoad(Section section)
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
    private void QuestionDataLoadUI(QuestionBase question)
    {
        //get current language
        Languages currentLang = LanguageSwitcher.GetLanguageFromLocale(locManager.GetLocale());

        //step 2 - load prepared data with selected language IMPORTANT
        QuestionData data = LoadQuestionData(question, currentLang.ToString());      
        
        bool isAuto = question.isAutomated;

        //clear previous question
        foreach (Transform child in questionArea)
        {
            Destroy(child.gameObject);
        }

        //step 3 - question type 1 loader
        // Text only question
        if (question.questionType == QuestionType.Text)
        {
            //routine to load UI Prefab    
            QuestionUILoad(question, 0);

            //load data to prefab
            qData = questionInstance.GetComponent<ExQManager01>();

            if (qData != null)
            {
                qData.soundPlayButton.GetComponent<ButtonImage>().SetDisabled(false);

                //set question text
                currentQuestion.ApplyQuestionText(data, qData.qestionText);

                //set answers
                currentQuestion.ApplyAnswers(data, qData);

                //load sound. Avoid load name - but clip is not ready. In db just name.
                qData.qAudioClip = LoadAudioAndSetButton(data.questionCategory, data.qSoundClipName, qData.soundBtn);
            }
        }

        //IMAGE question
        if (question.questionType == QuestionType.Image)
        {
            //routine to load UI    
            QuestionUILoad(question, 1);

            //load data to prefab
            qData = questionInstance.GetComponent<ExQManager01>();

            //Debug.Log($"Loading Type 2 question...IMG: {data.questionImageFile.Length}, CAT:{data.questionCategory}, IMGCOUNT: {question.imagesCount}");

            //get count of images - question with images
            int imagesCount = data.imagesCount;
            QImage01 qImage = qData.imagePrefab.GetComponent<QImage01>();

            //after 10 show 1 image
            if (imagesCount > 10)
            {
                imagesCount = 1;                
                string countText = $"x{data.imagesCount}";
                qImage.ShowImagesCountText(countText);
            }
            else
            {
                qImage.ShowImagesCountText("");
            }


            if (qData != null)
            {
                //load image
                if (data.questionCategory != null && data.questionImageFile.Length > 0)
                {
                    qData.qImagePanel.gameObject.SetActive(true);

                    if (isAuto)
                    {
                        LoadImages(imagesCount, question, data);
                    }
                    else
                    {
                        //load images
                        LoadImages(data.questionImageFile.Length, question, data);

/*                        for (int i = 0; i < data.questionImageFile.Length; i++)
                        {
                            GameObject newImg = Instantiate(qData.imagePrefab, qData.qImagePanel);
                            newImg.name = "QImage_" + i;
                            Image img = newImg.GetComponent<Image>();
                            img.sprite = DBUtils.Instance.LoadSpriteByName(data.questionCategory, data.questionImageFile[i]);
                            img.SetNativeSize();
                            img.color = question.questionImageColor;
                            float randomZ = UnityEngine.Random.Range(-5f, 5f);
                            img.rectTransform.localEulerAngles = new Vector3(0f, 0f, randomZ);
                        }*/
                    }

                    //load sound. Avoid load name - but clip is not ready. In db just name.                    
                    qData.qAudioClip = LoadAudioAndSetButton(data.questionCategory, data.qSoundClipName, qData.soundBtn);
                }

                //set answers
                currentQuestion.ApplyAnswers(data, qData);
            }
        }

        //INPUT question
        if (question.questionType == QuestionType.Input)
        {
            //type3
            QuestionUILoad(question, 2);

            //load data to prefab
            qData = questionInstance.GetComponent<ExQManager01>();

            if (qData != null)
            {
                //load sound. Avoid load name - but clip is not ready. In db just name.
                qData.qAudioClip = LoadAudioAndSetButton(data.questionCategory, data.qSoundClipName, qData.soundBtn);

                //load question text
                currentQuestion.ApplyQuestionText(data, qData.qestionText);

                //build inputs
                if(qData.qestionText.text != null)
                    BuildQuestionWithInputs(qData.qestionText.text, qData.questionContainer);
            }
        }
        //INPUT question END

        // SOUND only question
        if (question.questionType == QuestionType.Sound)
        {
            //routine to load UI Prefab    
            QuestionUILoad(question, 3);

            //load data to prefab
            qData = questionInstance.GetComponent<ExQManager01>();

            if (qData != null)
            {
                ButtonImage soundButton = qData.soundPlayButton.GetComponent<ButtonImage>();
                soundButton.SetDisabled(false);
                soundButton.PlayAnimation(true, ButtonImage.ButtonAnimation.Scale.ToString());
                soundButton.RefreshState();

                //set question text
                try
                {
                    currentQuestion.ApplyQuestionText(data, qData.qestionText);
                }
                catch(Exception e)
                {
                    Debug.LogError("Error setting question text: " + e.Message);
                }

                //set answers
                currentQuestion.ApplyAnswers(data, qData);

                //load sound. Avoid load name - but clip is not ready. In db just name.
                qData.qAudioClip = LoadAudioAndSetButton(data.questionCategory, data.qSoundClipName, qData.soundBtn);
            }
        }

        // Learn type 01
        if (question.questionType == QuestionType.Learn)
        {
            for(int i = 0; i < tempQuestions.Count; i++)
            {
                QuestionUILoad(tempQuestions[i], 4);

                currentQuestion = tempQuestions[i];

                //step 2 - load prepared data with selected language IMPORTANT
                QuestionData learnData = LoadQuestionData(currentQuestion, currentLang.ToString());

                //load data to prefab
                qData = questionInstance.GetComponent<ExQManager01>();

                if (qData != null)
                {
                    ButtonImage soundButton = qData.soundPlayButton.GetComponent<ButtonImage>();
                    soundButton.SetDisabled(false);
                    soundButton.RefreshState();

                    qData.learnImage.sprite = DBUtils.Instance.LoadSpriteByName(learnData.questionCategory, learnData.questionImageFile[0]);

                    //set question text
                    try
                    {
                        currentQuestion.ApplyQuestionText(learnData, qData.qestionText);
                    }
                    catch (Exception e)
                    {
                        Debug.LogError("Error setting question text: " + e.Message);
                    }
                    
                    //load sound. Avoid load name - but clip is not ready. In db just name.
                    qData.qAudioClip = LoadAudioAndSetButton(learnData.questionCategory, learnData.qSoundClipName, qData.soundBtn);
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


    private void QuestionUILoad(QuestionBase question, int index)
    {
        questionInstance = Instantiate(questionPrefabs[index], questionArea);
        questionInstance.name = question.name;

        RectTransform rt = questionInstance.GetComponent<RectTransform>();
        rt.localScale = Vector3.one;
        rt.anchoredPosition = Vector2.zero;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    //check button click
    public void Check()
    {
        gameState = GameState.Pause;

        //load data to prefab
        ExQManager01 qData = questionInstance.GetComponent<ExQManager01>();

        //get index
        int selectedIndex = qData.GetSelectedAnswerIndex();

        //Debug.Log($"selectedIndex={selectedIndex}");
        
        //get correct index
        int correctIndex = (int)currentQuestion.correctAnswerNumber;

        if( correctIndex < 0 )
            correctIndex = DBUtils.Instance.GetCorrectIndex();

        //get question
        QuestionBase question = currentQuestion;

        Debug.Log(currentSection.sectionType.ToString());

        //check selected Text, Sound IMPORTANT
        if (currentSection.sectionType == Section.SectionType.Text || currentSection.sectionType == Section.SectionType.Sound || currentSection.sectionType == Section.SectionType.Image)
        {
            //compare indexes
            if (selectedIndex == correctIndex)
            {
                qData.CheckAnswer(selectedIndex, correctIndex);
                tempScore = tempScore + currentQuestion.rewardAmount;
            }
            else
            {
                qData.CheckAnswer(selectedIndex, correctIndex);
            }

            //next question or finish
            if (qCounter < currentSection.questions.Length - 1)
            {
                nextButton.interactable = true;
                nextBtn.PlayAnimation(true, ButtonImage.ButtonAnimation.Scale.ToString());
                nextBtn.RefreshState();
            }
            else
            {
                gameState = GameState.Finish;
                resultRoutine = StartCoroutine(ResultProcessingDelayed(CHECK_DELAY)); // wait 1.5 seconds
            }
        }
        else if (currentSection.sectionType == Section.SectionType.LearnType01 || currentSection.sectionType == Section.SectionType.Exam)
        {
            Debug.Log("Checking LearnType01 or Exam question...");

            //next question or finish
            if (qCounter < currentSection.questions.Length - 1)
            {
                nextButton.interactable = true;
                nextBtn.PlayAnimation(true, ButtonImage.ButtonAnimation.Scale.ToString());
                nextBtn.RefreshState();
            }
            else
            {
                gameState = GameState.Finish;
                resultRoutine = StartCoroutine(ResultProcessingDelayed(CHECK_DELAY)); // wait 1.5 seconds
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

        //Debug.Log(correctAnswerText);

        //get question
        QuestionBase question = currentQuestion;

        //check selected Type Input
        if (currentSection.sectionType == Section.SectionType.Input)
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
                resultRoutine = StartCoroutine(ResultProcessingDelayed(CHECK_DELAY)); // wait 1.5 seconds
            }
        }
    }

    //wait
    private IEnumerator ResultProcessingDelayed(float delay)
    {
        //open wait panel
        //yield return new WaitForSeconds(delay/2);

        //PanelManager.Open("waiting");
        nextButton.gameObject.SetActive(false);

        //open progress
        finishButton.gameObject.SetActive(true);
        EX_ProgressButton progressButton = finishButton.GetComponent<EX_ProgressButton>();
        
        // Subscribe
        progressButton.OnProgressClicked += ResultProcessingImmediate;

        progressButton.StartProgress(delay);

        //set text        
        progressText.text = $"{questionsCount}/{questionsCount}";

        yield return new WaitForSeconds(delay);
        ResultProcessing();
    }

    private void ResultProcessingImmediate()
    {
        if (resultRoutine != null)
        {
            StopCoroutine(resultRoutine);
            resultRoutine = null;
        }

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

    public void nextBtnClicked()
    {
        //srt game state
        gameState = GameState.Play;

        //increase question
        qCounter++;

        Debug.Log($"qCounter = {qCounter}");

        //load next question
        if (qCounter < currentSection.questions.Length && qCounter < tempQuestions.Count)
        {
            //set progress
            int targetValue = qCounter + 1;
            AnimateProgress(progressBar.value, targetValue, 1f);

            //set text
            progressText.text = $"{qCounter}/{currentSection.questions.Length}";

            //get question
            currentQuestion = tempQuestions[qCounter];

            //currentQuestion = currentSection.questions[qCounter];            

            //question load
            QuestionBase question = currentQuestion;

            QuestionDataLoadUI(question);

            //set button state
            if(!isLearnSection)
                NextButtonRoutine(ButtonImage.ButtonAnimation.Idle.ToString());
            else
                NextButtonRoutine(ButtonImage.ButtonAnimation.Scale.ToString());
        }
    }

    public void SwipeBack()
    {
        //srt game state
        gameState = GameState.Play;

        // decrement and clamp (0-based)
        qCounter = Mathf.Max(qCounter - 1, 0);

        // bounds check
        if (!isLearnSection)
            return;

        if (qCounter >= currentSection.questions.Length)
            return;

        if (qCounter >= tempQuestions.Count)
            return;

        //set progress
        int targetValue = qCounter + 1;

        // clamp
        targetValue = Mathf.Max(targetValue, 1);

        // animate only if value actually changes
        if (progressBar.value != targetValue)
        {
            AnimateProgress(progressBar.value, targetValue, 1f);
        }

        //set text
        progressText.text = $"{qCounter}/{currentSection.questions.Length}";

        //get question
        currentQuestion = tempQuestions[qCounter];            

        //question load
        QuestionBase question = currentQuestion;

        QuestionDataLoadUI(question);

        //set button state
        NextButtonRoutine(ButtonImage.ButtonAnimation.Scale.ToString());
        
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

        PanelManager.OpenScene("ExMenu");                
    }

    public void InterruptedExit()
    {
        //save progress only
        SaveGameProgress();

        PanelManager.CloseAll();

        PanelManager.OpenScene("ExMenu");
    }

    private void SaveGameProgress()
    {
        if (dbUtils == null) return;

        int currentProgress = (int)progressBar.value;

        int savedProgress = 0;

        //avoid empty section and bundles
        if (currentSection != null && !currentSection.isBundle)
            savedProgress = dbUtils.GetSectionProgress(currentSection.name);

        if(currentProgress > savedProgress && currentSection != null)
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

    private AudioClip LoadAudioAndSetButton(string category, string[] soundNames, ButtonImage soundButton, int index = 0)
    {
        // Validate input
        if (soundNames == null || soundNames.Length == 0 || index >= soundNames.Length || string.IsNullOrEmpty(soundNames[index]))
        {
            soundButton?.SetDisabled(true);
            return null;
        }

        // Try to load audio
        AudioClip audio = soundManager.LoadAudioClipByName(category, soundNames[index]);

        // Update button state based on result
        if (audio != null)
        {
            soundButton?.SetDisabled(false);
        }
        else
        {
            soundButton?.SetDisabled(true);
            Debug.LogWarning($"Failed to load audio: {soundNames[index]} from category: {category}");
        }

        return audio;
    }

    private void LoadImages(int imagesCount, QuestionBase question, QuestionData data)
    {        
        for (int i = 0; i < imagesCount; i++)
        {
            GameObject newImg = Instantiate(qData.imagePrefab, qData.qImagePanel);
            newImg.name = "QImage_" + i;
            Image img = newImg.GetComponent<Image>();
            img.sprite = DBUtils.Instance.LoadSpriteByName(data.questionCategory, data.questionImageFile[0]);
            img.SetNativeSize();
            img.color = question.questionImageColor;
            float randomZ = UnityEngine.Random.Range(-5f, 5f);
            img.rectTransform.localEulerAngles = new Vector3(0f, 0f, randomZ);
        }
    }

    private void NextButtonRoutine(string animationName)
    {
        //disable check button for question and enable for learn
        if (nextButton != null & isLearnSection == false)
        {
            Debug.Log("HERE1");
            nextButton.interactable = false;
            nextBtn.PlayAnimation(false, animationName);
            nextBtn.RefreshState();
        }
        else if (nextButton != null & isLearnSection == true)
        {            
            Debug.Log("HERE2");
            nextButton.interactable = true;
            nextBtn.PlayAnimation(true, animationName);
            nextBtn.RefreshState();
        }
    }

    IEnumerator SetupUI()
    {
        yield return LocalizationSettings.InitializationOperation;

        // Get localized string from table - test
        string textForTest = LocalizationSettings.StringDatabase.GetLocalizedString("LTLRN", "TestingLevelTxt");

        // Get localized string from table - learning
        string textForLearn = LocalizationSettings.StringDatabase.GetLocalizedString("LTLRN", "LangLevel");


        Debug.Log($"{textForTest}, {textForLearn}");

        //avoid scroll for question sections
        ScrollRect scRect = panelScroll.GetComponent<ScrollRect>();

        //get height of question container
        ExQManager01 uiPrefab = questionPrefabs[4].GetComponent<ExQManager01>();
        float learnPanelHeight = uiPrefab.questionContainer.rect.height + LEARN_PANEL_PADDING;

        //UI diff in learn mode
        if (!isLearnSection)
        {
            NextButtonRoutine(ButtonImage.ButtonAnimation.Idle.ToString()); //question
            scRect.vertical = false;

            skillLevelText.text = textForTest;
        }
        else
        {
            //hide unnecessary buttons and progress
            nextButton.gameObject.SetActive(false);
            progressBar.gameObject.SetActive(false);
            progressText.gameObject.SetActive(false);
            scRect.vertical = true;

            //set height of question container based on questions count
            float height = currentSection.questions.Length * learnPanelHeight;
            questionArea.sizeDelta = new Vector2(questionArea.sizeDelta.x, height);

            skillLevelText.text = textForLearn;
        }
    }

}
