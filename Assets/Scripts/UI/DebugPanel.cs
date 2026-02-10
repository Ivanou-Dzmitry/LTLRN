using LTLRN.UI;
using TMPro;
using UnityEngine;
using UnityEngine.Diagnostics;
using UnityEngine.UI;

//debug panel

public class DebugPanel : Panel
{
    private DBUtils dbUtils;
    private GameData gameData;

    [Header("Buttons")]
    [SerializeField] private Button resetSectionBtn;
    [SerializeField] private Button resetGameBtn;

    [Header("UI")]
    [SerializeField] private TMP_Dropdown debugDrop;
    [SerializeField] private TMP_Text debugText;

    public override void Initialize()
    {
        if (IsInitialized)
        {
            return;
        }
        
        resetGameBtn.onClick.AddListener(OnResetClicked);
        resetSectionBtn.onClick.AddListener(OnResetSectionClick);

        base.Initialize();
    }

    public override void Open()
    {
        gameData = GameObject.FindWithTag("GameData").GetComponent<GameData>();
        dbUtils = GameObject.FindWithTag("DBUtils").GetComponent<DBUtils>();

        debugDrop.value = gameData.saveData.debugMode ? 1 : 0;

        base.Open();
    }

    private void OnResetClicked()
    {
        bool result = false;

        if (gameData != null )
            result = gameData.AddDefaultData();

        debugText.text = $"Game Data Reset to Default Values = {result}";
    }

    private void OnResetSectionClick()
    {
        bool result = dbUtils.DropSection();

        debugText.text = $"Sections Reset to Default Values = {result}";
    }

    private void OnDestroy()
    {
        resetGameBtn.onClick.RemoveListener(OnResetClicked);
        resetSectionBtn.onClick.RemoveListener(OnResetSectionClick);
    }


    public void DebugMode()
    {
        if(gameData== null)
            gameData = GameObject.FindWithTag("GameData").GetComponent<GameData>();

        if (gameData != null)
        {
            gameData.saveData.debugMode = debugDrop.value == 0 ? false : true;
            gameData.SaveToFile();

            debugText.text = "Debug: " + debugDrop.value;
        }        
    }

}
