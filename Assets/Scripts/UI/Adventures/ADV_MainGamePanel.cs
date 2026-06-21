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

    [Tooltip("Camera that renders the map — told how tall panel01/03 are so it can stop scrolling before showing space hidden behind them.")]
    [SerializeField] private ADV_CameraMove cameraMove;

    public float fadeDuration = 2f;

    [Header("Game data")]
    public Button lifeBtn;
    public Button crystalsBtn;
    public Button starsBtn;
    
    private ButtonImage starsBtnImage;
    private ButtonImage lifeBtnImage;
    private ButtonImage crystalsBtnImage;

    private Image panelImage;

    [Header("Paels")]
    [SerializeField] private float PANEL01_HEIGHT = 305f;
    [SerializeField] private float PANEL03_HEIGHT = 400f;

    private void Start()
    {       
        LoadData();

        panelImage = panel02.GetComponent<Image>();

        PanelFadeOut();

        SetPanelHeight();
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

        if (canvasRoot == null)
            return;

        if (panel01 == null || panel02 == null)
            return;

        // Get canvas scale factor
        float scaleFactor = canvasRoot.scaleFactor;

        // Calculate available height in safe area (accounting for canvas scale)
        float safeAreaHeight = safeArea.height / scaleFactor;

        // Pixels cut off by notch/status bar (top) and home indicator (bottom).
        // Screen.safeArea origin is bottom-left, so top inset = screenHeight - (y + height).
        float topInsetPx    = Screen.height - (safeArea.y + safeArea.height);
        float bottomInsetPx = safeArea.y;

        float topInset    = topInsetPx / scaleFactor;
        float bottomInset = bottomInsetPx / scaleFactor;

        // Grow top/bottom panels by the inset so their background bleeds under the
        // notch / home indicator, while their content (anchored bottom/top respectively)
        // stays within the safe area. On phones with no notch, inset is 0 — no change.
        float panel01Height = PANEL01_HEIGHT + topInset;
        float panel03Height = PANEL03_HEIGHT + bottomInset;

        float panel02Height = safeAreaHeight - PANEL01_HEIGHT - PANEL03_HEIGHT;
        panel02Height = Mathf.Max(panel02Height, 0f);

        // Set heights
        panel01.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, panel01Height);
        panel02.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, panel02Height);
        panel03.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, panel03Height);

        // Tell the camera how tall the panels are in actual screen pixels, so it can
        // stop scrolling before showing map area that's hidden behind them.
        if (cameraMove != null)
        {
            float panel01PixelHeight = panel01Height * scaleFactor;
            float panel03PixelHeight = panel03Height * scaleFactor;
            cameraMove.SetUIPanelInsets(panel01PixelHeight, panel03PixelHeight);
        }
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
