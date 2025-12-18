using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class BasePanelAsset : MonoBehaviour
{
    public enum PanelColor
    {
        Panel01 = 0,
        Panel02 = 1,
        Transparent = 2
    }

    [Header("References")]
    [SerializeField] private Image panelImage;
    [SerializeField] private UIColorPalette palette;

    [Header("Style")]
    [SerializeField] private PanelColor panelColor = PanelColor.Panel01;

    private void Awake()
    {
        if (panelImage == null)
            panelImage = GetComponent<Image>();

        Apply();
    }

    private void Apply()
    {
        if (panelImage == null || palette == null)
            return;

        panelImage.color = GetPanelColor(panelColor);
    }

    private Color GetPanelColor(PanelColor color)
    {
        return color switch
        {
            PanelColor.Panel01 => palette.Panel01,
            PanelColor.Panel02 => palette.Panel02,
            PanelColor.Transparent => palette.TransparentPanel,
            _ => palette.Panel01
        };
    }

    // Runtime API
    public void SetPanelColor(PanelColor color)
    {
        panelColor = color;
        Apply();
    }
}

