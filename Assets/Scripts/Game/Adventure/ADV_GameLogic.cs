using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameLogic : MonoBehaviour
{
    public enum GameState
    {
        Play,
        Finish,
        Pause
    }

    private GameData gameData;
    private DBUtils dbUtils;
    private SoundManager soundManager;
    private ScoreManager scoreManager;
    private LanguageSwitcher locManager;

    [Header("Camera")]
    public Camera mainCamera;

    [Header("Player")]
    public GameObject player;
    public bool isInteraction = false;

    [Header("Diallog")]
    public GameObject dialogPanelTop;
    public GameObject dialogPanelBottom;

    [Header("App State")]
    public GameState gameState;

    [Header("Score")]
    public int tempScore = 0;

    //timers
    private float sessionStartTime;
    public float sessionDuration;

    private Languages currentLang;

    private void Start()
    {
        StartCoroutine(WaitAndLoadData());
    }

    private IEnumerator WaitAndLoadData()
    {
        // Find DBUtils - Updated
        dbUtils = FindFirstObjectByType<DBUtils>();

        if (dbUtils == null)
        {
            Debug.LogError("DBUtils not found in scene!");
            yield break;
        }

        // Wait for database to be ready
        while (!dbUtils.IsReady)
        {
            yield return null;
        }

        // IMPORTANT
        LoadGameData();
    }

    private void LoadGameData()
    {
        //get classes
        gameData = GameObject.FindWithTag("GameData").GetComponent<GameData>();
        soundManager = GameObject.FindWithTag("SoundManager").GetComponent<SoundManager>();
        //scoreManager = GameObject.FindWithTag("ScoreManager").GetComponent<ScoreManager>();
        locManager = GameObject.FindWithTag("LangSwitcher").GetComponent<LanguageSwitcher>();

        //load theme
        if (gameData != null)
        {
            player.transform.position = gameData.saveData.playerPosition;

            //Debug.Log("Load data!");
        }
        else
        {
            Debug.LogError("GameData not found in scene!");
        }

        //load sound
        if (soundManager != null)
        {
            soundManager.LoadSoundData();
        }
        else
        {
            Debug.LogError("SoundManager not found in scene!");
        }

        //start time
        sessionStartTime = Time.time;

        //start game
        gameState = GameState.Play;

        tempScore = 0;

        //get current language
        currentLang = LanguageSwitcher.GetLanguageFromLocale(locManager.GetLocale());
    }


    public void StartInteraction(string param01, string param02)
    {
        isInteraction = true;
        dialogPanelTop.gameObject.SetActive(true);
        Debug.Log($"Start interaction with {param01}, {param02}");
    }


    private void OnApplicationQuit()
    {
        gameData.saveData.playerPosition = player.transform.position;
        gameData.SaveToFile();
    }

}
