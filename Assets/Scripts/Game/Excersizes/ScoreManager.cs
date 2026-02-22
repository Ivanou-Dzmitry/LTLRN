using UnityEngine;
using System.Threading.Tasks;

public class ScoreManager : MonoBehaviour
{
    private ExGameLogic exGameLogic;
    private GameData gameData;
    private LeaderboardManager leaderboardManager;
    [SerializeField] public int tmpScore = 0;
    public bool newRecord = false;
    public int matchScore;


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
