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
    private GameObjectsState objState;

    [Header("Buttons")]
    [SerializeField] private Button resetSectionBtn;
    [SerializeField] private Button resetGameBtn;
    [SerializeField] private Button resetStateBtn;
    [SerializeField] private Button resetInventoryBtn;

    [Header("UI")]
    [SerializeField] private TMP_Dropdown debugDrop;
    [SerializeField] private TMP_Text debugText;

    public override void Initialize()
    {
        if (IsInitialized)
        {
            return;
        }
        
        if(resetGameBtn != null)
            resetGameBtn.onClick.AddListener(OnResetClicked);

        if (resetSectionBtn != null)
            resetSectionBtn.onClick.AddListener(OnResetSectionClick);

        resetStateBtn.onClick.AddListener(OnResetStateClick);
        resetInventoryBtn.onClick.AddListener(OnResetInventoryClick);

        base.Initialize();
    }

    public override void Open()
    {
        gameData = GameObject.FindWithTag("GameData").GetComponent<GameData>();
        dbUtils = GameObject.FindWithTag("DBUtils").GetComponent<DBUtils>();
        objState = GameObject.FindWithTag("GameObjState").GetComponent<GameObjectsState>();

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

    private void OnResetStateClick()
    {
        bool result = objState.ResetInteractionStates();

        debugText.text = $"Object states = {result}";
    }

    private void OnResetInventoryClick()
    {
        bool result = objState.ResetInventory();
        debugText.text = $"Inventory Reset to Default Values = {result}";
    }

    private void OnDestroy()
    {
        resetGameBtn.onClick.RemoveListener(OnResetClicked);
        resetSectionBtn.onClick.RemoveListener(OnResetSectionClick);
        resetStateBtn.onClick.RemoveListener(OnResetStateClick);
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
