using LTLRN.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

//debug panel

public class DebugPanel : Panel
{
    //private DBUtils dbUtils;
    private GameData gameData;

    [Header("Buttons")]
    [SerializeField] private Button resetSectionBtn;
    [SerializeField] private Button resetGameBtn;
    [SerializeField] private TMP_Dropdown debugDrop;
    [SerializeField] private TMP_Text debugText;

    public override void Initialize()
    {
        if (IsInitialized)
        {
            return;
        }
        
        resetGameBtn.onClick.AddListener(OnResetClicked);

        base.Initialize();
    }

    public override void Open()
    {
        gameData = GameObject.FindWithTag("GameData").GetComponent<GameData>();

        debugDrop.value = gameData.saveData.debugMode ? 1 : 0;

        base.Open();
    }

    private void OnResetClicked()
    {       
        if (gameData != null )
            gameData.AddDefaultData();

        debugText.text = "Game Data Reset to Default Values.";
    }

    private void OnDestroy()
    {
        resetGameBtn.onClick.RemoveListener(OnResetClicked);       
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
