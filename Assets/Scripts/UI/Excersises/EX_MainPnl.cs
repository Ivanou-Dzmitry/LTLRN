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
    public Button openLBoardBtn;

    [Header("Theme")]
    //public Button themeButton; //run theme selector
    //private ButtonImage themeButtonData;    

    [Header("Like filter")]
    [SerializeField] private Button likeFilterButton;
    private ButtonImage likeFBtn;

    private bool isProcessing = false;
    private bool filterLikedOnly = false;
    
    [SerializeField] private Transform contentPanel;

    [Header("Run random")]
    public Button runRandomThemeButton;

    [Header("UI")]
    public Canvas canvasRoot;
    public RectTransform panel_01;
    public RectTransform panel_02;
    public RectTransform panel_03;

    [Header("Text prefab")]
    public TMP_Text textPrefab;
    private TMP_Text infoText;

    [SerializeField] private TMP_Text errorTextAsset;

    public RectTransform scrollPanel;

    // for ui layout
    private const float PANEL01_HEIGHT = 440f;
    private const float PANEL03_HEIGHT = 208f;
    private const float SPACING = 0;
    private const float HEADER_HEIGHT = 0f;

    public override void Initialize()
    {
        // Cache components once
        starsBtnImage = starsBtn.GetComponent<ButtonImage>();
        lifeBtnImage = lifeBtn.GetComponent<ButtonImage>();
        crystalsBtnImage = crystalsBtn.GetComponent<ButtonImage>();

        // Remove all existing listeners first
        likeFilterButton.onClick.RemoveAllListeners();

        likeFilterButton.onClick.AddListener(ToggleLikeFilter);
        likeFBtn = likeFilterButton.GetComponent<ButtonImage>();
        //inital color for like filter button
        likeFBtn.buttonIcon.color = palette.Transparent50Panel;

        openLBoardBtn.onClick.AddListener(OpenLeaderboard);
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
        float scaleFactor = canvasRoot.scaleFactor;

        // safe area in screen pixels - convert to canvas units
        float safeAreaHeight = safeArea.height / scaleFactor;

        // also account for bottom offset (home bar on iPhone)
        float bottomOffset = safeArea.y / scaleFactor;
        // and top offset (notch)
        float topOffset = (Screen.height - safeArea.y - safeArea.height) / scaleFactor;

        Debug.Log($"[SafeArea] screen: {Screen.height}px | safeArea: {safeArea} | scaleFactor: {scaleFactor}");
        Debug.Log($"[SafeArea] safeAreaHeight: {safeAreaHeight} | top: {topOffset} | bottom: {bottomOffset}");

        // panel02 fills whatever is left
        float panel02Height = safeAreaHeight - PANEL01_HEIGHT - PANEL03_HEIGHT;
        panel02Height = Mathf.Max(panel02Height, 0f);

        Debug.Log($"[SetPanelHeight] p01: {PANEL01_HEIGHT} | p02: {panel02Height} | p03: {PANEL03_HEIGHT}");

        panel_01.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, PANEL01_HEIGHT);
        panel_02.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, panel02Height);
        panel_03.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, PANEL03_HEIGHT);

        float scrollPanelHeight = panel02Height - HEADER_HEIGHT - SPACING;
        scrollPanel.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, scrollPanelHeight);
    }

    private void ToggleLikeFilter()
    {
        // Prevent multiple calls
        if (isProcessing)
        {
            Debug.Log("Already processing, ignoring");
            return;
        }

        isProcessing = true;

        // Toggle filter state
        filterLikedOnly = !filterLikedOnly;

        if(filterLikedOnly)
            likeFBtn.buttonIcon.color = palette.Error;
        else
            likeFBtn.buttonIcon.color = palette.Transparent50Panel;

        int likedCount = 0;

        // Update visibility of section panels
        foreach (Transform child in contentPanel)
        {
            if (!child.CompareTag("SectionPanel"))
                continue;

            SectionPanel sectionPanel = child.GetComponent<SectionPanel>();
            
            if (sectionPanel == null)
                continue;

            if (sectionPanel.isLiked)
                likedCount++;

            bool shouldBeVisible = !filterLikedOnly || sectionPanel.isLiked;
            
            child.gameObject.SetActive(shouldBeVisible);
        }

        //show text if no likes
        ShowInfoText(likedCount, filterLikedOnly);

        isProcessing = false;
    }

    private void ShowInfoText(int likesCount, bool visible)
    {
        if (infoText != null)
        {
            Destroy(infoText.gameObject);
            infoText = null;
        }

        if (likesCount == 0)
        {
            infoText = Instantiate(textPrefab, contentPanel);
            infoText.name = "NoLikedContentText";
            infoText.gameObject.SetActive(visible);
            
            string info = LocalizationSettings.StringDatabase.GetLocalizedString("KELIAS_UI", "NoLikesTxt");
            infoText.text = info;
            
            //set width to fit panel
            RectTransform rt = infoText.GetComponent<RectTransform>();
            RectTransform contentRect = contentPanel as RectTransform;
            if (contentRect != null)
            {
                rt.sizeDelta = new Vector2(contentRect.rect.width, rt.sizeDelta.y);
            }
        }
    }

    private void OpenLeaderboard()
    {
        LeaderboardManager.Instance.ShowLeaderboardUI(errorTextAsset);
    }

    private void OnDestroy()
    {
        //remove listeners
        likeFilterButton.onClick.RemoveListener(ToggleLikeFilter);

        openLBoardBtn.onClick.RemoveListener(OpenLeaderboard);
    }

}
