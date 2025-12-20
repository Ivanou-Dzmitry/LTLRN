using UnityEngine;

public class GameBootstrap : MonoBehaviour
{
    public GameObject soundManagerPrefab;
    public GameObject gameDataPrefab;
    public GameObject logPrefab;

    private void Awake()
    {
        if (SoundManager.soundManager == null)
        {
            Instantiate(soundManagerPrefab);
        }

        if (GameData.gameData == null)
        {
            Instantiate(gameDataPrefab);
            GameData.gameData.LoadFromFile();
        }

        if (LogManager.log == null)
        {
            Instantiate(logPrefab);
        }
    }
}