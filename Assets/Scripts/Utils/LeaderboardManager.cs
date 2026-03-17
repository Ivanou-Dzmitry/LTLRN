using GooglePlayGames;
using TMPro;
using UnityEngine;

public class LeaderboardManager : MonoBehaviour
{
    public static LeaderboardManager Instance { get; private set; }

    [Header("Leaderboard ID from Google Play Console")]
    public string leaderboardID = "";
    private const long minScore = 1;    

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void ReportScore(long score)
    {
        if (score < 0)
        {
            Debug.LogError("Score cannot be negative.");
            return;
        }

        if (string.IsNullOrEmpty(leaderboardID))
        {
            Debug.LogError("Leaderboard ID is not set.");
            return;
        }

        if (score < minScore)
        {
            Debug.LogWarning($"Score {score} is less than the minimum score of {minScore}. Not reporting.");
            return;
        }

#if UNITY_ANDROID || UNITY_STANDALONE_WIN
        if (GPGSManager.Instance.IsAuthenticated())
        {
            PlayGamesPlatform.Instance.ReportScore(score, leaderboardID, (bool success) =>
            {
                if (success)
                {
                    Debug.Log("Score reported successfully.");
                }
                else
                {
                    Debug.LogError("Failed to report score.");
                }
            });
        }
        else
        {
            Debug.LogWarning("User is not authenticated. Cannot report score.");
        }
#else
    Debug.Log("GPGS not supported on this platform. Score not reported.");
#endif
    }

    public void ShowLeaderboardUI(TMP_Text errorText)
    {
#if UNITY_ANDROID || UNITY_STANDALONE_WIN
        if (GPGSManager.Instance.IsAuthenticated())
        {
            PlayGamesPlatform.Instance.ShowLeaderboardUI(leaderboardID);
        }
        else
        {
            string errorTxt = "User is not authenticated. Cannot show leaderboard UI.";
            PanelManager.Open("error");
            PanelManager.ShowText(errorText, errorTxt);
            Debug.LogWarning(errorTxt);
        }
#else
    string errorTxt = "Leaderboard is not supported on this platform.";
    PanelManager.Open("error");
    PanelManager.ShowText(errorText, errorTxt);
    Debug.Log(errorTxt);
#endif
    }
}
