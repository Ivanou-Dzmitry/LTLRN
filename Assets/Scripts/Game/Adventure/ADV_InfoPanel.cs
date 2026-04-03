using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class ADV_InfoPanel : MonoBehaviour
{
    [SerializeField] private GameObject infoPanel;
    [SerializeField] private TMP_Text infoPanelText;
    [SerializeField] private int timeToShowInfoPanel = 2;
    [SerializeField] private float hideDuration = 0.5f;
    [SerializeField] private Button closeInfoPanel;    
    private Coroutine infoRoutine;

    private void Awake()
    {
        closeInfoPanel.onClick.AddListener(CloseInfoPanel);
    }

    public void ShowInfo(string text)
    {
        if (string.IsNullOrEmpty(text))
            return;

        if (infoRoutine != null)
            StopCoroutine(infoRoutine);

        infoRoutine = StartCoroutine(ShowInfoRoutine(text));
    }

    private IEnumerator ShowInfoRoutine(string text)
    {
        // --- TYPEWRITER ---
        infoPanelText.text = text;
        infoPanelText.maxVisibleCharacters = 0;

        float charDelay = 0.03f; // speed (lower = faster)

        int totalChars = infoPanelText.textInfo.characterCount;

        for (int i = 0; i <= totalChars; i++)
        {
            infoPanelText.maxVisibleCharacters = i;
            yield return new WaitForSeconds(charDelay);
        }

        Image panelImage = infoPanel.GetComponent<Image>();
        Color color = panelImage.color; // cache initial color

        yield return new WaitForSeconds(timeToShowInfoPanel);

        hideDuration = 0.5f;
        float time = 0f;

        while (time < hideDuration)
        {
            time += Time.deltaTime;

            float t = time / hideDuration;
            t = 1f - Mathf.Pow(1f - t, 2f); // ease-out

            color.a = Mathf.Lerp(1f, 0f, t); // change ONLY alpha
            panelImage.color = color;

            yield return null;
        }

        //restore color
        color.a = 1f;
        panelImage.color = color;

        infoPanel.SetActive(false);
    }


    private void CloseInfoPanel()
    {
        if (infoRoutine != null)
            StopCoroutine(infoRoutine);

        infoPanel.SetActive(false);
    }
}
