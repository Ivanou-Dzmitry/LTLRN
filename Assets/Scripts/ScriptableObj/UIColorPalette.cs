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

    public Color Success;
    public Color Warning;
    public Color Error;

    public Color DisabledButton;
}
