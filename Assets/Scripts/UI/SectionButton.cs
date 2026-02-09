using TMPro;
using UnityEngine;
using UnityEngine.Diagnostics;
using UnityEngine.UI;

public class SectionButton : MonoBehaviour
{
    private ExDataLoader dataLoader;
    private GameData gameData;
    private DBUtils dbUtils;

    [Header("UI")]
    public TMP_Text sectionText;
    public Image sectionIcon;
    public TMP_Text sectionDifficulty;
    public Button button;
    public Slider progressSlider;

    [Header("Data")]
    public Section sectionName;

    private void Awake()
    {
        if (button == null)
            button = GetComponent<Button>();

        button.onClick.AddListener(OnClicked);
    }

    private void OnClicked()
    {
        //get data loader and game data
        dataLoader = GameObject.FindWithTag("ExDataLoader").GetComponent<ExDataLoader>();
        dbUtils = GameObject.FindWithTag("DBUtils").GetComponent<DBUtils>();

        //set current section
        if (dataLoader != null)
        {
            dataLoader.sectionClass = sectionName;

            if(sectionName.sectionType == Section.SectionType.LearnType01)
                dbUtils.SetSectionComplete(sectionName.name, true);

            Debug.Log("Section set to: " + sectionName.ToString());
        }

        //get game data
        if (gameData == null)
            gameData = GameObject.FindWithTag("GameData").GetComponent<GameData>();

        gameData.saveData.sectionToLoad = sectionName;

        //load game
        PanelManager.OpenScene("ExGame");
    }

    private void OnDestroy()
    {
        button.onClick.RemoveListener(OnClicked);
    }

}
