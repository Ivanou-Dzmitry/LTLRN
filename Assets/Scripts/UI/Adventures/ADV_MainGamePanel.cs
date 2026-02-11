using UnityEngine;

public class ADV_MainGamePanel : MonoBehaviour
{
    [Header("UI")]
    public Canvas canvasRoot;
    public RectTransform panel01;
    public RectTransform panel02;
    public RectTransform panel03;

    private const float PANEL01_HEIGHT = 200f;
    private const float PANEL03_HEIGHT = 450f;
    private void Start()
    {
        SetPanelHeight();
    }

    void SetPanelHeight()
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
}
