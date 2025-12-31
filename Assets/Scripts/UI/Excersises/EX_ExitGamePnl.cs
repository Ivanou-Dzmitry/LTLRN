using LTLRN.UI;
using UnityEngine;
using UnityEngine.UI;

public class EX_ExitGamePnl : Panel
{
    private ExGameLogic exGameLogic;

    [Header("Buttons")]
    public Button exitButton;
    public Button returnButton;

    public override void Initialize()
    {
        if (IsInitialized)
        {
            return;
        }

        //buttons
        exitButton.onClick.AddListener(OnExitClick);
        returnButton.onClick.AddListener(OnReturnClick);

        base.Initialize();
    }

    public override void Open()
    {
        exGameLogic = GameObject.FindWithTag("ExGameLogic").GetComponent<ExGameLogic>();

        base.Open();
    }

    private void OnExitClick()
    {
        exGameLogic.InterruptedExit();
    }

    private void OnReturnClick()
    {
        PanelManager.CloseAll();
        PanelManager.Open("exgamemain");
    }

}
