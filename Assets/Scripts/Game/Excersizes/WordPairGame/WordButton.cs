using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WordButton : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI wordText;
    [SerializeField] private Button button;
    [SerializeField] private Image buttonImage;

    [Header("Colors")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color selectedColor = Color.yellow;
    [SerializeField] private Color correctColor = Color.green;
    [SerializeField] private Color errorColor = Color.red;

    private string word;
    private int pairIndex;
    private bool isFirstWord;
    private MatchingGame gameManager;

    private void Awake()
    {
        if (button == null)
            button = GetComponent<Button>();
        if (buttonImage == null)
            buttonImage = GetComponent<Image>();

        button.onClick.AddListener(OnButtonClick);
    }

    public void Initialize(string word, int pairIndex, bool isFirstWord, MatchingGame gameManager)
    {
        this.word = word;
        this.pairIndex = pairIndex;
        this.isFirstWord = isFirstWord;
        this.gameManager = gameManager;

        wordText.text = word;
        SetNormalState();
    }

    private void OnButtonClick()
    {
        gameManager.OnWordButtonClicked(this);
    }

    public void SetNormalState()
    {
        buttonImage.color = normalColor;
    }

    public void SetSelectedState()
    {
        buttonImage.color = selectedColor;
    }

    public void SetCorrectState()
    {
        buttonImage.color = correctColor;
    }

    public void SetErrorState()
    {
        buttonImage.color = errorColor;
    }

    public string GetWord() => word;
    public int GetPairIndex() => pairIndex;
    public bool IsFirstWord() => isFirstWord;

    public void SetInteractable(bool interactable)
    {
        button.interactable = interactable;
    }
}
