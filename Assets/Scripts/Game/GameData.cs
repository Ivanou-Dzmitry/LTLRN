using System;
using System.IO;
using UnityEditor.Overlays;
using UnityEngine;

[Serializable]
public class SaveData
{
    public int levelToLoad;
}

public class GameData : MonoBehaviour
{
    public static GameData gameData;
    public SaveData saveData;

    const string SETTINGS_FILE_NAME = "ltlrn_settings.json";

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    public void SaveToFile()
    {
        //check game data and save data
        if (gameData == null || gameData.saveData == null)
        {
            Debug.LogError("gameData or gameData.saveData is null. Cannot save to file.");
            return;
        }

        try
        {
            string savingData = JsonUtility.ToJson(gameData.saveData, true);
            string filePath = Path.Combine(Application.persistentDataPath, SETTINGS_FILE_NAME);

            File.WriteAllText(filePath, savingData);
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
    }

    private void PatchSavedData()
    {
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
