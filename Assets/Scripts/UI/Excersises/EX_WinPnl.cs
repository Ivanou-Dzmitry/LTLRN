using UnityEngine;
using LTLRN.UI;
using UnityEngine.UI;
using TMPro;

//win panel Excersises

public class ExWinPnl : Panel
{
    private ExGameLogic exGameLogic;

    [Header("Buttons")]
    public Button exitButton;
    public Button replayButton;
    public Button nextButton;
    public ButtonImage nextBtn;

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

        if(exGameLogic != null)
        {
            score.text = exGameLogic.tempScore.ToString();
            time.text = exGameLogic.FormatTime(exGameLogic.sessionDuration).ToString();
        }

        bool next = exGameLogic.NextSection();

        Debug.Log(next);
        if (next)
            nextBtn.SetDisabled(false);
        
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
        bool next = exGameLogic.NextSection();

        if (next)
        {
            nextButton.interactable = true;
            //reload scene
            //PanelManager.OpenScene("ExGame");
        }
        else
        {
            nextButton.interactable = false;
            //go to main menu
            //PanelManager.OpenScene("MainMenu");
        }
    }

    private void OnDestroy()
    {
        exitButton.onClick.RemoveListener(OnExitClick);
        replayButton.onClick.RemoveListener(OnReplayClick);
        nextButton.onClick.RemoveListener(OnNextClick);
    }
}
