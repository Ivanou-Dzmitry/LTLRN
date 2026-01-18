using LTLRN.UI;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EX_InfoPanel : Panel
{
    private ExGameLogic exGameLogic;

    [Header("UI")]
    public TMP_Text excersiseInfo;
    public Button okButton;
    private ButtonImage okBtn;

    public override void Initialize()
    {
        if (IsInitialized)
        {
            return;
        }

        okButton.onClick.AddListener(OnOKClicked);

        okBtn = okButton.GetComponent<ButtonImage>();

        base.Initialize();
    }

    public override void Open()
    {
        exGameLogic = GameObject.FindWithTag("ExGameLogic").GetComponent<ExGameLogic>();

        if (exGameLogic == null)
            excersiseInfo.text = "Error data loading";
        else
            LoadInfoData();

        base.Open();

        okBtn.PlayAnimation(true, ButtonImage.ButtonAnimation.Scale.ToString());
        okBtn.RefreshState();
    }

    private void LoadInfoData()
    {
        //load rules and etc
        excersiseInfo.text = exGameLogic.sectionInfo;
    }

    private void OnOKClicked()
    {        
        if (exGameLogic != null)
            exGameLogic.gameState = GameState.Play;

        PanelManager.CloseAll();
        PanelManager.Open("exgamemain");
    }

    private void OnDestroy()
    {
        okButton.onClick.RemoveListener(OnOKClicked);
    }
}
