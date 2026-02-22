using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;

public class GameServiceManager : MonoBehaviour
{
    private static GameServiceManager _instance;
    public static GameServiceManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<GameServiceManager>();
                if (_instance == null)
                {
                    GameObject go = new GameObject("GameServiceManager");
                    _instance = go.AddComponent<GameServiceManager>();
                }
            }
            return _instance;
        }
    }

    [Header("Service State")]
    public bool isAuthenticated = false;
    public string playerName = "";
    public string playerId = "";

    [Header("References")]
    public GameData gameData;

    private void Awake()
    {
        // Singleton pattern - keep this object alive across scenes
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        gameData = GameObject.FindWithTag("GameData").GetComponent<GameData>();
    }

    public async Task<bool> InitializeServicesAsync()
    {
        try
        {
            // Initialize Unity Services if not already
            if (UnityServices.State != ServicesInitializationState.Initialized)
            {
                var options = new InitializationOptions();
                options.SetProfile("default_profile");
                await UnityServices.InitializeAsync(options);
                Debug.Log("Unity Services initialized successfully");
            }

            // Check if already signed in
            if (AuthenticationService.Instance.IsSignedIn)
            {
                UpdatePlayerInfo();
                isAuthenticated = true;
                Debug.Log($"Already signed in as {playerName}");
                return true;
            }

            Debug.Log("Not signed in yet");
            return false;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Failed to initialize services: {ex.Message}");
            isAuthenticated = false;
            return false;
        }
    }

    public async Task<bool> SignInAnonymouslyAsync()
    {
        try
        {
            // Force sign out first to ensure a new anonymous session
            if (AuthenticationService.Instance.IsSignedIn)
            {
                AuthenticationService.Instance.SignOut();
            }

            await AuthenticationService.Instance.SignInAnonymouslyAsync();

            // Set default PlayerName
            string timestamp = System.DateTime.Now.ToString("HHmmss");
            string desiredName = "Guest" + timestamp;
            await AuthenticationService.Instance.UpdatePlayerNameAsync(desiredName);

            // Unity adds a discriminator (#3004), so we need to strip it
            string fullName = AuthenticationService.Instance.PlayerName;

            // Remove discriminator (everything after #)
            if (fullName.Contains("#"))
            {
                playerName = fullName.Split('#')[0];
            }
            else
            {
                playerName = fullName;
            }

            Debug.Log($"Player name set to: {playerName} (full: {fullName})");

            UpdatePlayerInfo();
            isAuthenticated = true;            

            // Save to GameData
            if (gameData != null)
            {
                gameData.LoadFromFile();
                SaveNamePass(playerName, "");
            }

            return true;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Sign in failed: {ex.Message}");
            isAuthenticated = false;
            return false;
        }
    }

    private void UpdatePlayerInfo()
    {
        if (AuthenticationService.Instance.IsSignedIn)
        {
            playerId = AuthenticationService.Instance.PlayerId;
            playerName = AuthenticationService.Instance.PlayerName;
        }
    }

    public string GetPlayerId()
    {
        return AuthenticationService.Instance.IsSignedIn
            ? AuthenticationService.Instance.PlayerId
            : playerId;
    }

    public string GetPlayerName()
    {
        return AuthenticationService.Instance.IsSignedIn
            ? AuthenticationService.Instance.PlayerName
            : playerName;
    }

    private void SaveNamePass(string name, string pass)
    {
        if (gameData != null)
        {
            gameData.saveData.playerName = name;
            gameData.saveData.playerPass = pass;
            gameData.SaveToFile();
        }
    }
}
