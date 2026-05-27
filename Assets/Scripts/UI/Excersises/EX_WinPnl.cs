using LTLRN.UI;
using TMPro;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;

//win panel Excersises

public class ExWinPnl : Panel
{
    private ExGameLogic exGameLogic;

    [Header("Buttons")]
    public Button exitButton;
    public Button replayButton;
    public Button nextButton;
    public ButtonImage nextBtn;

    [Header("Slider")]
    public Slider resultSlider;
    [SerializeField] private TMP_Text resultSliderText;

    [Header("Result")]
    [SerializeField] private TMP_Text score;
    [SerializeField] private TMP_Text time;

    public override void Initialize()
    {
        if (IsInitialized)
        {
            return;
        }

        //buttons
        exitButton.onClick.AddListener(OnExitClick);
        replayButton.onClick.AddListener(OnReplayClick);
        nextButton.onClick.AddListener(OnNextClick);

        nextBtn = nextButton.GetComponent<ButtonImage>();

        base.Initialize();

        SetPanelHeight();
    }


    public override void Open()
    {        
        base.Open();

        LoadResultsData();
    }

    private void LoadResultsData()
    {
        exGameLogic = GameObject.FindWithTag("ExGameLogic").GetComponent<ExGameLogic>();

        if (exGameLogic == null)
        {
            Debug.LogError("ExGameLogic not found in scene.");
            return;
        }

        //get question count
        int scoreValue = exGameLogic.tempScore;
        string fromTxt = LocalizationSettings.StringDatabase.GetLocalizedString("KELIAS_UI", "FromSTxt");
        int qCount = exGameLogic.questionsCount;

        //based on qustion count
        string scoreText = $"{scoreValue} {fromTxt} {qCount}";

        //set slider max based on questions
        resultSlider.maxValue = qCount;        

        //text
        float percentTopic = 0;
        percentTopic = (float)scoreValue / qCount * 100f;
        
        //set text with %
        resultSliderText.text = $"{percentTopic:0}%";

        //animate slider to score value
        resultSlider.GetComponent<EX_SliderAnimator>().AnimateTo(scoreValue, 0.5f);

        //set score and time
        score.text = scoreText;
        time.text = exGameLogic.FormatTime(exGameLogic.sessionDuration).ToString();

        //get type of current section
        bool isBundle = exGameLogic.currentBundelSection != null &&
                        exGameLogic.currentBundelSection.isBundle;

        // final result
        bool hasNext;

        if (isBundle)
        {
            hasNext = exGameLogic.GetNextBundleSection();

            // show next button only if there is next section in bundle, if not -> hide it and try to find next normal section
            nextBtn.gameObject.SetActive(hasNext);

            // fallback to next normal section
            if (!hasNext)
            {
                hasNext = exGameLogic.GetNextSection();
            }
        }
        else
        {
            hasNext = exGameLogic.GetNextSection();
        }

        Debug.Log($"IsBundle: {isBundle}, HasNext: {hasNext}");

        // update button once
        NextSectionButton(hasNext);

        // reset current question and section to avoid issues in next section
        exGameLogic.currentQuestion = null;
    }
    
    private void NextSectionButton(bool isNextEsists)
    {
        if (isNextEsists)
        {
            nextBtn.SetDisabled(false);
            nextBtn.PlayAnimation(true, ButtonImage.ButtonAnimation.Scale.ToString());
        }
        else
        {
            nextBtn.SetDisabled(true);
            nextBtn.PlayAnimation(false, ButtonImage.ButtonAnimation.Idle.ToString());
        }

        nextBtn.RefreshState();
    }

    private void OnExitClick()
    {
        exGameLogic.ExitGame();
    }

    private void OnReplayClick()
    {
        //reload scene
        PanelManager.OpenScene("ExGame");
    }

    //next section
    private void OnNextClick()
    {
        //get type of current section
        bool isBundle = exGameLogic.currentBundelSection != null &&
                        exGameLogic.currentBundelSection.isBundle;

        if (isBundle)
            exGameLogic.NextBundleSection();
        else
            exGameLogic.NextSection();
    }

    private void OnDestroy()
    {
        exitButton.onClick.RemoveListener(OnExitClick);
        replayButton.onClick.RemoveListener(OnReplayClick);
        nextButton.onClick.RemoveListener(OnNextClick);
    }
}
