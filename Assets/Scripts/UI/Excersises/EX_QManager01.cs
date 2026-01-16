using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

//panel with question. Type 1
public class ExQManager01 : MonoBehaviour
{
    private SoundManager soundManager;
    private ExGameLogic exGameLogic;
    private GameData gameData;

    [Header("Palette")]
    [SerializeField] private UIColorPalette palette;

    [Header("Question Content")]
    public RectTransform questionContainer;
    public GameObject imagePrefab;
    public Transform qImagePanel;

    //public Image qImage;
    [SerializeField] public TMP_Text qestionText;

    [Header("Prefabs")]
    public GameObject inputPrefab;
    public GameObject textPrefab;

    [Header("Answer icons")]
    public Sprite[] answerIcon;

    [Header("Sound")]
    public AudioClip qAudioClip;
    //public Button soundBtn;

    [Header("Particles")]
    public ParticleSystem playSoundPart;

    [Header("Input")]
    public GameObject inputPanel;
    private TMP_InputField inputField;
    public Button inputSubmitButton;

    //submit answer
    private ButtonImage inputSubmitBtn;

    [Header("Input Answer")]
    [SerializeField] private GameObject inputAnswerPanel;
    [SerializeField] private TMP_Text inputAnswerCorrectText;
    [SerializeField] private Image[] answerIcons;
    private Image labelIcon;    
    private string tempText; //for correct answer

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
    [SerializeField] public Button soundPlayButton;
    public ButtonImage soundBtn;
    public GameObject answerPanel;

    private int selectedAnswerIndex = -1;

    [Header("Debug")]
    public TMP_Text debugText;


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

        //setup sound button
        if (soundPlayButton != null)
            soundPlayButton.onClick.AddListener(playSoundClicked);

        soundBtn = soundPlayButton.GetComponent<ButtonImage>();

        //for submit text input TYPE 3
        if (inputSubmitButton != null)
        {
            inputSubmitButton.onClick.AddListener(SubmitInput);
            inputSubmitButton.interactable = false;
            inputSubmitBtn = inputSubmitButton.GetComponent<ButtonImage>();
        }

        //answer panel for input questions
        if(inputAnswerPanel != null)
            inputAnswerPanel.SetActive(false);

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
        gameData = GameObject.FindWithTag("GameData").GetComponent<GameData>();

        // Set button texts and refresh
        foreach (var answer in answerButtons)
        {
            answer.buttonImage.buttonTextStr = answer.answerText;
            answer.buttonImage.RefreshState();
        }

