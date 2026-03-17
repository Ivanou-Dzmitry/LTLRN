using UnityEngine;
using GooglePlayGames;
using GooglePlayGames.BasicApi;

public class GPGSManager : MonoBehaviour
{
    public static GPGSManager Instance { get; private set; }
    private bool isAuthenticated = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeGPGS();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeGPGS()
    {
#if UNITY_ANDROID || UNITY_STANDALONE_WIN
        PlayGamesPlatform.DebugLogEnabled = true;
        PlayGamesPlatform.Activate();
        PlayGamesPlatform.Instance.Authenticate(OnSignInResult);
#else
    Debug.Log("Skipping GPGS init: unsupported platform");
#endif
    }

    private void OnSignInResult(SignInStatus status)
    {
        if (status == SignInStatus.Success)
        {
            Debug.Log("Google Play Games sign-in successful.");
            isAuthenticated = true;
        }
        else
        {
            Debug.LogError("Google Play Games sign-in failed: " + status);
            isAuthenticated = false;
        }
    }

    public bool IsAuthenticated()
    {
        return isAuthenticated;
    }
}
