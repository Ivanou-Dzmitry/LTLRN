using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    private ExGameLogic exGameLogic;
    private GameData gameData;
    [SerializeField] public int tmpScore = 0;


    public void SaveCrystals(int score)
    {
        if(gameData == null)
            gameData = GameObject.FindWithTag("GameData").GetComponent<GameData>();

        gameData.saveData.crystals += score;
        gameData.SaveToFile();
    }

    public void AddStar(int star)
    {
        if (gameData == null)
            gameData = GameObject.FindWithTag("GameData").GetComponent<GameData>();

        gameData.saveData.stars += star;
        gameData.SaveToFile();
    }

}
