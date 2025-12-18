using System;
using System.Threading.Tasks;
using TMPro;
using Unity.Services.Leaderboards;  
using UnityEngine;
using UnityEngine.UI;


public class LeaderboardManager : MonoBehaviour
{    
    private GameData gameData;

    [SerializeField] private int playersPerPage = 5; 
    [SerializeField] private LeaderboardPlayerItem playerItemPrefab = null; 
    [SerializeField] private RectTransform playersContainer = null; 
    [SerializeField] public TextMeshProUGUI pageText = null; 
    [SerializeField] private Button nextButton = null; 
    [SerializeField] private Button prevButton = null; 

    private int currentPage = 1;
    private int totalPages = 0;

    public bool isInitialized = false;

    // Replace with your actual leaderboard ID
    private const string leaderboardId = "LTLRN";

    public void InitializeBoard()
    {        
        gameData = GameObject.FindWithTag("GameData").GetComponent<GameData>();

        ClearPlayersList();

        nextButton.onClick.AddListener(NextPage);
        prevButton.onClick.AddListener(PrevPage);
    }

    public async Task AddScoreLeaderboard(int score)
    {
        try
        {
            var playerEntry = await LeaderboardsService.Instance.AddPlayerScoreAsync(leaderboardId: leaderboardId, score);            
            await LoadPlayers(currentPage); // await the async method properly
        }
        catch (Exception exception)
        {
            Debug.LogError(exception.Message);
        }
    }

    public async Task LoadPlayers(int page)
    {
        if (!isInitialized)
        {
            Debug.LogWarning("Services not initialized yet!");
            return;
        }

        try
        {
            nextButton.interactable = false;
            prevButton.interactable = false;

            GetScoresOptions options = new GetScoresOptions
            {
                Offset = (page - 1) * playersPerPage,
                Limit = playersPerPage
            };

            var score = await LeaderboardsService.Instance.GetScoresAsync(leaderboardId, options);            

            ClearPlayersList();

            //add players to the list
            for (int i = 0; i < score.Results.Count; i++)
            {
                LeaderboardPlayerItem playerItem = Instantiate(playerItemPrefab, playersContainer);
               
                playerItem.Inialize(score.Results[i]);

                //get score
                int currentScore = (int)score.Results[i].Score;

                //get name
                string curentName = playerItem.nameText.text;

                //select player name
                if (curentName == gameData.saveData.playerName)
                {
                    playerItem.nameText.fontStyle = FontStyles.Bold;
                    playerItem.nameText.color = new Color32(144, 238, 144, 255);

                    //fix score
                    if(gameData.saveData.totalScore != currentScore)
                        gameData.saveData.totalScore = currentScore;
                }
            }

            totalPages = Mathf.CeilToInt((float)score.Total / (float)score.Limit);
            currentPage = page;
        }
        catch (Exception ex)
        {
            Debug.LogError(ex.Message);
        }

        pageText.text = $"{currentPage}/{totalPages}";
        nextButton.interactable = currentPage < totalPages && totalPages > 1;
        prevButton.interactable = currentPage > 1 && totalPages > 1;
    }

    private async void NextPage()
    {
        // Go forward unless we're already on the last page
        if (currentPage < totalPages)
        {
            await LoadPlayers(currentPage + 1);
        }
        else
        {
            Debug.Log("Already on the last page.");
        }
    }

    private async void PrevPage()
    {
        // Go back unless we're already on the first page
        if (currentPage > 1)
        {
            await LoadPlayers(currentPage - 1);
        }
        else
        {
            Debug.Log("Already on the first page.");
        }
    }

    public void ClearPlayersList()
    {
        LeaderboardPlayerItem[] playersItems = playersContainer.GetComponentsInChildren<LeaderboardPlayerItem>();

        if (playersItems != null && playersItems.Length > 0)
        {
            foreach (LeaderboardPlayerItem item in playersItems)
            {
                Destroy(item.gameObject);
            }
        }

    }
}
