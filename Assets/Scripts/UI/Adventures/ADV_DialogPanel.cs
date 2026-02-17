using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class ADV_DialogPanel : MonoBehaviour
{    
    private GameLogic gameLogic;
    private Animator animator;
    private const float ANIMATION_DURATION = 1f;

    [SerializeField] private Button closeDiallogButton;
    [SerializeField] private TMP_Text dialogText;

    private void Awake()
    {
        closeDiallogButton.onClick.AddListener(OnDiallogClose);
        animator = GetComponent<Animator>();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //get logic
        gameLogic = GameObject.FindWithTag("ADVGameLogic").GetComponent<GameLogic>();
    }

    public void OpenPanel(string text)
    {
        gameObject.SetActive(true);
        dialogText.text = text;
    }

    public void OnDiallogClose()
    {               
        if (animator != null)
        {
            animator.SetBool("closePanel", true);
        }

        StartCoroutine(DisableAfterDelay(ANIMATION_DURATION));
    }

    private IEnumerator DisableAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);        
        gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        //remove listeners
        closeDiallogButton.onClick.RemoveListener(OnDiallogClose);
    }

}
