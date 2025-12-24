using TMPro;
using UnityEngine;
using UnityEngine.UI;

//button for select theme

public class EX_ThemeBtn : MonoBehaviour
{
    private ExDataLoader dataLoader;
    private GameData gameData;

    private bool isSelected = false;

    [Header("UI")]
    public Button button;
    public TMP_Text themeName;
    public TMP_Text themeDescription;
    public Image buttonImage;
    public int themeIndex;

    [Header("Palette")]
    [SerializeField] private UIColorPalette palette;

    [Header("Data")]
    public SectionManager sectionManager;


    private void Awake()
    {
        if (button == null)
            button = GetComponent<Button>();

        button.onClick.AddListener(OnClicked);

        //sety color
        Image buttonImg = button.GetComponent<Image>();
        buttonImg.color = palette.PrimaryLight;

        //set default colors
        //SetSelected(false);
    }

    private void OnClicked()
    {
        dataLoader = GameObject.FindWithTag("ExDataLoader").GetComponent<ExDataLoader>();

        gameData = GameObject.FindWithTag("GameData").GetComponent<GameData>();

        if (dataLoader != null && dataLoader.tempSectionManager != null)
        {
            dataLoader.sectionManager = sectionManager;
            gameData.saveData.selectedThemeIndex = themeIndex;
            gameData.SaveToFile();
        }

        PanelManager.Close("themes");
        PanelManager.Open("exmain");

        dataLoader.LoadData();

        isSelected = true;

        //SetSelected(true);
    }

/*    public void SetSelected(bool selected)
    {
        if (selected)
        {
            buttonImage.color = palette.Primary;
            themeName.color = palette.TextPrimary;
            themeDescription.color = palette.TextPrimary;
        }
        else
        {
            buttonImage.color = palette.PrimaryLight;
            themeName.color = palette.TextSecondary;
            themeDescription.color = palette.TextSecondary;
        }
    }*/

}
