using TMPro;
using UnityEngine;
using UnityEngine.UI;

//button for select theme

public class EX_ThemeBtn : MonoBehaviour
{
    private ExDataLoader dataLoader;
    private GameData gameData;

    [Header("UI")]
    public Button button;
    public TMP_Text themeName;
    public TMP_Text themeDescription;
    public Image themeIcon;
    //public TMP_Text themeDifficulty;
    //public Image buttonImage;
    public int themeIndex;

    [Header("Info")]
    public TMP_Text sectionsCount;
    public TMP_Text questionsCount;

    [Header("Sliders")]
    public Slider themeDifSlider;

    [Header("Panels")]
    public GameObject topPanel;
    public GameObject bottomPanel;

    [Header("Palette")]
    [SerializeField] private UIColorPalette palette;

    [Header("Data")]
    public SectionManager sectionManager;


    private void Awake()
    {
        if (button == null)
            button = GetComponent<Button>();

        button.onClick.AddListener(OnClicked);

        //set colors
        Image topPnlImg = topPanel.GetComponent<Image>();   
        Image botPnlImg = bottomPanel.GetComponent<Image>();

        topPnlImg.color = palette.PrimaryLight;
        botPnlImg.color = palette.Gray6Ligth;
    }

    private void OnClicked()
    {
        dataLoader = GameObject.FindWithTag("ExDataLoader").GetComponent<ExDataLoader>();
        gameData = GameObject.FindWithTag("GameData").GetComponent<GameData>();

        if (gameData == null || dataLoader == null) return;

        //Load data
        if (dataLoader != null && dataLoader.tempSectionManager != null)
        {
            dataLoader.sectionManager = sectionManager;
            gameData.saveData.selectedThemeIndex = themeIndex;
            gameData.SaveToFile();
        }
        
        //work with panels
        PanelManager.CloseAll();
        PanelManager.Open("exmain");

        dataLoader.LoadData();
    }

    private void OnDestroy()
    {
        button.onClick.RemoveListener(OnClicked);        
    }

}
