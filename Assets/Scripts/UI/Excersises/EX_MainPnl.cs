using LTLRN.UI;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class EX_MainPanel : Panel
{
    private GameData gameData;

    private ButtonImage starsBtnImage;
    private ButtonImage lifeBtnImage;
    private ButtonImage crystalsBtnImage;
    
    [Header("Game data")]
    public Button lifeBtn;
    public Button crystalsBtn;
    public Button starsBtn;

    [Header("Theme")]
    public Button themeButton; //run theme selector
    private ButtonImage themeButtonData;

    [Header("Like filter")]
    [SerializeField] private Button likeFilterButton;
    private ButtonImage likeFBtn;
    private bool filterLikedOnly = false;
    [SerializeField] private Transform contentPanel;

    [Header("Run random")]
    public Button runRandomThemeButton;

    [Header("UI")]
    public Canvas canvasRoot;
    public RectTransform panel_01;
    public RectTransform panel_02;
    public RectTransform panel_03;

    public RectTransform scrollPanel;

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

        likeFilterButton.onClick.AddListener(ToggleLikeFilter);
        likeFBtn = likeFilterButton.GetComponent<ButtonImage>();
    }

    private void Start()
    {
        //get gamedata
        gameData = GameObject.FindWithTag("GameData").GetComponent<GameData>();

        if (gameData == null) return;

        LoadGameData();
        
        SetPanelHeight();
        
        base.Open();        
    }

    private void LoadGameData()
    {        
        //update buttons
        if (gameData != null)
        {
            UpdateButton(starsBtnImage, gameData.saveData.stars);
            UpdateButton(lifeBtnImage, gameData.saveData.life);
            UpdateButton(crystalsBtnImage, gameData.saveData.crystals);
        }

/*
        //upb theme button text       
        if (exDataLoader != null)
        {
            SectionManager currentTheme = exDataLoader.sectionManager;
            Locale locale = GetLocale();

            //update theme button text
            if (currentTheme != null)
            {
                themeButtonData.buttonTextStr = currentTheme.GetThemeName(currentTheme, locale);
                themeButtonData.RefreshState();
            }            
        }*/
    }

    //game resources buttons update
    private void UpdateButton(ButtonImage btn, int value)
    {
        //update button text
        string str = value.ToString();
        btn.buttonTextStr = str;
        btn.SetText(str);
        btn.RefreshState();
    }

    void SetPanelHeight()
    {
        Rect safeArea = Screen.safeArea;

        // Set main panel bottom padding to safe area
        base.SetBottom(safeArea.yMin/2);

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

        // Set scroll panel height
        float scrollPanelHeight = panel02Height - HEADER_HEIGHT;
        scrollPanel.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, scrollPanelHeight);
    }

    private void ToggleLikeFilter()
    {
        // Toggle filter state
        filterLikedOnly = !filterLikedOnly;

        // Update button visual state
        likeFBtn.SetSelected(filterLikedOnly);

        // Update visibility of section panels
        foreach (Transform child in contentPanel)
        {
            // Ensure it's a SectionPanel
            if (!child.CompareTag("SectionPanel"))
                continue;
            
            // Get SectionPanel component
            SectionPanel sectionPanel = child.GetComponent<SectionPanel>();
            if (sectionPanel == null)
                continue;

            // If filter ON - show only liked
            // If filter OFF - show all
            bool shouldBeVisible =
                !filterLikedOnly || sectionPanel.isLiked;

            // Set visibility
            child.gameObject.SetActive(shouldBeVisible);
        }
    }

    private void OnDestroy()
    {
        //remove listeners
        likeFilterButton.onClick.RemoveListener(ToggleLikeFilter);
    }

}
