using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ADV_MainGamePanel : MonoBehaviour
{
    private GameData gameData;

    [Header("UI")]
    public Canvas canvasRoot;
    public RectTransform panel01;
    public RectTransform panel02;
    public RectTransform panel03;

    public float fadeDuration = 2f;

    [Header("Game data")]
    public Button lifeBtn;
    public Button crystalsBtn;
    public Button starsBtn;
    
    private ButtonImage starsBtnImage;
    private ButtonImage lifeBtnImage;
    private ButtonImage crystalsBtnImage;

    private Image panelImage;

    private const float PANEL01_HEIGHT = 0f;
    private const float PANEL03_HEIGHT = 450f;

    private void Start()
    {
        SetPanelHeight();

        LoadData();

        panelImage = panel02.GetComponent<Image>();

        PanelFadeOut();
    }

    private void LoadData()
    {
        //get gamedata
        if (gameData == null)
            gameData = GameObject.FindWithTag("GameData").GetComponent<GameData>();

        //update buttons
        if (gameData != null)
        {
            UpdateButton(starsBtn, gameData.saveData.stars);
            UpdateButton(lifeBtn, gameData.saveData.life);
            UpdateButton(crystalsBtn, gameData.saveData.crystals);
        }
    }

    private void SetPanelHeight()
    {
        Rect safeArea = Screen.safeArea;

        // Get canvas scale factor
        float scaleFactor = canvasRoot.scaleFactor;

        // Calculate available height in safe area (accounting for canvas scale)
        float safeAreaHeight = safeArea.height / scaleFactor;

        // Calculate panel_02 height
        float panel02Height = safeAreaHeight - PANEL01_HEIGHT - PANEL03_HEIGHT;
        panel02Height = Mathf.Max(panel02Height, 0f);

        //Debug.Log($"{safeAreaHeight}, {PANEL01_HEIGHT}, {panel02Height}, {PANEL03_HEIGHT}");

        // Set heights
        panel01.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, PANEL01_HEIGHT);
        panel02.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, panel02Height);
        panel03.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, PANEL03_HEIGHT);
    }

    //game resources buttons update
    private void UpdateButton(Button button, int value)
    {
        if(button == null)
            return;

        ButtonImage btn = button.GetComponent<ButtonImage>();

        //update button text
        string str = value.ToString();
        btn.buttonTextStr = str;
        btn.SetText(str);
        btn.RefreshState();
    }

    public void PanelFadeOut()
    {
        StartCoroutine(FadeOut());
    }

    private IEnumerator FadeOut()
    {
        float time = 0f;
        Color color = panelImage.color;

        color.a = 1f;
        panelImage.color = color;

        while (time < fadeDuration)
        {
            time += Time.deltaTime;

            float t = time / fadeDuration;
            t = t * t; // ease-in

            color.a = 1f - t;
            panelImage.color = color;

            yield return null;
        }

        color.a = 0f;
        panelImage.color = color;
    }
}
