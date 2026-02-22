using LTLRN.UI;
using System;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;
using UnityEngine.Localization.Settings;
using System.Threading.Tasks;


#if UNITY_ANDROID
using GooglePlayGames;
using GooglePlayGames.BasicApi;
#endif

#if UNITY_IOS
using Unity.Advertisement.IosSupport;
#endif


public class MenuManager : MonoBehaviour
{
    private GameData gameData;

    private bool initialized = false;
    private bool eventsInitialized = false;
    
    private static MenuManager singleton = null;

    //public LeaderboardManager leaderboardManager;

    private string pendingUsername;

    private string m_GooglePlayGamesToken;

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

        //open waiting panel
        PanelManager.Open("waiting");

#if UNITY_ANDROID
        PlayGamesPlatform.DebugLogEnabled = true;
        PlayGamesPlatform.Activate();
        LoginGooglePlayGames();
#else
    GuestLogin();    
#endif        
    }

    //google play games login
    public void LoginGooglePlayGames()
    {
        #if UNITY_ANDROID
        PlayGamesPlatform.Instance.Authenticate((success) =>
        {
            if (success == SignInStatus.Success)
            {
                //Debug.Log("Login with Google Play games successful.");

                if(gameData == null)
                    gameData = GameObject.FindWithTag("GameData").GetComponent<GameData>();

                gameData.LoadFromFile();

                //sve name and pass
                string playerName = PlayGamesPlatform.Instance.GetUserDisplayName();
                SaveNamePass(playerName, "000000");

                PlayGamesPlatform.Instance.RequestServerSideAccess(true, code =>
                {                    
                    m_GooglePlayGamesToken = code;

                    PanelManager.CloseAll();
                    PanelManager.Open("main");                  
                });
            }
            else
            {
                string uiMessage = "Failed to retrieve Google play games authorization code";

                string locText = LocalizationSettings.StringDatabase.GetLocalizedString("LTLRN", "GuestLoginTxt");

                ShowError(ErrorMenu.Action.SignIn, uiMessage, locText);

                //StartClientService();
                //StartSignInOrLink();
            }
        });
        #endif
    }

    private async void GuestLogin()
    {
        try
        {
            // Initialize Unity Services if not already
            if (UnityServices.State != ServicesInitializationState.Initialized)
            {
                var options = new InitializationOptions();
                options.SetProfile("default_profile");
                await UnityServices.InitializeAsync();
            }

            SignInAnonymouslyAsync();
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
            string locText = LocalizationSettings.StringDatabase.GetLocalizedString("LTLRN", "Error01");
            string uiMessage = locText + FirstSentence(exception.Message);
            ShowError(ErrorMenu.Action.StartService, uiMessage, "Retry");
        }
    }

    //google play games login
    public void StartSignInOrLink()
    {
        #if UNITY_ANDROID
        if (!PlayGamesPlatform.Instance.IsAuthenticated())
        {
            Debug.LogWarning("Not yet authenticated with Google Play Games -- attempting login again");
            LoginGooglePlayGames();
            return;
        }

        // Already authenticated with GPG, proceed with Unity Authentication
        SignInOrLinkWithGooglePlayGames();
        #endif
    }


    //google play games login
    private async void SignInOrLinkWithGooglePlayGames()
    {
        if (string.IsNullOrEmpty(m_GooglePlayGamesToken))
        {
            Debug.LogWarning("Authorization code is null or empty!");
            return;
        }

        if (!AuthenticationService.Instance.IsSignedIn)
        {
            await SignInWithGooglePlayGamesAsync(m_GooglePlayGamesToken);
        }

        else
        {
            await LinkWithGooglePlayGamesAsync(m_GooglePlayGamesToken);
        }
    }

    //google play games login
    private async Task SignInWithGooglePlayGamesAsync(string authCode)
    {
        try
        {
            await AuthenticationService.Instance.SignInWithGooglePlayGamesAsync(authCode); Debug.Log("SignIn is successful.");
        }

        catch (AuthenticationException ex)
        {
            // Compare error code to AuthenticationErrorCodes
            // Notify the player with the proper error message

            string locText = LocalizationSettings.StringDatabase.GetLocalizedString("LTLRN", "GuestLoginTxt");
            ShowError(ErrorMenu.Action.SignIn, ex.ToString(), locText);
            Debug.LogException(ex);
        }

        catch (RequestFailedException ex)
        {
            // Compare error code to CommonErrorCodes
            // Notify the player with the proper error message

            string locText = LocalizationSettings.StringDatabase.GetLocalizedString("LTLRN", "GuestLoginTxt");
            ShowError(ErrorMenu.Action.SignIn, ex.ToString(), locText);
            Debug.LogException(ex);
        }
    }

    //google play games login
    private async Task LinkWithGooglePlayGamesAsync(string authCode)
    {
        try
        {
            await AuthenticationService.Instance.LinkWithGooglePlayGamesAsync(authCode);
            Debug.Log("Link is successful.");
        }
    
        catch (AuthenticationException ex) when(ex.ErrorCode == AuthenticationErrorCodes.AccountAlreadyLinked)
        {
            // Prompt the player with an error message.
            string locText = LocalizationSettings.StringDatabase.GetLocalizedString("LTLRN", "GuestLoginTxt");
            string errorM = "This user is already linked with another account. Log in instead.";
            ShowError(ErrorMenu.Action.SignIn, errorM, locText);
            Debug.LogWarning(errorM);
        }

        catch (AuthenticationException ex)
        {
            // Compare error code to AuthenticationErrorCodes

            string locText = LocalizationSettings.StringDatabase.GetLocalizedString("LTLRN", "GuestLoginTxt");
            ShowError(ErrorMenu.Action.SignIn, ex.ToString(), locText);
            Debug.LogException(ex);
        }

        // Notify the player with the proper error message
       
        catch (RequestFailedException ex)
        {
            // Compare error code to CommonErrorCodes
            // Notify the player with the proper error message

            string locText = LocalizationSettings.StringDatabase.GetLocalizedString("LTLRN", "GuestLoginTxt");
            ShowError(ErrorMenu.Action.SignIn, ex.ToString(), locText);
            Debug.LogException(ex);
        }

    }


    public async void StartClientService()
    {
        PanelManager.CloseAll();
     
        try
        {
            // Initialize Unity Services if not already
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
                if(gameData == null)
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
/*            if (leaderboardManager != null)
            {
                leaderboardManager.isInitialized = true;
                leaderboardManager.InitializeBoard();
            }
            else
            {
                Debug.LogWarning("LeaderboardManager reference is missing in MenuManager.");
            }  */              
            
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
            string locText = LocalizationSettings.StringDatabase.GetLocalizedString("LTLRN", "Error01");
            string uiMessage = locText + FirstSentence(exception.Message);            
            ShowError(ErrorMenu.Action.StartService, uiMessage, "Retry");
        }
    }

    public async void SignInAnonymouslyAsync()
    {
        try
        {
            // FIRST: Initialize services through the persistent manager
            bool initialized = await GameServiceManager.Instance.InitializeServicesAsync();

            if (!initialized)
            {
                Debug.LogWarning("Services initialized but user not signed in yet. Proceeding to sign in...");
            }

            // SECOND: Sign in
            bool signedIn = await GameServiceManager.Instance.SignInAnonymouslyAsync();

            if (!signedIn)
            {
                Debug.LogError("MM: Sign in failed");
                return;
            }

            Debug.Log("MM: Sign in successful");

            // THIRD: Set GameData reference in the manager
            gameData = GameObject.FindWithTag("GameData")?.GetComponent<GameData>();
            if (gameData != null)
            {
                GameServiceManager.Instance.gameData = gameData;
            }

/*            // FOURTH: Initialize LeaderboardManager
            if (leaderboardManager != null)
            {
                leaderboardManager.isInitialized = true;
                leaderboardManager.InitializeBoard();
                await leaderboardManager.LoadPlayers(1);
                Debug.Log("MM: Leaderboard initialized and loaded");
            }
            else
            {
                Debug.LogWarning("LeaderboardManager reference is missing in MenuManager.");
            }*/

            // FIFTH: Open login panel
            PanelManager.CloseAll();
            var panel = PanelManager.GetSingleton("login") as SignOutMenu;
            if (panel != null)
            {
                panel.Open();
                panel.UpdatePlayerNameUI(GameServiceManager.Instance.GetPlayerName());
            }
        }
        catch (AuthenticationException exception)
        {
            Debug.LogException(exception);
            string errText = LocalizationSettings.StringDatabase.GetLocalizedString("LTLRN", "Error02");
            ShowError(ErrorMenu.Action.OpenAuthMenu, errText + exception, "OK");
        }
        catch (RequestFailedException exception)
        {
            Debug.LogException(exception);
            string errText = LocalizationSettings.StringDatabase.GetLocalizedString("LTLRN", "Error03");
            string btnText = LocalizationSettings.StringDatabase.GetLocalizedString("LTLRN", "RetryTxt01");
            ShowError(ErrorMenu.Action.SignIn, errText + exception, btnText);
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

/*            if (leaderboardManager != null)
                await leaderboardManager.LoadPlayers(1);*/
        }
        catch (AuthenticationException exception)
        {
            Debug.LogException(exception);
            string errText = LocalizationSettings.StringDatabase.GetLocalizedString("LTLRN", "Error04");
            string uiMessage = errText + FirstSentence(exception.Message);
            ShowError(ErrorMenu.Action.OpenAuthMenu, uiMessage, "OK");
        }
        catch (RequestFailedException exception)
        {
            Debug.LogException(exception);
            string errText = LocalizationSettings.StringDatabase.GetLocalizedString("LTLRN", "Error05");
            string uiMessage = errText + FirstSentence(exception.Message);
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

/*            if (leaderboardManager != null)
                await leaderboardManager.LoadPlayers(1);*/
        }
        catch (AuthenticationException exception)
        {
            Debug.LogException(exception);
            //Failed to sign you up. Try again. \n Error: 06
            string errText = LocalizationSettings.StringDatabase.GetLocalizedString("LTLRN", "Error06");
            string uiMessage = errText + FirstSentence(exception.Message);
            ShowError(ErrorMenu.Action.OpenAuthMenu, uiMessage, "OK");
        }
        catch (RequestFailedException exception)
        {
            Debug.LogException(exception);
            string errText = LocalizationSettings.StringDatabase.GetLocalizedString("LTLRN", "Error06");
            string uiMessage = errText + FirstSentence(exception.Message);
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
            //Problem with sign in.\n Error: Error07
            string errText = LocalizationSettings.StringDatabase.GetLocalizedString("LTLRN", "Error07");
            string uiMessage = errText + FirstSentence(exception.Message);
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
        if(gameData == null)
            gameData = GameObject.FindWithTag("GameData").GetComponent<GameData>();

        if (gameData != null)
        {
            gameData.saveData.playerName = playerName;
            
            if(playerPass.Length > 0)
                gameData.saveData.playerPass = playerPass;
            
            gameData.SaveToFile();
        }
    }
}