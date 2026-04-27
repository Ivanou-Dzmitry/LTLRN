using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;


//button for select theme

public class EX_ThemeBtn : MonoBehaviour
{
    private ExDataLoader dataLoader;
    private GameData gameData;

    [Header("Data")]
    public SectionManager sectionManager;

    [Header("Level buttons")]
    public Button[] levelButtons;

    [Header("Button")]
    public Button button;

    [Header("Theme text stuff")]
    public TMP_Text themeName;
    public TMP_Text themeNameLocal;
    public TMP_Text themeDescription;

    [Header("Icons")]
    public Image themeIcon;
    public Image topPnlImg;
    public int themeIndex; //used for save

    [Header("Info")]
    public TMP_Text sectionsCount;
    public TMP_Text tasksCount;
    public TMP_Text questionsCount;

    [Header("Sliders")]
    public Slider themeProgressSlider;

    [Header("Panels")]
    [SerializeField] private GameObject topPanel;
    [SerializeField] private GameObject bottomPanel;    
    [SerializeField] private GameObject infoPanel;

    [Header("Transform")]
    [SerializeField] private RectTransform panelTransform;

    [Header("Palette")]
    [SerializeField] private UIColorPalette palette;

    private bool isExpanded = true;

    [Header("Exapnd button")]
    [SerializeField] private Button buttonExpand;
    [SerializeField] private RectTransform imageExpandTransform;

    private void Awake()
    {
        if (button == null)
            button = GetComponent<Button>();

        //button.onClick.AddListener(TogglePanel);

        //epand button
        buttonExpand.onClick.AddListener(TogglePanel);

        //set colors
        topPnlImg = topPanel.GetComponent<Image>();
    }

    private void Start()
    {
        TogglePanel();
    }

    public void UpdateUI()
    {
        //hide info panel if no sections
        if (sectionsCount.text == "0")
        {
            infoPanel.gameObject.SetActive(false);
        }            
    }

    private void SaveLoadData()
    {
        //get data
        dataLoader = GameObject.FindWithTag("ExDataLoader").GetComponent<ExDataLoader>();
        gameData = GameObject.FindWithTag("GameData").GetComponent<GameData>();

        //check data
        if (gameData == null || dataLoader == null) return;

        //Load data
        if (dataLoader.tempSectionManager != null)
        {
            //set section from theme to loader
            dataLoader.sectionManager = sectionManager;

            //save selected theme index
            gameData.saveData.selectedThemeIndex = themeIndex;
            gameData.SaveToFile();
        }

        //work with panels
        PanelManager.CloseAll();
        PanelManager.Open("exmain");

        //load exercise data
        dataLoader.LoadExerciseData();        
    }

    private void LevelSelector()
    {
        // Implement level selection logic here
    }

    private void OnDestroy()
    {
        button.onClick.RemoveListener(TogglePanel);  
        buttonExpand.onClick.RemoveListener(TogglePanel);
    }

    public void TogglePanel()
    {
        isExpanded = !isExpanded;

        //height change
        float height = isExpanded ? 360f : 176f;

        //panel size change
        Vector2 size = panelTransform.sizeDelta;
        size.y = height;
        panelTransform.sizeDelta = size;

        //iocn flip
        Vector3 scale = imageExpandTransform.localScale;
        scale.y = isExpanded ? -1f : 1f;
        imageExpandTransform.localScale = scale;

        //panels show/hide
        bottomPanel.gameObject.SetActive(isExpanded);
    }

}
