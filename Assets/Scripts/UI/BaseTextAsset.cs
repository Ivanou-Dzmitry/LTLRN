using TMPro;
using UnityEngine;

public class BaseTextAsset : MonoBehaviour
{
    public enum TextColor
    {
        Primary,
        Secondary,
        Gray6Light,
        Gray6Dark
    }

    public enum TextSize
    {
        Header = 0,
        Base = 1,
        Medium = 2
    }

    [Header("References")]
    [SerializeField] private TMP_Text text;
    [SerializeField] private UIColorPalette palette;


    [Header("Style")]
    [SerializeField] private TextColor textColor = TextColor.Primary;
    [SerializeField] private TextSize textSize = TextSize.Base;

    private void Awake()
    {
        ApplyAll();
    }

    private void ApplyAll()
    {
        if (text == null || palette == null)
            return;

        text.color = GetTextColor(textColor);
        text.fontSize = GetFontSize(textSize);
    }

    private Color GetTextColor(TextColor color)
    {
        return color switch
        {
            TextColor.Primary => palette.TextPrimary,
            TextColor.Secondary => palette.TextSecondary,
            TextColor.Gray6Light => palette.Gray6Ligth,
            TextColor.Gray6Dark => palette.Gray6Dark,
            _ => palette.TextPrimary
        };
    }

    private float GetFontSize(TextSize size)
    {
        return size switch
        {
            TextSize.Header => 80f,
            TextSize.Medium => 50f,
            TextSize.Base => 26f,
            _ => 26f
        };
    }

    // Runtime API (optional)
    public void SetColor(TextColor color)
    {
        textColor = color;
        text.color = GetTextColor(color);        
    }

    public void SetSize(TextSize size)
    {
        textSize = size;
        text.fontSize = GetFontSize(size);        
    }
}

