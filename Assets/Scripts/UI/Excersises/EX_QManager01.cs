using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

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

    [Header("Input Answer")]
    [SerializeField] private GameObject inputAnswerPanel;
    [SerializeField] private GameObject[] answerPanels;
    [SerializeField] private Image[] answerIcons;
    [SerializeField] private TMP_Text inputAnswerCorrectText;
    [SerializeField] private TMP_Text inputAnswerWrongText;

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
        //for type 1 and 2 answers
        if (answerButtons != null)
        {
            // Cache ButtonImage components and setup listeners
            for (int i = 0; i < answerButtons.Length; i++)
            {
                answerButtons[i].buttonImage = answerButtons[i].button.GetComponent<ButtonImage>();

                int index = i; // Capture for closure
                answerButtons[i].button.onClick.AddListener(() => OnAnswerClicked(index));
            }
        }

        if(soundBtn != null)
            soundBtn.onClick.AddListener(playSoundClicked);

        //for submit text input TYPE 3
        if (inputField != null)
        {
            inputField.onValueChanged.AddListener(OnValueChanged);
        }
            
        //for submit text input TYPE 3
        if(inputSubmitButton != null)
        {
            inputSubmitButton.onClick.AddListener(SubmitInput);
            inputSubmitButton.interactable = false;
            inputSubmitBtn = inputSubmitButton.GetComponent<ButtonImage>();
        }

        //root answer panel
        if(inputAnswerPanel != null)
            inputAnswerPanel.SetActive(false);

        //hide answer panels with text
        if (inputAnswerCorrectText != null)
            inputAnswerCorrectText.gameObject.SetActive(false);

        if(inputAnswerWrongText != null)
            inputAnswerWrongText.gameObject.SetActive(false);

        //colorize answer icons
        if(answerIcons != null && answerIcons.Length >= 2)
        {
            if (answerIcons[0] != null)
                answerIcons[0].color = palette.Success;

            if (answerIcons[1] != null)
                answerIcons[1].color = palette.Gray6Dark;
        }

    }


    private void Start()
    {
        //get game logic
        exGameLogic = GameObject.FindWithTag("ExGameLogic").GetComponent<ExGameLogic>();
        soundManager = GameObject.FindWithTag("SoundManager").GetComponent<SoundManager>();

        //for input field
        if (inputField != null)
        {
            inputField.Select();
            ShowKeyboard();

            // Force caret visibility
            inputField.caretWidth = 2; // Make it wider (default is 1)
            inputField.caretBlinkRate = 0.85f; // Blink speed

            // Set caret color (make sure it's visible)
            inputField.caretColor = Color.black; // Or whatever contrasts with background

            // Make sure caret is enabled
            inputField.customCaretColor = true;
        }

        // Set button texts and refresh
        foreach (var answer in answerButtons)
        {
            answer.buttonImage.buttonTextStr = answer.answerText;
            answer.buttonImage.RefreshState();
        }
    }


    private void ShowKeyboard()
    {
        if (inputField != null)
        {
            //inputField.ActivateInputField();

#if UNITY_ANDROID || UNITY_IOS
            // Show keyboard with Lithuanian locale
            TouchScreenKeyboard.Open(
                inputField.text,
                TouchScreenKeyboardType.Default,
                false, // autocorrection
                false, // multiline
                false, // secure
                false, // alert
                "", // placeholder
                0 // character limit (0 = no limit)
            );
#endif
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

    public void CheckInputAnswer(string value, string value2, bool correct)
    {
        string tempText = qestionText.text;

        string styledValue = correct
            ? $"<u><b>{value}</b></u>"
            : $"<u><i>{value}</i></u>";

        // Remove ALL underscores anywhere, then insert value
        string answer = Regex.Replace(tempText, "_+", styledValue);

        string styledCorrectValue = $"<u><b>{value2}</b></u>";

        //get correct answer with second value
        string rightAnswer = Regex.Replace(tempText, "_+", styledCorrectValue);

        //qestionText.text = answer;

        qestionText.gameObject.SetActive(false);

        if (inputAnswerPanel != null)
            inputAnswerPanel.SetActive(true);

        if (correct)
        {
            if (inputAnswerCorrectText != null)
                inputAnswerCorrectText.text = answer;

            inputAnswerCorrectText.gameObject.SetActive(true);
            inputAnswerCorrectText.color = palette.Success;
            answerPanels[1].SetActive(false);
        }
        else
        {
            //set correct answer
            if (inputAnswerCorrectText != null)
                inputAnswerCorrectText.text = rightAnswer;

            inputAnswerCorrectText.gameObject.SetActive(true);
            inputAnswerCorrectText.color = palette.Success;            
            answerPanels[0].SetActive(true);

            //set answer
            if (inputAnswerWrongText != null)
                inputAnswerWrongText.text = answer;

            inputAnswerWrongText.gameObject.SetActive(true);
            inputAnswerWrongText.color = palette.Gray6Dark;
            answerPanels[1].SetActive(true);            
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

        if(inputSubmitButton != null)
            inputSubmitButton.onClick.RemoveListener(SubmitInput);
        
        if(inputField != null)
            inputField.onValueChanged.RemoveListener(OnValueChanged);
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

        //always lowercase
        inputField.text = value.ToLower();

        //set answer text in game logic IMPORTANT
        if (value.Length > 0)
        {
            inputSubmitButton.interactable = true;
            inputSubmitBtn.PlayAnimation(true, "Scale");
            inputSubmitBtn.RefreshState();
        }
        else
        {
            inputSubmitButton.interactable = false;
            inputSubmitBtn.PlayAnimation(false, "Idle");
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
