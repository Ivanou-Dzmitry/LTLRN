using System;
using System.IO;
using UnityEngine;

[Serializable]
public class SaveData
{
    [Header("Game")]
    public int totalScore;

    [Header("Data")]
    public int selectedThemeIndex;
    public int selectedSectionIndex;

    [Header("Player")]
    public string playerName;
    public string playerPass;
    public int playerIconIndex;

    public string lang = "en";

    //bonus stuff
    [Header("Resources")]
    public int stars;
    public int life;
    public int crystals;

    [Header("Volume control")]
    public bool soundToggle;
    public bool musicToggle;
    public float soundVolume;
    public float musicVolume;
    public float soundSpeed;

    [Header("Debug")]
    public bool debugMode = false;
}

public class GameData : MonoBehaviour
{
    public static GameData gameData;

    //public static SoundManager Instance;
    public SaveData saveData;

    const string SETTINGS_FILE_NAME = "ltlrn_settings.json";


    private void Awake()
    {
        if (gameData == null)
        {
            DontDestroyOnLoad(this.gameObject);
            gameData = this;
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //empty
    }

    public void SaveToFile()
    {
        //check game data and save data
        if (gameData == null || gameData.saveData == null)
        {
            Debug.LogError("gameData or gameData.saveData is null. Cannot save to file.");
            return;
        }

        //try save
        try
        {
            string savingData = JsonUtility.ToJson(gameData.saveData, true);
            string filePath = Path.Combine(Application.persistentDataPath, SETTINGS_FILE_NAME);

            File.WriteAllText(filePath, savingData);
            Debug.Log(filePath);
        }
        catch (Exception ex)
        {
            Debug.LogError("Error while saving game data: " + ex.Message);
        }
    }

    public void LoadFromFile()
    {
        //check file
        string filePath = Path.Combine(Application.persistentDataPath, SETTINGS_FILE_NAME);

        if (File.Exists(filePath))
        {
            string loadedData = File.ReadAllText(filePath);
            saveData = JsonUtility.FromJson<SaveData>(loadedData);

            // Fix any size mismatch
            PatchSavedData();
        }
        else
        {
            //default values
            AddDefaultData();
        }
    }

    public void AddDefaultData()
    {
        saveData = new SaveData();

        gameData.saveData.selectedThemeIndex = 0;
        gameData.saveData.selectedSectionIndex = 0;

        //score
        gameData.saveData.totalScore = 0;

        gameData.saveData.playerName = "Player";
        gameData.saveData.playerPass = "";
        gameData.saveData.playerIconIndex = 0;

        //language
        saveData.lang = "en";

        //bonus stuff
        gameData.saveData.stars = 0;
        gameData.saveData.life = 5;
        gameData.saveData.crystals = 0;

        //sound and music settings
        saveData.soundToggle = false;
        saveData.musicToggle = true;
        saveData.soundVolume = 0.5f;
        saveData.musicVolume = 0.1f;
        saveData.soundSpeed = 1.0f;

        saveData.debugMode = false;
    }

    private void PatchSavedData()
    {
        // Ensure life is at least 1
        if (gameData.saveData.life == 0)
            gameData.saveData.life = 5;


        if (saveData.lang == string.Empty)
        {
            saveData.lang = "en";
        }

        SaveToFile();
    }

    private void OnDisable()
    {
        SaveToFile();
    }

    private void OnApplicationQuit()
    {
        SaveToFile();
    }
}
