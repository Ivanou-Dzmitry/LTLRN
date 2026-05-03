using LTLRN.UI;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainPanel : Panel
{
    [SerializeField] private Button mode1Button;
    [SerializeField] private Button mode2Button;
    [SerializeField] private Button openLBoardBtn;

    [SerializeField] private TMP_Text errorTextAsset;

    public override void Initialize()
    {
        if (IsInitialized)
            return;

        mode1Button.onClick.AddListener(AdventureMode);
        mode2Button.onClick.AddListener(ExercisesMode);

        openLBoardBtn.onClick.AddListener(OpenLeaderboard);

        base.Initialize();
    }

    private void AdventureMode()
    {
        PanelManager.OpenScene("GameBoard");
        PanelManager.Open("waiting");
    }

    private void ExercisesMode()
    {
        PanelManager.OpenScene("ExMenu");
        PanelManager.Open("waiting");
    }

    private void Start()
    {     
        SetPanelHeight();
    }

    private void OnDestroy()
    {
        //remove listeners
        openLBoardBtn.onClick.RemoveListener(OpenLeaderboard);
    }

    private void OpenLeaderboard()
    {
        LeaderboardManager.Instance.ShowLeaderboardUI(errorTextAsset);
    }
}
