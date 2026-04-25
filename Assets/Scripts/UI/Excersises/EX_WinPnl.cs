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
    }


    public override void Open()
    {        
        base.Open();

        LoadResultsData();
    }

    private void LoadResultsData()
    {
        exGameLogic = GameObject.FindWithTag("ExGameLogic").GetComponent<ExGameLogic>();

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

        resultSlider.GetComponent<EX_SliderAnimator>().AnimateTo(scoreValue, 0.5f);

        if (exGameLogic != null)
        {
            score.text = scoreText;
            time.text = exGameLogic.FormatTime(exGameLogic.sessionDuration).ToString();
        }

        //get next section availability
        bool next = exGameLogic.GetNextSection();
        bool nextBundle = exGameLogic.GetNextBundleSection();

        //turn button on/off based on next section availability
        if (next)
            NextSectionButton(next);

        if(nextBundle)
            NextSectionButton(nextBundle);        
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

    private void OnNextClick()
    {
        //standart next section
        bool next = exGameLogic.GetNextSection();

        if (next)
        {
            exGameLogic.NextSection();
        }

        //bundle next section
        bool nextInBundle = exGameLogic.GetNextBundleSection();

        if (nextInBundle)
        {
            exGameLogic.NextBundleSection();
        }
    }

    private void OnDestroy()
    {
        exitButton.onClick.RemoveListener(OnExitClick);
        replayButton.onClick.RemoveListener(OnReplayClick);
        nextButton.onClick.RemoveListener(OnNextClick);
    }
}
