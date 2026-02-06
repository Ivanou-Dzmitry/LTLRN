using LTLRN.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Ex_GamePanel : Panel
{
    private GameData gameData;
    private ExGameLogic exGameLogic;

    //buttons
    private ButtonImage starsBtnImage;
    private ButtonImage lifeBtnImage;
    private ButtonImage crystalsBtnImage;

    [Header("Game data")]
    public Button lifeBtn;
    public Button crystalsBtn;
    public Button starsBtn;
    public TMP_Text themeNameSysLang;
    public TMP_Text themeNameTargetLang;
    public TMP_Text sectionDifficulty;
    public TMP_Text sectionDescribeTxt;

    [Header("UI")]
    public Canvas canvasRoot;
    public RectTransform panel_01;
    public RectTransform panel_02;
    public RectTransform panel_03;
    public Button infoButton;
    
    // for ui layout
    private const float PANEL_01_HEIGHT = 270f;
    private const float PANEL_03_HEIGHT = 152f;

    //private const float HEADER_HEIGHT = 152f;

    public override void Initialize()
    {
        // Cache components once
        starsBtnImage = starsBtn.GetComponent<ButtonImage>();
        lifeBtnImage = lifeBtn.GetComponent<ButtonImage>();
        crystalsBtnImage = crystalsBtn.GetComponent<ButtonImage>();

        infoButton.onClick.AddListener(OnInfoClicked);
    }

    private void Start()
    {        
        base.Open();        
        
        SetPanelHeight();

        LoadGameData();
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

    public void SetUIData(string lang)
    {
        if (exGameLogic == null)
            exGameLogic = GameObject.FindWithTag("ExGameLogic").GetComponent<ExGameLogic>();

        if(exGameLogic != null)
        {
            //theme
            if(lang =="EN")
                themeNameSysLang.text = exGameLogic.sectionManager.themeName.en;
            else
                themeNameSysLang.text = exGameLogic.sectionManager.themeName.ru;
            
            //theme on target
            themeNameTargetLang.text = exGameLogic.sectionManager.themeNameTargetLang;

            //difficulty
            sectionDifficulty.text = exGameLogic.currentSection.difficultyType.ToString();            
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
        //base.SetBottom(safeArea.yMin/2);

        // Get canvas scale factor
        float scaleFactor = canvasRoot.scaleFactor;

        // Calculate available height in safe area (accounting for canvas scale)
        float safeAreaHeight = safeArea.height / scaleFactor;
       
        // Calculate panel_02 height
        float panel02Height = safeAreaHeight - PANEL_01_HEIGHT - PANEL_03_HEIGHT;
        panel02Height = Mathf.Max(panel02Height, 0f);

        //Debug.Log($"{safeAreaHeight}/ {panel02Height}");

        // Set heights
        panel_01.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, PANEL_01_HEIGHT);
        panel_02.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, panel02Height);
        panel_03.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, PANEL_03_HEIGHT);
    }

    private void OnInfoClicked()
    {
        if (exGameLogic == null)
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
