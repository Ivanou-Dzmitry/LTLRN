using LTLRN.UI;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class EX_MainPanel : Panel
{
    private GameData gameData;

    [Header("Game data")]
    public TMP_Text life;
    public TMP_Text crystals;
    public TMP_Text stars;

    public override void Initialize()
    {
        if (IsInitialized)
            return;        

        base.Initialize();             
        base.Open();
    }

    public override void Open()
    {
        gameData = GameObject.FindWithTag("GameData").GetComponent<GameData>();

        LoadGameData();
        base.Open();
    }

    private void LoadGameData()
    {
        //soundBtnImg = soundButton.GetComponent<ButtonImage>().buttonIcon;

        life.text = gameData.saveData.life.ToString();
        crystals.text = gameData.saveData.crystals.ToString();
        stars.text = gameData.saveData.stars.ToString();
    }
}
