using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Runtime.CompilerServices;

public class GameLogic : MonoBehaviour
{
    public enum GameState
    {
        Play,        
        Finish,
        Pause
    }

    public enum InterractState
    {
        Start,
        End        
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
    private Player playerClass;
    public bool isInteraction = false;

    [Header("Diallog")]
    public GameObject dialogPanelTop;
    private ADV_DialogPanel dialogPanelClass;
    
    public GameObject dialogPanelBottom;

    [Header("App State")]
    public GameState gameState;

    [Header("Interract State")]
    public InterractState interractState;
    public Button interactButton;
    private ButtonImage interactBtn;

    [Header("Score")]
    public int tempScore = 0;

    //timers
    private float sessionStartTime;
    public float sessionDuration;

    private Languages currentLang;


    private void Awake()
    {
        interactButton.onClick.AddListener(OnInteract);
        interactBtn = interactButton.GetComponent<ButtonImage>();
    }

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

        playerClass = player.GetComponent<Player>();

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

        interractState = InterractState.End;

        //get dialog panel class
        dialogPanelClass = dialogPanelTop.GetComponent<ADV_DialogPanel>();
    }


    public void StartInteraction(string[] tileParam)
    {
        //isInteraction = true;
        
        interractState = InterractState.Start;
        
        string text = $"You interract with {tileParam[0]}, its {tileParam[1]}";
        
        //open panel with diallog
        dialogPanelClass.OpenPanel(text);

        interactBtn.SetSelected(true);
    }

    public void EndInteraction()
    {
        if(isInteraction)
            isInteraction = false;               

        dialogPanelClass.OnDiallogClose();

        interractState = InterractState.End;
        interactBtn.SetSelected(false);

        playerClass.InteractIconRoutine(false);     
    }


    private void OnInteract()
    {
        //toggle
        isInteraction = !isInteraction;

        if (isInteraction)
            playerClass.Interact();
        else
            EndInteraction();
    }

    private void OnApplicationQuit()
    {
        gameData.saveData.playerPosition = player.transform.position;
        gameData.SaveToFile();
    }

    private void OnDestroy()
    {
        //remove listeners
        interactButton.onClick.RemoveListener(OnInteract);
    }

}
