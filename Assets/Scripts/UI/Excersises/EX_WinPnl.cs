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

        base.Initialize();
    }


    public override void Open()
    {
        LoadResultsData();

        base.Open();
    }

    private void LoadResultsData()
    {
        exGameLogic = GameObject.FindWithTag("ExGameLogic").GetComponent<ExGameLogic>();

        if(exGameLogic != null)
        {
            score.text = exGameLogic.tempScore.ToString();
            time.text = exGameLogic.FormatTime(exGameLogic.sessionDuration).ToString();
        }
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
        exGameLogic.NextSection();
    }

    private void OnDestroy()
    {
        exitButton.onClick.RemoveListener(OnExitClick);
        replayButton.onClick.RemoveListener(OnReplayClick);
        nextButton.onClick.RemoveListener(OnNextClick);
    }
}
