using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ExQManager01 : MonoBehaviour
{
    private SoundManager soundManager;
    private ExGameLogic exGameLogic;

    [Header("Palette")]
    [SerializeField] private UIColorPalette palette;

    [Header("Question Content")]
    public Image qImage;
    [SerializeField] public TMP_Text qestionText;

/*    [Header("Buttons")]
    public Button answer01Btn;
    public Button answer02Btn;
    public Button answer03Btn;
    public Button answer04Btn;

    [SerializeField] public string answer01;
    [SerializeField] public string answer02;
    [SerializeField] public string answer03;
    [SerializeField] public string answer04;*/

    [Header("Sound")]
    public AudioClip qAudioClip;
    //public Button soundBtn;

/*    //buttons
    private ButtonImage a01Btn;
    private ButtonImage a02Btn;
    private ButtonImage a03Btn;
    private ButtonImage a04Btn;*/


    //new structure for answers
    [System.Serializable]
    public class AnswerButton
    {
        public Button button;
        [HideInInspector] public ButtonImage buttonImage;
        public string answerText;
    }

    [Header("Answer Buttons")]
    [SerializeField] private AnswerButton[] answerButtons;
    [SerializeField] private Button soundBtn;

    private int selectedAnswerIndex = -1;


    private void Awake()
    {
        // Cache ButtonImage components and setup listeners
        for (int i = 0; i < answerButtons.Length; i++)
        {
            answerButtons[i].buttonImage = answerButtons[i].button.GetComponent<ButtonImage>();

            int index = i; // Capture for closure
            answerButtons[i].button.onClick.AddListener(() => OnAnswerClicked(index));
        }

        soundBtn.onClick.AddListener(playSoundClicked);
    }

    private void Start()
    {
        // Set button texts and refresh
        foreach (var answer in answerButtons)
        {
            answer.buttonImage.buttonTextStr = answer.answerText;
            answer.buttonImage.RefreshState();
        }
    }

    private void OnAnswerClicked(int index)
    {
        exGameLogic = GameObject.FindWithTag("ExGameLogic").GetComponent<ExGameLogic>();

        //set answer index in game logic IMPORTANT
        exGameLogic.currentAnswerIndex = index;

        //Debug.Log($"Answer {index + 1} clicked");

        // Reset all buttons to Primary
        foreach (var answer in answerButtons)
        {
            answer.buttonImage.SetButtonColor(ButtonImage.ButtonColor.Primary);
        }

        // Highlight selected button
        answerButtons[index].buttonImage.SetButtonColor(ButtonImage.ButtonColor.SuccessLight);
        selectedAnswerIndex = index;

        //check
        exGameLogic.Check();
    }

    private void playSoundClicked()
    {
        soundManager = GameObject.FindWithTag("SoundManager").GetComponent<SoundManager>();

        if (soundManager != null)
        {
            if(qAudioClip != null)
                soundManager.PlaySound(qAudioClip);
        }

        Debug.Log("Play sound clicked");
    }


    public void SetAnswers(string[] answers)
    {
        for (int i = 0; i < Mathf.Min(answers.Length, answerButtons.Length); i++)
        {
            answerButtons[i].answerText = answers[i];

            if (answerButtons[i].buttonImage != null)
            {
                answerButtons[i].buttonImage.buttonTextStr = answers[i];
                answerButtons[i].buttonImage.RefreshState();
            }
        }
    }

    public void CheckAnswer(int selectedIndex, int correctIndex)
    {
        // Reset all buttons first
        foreach (var answer in answerButtons)
        {
            answer.buttonImage.SetButtonColor(ButtonImage.ButtonColor.Primary);
        }

        // Show correct answer in green
        answerButtons[correctIndex].buttonImage.SetButtonColor(ButtonImage.ButtonColor.Success);

        // If wrong answer was selected, show it in red
        if (selectedIndex != correctIndex)
        {
            answerButtons[selectedIndex].buttonImage.SetButtonColor(ButtonImage.ButtonColor.Error);
        }
    }

    public void ResetAnswerColors()
    {
        foreach (var answer in answerButtons)
        {
            answer.buttonImage.SetButtonColor(ButtonImage.ButtonColor.Primary);
        }
    }

    public int GetSelectedAnswerIndex()
    {
        return selectedAnswerIndex;
    }

}
