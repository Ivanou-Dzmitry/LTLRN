using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class SectionButton : MonoBehaviour
{
    private ExDataLoader dataLoader;
    private GameData gameData;

    [Header("UI")]
    public TMP_Text sectionText;
    public Image sectionIcon;
    public Button button;

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

        //set current section
        if (dataLoader != null)
        {
            dataLoader.sectionClass = sectionName;
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
