using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ButtonImage : MonoBehaviour
{
    public enum ButtonColor
    {
        Primary,
        Secondary,
        Accent,
        Success,
        Warning,
        Error,
        Disabled,
        Transparent,
        PrimaryLight,
        SuccessLight,        
    }

    public enum TextColor
    {
        Primary,
        Secondary,
        OnPrimary,
        OnAccent,
        Disabled
    }

    public enum ButtonState
    {
        Normal,
        Disabled,
        Selected
    }

    public enum ButtonAnimation
    {
        Idle,
        Scale        
    }

    [Header("ButtonState")]
    public ButtonState currentState = ButtonState.Normal;

    [Header("Palette")]
    [SerializeField] private UIColorPalette palette;

    [Header("Button Color")]
    public ButtonColor selectedButtonColor = ButtonColor.Primary;

    [Header("Button Icon")]
    public Image buttonIcon;

    [Header("Text Color")]
    public TextColor selectedTextColor = TextColor.Primary;

    [Header("Text")]
    public string buttonTextStr = string.Empty;
    public TMP_Text buttonText;

    private Button button;
    private Image buttonImage;

    [Header("Animator")]
    [SerializeField] private Animator btnAnimator;

    private void Awake()
    {
        button = GetComponent<Button>();
        buttonImage = GetComponent<Image>();

        ApplyAll();
    }

    private void ApplyAll()
    {
        ApplyButtonColor();
        ApplyButtonText();
    }

    private void ApplyButtonText()
    {
        if (buttonText == null || palette == null)
            return;

        if (!string.IsNullOrEmpty(buttonTextStr))
            buttonText.text = buttonTextStr;

        buttonText.color = GetTextColor(selectedTextColor);
    }

    private void ApplyButtonColor()
    {
        if (buttonImage == null || palette == null)
            return;

        if (!button.interactable)
        {
            buttonImage.color = palette.DisabledButton;
            buttonText.color = palette.Gray4Dark;
            return;
        }

        if(currentState == ButtonState.Normal)
        {
            buttonImage.color = GetButtonColor(selectedButtonColor);
        }
        else if(currentState == ButtonState.Selected)
        {
            buttonImage.color = GetButtonColor(ButtonColor.SuccessLight);
        }
            
    }

    private Color GetButtonColor(ButtonColor color)
    {
        return color switch
        {
            ButtonColor.Primary => palette.Primary,
            ButtonColor.Secondary => palette.Secondary,
            ButtonColor.Accent => palette.Accent,
            ButtonColor.Success => palette.Success,
            ButtonColor.Warning => palette.Warning,
            ButtonColor.Error => palette.Error,
            ButtonColor.Disabled => palette.DisabledButton,
            ButtonColor.Transparent => palette.TransparentPanel,
            ButtonColor.PrimaryLight => palette.PrimaryLight,
            ButtonColor.SuccessLight => palette.SuccessLight,
            _ => palette.Primary
        };
    }

    private Color GetTextColor(TextColor color)
    {
        return color switch
        {
            TextColor.Primary => palette.TextPrimary,
            TextColor.Secondary => palette.TextSecondary,
            TextColor.OnPrimary => palette.TextPrimary,
            TextColor.OnAccent => palette.TextPrimary,
            TextColor.Disabled => palette.TextSecondary,
            _ => palette.TextPrimary
        };
    }

    // Runtime API
    public void SetButtonColor(ButtonColor color)
    {
        selectedButtonColor = color;
        ApplyButtonColor();
    }

    public void SetTextColor(TextColor color)
    {
        selectedTextColor = color;
        ApplyButtonText();
    }

    public void SetText(string text)
    {
        buttonText.text = text;
        ApplyButtonText();
    }

    public void SetDisabled(bool disabled)
    {
        button.interactable = !disabled;
        currentState = disabled ? ButtonState.Disabled : ButtonState.Normal;
        ApplyButtonColor();
    }

    public void SetSelected(bool selected)
    {
        currentState = selected ? ButtonState.Selected : ButtonState.Normal;
        ApplyButtonColor();
    }

    public void RefreshState()
    {
        ApplyAll();
    }

    public void PlayAnimation(bool play, string triggerName)
    {
        if (btnAnimator != null)
        {
            if (play)
            {
                btnAnimator.enabled = true;
                btnAnimator.ResetTrigger(triggerName);
                btnAnimator.SetTrigger(triggerName);
            }
            else
            {
                btnAnimator.enabled = false;
            }                
        }
    }
}
