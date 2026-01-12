using LTLRN.UI;
using UnityEngine;
using UnityEngine.UI;

public class EX_ExitGamePnl : Panel
{
    private ExGameLogic exGameLogic;

    [Header("Buttons")]
    public Button exitButton;
    public Button returnButton;
    private ButtonImage returnBtn;

    public override void Initialize()
    {
        if (IsInitialized)
        {
            return;
        }

        //buttons
        exitButton.onClick.AddListener(OnExitClick);
        returnButton.onClick.AddListener(OnReturnClick);
        returnBtn = returnButton.GetComponent<ButtonImage>();

        base.Initialize();
    }

    public override void Open()
    {
        exGameLogic = GameObject.FindWithTag("ExGameLogic").GetComponent<ExGameLogic>();
        
        base.Open();

        //run animation
        returnBtn.PlayAnimation(true, ButtonImage.ButtonAnimation.Scale.ToString());
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

    private void OnDestroy()
    {
        //remove listeners
        exitButton.onClick.RemoveListener(OnExitClick);
        returnButton.onClick.RemoveListener(OnReturnClick);
    }

}
