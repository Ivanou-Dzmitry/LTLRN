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
            time.text = FormatTime(exGameLogic.sessionDuration).ToString();
        }
    }

    private string FormatTime(float seconds)
    {
        int minutes = Mathf.FloorToInt(seconds / 60f);
        int secs = Mathf.FloorToInt(seconds % 60f);
        return $"{minutes:00}:{secs:00}";
    }
}
