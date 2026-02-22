using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MatchingGame : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Transform gridContainer;
    [SerializeField] private WordButton wordButtonPrefab;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI errorsText;
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private TextMeshProUGUI finalScoreText;

    [Header("Game Settings")]
    [SerializeField] private int gridSize = 5;
    [SerializeField] private float gameDuration = 60f;
    [SerializeField] private int maxErrors = 3;
    [SerializeField] private float matchFeedbackDelay = 0.5f;

    [Header("Word Pairs")]
    [SerializeField]
    private List<WordPair> wordPairs = new List<WordPair>
    {
        new WordPair("Aš", "Man"),
        new WordPair("Tu", "Tau"),
        new WordPair("Jis", "Jam"),
        new WordPair("Ji", "Jai"),
        new WordPair("Mes", "Mums"),
        new WordPair("Jūs", "Jums"),
        new WordPair("Jie", "Jiems")
    };

    private List<WordButton> activeButtons = new List<WordButton>();
    private WordButton selectedButton = null;
    private int score = 0;
    private int errors = 0;
    private float timeRemaining;
    private bool isGameActive = false;
    private List<int> usedPairIndices = new List<int>();

    private void Start()
    {
        gameOverPanel.SetActive(false);
        StartGame();
    }

    private void Update()
    {
        if (isGameActive)
        {
            timeRemaining -= Time.deltaTime;
            UpdateTimerUI();

            if (timeRemaining <= 0)
            {
                EndGame();
            }
        }
    }

    public void StartGame()
    {
        score = 0;
        errors = 0;
        timeRemaining = gameDuration;
        isGameActive = true;
        selectedButton = null;
        usedPairIndices.Clear();

        gameOverPanel.SetActive(false);

        ClearGrid();
        PopulateGrid();
        UpdateUI();
    }

    private void ClearGrid()
    {
        foreach (WordButton btn in activeButtons)
        {
            if (btn != null)
                Destroy(btn.gameObject);
        }
        activeButtons.Clear();
    }

    private void PopulateGrid()
    {
        int totalButtons = gridSize * gridSize;
        int pairsNeeded = totalButtons / 2;

        // Generate random pairs
        List<(string word, int pairIndex, bool isFirstWord)> words = new List<(string, int, bool)>();

        for (int i = 0; i < pairsNeeded; i++)
        {
            int randomPairIndex = GetRandomUnusedPairIndex();
            WordPair pair = wordPairs[randomPairIndex];

            words.Add((pair.word1, randomPairIndex, true));
            words.Add((pair.word2, randomPairIndex, false));
        }

        // Shuffle words
        words = words.OrderBy(x => Random.value).ToList();

        // Create buttons
        for (int i = 0; i < totalButtons; i++)
        {
            if (i < words.Count)
            {
                WordButton btn = Instantiate(wordButtonPrefab, gridContainer);
                btn.Initialize(words[i].word, words[i].pairIndex, words[i].isFirstWord, this);
                activeButtons.Add(btn);
            }
        }
    }

    private int GetRandomUnusedPairIndex()
    {
        // Get available pairs
        List<int> availableIndices = new List<int>();
        for (int i = 0; i < wordPairs.Count; i++)
        {
            if (!usedPairIndices.Contains(i))
                availableIndices.Add(i);
        }

        // If all pairs used, reset
        if (availableIndices.Count == 0)
        {
            usedPairIndices.Clear();
            for (int i = 0; i < wordPairs.Count; i++)
                availableIndices.Add(i);
        }

        int randomIndex = availableIndices[Random.Range(0, availableIndices.Count)];
        usedPairIndices.Add(randomIndex);
        return randomIndex;
    }

    public void OnWordButtonClicked(WordButton clickedButton)
    {
        if (!isGameActive || clickedButton == selectedButton)
            return;

        if (selectedButton == null)
        {
            // First selection
            selectedButton = clickedButton;
            selectedButton.SetSelectedState();
        }
        else
        {
            // Second selection - check match
            CheckMatch(selectedButton, clickedButton);
        }
    }

    private void CheckMatch(WordButton first, WordButton second)
    {
        SetAllButtonsInteractable(false);

        if (first.GetPairIndex() == second.GetPairIndex())
        {
            // Correct match!
            StartCoroutine(HandleCorrectMatch(first, second));
        }
        else
        {
            // Wrong match!
            StartCoroutine(HandleWrongMatch(first, second));
        }
    }

    private IEnumerator HandleCorrectMatch(WordButton first, WordButton second)
    {
        first.SetCorrectState();
        second.SetCorrectState();

        score++;
        UpdateUI();

        yield return new WaitForSeconds(matchFeedbackDelay);

        // Remove matched buttons
        activeButtons.Remove(first);
        activeButtons.Remove(second);
        Destroy(first.gameObject);
        Destroy(second.gameObject);

        // Add new pair
        AddNewPair();

        selectedButton = null;
        SetAllButtonsInteractable(true);
    }

    private IEnumerator HandleWrongMatch(WordButton first, WordButton second)
    {
        first.SetErrorState();
        second.SetErrorState();

        errors++;
        UpdateUI();

        yield return new WaitForSeconds(matchFeedbackDelay);

        if (errors >= maxErrors)
        {
            EndGame();
        }
        else
        {
            first.SetNormalState();
            second.SetNormalState();
            selectedButton = null;
            SetAllButtonsInteractable(true);
        }
    }

    private void AddNewPair()
    {
        int randomPairIndex = GetRandomUnusedPairIndex();
        WordPair pair = wordPairs[randomPairIndex];

        // Create two new buttons
        WordButton btn1 = Instantiate(wordButtonPrefab, gridContainer);
        btn1.Initialize(pair.word1, randomPairIndex, true, this);
        activeButtons.Add(btn1);

        WordButton btn2 = Instantiate(wordButtonPrefab, gridContainer);
        btn2.Initialize(pair.word2, randomPairIndex, false, this);
        activeButtons.Add(btn2);

        // Randomize position in grid
        btn1.transform.SetSiblingIndex(Random.Range(0, gridContainer.childCount));
        btn2.transform.SetSiblingIndex(Random.Range(0, gridContainer.childCount));
    }

    private void SetAllButtonsInteractable(bool interactable)
    {
        foreach (WordButton btn in activeButtons)
        {
            if (btn != null)
                btn.SetInteractable(interactable);
        }
    }

    private void UpdateUI()
    {
        scoreText.text = $"Score: {score}";
        errorsText.text = $"Errors: {errors}/{maxErrors}";
    }

    private void UpdateTimerUI()
    {
        int minutes = Mathf.FloorToInt(timeRemaining / 60f);
        int seconds = Mathf.FloorToInt(timeRemaining % 60f);
        timerText.text = $"Time: {minutes:00}:{seconds:00}";
    }

    private void EndGame()
    {
        isGameActive = false;
        SetAllButtonsInteractable(false);

        gameOverPanel.SetActive(true);
        finalScoreText.text = $"Final Score: {score}\nErrors: {errors}";
    }

    public void RestartGame()
    {
        StartGame();
    }
}