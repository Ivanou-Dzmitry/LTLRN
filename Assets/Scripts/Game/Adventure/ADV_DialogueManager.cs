using Ink.Runtime;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using static ButtonImage;
using static GameLogic;

public class ADV_DialogueManager : MonoBehaviour
{
    public enum DialogueState
    {
        Start,
        End
    }

    private GameLogic gameLogic;

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

    public Button closePanelButton;
    public Button nextStoryButton;

    private Story currentStory = null;
    public bool dialogueIsPalying { get; private set; }

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
    }

    private void Start()
    {
        gameLogic = GameObject.FindWithTag("ADVGameLogic").GetComponent<GameLogic>();

        dialogueIsPalying = false;
        dialogPanelTop.SetActive(false);

        playerClass = player.GetComponent<Player>();

        //get dialog panel class
        dialogPanelClass = dialogPanelTop.GetComponent<ADV_DialogPanel>();

        dialogueState = DialogueState.End;
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

        //buttons
        nextStoryButton.gameObject.SetActive(true);
        closePanelButton.gameObject.SetActive(false);

        //set state
        dialogueState = DialogueState.Start;

        dialogPanelTop.SetActive(true);

        ContinueStory();

        playerInput.defaultActionMap = "UI";

        //button on
        communicateBtn.SetSelected(true);
    }

    private void ExitDialogueMode()
    {
        dialogueIsPalying = false;
        currentStory = null;
        dialogPanelClass.OnDiallogClose();
        playerInput.defaultActionMap = "Player";

        //button off
        communicateBtn.SetSelected(false);

        //set state
        dialogueState = DialogueState.End;
    }

    public void ContinueStory()
    {
        if (currentStory.canContinue)
        {
            dialogueText.text = currentStory.Continue();
            
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
        }
    }

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