        ShowDebugFeatures();
    }

    private void OnEnable()
    {
        ShowDebugFeatures();
    }

    private void ShowDebugFeatures()
    {
        if (gameData == null || debugText == null)
            return;

        if (gameData.saveData.debugMode)
            debugText.gameObject.SetActive(true);
        else
            debugText.gameObject.SetActive(false);
    }


    public void ShowKeyboard()
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
        debugText.text = $"Aclip: {qAudioClip.name}";

        if (soundManager == null)
            soundManager = GameObject.FindWithTag("SoundManager").GetComponent<SoundManager>();     

        //run sound play
        if (soundManager != null)
        {
            if(qAudioClip != null)
                soundManager.PlaySound(qAudioClip);

            debugText.text = $"Aclip: {qAudioClip.name} / {soundManager.effectsSource.volume} / {soundManager.effectsSource.mute}";
        }

        //run particles
        playSoundPart.Play();
    }


    public void SetAnswers(string[] answers, string[] answers2 = null)
    {
        Debug.Log(answers);
        Debug.Log(answers2);

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

    //from input field
    public void CheckInputAnswer(string value, string value2, bool correct)
    {
        string styledCorrectValue = $"<u><b>{value2}</b></u>";
        string rightAnswer = Regex.Replace(tempText, "_+", styledCorrectValue);

        if (correct)
        {
            if(inputField != null)
                inputField.image.color = palette.SuccessLight;

            //show icon
            if (labelIcon != null)
            {
                labelIcon.enabled = true;
                labelIcon.sprite = answerIcon[0];
                labelIcon.color = palette.Success;
            }

        }
        else
        {
            //input color
            if (inputField != null)
                inputField.image.color = palette.Gray2Dark;

            //label color
            if (labelIcon != null)
            {
                labelIcon.enabled = true;
                labelIcon.sprite = answerIcon[1];
                labelIcon.color = palette.Gray3Dark;
            }

            //show correct answer panel
            if (inputAnswerPanel != null)
                inputAnswerPanel.SetActive(true);

            //set correct answer text
            inputAnswerCorrectText.text = rightAnswer;
            inputAnswerCorrectText.color = palette.Success;
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
        soundPlayButton.onClick.RemoveListener(playSoundClicked);

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

        inputSubmitButton.interactable = false;
        inputSubmitBtn.PlayAnimation(false, "Idle");
        inputSubmitBtn.RefreshState();
    }

    public void AddDataToQuestionContainer(string source)
    {
        //save source
        tempText = source;

        //Debug.Log($"Source: {source}");

        var parts = SplitSource(source);

        AddIcon();

        inputAnswerCorrectText.text = string.Empty;

        //add parts
        foreach (var p in parts)
        {
            if (Regex.IsMatch(p, "^_+$"))
            {
                GameObject inp = Instantiate(inputPrefab, questionContainer);
                inp.name = "InputField";

                inputField = inp.GetComponent<TMP_InputField>();

                inputField.characterLimit = 3; // Set character limit based on underscores p.Length

                //inputField = inputFld; // Cache for later use

                // Configure size
                RectTransform rt = inp.GetComponent<RectTransform>();
                float width = Mathf.Max(80, p.Length * 10); // Width based on underscores
                rt.sizeDelta = new Vector2(width, 60);

                // Add/configure LayoutElement ONLY (no ContentSizeFitter)
                LayoutElement le = inp.GetComponent<LayoutElement>();
                if (le == null)
                    le = inp.AddComponent<LayoutElement>();

                le.minWidth = width;

                if (inputField != null)
                {
                    inputField.onValueChanged.AddListener(OnValueChanged);
                    inputField.Select();
                    ShowKeyboard();
                }
            }
            else
            {
                // Create text block
                GameObject textObj = Instantiate(textPrefab, questionContainer);
                TextMeshProUGUI txt = textObj.GetComponent<TextMeshProUGUI>();                

                //add spaces
                if (p == " ")
                {
                    textObj.name = "Space";
                    txt.text = "-";
                    txt.color = new Color(0, 0, 0, 0);
                }
                else
                {
                    textObj.name = p;
                    txt.text = p;
                }
                    
                txt.alignment = TextAlignmentOptions.Center;
                txt.enableAutoSizing = false;
                txt.fontSize = 50; // Set fixed font size
                txt.overflowMode = TextOverflowModes.Overflow;
                txt.textWrappingMode = TextWrappingModes.NoWrap; // Updated property
            }
        }

        StartCoroutine(ForceLayoutRebuild(questionContainer));
    }


    private IEnumerator ForceLayoutRebuild(RectTransform container)
    {
        yield return new WaitForEndOfFrame();

        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(container);

        yield return null;

        LayoutRebuilder.ForceRebuildLayoutImmediate(container);

        if (inputField != null)
        {
            inputField.Select();
            ShowKeyboard();
        }
    }
    List<string> SplitSource(string source)
    {
        var result = new List<string>();

        // Split by underscores while keeping them
        var parts = Regex.Split(source, @"(_+)");

        foreach (var part in parts)
        {
            if (string.IsNullOrEmpty(part))
                continue;

            if (Regex.IsMatch(part, @"^_+$"))
            {
                // It's underscores
                result.Add(part);
            }
            else
            {
                // It's text - split by spaces but keep them
                var words = Regex.Split(part, @"( +)");

                foreach (var word in words)
                {
                    if (!string.IsNullOrEmpty(word))
                    {
                        result.Add(word);
                    }
                }
            }
        }

        return result;
    }


    private void AddIcon()
    {
        //image for icon
        GameObject goIcon = new GameObject("LabelIcon", typeof(RectTransform));
        goIcon.transform.SetParent(questionContainer, false);

        // Get RectTransform and set size
        RectTransform iconRect = goIcon.GetComponent<RectTransform>();
        iconRect.sizeDelta = new Vector2(64, 64);
        iconRect.anchorMin = new Vector2(0.5f, 0.5f);
        iconRect.anchorMax = new Vector2(0.5f, 0.5f);
        iconRect.pivot = new Vector2(0.5f, 0.5f);

        labelIcon = goIcon.AddComponent<Image>();
        labelIcon.sprite = null;
        labelIcon.preserveAspect = true;
        labelIcon.enabled = false;
    }

}
