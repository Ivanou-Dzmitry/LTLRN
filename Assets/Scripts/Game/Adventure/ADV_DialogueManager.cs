using Ink.Runtime;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;
using static ButtonImage;
using static GameLogic;

public class ADV_DialogueManager : MonoBehaviour
{
    public enum DialogueState
    {
        None,
        Start,
        End,
        Possible
    }

    private GameLogic gameLogic;

    [SerializeField] private ADV_IconsManager iconsManager;

    [Header("Dialogue")]
    public DialogueState dialogueState;

    [Header("Player")]
    public GameObject player;
    private Player playerClass;
    [SerializeField] private PlayerInput playerInput;

    [Header("Diallog UI Top")]
    [SerializeField] private GameObject dialogPanelTop;
    [SerializeField] private TextMeshProUGUI dialogueText;
    private ADV_DialogPanel dialogPanelClass;

    [Header("Diallog UI Bottom")]
    [SerializeField] private GameObject dialogPanelBottom;

    [Header("Buttons")]
    public Button communicateButton;
    private ButtonImage communicateBtn;

    [Header("Dialogue Buttons")]
    public Button closePanelButton;
    public Button nextStoryButton;

    private Story currentStory = null;
    public bool dialogueIsPalying { get; private set; }

    //tags
    private const string LOC_TAG = "loc";
    private const string PORTRAIT_TAG = "portrait";
    private const string LAYOUT_TAG = "layout";

    //private static ADV_DialogueManager instance;

    public static ADV_DialogueManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        playerInput = player.GetComponent<PlayerInput>();

        //set buttons
        communicateButton.onClick.AddListener(OnCommunicate);
        communicateBtn = communicateButton.GetComponent<ButtonImage>();

        closePanelButton.onClick.AddListener(OnClosePanel);
        nextStoryButton.onClick.AddListener(NextStory);

        dialogueState = DialogueState.None;
    }

    private void Start()
    {
        gameLogic = GameObject.FindWithTag("ADVGameLogic").GetComponent<GameLogic>();

        //set dialogue off
        dialogueIsPalying = false;
        dialogPanelTop.SetActive(false);

        //get player class
        playerClass = player.GetComponent<Player>();

        //get dialog panel class
        dialogPanelClass = dialogPanelTop.GetComponent<ADV_DialogPanel>();

        //set state
        dialogueState = DialogueState.None;
    }

    public void OnSubmit(InputAction.CallbackContext context)
    {        
        if (!context.performed)
            return;

        if (!dialogueIsPalying)
            return;

        ContinueStory();
    }

    public void OnCommunicate()
    {
        if (!playerClass.readyForDialogue)
            return;

        //toggle
        dialogueIsPalying = !dialogueIsPalying;

        //try read story
        if (currentStory != null && currentStory.canContinue)
        {
            ContinueStory();
            return;
        }        

        if (dialogueIsPalying && currentStory == null)
            playerClass.TryToDialogue();
        else
            ExitDialogueMode();
    }

    public void EnterDialogueMode(TextAsset inkJSON)
    {
        currentStory = new Story(inkJSON.text);

        // jump to start knot explicitly
        currentStory.ChoosePathString("start");
        //Debug.Log($"[EnterDialogue] canContinue after ChoosePathString: {currentStory.canContinue}");

        nextStoryButton.gameObject.SetActive(true);
        closePanelButton.gameObject.SetActive(false);
        dialogueState = DialogueState.Start;
        dialogPanelTop.SetActive(true);

        ContinueStory();

        playerInput.defaultActionMap = "UI";
        communicateBtn.SetSelected(true);
    }

    private void ExitDialogueMode()
    {
        Debug.Log("Exit dialogue mode");

        dialogueIsPalying = false;
        currentStory = null;
        dialogPanelClass.OnDiallogClose();
        playerInput.defaultActionMap = "Player";

        //button off
        communicateBtn.SetSelected(false);

        //set state
        dialogueState = DialogueState.End;

        playerClass.currentPlayerState = Player.PlayerState.Idle;
    }

   
    public void ContinueStory()
    {
        if (!currentStory.canContinue)
        {
            ExitDialogueMode();
            return;
        }

        // MUST call Continue() first — tags are only populated after this
        string rawLine = currentStory.Continue().Trim();

        Debug.Log($"[ContinueStory] raw: '{rawLine}' > tags: [{string.Join(", ", currentStory.currentTags)}]");

        // parse all tags into a dict for easy lookup
        Dictionary<string, string> tags = ParseTags(currentStory.currentTags);

        // handle side effects (portrait, layout, etc.)
        HandleTags(tags);

        // get localized text
        string lineText = "";
        if (tags.TryGetValue(LOC_TAG, out string locKey))
        {
            lineText = LocalizationSettings
                .StringDatabase
                .GetLocalizedStringAsync("KELIAS_DLG", locKey)
                .WaitForCompletion();

            Debug.Log($"[ContinueStory] loc key: '{locKey}' > '{lineText}'");
        }

        // fallback to raw line if no loc tag
        if (string.IsNullOrEmpty(lineText))
            lineText = rawLine;

        if (string.IsNullOrEmpty(lineText))
            lineText = "...";

        dialogueText.text = lineText;
        StartCoroutine(dialogPanelClass.ShowDiallogueTextRoutine(lineText));

        if (!currentStory.canContinue && currentStory.currentChoices.Count == 0)
        {
            nextStoryButton.gameObject.SetActive(false);
            closePanelButton.gameObject.SetActive(true);
        }
    }

    // returns dict of tagKey > tagValue
    private Dictionary<string, string> ParseTags(List<string> rawTags)
    {
        var result = new Dictionary<string, string>();

        foreach (var tag in rawTags)
        {
            string[] split = tag.Split(':');

            if (split.Length != 2)
            {
                Debug.LogWarning($"[HandleTags] Invalid tag format: '{tag}'. Expected 'key:value'.");
                continue;
            }

            string key = split[0].Trim();
            string value = split[1].Trim();
            result[key] = value;

            Debug.Log($"[HandleTags] Parsed tag > key: '{key}' value: '{value}'");
        }

        return result;
    }

    private void HandleTags(Dictionary<string, string> tags)
    {
        if (tags.TryGetValue(PORTRAIT_TAG, out string portrait))
        {
            Debug.Log($"[HandleTags] Portrait: '{portrait}'");

            dialogPanelClass.SetCharacterImage(iconsManager.GetIcon(portrait));

            // TODO: set portrait sprite
        }

        if (tags.TryGetValue(LAYOUT_TAG, out string layout))
        {
            Debug.Log($"[HandleTags] Layout: '{layout}'");
            // TODO: apply layout
        }
    }


    /*        if (currentStory.canContinue)
        {
            //show next line
            dialogueText.text = currentStory.Continue();

            //typewriter effect
            StartCoroutine(dialogPanelClass.ShowDiallogueTextRoutine(dialogueText.text));

            //show butttons
            if (!currentStory.canContinue)
            {
                nextStoryButton.gameObject.SetActive(false);
                closePanelButton.gameObject.SetActive(true);
            }                
        }
        else
        {            
            ExitDialogueMode();
        }*/


    private void NextStory()
    {
        if (currentStory.canContinue)
            ContinueStory();
    }

    private void OnClosePanel()
    {
        ExitDialogueMode();
    }

    private void OnDestroy()
    {
        //remove listeners
        communicateButton.onClick.RemoveListener(OnCommunicate);
        closePanelButton.onClick.RemoveListener(OnClosePanel);
    }

}
