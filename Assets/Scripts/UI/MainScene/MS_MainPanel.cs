using LTLRN.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainPanel : Panel
{
    public Button mode1Button;
    public Button mode2Button;

    [Header("UI")]
    public Canvas canvasRoot;
    public RectTransform panel_01;
    public RectTransform panel_02;
    public RectTransform panel_03;

    private float panel01Height = 128f;
    private float panel03Height = 192f;


    public override void Initialize()
    {
        if (IsInitialized)
            return;

        mode1Button.onClick.AddListener(OpenAdventureMenu);
        mode2Button.onClick.AddListener(OpenExercisesMenu);

        base.Initialize();
    }

    private void OpenAdventureMenu()
    {
        PanelManager.OpenScene("GameBoard");
    }

    private void OpenExercisesMenu()
    {
        PanelManager.OpenScene("ExMenu");
    }

    private void Start()
    {     
        SetPanelHeight();
    }

    void SetPanelHeight()
    {
        Rect safeArea = Screen.safeArea;

        // Set main panel bottom padding to safe area
        base.SetBottom(safeArea.yMin);

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
