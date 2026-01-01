using LTLRN.UI;
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

    public override void Initialize()
    {
        if (IsInitialized)
        {
            return;
        }
        
        resetGameBtn.onClick.AddListener(OnResetClicked);

        base.Initialize();
    }

    private void OnResetClicked()
    {
        gameData = GameObject.FindWithTag("GameData").GetComponent<GameData>();

        if (gameData != null )
            gameData.AddDefaultData();
    }

    private void OnDestroy()
    {
        resetGameBtn.onClick.RemoveListener(OnResetClicked);       
    }

}
