using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class ADV_DialogPanel : MonoBehaviour
{    
    private GameLogic gameLogic;
    private Animator animator;
    private const float ANIMATION_DURATION = 1f;
    private const float TYPEWRITER_DELAY  = 0.03f;

    [Header("Dialogue Text")]
    [SerializeField] private TMP_Text dialogText;

    [Header("Image")]
    [SerializeField] private Image characterImage;

    [Header("Buttons")]
    [SerializeField] private Button closeDiallogButton;
    [SerializeField] private Button submitButton;
    [SerializeField] private Button saveButton;

    [Header("Settings")]
    [SerializeField] private Canvas canvasRoot;
    [SerializeField] private GameObject dialogPanel;
    [SerializeField] private float panelHeight = 400;

    private void Awake()
    {
        closeDiallogButton.onClick.AddListener(OnDiallogClose);
        
        //get animator
        animator = GetComponent<Animator>();

        //add listener on button
        submitButton.onClick.AddListener(HandleSubmit);

        //get canvas
        if (canvasRoot == null)
            canvasRoot = dialogPanel.GetComponentInParent<Canvas>();
    }

    void Start()
    {
        //get logic
        gameLogic = GameObject.FindWithTag("ADVGameLogic").GetComponent<GameLogic>();
    }

    public void OnDiallogClose()
    {       
        if (animator != null)
        {
            animator.SetBool("closePanel", true);
        }

        if(gameObject.activeSelf) 
            StartCoroutine(DisableAfterDelay(ANIMATION_DURATION));
    }

    private void HandleSubmit()
    {
        if (!ADV_DialogueManager.Instance.dialogueIsPalying)
            return;

        ADV_DialogueManager.Instance.ContinueStory();
    }

    private IEnumerator DisableAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        dialogText.text = "";
        gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        //remove listeners
        closeDiallogButton.onClick.RemoveListener(OnDiallogClose);
        submitButton.onClick.RemoveListener(HandleSubmit);
    }

    public IEnumerator ShowDiallogueTextRoutine(string text)
    {
        ApplyPanelHeight();

        dialogText.maxVisibleCharacters = 0;
        dialogText.text = text;
        dialogText.ForceMeshUpdate();
        
        int totalChars = dialogText.textInfo.characterCount;

        for (int i = 0; i <= totalChars; i++)
        {
            dialogText.maxVisibleCharacters = i;
            yield return new WaitForSeconds(TYPEWRITER_DELAY);
        }
    }

    public void SetCharacterImage(Sprite image)
    {
        if (characterImage != null)
        {
            characterImage.sprite = image;            
        }
    }

    private void ApplyPanelHeight()
    {
        if (canvasRoot == null)
            return;

        RectTransform rt = dialogPanel.GetComponent<RectTransform>();
        if (rt == null)
            return;

        float scaleFactor = canvasRoot.scaleFactor;

        Rect safeArea = Screen.safeArea;
        float topInsetPx = Screen.height - (safeArea.y + safeArea.height);
        float topInset = topInsetPx / scaleFactor;

        rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, panelHeight + topInset);
    }

    public void SaveButtonVisibity(bool visible)
    {
        if(saveButton ==null)
            return;

        if (visible)
            saveButton.gameObject.SetActive(true);
        else
            saveButton.gameObject.SetActive(false);
    }

}
