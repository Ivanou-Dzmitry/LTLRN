using UnityEngine;
using LTLRN.UI;
public class Ex_GamePanel : Panel
{
    [Header("UI")]
    public Canvas canvasRoot;
    public RectTransform panel_01;
    public RectTransform panel_02;
    public RectTransform panel_03;
    
    // for ui layout
    private const float panel01Height = 128f;
    private const float panel03Height = 192f;

    private const float HEADER_HEIGHT = 152f;


    public override void Open()
    {
        base.Open();
    }

    private void Start()
    {
        base.Open();
        SetPanelHeight();
    }

    void SetPanelHeight()
    {
        Rect safeArea = Screen.safeArea;

        // Set main panel bottom padding to safe area
        base.SetBottom(safeArea.yMin / 2);

        // Get canvas scale factor
        float scaleFactor = canvasRoot.scaleFactor;

        // Calculate available height in safe area (accounting for canvas scale)
        float safeAreaHeight = safeArea.height / scaleFactor;

        // Calculate panel_02 height
        float panel02Height = safeAreaHeight - panel01Height - panel03Height;
        panel02Height = Mathf.Max(panel02Height, 0f);

        // Set heights
        panel_01.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, panel01Height);
        panel_02.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, panel02Height);
        panel_03.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, panel03Height);
    }
}
