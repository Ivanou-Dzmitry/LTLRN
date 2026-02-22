using LTLRN.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainPanel : Panel
{
    [SerializeField] private Button mode1Button;
    [SerializeField] private Button mode2Button;

    [Header("UI")]
    [SerializeField] private Canvas canvasRoot;
    [SerializeField] private RectTransform panel_01;
    [SerializeField] private RectTransform panel_02;
    [SerializeField] private RectTransform panel_03;

    private float panel01Height = 128f;
    private float panel03Height = 192f;


    public override void Initialize()
    {
        if (IsInitialized)
            return;

        mode1Button.onClick.AddListener(AdventureMode);
        mode2Button.onClick.AddListener(ExercisesMode);

        base.Initialize();
    }

    private void AdventureMode()
    {
        PanelManager.OpenScene("GameBoard");
        PanelManager.Open("waiting");
    }

    private void ExercisesMode()
    {
        PanelManager.OpenScene("ExMenu");
        PanelManager.Open("waiting");
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
