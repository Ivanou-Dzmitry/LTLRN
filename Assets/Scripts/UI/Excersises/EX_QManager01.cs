using TMPro;
using UnityEngine;
using UnityEngine.UI;

//panel with question. Type 1
public class ExQManager01 : MonoBehaviour
{
    private SoundManager soundManager;
    private ExGameLogic exGameLogic;

    [Header("Palette")]
    [SerializeField] private UIColorPalette palette;

    [Header("Question Content")]
    public GameObject imagePrefab;
    public Transform qImagePanel;

    //public Image qImage;
    [SerializeField] public TMP_Text qestionText;

    [Header("Ansver icons")]
    public Sprite[] answerIcon;


    [Header("Sound")]
    public AudioClip qAudioClip;
    //public Button soundBtn;


    [Header("Particles")]
    public ParticleSystem playSoundPart;

    [Header("Input")]
    public GameObject inputPanel;
    public TMP_InputField inputField;
    public Button inputSubmitButton;

    private ButtonImage inputSubmitBtn;

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
    [SerializeField] public Button soundBtn;
    public GameObject answerPanel;

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
        inputField.onValueChanged.AddListener(OnValueChanged);

        //for submit text input
        inputSubmitButton.onClick.AddListener(SubmitInput);
        inputSubmitButton.interactable = false;

        inputSubmitBtn = inputSubmitButton.GetComponent<ButtonImage>();
    }

    private void Start()
    {
        //get game logic
        exGameLogic = GameObject.FindWithTag("ExGameLogic").GetComponent<ExGameLogic>();
        soundManager = GameObject.FindWithTag("SoundManager").GetComponent<SoundManager>();

        // Set button texts and refresh
        foreach (var answer in answerButtons)
        {
            answer.buttonImage.buttonTextStr = answer.answerText;
            answer.buttonImage.RefreshState();
        }
    }

    private void OnAnswerClicked(int index)
    {
        //if logic exist and in play mode
        if (exGameLogic == null || exGameLogic.gameState == GameState.Pause)
            return;
        
        //set answer index in game logic IMPORTANT
        exGameLogic.currentAnswerIndex = index;

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
        if(soundManager == null) return;

        //run sound play
        if (soundManager != null)
        {
            if(qAudioClip != null)
                soundManager.PlaySound(qAudioClip);
        }

        //run particles
        playSoundPart.Play();
    }


    public void SetAnswers(string[] answers, string[] answers2 = null)
    {
        for (int i = 0; i < Mathf.Min(answers.Length, answerButtons.Length); i++)
        {
            //combine two words if second array exist
            string word1 = answers[i];

            //second word
            string word2 = string.Empty;

            //check second array
            if (answers2 != null)
                word2 = answers2[i];

            //set answer text
            answerButtons[i].answerText = word1 + " " + word2;

            //update button image
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
        ButtonStateSwitcher(correctIndex, true);

        // If wrong answer was selected, show it in red
        if (selectedIndex != correctIndex)
        {
            ButtonStateSwitcher(selectedIndex, false);
        }
    }

    public void CheckInputAnswer(string value, bool correct)
    {
        string tempText = qestionText.text;

        // Remove ALL underscores anywhere, then insert value
        qestionText.text = tempText.Replace("_", "") + value;

        if (correct)
        {
            qestionText.color = palette.Success;
        }
        else
        {
            qestionText.color = palette.Error;
        }
    }


    private void ButtonStateSwitcher(int index, bool correct)
    {
        if (correct)
        {
            answerButtons[index].buttonImage.SetButtonColor(ButtonImage.ButtonColor.Success);
            answerButtons[index].buttonImage.buttonIcon.sprite = answerIcon[0]; // right icon            
        }
        else
        {
            answerButtons[index].buttonImage.SetButtonColor(ButtonImage.ButtonColor.Disabled);
            answerButtons[index].buttonImage.buttonIcon.sprite = answerIcon[1]; // wrong icon             
        }

        answerButtons[index].buttonImage.buttonIcon.color = palette.Gray6Ligth;
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

    private void OnDestroy()
    {
        //remove listeners
        soundBtn.onClick.RemoveListener(playSoundClicked);
    }

    public void ActivateInputField(bool activate)
    {
        if(inputPanel != null)
            inputPanel.SetActive(activate);
    }

    public void ActivateAnswerButtons(bool activate)
    {
        if (answerPanel != null)
            answerPanel.SetActive(activate);
    }

    private void OnValueChanged(string value)
    {
        //if logic exist and in play mode
        if (exGameLogic == null || exGameLogic.gameState == GameState.Pause)
            return;
     
        //set answer text in game logic IMPORTANT
        if(value.Length > 0)
        {
            inputSubmitButton.interactable = true;
            inputSubmitBtn.RefreshState();
        }
        else
        {
            inputSubmitButton.interactable = false;
            inputSubmitBtn.RefreshState();
        }            
    }

    private void SubmitInput()
    {
        //if logic exist and in play mode
        if (exGameLogic == null || exGameLogic.gameState == GameState.Pause)
            return;

        //disable submit button
        inputSubmitButton.interactable = false;
        inputSubmitBtn.RefreshState();

        inputField.interactable = false;

        //enable next button
        exGameLogic.nextButton.interactable = true;
        exGameLogic.nextBtn.RefreshState();

        //set answer text in game logic IMPORTANT
        //exGameLogic.currentAnswerText = inputField.text;

        //check
        exGameLogic.CheckInput(inputField.text);
        //exGameLogic.Check();
    }

}
