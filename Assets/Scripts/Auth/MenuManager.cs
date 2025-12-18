using LTLRN.UI;
using System;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;


public class MenuManager : MonoBehaviour
{
    private GameData gameData;

    private bool initialized = false;
    private bool eventsInitialized = false;
    
    private static MenuManager singleton = null;

    public LeaderboardManager leaderboardManager;

    private string pendingUsername;

    public static MenuManager Singleton
    {
        get
        {
            if (singleton == null)
            {
                singleton = FindFirstObjectByType<MenuManager>();
                singleton.Initialize();
            }
            return singleton; 
        }
    }

    private void Initialize()
    {
        if (initialized) { return; }
        initialized = true;
    }
    
    private void OnDestroy()
    {
        if (singleton == this)
        {
            singleton = null;
        }
    }

    private void Awake()
    {
        Application.runInBackground = true;

        StartClientService();
    }

    public async void StartClientService()
    {
        PanelManager.CloseAll();
     
        try
        {
            if (UnityServices.State != ServicesInitializationState.Initialized)
            {
                var options = new InitializationOptions();
                options.SetProfile("default_profile");
                await UnityServices.InitializeAsync();
            }

            if (!eventsInitialized)
            {
                SetupEvents();
            }

            if (AuthenticationService.Instance.SessionTokenExists)
            {
                gameData = GameObject.FindWithTag("GameData").GetComponent<GameData>();

                string name = string.Empty;
                string pass = string.Empty;

                if (gameData != null)
                {
                    gameData.LoadFromFile();
                    name = gameData.saveData.playerName;
                    pass = gameData.saveData.playerPass;                    
                }

                //try to conect with saved name and pass
                if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(pass))
                {
                    SignInWithUsernameAndPasswordAsync(name, pass);
                }
                else
                {
                    SignInAnonymouslyAsync();
                }                    
            }
            else
            {
                PanelManager.Open("auth");
            }

            // Initialize LeaderboardManager
            if (leaderboardManager != null)
            {
                leaderboardManager.isInitialized = true;
                leaderboardManager.InitializeBoard();
            }
            else
            {
                Debug.LogWarning("LeaderboardManager reference is missing in MenuManager.");
            }                
            
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
            string uiMessage = "Failed to sign you up.\n Error: " + FirstSentence(exception.Message);
            ShowError(ErrorMenu.Action.StartService, uiMessage, "Retry");
        }
    }

    public async void SignInAnonymouslyAsync()
    {
        //PanelManager.Open("loading");

        try
        {
            // Force sign out first to ensure a new anonymous session
            if (AuthenticationService.Instance.IsSignedIn)
            {
                AuthenticationService.Instance.SignOut();
            }

            await AuthenticationService.Instance.SignInAnonymouslyAsync();

            // Check and set default PlayerName if empty
            string timestamp = DateTime.Now.ToString("HHmmss");
            string playerName = "GuestPlayer" + timestamp;
            await AuthenticationService.Instance.UpdatePlayerNameAsync(playerName);

            SaveNamePass(playerName, "");

            if (leaderboardManager != null)
                await leaderboardManager.LoadPlayers(1);

            // Open login panel **after username is set**
            PanelManager.CloseAll();
            var panel = PanelManager.GetSingleton("login") as SignOutMenu;
            if (panel != null)
            {
                panel.Open();
                panel.UpdatePlayerNameUI(playerName);
            }

        }
        catch (AuthenticationException exception)
        {
            Debug.LogException(exception);
            ShowError(ErrorMenu.Action.OpenAuthMenu, "Failed to sign in." + exception, "OK");
        }
        catch (RequestFailedException exception)
        {
            Debug.LogException(exception);
            ShowError(ErrorMenu.Action.SignIn, "Failed to connect to the network."+ exception, "Retry");
        }
    }

    public async void SignInWithUsernameAndPasswordAsync(string username, string password)
    {
        //PanelManager.Open("loading");

        try
        {
            pendingUsername = username;

            await AuthenticationService.Instance.SignInWithUsernamePasswordAsync(username, password);

            //name
            await AuthenticationService.Instance.UpdatePlayerNameAsync(username);

            //save
            SaveNamePass(username, password);

            if (leaderboardManager != null)
                await leaderboardManager.LoadPlayers(1);
        }
        catch (AuthenticationException exception)
        {
            Debug.LogException(exception);
            string uiMessage = "Username or password is wrong. Try again. \n Error: " + FirstSentence(exception.Message);
            ShowError(ErrorMenu.Action.OpenAuthMenu, uiMessage, "OK");
        }
        catch (RequestFailedException exception)
        {
            Debug.LogException(exception);
            string uiMessage = "Failed to login with current name and password. Try again. \n Error: " + FirstSentence(exception.Message);
            ShowError(ErrorMenu.Action.OpenAuthMenu, uiMessage, "OK");
        }
    }
    
    public async void SignUpWithUsernameAndPasswordAsync(string username, string password)
    {
        //PanelManager.Open("loading");
        try
        {
            pendingUsername = username;

            await AuthenticationService.Instance.SignUpWithUsernamePasswordAsync(username, password);
            
            //name
            await AuthenticationService.Instance.UpdatePlayerNameAsync(username);

            //save
            SaveNamePass(username, password);

            if (leaderboardManager != null)
                await leaderboardManager.LoadPlayers(1);
        }
        catch (AuthenticationException exception)
        {
            Debug.LogException(exception);
            string uiMessage = "Failed to sign you up. Try again. \n Error: " + FirstSentence(exception.Message);
            ShowError(ErrorMenu.Action.OpenAuthMenu, uiMessage, "OK");
        }
        catch (RequestFailedException exception)
        {
            Debug.LogException(exception);
            string uiMessage = "Failed to sign you up. Try again. \n Error: " + FirstSentence(exception.Message);
            ShowError(ErrorMenu.Action.OpenAuthMenu, uiMessage, "OK");
        }
    }
    
    public void SignOut()
    {
        AuthenticationService.Instance.SignOut();
        PanelManager.CloseAll();

        //open panel
        PanelManager.Open("auth");
    }
    
    private void SetupEvents()
    {
        eventsInitialized = true;

        AuthenticationService.Instance.SignedIn += () =>
        {           
            PanelManager.CloseAll();

            Panel panelBase = PanelManager.GetSingleton("login");
            //PanelManager.Open("login");

            if (panelBase != null)
            {
                panelBase.Open(); // or call a method like RefreshPlayerName() if you added it to Panel
                var loginPanel = panelBase.GetComponent<SignOutMenu>();
                if (loginPanel != null)
                {
                    loginPanel.UpdatePlayerNameUI(pendingUsername);
                }
            }

            //panel?.RefreshPlayerName();

            //SignInConfirmAsync();
        };

        AuthenticationService.Instance.SignedOut += () =>
        {
            PanelManager.CloseAll();

            //open panel
            PanelManager.Open("auth");
        };
        
        AuthenticationService.Instance.Expired += () =>
        {
            SignInAnonymouslyAsync();
        };
    }
    
    private void ShowError(ErrorMenu.Action action = ErrorMenu.Action.None, string error = "", string button = "")
    {
        //PanelManager.Close("loading");
        ErrorMenu panel = (ErrorMenu)PanelManager.GetSingleton("error");
        panel.Open(action, error, button);
    }
    
    public async void SignInConfirmAsync(string username)
    {
        /*        try
                {
                    if (string.IsNullOrEmpty(AuthenticationService.Instance.PlayerName))
                    {
                        await AuthenticationService.Instance.UpdatePlayerNameAsync("Player");
                    }

                    PanelManager.CloseAll();
                    PanelManager.Open("login");
                }*/
        try
        {
            await AuthenticationService.Instance.UpdatePlayerNameAsync(username);

            PanelManager.CloseAll();
            PanelManager.Open("login");
        }
        catch (AuthenticationException exception)
        {
            string uiMessage = "Problem with sign in.\n Error: " + FirstSentence(exception.Message);
            ShowError(ErrorMenu.Action.OpenAuthMenu, uiMessage, "OK");
        }
    }


    private string FirstSentence(string message)
    {
        if (string.IsNullOrEmpty(message))
            return string.Empty;

        int index = message.IndexOf('.');
        return index > 0
            ? message.Substring(0, index + 1)
            : message;
    }


    private void SaveNamePass(string playerName, string playerPass)
    {        
        if (gameData != null)
        {
            gameData.saveData.playerName = playerName;
            
            if(playerPass.Length > 0)
                gameData.saveData.playerPass = playerPass;
            
            gameData.SaveToFile();
        }
    }
}