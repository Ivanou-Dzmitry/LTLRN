using UnityEngine;

[CreateAssetMenu(
    fileName = "UIColorPalette",
    menuName = "UI/Color Palette"
)]
public class UIColorPalette : ScriptableObject
{
    public Color Primary;
    public Color Secondary;
    public Color Accent;

    public Color Background;

    [Header("Panels")]
    public Color Panel01;
    public Color Panel02;
    public Color TransparentPanel;

    [Header("Texts")]
    public Color TextPrimary;
    public Color TextSecondary;
    public Color Gray6Ligth;
    public Color Gray6Dark;

    [Header("Info colors")]
    public Color Success;
    public Color Warning;
    public Color Error;

    [Header("Other colors")]
    public Color DisabledButton;
    public Color PrimaryLight;
    public Color SuccessLight;

    [Header("Grays")]
    public Color Gray3Dark;
    public Color Gray2Dark;
}
