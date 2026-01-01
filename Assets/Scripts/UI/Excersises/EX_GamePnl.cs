using UnityEngine;
using LTLRN.UI;
using UnityEngine.UI;

public class Ex_GamePanel : Panel
{
    private GameData gameData;
    private ExGameLogic exGameLogic;

    private ButtonImage starsBtnImage;
    private ButtonImage lifeBtnImage;
    private ButtonImage crystalsBtnImage;

    [Header("Game data")]
    public Button lifeBtn;
    public Button crystalsBtn;
    public Button starsBtn;

    [Header("UI")]
    public Canvas canvasRoot;
    public RectTransform panel_01;
    public RectTransform panel_02;
    public RectTransform panel_03;
    public Button infoButton;
    
    // for ui layout
    private const float panel01Height = 96f;
    private const float panel03Height = 192f;

    private const float HEADER_HEIGHT = 152f;

    public override void Initialize()
    {
        // Cache components once
        starsBtnImage = starsBtn.GetComponent<ButtonImage>();
        lifeBtnImage = lifeBtn.GetComponent<ButtonImage>();
        crystalsBtnImage = crystalsBtn.GetComponent<ButtonImage>();

        infoButton.onClick.AddListener(OnInfoClicked);
    }

    public override void Open()
    {        
        base.Open();
    }

    private void Start()
    {
        LoadGameData();
        base.Open();
        SetPanelHeight();
    }

    private void LoadGameData()
    {
        gameData = GameObject.FindWithTag("GameData").GetComponent<GameData>();
        
        if (gameData != null)
        {
            UpdateButton(starsBtnImage, gameData.saveData.stars);
            UpdateButton(lifeBtnImage, gameData.saveData.life);
            UpdateButton(crystalsBtnImage, gameData.saveData.crystals);
        }
    }

    private void UpdateButton(ButtonImage btn, int value)
    {
        string str = value.ToString();
        btn.buttonTextStr = str;
        btn.SetText(str);
        btn.RefreshState();
    }

    void SetPanelHeight()
    {
        Rect safeArea = Screen.safeArea;

        // Set main panel bottom padding to safe area
        base.SetBottom(safeArea.yMin / 2);

        // Get canvas scale factor
        float scaleFactor = canvasRoot.scaleFactor;

        // Calculate available height in safe area (accounting for canvas scale)
        float safeAreaHeight = safeArea.height / scaleFactor;

        // Calculate panel_02 height
        float panel02Height = safeAreaHeight - panel01Height - panel03Height;
        panel02Height = Mathf.Max(panel02Height, 0f);

        // Set heights
        panel_01.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, panel01Height);
        panel_02.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, panel02Height);
        panel_03.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, panel03Height);
    }

    private void OnInfoClicked()
    {
        exGameLogic = GameObject.FindWithTag("ExGameLogic").GetComponent<ExGameLogic>();

        //pause game
        if (exGameLogic != null)
            exGameLogic.gameState = GameState.Pause;

        PanelManager.CloseAll();
        PanelManager.Open("info");
    }

    private void OnDestroy()
    {
        infoButton.onClick.RemoveListener(OnInfoClicked);
    }
}
