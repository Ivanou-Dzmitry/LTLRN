using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class QuestionButton : MonoBehaviour
{
    [Header("UI")]
    public TMP_Text qBtnText;
    public Button button;

    private void Awake()
    {
        if (button == null)
            button = GetComponent<Button>();

        button.onClick.AddListener(OnClicked);
    }

    private void OnClicked()
    {
        Debug.Log("Click on " + qBtnText.text);
    }

}
