using UnityEngine;

public class GameBootstrap : MonoBehaviour
{
    [SerializeField] private GameObject soundManagerPrefab;
    [SerializeField] private GameObject gameDataPrefab;
    [SerializeField] private GameObject logPrefab;

    private void Awake()
    {
        InitializeSoundManager();
        InitializeGameData();
        InitializeLogManager();
    }

    private void InitializeSoundManager()
    {
        if (SoundManager.soundManager != null)
            return;

        if (soundManagerPrefab == null)
        {
            Debug.LogError("SoundManager prefab not assigned.");
            return;
        }

        Instantiate(soundManagerPrefab);
    }

    private void InitializeGameData()
    {
        if (GameData.gameData != null)
            return;

        if (gameDataPrefab == null)
        {
            Debug.LogError("GameData prefab not assigned.");
            return;
        }

        var instance = Instantiate(gameDataPrefab);

        var data = instance.GetComponent<GameData>();
        if (data != null)
        {
            data.LoadFromFile();
        }
        else
        {
            Debug.LogError("GameData component missing on prefab.");
        }
    }

    private void InitializeLogManager()
    {
        if (LogManager.log != null)
            return;

        if (logPrefab == null)
        {
            Debug.LogError("LogManager prefab not assigned.");
            return;
        }

        Instantiate(logPrefab);
    }
}