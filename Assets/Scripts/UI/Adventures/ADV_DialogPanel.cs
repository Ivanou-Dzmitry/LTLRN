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

    [SerializeField] private Button closeDiallogButton;
    [SerializeField] private TMP_Text dialogText;
    [SerializeField] private Button submitButton;

    private void Awake()
    {
        closeDiallogButton.onClick.AddListener(OnDiallogClose);
        animator = GetComponent<Animator>();

        submitButton.onClick.AddListener(HandleSubmit);
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

}
